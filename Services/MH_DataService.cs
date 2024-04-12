using Hangfire;
using MongoDB.Bson;
using MongoDB.Driver;
using SaleCheckApp.Models;
using System.Security.Cryptography;
using static ProductController;


namespace SaleCheckApp.Services
{
    public class MH_DataService
    {
        private readonly HttpClient _httpClient;
        public MH_DataService()
        {
            _httpClient = new HttpClient();
            RecurringJob.AddOrUpdate(() => DownloadCsv(), Cron.Daily);

        }
        public async Task DownloadCsv()
        {
            var respons = await _httpClient.GetAsync("https://www.mojehobby.pl/cennik.php?force_test_mode=0&export_format=csv&export=t\r\n");
            var csvData = await respons.Content.ReadAsStringAsync();
            var SaleCheckTest = ParseData(csvData);

            string connectionString = "mongodb://localhost:27017";
            MongoClient client = new MongoClient(connectionString);
            IMongoDatabase database = client.GetDatabase("SALECHECKAPP");
            IMongoCollection<MH_Data> collection = database.GetCollection<MH_Data>("SaleCheckTest");

            // Archiwizacja istniejących danych do innego zbioru w bazie
            IMongoCollection<MH_Data> archiveCollection = database.GetCollection<MH_Data>("SaleCheckTest_Archive");
            await archiveCollection.InsertManyAsync(await collection.Find(FilterDefinition<MH_Data>.Empty).ToListAsync());

            // Usunięcie istniejących danych
            await collection.DeleteManyAsync(FilterDefinition<MH_Data>.Empty);

            // Dodanie nowych danych
            await collection.InsertManyAsync(SaleCheckTest);
        }
        static List<MH_Data> ParseData(string data)
        {
            List<MH_Data> SaleCheckTest = new List<MH_Data>();
            string[] lines = data.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.RemoveEmptyEntries);

            foreach (var line in lines)
            {
                string[] fields = line.Split(',');
                MH_Data mh_data = new MH_Data();

                // Assuming the data always follows the given format
                mh_data.Id = ObjectId.GenerateNewId();
                mh_data.Cena = decimal.Parse(fields[0].Trim());
                mh_data.CenaZRabatem = decimal.Parse(fields[1].Trim());
                mh_data.Status = "";
                mh_data.VAT = decimal.Parse(fields[2].Trim());
                mh_data.Nazwa = "";
                mh_data.Waga = decimal.Parse(fields[3].Trim());
                mh_data.CenaBezRabatu = decimal.Parse(fields[4].Trim());
                mh_data.Source = "";
                mh_data.PrzetworzonyStatus = "";

                SaleCheckTest.Add(mh_data);
            }

            return SaleCheckTest;
        }

    }
}
