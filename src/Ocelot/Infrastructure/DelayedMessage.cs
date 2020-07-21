namespace Ocelot.Infrastructure
{
    internal class DelayedMessage<T>
    {
        public DelayedMessage(T message, int delay)
        {
            Delay = delay;
            Message = message;
        }

        public T Message { get; set; }

        public int Delay { get; set; }
    }
}
