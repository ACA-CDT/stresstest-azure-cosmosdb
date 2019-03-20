using Azure.Cosmos.Stress.Models;
using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;
using System.Threading.Tasks;
using Newtonsoft.Json;
using System.Threading;

namespace Azure.Cosmos.Stress.Services
{
    public class CosmosService
    {
        static readonly string utc_date = DateTime.UtcNow.ToString("r");
        string Url { get; set; }
        string PrimaryKey { get; set; }
        public DocumentClient Client { get; set; }
        string Database { get; set; }
        string Collection { get; set; }
        List<string> PreTriggers { get; set; }
        List<string> PostTriggers { get; set; }
        RequestOptions RequestOption { get; set; }

        public CosmosService(string url, string primaryKey, string database, string collection, string preTrigger, string postTrigger)
        {
            Url = url;
            PrimaryKey = primaryKey;
            Client = new DocumentClient(new Uri(Url), PrimaryKey);
            Database = database;
            Collection = collection;

            if (!string.IsNullOrWhiteSpace(preTrigger))
            {
                PreTriggers = new List<string>();
                PreTriggers.Add(preTrigger);
            }

            if (!string.IsNullOrWhiteSpace(postTrigger))
            {
                PostTriggers = new List<string>();
                PostTriggers.Add(postTrigger);
            }

            RequestOption = new RequestOptions()
            {
                PreTriggerInclude = PreTriggers,
                PostTriggerInclude = PostTriggers
            };
        }

        public async void SaveItems(IEnumerable<dynamic> records)
        {
            foreach (dynamic record in records)
            {
                try
                {
                    ResourceResponse<Document> documentResponse = await SaveItem(record);
                    ResponseCollection.Instance.Add(documentResponse.StatusCode);
                }
                catch (DocumentClientException ex)
                {
                    if (ex.StatusCode.HasValue)
                    {

                        ResponseCollection.Instance.Add(ex.StatusCode.Value);
                    }
                }

            }

            Console.WriteLine(string.Format("Thread {0} finished import", Thread.CurrentThread.ManagedThreadId));
        }

        public Task<ResourceResponse<Document>> SaveItem(dynamic document)
        {
            Uri upsertUri = UriFactory.CreateDocumentCollectionUri(Database, Collection);
            return Client.UpsertDocumentAsync(upsertUri, document, RequestOption);
        }

    }
}
