using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace WindowsAutoPrintPdf.DL
{
    class CBarcode
    {
        [JsonProperty("id")]

        public string id { get; set; }
        [JsonProperty("type")]

        public string type { get; set; }
        [JsonProperty("barcode")]

        public string barcode { get; set; }

    }

}
