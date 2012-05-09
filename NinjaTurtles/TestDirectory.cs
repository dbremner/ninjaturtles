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
using System.IO;

namespace NinjaTurtles
{
	public class TestDirectory : IDisposable
	{
		private readonly string _folder;

		public TestDirectory() : this(null)
		{
		}
		
		public TestDirectory(string sourceFolder)
		{
			_folder = Path.Combine(Path.GetTempPath(),
			                       "NinjaTurtles",
			                       Guid.NewGuid().ToString("N"));
			if (!Directory.Exists(_folder))
			{
				Directory.CreateDirectory(_folder);
			}
			
			if (!string.IsNullOrEmpty(sourceFolder))
			{
				CopyDirectoryContents(sourceFolder, _folder);
			}
		}
		
		public void SaveAssembly(Module module)
		{
			module.AssemblyDefinition.Write(Path.Combine(_folder, Path.GetFileName(module.AssemblyLocation)));
		}
		
		private static void CopyDirectoryContents(string sourceFolder, string targetFolder)
		{
			foreach (var file in Directory.GetFiles(sourceFolder))
			{
				string targetFile = Path.Combine(targetFolder, Path.GetFileName(file));
				File.Copy(file, targetFile);
			}
			foreach (var directory in Directory.GetDirectories(sourceFolder))
			{
				string targetDirectory = Path.Combine(targetFolder, Path.GetFileName(directory));
				Directory.CreateDirectory(targetDirectory);
				CopyDirectoryContents(directory, targetDirectory);
			}
		}
		
		public string FullName
		{
			get { return _folder; }
		}

		public void Dispose()
		{
            try
            {
                Directory.Delete(_folder, true);
            }
            catch {}
		}
	}
}

