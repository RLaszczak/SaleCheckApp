using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Net;
using HtmlAgilityPack;
using SaleCheckApp.Models;

namespace SaleCheckApp.Services
{
    public class Scrapper_OLX
    {
        private readonly IMongoCollection<OLX_Data> _olxDataCollection;

        public Scrapper_OLX(string connectionString, string databaseName, string collectionName)
        {
            var client = new MongoClient(connectionString);
            var database = client.GetDatabase(databaseName);
            _olxDataCollection = database.GetCollection<OLX_Data>(collectionName);
        }

        public void ScrapeAndSaveModelingListings()
        {
            List<OLX_Data> listings = ScrapeModelingListings();

            foreach (var listing in listings)
            {
                // Uzupełnianie brakujących wartości zerami
                if (string.IsNullOrEmpty(listing.ProductName))
                    listing.ProductName = "0";
                if (string.IsNullOrEmpty(listing.Price))
                    listing.Price = "0";
                if (string.IsNullOrEmpty(listing.Location))
                    listing.Location = "0";
                if (string.IsNullOrEmpty(listing.Link))
                    listing.Link = "0";
                if (listing.ListingDate == default(DateTime))
                    listing.ListingDate = DateTime.MinValue;
                if (string.IsNullOrEmpty(listing.Description))
                    listing.Description = "0";

                _olxDataCollection.InsertOne(listing);
            }
        }

        private List<OLX_Data> ScrapeModelingListings()
        {
            List<OLX_Data> listings = new List<OLX_Data>();

            string url = "https://www.olx.pl/sport-hobby/pozostaly-sport-hobby/q-modelarstwo/";

            HtmlWeb web = new HtmlWeb();
            HtmlDocument doc = web.Load(url);

            var offerNodes = doc.DocumentNode.SelectNodes("//div[contains(@class, 'offer-wrapper')]");

            if (offerNodes != null)
            {
                foreach (var offerNode in offerNodes)
                {
                    var titleNode = offerNode.SelectSingleNode(".//h3");
                    var priceNode = offerNode.SelectSingleNode(".//p[contains(@class, 'price')]");
                    var locationNode = offerNode.SelectSingleNode(".//p[contains(@class, 'lheight16')]");
                    var linkNode = offerNode.SelectSingleNode(".//a[contains(@class, 'detailsLink')]");

                    if (titleNode != null && priceNode != null && locationNode != null && linkNode != null)
                    {
                        string productName = titleNode.InnerText.Trim();
                        string price = priceNode.InnerText.Trim();
                        string location = locationNode.InnerText.Trim();
                        string link = linkNode.GetAttributeValue("href", "");

                        // Additional scraping for date and description
                        var listingDateNode = offerNode.SelectSingleNode(".//td[contains(@class, 'bottom-cell')]");
                        var descriptionNode = offerNode.SelectSingleNode(".//div[contains(@class, 'description')]");

                        string listingDateStr = listingDateNode?.InnerText.Trim();
                        DateTime listingDate = DateTime.TryParse(listingDateStr, out DateTime date) ? date : DateTime.MinValue;

                        string description = descriptionNode?.InnerText.Trim() ?? "";

                        listings.Add(new OLX_Data
                        {
                            ProductName = productName,
                            Price = price,
                            Location = location,
                            Link = link,
                            ListingDate = listingDate,
                            Description = description
                        });
                    }
                }
            }

            return listings;
        }
    }
}
