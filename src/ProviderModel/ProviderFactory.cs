using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Configuration;
using System.Configuration.Provider;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using ProviderModel.Configuration;

namespace ProviderModel
{
    /// <summary>
    /// Base class to support the provider pattern
    /// </summary>
    /// <typeparam name="TProvider">The type of the provider.</typeparam>
    public abstract class ProviderFactory<TProvider> where TProvider : ProviderBase
    {
        private static readonly object _syncLock = new object();
        private readonly Func<Type, string, TProvider> _instanceResolver;
        private IList<KeyValuePair<string, Lazy<TProvider>>> _providers;
        private ProviderSectionHandler _section;
        private TProvider _defaultProvider;
        private bool _sectionInitialized;

        /// <summary>
        /// Initializes a new instance of the <see cref="ProviderFactory{TProvider}"/> class.
        /// </summary>
        protected ProviderFactory()
            : this((t, n) => Activator.CreateInstance(t) as TProvider)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ProviderFactory{TProvider}"/> class.
        /// </summary>
        /// <param name="instanceResolver">The instance resolver that should expect a provider type and a provider name.</param>
        protected ProviderFactory(Func<Type, string, TProvider> instanceResolver)
        {
            _instanceResolver = instanceResolver;
        }

        /// <summary>
        /// Gets the name of the configuration section.
        /// </summary>
        /// <value>
        /// The name of the configuration section.
        /// </value>
        protected abstract string ConfigurationSectionName { get; }

        /// <summary>
        /// Gets the section.
        /// </summary>
        /// <value>
        /// The section.
        /// </value>
        protected virtual ProviderSectionHandler Section
        {
            get
            {
                lock (_syncLock)
                {
                    if (_section == null)
                    {
                        _section = ConfigurationManager.GetSection(ConfigurationSectionName) as ProviderSectionHandler;

                        if (_section != null)
                        {
                            _sectionInitialized = true;
                        }
                    }
                }

                return _section;
            }
        }

        /// <summary>
        /// Gets the configured providers.
        /// </summary>
        /// <value>
        /// The configured providers.
        /// </value>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures", Justification = "The API is properly designed")]
        protected IList<KeyValuePair<string, Lazy<TProvider>>> ConfiguredProviders
        {
            get
            {
                if (_providers == null || _sectionInitialized)
                {
                    _providers = GetConfiguredProviders();
                }

                return _providers;
            }
        }

        /// <summary>
        /// Gets the provider by its name.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <returns>An instance of TProvider</returns>
        /// <exception cref="ConfigurationErrorsException">Throws if the provider was not found</exception>
        public TProvider GetProvider(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                throw new ArgumentNullException("name", "The name of the provider cannot be null or empty");
            }

            var provider = (from cp in ConfiguredProviders
                            where cp.Key.Equals(name, StringComparison.OrdinalIgnoreCase)
                            select cp.Value.Value).FirstOrDefault();

            if (provider == null)
            {
                throw new ConfigurationErrorsException(string.Format(
                            "The provider with the name {0} is not configured on the {1} configuration section",
                            name,
                            ConfigurationSectionName));
            }

            return provider;
        }

        /// <summary>
        /// Gets the default provider.
        /// </summary>
        /// <returns>The default configured provider</returns>
        /// <exception cref="ConfigurationErrorsException">Throws if there are no configured providers</exception>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate", Justification = "We want this to be a mehotd")]
        public TProvider GetDefaultProvider()
        {
            if (_defaultProvider != null)
            {
                return _defaultProvider;
            }

            if (ConfiguredProviders.Count == 0)
            {
                throw new ConfigurationErrorsException(string.Format(
            "There are no providers configured on the application configuration file, make sure the section {0} is configured properly and the providers element has at least one cache provider",
                            ConfigurationSectionName));
            }

            if (Section == null || string.IsNullOrWhiteSpace(Section.DefaultProvider))
            {
                _defaultProvider = _providers.First().Value.Value;
            }
            else
            {
                _defaultProvider = GetProvider(Section.DefaultProvider);
            }

            return _defaultProvider;
        }

        /// <summary>
        /// Gets the providers.
        /// </summary>
        /// <returns>An <see cref="IList&lt;TProvider&gt;" /> with all the configured providers</returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate", Justification = "We want this to be a method")]
        public IList<TProvider> GetProviders()
        {
            return ConfiguredProviders.Select(kvp => kvp.Value.Value).ToList();
        }

        /// <summary>
        /// Gets the provider names.
        /// </summary>
        /// <returns>An <see cref="IList&lt;TProvider&gt;" /> with all the configured provider names</returns>
        public IList<string> GetProviderNames()
        {
            return ConfiguredProviders.Select(kvp => kvp.Key).ToList();
        }

        /// <summary>
        /// Called when the provider has been initialized. Use this method to perform after initialization code for the provider or even transform the provider.
        /// </summary>
        /// <param name="provider">The provider.</param>
        /// <param name="providerSettings">The provider settings.</param>
        /// <returns>The provider</returns>
        protected virtual TProvider OnProviderInitialized(TProvider provider, ProviderSettings providerSettings)
        {
            return provider;
        }

        /// <summary>
        /// Initializes the providers when no configuration is found in the application configuration file.
        /// </summary>
        /// <returns>A list of providers</returns>
        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures", Justification = "This is by design")]
        protected virtual IEnumerable<KeyValuePair<string, Lazy<TProvider>>> GetDefaultProvidersWithoutConfiguration()
        {
            return null;
        }

        private static void ThrowProviderConfigurationException(ProviderSettings providerSettings, Exception innerException = null)
        {
            throw new Exception(string.Format("The provider type {0} with name {1} could not be created", providerSettings.Type, providerSettings.Name), innerException);
        }

        private IList<KeyValuePair<string, Lazy<TProvider>>> GetConfiguredProviders()
        {
            IList<KeyValuePair<string, Lazy<TProvider>>> providers = new List<KeyValuePair<string, Lazy<TProvider>>>();

            if (Section == null || Section.Providers.Count == 0)
            {
                var defaultProviders = GetDefaultProvidersWithoutConfiguration();

                if (defaultProviders == null || !defaultProviders.Any())
                {
                    throw new ConfigurationErrorsException(string.Format("There are no default providers configured"));
                }

                foreach (var defaultProvider in defaultProviders)
                {
                    var provider = defaultProvider.Value.Value;
                    var settings = new ProviderSettings(defaultProvider.Key, provider.GetType().FullName);
                    provider.Initialize(defaultProvider.Key, settings.Parameters);
                    provider = OnProviderInitialized(provider, settings);
                    providers.Add(new KeyValuePair<string, Lazy<TProvider>>(defaultProvider.Key, new Lazy<TProvider>(() => provider)));
                }

                return providers;
            }

            foreach (ProviderSettings provider in Section.Providers)
            {
                var providerSettings = provider;

                var lazyProvider = new Lazy<TProvider>(
                    () =>
                    {
                        var type = Type.GetType(providerSettings.Type);

                        if (type == null)
                        {
                            ThrowProviderConfigurationException(providerSettings);
                        }

                        var providerInstance = CreateProviderInstance(type, providerSettings);

                        providerInstance.Initialize(providerSettings.Name, providerSettings.Parameters);

                        providerInstance = OnProviderInitialized(providerInstance, providerSettings);

                        return providerInstance;
                    },
                    true);

                providers.Add(new KeyValuePair<string, Lazy<TProvider>>(provider.Name, lazyProvider));
            }

            return providers;
        }

        private TProvider CreateProviderInstance(Type type, ProviderSettings providerInClosure)
        {
            TProvider providerInstance = null;

            try
            {
                providerInstance = _instanceResolver(type, providerInClosure.Name);
            }
            catch (Exception ex)
            {
                ThrowProviderConfigurationException(providerInClosure, ex);
            }

            return providerInstance;
        }
    }
}
