using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MicroformAzure.Web.Models
{
    public class CardViewModel
    {
        public string ExpirationMonth { get; set; }
        public string ExpirationYear { get; set; }
        public string Type { get; set; }
    }
}
