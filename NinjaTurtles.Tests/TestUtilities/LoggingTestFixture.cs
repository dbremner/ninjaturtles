using System.Collections.Generic;
using System.Linq;

using NLog;
using NLog.Config;
using NLog.Targets;

using NUnit.Framework;

namespace NinjaTurtles.Tests.TestUtilities
{
    [TestFixture]
    public abstract class LoggingTestFixture
    {
        private MemoryTarget _logTarget;

        [TestFixtureSetUp]
        public void TestFixtureSetUp()
        {
            var config = new LoggingConfiguration();

            _logTarget = new MemoryTarget();
            _logTarget.Layout = "${level:uppercase=true}|${message}";
            config.AddTarget("memory", _logTarget);

            var consoleTarget = new ConsoleTarget();
            consoleTarget.Layout = "${longdate}|${logger}|${level:uppercase=true}|${message}";
            config.AddTarget("console", consoleTarget);

            var rule = new LoggingRule("*", LogLevel.Trace, _logTarget);
            rule.Targets.Add(consoleTarget);
            config.LoggingRules.Add(rule);

            LogManager.Configuration = config;
        }

        [TestFixtureTearDown]
        public void TestFixtureTearDown()
        {
            LogManager.Configuration = null;
        }

        [SetUp]
        public void SetUp()
        {
            _logTarget.Logs.Clear();
            Assert.AreEqual(0, _logTarget.Logs.Count);
        }

        protected IList<string> Logs
        {
            get { return _logTarget.Logs; }
        }

        public void AssertLogContains(string message, bool startOfMessageOnly = false)
        {
            if (startOfMessageOnly)
            {
                Assert.IsTrue(Logs.Any(m => m.StartsWith(message)));
            }
            else
            {
                Assert.IsTrue(Logs.Any(m => m == message));
            }
        }
    }
}
