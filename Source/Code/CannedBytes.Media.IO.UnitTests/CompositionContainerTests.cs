using System.ComponentModel.Composition.Hosting;
using CannedBytes.ComponentModel.Composition;
using CannedBytes.Media.IO.UnitTests.Stubs;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CannedBytes.Media.IO.UnitTests
{
    [TestClass]
    public class CompositionContainerTests
    {
        private static CompositionContainer CreateNewContainer()
        {
            var factory = new CompositionContainerFactory();
            var container = factory.CreateNew();
            return container;
        }

        [TestMethod]
        public void AddInstanceExtensionMethod_AddRunningInstance_IsRetrievableExport()
        {
            var container = CreateNewContainer();

            var expected = new ComposableTestClass();
            container.AddInstance(expected);

            var actual = container.GetExport<ComposableTestClass>();

            Assert.ReferenceEquals(expected, actual);
        }

        [TestMethod]
        public void AddInstanceExtensionMethod_AddRunningInstance_IsRetrievableInterface()
        {
            var container = CreateNewContainer();

            ITestInterface expected = new ComposableTestClass();
            container.AddInstance(expected);

            var actual = container.GetService<ITestInterface>();

            Assert.ReferenceEquals(expected, actual);
        }

        [TestMethod]
        public void CreateFileChunkReader_ThrowsException_OnThisNull()
        {
            try
            {
                CompositionContainer container = null;
                var expected = container.CreateFileChunkReader();

                Assert.Fail();
            }
            catch
            {
            }
        }

        [TestMethod]
        public void AddInstance_ThrowsException_OnNullArg()
        {
            try
            {
                var container = CreateNewContainer();
                container.AddInstance<object>(null);

                Assert.Fail();
            }
            catch
            {
            }
        }

        [TestMethod]
        public void AddInstance_ThrowsException_OnThisNull()
        {
            try
            {
                CompositionContainer container = null;
                container.AddInstance<object>(null);

                Assert.Fail();
            }
            catch
            {
            }
        }
    }
}