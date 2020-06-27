using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.IO;

namespace CannedBytes.Media.IO.UnitTests.Media
{
    [TestClass]
    [DeploymentItem(@"Media\tada.wav")]
    [DeploymentItem(@"Media\boxed-delete.avi")]
    public class MediaTests
    {
        public TestContext TestContext { get; set; }

        [TestMethod]
        public void Check_Tada_Wav_Exists()
        {
            File.Exists(Path.Combine(TestContext.DeploymentDirectory, TestMedia.WaveFileName)).Should().BeTrue();
        }

        [TestMethod]
        public void Check_BoxedDelete_Avi_Exists()
        {
            File.Exists(Path.Combine(TestContext.DeploymentDirectory, TestMedia.AviFileName)).Should().BeTrue();
        }
    }
}