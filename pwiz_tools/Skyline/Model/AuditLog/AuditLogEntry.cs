/*
 * Original author: Tobias Rohde <tobiasr .at. uw.edu>,
 *                  MacCoss Lab, Department of Genome Sciences, UW
 *
 * Copyright 2018 University of Washington - Seattle, WA
 * 
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 *
 *     http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Security.Principal;
using System.Xml;
using System.Xml.Serialization;
using pwiz.Common.Collections;
using pwiz.Skyline.Model.DocSettings;
using pwiz.Skyline.Model.Serialization;
using pwiz.Skyline.Util;

namespace pwiz.Skyline.Model.AuditLog
{
    [XmlRoot(XML_ROOT)]
    public class AuditLogEntry : XmlNamedElement
    {
        public const string XML_ROOT = "audit_log_entry"; // Not L10N

        private SkylineWindow _window;

        public AuditLogEntry(SkylineWindow window, DocumentFormat formatVersion, DiffTree tree, string reason = null)
        {
            SkylineVersion = Install.Version;
            if (Install.Is64Bit)
                SkylineVersion += " (64-Bit)"; // Not L10N

            _window = window;

            FormatVersion = formatVersion;

            TimeStamp = tree.TimeStamp;

            var nodeNamePair = tree.Root.FindFirstNotOneChildNode(tree, PropertyName.Root, true, false);
            // Remove "Settings" from property name if possible
            if (nodeNamePair.Name.Parent != PropertyName.Root)
            {
                var name = nodeNamePair.Name;
                while (name.Parent.Parent != PropertyName.Root)
                    name = name.Parent;

                if (name.Parent.Name == "{0:Settings}") // Not L10N
                {
                    PropertyName.Root.SubProperty(name, false);
                    nodeNamePair = new DiffNodeNamePair(nodeNamePair.Node, nodeNamePair.Name, false);
                }
            }

            UndoRedo = nodeNamePair.ToMessage(LogLevel.undo_redo);
            Summary = tree.Root.FindFirstNotOneChildNode(tree, PropertyName.Root, false, false).ToMessage(LogLevel.summary);
            AllInfo = tree.Root.FindAllLeafNodes(tree, PropertyName.Root, true)
                .Select(n => n.ToMessage(LogLevel.all_info)).ToArray();

            using (var identity = WindowsIdentity.GetCurrent())
            {
                User = identity.Name;
            }

            Reason = reason ?? string.Empty;
        }

        public void SetWindow(SkylineWindow window)
        {
            _window = window;
        }

        public string SkylineVersion { get; private set; }
        public DocumentFormat FormatVersion { get; private set; }

        public DateTime TimeStamp { get; private set; }
        public string User { get; private set; }
        public string Reason { get; private set; }
        public LogMessage UndoRedo { get; private set; }
        public LogMessage Summary { get; private set; }
        public IList<LogMessage> AllInfo { get; private set; }
        public string Text { get; set; }

        public AuditLogEntry ChangeReason(string reason)
        {
            return ChangeProp(ImClone(this), im => im.Reason = reason);
        }

        public AuditLogEntry ChangeAllInfo(IList<LogMessage> allInfo)
        {
            return ChangeProp(ImClone(this), im => im.AllInfo = allInfo);
        }

        public AuditLogRow UndoRedoRow
        {
            get { return new AuditLogRow(_window, this, LogLevel.undo_redo, SkylineVersion, FormatVersion, TimeStamp, User); }
        }

        public AuditLogRow SummaryRow
        {
            get { return new AuditLogRow(_window, this, LogLevel.summary, SkylineVersion, FormatVersion, TimeStamp, User); }
        }

        public IEnumerable<AuditLogRow> AllInfoRows
        {
            get { return AllInfo.Select((s,i) => new AuditLogRow(_window, this, LogLevel.all_info, SkylineVersion, FormatVersion, TimeStamp, User, i)); }
        }

        #region Implementation of IXmlSerializable
        private AuditLogEntry()
        {
            
        }

        private enum ATTR
        {
            format_version,
            time_stamp,
            user
        }

        private enum EL
        {
            message,
            reason
        }

        public static AuditLogEntry Deserialize(XmlReader reader)
        {
            return reader.Deserialize(new AuditLogEntry());
        }

        public override void WriteXml(XmlWriter writer)
        {
            writer.WriteAttribute(ATTR.format_version, FormatVersion.AsDouble());
            writer.WriteAttribute(ATTR.time_stamp, TimeStamp.ToUniversalTime().ToString(CultureInfo.InvariantCulture));
            writer.WriteAttribute(ATTR.user, User);

            if (!string.IsNullOrEmpty(Reason))
                writer.WriteElementString(EL.reason, Reason);
            
            writer.WriteElement(EL.message, UndoRedo);
            writer.WriteElement(EL.message, Summary);

            foreach (var info in AllInfo)
            {
                writer.WriteElement(EL.message, info);
            }
        }

        public override void ReadXml(XmlReader reader)
        {
            FormatVersion = new DocumentFormat(reader.GetDoubleAttribute(ATTR.format_version));
            var time = DateTime.Parse(reader.GetAttribute(ATTR.time_stamp));
            TimeStamp = DateTime.SpecifyKind(time, DateTimeKind.Utc).ToLocalTime();
            User = reader.GetAttribute(ATTR.user);
            reader.ReadStartElement();

            Reason = reader.IsStartElement(EL.reason) ? reader.ReadElementString() : string.Empty;

            UndoRedo = reader.DeserializeElement<LogMessage>();
            Summary = reader.DeserializeElement<LogMessage>();

            AllInfo = new List<LogMessage>();
            while (reader.IsStartElement(EL.message))
            {
                AllInfo.Add(reader.DeserializeElement<LogMessage>());
            }

            reader.ReadEndElement();
        }
        #endregion
    }

    public class AuditLogRow
    {
        private AuditLogEntry _entry;
        private readonly int _allInfoIndex;
        private readonly SkylineWindow _window;
        private readonly LogLevel _level;

        public AuditLogRow(SkylineWindow window, AuditLogEntry entry, LogLevel level, string skylineVersion,
            DocumentFormat documentFormat, DateTime timeStamp, string user, int allInfoIndex = -1)
        {
            Assume.IsNotNull(window);
            _window = window;
            Assume.IsNotNull(entry);
            _entry = entry;

            if (level == LogLevel.all_info && (allInfoIndex < 0 || allInfoIndex >= _entry.AllInfo.Count))
                throw new ArgumentException();

            _level = level;
            _allInfoIndex = allInfoIndex;

            SkylineVersion = skylineVersion;
            DocumentFormat = documentFormat.AsDouble();
            TimeStamp = timeStamp.ToString(CultureInfo.CurrentCulture);

            User = user;
        }

        private LogMessage GetMessage()
        {
            switch (_level)
            {
                case LogLevel.undo_redo:
                    return _entry.UndoRedo;
                case LogLevel.summary:
                    return _entry.Summary;
                case LogLevel.all_info:
                    return _entry.AllInfo[_allInfoIndex];
                default:
                    return null;
            }
        }

        public string TimeStamp { get; private set; }

        public string Text
        {
            get
            {
                var msg = GetMessage();
                var text = msg.ToString();
                if (msg.Expanded)
                    text += " (exp)";
                return text;
            }
        }

        public string SkylineVersion { get; private set; }
        public double DocumentFormat { get; private set; }
        public string User { get; private set; }

        public string Reason
        {
            get
            {
                switch (_level)
                {
                    case LogLevel.undo_redo:
                    case LogLevel.summary:
                        return _entry.Reason;
                    case LogLevel.all_info:
                        return _entry.AllInfo[_allInfoIndex].Reason;
                }

                return string.Empty;
            }
            set
            {
                var copy = new List<AuditLogEntry>(_window.Document.AuditLog);
                var index = copy.FindIndex(e => ReferenceEquals(e, _entry));

                if (index >= 0)
                {
                    var oneAllInfoRow = _entry.AllInfo.Count == 1;
                    if (_level == LogLevel.undo_redo || _level == LogLevel.summary || oneAllInfoRow)
                    {
                        _entry = _entry.ChangeReason(value);
                    }

                    if (_level == LogLevel.all_info || oneAllInfoRow)
                    {
                        var infoIndex = oneAllInfoRow ? 0 : _allInfoIndex;
                        var msg = _entry.AllInfo[infoIndex];
                        msg = msg.ChangeReason(value);
                        var infoCopy = new List<LogMessage>(_entry.AllInfo);
                        infoCopy.RemoveAt(infoIndex);
                        infoCopy.Insert(infoIndex, msg);
                        _entry = _entry.ChangeAllInfo(infoCopy);
                    }

                    copy.RemoveAt(index);
                    copy.Insert(index, _entry);
                    _window.ModifyDocument("Changed audit log reason", d => d.ChangeAuditLog(ImmutableList<AuditLogEntry>.ValueOf(copy)));
                }
            }
        }
    }
}