namespace Ocelot.Configuration
{
    public class QoSOptions
    {
        public QoSOptions(
            int exceptionsAllowedBeforeBreaking,
            int durationofBreak,
            int timeoutValue,
            string key,
            string timeoutStrategy = "Pessimistic")
        {
            ExceptionsAllowedBeforeBreaking = exceptionsAllowedBeforeBreaking;
            DurationOfBreak = durationofBreak;
            TimeoutValue = timeoutValue;
            TimeoutStrategy = timeoutStrategy;
            Key = key;
        }

        public int ExceptionsAllowedBeforeBreaking { get; }

        public int DurationOfBreak { get; }

        public int TimeoutValue { get; }

        public string TimeoutStrategy { get; }

        public bool UseQos => ExceptionsAllowedBeforeBreaking > 0 || TimeoutValue > 0;

        public string Key { get; }
    }
}
