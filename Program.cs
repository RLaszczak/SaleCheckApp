using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using MongoDB.Driver;
using System;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

//Mongo
builder.Services.AddSingleton<IMongoDatabase>(serviceProvider =>
{
    const string connectionUri = "mongodb+srv://Admin:Admin123@salecheck.xbacbcx.mongodb.net/?retryWrites=true&w=majority";

    var mongoClient = new MongoClient(connectionUri);

    // Dodaj sprawdzenie stanu klastra MongoDB
    var clusterState = mongoClient.Cluster.Description.State;
    if (clusterState == MongoDB.Driver.Core.Clusters.ClusterState.Connected)
    {
        Console.WriteLine("Po³¹czono z baz¹ danych.");
    }
    else
    {
        Console.WriteLine("Nie uda³o siê po³¹czyæ z baz¹ danych.");
    }

    return mongoClient.GetDatabase("SALECHECK");
});

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
