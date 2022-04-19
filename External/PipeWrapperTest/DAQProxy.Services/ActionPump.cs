using System;
using System.Collections.Concurrent;
using System.Threading;

namespace DAQProxy.Services
{
    public interface IActionPump
    {
        void Do(Action action);
    }

    public sealed class ActionPump
    {
        private ConcurrentQueue<Action> queue;

        public ActionPump()
        {
            queue = new ConcurrentQueue<Action>();
        }

        public void Do(Action action)
        {
            queue.Enqueue(action);
        }

        public void Quit()
        {
            queue.Enqueue(() =>
            {
                Environment.Exit(0);
            });
        }

        public void Join()
        {
            while (true)
            {
                if (queue.TryDequeue(out var action))
                    action();
                else
                    Thread.Sleep(50);
            }
        }
    }
}
