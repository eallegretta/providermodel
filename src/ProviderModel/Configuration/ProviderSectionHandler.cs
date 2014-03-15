using System.Configuration;

namespace ProviderModel.Configuration
{
    /// <summary>
    /// The provider configuration section handler
    /// </summary>
    public class ProviderSectionHandler : ConfigurationSection, IProvidersSettings
    {
        /// <summary>
        /// Gets the default provider.
        /// </summary>
        /// <value>
        /// The default provider.
        /// </value>
        [ConfigurationProperty("defaultProvider")]
        public string DefaultProvider
        {
            get { return (string)base["defaultProvider"]; }
        }

        /// <summary>
        /// Gets the providers.
        /// </summary>
        /// <value>
        /// The providers.
        /// </value>
        /// <remarks>This property is required</remarks>
        [ConfigurationProperty("providers", IsRequired = true)]
        public ProviderSettingsCollection Providers
        {
            get { return (ProviderSettingsCollection)base["providers"]; }
        }
    }
}
