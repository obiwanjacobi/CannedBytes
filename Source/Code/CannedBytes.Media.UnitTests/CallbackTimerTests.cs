using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Threading;

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

        [TestCategory("LocalOnly")]
        [TestMethod]
        public void StartTimer_WithShortWait_CallbackFired()
        {
            bool callback = false;

            using (var timer = new CallbackTimer(TimerMode.Periodic))
            {
                timer.Callback += (sender, e) => { callback = true; };
                timer.StartTimer();

                Thread.Sleep(50);

                callback.Should().BeTrue();
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
#if NET4
                timer.AddCallbackHandler((sender, e) => { callback1Count++; }, 1, null);
                timer.AddCallbackHandler((sender, e) => { callback2Count++; }, divider, null);
#else
                timer.AddCallbackHandler((sender, e) => { callback1Count++; }, 1);
                timer.AddCallbackHandler((sender, e) => { callback2Count++; }, divider);
#endif
                timer.StartTimer();

                Thread.Sleep(75);

                callback1Count.Should().BeGreaterThan(0);
                callback2Count.Should().BeGreaterThan(0);
                callback2Count.Should().Be(callback1Count / divider);
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
#if NET4
                timer.AddCallbackHandler((sender, e) => { callback1Count++; }, 1, null);
                timer.AddCallbackHandler((sender, e) => { callback2Count++; }, divider, null);
#else
                timer.AddCallbackHandler((sender, e) => { callback1Count++; }, 1);
                timer.AddCallbackHandler((sender, e) => { callback2Count++; }, divider);
#endif
                timer.StartTimer();

                Thread.Sleep(500);

                callback1Count.Should().BeGreaterThan(0);
                callback2Count.Should().BeGreaterThan(0);
                callback2Count.Should().Be(callback1Count / divider);
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

                eventFired.Should().BeTrue();
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

                eventFired.Should().BeTrue();
            }
        }
    }
}