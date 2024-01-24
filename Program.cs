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
    var configuration = serviceProvider.GetRequiredService<IConfiguration>();
    var mongoDbConnectionString = configuration.GetConnectionString("MongoDBAtlas");

    var mongoClient = new MongoClient(mongoDbConnectionString);

    // Dodaj sprawdzenie stanu klastra MongoDB
    var clusterState = mongoClient.Cluster.Description.State;
    if (clusterState == MongoDB.Driver.Core.Clusters.ClusterState.Connected)
    {
        Console.WriteLine("Po��czono z baz� danych.");
    }
    else
    {
        Console.WriteLine("Nie uda�o si� po��czy� z baz� danych.");
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
