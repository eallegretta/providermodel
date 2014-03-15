# ProviderModel

An improvement over the existing Provider Model that is bundled with the .NET Framework


## Usage

The provider model has two distinct parts, the providers definition in C# and the providers configuration in the application configuration file

### Providers definition

The library reuses the existing Provider pattern that the .NET framework provides, a provider base abstract that will have the common definition for a provider should inherit from System.Configuration.Provider.ProviderBase such as

    public abstract class GreeterProviderBase: ProviderBase
    {
        public string GreetName { get; set; }

        public abstract string Greet();

        public override void Initialize(string name, System.Collections.Specialized.NameValueCollection config)
        {
            base.Initialize(name, config);

            string greetName = config["greetname"];

            if (!string.IsNullOrWhiteSpace(greetName))
            {
                GreetName = greetName;
            }
        }
    }

Then we can start creating the providers that will be used in the application

    public class SpanishGreeterProvider : GreeterProviderBase
    {
        public override string Greet()
        {
            return "Hola " + GreetName;
        }
    }

    public class EnglishGreeterProvider : GreeterProviderBase
    {
        public override string Greet()
        {
            return "Hello " + GreetName;
        }
    }

    public class FrenchGreeterProvider : GreeterProviderBase
    {
        public override string Greet()
        {
            return "Bonjour " + GreetName;
        }
    }

In order to consume those providers we library provides the ProviderFactory<T> abstract class that should be inherited from that class will take care of parsing the configuration, creating the instance of the providers (using Activator.CreateInstance as default or a custom resolver function can be used instead) and initializing them sending the corresponding settings

    public class GreeterProviderFactory: ProviderFactory<GreeterProviderBase>
    {
        private GreeterProviderFactory()
        {
        }

        public static readonly Instance = new GreeterProviderFactory(); // Provide a static singleton

        protected override string ConfigurationSectionName
        {
            get { return "greeters"; }
        }
    }

Then on the consuming application the library is used as

    var factory = GreeterProviderFactory.Instance;

    var providers = factory.GetProviders();
    var defaultProvider = factory.GetDefaultProvider();
    var providerByName = factory.GetProvider("english"); 

### Providers configuration

The providers can be configured using two different methods, either override the GetDefaultProvidersWithoutConfiguration on the custom ProviderFactory implementation or on the application configuration file

#### Configuration-less

Simply override the GetDefaultProvidersWithoutConfiguration method from the ProviderFactory class

    public class GreeterProviderFactoryNoConfiguration : ProviderFactory<GreeterProviderBase>
    {
        protected override string ConfigurationSectionName
        {
            get { return null; }
        }

        protected override IEnumerable<KeyValuePair<string, Lazy<GreeterProviderBase>>> GetDefaultProvidersWithoutConfiguration()
        {
            yield return new KeyValuePair<string, Lazy<GreeterProviderBase>>("English", new Lazy<GreeterProviderBase>(() => new EnglishGreeterProvider { GreetName = "John Doe" }));
            yield return new KeyValuePair<string, Lazy<GreeterProviderBase>>("Spanish", new Lazy<GreeterProviderBase>(() => new SpanishGreeterProvider { GreetName = "Juan Perez" }));
        }
    }

#### Configuration on the application config file
    <configuration>
        <configSections>
            <section name="greeters" type="ProviderModel.Configuration.ProviderSectionHandler, ProviderModel" />
        </configSections>
        <greeters defaultProvider="English">
            <providers>
                <add name="English" description="English provider" type="ProviderModel.Tests.Providers.EnglishGreeterProvider, ProviderModel.Tests" greetname="John Doe"></add>
                <add name="Spanish" description="Spanish provider" type="ProviderModel.Tests.Providers.SpanishGreeterProvider, ProviderModel.Tests" greetname="Juan Perez"></add>
                <add name="French" description="French provider" type="ProviderModel.Tests.Providers.FrenchGreeterProvider, ProviderModel.Tests" greetname="Monsieur Dupont"></add>
            </providers>
        </greeters>
    </configuration>
