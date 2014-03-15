using System.Configuration.Provider;

namespace ProviderModel.Tests.Providers
{
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
}
