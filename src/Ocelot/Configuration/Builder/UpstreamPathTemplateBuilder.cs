namespace Ocelot.Configuration.Builder
{
    using Values;

    public class UpstreamPathTemplateBuilder
    {
        private string _template;
        private int _priority;
        private bool _containsQueryString;
        private string _originalValue;

        public UpstreamPathTemplateBuilder WithTemplate(string template)
        {
            _template = template;
            return this;
        }

        public UpstreamPathTemplateBuilder WithPriority(int priority)
        {
            _priority = priority;
            return this;
        }

        public UpstreamPathTemplateBuilder WithContainsQueryString(bool containsQueryString)
        {
            _containsQueryString = containsQueryString;
            return this;
        }

        public UpstreamPathTemplateBuilder WithOriginalValue(string originalValue)
        {
            _originalValue = originalValue;
            return this;
        }

        public UpstreamPathTemplate Build()
        {
            return new UpstreamPathTemplate(_template, _priority, _containsQueryString, _originalValue);
        }
    }
}
