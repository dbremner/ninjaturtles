using System;
using System.IO;

using Mono.Cecil;

using NUnit.Framework;

namespace NinjaTurtles.Tests
{
	[TestFixture]
	public class TestDirectoryTests
	{
		[Test]
		public void Constructor_Creates_Directory_And_Returns_In_FullName_Property()
		{
			using (var testDirectory = new TestDirectory())
			{
				Assert.IsTrue(Directory.Exists(testDirectory.FullName));
			}
		}
		
		[Test]
		public void Constructor_Copies_Empty_Source_Directory()
		{
			string tempFolder = Path.GetTempPath();
			string sourceFolder = Path.Combine(tempFolder, Guid.NewGuid().ToString("N"));
			string fileName = string.Format("{0:N}.txt", Guid.NewGuid());
			Directory.CreateDirectory(sourceFolder);
			File.WriteAllText(Path.Combine(sourceFolder, fileName), "Ninja");
			
			using (var testDirectory = new TestDirectory(sourceFolder))
			{
				Assert.IsTrue(File.Exists(Path.Combine(testDirectory.FullName, fileName)));
			}
			
			Directory.Delete(sourceFolder, true);
		}

	    [Test]
		public void Constructor_Copies_Source_Directory_Recursively()
		{
			string tempFolder = Path.GetTempPath();
			string sourceFolder = Path.Combine(tempFolder, Guid.NewGuid().ToString("N"));
			string fileName = string.Format("{0:N}.txt", Guid.NewGuid());
			string intermediateDirectory = Guid.NewGuid().ToString("N");
			Directory.CreateDirectory(sourceFolder);
			Directory.CreateDirectory(Path.Combine(sourceFolder, intermediateDirectory));
			File.WriteAllText(Path.Combine(sourceFolder, intermediateDirectory, fileName), "Ninja");
			
			using (var testDirectory = new TestDirectory(sourceFolder))
			{
				Assert.IsTrue(File.Exists(Path.Combine(testDirectory.FullName, intermediateDirectory, fileName)));
			}
			
			Directory.Delete(sourceFolder, true);
		}

		[Test]
		public void Directory_Name_Contains_NinjaTurtles()
		{
			using (var testDirectory = new TestDirectory())
			{
				StringAssert.Contains("NinjaTurtles", testDirectory.FullName);
			}
		}
		
		[Test]
		public void Dispose_Removes_Empty_Directory()
		{
			string path;
			using (var testDirectory = new TestDirectory())
			{
				path = testDirectory.FullName;
			}
			Assert.IsFalse(Directory.Exists(path));
		}
		
		[Test]
		public void Dispose_Removes_Non_Empty_Directory()
		{
			string tempFolder = Path.GetTempPath();
			string sourceFolder = Path.Combine(tempFolder, Guid.NewGuid().ToString("N"));
			string fileName = string.Format("{0:N}.txt", Guid.NewGuid());
			string intermediateDirectory = Guid.NewGuid().ToString("N");
			Directory.CreateDirectory(sourceFolder);
			Directory.CreateDirectory(Path.Combine(sourceFolder, intermediateDirectory));
			File.WriteAllText(Path.Combine(sourceFolder, intermediateDirectory, fileName), "Ninja");
			
			string path;
			using (var testDirectory = new TestDirectory(sourceFolder))
			{
				path = testDirectory.FullName;
			}
			
			Assert.IsFalse(Directory.Exists(path));
		}
		
		[Test]
		public void SaveAssembly_Saves_Assembly()
		{
		    var module = new Module(GetType().Assembly.Location);
			string fileName = Path.GetFileName(module.AssemblyLocation);
			
			using (var testDirectory = new TestDirectory())
			{
				testDirectory.SaveAssembly(module);
				Assert.IsTrue(File.Exists(Path.Combine(testDirectory.FullName, fileName)));
			}
		}
	}
}

