﻿/*
 * Original author: Don Marsh <donmarsh .at. u.washington.edu>,
 *                  MacCoss Lab, Department of Genome Sciences, UW
 *
 * Copyright 2014 University of Washington - Seattle, WA
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

namespace SkylineTool
{
    public class SkylineToolClient : IDisposable
    {
        public event EventHandler DocumentChanged; 
        
        private readonly Client _client;
        private readonly string _toolName;
        private readonly DocumentChangeReceiver _documentChangeReceiver;

        public SkylineToolClient(string connectionName, string toolName)
        {
            _client = new Client(connectionName);
            _toolName = toolName;
            _documentChangeReceiver = new DocumentChangeReceiver(Guid.NewGuid().ToString(), this);
            _client.AddDocumentChangeReceiver(_documentChangeReceiver.ConnectionName);
        }

        public void Dispose()
        {
            _client.RemoveDocumentChangeReceiver(_documentChangeReceiver.ConnectionName);
            _documentChangeReceiver.Dispose();
            _client.Dispose();
        }

        public IReport GetReport(string reportName)
        {
            var reportCsv = _client.GetReport(_toolName + "," + reportName); // Not L10N
            return new Report(reportCsv);
        }

        public void Select(string link)
        {
            _client.Select(link);
        }

        public string DocumentPath
        {
            get { return _client.GetDocumentPath(); }
        }

        public Version SkylineVersion
        {
            get
            {
                var versionString = _client.GetVersion();
                return new Version(versionString);
            }
        }

        private class DocumentChangeReceiver : RemoteService, IDocumentChangeReceiver
        {
            private readonly SkylineToolClient _toolClient;

            public DocumentChangeReceiver(string connectionName, SkylineToolClient toolClient)
                : base(connectionName)
            {
                _toolClient = toolClient;
            }

            public void DocumentChanged()
            {
                if (_toolClient.DocumentChanged != null)
                    _toolClient.DocumentChanged(_toolClient, null);
            }
        }

        private class Client : RemoteClient, ISkylineTool
        {
            public Client(string connectionName)
                : base(connectionName)
            {
            }

            public string GetReport(string toolReportName)
            {
                return RemoteCall("GetReport", true, toolReportName); // Not L10N
            }

            public void Select(string link)
            {
                RemoteCall("Select", false, link); // Not L10N
            }

            public string GetDocumentPath()
            {
                return RemoteCall("GetDocumentPath", true); // Not L10N
            }

            public string GetVersion()
            {
                return RemoteCall("GetVersion", true); // Not L10N
            }

            public void AddDocumentChangeReceiver(string receiverName)
            {
                RemoteCall("AddDocumentChangeReceiver", false, receiverName); // Not L10N
            }

            public void RemoveDocumentChangeReceiver(string receiverName)
            {
                RemoteCall("RemoveDocumentChangeReceiver", false, receiverName); // Not L10N
            }
        }

        private class Report : IReport
        {
            public Report(string reportCsv)
            {
                var lines = reportCsv.Split(new [] {"\r\n"}, StringSplitOptions.None); // Not L10N
                ColumnNames = lines[0].Split(',');
                Cells = new string[lines.Length-1][];
                CellValues = new double?[lines.Length-1][];
                for (int i = 0; i < lines.Length-1; i++)
                {
                    Cells[i] = new string[ColumnNames.Length];
                    CellValues[i] = new double?[ColumnNames.Length];
                    var row = lines[i + 1].Split(',');
                    for (int j = 0; j < row.Length; j++)
                    {
                        Cells[i][j] = row[j];
                        double value;
                        if (double.TryParse(row[j], out value))
                            CellValues[i][j] = value;
                    }
                }
            }

            public string[] ColumnNames { get; private set; }
            public string[][] Cells { get; private set; }
            public double?[][] CellValues { get; private set; }
            
            public string Cell(int row, string columnName)
            {
                int column = FindColumn(columnName);
                return column >= 0 ? Cells[row][column] : null;
            }

            public double? CellValue(int row, string columnName)
            {
                int column = FindColumn(columnName);
                return column >= 0 ? CellValues[row][column] : null;
            }

            private int FindColumn(string columnName)
            {
                for (int i = 0; i < ColumnNames.Length; i++)
                {
                    if (string.Equals(columnName, ColumnNames[i], StringComparison.InvariantCultureIgnoreCase))
                        return i;
                }
                return -1;
            }
        }
    }

    public interface IReport
    {
        string[] ColumnNames { get; }
        string[][] Cells { get; }
        double?[][] CellValues { get; }
        string Cell(int row, string column);
        double? CellValue(int row, string column);
    }

    public class Version
    {
        public int Major { get; private set; }
        public int Minor { get; private set; }
        public int Build { get; private set; }
        public int Revision { get; private set; }

        public Version(int major, int minor, int build, int revision)
        {
            Major = major;
            Minor = minor;
            Build = build;
            Revision = revision;
        }

        public Version(string version)
        {
            var parts = version.Split(',');
            Major = int.Parse(parts[0]);
            Minor = int.Parse(parts[1]);
            Build = int.Parse(parts[2]);
            Revision = int.Parse(parts[3]);
        }

        public override string ToString()
        {
            return string.Format("{0},{1},{2},{3}", Major, Minor, Build, Revision); // Not L10N
        }
    }
}
