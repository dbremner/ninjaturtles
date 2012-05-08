#region Copyright & licence

// This file is part of NinjaTurtles.
// 
// NinjaTurtles is free software: you can redistribute it and/or modify
// it under the terms of the GNU Lesser General Public License as
// published by the Free Software Foundation, either version 3 of the
// License, or (at your option) any later version.
// 
// NinjaTurtles is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU Lesser General Public License for more details.
// 
// You should have received a copy of the GNU Lesser General Public
// License along with Refix.  If not, see <http://www.gnu.org/licenses/>.
// 
// Copyright (C) 2012 David Musgrove and others.

#endregion

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;

namespace NinjaTurtles
{
    public static class ConsoleProcessFactory
    {
        internal static bool IsMono { get; set; }
        internal static bool IsWindows { get; set; }

        static ConsoleProcessFactory()
        {
            IsMono = Type.GetType("Mono.Runtime") != null;
            IsWindows = Environment.OSVersion.Platform.ToString().StartsWith("Win")
                        || Environment.OSVersion.Platform == PlatformID.Xbox;

        }

        public static Process CreateProcess(string exeName, string arguments)
        {
            exeName = FindExecutable(exeName);

            if (IsMono)
            {
                arguments = string.Format("--runtime=v4.0 \"{0}\" {1}", exeName, arguments);
                exeName = "mono";
            }

            arguments = string.Format(arguments, IsWindows ? "/" : "-");

            var processStartInfo = new ProcessStartInfo(exeName, arguments);
            processStartInfo.UseShellExecute = false;
            processStartInfo.WindowStyle = ProcessWindowStyle.Hidden;
            processStartInfo.RedirectStandardOutput = true;

            var process = new Process
            {
                StartInfo = processStartInfo
            };

            return process;
        }

        private static string FindExecutable(string exeName)
        {
            var searchPath = new List<string>();
            string programFilesFolder = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles);
            searchPath.AddRange(new[]
                                    {
                                        Path.Combine(programFilesFolder, "NUnit 2.6\\bin"),
                                        Path.Combine(programFilesFolder, "NUnit 2.5\\bin")
                                    });
            string programFilesX86Folder = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86);
            if (!string.IsNullOrEmpty(programFilesX86Folder))
            {
                searchPath.AddRange(new[]
                        {
                            Path.Combine(programFilesX86Folder, "NUnit 2.6\\bin"),
                            Path.Combine(programFilesX86Folder, "NUnit 2.5\\bin")
                        });
            }
            string environmentSearchPath = Environment.GetEnvironmentVariable("PATH") ?? "";
            searchPath.AddRange(environmentSearchPath
                .Split(IsWindows ? ';' : ':'));

            foreach (string folder in searchPath)
            {
                string fullExePath = Path.Combine(folder, exeName);
                if (File.Exists(fullExePath))
                {
                    return fullExePath;
                }
            }
            return exeName;
        }
    }
}
