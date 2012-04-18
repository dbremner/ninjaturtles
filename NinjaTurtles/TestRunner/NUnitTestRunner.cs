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
// Copyright (C) 2012 David Musgrove.

#endregion

using System.Collections.Generic;
using System.IO;

namespace NinjaTurtles.TestRunner
{
    public class NUnitTestRunner : ConsoleTestRunner
    {
        public NUnitTestRunner()
        {
            Path = @"..\..\..\packages\NUnit.Runners.2.6.0.12051\tools\";
        }

        public string Path { get; set; }

        protected override string GetCommandLine(string testLibrary, IEnumerable<string> tests)
        {
            string path = System.IO.Path.GetTempFileName();
            File.WriteAllLines(path, tests);
            return string.Format("\"{0}nunit-console.exe\" \"{1}\" /runlist=\"{2}\"",
                Path, testLibrary, path);
        }
    }
}
