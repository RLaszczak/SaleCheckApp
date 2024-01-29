using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using MongoDB.Driver;
using System;
using static ProductController;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

//Mongo
builder.Services.AddSingleton<IMongoCollection<Product>>(serviceProvider =>
{
    const string connectionUri = "mongodb+srv://Admin:Admin123@salecheck.xbacbcx.mongodb.net/?retryWrites=true&w=majority";

 

    var mongoClient = new MongoClient(connectionUri);
    var database = mongoClient.GetDatabase("SALECHECK");    

    var collection = database.GetCollection<Product>("SaleCheckTest");

    return collection;
});
//get mongo

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}
    
app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
