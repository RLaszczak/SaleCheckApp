using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Driver;
using System;
using System.Linq;

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
        try
        {
            var filter = Builders<Product>.Filter.Regex("Nazwa", new BsonRegularExpression($".*{query}.*", "i"));
            var results = _productCollection.Find(filter).ToList();

            foreach (var product in results)
            {
                product.PrzetworzonyStatus = MapujStatus(product.Status);
            }

            return Ok(results);
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Internal server error: {ex.Message}");
        }
    }

    private string MapujStatus(string? status)
    {
        if (status == null)
        {
            return "Nieznany"; 
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
            return "Nieznany"; 
        }
    }

    [HttpGet("allProducts")]
    public IActionResult GetAllProducts()
    {
        try
        {
            var products = _productCollection.Find(p => true).ToList();

            foreach (var product in products)
            {
                product.PrzetworzonyStatus = MapujStatus(product.Status);
            }

            return Ok(products);
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Internal server error: {ex.Message}");
        }
    }
}
