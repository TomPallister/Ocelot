using System;

namespace Ocelot.Infrastructure
{
    public interface IBus<T>
    {
        void Subscribe(Action<T> action);

        void Publish(T message, int delay);
    }
}
