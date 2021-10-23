using MicroformAzure.Functions.Entities;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MicroformAzure.Functions
{
    public class Configuration
    {
        // initialize dictionary object
        private readonly Dictionary<string, string> _configurationDictionary = new Dictionary<string, string>();
        private readonly MicroformAzureContext _context;
        CybersourceConfigurationEntity _configEntity;

        public Configuration(MicroformAzureContext context, string merchantId = "testrest")
        {
            _context = context;

            _configEntity = _context
                .CybersourceConfiguration
                .FirstOrDefault(x => x.MerchantID == merchantId);

            if(_configEntity == null)
            {
                throw new Exception("No Cybersource Configuration found.");
            }
        }

        public Dictionary<string, string> GetConfiguration()
        {
            _configurationDictionary.Add("authenticationType", _configEntity.AuthenticationType);
            _configurationDictionary.Add("merchantID", _configEntity.MerchantID);
            _configurationDictionary.Add("merchantsecretKey", _configEntity.MerchantsecretKey);
            _configurationDictionary.Add("merchantKeyId", _configEntity.MerchantKeyId);
            _configurationDictionary.Add("keysDirectory", _configEntity.KeysDirectory);
            _configurationDictionary.Add("keyFilename", _configEntity.KeyFilename);
            _configurationDictionary.Add("runEnvironment", _configEntity.RunEnvironment);
            _configurationDictionary.Add("keyAlias", _configEntity.KeyAlias);
            _configurationDictionary.Add("keyPass", _configEntity.KeyPass);
            _configurationDictionary.Add("enableLog", _configEntity.EnableLog);
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
            _configurationDictionary.Add("merchantID", "testrest_cpctv");
            _configurationDictionary.Add("merchantsecretKey", "JXm4dqKYIxWofM1TIbtYY9HuYo7Cg1HPHxn29f6waRo=");
            _configurationDictionary.Add("merchantKeyId", "e547c3d3-16e4-444c-9313-2a08784b906a");
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
