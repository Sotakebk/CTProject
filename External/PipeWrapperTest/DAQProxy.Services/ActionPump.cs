using System;
using System.Collections.Concurrent;
using System.Threading;

namespace DAQProxy.Services
{
    public interface IActionPump
    {
        void Do(Action action);
    }

    public sealed class ActionPump : IActionPump
    {
        private ConcurrentQueue<Action> queue;
        private bool quit = false;

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
                quit = true;
            });
        }

        public void Join()
        {
            while (!quit)
            {
                if (queue.TryDequeue(out var action))
                    action();
                else
                    Thread.Sleep(50);
            }
        }
    }
}
