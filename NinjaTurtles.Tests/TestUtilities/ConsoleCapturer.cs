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

