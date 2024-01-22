using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson;
using MongoDB.Driver;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

public class Product
{
    public ObjectId Id { get; set; }
    public decimal Price { get; set; }
    public decimal DiscountPrice { get; set; }
    public string? Status { get; set; }
    public decimal VAT { get; set; }
    public string? Name { get; set; }
    public decimal Weight { get; set; }
    public decimal PriceWithoutDiscount { get; set; }
    public string Source { get; set; } = "MojeHobby";
}

public class MojeHobbyDataController : Controller
{
    private readonly IMongoCollection<Product> _productCollection;

    public MojeHobbyDataController(IMongoDatabase database)
    {
        _productCollection = database.GetCollection<Product>("products");
    }

    public IActionResult Index()
    {
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> ImportCsv()
    {
        try
        {
            var sourceDirectory = Path.Combine(Environment.CurrentDirectory, "Source");
            var csvPath = Path.Combine(sourceDirectory, "pricelist.csv");

            var lines = await System.IO.File.ReadAllLinesAsync(csvPath);

            foreach (var line in lines)
            {
                var values = line.Split(' '); // Zmiana separatora z ',' na spacje
                var product = new Product();

                // Przyjmujemy, że dane są zawsze w odpowiedniej kolejności
                product.Id = ObjectId.GenerateNewId();
                product.Status = values[4].Trim().Equals("STATUS_1", StringComparison.OrdinalIgnoreCase) ? "Dostępny" : "Niedostępny";
                product.Price = decimal.TryParse(values[2], out var price) ? price : 0.0m;
                product.DiscountPrice = decimal.TryParse(values[3], out var discountPrice) ? discountPrice : 0.0m;
                product.VAT = decimal.TryParse(values[5], out var vat) ? vat : 0.0m;
                product.Name = values[6].Trim();
                product.Weight = decimal.TryParse(values[7], out var weight) ? weight : 0.0m;
                product.PriceWithoutDiscount = decimal.TryParse(values[8], out var priceWithoutDiscount) ? priceWithoutDiscount : 0.0m;

                // Pomijamy kolumnę "barcode" i ustawiamy Source na "MojeHobby"
                product.Source = "MojeHobby";

                await _productCollection.InsertOneAsync(product);
            }

            return RedirectToAction("Index");
        }
        catch (Exception ex)
        {
            // Obsługa błędów
            ViewBag.Error = $"Błąd importu: {ex.Message}";
            return View("Index");
        }
    }
}
