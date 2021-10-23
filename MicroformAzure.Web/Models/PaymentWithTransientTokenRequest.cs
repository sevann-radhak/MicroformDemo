using CyberSource.Model;

namespace MicroformAzure.Web.Models
{
    public class PaymentWithTransientTokenRequest
    {
        public string AccessCode { get; set; }
        public Ptsv2paymentsOrderInformationBillTo OrderInformationBillTo { get; set; }
        public Ptsv2paymentsTokenInformation TokenInformation { get; set; }
    }
}
