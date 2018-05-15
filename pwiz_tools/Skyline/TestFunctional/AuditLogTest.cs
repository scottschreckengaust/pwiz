using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using pwiz.Common.Collections;
using pwiz.Common.DataBinding;
using pwiz.Common.DataBinding.Layout;
using pwiz.Skyline.Controls.AuditLog;
using pwiz.Skyline.Model;
using pwiz.Skyline.Model.AuditLog;
using pwiz.Skyline.Model.DocSettings;
using pwiz.Skyline.Model.DocSettings.Extensions;
using pwiz.Skyline.Properties;
using pwiz.Skyline.Util;
using pwiz.SkylineTestUtil;

namespace pwiz.SkylineTestFunctional
{
    [TestClass]
    public class AuditLogTest : AbstractFunctionalTestEx
    {
        //PropertyNames.ResourceManager,
        //PropertyElementNames.ResourceManager,
        //AuditLogStrings.ResourceManager

        [TestMethod]
        public void TestAuditLogLocalization()
        {
            VerifyTypeLocalization(typeof(SrmDocument));
            
            // Verify localized string parsing

            // Same name but different resource file
            VerifyStringLocalization(PropertyNames.Views, "{0:Views}");
            VerifyStringLocalization(PropertyElementNames.Views, "{1:Views}");

            // Empty curly braces
            VerifyStringLocalization("{}", "{}");

            // Multiple strings
            VerifyStringLocalization(
                PropertyNames.Settings + AuditLogStrings.PropertySeparator + PropertyNames.TransitionSettings,
                "{0:Settings}{2:PropertySeparator}{0:TransitionSettings}");

            // string in quotesd
            VerifyStringLocalization("\"{0:Settings}\"", "\"{0:Settings}\"");

            // Non existen resource name
            VerifyStringLocalization("{0:SEttings}", "{0:SEttings}");
        }

        [TestMethod]
        public void TestAuditLog()
        {
            TestFilesZip = "TestFunctional/AreaCVHistogramTest.zip";
            RunFunctionalTest();
        }

        // Verifies that all diff properties of T are localized, unless their name can be ignored
        // or a custom localizer is provided
        private void VerifyTypeLocalization(Type T)
        {
            var properties = Reflector.GetProperties(T);

            foreach (var property in properties)
            {
                if (!property.IgnoreName)
                {
                    string[] names;
                    if (property.CustomLocalizer != null)
                    {
                        var localizer = CustomPropertyLocalizer.CreateInstance(property.CustomLocalizer);
                        names = localizer.PossibleResourceNames;
                    }
                    else
                    {
                        names = new[] { property.PropertyInfo.Name };
                    }

                    foreach (var name in names)
                    {
                        var localized = PropertyNames.ResourceManager.GetString(name);
                        Assert.IsNotNull(localized, "Property {0} not localized", name);
                        //Console.WriteLine("{0} = {1}", name, localized);
                    }
                }

                // The reflector will fail with a non class type because of the type restrictions
                // on Reflector<T>
                if(property.PropertyInfo.PropertyType.IsClass)
                    VerifyTypeLocalization(property.PropertyInfo.PropertyType);
            }
        }

        private void VerifyStringLocalization(string expected, string unlocalized)
        {
            Assert.AreEqual(expected, LogMessage.LocalizeLogStringProperties(unlocalized));
        }

        protected override void DoTest()
        {
            OpenDocument(@"Rat_plasma.sky");

            AuditLogForm.EnableAuditLogging(true);

            // Test audit log messages
            LOG_ENTRIES.ForEach(e => { e.Verify(); });

            // Test audit log clear
            Assert.AreEqual(LOG_ENTRIES.Length, LogEntry.GetAuditLogEntryCount());
            RunUI(() => SkylineWindow.ClearAuditLog());
            Assert.AreEqual(0, LogEntry.GetAuditLogEntryCount());
            // Clearing the audit log can be undone
            RunUI(() => SkylineWindow.Undo());
            Assert.AreEqual(LOG_ENTRIES.Length, LogEntry.GetAuditLogEntryCount());

            // Test disable audit logging
            AuditLogForm.EnableAuditLogging(false);
            // This would add one entry to the audit log if loggin was enabled
            RunUI(LOG_ENTRIES[0].SettingsChange);
            // Length shouldn't have changed
            Assert.AreEqual(LOG_ENTRIES.Length, LogEntry.GetAuditLogEntryCount());

            if(LogEntry.DEBUG_NEW_ENTRY)
                PauseTest();
        }

        public class LogEntry
        {
            public static readonly bool DEBUG_NEW_ENTRY = false;
            private static int _expectedAuditLogEntryCount;

            public LogEntry(Action settingsChange, LogEntryMessages messages)
            {
                SettingsChange = settingsChange;
                ExpectedMessages = messages;
            }

            private static string LogMessageToCode(LogMessage msg, int indentLvl = 0)
            {
                var indent = "";
                for (var i = 0; i < indentLvl; ++i)
                    indent += "    ";

                var result = string.Format(indent + "new LogMessage(LogLevel.{0}, MessageType.{1}, string.Empty, {2},\r\n", msg.Level, msg.Type, msg.Expanded ? "true" : "false");
                foreach (var name in msg.Names)
                {
                    var n = name.Replace("\"", "\\\"");
                    result += indent + string.Format("    \"{0}\",\r\n", n);
                }
                return result.Substring(0, result.Length - 3) + "),\r\n";
            }

            public string AuditLogEntryToCode(AuditLogEntry entry)
            {
                var text = "";

                text += "            new LogEntryMessages(\r\n";
                text += LogMessageToCode(entry.UndoRedo, 4);
                text += LogMessageToCode(entry.Summary, 4);

                text += "                new[]\r\n                {\r\n";
                text = entry.AllInfo.Aggregate(text, (current, info) => current + LogMessageToCode(info, 5));

                return text + "                }),";
            }

            public static int GetAuditLogEntryCount()
            {
                var count = -1;
                RunUI(() => count = SkylineWindow.DocumentUI.AuditLog.Count);
                return count;
            }

            public static AuditLogEntry GetNewestEntry()
            {
                var count = GetAuditLogEntryCount();
                if (count == 0)
                    return null;

                AuditLogEntry result = null;
                RunUI(() => result = SkylineWindow.DocumentUI.AuditLog[count - 1]);
                return result;
            }

            public void Verify()
            {
                var isLast = ReferenceEquals(this, LOG_ENTRIES[LOG_ENTRIES.Length - 1]);
                if (DEBUG_NEW_ENTRY && isLast)
                    System.Diagnostics.Debugger.Break();

                RunUI(SettingsChange);

                var newestEntry = GetNewestEntry();
                //PauseTest(newestEntry.UndoRedo.ToString());

                if (RECORD_DATA)
                {
                    Console.WriteLine(AuditLogEntryToCode(newestEntry));
                    return;
                }
               
                if (DEBUG_NEW_ENTRY && isLast)
                {
                    Console.WriteLine("SettingsChange: " + ExpectedMessages.ExpectedUndoRedo);
                    Console.WriteLine(AuditLogEntryToCode(newestEntry));
                    Console.WriteLine();
                    PauseTest();

                    return;
                }

                ++_expectedAuditLogEntryCount;
                Assert.AreEqual(_expectedAuditLogEntryCount, GetAuditLogEntryCount());
                Assert.IsNotNull(newestEntry);


                Assert.AreEqual(ExpectedMessages.ExpectedUndoRedo, newestEntry.UndoRedo);
                Assert.AreEqual(ExpectedMessages.ExpectedSummary, newestEntry.Summary);

                // No nice error messages
                //CollectionAssert.AreEqual(ExpectedAllInfo, newestEntry.AllInfo);
                Assert.AreEqual(ExpectedMessages.ExpectedAllInfo.Length, newestEntry.AllInfo.Count);

                for (var i = 0; i < ExpectedMessages.ExpectedAllInfo.Length; ++i)
                    Assert.AreEqual(ExpectedMessages.ExpectedAllInfo[i], newestEntry.AllInfo[i]);

                // Test Undo-Redo
                RunUI(() => SkylineWindow.Undo());
                Assert.AreEqual(_expectedAuditLogEntryCount - 1, GetAuditLogEntryCount());
                RunUI(() => SkylineWindow.Redo());
                Assert.AreEqual(_expectedAuditLogEntryCount, GetAuditLogEntryCount());
                Assert.IsTrue(ReferenceEquals(newestEntry, GetNewestEntry()));
            }

            public Action SettingsChange { get; set; }
            public LogEntryMessages ExpectedMessages { get; set; }
        }

        public class LogEntryMessages
        {
            public LogEntryMessages(LogMessage expectedUndoRedo, LogMessage expectedSummary, LogMessage[] expectedAllInfo)
            {
                ExpectedUndoRedo = expectedUndoRedo;
                ExpectedSummary = expectedSummary;
                ExpectedAllInfo = expectedAllInfo;
            }

            public LogMessage ExpectedUndoRedo { get; set; }
            public LogMessage ExpectedSummary { get; set; }
            public LogMessage[] ExpectedAllInfo { get; set; }
        }

        private static bool RECORD_DATA = false;
        //Has to be defined prior to LOG_ENTRIES
        #region DATA
        private static readonly LogEntryMessages[] LOG_ENTRY_MESSAGESES =
        {
            new LogEntryMessages(
                new LogMessage(LogLevel.undo_redo, MessageType.changed_to, string.Empty, false,
                    "{0:TransitionSettings}{2:TabSeparator}{0:Prediction}{2:PropertySeparator}{0:PrecursorMassType}",
                    "\"Average\""),
                new LogMessage(LogLevel.summary, MessageType.changed_to, string.Empty, false,
                    "{0:Settings}{2:PropertySeparator}{0:TransitionSettings}{2:TabSeparator}{0:Prediction}{2:PropertySeparator}{0:PrecursorMassType}",
                    "\"Average\""),
                new[]
                {
                    new LogMessage(LogLevel.all_info, MessageType.changed_from_to, string.Empty, false,
                        "{0:Settings}{2:PropertySeparator}{0:TransitionSettings}{2:TabSeparator}{0:Prediction}{2:PropertySeparator}{0:PrecursorMassType}",
                        "\"Monoisotopic\"",
                        "\"Average\""),
                }),
            new LogEntryMessages(
                new LogMessage(LogLevel.undo_redo, MessageType.changed_from_to, string.Empty, false,
                    "{0:TransitionSettings}{2:TabSeparator}{0:Prediction}{2:PropertySeparator}{0:CollisionEnergy}",
                    "\"Thermo\"",
                    "\"Thermo TSQ Quantiva\""),
                new LogMessage(LogLevel.summary, MessageType.changed_from_to, string.Empty, false,
                    "{0:Settings}{2:PropertySeparator}{0:TransitionSettings}{2:TabSeparator}{0:Prediction}{2:PropertySeparator}{0:CollisionEnergy}",
                    "\"Thermo\"",
                    "\"Thermo TSQ Quantiva\""),
                new[]
                {
                    new LogMessage(LogLevel.all_info, MessageType.changed_from_to, string.Empty, false,
                        "{0:Settings}{2:PropertySeparator}{0:TransitionSettings}{2:TabSeparator}{0:Prediction}{2:PropertySeparator}{0:CollisionEnergy}",
                        "\"Thermo\"",
                        "\"Thermo TSQ Quantiva\""),
                    new LogMessage(LogLevel.all_info, MessageType.contains, string.Empty, true,
                        "{0:Settings}{2:PropertySeparator}{0:TransitionSettings}{2:TabSeparator}{0:Prediction}{2:PropertySeparator}{0:CollisionEnergy}{2:PropertySeparator}{0:Conversions}",
                        "{ {0:Charge}=\"2\", {0:Slope}=\"0.0339\", {0:Intercept}=\"2.3597\" }"),
                    new LogMessage(LogLevel.all_info, MessageType.contains, string.Empty, true,
                        "{0:Settings}{2:PropertySeparator}{0:TransitionSettings}{2:TabSeparator}{0:Prediction}{2:PropertySeparator}{0:CollisionEnergy}{2:PropertySeparator}{0:Conversions}",
                        "{ {0:Charge}=\"3\", {0:Slope}=\"0.0295\", {0:Intercept}=\"1.5123\" }"),
                    new LogMessage(LogLevel.all_info, MessageType.is_, string.Empty, true,
                        "{0:Settings}{2:PropertySeparator}{0:TransitionSettings}{2:TabSeparator}{0:Prediction}{2:PropertySeparator}{0:CollisionEnergy}{2:PropertySeparator}{0:StepSize}",
                        "\"1\""),
                    new LogMessage(LogLevel.all_info, MessageType.is_, string.Empty, true,
                        "{0:Settings}{2:PropertySeparator}{0:TransitionSettings}{2:TabSeparator}{0:Prediction}{2:PropertySeparator}{0:CollisionEnergy}{2:PropertySeparator}{0:StepCount}",
                        "\"5\""),
                }),
            new LogEntryMessages(
                new LogMessage(LogLevel.undo_redo, MessageType.changed_from_to, string.Empty, false,
                    "{0:TransitionSettings}{2:TabSeparator}{0:Prediction}{2:PropertySeparator}{0:DeclusteringPotential}",
                    "{2:Missing}",
                    "\"ABI\""),
                new LogMessage(LogLevel.summary, MessageType.changed_from_to, string.Empty, false,
                    "{0:Settings}{2:PropertySeparator}{0:TransitionSettings}{2:TabSeparator}{0:Prediction}{2:PropertySeparator}{0:DeclusteringPotential}",
                    "{2:Missing}",
                    "\"ABI\""),
                new[]
                {
                    new LogMessage(LogLevel.all_info, MessageType.changed_from_to, string.Empty, false,
                        "{0:Settings}{2:PropertySeparator}{0:TransitionSettings}{2:TabSeparator}{0:Prediction}{2:PropertySeparator}{0:DeclusteringPotential}",
                        "{2:Missing}",
                        "\"ABI\""),
                    new LogMessage(LogLevel.all_info, MessageType.is_, string.Empty, true,
                        "{0:Settings}{2:PropertySeparator}{0:TransitionSettings}{2:TabSeparator}{0:Prediction}{2:PropertySeparator}{0:DeclusteringPotential}{2:PropertySeparator}{0:Slope}",
                        "\"0.0729\""),
                    new LogMessage(LogLevel.all_info, MessageType.is_, string.Empty, true,
                        "{0:Settings}{2:PropertySeparator}{0:TransitionSettings}{2:TabSeparator}{0:Prediction}{2:PropertySeparator}{0:DeclusteringPotential}{2:PropertySeparator}{0:Intercept}",
                        "\"31.117\""),
                    new LogMessage(LogLevel.all_info, MessageType.is_, string.Empty, true,
                        "{0:Settings}{2:PropertySeparator}{0:TransitionSettings}{2:TabSeparator}{0:Prediction}{2:PropertySeparator}{0:DeclusteringPotential}{2:PropertySeparator}{0:StepSize}",
                        "\"1\""),
                    new LogMessage(LogLevel.all_info, MessageType.is_, string.Empty, true,
                        "{0:Settings}{2:PropertySeparator}{0:TransitionSettings}{2:TabSeparator}{0:Prediction}{2:PropertySeparator}{0:DeclusteringPotential}{2:PropertySeparator}{0:StepCount}",
                        "\"5\""),
                }),
            new LogEntryMessages(
                new LogMessage(LogLevel.undo_redo, MessageType.changed, string.Empty, false,
                    "{0:TransitionSettings}{2:TabSeparator}{0:Filter}{2:PropertySeparator}{0:MeasuredIons}"),
                new LogMessage(LogLevel.summary, MessageType.changed, string.Empty, false,
                    "{0:Settings}{2:PropertySeparator}{0:TransitionSettings}{2:TabSeparator}{0:Filter}{2:PropertySeparator}{0:MeasuredIons}"),
                new[]
                {
                    new LogMessage(LogLevel.all_info, MessageType.added_to, string.Empty, false,
                        "{0:Settings}{2:PropertySeparator}{0:TransitionSettings}{2:TabSeparator}{0:Filter}{2:PropertySeparator}{0:MeasuredIons}",
                        "\"N-terminal to Proline\""),
                    new LogMessage(LogLevel.all_info, MessageType.is_, string.Empty, true,
                        "{0:Settings}{2:PropertySeparator}{0:TransitionSettings}{2:TabSeparator}{0:Filter}{2:PropertySeparator}{0:MeasuredIons}{2:PropertySeparator}\"N-terminal to Proline\"{2:PropertySeparator}{0:Fragment}",
                        "\"P\""),
                    new LogMessage(LogLevel.all_info, MessageType.is_, string.Empty, true,
                        "{0:Settings}{2:PropertySeparator}{0:TransitionSettings}{2:TabSeparator}{0:Filter}{2:PropertySeparator}{0:MeasuredIons}{2:PropertySeparator}\"N-terminal to Proline\"{2:PropertySeparator}{0:Restrict}",
                        "{2:Missing}"),
                    new LogMessage(LogLevel.all_info, MessageType.is_, string.Empty, true,
                        "{0:Settings}{2:PropertySeparator}{0:TransitionSettings}{2:TabSeparator}{0:Filter}{2:PropertySeparator}{0:MeasuredIons}{2:PropertySeparator}\"N-terminal to Proline\"{2:PropertySeparator}{0:Terminus}",
                        "\"N\""),
                    new LogMessage(LogLevel.all_info, MessageType.is_, string.Empty, true,
                        "{0:Settings}{2:PropertySeparator}{0:TransitionSettings}{2:TabSeparator}{0:Filter}{2:PropertySeparator}{0:MeasuredIons}{2:PropertySeparator}\"N-terminal to Proline\"{2:PropertySeparator}{0:MinFragmentLength}",
                        "\"3\""),
                    new LogMessage(LogLevel.all_info, MessageType.is_, string.Empty, true,
                        "{0:Settings}{2:PropertySeparator}{0:TransitionSettings}{2:TabSeparator}{0:Filter}{2:PropertySeparator}{0:MeasuredIons}{2:PropertySeparator}\"N-terminal to Proline\"{2:PropertySeparator}{0:Charge}",
                        "\"1\""),
                    new LogMessage(LogLevel.all_info, MessageType.is_, string.Empty, true,
                        "{0:Settings}{2:PropertySeparator}{0:TransitionSettings}{2:TabSeparator}{0:Filter}{2:PropertySeparator}{0:MeasuredIons}{2:PropertySeparator}\"N-terminal to Proline\"{2:PropertySeparator}{0:IsFragment}",
                        "\"True\""),
                    new LogMessage(LogLevel.all_info, MessageType.is_, string.Empty, true,
                        "{0:Settings}{2:PropertySeparator}{0:TransitionSettings}{2:TabSeparator}{0:Filter}{2:PropertySeparator}{0:MeasuredIons}{2:PropertySeparator}\"N-terminal to Proline\"{2:PropertySeparator}{0:IsCustom}",
                        "\"False\""),
                    new LogMessage(LogLevel.all_info, MessageType.added_to, string.Empty, false,
                        "{0:Settings}{2:PropertySeparator}{0:TransitionSettings}{2:TabSeparator}{0:Filter}{2:PropertySeparator}{0:MeasuredIons}",
                        "\"C-terminal to Glu or Asp\""),
                    new LogMessage(LogLevel.all_info, MessageType.is_, string.Empty, true,
                        "{0:Settings}{2:PropertySeparator}{0:TransitionSettings}{2:TabSeparator}{0:Filter}{2:PropertySeparator}{0:MeasuredIons}{2:PropertySeparator}\"C-terminal to Glu or Asp\"{2:PropertySeparator}{0:Fragment}",
                        "\"ED\""),
                    new LogMessage(LogLevel.all_info, MessageType.is_, string.Empty, true,
                        "{0:Settings}{2:PropertySeparator}{0:TransitionSettings}{2:TabSeparator}{0:Filter}{2:PropertySeparator}{0:MeasuredIons}{2:PropertySeparator}\"C-terminal to Glu or Asp\"{2:PropertySeparator}{0:Restrict}",
                        "{2:Missing}"),
                    new LogMessage(LogLevel.all_info, MessageType.is_, string.Empty, true,
                        "{0:Settings}{2:PropertySeparator}{0:TransitionSettings}{2:TabSeparator}{0:Filter}{2:PropertySeparator}{0:MeasuredIons}{2:PropertySeparator}\"C-terminal to Glu or Asp\"{2:PropertySeparator}{0:Terminus}",
                        "\"C\""),
                    new LogMessage(LogLevel.all_info, MessageType.is_, string.Empty, true,
                        "{0:Settings}{2:PropertySeparator}{0:TransitionSettings}{2:TabSeparator}{0:Filter}{2:PropertySeparator}{0:MeasuredIons}{2:PropertySeparator}\"C-terminal to Glu or Asp\"{2:PropertySeparator}{0:MinFragmentLength}",
                        "\"3\""),
                    new LogMessage(LogLevel.all_info, MessageType.is_, string.Empty, true,
                        "{0:Settings}{2:PropertySeparator}{0:TransitionSettings}{2:TabSeparator}{0:Filter}{2:PropertySeparator}{0:MeasuredIons}{2:PropertySeparator}\"C-terminal to Glu or Asp\"{2:PropertySeparator}{0:Charge}",
                        "\"1\""),
                    new LogMessage(LogLevel.all_info, MessageType.is_, string.Empty, true,
                        "{0:Settings}{2:PropertySeparator}{0:TransitionSettings}{2:TabSeparator}{0:Filter}{2:PropertySeparator}{0:MeasuredIons}{2:PropertySeparator}\"C-terminal to Glu or Asp\"{2:PropertySeparator}{0:IsFragment}",
                        "\"True\""),
                    new LogMessage(LogLevel.all_info, MessageType.is_, string.Empty, true,
                        "{0:Settings}{2:PropertySeparator}{0:TransitionSettings}{2:TabSeparator}{0:Filter}{2:PropertySeparator}{0:MeasuredIons}{2:PropertySeparator}\"C-terminal to Glu or Asp\"{2:PropertySeparator}{0:IsCustom}",
                        "\"False\""),
                }),
            new LogEntryMessages(
                new LogMessage(LogLevel.undo_redo, MessageType.changed, string.Empty, false,
                    "{0:TransitionSettings}"),
                new LogMessage(LogLevel.summary, MessageType.changed, string.Empty, false,
                    "{0:Settings}{2:PropertySeparator}{0:TransitionSettings}"),
                new[]
                {
                    new LogMessage(LogLevel.all_info, MessageType.changed_from_to, string.Empty, false,
                        "{0:Settings}{2:PropertySeparator}{0:TransitionSettings}{2:TabSeparator}{0:Prediction}{2:PropertySeparator}{0:PrecursorMassType}",
                        "\"Average\"",
                        "\"Monoisotopic\""),
                    new LogMessage(LogLevel.all_info, MessageType.changed_from_to, string.Empty, false,
                        "{0:Settings}{2:PropertySeparator}{0:TransitionSettings}{2:TabSeparator}{0:FullScan}{2:PropertySeparator}{0:PrecursorIsotopes}",
                        "\"None\"",
                        "\"Count\""),
                    new LogMessage(LogLevel.all_info, MessageType.changed_from_to, string.Empty, false,
                        "{0:Settings}{2:PropertySeparator}{0:TransitionSettings}{2:TabSeparator}{0:FullScan}{2:PropertySeparator}{0:PrecursorIsotopeFilter}",
                        "{2:Missing}",
                        "\"1\""),
                    new LogMessage(LogLevel.all_info, MessageType.changed_from_to, string.Empty, false,
                        "{0:Settings}{2:PropertySeparator}{0:TransitionSettings}{2:TabSeparator}{0:FullScan}{2:PropertySeparator}{0:PrecursorMassAnalyzer}",
                        "\"none\"",
                        "\"centroided\""),
                    new LogMessage(LogLevel.all_info, MessageType.changed_from_to, string.Empty, false,
                        "{0:Settings}{2:PropertySeparator}{0:TransitionSettings}{2:TabSeparator}{0:FullScan}{2:PropertySeparator}{0:MassAccuracy}",
                        "{2:Missing}",
                        "\"10\""),
                    new LogMessage(LogLevel.all_info, MessageType.changed_from_to, string.Empty, false,
                        "{0:Settings}{2:PropertySeparator}{0:TransitionSettings}{2:TabSeparator}{0:FullScan}{2:PropertySeparator}{0:IsotopeEnrichments}",
                        "{2:Missing}",
                        "\"Default\""),
                    new LogMessage(LogLevel.all_info, MessageType.contains, string.Empty, true,
                        "{0:Settings}{2:PropertySeparator}{0:TransitionSettings}{2:TabSeparator}{0:FullScan}{2:PropertySeparator}{0:IsotopeEnrichments}{2:PropertySeparator}{0:Enrichments}",
                        "\"H' = 98%\""),
                    new LogMessage(LogLevel.all_info, MessageType.contains, string.Empty, true,
                        "{0:Settings}{2:PropertySeparator}{0:TransitionSettings}{2:TabSeparator}{0:FullScan}{2:PropertySeparator}{0:IsotopeEnrichments}{2:PropertySeparator}{0:Enrichments}",
                        "\"C' = 99.5%\""),
                    new LogMessage(LogLevel.all_info, MessageType.contains, string.Empty, true,
                        "{0:Settings}{2:PropertySeparator}{0:TransitionSettings}{2:TabSeparator}{0:FullScan}{2:PropertySeparator}{0:IsotopeEnrichments}{2:PropertySeparator}{0:Enrichments}",
                        "\"N' = 99.5%\""),
                    new LogMessage(LogLevel.all_info, MessageType.contains, string.Empty, true,
                        "{0:Settings}{2:PropertySeparator}{0:TransitionSettings}{2:TabSeparator}{0:FullScan}{2:PropertySeparator}{0:IsotopeEnrichments}{2:PropertySeparator}{0:Enrichments}",
                        "\"O\" = 99%\""),
                    new LogMessage(LogLevel.all_info, MessageType.contains, string.Empty, true,
                        "{0:Settings}{2:PropertySeparator}{0:TransitionSettings}{2:TabSeparator}{0:FullScan}{2:PropertySeparator}{0:IsotopeEnrichments}{2:PropertySeparator}{0:Enrichments}",
                        "\"O' = 99%\""),
                    new LogMessage(LogLevel.all_info, MessageType.contains, string.Empty, true,
                        "{0:Settings}{2:PropertySeparator}{0:TransitionSettings}{2:TabSeparator}{0:FullScan}{2:PropertySeparator}{0:IsotopeEnrichments}{2:PropertySeparator}{0:Enrichments}",
                        "\"Cl' = 99%\""),
                    new LogMessage(LogLevel.all_info, MessageType.contains, string.Empty, true,
                        "{0:Settings}{2:PropertySeparator}{0:TransitionSettings}{2:TabSeparator}{0:FullScan}{2:PropertySeparator}{0:IsotopeEnrichments}{2:PropertySeparator}{0:Enrichments}",
                        "\"Br' = 99%\""),
                    new LogMessage(LogLevel.all_info, MessageType.contains, string.Empty, true,
                        "{0:Settings}{2:PropertySeparator}{0:TransitionSettings}{2:TabSeparator}{0:FullScan}{2:PropertySeparator}{0:IsotopeEnrichments}{2:PropertySeparator}{0:Enrichments}",
                        "\"P' = 99%\""),
                    new LogMessage(LogLevel.all_info, MessageType.contains, string.Empty, true,
                        "{0:Settings}{2:PropertySeparator}{0:TransitionSettings}{2:TabSeparator}{0:FullScan}{2:PropertySeparator}{0:IsotopeEnrichments}{2:PropertySeparator}{0:Enrichments}",
                        "\"S\" = 99%\""),
                    new LogMessage(LogLevel.all_info, MessageType.contains, string.Empty, true,
                        "{0:Settings}{2:PropertySeparator}{0:TransitionSettings}{2:TabSeparator}{0:FullScan}{2:PropertySeparator}{0:IsotopeEnrichments}{2:PropertySeparator}{0:Enrichments}",
                        "\"S' = 99%\""),
                }),
            new LogEntryMessages(
                new LogMessage(LogLevel.undo_redo, MessageType.changed, string.Empty, false,
                    "{0:TransitionSettings}{2:TabSeparator}{0:FullScan}"),
                new LogMessage(LogLevel.summary, MessageType.changed, string.Empty, false,
                    "{0:Settings}{2:PropertySeparator}{0:TransitionSettings}{2:TabSeparator}{0:FullScan}"),
                new[]
                {
                    new LogMessage(LogLevel.all_info, MessageType.changed_from_to, string.Empty, false,
                        "{0:Settings}{2:PropertySeparator}{0:TransitionSettings}{2:TabSeparator}{0:FullScan}{2:PropertySeparator}{0:PrecursorMassAnalyzer}",
                        "\"centroided\"",
                        "\"qit\""),
                    new LogMessage(LogLevel.all_info, MessageType.changed_from_to, string.Empty, false,
                        "{0:Settings}{2:PropertySeparator}{0:TransitionSettings}{2:TabSeparator}{0:FullScan}{2:PropertySeparator}{0:Resolution}",
                        "\"10\"",
                        "\"0.7\""),
                    new LogMessage(LogLevel.all_info, MessageType.changed_from_to, string.Empty, false,
                        "{0:Settings}{2:PropertySeparator}{0:TransitionSettings}{2:TabSeparator}{0:FullScan}{2:PropertySeparator}{0:IsotopeEnrichments}",
                        "\"Default\"",
                        "{2:Missing}"),
                }),
            new LogEntryMessages(
                new LogMessage(LogLevel.undo_redo, MessageType.removed, string.Empty, false,
                    "{1:AnnotationDefs}{2:ElementTypeSeparator}\"SubjectId\""),
                new LogMessage(LogLevel.summary, MessageType.removed_from, string.Empty, false,
                    "{0:Settings}{2:PropertySeparator}{0:DataSettings}{2:TabSeparator}{0:AnnotationDefs}",
                    "\"SubjectId\""),
                new[]
                {
                    new LogMessage(LogLevel.all_info, MessageType.removed_from, string.Empty, false,
                        "{0:Settings}{2:PropertySeparator}{0:DataSettings}{2:TabSeparator}{0:AnnotationDefs}",
                        "\"SubjectId\""),
                }),
            new LogEntryMessages(
                new LogMessage(LogLevel.undo_redo, MessageType.added, string.Empty, false,
                    "{1:AnnotationDefs}{2:ElementTypeSeparator}\"SubjectId\""),
                new LogMessage(LogLevel.summary, MessageType.added_to, string.Empty, false,
                    "{0:Settings}{2:PropertySeparator}{0:DataSettings}{2:TabSeparator}{0:AnnotationDefs}",
                    "\"SubjectId\""),
                new[]
                {
                    new LogMessage(LogLevel.all_info, MessageType.added_to, string.Empty, false,
                        "{0:Settings}{2:PropertySeparator}{0:DataSettings}{2:TabSeparator}{0:AnnotationDefs}",
                        "\"SubjectId\""),
                    new LogMessage(LogLevel.all_info, MessageType.is_, string.Empty, true,
                        "{0:Settings}{2:PropertySeparator}{0:DataSettings}{2:TabSeparator}{0:AnnotationDefs}{2:PropertySeparator}\"SubjectId\"{2:PropertySeparator}{0:AnnotationTargets}",
                        "\"replicate\""),
                    new LogMessage(LogLevel.all_info, MessageType.is_, string.Empty, true,
                        "{0:Settings}{2:PropertySeparator}{0:DataSettings}{2:TabSeparator}{0:AnnotationDefs}{2:PropertySeparator}\"SubjectId\"{2:PropertySeparator}{0:Type}",
                        "\"text\""),
                    new LogMessage(LogLevel.all_info, MessageType.is_, string.Empty, true,
                        "{0:Settings}{2:PropertySeparator}{0:DataSettings}{2:TabSeparator}{0:AnnotationDefs}{2:PropertySeparator}\"SubjectId\"{2:PropertySeparator}{0:Items}",
                        "[  ]"),
                }),
            new LogEntryMessages(
                new LogMessage(LogLevel.undo_redo, MessageType.added, string.Empty, false,
                    "{1:Views}{2:ElementTypeSeparator}\"Mixed Transition List\""),
                new LogMessage(LogLevel.summary, MessageType.added_to, string.Empty, false,
                    "{0:Settings}{2:PropertySeparator}{0:DataSettings}{2:PropertySeparator}{0:Views}",
                    "\"Mixed Transition List\""),
                new[]
                {
                    new LogMessage(LogLevel.all_info, MessageType.added_to, string.Empty, false,
                        "{0:Settings}{2:PropertySeparator}{0:DataSettings}{2:PropertySeparator}{0:Views}",
                        "\"Mixed Transition List\""),
                    new LogMessage(LogLevel.all_info, MessageType.contains, string.Empty, true,
                        "{0:Settings}{2:PropertySeparator}{0:DataSettings}{2:PropertySeparator}{0:Views}{2:PropertySeparator}\"Mixed Transition List\"{2:PropertySeparator}{0:Columns}",
                        "\"Precursor.Peptide.Protein.Name\""),
                    new LogMessage(LogLevel.all_info, MessageType.contains, string.Empty, true,
                        "{0:Settings}{2:PropertySeparator}{0:DataSettings}{2:PropertySeparator}{0:Views}{2:PropertySeparator}\"Mixed Transition List\"{2:PropertySeparator}{0:Columns}",
                        "\"Precursor.Peptide.ModifiedSequence\""),
                    new LogMessage(LogLevel.all_info, MessageType.contains, string.Empty, true,
                        "{0:Settings}{2:PropertySeparator}{0:DataSettings}{2:PropertySeparator}{0:Views}{2:PropertySeparator}\"Mixed Transition List\"{2:PropertySeparator}{0:Columns}",
                        "\"Precursor.Peptide.MoleculeName\""),
                    new LogMessage(LogLevel.all_info, MessageType.contains, string.Empty, true,
                        "{0:Settings}{2:PropertySeparator}{0:DataSettings}{2:PropertySeparator}{0:Views}{2:PropertySeparator}\"Mixed Transition List\"{2:PropertySeparator}{0:Columns}",
                        "\"Precursor.Peptide.MoleculeFormula\""),
                    new LogMessage(LogLevel.all_info, MessageType.contains, string.Empty, true,
                        "{0:Settings}{2:PropertySeparator}{0:DataSettings}{2:PropertySeparator}{0:Views}{2:PropertySeparator}\"Mixed Transition List\"{2:PropertySeparator}{0:Columns}",
                        "\"Precursor.IonFormula\""),
                    new LogMessage(LogLevel.all_info, MessageType.contains, string.Empty, true,
                        "{0:Settings}{2:PropertySeparator}{0:DataSettings}{2:PropertySeparator}{0:Views}{2:PropertySeparator}\"Mixed Transition List\"{2:PropertySeparator}{0:Columns}",
                        "\"Precursor.NeutralFormula\""),
                    new LogMessage(LogLevel.all_info, MessageType.contains, string.Empty, true,
                        "{0:Settings}{2:PropertySeparator}{0:DataSettings}{2:PropertySeparator}{0:Views}{2:PropertySeparator}\"Mixed Transition List\"{2:PropertySeparator}{0:Columns}",
                        "\"Precursor.Adduct\""),
                    new LogMessage(LogLevel.all_info, MessageType.contains, string.Empty, true,
                        "{0:Settings}{2:PropertySeparator}{0:DataSettings}{2:PropertySeparator}{0:Views}{2:PropertySeparator}\"Mixed Transition List\"{2:PropertySeparator}{0:Columns}",
                        "\"Precursor.Mz\""),
                    new LogMessage(LogLevel.all_info, MessageType.contains, string.Empty, true,
                        "{0:Settings}{2:PropertySeparator}{0:DataSettings}{2:PropertySeparator}{0:Views}{2:PropertySeparator}\"Mixed Transition List\"{2:PropertySeparator}{0:Columns}",
                        "\"Precursor.Charge\""),
                    new LogMessage(LogLevel.all_info, MessageType.contains, string.Empty, true,
                        "{0:Settings}{2:PropertySeparator}{0:DataSettings}{2:PropertySeparator}{0:Views}{2:PropertySeparator}\"Mixed Transition List\"{2:PropertySeparator}{0:Columns}",
                        "\"Precursor.CollisionEnergy\""),
                    new LogMessage(LogLevel.all_info, MessageType.contains, string.Empty, true,
                        "{0:Settings}{2:PropertySeparator}{0:DataSettings}{2:PropertySeparator}{0:Views}{2:PropertySeparator}\"Mixed Transition List\"{2:PropertySeparator}{0:Columns}",
                        "\"Precursor.ExplicitCollisionEnergy\""),
                    new LogMessage(LogLevel.all_info, MessageType.contains, string.Empty, true,
                        "{0:Settings}{2:PropertySeparator}{0:DataSettings}{2:PropertySeparator}{0:Views}{2:PropertySeparator}\"Mixed Transition List\"{2:PropertySeparator}{0:Columns}",
                        "\"Precursor.Peptide.ExplicitRetentionTime\""),
                    new LogMessage(LogLevel.all_info, MessageType.contains, string.Empty, true,
                        "{0:Settings}{2:PropertySeparator}{0:DataSettings}{2:PropertySeparator}{0:Views}{2:PropertySeparator}\"Mixed Transition List\"{2:PropertySeparator}{0:Columns}",
                        "\"Precursor.Peptide.ExplicitRetentionTimeWindow\""),
                    new LogMessage(LogLevel.all_info, MessageType.contains, string.Empty, true,
                        "{0:Settings}{2:PropertySeparator}{0:DataSettings}{2:PropertySeparator}{0:Views}{2:PropertySeparator}\"Mixed Transition List\"{2:PropertySeparator}{0:Columns}",
                        "\"ProductMz\""),
                    new LogMessage(LogLevel.all_info, MessageType.contains, string.Empty, true,
                        "{0:Settings}{2:PropertySeparator}{0:DataSettings}{2:PropertySeparator}{0:Views}{2:PropertySeparator}\"Mixed Transition List\"{2:PropertySeparator}{0:Columns}",
                        "\"ProductCharge\""),
                    new LogMessage(LogLevel.all_info, MessageType.contains, string.Empty, true,
                        "{0:Settings}{2:PropertySeparator}{0:DataSettings}{2:PropertySeparator}{0:Views}{2:PropertySeparator}\"Mixed Transition List\"{2:PropertySeparator}{0:Columns}",
                        "\"FragmentIon\""),
                    new LogMessage(LogLevel.all_info, MessageType.contains, string.Empty, true,
                        "{0:Settings}{2:PropertySeparator}{0:DataSettings}{2:PropertySeparator}{0:Views}{2:PropertySeparator}\"Mixed Transition List\"{2:PropertySeparator}{0:Columns}",
                        "\"ProductIonFormula\""),
                    new LogMessage(LogLevel.all_info, MessageType.contains, string.Empty, true,
                        "{0:Settings}{2:PropertySeparator}{0:DataSettings}{2:PropertySeparator}{0:Views}{2:PropertySeparator}\"Mixed Transition List\"{2:PropertySeparator}{0:Columns}",
                        "\"ProductNeutralFormula\""),
                    new LogMessage(LogLevel.all_info, MessageType.contains, string.Empty, true,
                        "{0:Settings}{2:PropertySeparator}{0:DataSettings}{2:PropertySeparator}{0:Views}{2:PropertySeparator}\"Mixed Transition List\"{2:PropertySeparator}{0:Columns}",
                        "\"ProductAdduct\""),
                    new LogMessage(LogLevel.all_info, MessageType.contains, string.Empty, true,
                        "{0:Settings}{2:PropertySeparator}{0:DataSettings}{2:PropertySeparator}{0:Views}{2:PropertySeparator}\"Mixed Transition List\"{2:PropertySeparator}{0:Columns}",
                        "\"FragmentIonType\""),
                    new LogMessage(LogLevel.all_info, MessageType.contains, string.Empty, true,
                        "{0:Settings}{2:PropertySeparator}{0:DataSettings}{2:PropertySeparator}{0:Views}{2:PropertySeparator}\"Mixed Transition List\"{2:PropertySeparator}{0:Columns}",
                        "\"FragmentIonOrdinal\""),
                    new LogMessage(LogLevel.all_info, MessageType.contains, string.Empty, true,
                        "{0:Settings}{2:PropertySeparator}{0:DataSettings}{2:PropertySeparator}{0:Views}{2:PropertySeparator}\"Mixed Transition List\"{2:PropertySeparator}{0:Columns}",
                        "\"CleavageAa\""),
                    new LogMessage(LogLevel.all_info, MessageType.contains, string.Empty, true,
                        "{0:Settings}{2:PropertySeparator}{0:DataSettings}{2:PropertySeparator}{0:Views}{2:PropertySeparator}\"Mixed Transition List\"{2:PropertySeparator}{0:Columns}",
                        "\"LossNeutralMass\""),
                    new LogMessage(LogLevel.all_info, MessageType.contains, string.Empty, true,
                        "{0:Settings}{2:PropertySeparator}{0:DataSettings}{2:PropertySeparator}{0:Views}{2:PropertySeparator}\"Mixed Transition List\"{2:PropertySeparator}{0:Columns}",
                        "\"Losses\""),
                    new LogMessage(LogLevel.all_info, MessageType.contains, string.Empty, true,
                        "{0:Settings}{2:PropertySeparator}{0:DataSettings}{2:PropertySeparator}{0:Views}{2:PropertySeparator}\"Mixed Transition List\"{2:PropertySeparator}{0:Columns}",
                        "\"LibraryRank\""),
                    new LogMessage(LogLevel.all_info, MessageType.contains, string.Empty, true,
                        "{0:Settings}{2:PropertySeparator}{0:DataSettings}{2:PropertySeparator}{0:Views}{2:PropertySeparator}\"Mixed Transition List\"{2:PropertySeparator}{0:Columns}",
                        "\"LibraryIntensity\""),
                    new LogMessage(LogLevel.all_info, MessageType.contains, string.Empty, true,
                        "{0:Settings}{2:PropertySeparator}{0:DataSettings}{2:PropertySeparator}{0:Views}{2:PropertySeparator}\"Mixed Transition List\"{2:PropertySeparator}{0:Columns}",
                        "\"IsotopeDistIndex\""),
                    new LogMessage(LogLevel.all_info, MessageType.contains, string.Empty, true,
                        "{0:Settings}{2:PropertySeparator}{0:DataSettings}{2:PropertySeparator}{0:Views}{2:PropertySeparator}\"Mixed Transition List\"{2:PropertySeparator}{0:Columns}",
                        "\"IsotopeDistRank\""),
                    new LogMessage(LogLevel.all_info, MessageType.contains, string.Empty, true,
                        "{0:Settings}{2:PropertySeparator}{0:DataSettings}{2:PropertySeparator}{0:Views}{2:PropertySeparator}\"Mixed Transition List\"{2:PropertySeparator}{0:Columns}",
                        "\"IsotopeDistProportion\""),
                    new LogMessage(LogLevel.all_info, MessageType.contains, string.Empty, true,
                        "{0:Settings}{2:PropertySeparator}{0:DataSettings}{2:PropertySeparator}{0:Views}{2:PropertySeparator}\"Mixed Transition List\"{2:PropertySeparator}{0:Columns}",
                        "\"FullScanFilterWidth\""),
                    new LogMessage(LogLevel.all_info, MessageType.contains, string.Empty, true,
                        "{0:Settings}{2:PropertySeparator}{0:DataSettings}{2:PropertySeparator}{0:Views}{2:PropertySeparator}\"Mixed Transition List\"{2:PropertySeparator}{0:Columns}",
                        "\"IsDecoy\""),
                    new LogMessage(LogLevel.all_info, MessageType.contains, string.Empty, true,
                        "{0:Settings}{2:PropertySeparator}{0:DataSettings}{2:PropertySeparator}{0:Views}{2:PropertySeparator}\"Mixed Transition List\"{2:PropertySeparator}{0:Columns}",
                        "\"ProductDecoyMzShift\""),
                    new LogMessage(LogLevel.all_info, MessageType.is_, string.Empty, true,
                        "{0:Settings}{2:PropertySeparator}{0:DataSettings}{2:PropertySeparator}{0:Views}{2:PropertySeparator}\"Mixed Transition List\"{2:PropertySeparator}{0:Filters}",
                        "[  ]"),
                    new LogMessage(LogLevel.all_info, MessageType.is_, string.Empty, true,
                        "{0:Settings}{2:PropertySeparator}{0:DataSettings}{2:PropertySeparator}{0:Views}{2:PropertySeparator}\"Mixed Transition List\"{2:PropertySeparator}{0:Layouts}",
                        "[  ]"),
                }),
            new LogEntryMessages(
                new LogMessage(LogLevel.undo_redo, MessageType.changed, string.Empty, false,
                    "{0:TransitionSettings}{2:TabSeparator}{0:FullScan}"),
                new LogMessage(LogLevel.summary, MessageType.changed, string.Empty, false,
                    "{0:Settings}{2:PropertySeparator}{0:TransitionSettings}{2:TabSeparator}{0:FullScan}"),
                new[]
                {
                    new LogMessage(LogLevel.all_info, MessageType.changed_from_to, string.Empty, false,
                        "{0:Settings}{2:PropertySeparator}{0:TransitionSettings}{2:TabSeparator}{0:FullScan}{2:PropertySeparator}{0:AcquisitionMethod}",
                        "\"None\"",
                        "\"DIA\""),
                    new LogMessage(LogLevel.all_info, MessageType.changed_from_to, string.Empty, false,
                        "{0:Settings}{2:PropertySeparator}{0:TransitionSettings}{2:TabSeparator}{0:FullScan}{2:PropertySeparator}{0:IsolationScheme}",
                        "{2:Missing}",
                        "\"SWATH (15 m/z)\""),
                    new LogMessage(LogLevel.all_info, MessageType.is_, string.Empty, true,
                        "{0:Settings}{2:PropertySeparator}{0:TransitionSettings}{2:TabSeparator}{0:FullScan}{2:PropertySeparator}{0:IsolationScheme}{2:PropertySeparator}{0:PrecursorFilter}",
                        "{2:Missing}"),
                    new LogMessage(LogLevel.all_info, MessageType.is_, string.Empty, true,
                        "{0:Settings}{2:PropertySeparator}{0:TransitionSettings}{2:TabSeparator}{0:FullScan}{2:PropertySeparator}{0:IsolationScheme}{2:PropertySeparator}{0:IsolationWidth}",
                        "\"results\""),
                    new LogMessage(LogLevel.all_info, MessageType.is_, string.Empty, true,
                        "{0:Settings}{2:PropertySeparator}{0:TransitionSettings}{2:TabSeparator}{0:FullScan}{2:PropertySeparator}{0:IsolationScheme}{2:PropertySeparator}{0:SpecialHandling}",
                        "\"None\""),
                    new LogMessage(LogLevel.all_info, MessageType.is_, string.Empty, true,
                        "{0:Settings}{2:PropertySeparator}{0:TransitionSettings}{2:TabSeparator}{0:FullScan}{2:PropertySeparator}{0:IsolationScheme}{2:PropertySeparator}{0:WindowsPerScan}",
                        "{2:Missing}"),
                    new LogMessage(LogLevel.all_info, MessageType.contains, string.Empty, true,
                        "{0:Settings}{2:PropertySeparator}{0:TransitionSettings}{2:TabSeparator}{0:FullScan}{2:PropertySeparator}{0:IsolationScheme}{2:PropertySeparator}{0:PrespecifiedIsolationWindows}",
                        "{ {0:Start}=\"396\", {0:End}=\"410\", {0:StartMargin}=\"0.5\", {0:CERange}=\"5\" }"),
                    new LogMessage(LogLevel.all_info, MessageType.contains, string.Empty, true,
                        "{0:Settings}{2:PropertySeparator}{0:TransitionSettings}{2:TabSeparator}{0:FullScan}{2:PropertySeparator}{0:IsolationScheme}{2:PropertySeparator}{0:PrespecifiedIsolationWindows}",
                        "{ {0:Start}=\"410\", {0:End}=\"424\", {0:StartMargin}=\"0.5\", {0:CERange}=\"5\" }"),
                    new LogMessage(LogLevel.all_info, MessageType.contains, string.Empty, true,
                        "{0:Settings}{2:PropertySeparator}{0:TransitionSettings}{2:TabSeparator}{0:FullScan}{2:PropertySeparator}{0:IsolationScheme}{2:PropertySeparator}{0:PrespecifiedIsolationWindows}",
                        "{ {0:Start}=\"424\", {0:End}=\"438\", {0:StartMargin}=\"0.5\", {0:CERange}=\"5\" }"),
                    new LogMessage(LogLevel.all_info, MessageType.contains, string.Empty, true,
                        "{0:Settings}{2:PropertySeparator}{0:TransitionSettings}{2:TabSeparator}{0:FullScan}{2:PropertySeparator}{0:IsolationScheme}{2:PropertySeparator}{0:PrespecifiedIsolationWindows}",
                        "{ {0:Start}=\"438\", {0:End}=\"452\", {0:StartMargin}=\"0.5\", {0:CERange}=\"5\" }"),
                    new LogMessage(LogLevel.all_info, MessageType.contains, string.Empty, true,
                        "{0:Settings}{2:PropertySeparator}{0:TransitionSettings}{2:TabSeparator}{0:FullScan}{2:PropertySeparator}{0:IsolationScheme}{2:PropertySeparator}{0:PrespecifiedIsolationWindows}",
                        "{ {0:Start}=\"452\", {0:End}=\"466\", {0:StartMargin}=\"0.5\", {0:CERange}=\"5\" }"),
                    new LogMessage(LogLevel.all_info, MessageType.contains, string.Empty, true,
                        "{0:Settings}{2:PropertySeparator}{0:TransitionSettings}{2:TabSeparator}{0:FullScan}{2:PropertySeparator}{0:IsolationScheme}{2:PropertySeparator}{0:PrespecifiedIsolationWindows}",
                        "{ {0:Start}=\"466\", {0:End}=\"480\", {0:StartMargin}=\"0.5\", {0:CERange}=\"5\" }"),
                    new LogMessage(LogLevel.all_info, MessageType.contains, string.Empty, true,
                        "{0:Settings}{2:PropertySeparator}{0:TransitionSettings}{2:TabSeparator}{0:FullScan}{2:PropertySeparator}{0:IsolationScheme}{2:PropertySeparator}{0:PrespecifiedIsolationWindows}",
                        "{ {0:Start}=\"480\", {0:End}=\"494\", {0:StartMargin}=\"0.5\", {0:CERange}=\"5\" }"),
                    new LogMessage(LogLevel.all_info, MessageType.contains, string.Empty, true,
                        "{0:Settings}{2:PropertySeparator}{0:TransitionSettings}{2:TabSeparator}{0:FullScan}{2:PropertySeparator}{0:IsolationScheme}{2:PropertySeparator}{0:PrespecifiedIsolationWindows}",
                        "{ {0:Start}=\"494\", {0:End}=\"508\", {0:StartMargin}=\"0.5\", {0:CERange}=\"5\" }"),
                    new LogMessage(LogLevel.all_info, MessageType.contains, string.Empty, true,
                        "{0:Settings}{2:PropertySeparator}{0:TransitionSettings}{2:TabSeparator}{0:FullScan}{2:PropertySeparator}{0:IsolationScheme}{2:PropertySeparator}{0:PrespecifiedIsolationWindows}",
                        "{ {0:Start}=\"508\", {0:End}=\"522\", {0:StartMargin}=\"0.5\", {0:CERange}=\"5\" }"),
                    new LogMessage(LogLevel.all_info, MessageType.contains, string.Empty, true,
                        "{0:Settings}{2:PropertySeparator}{0:TransitionSettings}{2:TabSeparator}{0:FullScan}{2:PropertySeparator}{0:IsolationScheme}{2:PropertySeparator}{0:PrespecifiedIsolationWindows}",
                        "{ {0:Start}=\"522\", {0:End}=\"536\", {0:StartMargin}=\"0.5\", {0:CERange}=\"5\" }"),
                    new LogMessage(LogLevel.all_info, MessageType.contains, string.Empty, true,
                        "{0:Settings}{2:PropertySeparator}{0:TransitionSettings}{2:TabSeparator}{0:FullScan}{2:PropertySeparator}{0:IsolationScheme}{2:PropertySeparator}{0:PrespecifiedIsolationWindows}",
                        "{ {0:Start}=\"536\", {0:End}=\"550\", {0:StartMargin}=\"0.5\", {0:CERange}=\"5\" }"),
                    new LogMessage(LogLevel.all_info, MessageType.contains, string.Empty, true,
                        "{0:Settings}{2:PropertySeparator}{0:TransitionSettings}{2:TabSeparator}{0:FullScan}{2:PropertySeparator}{0:IsolationScheme}{2:PropertySeparator}{0:PrespecifiedIsolationWindows}",
                        "{ {0:Start}=\"550\", {0:End}=\"564\", {0:StartMargin}=\"0.5\", {0:CERange}=\"5\" }"),
                    new LogMessage(LogLevel.all_info, MessageType.contains, string.Empty, true,
                        "{0:Settings}{2:PropertySeparator}{0:TransitionSettings}{2:TabSeparator}{0:FullScan}{2:PropertySeparator}{0:IsolationScheme}{2:PropertySeparator}{0:PrespecifiedIsolationWindows}",
                        "{ {0:Start}=\"564\", {0:End}=\"578\", {0:StartMargin}=\"0.5\", {0:CERange}=\"5\" }"),
                    new LogMessage(LogLevel.all_info, MessageType.contains, string.Empty, true,
                        "{0:Settings}{2:PropertySeparator}{0:TransitionSettings}{2:TabSeparator}{0:FullScan}{2:PropertySeparator}{0:IsolationScheme}{2:PropertySeparator}{0:PrespecifiedIsolationWindows}",
                        "{ {0:Start}=\"578\", {0:End}=\"592\", {0:StartMargin}=\"0.5\", {0:CERange}=\"5\" }"),
                    new LogMessage(LogLevel.all_info, MessageType.contains, string.Empty, true,
                        "{0:Settings}{2:PropertySeparator}{0:TransitionSettings}{2:TabSeparator}{0:FullScan}{2:PropertySeparator}{0:IsolationScheme}{2:PropertySeparator}{0:PrespecifiedIsolationWindows}",
                        "{ {0:Start}=\"592\", {0:End}=\"606\", {0:StartMargin}=\"0.5\", {0:CERange}=\"5\" }"),
                    new LogMessage(LogLevel.all_info, MessageType.contains, string.Empty, true,
                        "{0:Settings}{2:PropertySeparator}{0:TransitionSettings}{2:TabSeparator}{0:FullScan}{2:PropertySeparator}{0:IsolationScheme}{2:PropertySeparator}{0:PrespecifiedIsolationWindows}",
                        "{ {0:Start}=\"606\", {0:End}=\"620\", {0:StartMargin}=\"0.5\", {0:CERange}=\"5\" }"),
                    new LogMessage(LogLevel.all_info, MessageType.contains, string.Empty, true,
                        "{0:Settings}{2:PropertySeparator}{0:TransitionSettings}{2:TabSeparator}{0:FullScan}{2:PropertySeparator}{0:IsolationScheme}{2:PropertySeparator}{0:PrespecifiedIsolationWindows}",
                        "{ {0:Start}=\"620\", {0:End}=\"634\", {0:StartMargin}=\"0.5\", {0:CERange}=\"5\" }"),
                    new LogMessage(LogLevel.all_info, MessageType.contains, string.Empty, true,
                        "{0:Settings}{2:PropertySeparator}{0:TransitionSettings}{2:TabSeparator}{0:FullScan}{2:PropertySeparator}{0:IsolationScheme}{2:PropertySeparator}{0:PrespecifiedIsolationWindows}",
                        "{ {0:Start}=\"634\", {0:End}=\"648\", {0:StartMargin}=\"0.5\", {0:CERange}=\"5\" }"),
                    new LogMessage(LogLevel.all_info, MessageType.contains, string.Empty, true,
                        "{0:Settings}{2:PropertySeparator}{0:TransitionSettings}{2:TabSeparator}{0:FullScan}{2:PropertySeparator}{0:IsolationScheme}{2:PropertySeparator}{0:PrespecifiedIsolationWindows}",
                        "{ {0:Start}=\"648\", {0:End}=\"662\", {0:StartMargin}=\"0.5\", {0:CERange}=\"5\" }"),
                    new LogMessage(LogLevel.all_info, MessageType.contains, string.Empty, true,
                        "{0:Settings}{2:PropertySeparator}{0:TransitionSettings}{2:TabSeparator}{0:FullScan}{2:PropertySeparator}{0:IsolationScheme}{2:PropertySeparator}{0:PrespecifiedIsolationWindows}",
                        "{ {0:Start}=\"662\", {0:End}=\"676\", {0:StartMargin}=\"0.5\", {0:CERange}=\"5\" }"),
                    new LogMessage(LogLevel.all_info, MessageType.contains, string.Empty, true,
                        "{0:Settings}{2:PropertySeparator}{0:TransitionSettings}{2:TabSeparator}{0:FullScan}{2:PropertySeparator}{0:IsolationScheme}{2:PropertySeparator}{0:PrespecifiedIsolationWindows}",
                        "{ {0:Start}=\"676\", {0:End}=\"690\", {0:StartMargin}=\"0.5\", {0:CERange}=\"5\" }"),
                    new LogMessage(LogLevel.all_info, MessageType.contains, string.Empty, true,
                        "{0:Settings}{2:PropertySeparator}{0:TransitionSettings}{2:TabSeparator}{0:FullScan}{2:PropertySeparator}{0:IsolationScheme}{2:PropertySeparator}{0:PrespecifiedIsolationWindows}",
                        "{ {0:Start}=\"690\", {0:End}=\"704\", {0:StartMargin}=\"0.5\", {0:CERange}=\"5\" }"),
                    new LogMessage(LogLevel.all_info, MessageType.contains, string.Empty, true,
                        "{0:Settings}{2:PropertySeparator}{0:TransitionSettings}{2:TabSeparator}{0:FullScan}{2:PropertySeparator}{0:IsolationScheme}{2:PropertySeparator}{0:PrespecifiedIsolationWindows}",
                        "{ {0:Start}=\"704\", {0:End}=\"718\", {0:StartMargin}=\"0.5\", {0:CERange}=\"5\" }"),
                    new LogMessage(LogLevel.all_info, MessageType.contains, string.Empty, true,
                        "{0:Settings}{2:PropertySeparator}{0:TransitionSettings}{2:TabSeparator}{0:FullScan}{2:PropertySeparator}{0:IsolationScheme}{2:PropertySeparator}{0:PrespecifiedIsolationWindows}",
                        "{ {0:Start}=\"718\", {0:End}=\"732\", {0:StartMargin}=\"0.5\", {0:CERange}=\"5\" }"),
                    new LogMessage(LogLevel.all_info, MessageType.contains, string.Empty, true,
                        "{0:Settings}{2:PropertySeparator}{0:TransitionSettings}{2:TabSeparator}{0:FullScan}{2:PropertySeparator}{0:IsolationScheme}{2:PropertySeparator}{0:PrespecifiedIsolationWindows}",
                        "{ {0:Start}=\"732\", {0:End}=\"746\", {0:StartMargin}=\"0.5\", {0:CERange}=\"5\" }"),
                    new LogMessage(LogLevel.all_info, MessageType.contains, string.Empty, true,
                        "{0:Settings}{2:PropertySeparator}{0:TransitionSettings}{2:TabSeparator}{0:FullScan}{2:PropertySeparator}{0:IsolationScheme}{2:PropertySeparator}{0:PrespecifiedIsolationWindows}",
                        "{ {0:Start}=\"746\", {0:End}=\"760\", {0:StartMargin}=\"0.5\", {0:CERange}=\"5\" }"),
                    new LogMessage(LogLevel.all_info, MessageType.contains, string.Empty, true,
                        "{0:Settings}{2:PropertySeparator}{0:TransitionSettings}{2:TabSeparator}{0:FullScan}{2:PropertySeparator}{0:IsolationScheme}{2:PropertySeparator}{0:PrespecifiedIsolationWindows}",
                        "{ {0:Start}=\"760\", {0:End}=\"774\", {0:StartMargin}=\"0.5\", {0:CERange}=\"5\" }"),
                    new LogMessage(LogLevel.all_info, MessageType.contains, string.Empty, true,
                        "{0:Settings}{2:PropertySeparator}{0:TransitionSettings}{2:TabSeparator}{0:FullScan}{2:PropertySeparator}{0:IsolationScheme}{2:PropertySeparator}{0:PrespecifiedIsolationWindows}",
                        "{ {0:Start}=\"774\", {0:End}=\"788\", {0:StartMargin}=\"0.5\", {0:CERange}=\"5\" }"),
                    new LogMessage(LogLevel.all_info, MessageType.contains, string.Empty, true,
                        "{0:Settings}{2:PropertySeparator}{0:TransitionSettings}{2:TabSeparator}{0:FullScan}{2:PropertySeparator}{0:IsolationScheme}{2:PropertySeparator}{0:PrespecifiedIsolationWindows}",
                        "{ {0:Start}=\"788\", {0:End}=\"802\", {0:StartMargin}=\"0.5\", {0:CERange}=\"5\" }"),
                    new LogMessage(LogLevel.all_info, MessageType.contains, string.Empty, true,
                        "{0:Settings}{2:PropertySeparator}{0:TransitionSettings}{2:TabSeparator}{0:FullScan}{2:PropertySeparator}{0:IsolationScheme}{2:PropertySeparator}{0:PrespecifiedIsolationWindows}",
                        "{ {0:Start}=\"802\", {0:End}=\"816\", {0:StartMargin}=\"0.5\", {0:CERange}=\"5\" }"),
                    new LogMessage(LogLevel.all_info, MessageType.contains, string.Empty, true,
                        "{0:Settings}{2:PropertySeparator}{0:TransitionSettings}{2:TabSeparator}{0:FullScan}{2:PropertySeparator}{0:IsolationScheme}{2:PropertySeparator}{0:PrespecifiedIsolationWindows}",
                        "{ {0:Start}=\"816\", {0:End}=\"830\", {0:StartMargin}=\"0.5\", {0:CERange}=\"5\" }"),
                    new LogMessage(LogLevel.all_info, MessageType.contains, string.Empty, true,
                        "{0:Settings}{2:PropertySeparator}{0:TransitionSettings}{2:TabSeparator}{0:FullScan}{2:PropertySeparator}{0:IsolationScheme}{2:PropertySeparator}{0:PrespecifiedIsolationWindows}",
                        "{ {0:Start}=\"830\", {0:End}=\"844\", {0:StartMargin}=\"0.5\", {0:CERange}=\"5\" }"),
                    new LogMessage(LogLevel.all_info, MessageType.contains, string.Empty, true,
                        "{0:Settings}{2:PropertySeparator}{0:TransitionSettings}{2:TabSeparator}{0:FullScan}{2:PropertySeparator}{0:IsolationScheme}{2:PropertySeparator}{0:PrespecifiedIsolationWindows}",
                        "{ {0:Start}=\"844\", {0:End}=\"858\", {0:StartMargin}=\"0.5\", {0:CERange}=\"5\" }"),
                    new LogMessage(LogLevel.all_info, MessageType.contains, string.Empty, true,
                        "{0:Settings}{2:PropertySeparator}{0:TransitionSettings}{2:TabSeparator}{0:FullScan}{2:PropertySeparator}{0:IsolationScheme}{2:PropertySeparator}{0:PrespecifiedIsolationWindows}",
                        "{ {0:Start}=\"858\", {0:End}=\"872\", {0:StartMargin}=\"0.5\", {0:CERange}=\"5\" }"),
                    new LogMessage(LogLevel.all_info, MessageType.contains, string.Empty, true,
                        "{0:Settings}{2:PropertySeparator}{0:TransitionSettings}{2:TabSeparator}{0:FullScan}{2:PropertySeparator}{0:IsolationScheme}{2:PropertySeparator}{0:PrespecifiedIsolationWindows}",
                        "{ {0:Start}=\"872\", {0:End}=\"886\", {0:StartMargin}=\"0.5\", {0:CERange}=\"5\" }"),
                    new LogMessage(LogLevel.all_info, MessageType.contains, string.Empty, true,
                        "{0:Settings}{2:PropertySeparator}{0:TransitionSettings}{2:TabSeparator}{0:FullScan}{2:PropertySeparator}{0:IsolationScheme}{2:PropertySeparator}{0:PrespecifiedIsolationWindows}",
                        "{ {0:Start}=\"886\", {0:End}=\"900\", {0:StartMargin}=\"0.5\", {0:CERange}=\"5\" }"),
                    new LogMessage(LogLevel.all_info, MessageType.contains, string.Empty, true,
                        "{0:Settings}{2:PropertySeparator}{0:TransitionSettings}{2:TabSeparator}{0:FullScan}{2:PropertySeparator}{0:IsolationScheme}{2:PropertySeparator}{0:PrespecifiedIsolationWindows}",
                        "{ {0:Start}=\"900\", {0:End}=\"914\", {0:StartMargin}=\"0.5\", {0:CERange}=\"5\" }"),
                    new LogMessage(LogLevel.all_info, MessageType.contains, string.Empty, true,
                        "{0:Settings}{2:PropertySeparator}{0:TransitionSettings}{2:TabSeparator}{0:FullScan}{2:PropertySeparator}{0:IsolationScheme}{2:PropertySeparator}{0:PrespecifiedIsolationWindows}",
                        "{ {0:Start}=\"914\", {0:End}=\"928\", {0:StartMargin}=\"0.5\", {0:CERange}=\"5\" }"),
                    new LogMessage(LogLevel.all_info, MessageType.contains, string.Empty, true,
                        "{0:Settings}{2:PropertySeparator}{0:TransitionSettings}{2:TabSeparator}{0:FullScan}{2:PropertySeparator}{0:IsolationScheme}{2:PropertySeparator}{0:PrespecifiedIsolationWindows}",
                        "{ {0:Start}=\"928\", {0:End}=\"942\", {0:StartMargin}=\"0.5\", {0:CERange}=\"5\" }"),
                    new LogMessage(LogLevel.all_info, MessageType.contains, string.Empty, true,
                        "{0:Settings}{2:PropertySeparator}{0:TransitionSettings}{2:TabSeparator}{0:FullScan}{2:PropertySeparator}{0:IsolationScheme}{2:PropertySeparator}{0:PrespecifiedIsolationWindows}",
                        "{ {0:Start}=\"942\", {0:End}=\"956\", {0:StartMargin}=\"0.5\", {0:CERange}=\"5\" }"),
                    new LogMessage(LogLevel.all_info, MessageType.contains, string.Empty, true,
                        "{0:Settings}{2:PropertySeparator}{0:TransitionSettings}{2:TabSeparator}{0:FullScan}{2:PropertySeparator}{0:IsolationScheme}{2:PropertySeparator}{0:PrespecifiedIsolationWindows}",
                        "{ {0:Start}=\"956\", {0:End}=\"970\", {0:StartMargin}=\"0.5\", {0:CERange}=\"5\" }"),
                    new LogMessage(LogLevel.all_info, MessageType.contains, string.Empty, true,
                        "{0:Settings}{2:PropertySeparator}{0:TransitionSettings}{2:TabSeparator}{0:FullScan}{2:PropertySeparator}{0:IsolationScheme}{2:PropertySeparator}{0:PrespecifiedIsolationWindows}",
                        "{ {0:Start}=\"970\", {0:End}=\"984\", {0:StartMargin}=\"0.5\", {0:CERange}=\"5\" }"),
                    new LogMessage(LogLevel.all_info, MessageType.contains, string.Empty, true,
                        "{0:Settings}{2:PropertySeparator}{0:TransitionSettings}{2:TabSeparator}{0:FullScan}{2:PropertySeparator}{0:IsolationScheme}{2:PropertySeparator}{0:PrespecifiedIsolationWindows}",
                        "{ {0:Start}=\"984\", {0:End}=\"998\", {0:StartMargin}=\"0.5\", {0:CERange}=\"5\" }"),
                    new LogMessage(LogLevel.all_info, MessageType.contains, string.Empty, true,
                        "{0:Settings}{2:PropertySeparator}{0:TransitionSettings}{2:TabSeparator}{0:FullScan}{2:PropertySeparator}{0:IsolationScheme}{2:PropertySeparator}{0:PrespecifiedIsolationWindows}",
                        "{ {0:Start}=\"998\", {0:End}=\"1012\", {0:StartMargin}=\"0.5\", {0:CERange}=\"10\" }"),
                    new LogMessage(LogLevel.all_info, MessageType.contains, string.Empty, true,
                        "{0:Settings}{2:PropertySeparator}{0:TransitionSettings}{2:TabSeparator}{0:FullScan}{2:PropertySeparator}{0:IsolationScheme}{2:PropertySeparator}{0:PrespecifiedIsolationWindows}",
                        "{ {0:Start}=\"1012\", {0:End}=\"1026\", {0:StartMargin}=\"0.5\", {0:CERange}=\"10\" }"),
                    new LogMessage(LogLevel.all_info, MessageType.contains, string.Empty, true,
                        "{0:Settings}{2:PropertySeparator}{0:TransitionSettings}{2:TabSeparator}{0:FullScan}{2:PropertySeparator}{0:IsolationScheme}{2:PropertySeparator}{0:PrespecifiedIsolationWindows}",
                        "{ {0:Start}=\"1026\", {0:End}=\"1040\", {0:StartMargin}=\"0.5\", {0:CERange}=\"10\" }"),
                    new LogMessage(LogLevel.all_info, MessageType.contains, string.Empty, true,
                        "{0:Settings}{2:PropertySeparator}{0:TransitionSettings}{2:TabSeparator}{0:FullScan}{2:PropertySeparator}{0:IsolationScheme}{2:PropertySeparator}{0:PrespecifiedIsolationWindows}",
                        "{ {0:Start}=\"1040\", {0:End}=\"1054\", {0:StartMargin}=\"0.5\", {0:CERange}=\"10\" }"),
                    new LogMessage(LogLevel.all_info, MessageType.contains, string.Empty, true,
                        "{0:Settings}{2:PropertySeparator}{0:TransitionSettings}{2:TabSeparator}{0:FullScan}{2:PropertySeparator}{0:IsolationScheme}{2:PropertySeparator}{0:PrespecifiedIsolationWindows}",
                        "{ {0:Start}=\"1054\", {0:End}=\"1068\", {0:StartMargin}=\"0.5\", {0:CERange}=\"10\" }"),
                    new LogMessage(LogLevel.all_info, MessageType.contains, string.Empty, true,
                        "{0:Settings}{2:PropertySeparator}{0:TransitionSettings}{2:TabSeparator}{0:FullScan}{2:PropertySeparator}{0:IsolationScheme}{2:PropertySeparator}{0:PrespecifiedIsolationWindows}",
                        "{ {0:Start}=\"1068\", {0:End}=\"1082\", {0:StartMargin}=\"0.5\", {0:CERange}=\"10\" }"),
                    new LogMessage(LogLevel.all_info, MessageType.contains, string.Empty, true,
                        "{0:Settings}{2:PropertySeparator}{0:TransitionSettings}{2:TabSeparator}{0:FullScan}{2:PropertySeparator}{0:IsolationScheme}{2:PropertySeparator}{0:PrespecifiedIsolationWindows}",
                        "{ {0:Start}=\"1082\", {0:End}=\"1096\", {0:StartMargin}=\"0.5\", {0:CERange}=\"10\" }"),
                    new LogMessage(LogLevel.all_info, MessageType.contains, string.Empty, true,
                        "{0:Settings}{2:PropertySeparator}{0:TransitionSettings}{2:TabSeparator}{0:FullScan}{2:PropertySeparator}{0:IsolationScheme}{2:PropertySeparator}{0:PrespecifiedIsolationWindows}",
                        "{ {0:Start}=\"1096\", {0:End}=\"1110\", {0:StartMargin}=\"0.5\", {0:CERange}=\"10\" }"),
                    new LogMessage(LogLevel.all_info, MessageType.contains, string.Empty, true,
                        "{0:Settings}{2:PropertySeparator}{0:TransitionSettings}{2:TabSeparator}{0:FullScan}{2:PropertySeparator}{0:IsolationScheme}{2:PropertySeparator}{0:PrespecifiedIsolationWindows}",
                        "{ {0:Start}=\"1110\", {0:End}=\"1124\", {0:StartMargin}=\"0.5\", {0:CERange}=\"10\" }"),
                    new LogMessage(LogLevel.all_info, MessageType.contains, string.Empty, true,
                        "{0:Settings}{2:PropertySeparator}{0:TransitionSettings}{2:TabSeparator}{0:FullScan}{2:PropertySeparator}{0:IsolationScheme}{2:PropertySeparator}{0:PrespecifiedIsolationWindows}",
                        "{ {0:Start}=\"1124\", {0:End}=\"1138\", {0:StartMargin}=\"0.5\", {0:CERange}=\"10\" }"),
                    new LogMessage(LogLevel.all_info, MessageType.contains, string.Empty, true,
                        "{0:Settings}{2:PropertySeparator}{0:TransitionSettings}{2:TabSeparator}{0:FullScan}{2:PropertySeparator}{0:IsolationScheme}{2:PropertySeparator}{0:PrespecifiedIsolationWindows}",
                        "{ {0:Start}=\"1138\", {0:End}=\"1152\", {0:StartMargin}=\"0.5\", {0:CERange}=\"10\" }"),
                    new LogMessage(LogLevel.all_info, MessageType.contains, string.Empty, true,
                        "{0:Settings}{2:PropertySeparator}{0:TransitionSettings}{2:TabSeparator}{0:FullScan}{2:PropertySeparator}{0:IsolationScheme}{2:PropertySeparator}{0:PrespecifiedIsolationWindows}",
                        "{ {0:Start}=\"1152\", {0:End}=\"1166\", {0:StartMargin}=\"0.5\", {0:CERange}=\"10\" }"),
                    new LogMessage(LogLevel.all_info, MessageType.contains, string.Empty, true,
                        "{0:Settings}{2:PropertySeparator}{0:TransitionSettings}{2:TabSeparator}{0:FullScan}{2:PropertySeparator}{0:IsolationScheme}{2:PropertySeparator}{0:PrespecifiedIsolationWindows}",
                        "{ {0:Start}=\"1166\", {0:End}=\"1180\", {0:StartMargin}=\"0.5\", {0:CERange}=\"10\" }"),
                    new LogMessage(LogLevel.all_info, MessageType.contains, string.Empty, true,
                        "{0:Settings}{2:PropertySeparator}{0:TransitionSettings}{2:TabSeparator}{0:FullScan}{2:PropertySeparator}{0:IsolationScheme}{2:PropertySeparator}{0:PrespecifiedIsolationWindows}",
                        "{ {0:Start}=\"1180\", {0:End}=\"1194\", {0:StartMargin}=\"0.5\", {0:CERange}=\"10\" }"),
                    new LogMessage(LogLevel.all_info, MessageType.contains, string.Empty, true,
                        "{0:Settings}{2:PropertySeparator}{0:TransitionSettings}{2:TabSeparator}{0:FullScan}{2:PropertySeparator}{0:IsolationScheme}{2:PropertySeparator}{0:PrespecifiedIsolationWindows}",
                        "{ {0:Start}=\"1194\", {0:End}=\"1208\", {0:StartMargin}=\"0.5\", {0:CERange}=\"10\" }"),
                    new LogMessage(LogLevel.all_info, MessageType.contains, string.Empty, true,
                        "{0:Settings}{2:PropertySeparator}{0:TransitionSettings}{2:TabSeparator}{0:FullScan}{2:PropertySeparator}{0:IsolationScheme}{2:PropertySeparator}{0:PrespecifiedIsolationWindows}",
                        "{ {0:Start}=\"1208\", {0:End}=\"1222\", {0:StartMargin}=\"0.5\", {0:CERange}=\"10\" }"),
                    new LogMessage(LogLevel.all_info, MessageType.contains, string.Empty, true,
                        "{0:Settings}{2:PropertySeparator}{0:TransitionSettings}{2:TabSeparator}{0:FullScan}{2:PropertySeparator}{0:IsolationScheme}{2:PropertySeparator}{0:PrespecifiedIsolationWindows}",
                        "{ {0:Start}=\"1222\", {0:End}=\"1236\", {0:StartMargin}=\"0.5\", {0:CERange}=\"10\" }"),
                    new LogMessage(LogLevel.all_info, MessageType.contains, string.Empty, true,
                        "{0:Settings}{2:PropertySeparator}{0:TransitionSettings}{2:TabSeparator}{0:FullScan}{2:PropertySeparator}{0:IsolationScheme}{2:PropertySeparator}{0:PrespecifiedIsolationWindows}",
                        "{ {0:Start}=\"1236\", {0:End}=\"1249\", {0:StartMargin}=\"0.5\", {0:CERange}=\"10\" }"),
                    new LogMessage(LogLevel.all_info, MessageType.changed_from_to, string.Empty, false,
                        "{0:Settings}{2:PropertySeparator}{0:TransitionSettings}{2:TabSeparator}{0:FullScan}{2:PropertySeparator}{0:ProductMassAnalyzer}",
                        "\"none\"",
                        "\"qit\""),
                    new LogMessage(LogLevel.all_info, MessageType.changed_from_to, string.Empty, false,
                        "{0:Settings}{2:PropertySeparator}{0:TransitionSettings}{2:TabSeparator}{0:FullScan}{2:PropertySeparator}{0:Resolution}",
                        "{2:Missing}",
                        "\"0.7\""),
                }),
        };
        #endregion

        private static readonly LogEntry[] LOG_ENTRIES =
        {
            // Basic property change
            new LogEntry(() => SkylineWindow.ChangeSettings(
                    SkylineWindow.DocumentUI.Settings.ChangeTransitionPrediction(p =>
                        p.ChangePrecursorMassType(MassType.Average)), true), LOG_ENTRY_MESSAGESES[0]),

            // Collection change: named to named
            new LogEntry(() => SkylineWindow.ChangeSettings(
                SkylineWindow.DocumentUI.Settings.ChangeTransitionPrediction(p =>
                    p.ChangeCollisionEnergy(Settings.Default.CollisionEnergyList[1])), true), LOG_ENTRY_MESSAGESES[1]),

            // Collection change: null to named
            new LogEntry(() => SkylineWindow.ChangeSettings(
                SkylineWindow.DocumentUI.Settings.ChangeTransitionPrediction(p =>
                    p.ChangeDeclusteringPotential(Settings.Default.DeclusterPotentialList[1])), true), LOG_ENTRY_MESSAGESES[2]),

            // Collection change: multiple named elements with sub properties added
            new LogEntry(() => SkylineWindow.ChangeSettings(
                SkylineWindow.DocumentUI.Settings.ChangeTransitionFilter(p =>
                    p.ChangeMeasuredIons(new[] { Settings.Default.MeasuredIonList[0], Settings.Default.MeasuredIonList[1] })), true), LOG_ENTRY_MESSAGESES[3]),

            // Custom localizer 1
            new LogEntry(() =>
                {
                    var settings = SkylineWindow.DocumentUI.Settings.ChangeTransitionPrediction(p =>
                        p.ChangePrecursorMassType(MassType.Monoisotopic)); // Need monoisotopic masses for next change
                    settings = settings.ChangeTransitionFullScan(p =>
                        p.ChangePrecursorResolution(FullScanMassAnalyzerType.centroided, 10, null));

                    SkylineWindow.ChangeSettings(settings, true);
                }, LOG_ENTRY_MESSAGESES[4]),
            
            // Custom localizer 2 and named to null change
            new LogEntry(() =>
                {
                    SkylineWindow.ChangeSettings(SkylineWindow.DocumentUI.Settings.ChangeTransitionFullScan(f => f.ChangePrecursorResolution(FullScanMassAnalyzerType.qit, 0.7, null)), true);
                }, LOG_ENTRY_MESSAGESES[5]),

            // Undo redo shortened names removed
            new LogEntry(() =>
                {
                    SkylineWindow.ChangeSettings(SkylineWindow.DocumentUI.Settings.ChangeAnnotationDefs(l =>
                    {
                        var newList = new List<AnnotationDef>(l);
                        newList.RemoveAt(0);
                        return newList;
                    }), true);
                }, LOG_ENTRY_MESSAGESES[6]),

            // Undo redo shortened names added
            new LogEntry(() =>
                {
                    SkylineWindow.ChangeSettings(SkylineWindow.DocumentUI.Settings.ChangeAnnotationDefs(l =>
                    {
                        var newList = new List<AnnotationDef>(l);
                        newList.Insert(0, Settings.Default.AnnotationDefList[0]);
                        return newList;
                    }), true);
                }, LOG_ENTRY_MESSAGESES[7]),
            
            // Add Mixed Transition List
            new LogEntry(() =>
                {
                    SkylineWindow.ChangeSettings(SkylineWindow.DocumentUI.Settings.ChangeDataSettings(d =>
                    {
                        var settingsViewSpecList = Settings.Default.PersistedViews.GetViewSpecList(PersistedViews.MainGroup.Id);
                        Assert.IsNotNull(settingsViewSpecList);
                        var viewSpecs = new List<ViewSpec>(d.ViewSpecList.ViewSpecs);
                        var viewLayouts = new List<ViewLayoutList>(d.ViewSpecList.ViewLayouts);
                        
                        var mixedTransitionList = settingsViewSpecList.ViewSpecs.FirstOrDefault(v => v.Name == Resources.SkylineViewContext_GetTransitionListReportSpec_Mixed_Transition_List);
                        Assert.IsNotNull(mixedTransitionList);
                        viewSpecs.Add(mixedTransitionList);
                        var newList = new ViewSpecList(viewSpecs, viewLayouts);
                        return d.ChangeViewSpecList(newList);
                    }), true);
                }, LOG_ENTRY_MESSAGESES[8]),

            // Isolation Scheme
            new LogEntry(() =>
                {
                    SkylineWindow.ChangeSettings(SkylineWindow.DocumentUI.Settings.ChangeTransitionSettings(t =>
                        {
                            return t.ChangeFullScan(t.FullScan.ChangeAcquisitionMethod(FullScanAcquisitionMethod.DIA,
                                Settings.Default.IsolationSchemeList.FirstOrDefault(i => i.Name == "SWATH (15 m/z)")));
                        }), true);
                }, LOG_ENTRY_MESSAGESES[9]),
        };
    }
}