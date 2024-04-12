using MongoDB.Bson;

namespace SaleCheckApp.Models
{
    public class MH_Data
    {
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
}
