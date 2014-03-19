using System;
using System.Collections.Generic;
using ProviderModel.Tests.Providers;
using Xunit;

namespace ProviderModel.Tests
{
    public class ProviderFactoryTests
    {
        [Fact]
        public void Should_get_default_providers_when_no_configuration_is_set()
        {
            var factory = new GreeterProviderFactoryNoConfiguration();

            var providers = factory.GetProviders();
            var providerNames = factory.GetProviderNames();
            var defaultProvider = factory.GetDefaultProvider();

            Assert.NotNull(providers);
            Assert.NotEmpty(providers);
            Assert.NotEmpty(providerNames);
            Assert.NotNull(defaultProvider);
            Assert.Single(providers);
            Assert.Single(providerNames);
            Assert.Equal("English", providers[0].Name);
            Assert.Equal("English", defaultProvider.Name);
            Assert.Equal(defaultProvider, providers[0]);
            Assert.Equal("Hello John Doe", defaultProvider.Greet());
        }

        [Fact]
        public void Should_get_configured_providers()
        {
            var factory = new GreeterProviderFactory();

            var providers = factory.GetProviders();
            var providerNames = factory.GetProviderNames();
            var defaultProvider = factory.GetDefaultProvider();

            Assert.NotNull(providers);
            Assert.NotEmpty(providers);
            Assert.NotEmpty(providerNames);
            Assert.NotNull(defaultProvider);
            Assert.Equal(3, providers.Count);
            Assert.Equal(3, providerNames.Count);
            Assert.Equal("English", providers[0].Name);
            Assert.Equal("Spanish", providers[1].Name);
            Assert.Equal("French", providers[2].Name);

            Assert.Equal("English", defaultProvider.Name);

            Assert.Equal(defaultProvider.Name, providers[0].Name);
            
            Assert.Equal("Hello John Doe", providers[0].Greet());
            Assert.Equal("Hola Juan Perez", providers[1].Greet());
            Assert.Equal("Bonjour Monsieur Dupont", providers[2].Greet());
        }


        public class GreeterProviderFactoryNoConfiguration : ProviderFactory<GreeterProviderBase>
        {
            protected override string ConfigurationSectionName
            {
                get { return null; }
            }

            protected override IEnumerable<KeyValuePair<string, Lazy<GreeterProviderBase>>> GetDefaultProvidersWithoutConfiguration()
            {
                yield return new KeyValuePair<string, Lazy<GreeterProviderBase>>("English", new Lazy<GreeterProviderBase>(() => new EnglishGreeterProvider { GreetName = "John Doe" }));
            }

            protected override GreeterProviderBase OnProviderInitialized(System.Configuration.ProviderSettings providerSettings, GreeterProviderBase provider)
            {
                if (provider is SpanishGreeterProvider)
                {
                    string providerName = provider.Name;
                    provider = new ArgentineGreeterProvider(provider as SpanishGreeterProvider);    
                    provider.Initialize(providerName, providerSettings.Parameters);
                }

                return provider;
            }
        }

        public class GreeterProviderFactory : ProviderFactory<GreeterProviderBase>
        {
            protected override string ConfigurationSectionName
            {
                get { return "greeters"; }
            }
        }
    }
}
