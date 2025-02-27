using System.Diagnostics;
using System.Text.Json.Serialization;
using Backend;
using Backend.Data;
using Backend.Models;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.FileProviders;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.ColorSpaces.Conversion;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using Color = Backend.Models.Color;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
    });
builder.Services.AddOpenApi();

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
if (string.IsNullOrEmpty(connectionString))
{
    throw new InvalidOperationException("Connection string is missing!");
}

builder.Services.AddDbContext<ClothingDbContext>(options =>
{
    options.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString))
        /*.LogTo(Console.WriteLine, LogLevel.Information)
        .EnableSensitiveDataLogging()
        .EnableDetailedErrors()*/;
});

builder.Services.Configure<FormOptions>(options =>
{
    options.BufferBody = true;  // Buffer the entire body before processing
    options.MultipartBodyLengthLimit = 100 * 1024 * 1024; // Increase limit if needed
});


var app = builder.Build();


// Seed the database with initial data
//TODO: understand better this thing
// Console.Out.WriteLine("Seeding database");
// using (var scope = app.Services.CreateScope())
// {
//     var context = scope.ServiceProvider.GetService<ClothingDbContext>();
//     if (context == null)
//     {
//         throw new InvalidOperationException("Failed to retrieve the database context.");
//     }
//
//     Product pants = context.Products.FirstOrDefault(p => p.Id == 78);
//     Product hoodie = context.Products.FirstOrDefault(p => p.Id == 24);
//     Product shoes = context.Products.FirstOrDefault(p => p.Id == 79);
//     
//     Console.WriteLine(pants.Name);
//     Console.WriteLine(hoodie.Name);
//     Console.WriteLine(shoes.Name);
//     
//     Image<Rgba32> hoodieImage = Image.Load<Rgba32>($"Upload/{hoodie.ImageName}");
//     Image<Rgba32> pantsImage = Image.Load<Rgba32>($"Upload/{pants.ImageName}");
//     pantsImage.Mutate(x => x.Resize(hoodieImage.Width  * 3/4, 0));
//     Image<Rgba32> shoesImage = Image.Load<Rgba32>($"Upload/{shoes.ImageName}");
//     shoesImage.Mutate(x => x.Resize(pantsImage.Width, 0));
//     
//     int width = 1000;
//     int height = pantsImage.Height + hoodieImage.Height;
//     
//     Image<Rgba32> fit = new Image<Rgba32>(1000, 2000);
//     fit.Mutate(x => x 
//         .DrawImage(hoodieImage, new Point(0, 0), 1)
//         .DrawImage(pantsImage, new Point(hoodieImage.Width  /6, hoodieImage.Height ), 1)
//         .DrawImage(shoesImage, new Point(int.Max(0, hoodieImage.Width /4 - shoesImage.Width/6), hoodieImage.Height + pantsImage.Height/*hoodieImage.Width, height - shoesImage.Height*/), 1));
//     fit.Save("Upload/fit.png");
// }



// Configure the 4TTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();
//app.UseRouting();
app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new PhysicalFileProvider(Path.Combine(Directory.GetCurrentDirectory(), "Upload")),
    RequestPath = "/Upload"
});

//app.UseAuthorization();

/*app.UseEndpoints(endpoints =>
{
    endpoints.MapControllers();
});*/

var summaries = new[]
{
    "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
};

app.MapGet("/weatherforecast", () =>
    {
        var forecast = Enumerable.Range(1, 5).Select(index =>
                new WeatherForecast
                (
                    DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
                    Random.Shared.Next(-20, 55),
                    summaries[Random.Shared.Next(summaries.Length)]
                ))
            .ToArray();
        return forecast;
    })
    .WithName("GetWeatherForecast");

app.MapControllerRoute(
    name: "default",
    pattern: "{controller}/{action=Index}/{id?}");
app.Run();

record WeatherForecast(DateOnly Date, int TemperatureC, string? Summary)
{
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
}