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
// Copyright (C) 2012-14 David Musgrove and others.

#endregion

using System;
using System.IO;
using System.Text;

namespace NinjaTurtles.Tests.TestUtilities
{
	public class ConsoleCapturer : IDisposable
	{
		private TextWriter _originalOut;
		private StringBuilder _builder;
		private StringWriter _writer;
		
		public ConsoleCapturer()
		{
			_originalOut = Console.Out;
			_builder = new StringBuilder();
			_writer = new StringWriter(_builder);
			Console.SetOut(_writer);
		}

		public string Output
		{
			get
			{
				_writer.Flush();
				return _builder.ToString();
			}
		}
		
		public void Dispose()
		{
			Console.SetOut(_originalOut);
		}
	}
}

