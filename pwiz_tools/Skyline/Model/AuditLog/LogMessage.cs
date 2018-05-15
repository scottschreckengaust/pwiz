using System;
using System.Collections.Generic;
using System.Linq;
using System.Resources;
using System.Xml;
using System.Xml.Serialization;
using pwiz.Common.Collections;
using pwiz.Skyline.Model.DocSettings;
using pwiz.Skyline.Util;

namespace pwiz.Skyline.Model.AuditLog
{
    [XmlRoot(XML_ROOT)]
    public class LogMessage : XmlNamedElement
    {
        private readonly int[] _expectedNames = {2, 3, 2, 1, 1, 1, 2, 2, 2};
        public const string XML_ROOT = "message"; // Not L10N

        public LogMessage(LogLevel level, MessageType type, string reason, bool expanded, params string[] names)
        {
            if (GetExpectedNameCount(type) != names.Length)
                throw new ArgumentException();

            Level = level;
            Type = type;
            Names = names.ToList();
            Reason = reason;
            Expanded = expanded;
        }

        public LogLevel Level { get; private set; }
        public MessageType Type { get; private set; }
        public string Reason { get; private set; }
        public bool Expanded { get; private set; }
        public IList<string> Names { get; private set; }

        public LogMessage ChangeReason(string reason)
        {
            return ChangeProp(ImClone(this), im => im.Reason = reason);
        }

        public static string Quote(string s)
        {
            if (s == null)
                return null;

            return "\"" + s + "\""; // Not L10N
        }

        public int GetExpectedNameCount(MessageType type)
        {
            return _expectedNames[(int) type];
        }

        public override string ToString()
        {
            var names = Names.Select(s => (object) LocalizeLogStringProperties(s)).ToArray();
            switch (Type)
            {
                case MessageType.is_:
                    return string.Format("{0} is {1}", names);
                case MessageType.changed_from_to:
                    return string.Format("{0} changed from {1} to {2}", names);
                //case MessageType.coll_changed_to:
                //    return string.Format("{0}: {1} changed to {2}", names);
                case MessageType.changed_to:
                    return string.Format(AuditLogStrings.LogMessage_ToString__0__changed_to__1_, names);
                case MessageType.changed:
                    return string.Format(AuditLogStrings.LogMessage_ToString__0__changed, names);
                case MessageType.removed:
                    return string.Format(AuditLogStrings.LogMessage_ToString__0__was_removed, names);
                case MessageType.added:
                    return string.Format(AuditLogStrings.LogMessage_ToString__0__was_added, names);
                case MessageType.contains:
                    return string.Format("{0}: contains {1}", names);
                case MessageType.removed_from:
                    return string.Format(AuditLogStrings.LogMessage_ToString__0____1__was_removed, names);
                case MessageType.added_to:
                    return string.Format(AuditLogStrings.LogMessage_ToString__0____1__was_added, names);
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private static readonly ResourceManager[] _resourceManagers =
        {
            PropertyNames.ResourceManager,
            PropertyElementNames.ResourceManager,
            AuditLogStrings.ResourceManager
        };

        // Replaces all unlocalized strings (e.g {0:PropertyName}) with their
        // corresponding localized string
        public static string LocalizeLogStringProperties(string str)
        {
            if (string.IsNullOrEmpty(str))
                return str;

            var quote = false;
            var expressionStartIndex = -1;

            for (var i = 0; i < str.Length; ++i)
            {
                if (str[i] == '"')
                    quote = !quote;

                if (!quote)
                {
                    if (str[i] == '{')
                    {
                        expressionStartIndex = i;
                    } 
                    else if (str[i] == '}')
                    {
                        if (expressionStartIndex >= 0 && i - expressionStartIndex - 1 > 0)
                        {
                            var subStr = str.Substring(expressionStartIndex + 1, i - expressionStartIndex - 1);

                            // The strings are formatted like this i:name, where i indicates the resource file and
                            // name the name of the resource
                            int index;
                            if (int.TryParse(subStr[0].ToString(), out index) && index >= 0 &&
                                index < _resourceManagers.Length)
                            {
                                var localized = _resourceManagers[index].GetString(subStr.Substring(2));
                                if (localized != null)
                                {
                                    str = str.Substring(0, expressionStartIndex) + localized + str.Substring(i + 1);
                                    i = expressionStartIndex + localized.Length - 1;
                                }
                            } 
                        }

                        expressionStartIndex = -1;
                    }
                }
            }

            return str;
        }

        protected bool Equals(LogMessage other)
        {
            return base.Equals(other) && Type == other.Type && CollectionUtil.EqualsDeep(Names, other.Names) &&
                   Expanded == other.Expanded;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((LogMessage) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                int hashCode = base.GetHashCode();
                hashCode = (hashCode * 397) ^ (int) Type;
                hashCode = (hashCode * 397) ^ (Names != null ? Names.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ Expanded.GetHashCode();
                return hashCode;
            }
        }

        #region Implementation of IXmlSerializable
        private LogMessage()
        {

        }

        private enum ATTR
        {
            type
        }

        private enum EL
        {
            reason,
            name
        }

        public static LogMessage Deserialize(XmlReader reader)
        {
            return reader.Deserialize(new LogMessage());
        }

        public override void WriteXml(XmlWriter writer)
        {
            writer.WriteAttribute(ATTR.type, Type);

            if(!string.IsNullOrEmpty(Reason))
                writer.WriteElementString(EL.reason, Reason);

            foreach (var name in Names)
                writer.WriteElementString(EL.name, name);
        }

        public override void ReadXml(XmlReader reader)
        {
            Type = (MessageType) Enum.Parse(typeof(MessageType), reader.GetAttribute(ATTR.type));
            reader.ReadStartElement();

            var names = new List<string>();
            
            Reason = reader.IsStartElement(EL.reason)
                ? reader.ReadElementString()
                : string.Empty;

            while (reader.IsStartElement(EL.name))
                names.Add(reader.ReadElementString());

            if (names.Count != GetExpectedNameCount(Type))
                throw new XmlException();

            Names = ImmutableList<string>.ValueOf(names);
            reader.ReadEndElement();
        }
        #endregion
    }
}