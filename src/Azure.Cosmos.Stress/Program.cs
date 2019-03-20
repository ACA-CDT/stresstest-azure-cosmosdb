using Azure.Cosmos.Stress.Models;
using Azure.Cosmos.Stress.Services;
using CsvHelper;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Timers;

namespace Azure.Cosmos.Stress
{
    class Program
    {
        #region CsvFile

        // Use a different CSV file location if you want
        const string CSV_FILE_LOCATION = @"1500000 Sales Records.csv";

        #endregion CsvFile

        #region CosmosDb

        // The connection to your Cosmos DB:
        const string COSMOS_DB_URL = "https://localhost:8081";
        const string COSMOS_DB_PRIMARY_KEY = "<insert-the-primary-key-id>";
        const string COSMOS_DB_COLLECTION_ID = "<insert-the-collection-id>";
        const string COSMOS_DB_DATABASE_ID = "<insert-the-database-id-id>";

        // If you want to test Cosmos DB triggers (pre- or post trigger) fill out these values
        const string COSMOS_DB_PRE_TRIGGER_NAME = "";
        const string COSMOS_DB_POST_TRIGGER_NAME = "";

        // The maximum number of threads created to stress the Cosmos DB at the same time:
        const int NUMBER_OF_THREADS = 20;

        #endregion CosmosDb

        #region Timer

        const int TIMER_INTERVAL = 10000;
        static Timer AnalysisTimer { get; set; }

        #endregion Timer

        #region StopWatch

        static Stopwatch Watch { get; set; }

        #endregion StopWatch

        static void Main(string[] args)
        {
            Console.WriteLine("Stress App started!");
            Execute();
            Console.ReadLine();
        }

        private static void StartTimer()
        {
            AnalysisTimer = new Timer(TIMER_INTERVAL);
            AnalysisTimer.Elapsed += AnalysisTimer_Elapsed;
            AnalysisTimer.Start();
        }

        private static void AnalysisTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            if (!ResponseCollection.Instance.HasItems())
            {
                return;
            }

            long elapsedMilliseconds = 0;

            if (Watch != null)
            {
                elapsedMilliseconds = Watch.ElapsedMilliseconds;
            }

            List<ResponseItem> items = ResponseCollection.Instance.GetItems()?.ToList();

            foreach (var line in items.GroupBy(info => info.StatusCode)
                                    .Select(group => new {
                                        StatusCode = group.Key,
                                        Count = group.Count()
                                    })
                                    .OrderBy(x => x.StatusCode))
            {
                if (line.StatusCode == System.Net.HttpStatusCode.Created)
                {
                    int createdItemsCount = line.Count;
                    decimal millisecondsPerItem = Convert.ToDecimal(elapsedMilliseconds / createdItemsCount);
                    Console.WriteLine("Milliseconds per item creation (avg): {0}, created items (count): {1}", millisecondsPerItem, line.Count);
                }
                else
                {
                    Console.WriteLine("Error codes: {0} {1}", line.StatusCode, line.Count);
                }
            }
        }

        private static async void Execute()
        {
            Console.WriteLine(string.Format("Csv File: {0}", CSV_FILE_LOCATION));

            Console.WriteLine(string.Format("Cosmos DB Collection: {0}", COSMOS_DB_COLLECTION_ID));
            Console.WriteLine(string.Format("Cosmos DB Database ID: {0}", COSMOS_DB_DATABASE_ID));

            using (var reader = new StreamReader(CSV_FILE_LOCATION))
            using (var csvReader = new CsvReader(reader))
            {
                Console.WriteLine(string.Format("Start reading CSV file: {0}", CSV_FILE_LOCATION));

                csvReader.Configuration.MissingFieldFound = null;
                csvReader.Configuration.Delimiter = ",";
                bool result = await csvReader.ReadAsync();

                if (result)
                {
                    csvReader.ReadHeader();
                    List<dynamic> records = csvReader.GetRecords<dynamic>()?.ToList();

                    Console.WriteLine(string.Format("{0} item(s) found in CSV", records.Count()));
                    int countOfSegmentsPerThread = GetCountOfSegmentsPerThread(records.Count(), NUMBER_OF_THREADS);
                    Console.WriteLine(string.Format("Count of segments per Thread: {0}", countOfSegmentsPerThread));

                    int countOfSkippedItems = 0;
                    CosmosService cosmosService = new CosmosService(COSMOS_DB_URL, COSMOS_DB_PRIMARY_KEY, COSMOS_DB_DATABASE_ID, COSMOS_DB_COLLECTION_ID, COSMOS_DB_PRE_TRIGGER_NAME, COSMOS_DB_POST_TRIGGER_NAME);

                    List<Task> taskList = new List<Task>();

                    Watch = new Stopwatch();
                    Watch.Start();
                    StartTimer();

                    for (int i = 0; i < NUMBER_OF_THREADS; i++)
                    {
                        IEnumerable<dynamic> skippedRecords = records.Skip(countOfSkippedItems).Take(countOfSegmentsPerThread);

                        Task task = StartExecuteWebserviceRequest(records, cosmosService);
                        taskList.Add(task);
                        Console.WriteLine(string.Format("Thread No. {0} started, Thread-Id {1} has to work with {2} count of records.", (i + 1), task.Id, skippedRecords.Count().ToString("n")));

                        if (i == 0 && countOfSkippedItems == 0)
                        {
                            countOfSkippedItems = countOfSegmentsPerThread;
                        }

                        countOfSkippedItems = (countOfSkippedItems + countOfSegmentsPerThread);
                    }
                }
            }
        }

        private static Task StartExecuteWebserviceRequest(IEnumerable<dynamic> records, CosmosService service)
        {
            return Task.Run(() => {
                service.SaveItems(records);
            });
        }

        private static int GetCountOfSegmentsPerThread(int countOfItems, int countOfThreads)
        {
            int countOfParalThreads = Convert.ToInt32(countOfThreads);
            return countOfItems / countOfParalThreads;
        }
    }
}
