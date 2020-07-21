using Xunit;

namespace Ocelot.UnitTests.Configuration.ChangeTracking
{
    using System;
    using Shouldly;
    using Ocelot.Configuration.ChangeTracking;
    using TestStack.BDDfy;

    public class OcelotConfigurationChangeTokenTests
    {
        [Fact]
        public void should_call_callback_with_state()
        {
            this.Given(_ => GivenIHaveAChangeToken())
                .And(_ => AndIRegisterACallback())
                .Then(_ => ThenIShouldGetADisposableWrapper())
                .Given(_ => GivenIActivateTheToken())
                .Then(_ => ThenTheCallbackShouldBeCalled())
                .BDDfy();
        }

        [Fact]
        public void should_not_call_callback_if_it_is_disposed()
        {
            this.Given(_ => GivenIHaveAChangeToken())
                .And(_ => AndIRegisterACallback())
                .Then(_ => ThenIShouldGetADisposableWrapper())
                .And(_ => GivenIActivateTheToken())
                .And(_ => AndIDisposeTheCallbackWrapper())
                .And(_ => GivenIActivateTheToken())
                .Then(_ => ThenTheCallbackShouldNotBeCalled())
                .BDDfy();
        }

        private OcelotConfigurationChangeToken _changeToken;
        private IDisposable _callbackWrapper;
        private int _callbackCounter;
        private readonly object _callbackInitialState = new object();
        private object _callbackState;

        private void Callback(object state)
        {
            _callbackCounter++;
            _callbackState = state;
            _changeToken.HasChanged.ShouldBeTrue();
        }

        private void GivenIHaveAChangeToken()
        {
            _changeToken = new OcelotConfigurationChangeToken();
        }

        private void AndIRegisterACallback()
        {
            _callbackWrapper = _changeToken.RegisterChangeCallback(Callback, _callbackInitialState);
        }

        private void ThenIShouldGetADisposableWrapper()
        {
            _callbackWrapper.ShouldNotBeNull();
        }

        private void GivenIActivateTheToken()
        {
            _callbackCounter = 0;
            _callbackState = null;
            _changeToken.Activate();
        }

        private void ThenTheCallbackShouldBeCalled()
        {
            _callbackCounter.ShouldBe(1);
            _callbackState.ShouldNotBeNull();
            _callbackState.ShouldBeSameAs(_callbackInitialState);
        }

        private void ThenTheCallbackShouldNotBeCalled()
        {
            _callbackCounter.ShouldBe(0);
            _callbackState.ShouldBeNull();
        }

        private void AndIDisposeTheCallbackWrapper()
        {
            _callbackState = null;
            _callbackCounter = 0;
            _callbackWrapper.Dispose();
        }
    }
}
