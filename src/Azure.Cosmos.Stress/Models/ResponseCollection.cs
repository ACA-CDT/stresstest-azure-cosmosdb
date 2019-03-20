using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Azure.Cosmos.Stress.Models
{
    public class ResponseCollection
    {
        List<ResponseItem> Documents { get; set; }

        private static readonly Lazy<ResponseCollection> _instance = new Lazy<ResponseCollection>(() => new ResponseCollection());
        public static ResponseCollection Instance
        {
            get
            {
                return _instance.Value;
            }
        }

        private ResponseCollection()
        {
            Documents = new List<ResponseItem>();
        }

        public void Add(System.Net.HttpStatusCode statusCode)
        {
            Documents.Add(new ResponseItem() { Date = DateTime.Now, StatusCode = statusCode });
        }

        public List<ResponseItem> GetItems()
        {
            return Documents?.ToList();
        }

        public bool HasItems()
        {
            if (Documents == null)
            {
                return false;
            }
            else if (Documents.Count == 0)
            {
                return false;
            }
            else
            { return true; }
        }

        public void Clear()
        {
            Documents.Clear();
        }
    }
}
