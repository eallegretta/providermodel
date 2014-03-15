using System.Configuration;

namespace ProviderModel.Configuration
{
    /// <summary>
    /// Defines the contract for a providers settings class
    /// </summary>
    public interface IProvidersSettings
    {
        /// <summary>
        /// Gets the default provider.
        /// </summary>
        /// <value>
        /// The default provider.
        /// </value>
        string DefaultProvider { get; }

        /// <summary>
        /// Gets the providers.
        /// </summary>
        /// <value>
        /// The providers.
        /// </value>
        ProviderSettingsCollection Providers { get; }
    }
}