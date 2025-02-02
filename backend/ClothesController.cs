using System.Drawing;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;
using Backend.Data;
using Backend.Models;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace Backend;

[ApiController]
[Route("[controller]")]
public class ClothesController : ControllerBase
{
    private readonly ClothingDbContext _context;
    private readonly IWebHostEnvironment _environment;

    public ClothesController(ClothingDbContext context, IWebHostEnvironment environment)
    {
        _context = context;
        _environment = environment;
    }

    [HttpGet]
    public async Task<IEnumerable<Product>> GetProducts()
    {
        // var a = await _context.Products.FirstAsync();
        //Console.WriteLine(a);
        //Console.WriteLine(a.Colors);
        return await _context.Products.Include(p => p.ProductColors).ToListAsync();
    }

    [HttpGet("colors")]
    public async Task<IEnumerable<ProductColor>> GetProductColors()
    {
        return await _context.ProductColors.ToListAsync();
    }

    [HttpGet("colors/{id}")]
    public async Task<IEnumerable<string>> GetColors(int id)
    {
        return await _context.ProductColors.Where(pc => pc.ProductId == id).Select(pc => $"#{pc.Color:X6}")
            .ToListAsync();
    }

    [HttpGet("{id}")]
    public async Task<Product> GetProduct(int id)
    {
        // var pc = _context.ProductColors.Where(pc => pc.ProductId == id);
        // Console.WriteLine(pc);
        // await pc.ForEachAsync(Console.WriteLine);
        return await _context.Products.Include(p => p.ProductColors).FirstAsync(p => p.Id == id);
    }


    public class ProductDto
    {
        public int? Id { get; set; }
        public string Name { get; set; }
        public string Brand { get; set; }
        public string Type { get; set; }
        public IFormFile Image { get; set; }
    }
    
    //Edit product
    [HttpPut("{id}")]
    public async Task<IActionResult> PutProduct(int id, [FromForm] ProductDto productDto)
    {
        /*if (id != productDto.Id)
        {
            return BadRequest(new { message = "Id mismatch" });
        }*/
        
        var product = await _context.Products.FirstOrDefaultAsync(p => p.Id == id);
        string oldImage = "";
        
        if (product == null)
        {
            return NotFound(new { message = "Product not found" });
        }

        product.Name = productDto.Name;
        product.Brand = productDto.Brand;
        product.Type = (ProductType)Enum.Parse(typeof(ProductType), productDto.Type, true);
        if (productDto.Image.FileName != product.ImageName)
        {
            string fileName = productDto.Type + "_" + productDto.Image.FileName;
            var path = Path.Combine(_environment.ContentRootPath, "Upload", fileName);
            Image<Rgba32> image;

            try
            {
                image = await SaveImage(productDto.Image, path);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return BadRequest(new { message = "Failed to upload image" });
            }
                
            var productColors = ColorExtractor.ExtractProductColors(image);
            //TODO: delete old image
            oldImage = product.ImageName;
            _context.ProductColors.RemoveRange(product.ProductColors);
            product.ImageName = fileName;
            product.ProductColors = productColors;
        }

        await _context.SaveChangesAsync();
        return Ok(new { message = "Product updated successfully" });
    }

    //Add product
    [HttpPost]
    public async Task<IActionResult> PostProduct([FromForm] ProductDto productDto)
    {
        Console.WriteLine(productDto.Name);
        Console.WriteLine(productDto.Brand);
        Console.WriteLine(productDto.Type);
        string fileName = productDto.Type + "_" + productDto.Image.FileName;
        var path = Path.Combine(_environment.ContentRootPath, "Upload", fileName);
        Image<Rgba32> image;

        try
        {
            image = await SaveImage(productDto.Image, path);
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            return BadRequest(new { message = "Failed to upload image" });
        }
        
        var productColors = ColorExtractor.ExtractProductColors(image);

        var product = new Product(productDto.Name, productDto.Brand,
            (ProductType)Enum.Parse(typeof(ProductType), productDto.Type, true), fileName, productColors);

        _context.Products.Add(product);
        await _context.SaveChangesAsync();
        
        return Ok(new { message = "Product uploaded successfully" });

        /*return CreatedAtAction("GetProduct", new { id = product.Id }, product);*/
    }

    private static async Task<Image<Rgba32>> SaveImage(IFormFile image, string path)
    {
        await using Stream imageStream = image.OpenReadStream();
        imageStream.Position = 0;

        await using var fileStream = new FileStream(path, FileMode.Create);
        await imageStream.CopyToAsync(fileStream);
        imageStream.Position = 0;

        return await Image.LoadAsync<Rgba32>(imageStream);
    }

    /*[HttpGet("seed")]
    public void SeedDatabase()
    {
        Console.WriteLine("Seeding database");
        if (!_context.Products.Any())
        {
            _context.Products.RemoveRange(_context.Products);
            _context.SaveChanges();
            _context.Products.AddRange(
                new Product
                {
                    Name = "Basic T-Shirt", Brand = "BrandA", Type = ProductType.Tshirt,
                    ProductColors = new List<ProductColor> { new(0), new(255) }
                },
                new Product
                {
                    Name = "Jeans", Brand = "BrandB", Type = ProductType.Pants,
                    ProductColors = new List<ProductColor> { new(324), new(7609) }
                },
                new Product
                {
                    Name = "Running Shoes", Brand = "BrandC", Type = ProductType.Shoes,
                    ProductColors = new List<ProductColor> { new(3459), new(2012) }
                }
            );
            _context.SaveChanges();
            Console.Out.WriteLine("Database seeded");
        }
    }*/
}