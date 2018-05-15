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
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using pwiz.Common.Collections;
using pwiz.Common.DataBinding;
using pwiz.Skyline.Controls.Databinding;
using pwiz.Skyline.Model;
using pwiz.Skyline.Model.AuditLog;
using pwiz.Skyline.Model.Databinding;
using pwiz.Skyline.Properties;
using pwiz.Skyline.Util;

namespace pwiz.Skyline.Controls.AuditLog
{
    public partial class AuditLogForm : DocumentGridForm
    {
        private readonly SkylineWindow _skylineWindow;
        private AuditLogRowSource _auditLogRowSource;
        private readonly ToolStripComboBox _logLevelComboBox;
        private readonly ToolStripButton _clearLogButton;

        public static LogLevel LogLevel
        {
            get { return Helpers.ParseEnum(Settings.Default.LogLevel, LogLevel.undo_redo); }
            set { Settings.Default.LogLevel = value.ToString(); }
        }

        public AuditLogForm(SkylineViewContext viewContext)
            : base(viewContext)
        {
            InitializeComponent();

            _skylineWindow = viewContext.SkylineDataSchema.SkylineWindow;

            _logLevelComboBox = new ToolStripComboBox { DropDownStyle = ComboBoxStyle.DropDownList };
            _logLevelComboBox.Items.AddRange(new object[] { AuditLogStrings.AuditLogForm_AuditLogForm_Undo_Redo, AuditLogStrings.AuditLogForm_AuditLogForm_Summary, AuditLogStrings.AuditLogForm_AuditLogForm_All_Info });
            _logLevelComboBox.SelectedIndex = (int)LogLevel;

            _clearLogButton = new ToolStripButton(AuditLogStrings.AuditLogForm_AuditLogForm_Clear_log);

            var enableAuditLogging = new CheckBox();
            enableAuditLogging.Text = AuditLogStrings.AuditLogForm_AuditLogForm_Enable_audit_logging;
            enableAuditLogging.CheckedChanged += enableAuditLogging_CheckedChanged;
            enableAuditLogging.Checked = Settings.Default.AuditLogging;
            enableAuditLogging.BackColor = Color.Transparent;
            var checkBoxHost = new ToolStripControlHost(enableAuditLogging);
            checkBoxHost.Alignment = ToolStripItemAlignment.Right;

            NavBar.BindingNavigator.Items.Add(_logLevelComboBox);
            NavBar.BindingNavigator.Items.Add(_clearLogButton);
            NavBar.BindingNavigator.Items.Add(checkBoxHost);
        }

        public static void EnableAuditLogging(bool enable)
        {
            Settings.Default.AuditLogging = enable;
        }

        void enableAuditLogging_CheckedChanged(object sender, EventArgs e)
        {
            EnableAuditLogging(((CheckBox)sender).Checked);
        }

        void _clearLogButton_Click(object sender, EventArgs e)
        {
            _skylineWindow.ClearAuditLog();
        }

        void _logLevelComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            LogLevel = (LogLevel)_logLevelComboBox.SelectedIndex;
            _auditLogRowSource.NotifyLogLevelChanged();
        }

        protected override void OnHandleCreated(EventArgs e)
        {
            base.OnHandleCreated(e);

            if (_skylineWindow != null)
            {
                _skylineWindow.DocumentUIChangedEvent += SkylineWindowOnDocumentUIChangedEvent;
            }

            _logLevelComboBox.SelectedIndexChanged += _logLevelComboBox_SelectedIndexChanged;
            _clearLogButton.Click += _clearLogButton_Click;
        }

        public void OnDocumentUIChanged(object sender, DocumentChangedEventArgs args)
        {
            if (_auditLogRowSource != null)
            {
                _auditLogRowSource.NotifyDocumentChanged();
            }
        }

        private void SkylineWindowOnDocumentUIChangedEvent(object sender, DocumentChangedEventArgs documentChangedEventArgs)
        {
            if (_auditLogRowSource != null)
            {
                _auditLogRowSource.NotifyDocumentChanged();
            }
        }

        protected override void OnHandleDestroyed(EventArgs e)
        {
            if (_skylineWindow != null)
            {
                _skylineWindow.DocumentUIChangedEvent -= SkylineWindowOnDocumentUIChangedEvent;
            }

            _logLevelComboBox.SelectedIndexChanged -= _logLevelComboBox_SelectedIndexChanged;
            _clearLogButton.Click -= _clearLogButton_Click;

            base.OnHandleDestroyed(e);
        }

        private static ViewInfo GetViewInfo(SkylineDataSchema dataSchema)
        {
            var columnDescriptor = ColumnDescriptor.RootColumn(dataSchema, typeof(AuditLogRow));
            var viewSpec = new ViewSpec().SetName(AuditLogStrings.AuditLogForm_GetViewInfo_Audit_Log).SetRowType(columnDescriptor.PropertyType);
            var columns = new List<ColumnSpec>
            {
                new ColumnSpec(PropertyPath.Root.Property("TimeStamp")), // Not L10N
                new ColumnSpec(PropertyPath.Root.Property("Text")), // Not L10N
            };

            viewSpec = viewSpec.SetColumns(columns);

            return new ViewInfo(columnDescriptor, viewSpec).ChangeViewGroup(ViewGroup.BUILT_IN);
        }

        public static AuditLogForm MakeAuditLogForm(SkylineWindow skylineWindow)
        {
            var dataSchema = new SkylineDataSchema(skylineWindow, SkylineDataSchema.GetLocalizedSchemaLocalizer());
            var viewInfo = GetViewInfo(dataSchema);

            var rowSource = new AuditLogRowSource(skylineWindow);
            var rowSourceInfo = new RowSourceInfo(typeof(AuditLogRow), rowSource, new[] { viewInfo });
            var viewContext = new SkylineViewContext(dataSchema, new[] { rowSourceInfo });

            return new AuditLogForm(viewContext) { _auditLogRowSource = rowSource };
        }

        private class AuditLogRowSource : IRowSource
        {
            private readonly SkylineWindow _window;

            public void NotifyDocumentChanged()
            {
                if (RowSourceChanged != null)
                {
                    RowSourceChanged();
                }
            }

            public AuditLogRowSource(SkylineWindow window)
            {
                _window = window;
            }

            public void NotifyLogLevelChanged()
            {
                if (RowSourceChanged != null)
                {
                    RowSourceChanged();
                }
            }

            public IEnumerable GetItems()
            {
                var doc = _window.Document;

                doc.AuditLog.ForEach(e => e.SetWindow(_window));

                if (LogLevel == LogLevel.undo_redo)
                {
                    return doc.AuditLog.Select(e => e.UndoRedoRow);
                }
                else if (LogLevel == LogLevel.summary)
                {
                    return doc.AuditLog.Select(e => e.SummaryRow);
                }
                else if (LogLevel == LogLevel.all_info)
                {
                    return doc.AuditLog.SelectMany(e => e.AllInfoRows);
                }

                return null;
            }

            public event Action RowSourceChanged;
        }
    }
}
