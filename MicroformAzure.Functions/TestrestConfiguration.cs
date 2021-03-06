using System.Collections.Generic;

namespace MicroformAzure.Functions
{
    public class TestrestConfiguration
    {
        // initialize dictionary object
        private readonly Dictionary<string, string> _configurationDictionary = new Dictionary<string, string>();

        public Dictionary<string, string> GetConfiguration()
        {
            _configurationDictionary.Add("authenticationType", "HTTP_SIGNATURE");
            _configurationDictionary.Add("merchantID", "testrest");
            _configurationDictionary.Add("merchantsecretKey", "yBJxy6LjM2TmcPGu+GaJrHtkke25fPpUX+UY6/L/1tE=");
            _configurationDictionary.Add("merchantKeyId", "08c94330-f618-42a3-b09d-e1e43be5efda");
            _configurationDictionary.Add("keysDirectory", "Resource");
            _configurationDictionary.Add("keyFilename", "testrest");
            _configurationDictionary.Add("runEnvironment", "apitest.cybersource.com");
            _configurationDictionary.Add("keyAlias", "testrest");
            _configurationDictionary.Add("keyPass", "testrest");
            _configurationDictionary.Add("enableLog", "FALSE");
            _configurationDictionary.Add("logDirectory", string.Empty);
            _configurationDictionary.Add("logFileName", string.Empty);
            _configurationDictionary.Add("logFileMaxSize", "5242880");
            _configurationDictionary.Add("timeout", "300000");

            // Configs related to meta key
            _configurationDictionary.Add("portfolioID", string.Empty);
            _configurationDictionary.Add("useMetaKey", "false");

            // Configs related to OAuth
            _configurationDictionary.Add("enableClientCert", "false");
            _configurationDictionary.Add("clientCertDirectory", "Resource");
            _configurationDictionary.Add("clientCertFile", "");
            _configurationDictionary.Add("clientCertPassword", "");
            _configurationDictionary.Add("clientId", "");
            _configurationDictionary.Add("clientSecret", "");

            // _configurationDictionary.Add("proxyAddress", string.Empty);
            // _configurationDictionary.Add("proxyPort", string.Empty);
            // _configurationDictionary.Add("proxyUsername", string.Empty);
            // _configurationDictionary.Add("proxyPassword", string.Empty);
            return _configurationDictionary;
        }

        public Dictionary<string, string> GetAlternativeConfiguration()
        {
            _configurationDictionary.Add("authenticationType", "HTTP_SIGNATURE");
            _configurationDictionary.Add("merchantID", "testrest");
            _configurationDictionary.Add("merchantsecretKey", "yBJxy6LjM2TmcPGu+GaJrHtkke25fPpUX+UY6/L/1tE=");
            _configurationDictionary.Add("merchantKeyId", "08c94330-f618-42a3-b09d-e1e43be5efda");
            _configurationDictionary.Add("keysDirectory", "Resource");
            _configurationDictionary.Add("keyFilename", "testrest_cpctv");
            _configurationDictionary.Add("runEnvironment", "apitest.cybersource.com");
            _configurationDictionary.Add("keyAlias", "testrest_cpctv");
            _configurationDictionary.Add("keyPass", "testrest_cpctv");
            _configurationDictionary.Add("enableLog", "FALSE");
            _configurationDictionary.Add("logDirectory", string.Empty);
            _configurationDictionary.Add("logFileName", string.Empty);
            _configurationDictionary.Add("logFileMaxSize", "5242880");
            _configurationDictionary.Add("timeout", "300000");
            // _configurationDictionary.Add("proxyAddress", string.Empty);
            // _configurationDictionary.Add("proxyPort", string.Empty);
            // _configurationDictionary.Add("proxyUsername", string.Empty);
            // _configurationDictionary.Add("proxyPassword", string.Empty);

            // Configs related to meta key
            _configurationDictionary.Add("portfolioID", string.Empty);
            _configurationDictionary.Add("useMetaKey", "false");

            // Configs related to OAuth
            _configurationDictionary.Add("enableClientCert", "false");
            _configurationDictionary.Add("clientCertDirectory", "Resource");
            _configurationDictionary.Add("clientCertFile", "");
            _configurationDictionary.Add("clientCertPassword", "");
            _configurationDictionary.Add("clientId", "");
            _configurationDictionary.Add("clientSecret", "");


            return _configurationDictionary;
        }
    }
}
