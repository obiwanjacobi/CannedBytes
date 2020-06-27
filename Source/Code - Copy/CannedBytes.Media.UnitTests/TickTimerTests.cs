using System.Threading;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CannedBytes.Media.UnitTests
{
    [TestClass]
    public class TickTimerTests
    {
        [TestMethod]
        public void Start_CleanupOnDispose_NoExceptions()
        {
            using (var timer = new TickTimer())
            {
                timer.StartTimer();
            }
        }

        [TestMethod]
        public void GetTicks_WithoutWait_ReturnsZeroValue()
        {
            using (var timer = new TickTimer())
            {
                timer.StartTimer();

                Assert.AreEqual(0, timer.Ticks);
            }
        }

        [TestMethod]
        public void GetTicks_AfterShortWait_ReturnsNonZeroValue()
        {
            using (var timer = new TickTimer())
            {
                timer.StartTimer();

                Thread.Sleep(20);

                Assert.AreNotEqual(0, timer.Ticks);
            }
        }

        [TestMethod]
        public void StartTimer_StopTimer_NoExceptions()
        {
            using (var timer = new TickTimer())
            {
                timer.StartTimer();

                timer.StopTimer();
            }
        }
    }
}