/*
 * Original author: Nicholas Shulman <nicksh .at. u.washington.edu>,
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
using SkylineTool;

namespace ToolServiceCmd
{
    public class ToolServiceClient : RemoteClient, IToolService, IDisposable
    {
        public ToolServiceClient(string connectionName)
            : base(connectionName)
        {
        }

        public string GetReport(string toolName, string reportName)
        {
            return RemoteCallFunction(GetReport, toolName, reportName);
        }

        public string GetReportFromDefinition(string reportDefinition)
        {
            return RemoteCallFunction(GetReportFromDefinition, reportDefinition);
        }

        public DocumentLocation GetDocumentLocation()
        {
            return RemoteCallFunction(GetDocumentLocation);
        }

        public void SetDocumentLocation(DocumentLocation documentLocation)
        {
            RemoteCall(SetDocumentLocation, documentLocation);
        }

        public string GetDocumentLocationName()
        {
            return RemoteCallFunction(GetDocumentLocationName);
        }

        public string GetReplicateName()
        {
            return RemoteCallFunction(GetReplicateName);
        }

        public Chromatogram[] GetChromatograms(DocumentLocation documentLocation)
        {
            return RemoteCallFunction(GetChromatograms, documentLocation);
        }

        public string GetDocumentPath()
        {
            return RemoteCallFunction(GetDocumentPath);
        }

        public SkylineTool.Version GetVersion()
        {
            return (SkylineTool.Version)RemoteCallFunction((Func<object>)GetVersion);
        }

        public void ImportFasta(string textFasta)
        {
            RemoteCall(ImportFasta, textFasta);
        }

        public void InsertSmallMoleculeTransitionList(string textCsv)
        {
            RemoteCall(InsertSmallMoleculeTransitionList, textCsv);
        }

        public void AddSpectralLibrary(string libraryName, string libraryPath)
        {
            RemoteCall(AddSpectralLibrary, libraryName, libraryPath);
        }

        public void AddDocumentChangeReceiver(string receiverName, string name)
        {
            RemoteCall(AddDocumentChangeReceiver, receiverName, name);
        }

        public void RemoveDocumentChangeReceiver(string receiverName)
        {
            RemoteCall(RemoveDocumentChangeReceiver, receiverName);
        }

        public void Dispose()
        {
            
        }
    }
}
