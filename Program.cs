using MongoDB.Bson.Serialization.Conventions;
using MongoDB.Driver;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

//Mongo

builder.Services.AddSingleton<IMongoDatabase>(serviceProvider =>
{
    var configuration = serviceProvider.GetRequiredService<IConfiguration>();
    var mongoDbConnectionString = configuration.GetConnectionString("MongoDB");

    var pack = new ConventionPack
    {
        new CamelCaseElementNameConvention(),
        new StringIdStoredAsObjectIdConvention()
    };
    ConventionRegistry.Register("ZPP2_convention", pack, _ => true);

    var url = new MongoUrl(mongoDbConnectionString);
    var settings = MongoClientSettings.FromUrl(url);
    var client = new MongoClient(settings);
    return client.GetDatabase(url.DatabaseName);
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

//³¹czenie z Mongo DB

/*var pack = new ConventionPack
{
    new CamelCaseElementNameConvention(),
    new StringIdStoredAsObjectIdConvention()
};
ConventionRegistry.Register("ZPP2_convention", pack, _ => true);*/


//MongoUrl url = new MongoUrl(builder.Configuration.GetConnectionString("MongoDB"));
//MongoClientSettings settings = MongoClientSettings.FromUrl(url);
//MongoClient client = new MongoClient(settings);
//IMongoDatabase database = client.GetDatabase(url.DatabaseName);


//³¹czenie z Mongo DB


app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
