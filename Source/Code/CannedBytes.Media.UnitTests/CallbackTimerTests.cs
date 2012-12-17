using System.Threading;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CannedBytes.Media.UnitTests
{
    [TestClass]
    public class CallbackTimerTests
    {
        [TestMethod]
        public void StartTimer_CleanupOnDispose_NoExceptions()
        {
            using (var timer = new CallbackTimer(TimerMode.Periodic, (sender, e) => { }))
            {
                timer.StartTimer();
            }
        }

        [TestMethod]
        public void StartTimer_StopTimer_NoExceptions()
        {
            using (var timer = new CallbackTimer(TimerMode.Periodic, (sender, e) => { }))
            {
                timer.StartTimer();

                timer.StopTimer();
            }
        }

        [TestMethod]
        public void StartTimer_WithShortWait_CallbackFired()
        {
            bool callback = false;

            using (var timer = new CallbackTimer(TimerMode.Periodic, (sender, e) => { callback = true; }))
            {
                timer.StartTimer();

                Thread.Sleep(20);

                Assert.IsTrue(callback);
            }
        }

        [TestMethod]
        public void StartTimer_OnNewInstance_StartedEventFires()
        {
            using (var timer = new CallbackTimer(TimerMode.Periodic, (sender, e) => { }))
            {
                bool eventFired = false;
                timer.Started += (sender, e) => { eventFired = true; };
                timer.StartTimer();

                Assert.IsTrue(eventFired);
            }
        }

        [TestMethod]
        public void StopTimer_OnNewInstance_StoppedEventFires()
        {
            using (var timer = new CallbackTimer(TimerMode.Periodic, (sender, e) => { }))
            {
                bool eventFired = false;
                timer.Stopped += (sender, e) => { eventFired = true; };

                timer.StartTimer();
                timer.StopTimer();

                Assert.IsTrue(eventFired);
            }
        }
    }
}