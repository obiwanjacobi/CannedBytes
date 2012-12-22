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
            using (var timer = new CallbackTimer(TimerMode.Periodic))
            {
                timer.StartTimer();
            }
        }

        [TestMethod]
        public void StartTimer_StopTimer_NoExceptions()
        {
            using (var timer = new CallbackTimer(TimerMode.Periodic))
            {
                timer.StartTimer();

                timer.StopTimer();
            }
        }

        [TestMethod]
        public void StartTimer_WithShortWait_CallbackFired()
        {
            bool callback = false;

            using (var timer = new CallbackTimer(TimerMode.Periodic))
            {
                timer.Callback += (sender, e) => { callback = true; };
                timer.StartTimer();

                Thread.Sleep(20);

                Assert.IsTrue(callback);
            }
        }

        [TestMethod]
        public void AddTwoHandlers_UseLowDivider_CallbacksFired()
        {
            int callback1Count = 0;
            int callback2Count = 0;
            int divider = 2;

            using (var timer = new CallbackTimer(TimerMode.Periodic))
            {
                timer.AddCallbackHandler((sender, e) => { callback1Count++; }, 1, null);
                timer.AddCallbackHandler((sender, e) => { callback2Count++; }, divider, null);
                timer.StartTimer();

                Thread.Sleep(50);

                Assert.AreNotEqual(0, callback1Count);
                Assert.AreNotEqual(0, callback2Count);
                Assert.AreEqual(callback1Count / divider, callback2Count);
            }
        }

        [TestMethod]
        public void AddTwoHandlers_UseHighDivider_CallbacksFired()
        {
            int callback1Count = 0;
            int callback2Count = 0;
            int divider = 200;

            using (var timer = new CallbackTimer(TimerMode.Periodic))
            {
                timer.AddCallbackHandler((sender, e) => { callback1Count++; }, 1, null);
                timer.AddCallbackHandler((sender, e) => { callback2Count++; }, divider, null);
                timer.StartTimer();

                Thread.Sleep(500);

                Assert.AreNotEqual(0, callback1Count);
                Assert.AreNotEqual(0, callback2Count);
                Assert.AreEqual(callback1Count / divider, callback2Count);
            }
        }

        [TestMethod]
        public void StartTimer_OnNewInstance_StartedEventFires()
        {
            using (var timer = new CallbackTimer(TimerMode.Periodic))
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
            using (var timer = new CallbackTimer(TimerMode.Periodic))
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