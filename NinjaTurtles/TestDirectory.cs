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
// License along with NinjaTurtles.  If not, see <http://www.gnu.org/licenses/>.
// 
// Copyright (C) 2012 David Musgrove and others.

#endregion

using System;
using System.IO;

using NLog;

namespace NinjaTurtles
{
	public class TestDirectory : IDisposable
    {
        #region Logging

        private static Logger _log = LogManager.GetCurrentClassLogger();

        #endregion

        private readonly string _folder;

		public TestDirectory()
		{
            _folder = Path.Combine(Path.GetTempPath(),
                                   "NinjaTurtles",
                                   Guid.NewGuid().ToString("N"));
            _log.Debug("Creating folder \"{0}\".", _folder);
            Directory.CreateDirectory(_folder);
        }
		
		public TestDirectory(string sourceFolder) : this()
		{
            _log.Debug("Copying contents from folder \"{0}\".", sourceFolder);
            CopyDirectoryContents(sourceFolder, _folder);
		}
		
		public void SaveAssembly(Module module)
		{
		    string fileName = Path.GetFileName(module.AssemblyLocation);
		    string path = Path.Combine(_folder, fileName);
            _log.Debug("Writing assembly \"{0}\" to \"{1}\".", fileName, _folder);
            module.AssemblyDefinition.Write(path);
		}

	    private static void CopyDirectoryContents
            (string directory, string targetDirectory)
		{
			foreach (var file in Directory.GetFiles(directory))
			{
				string target = Path.Combine(targetDirectory, Path.GetFileName(file));
                _log.Trace("Copying file \"{0}\".", Path.GetFileName(file));
				File.Copy(file, target);
			}
			foreach (var subDirectory in Directory.GetDirectories(directory))
			{
			    string subDirectoryName = Path.GetFileName(subDirectory);
			    string target = Path.Combine(targetDirectory, subDirectoryName);
                _log.Trace("Creating subdirectory \"{0}\".", subDirectoryName);
                Directory.CreateDirectory(target);
				CopyDirectoryContents(subDirectory, target);
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
                _log.Debug("Deleting folder \"{0}\".", _folder);
                Directory.Delete(_folder, true);
            }
            catch (Exception ex)
            {
                string message = string.Format("Failed to delete folder \"{0}\".", _folder);
                _log.ErrorException(message, ex);
            }
		}
	}
}

