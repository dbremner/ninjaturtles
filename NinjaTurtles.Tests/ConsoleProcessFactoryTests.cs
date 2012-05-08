using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;

using NUnit.Framework;

namespace NinjaTurtles.Tests
{
    [TestFixture]
    public class ConsoleProcessFactoryTests
    {
        [Test]
        public void CreateProcess_Redirects_Standard_Output()
        {
            var process = ConsoleProcessFactory.CreateProcess("cmd.exe", "");

            Assert.IsFalse(process.StartInfo.UseShellExecute);
            Assert.IsTrue(process.StartInfo.RedirectStandardOutput);
        }

        [Test]
        public void CreateProcess_Hides_Window()
        {
            var process = ConsoleProcessFactory.CreateProcess("cmd.exe", "");

            Assert.AreEqual(ProcessWindowStyle.Hidden, process.StartInfo.WindowStyle);
        }

        [Test]
        public void CreateProcess_Gives_Correct_Information_For_CLR()
        {
            bool isMono = ConsoleProcessFactory.IsMono;
            ConsoleProcessFactory.IsMono = false;
            var process = ConsoleProcessFactory.CreateProcess("cmd.exe", "");
            ConsoleProcessFactory.IsMono = isMono;

            StringAssert.Contains("cmd.exe", process.StartInfo.FileName);
        }

        [Test]
        public void CreateProcess_Gives_Correct_Information_For_Mono()
        {
            bool isMono = ConsoleProcessFactory.IsMono;
            ConsoleProcessFactory.IsMono = true;
            var process = ConsoleProcessFactory.CreateProcess("cmd.exe", "");
            ConsoleProcessFactory.IsMono = isMono;

            Assert.AreEqual("mono", process.StartInfo.FileName);
            StringAssert.StartsWith("--runtime=v4.0", process.StartInfo.Arguments);
            StringAssert.Contains("cmd.exe\"", process.StartInfo.Arguments);
        }

        [Test]
        public void CreateProcess_Resolves_Full_Exe_Path()
        {
            bool isMono = ConsoleProcessFactory.IsMono;
            ConsoleProcessFactory.IsMono = false;
            var process = ConsoleProcessFactory.CreateProcess("cmd.exe", "");
            ConsoleProcessFactory.IsMono = isMono;

            Assert.AreNotEqual("cmd.exe", process.StartInfo.FileName);
            Assert.IsTrue(File.Exists(process.StartInfo.FileName));
        }

        [Test]
        public void CreateProcess_Uses_Correct_Switch_Format_For_Windows()
        {
            bool isWindows = ConsoleProcessFactory.IsWindows;
            ConsoleProcessFactory.IsWindows = true;
            var process = ConsoleProcessFactory.CreateProcess("cmd.exe", "{0}arg=val");
            ConsoleProcessFactory.IsWindows = isWindows;

            StringAssert.Contains("/arg=val", process.StartInfo.Arguments);
        }

        [Test]
        public void CreateProcess_Uses_Correct_Switch_Format_For_Non_Windows()
        {
            bool isWindows = ConsoleProcessFactory.IsWindows;
            ConsoleProcessFactory.IsWindows = false;
            var process = ConsoleProcessFactory.CreateProcess("cmd.exe", "{0}arg=val");
            ConsoleProcessFactory.IsWindows = isWindows;

            StringAssert.Contains("-arg=val", process.StartInfo.Arguments);
        }
    }
}
