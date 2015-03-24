using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace BCWallet.Utilities.Threading
{
    internal class AsyncHelper
    {
        public static void BackgroundPoll(Func<bool> conditionFunction, Action callback, int intervalInMillis = 500, int timeoutInMillis = Timeout.Infinite, bool oneshot = true, CancellationToken cancelToken = new CancellationToken())
        {
            Timer timeoutTimer = null;
            Timer conditionalPollingTimer = null;

            Action<Timer> stopTimerAction = timer => { try { timer.Change(Timeout.Infinite, Timeout.Infinite); } catch { } };
            Action<Timer> disposeTimerAction = timer => { try { timer.Dispose(); } catch { } };
            Action killTimersAction = () =>
            {
                stopTimerAction(timeoutTimer);
                stopTimerAction(conditionalPollingTimer);
                disposeTimerAction(timeoutTimer);
                disposeTimerAction(conditionalPollingTimer);
            };

            Action<object> conditionalPollingTimerExpiredAction = state =>
            {
                if (cancelToken.IsCancellationRequested)
                    killTimersAction();

                if (conditionalPollingTimer == null)
                    return;

                if (conditionFunction())
                {
                    Task.Run(callback);
                    if (oneshot)
                        killTimersAction();
                }
            };

            conditionalPollingTimer = new Timer(
                    new TimerCallback(conditionalPollingTimerExpiredAction),
                    null,
                    intervalInMillis,
                    intervalInMillis);

            Action<object> timeoutTimerExpiredAction = state => killTimersAction();
            timeoutTimer = new Timer(
                    new TimerCallback(timeoutTimerExpiredAction),
                    null,
                    timeoutInMillis,
                    Timeout.Infinite);
        }
    }
}
