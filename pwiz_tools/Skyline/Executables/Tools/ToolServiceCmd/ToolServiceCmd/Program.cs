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
using CommandLine;

namespace ToolServiceCmd
{
    static class Program
    {
        public const string TOOL_NAME = "ToolServiceCmd";
        static int Main(string[] args)
        {
            var parseResult = Parser.Default.ParseArguments(args, 
                typeof(GetVersionCommand),
                typeof(GetReportCommand),
                typeof(GetDocumentPath)
            );
            Parsed<object> parsed = parseResult as Parsed<object>;
            if (parsed == null)
                return 1;
            return ((BaseCommand) parsed.Value).PerformCommand();
        }

        [Verb("GetVersion")]
        public class GetVersionCommand : BaseCommand
        {
            public override int PerformCommand()
            {
                using (var client = GetSkylineToolClient())
                {
                    Console.Out.WriteLine("{0}", client.GetVersion());
                }
                return 0;
            }
        }

        [Verb("GetDocumentPath")]
        public class GetDocumentPath : BaseCommand
        {
            public override int PerformCommand()
            {
                using (var client = GetSkylineToolClient())
                {
                    Console.Out.WriteLine("{0}", client.GetDocumentPath());
                }
                return 0;
            }
        }
    }
}
