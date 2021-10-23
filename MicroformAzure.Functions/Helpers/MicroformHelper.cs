using CyberSource.Model;
using MicroformAzure.Functions.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;

namespace MicroformAzure.Functions.Helpers
{
    public static class MicroformHelper
    {

        public static string GetUrlRedirectAfterPayment(
           PtsV2PaymentsPost201Response r,
           ApplicationPayerInfoEntity apie,
           ApplicationRequestEntity apre)
        {
            // Build a List of the querystring parameters (this could optionally also have a .ToLookup(qs => qs.key, qs => qs.value) call)
            var querystringParams = new[] {
                new { key = "amount", value = r.OrderInformation.AmountDetails.AuthorizedAmount ?? r.OrderInformation.AmountDetails.TotalAmount},
                new { key = "applicationName", value = apre.ApplicationName },
                new { key = "orderCode", value = apre.OrderCode },
                new { key = "userId", value = apie.PayerId },
                new { key = "status", value = r.Status }
            };

            // format each querystring parameter, and ensure its value is encoded
            IEnumerable<string> encodedQueryStringParams = querystringParams
                .Select(p => string.Format("{0}={1}", p.key, HttpUtility.UrlEncode(p.value)));

            // Construct a strongly-typed Uri, with the querystring parameters appended
            UriBuilder url = new UriBuilder(apre.UrlRedirectAfterPayment)
            {
                Query = string.Join("&", encodedQueryStringParams)
            };

            return url.Uri.ToString();
        }

        public static string MaskCardNumber(string cardNumber)
        {
            cardNumber = cardNumber.Remove(0, 6);
            return $"XXXXXX{cardNumber}";
        }

        public static CyberSource.Client.Configuration SetUpCybersourceConfig(MicroformAzureContext context)
        {
            try
            {
                Dictionary<string, string> configDictionary = new Configuration(context).GetConfiguration();
                return new CyberSource.Client.Configuration(merchConfigDictObj: configDictionary);
            }
            catch (Exception e)
            {
                throw new Exception($"There was an error while configuring the API Instance. {e.Message}");
            }
        }
    }
}
