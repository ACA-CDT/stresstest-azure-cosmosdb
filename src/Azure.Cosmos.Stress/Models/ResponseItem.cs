using System;
using System.Collections.Generic;
using System.Text;

namespace Azure.Cosmos.Stress.Models
{
    public class ResponseItem
    {
        public DateTime Date { get; set; }
        public System.Net.HttpStatusCode StatusCode { get; set; }
    }
}
