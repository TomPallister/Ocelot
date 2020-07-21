using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Ocelot.Infrastructure
{
    public class InMemoryBus<T> : IBus<T>
    {
        private readonly BlockingCollection<DelayedMessage<T>> _queue;
        private readonly List<Action<T>> _subscriptions;
        private Thread _processing;

        public InMemoryBus()
        {
            _queue = new BlockingCollection<DelayedMessage<T>>();
            _subscriptions = new List<Action<T>>();
            _processing = new Thread(async () => await Process());
            _processing.Start();
        }

        public void Subscribe(Action<T> action)
        {
            _subscriptions.Add(action);
        }

        public void Publish(T message, int delay)
        {
            var delayed = new DelayedMessage<T>(message, delay);
            _queue.Add(delayed);
        }

        private async Task Process()
        {
            foreach (var delayedMessage in _queue.GetConsumingEnumerable())
            {
                await Task.Delay(delayedMessage.Delay);

                foreach (var subscription in _subscriptions)
                {
                    subscription(delayedMessage.Message);
                }
            }
        }
    }
}
