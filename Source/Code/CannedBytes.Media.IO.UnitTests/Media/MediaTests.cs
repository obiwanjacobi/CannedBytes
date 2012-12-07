using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CannedBytes.Media.IO.UnitTests.Media
{
    [TestClass]
    [DeploymentItem(@"Media\tada.wav")]
    [DeploymentItem(@"Media\boxed-delete.avi")]
    [DeploymentItem(@"Media\town.mid")]
    public class MediaTests
    {
        public TestContext TestContext { get; set; }

        [TestMethod]
        public void Check_Tada_Wav_Exists()
        {
            Assert.IsTrue(File.Exists(Path.Combine(TestContext.DeploymentDirectory, TestMedia.WaveFileName)));
        }

        [TestMethod]
        public void Check_BoxedDelete_Avi_Exists()
        {
            Assert.IsTrue(File.Exists(Path.Combine(TestContext.DeploymentDirectory, TestMedia.AviFileName)));
        }

        [TestMethod]
        public void Check_Town_Mid_Exists()
        {
            Assert.IsTrue(File.Exists(Path.Combine(TestContext.DeploymentDirectory, TestMedia.MidFileName)));
        }
    }
}