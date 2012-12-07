using CannedBytes.ComponentModel.Composition;
using CannedBytes.Media.IO.UnitTests.Stubs;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CannedBytes.Media.IO.UnitTests
{
    [TestClass]
    public class CompositionContainerFactoryTests
    {
        [TestMethod]
        public void CreateDefault_NeverReturnsNull()
        {
            var container = CompositionContainerFactory.CreateDefault();

            Assert.IsNotNull(container);
        }

        [TestMethod]
        public void CreateNew_NeverReturnsNull()
        {
            var factory = new CompositionContainerFactory();
            var container = factory.CreateNew();

            Assert.IsNotNull(container);
        }

        [TestMethod]
        public void AddTypes_TypeAdded_CanBeInstantiatedInContainer()
        {
            var factory = new CompositionContainerFactory();

            factory.AddTypes(typeof(ComposableTestClass));

            var container = factory.CreateNew();

            var instance = container.GetExport<ComposableTestClass>();

            Assert.IsNotNull(instance);
        }
    }
}