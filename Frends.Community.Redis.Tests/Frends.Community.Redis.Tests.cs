using NUnit.Framework;
using System;

namespace Frends.Community.Redis.Tests
{
    [TestFixture]
    class TestClass
    {
        /// <summary>
        /// You need to run Frends.Community.Redis.SetPaswordsEnv.ps1 before running unit test, or some other way set environment variables e.g. with GitHub Secrets.
        /// To do meaningful unit tests, implementation needs to switch to use IConnectionMultiplexer interface instead of the concrete class.
        /// </summary>
        [Test]
        public void TestDummy()
        {
            Assert.IsTrue(true);
        }
    }
}
