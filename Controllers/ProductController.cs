using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;
using MongoDB.Driver;

[ApiController]
[Route("api/products")]
public class ProductController : ControllerBase
{
    private readonly IMongoCollection<Product> _productCollection;

    public class Product
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public ObjectId Id { get; set; }
        public decimal Cena { get; set; }
        public decimal CenaZRabatem { get; set; }
        public string? Status { get; set; }
        public decimal VAT { get; set; }
        public string? Nazwa { get; set; }
        public decimal Waga { get; set; }
        public decimal CenaBezRabatu { get; set; }
        public string? Source { get; set; }
        public string? PrzetworzonyStatus { get; set; }
    }

    public ProductController(IMongoDatabase database)
    {
        _productCollection = database.GetCollection<Product>("SaleCheckTest");
    }

    [HttpGet]
    public IActionResult GetProducts(string query)
    {
        var filter = Builders<Product>.Filter.Text(query);
        var results = _productCollection.Find(filter).ToList();

        // Przetwórz status dla każdego produktu przed zwróceniem wyników
        foreach (var product in results)
        {
            product.PrzetworzonyStatus = MapujStatus(product.Status);
        }

        return Ok(results);
    }

    private string MapujStatus(string? status)
    {
        if (status == null)
        {
            return "Nieznany"; // lub inna wartość domyślna dla przypadku null
        }
        else if (status == "STATUS_1")
        {
            return "Dostępny";
        }
        else if (status == "STATUS_2")
        {
            return "Niedostępny";
        }
        else
        {
            return "Nieznany"; // lub inna wartość domyślna dla nieznanych wartości
        }
    }
}

