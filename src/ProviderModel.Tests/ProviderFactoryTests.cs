using System;
using System.Collections.Generic;
using System.Configuration;
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

        [Fact]
        public void Should_get_transformed_provider_when_no_configuration_is_set()
        {
            var factory = new GreeterProviderFactoryTransformerNoConfiguraiton();

            var defaultProvider = factory.GetDefaultProvider();

            Assert.IsType<ArgentineGreeterProvider>(defaultProvider);
        }

        [Fact]
        public void Should_get_configured_transformed_provider()
        {
            var factory = new GreeterProviderFactoryTransformerNoConfiguraiton();

            var provider = factory.GetProvider("Spanish");

            Assert.IsType<ArgentineGreeterProvider>(provider);
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
        }

        public class GreeterProviderFactory : ProviderFactory<GreeterProviderBase>
        {
            protected override string ConfigurationSectionName
            {
                get { return "greeters"; }
            }
        }

        public class GreeterProviderFactoryTransformerNoConfiguraiton : ProviderFactory<GreeterProviderBase>
        {
            protected override string ConfigurationSectionName
            {
                get { return null; }
            }

            protected override IEnumerable<KeyValuePair<string, Lazy<GreeterProviderBase>>> GetDefaultProvidersWithoutConfiguration()
            {
                yield return new KeyValuePair<string, Lazy<GreeterProviderBase>>("Spanish", new Lazy<GreeterProviderBase>(() => new SpanishGreeterProvider { GreetName = "Juan Perez" }));
            }

            protected override GreeterProviderBase OnProviderInitialized(GreeterProviderBase provider, ProviderSettings providerSettings)
            {
                var providerName = provider.Name;
                provider =  new ArgentineGreeterProvider(provider as SpanishGreeterProvider);
                provider.Initialize(providerName, providerSettings.Parameters);
                return provider;
            }
        }

        public class GreeterProviderFactoryTransformer : ProviderFactory<GreeterProviderBase>
        {
            protected override string ConfigurationSectionName
            {
                get { return "greeters"; }
            }

            protected override GreeterProviderBase OnProviderInitialized(GreeterProviderBase provider, ProviderSettings providerSettings)
            {
                var providerName = provider.Name;
                provider = new ArgentineGreeterProvider(provider as SpanishGreeterProvider);
                provider.Initialize(providerName, providerSettings.Parameters);
                return provider;
            }
        }
    }
}
