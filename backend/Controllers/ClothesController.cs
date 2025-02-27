using System.Globalization;
using System.IO.Compression;
using System.Text;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;
using Backend.Data;
using Backend.Models;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using Color = Backend.Models.Color;

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
        return await _context.Products.Include(p => p.ProductColors).ThenInclude(p => p.Color).ToListAsync();
    }

    [HttpGet("colors")]
    public async Task<IEnumerable<ProductColor>> GetProductColors()
    {
        return await _context.ProductColors.ToListAsync();
    }

    [HttpGet("colors/{id}")]
    public async Task<IEnumerable<string>> GetColors(int id)
    {
        return await _context.ProductColors.Where(pc => pc.ProductId == id).Select(pc => $"#{pc.ExactColor:X6}")
            .ToListAsync();
    }

    [HttpGet("brands")]
    public async Task<IEnumerable<object>> GetBrands()
    {
        return await _context.Products.Select(p => p.Brand).Distinct().Select(b => new { value = b, label = b })
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


    //Add product
    [HttpPost]
    public async Task<IActionResult> PostProduct([FromForm] ProductDto productDto)
    {
        Console.WriteLine(productDto.Name);
        Console.WriteLine(productDto.Brand);
        Console.WriteLine(productDto.Type);
        string fileName = DateTime.Now.Ticks + ".png";
        var path = Path.Combine(_environment.ContentRootPath, "Upload", fileName);
        Image<Rgba32> image = await ConvertToImage(productDto.Image);
        image = CropImage(image);

        try
        {
            //image = await SaveImage(productDto.Image, path);
            await image.SaveAsPngAsync(path);
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            return BadRequest(new { message = "Failed to upload image" });
        }

        var productColors = GetProductColors(image);

        var product = new Product(productDto.Name, productDto.Brand,
            (ProductType)Enum.Parse(typeof(ProductType), productDto.Type, true), fileName, productColors);
        product.Name = $"{product.Brand} {productDto.Type} {productColors[0].Color.Name}";

        _context.Products.Add(product);
        await _context.SaveChangesAsync();

        //return Ok(new { message = "Product uploaded successfully" });

        return CreatedAtAction("GetProduct", new { id = product.Id }, product);
    }

    //Edit product
    [HttpPut("{id}")]
    public async Task<IActionResult> PutProduct(int id, [FromForm] ProductDto productDto)
    {
        /*if (id != productDto.Id)
        {
            return BadRequest(new { message = "Id mismatch" });
        }*/

        var product = await _context.Products.Include(p => p.ProductColors).ThenInclude(p => p.Color)
            .FirstOrDefaultAsync(p => p.Id == id);
        Console.WriteLine("Product: " + product.Name);
        product.ProductColors.ToList().ForEach(pc => Console.WriteLine(pc.Color.Name));
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
            string fileName = DateTime.Now.Ticks + ".png";
            var path = Path.Combine(_environment.ContentRootPath, "Upload", fileName);
            Image<Rgba32> image = await ConvertToImage(productDto.Image);
            image = CropImage(image);

            try
            {
                //image = await SaveImage(productDto.Image, path);
                await image.SaveAsPngAsync(path);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return BadRequest(new { message = "Failed to upload image" });
            }

            //TODO: delete old image
            oldImage = product.ImageName;
            DeleteImage(Path.Combine(_environment.ContentRootPath, "Upload", oldImage));

            var productColors = GetProductColors(image);
            _context.ProductColors.RemoveRange(_context.ProductColors.Where(pc => pc.ProductId == product.Id));
            product.ImageName = fileName;
            product.ProductColors = productColors;
        }

        product.Name = $"{product.Brand} {productDto.Type} {product.ProductColors.First().Color.Name}";

        await _context.SaveChangesAsync();
        //return Ok(new { message = "Product updated successfully" });

        return CreatedAtAction("GetProduct", new { id = product.Id }, product);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteProduct(int id)
    {
        var product = await _context.Products.FindAsync(id);
        if (product == null)
        {
            return NotFound(new { message = "Product not found" });
        }

        _context.Products.Remove(product);
        await _context.SaveChangesAsync();
        DeleteImage(Path.Combine(_environment.ContentRootPath, "Upload", product.ImageName));
        return Ok(new { message = "Product deleted successfully" });
    }

    private static async Task<Image<Rgba32>> ConvertToImage(IFormFile image)
    {
        await using Stream imageStream = image.OpenReadStream();
        imageStream.Position = 0;

        return await Image.LoadAsync<Rgba32>(imageStream);
    }

    private static Image<Rgba32> CropImage(Image<Rgba32> image)
    {
        int left = int.MaxValue, top = int.MaxValue;
        int right = 0, bottom = 0;
        
        Image<Rgba32> newImage = image.CloneAs<Rgba32>();

        for (int i = 0; i < image.Width; i++)
        {
            for (int j = 0; j < image.Height; j++)
            {
                Rgba32 pixel = image[i, j];
                if (pixel.A == 0) continue;

                if (j < top)
                {
                    Console.WriteLine(j);
                    top = j;
                }
                if (j > bottom) bottom = j;
                if (i < left) left = i;
                if (i > right) right = i;
            }
        }
        
        newImage.Mutate(x => x.Crop(Rectangle.FromLTRB(left, top, right, bottom)));
        
        return newImage;
    }
    
    private static async Task SaveImage(Image image, string path)
    {
        await image.SaveAsPngAsync(path);
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

    public static void DeleteImage(string path)
    {
        Console.WriteLine("Deleting image - {0}", path);
        System.IO.File.Delete(path);
    }

    private List<ProductColor> GetProductColors(Image<Rgba32> image)
    {
        Console.WriteLine("Getting colors");
        var rgbList = ColorExtractor.ExtractColors(image);
        Console.WriteLine("Colors extracted");
        var colorList = _context.Colors.ToList();
        var colors = rgbList.Select(c => new KeyValuePair<uint, Color>(c, ColorExtractor.GetClosestColor(colorList, c))).ToList();
        Console.WriteLine("Colors matched");
        colors.ForEach(c =>
        {
            Console.WriteLine($"#{c.Key:X6} -> {c.Value.Name}");
        });
        var productColors = colors.Select(c => new ProductColor(c.Key, c.Value)).ToList();
        Console.WriteLine("Product colors created");
        return productColors;
    }

    public class ProductFilterDTO
    {
        public List<string> Type { get; set; }
        public List<string> Brand { get; set; }
        public List<string> Color { get; set; }
    }

    [HttpPost("filter")]
    public IEnumerable<Product> GetFilteredProducts([FromBody] ProductFilterDTO filterDto)
    {
        Console.WriteLine("Filter DTO Types: " + string.Join(", ", filterDto.Type));
        Console.WriteLine("Filter DTO Brands: " + string.Join(", ", filterDto.Brand));
        Console.WriteLine("Filter DTO Colors: " + string.Join(", ", filterDto.Color));

        foreach (var product in _context.Products.Include(p => p.ProductColors).ThenInclude(pc => pc.Color))
        {
            Console.WriteLine(
                $"Product: {product.Name}, Has Colors: {product.ProductColors != null}, Type: {product.Type}, Brand: {product.Brand}");
            Console.WriteLine("Colors: ");
            foreach (var pc in product.ProductColors)
            {
                Console.WriteLine($"Color: {pc.Color.Name}");
            }
        }

        return _context.Products.Include(p => p.ProductColors).ThenInclude(p => p.Color).ToList().Where(p =>
            (filterDto.Brand.Count < 1 || filterDto.Brand.Contains(p.Brand)) &&
            (filterDto.Type.Count < 1 || filterDto.Type.Contains(p.Type.ToString().ToLower())) &&
            (filterDto.Color.Count < 1 || (p.ProductColors != null &&
                                           filterDto.Color.Any(c =>
                                               p.ProductColors.Any(pc => pc.Color != null && pc.Color.Name == c))))
        );
    }

    [HttpGet("download")]
    public async Task<IActionResult> DownloadTheZipFile()
    {
        var products = await GetProducts();

        Console.WriteLine("Products: ");
        foreach (var product in products)
        {
            Console.WriteLine($"Product: {product.Name}, Type: {product.Type}, Brand: {product.Brand}");
            Console.WriteLine("Colors: ");
            foreach (var pc in product.ProductColors)
            {
                Console.WriteLine($"Color: {pc.Color.Name}");
            }
        }

        var memoryStream = new MemoryStream();

        using (var zipArchive = new ZipArchive(memoryStream, ZipArchiveMode.Create, true))
        {
            var jsonEntry = zipArchive.CreateEntry("products.json");

            using (var entryStream = jsonEntry.Open())
            using (var streamWriter = new StreamWriter(entryStream, Encoding.UTF8))
            {
                string jsonData = JsonSerializer.Serialize(products);
                await streamWriter.WriteAsync(jsonData);
            }

            foreach (var product in products)
            {
                var imageEntry = zipArchive.CreateEntry("images/" + product.ImageName);
                using (var entryStream = imageEntry.Open())
                {
                    using var imageStream =
                        System.IO.File.OpenRead(Path.Combine(_environment.ContentRootPath, "Upload",
                            product.ImageName));
                    await imageStream.CopyToAsync(entryStream);
                }
            }
        }

        memoryStream.Position = 0;
        return File(memoryStream, "application/zip", "export.zip");
    }

    [HttpPost("upload")]
    public async Task<IActionResult> UploadTheZipFile([FromForm] IFormFile file)
    {
        Console.WriteLine("Uploading file");
        if (file == null)
        {
            return BadRequest(new { message = "No file uploaded" });
        }

        await using var fileStream = file.OpenReadStream();
        var memoryStream = new MemoryStream();
        await fileStream.CopyToAsync(memoryStream);
        memoryStream.Position = 0;

        using (var zipArchive = new ZipArchive(memoryStream, ZipArchiveMode.Read, true))
        {
            var jsonEntry = zipArchive.GetEntry("products.json");
            if (jsonEntry == null)
            {
                return BadRequest(new { message = "No json file found in zip" });
            }

            using var entryStream = jsonEntry.Open();
            using var streamReader = new StreamReader(entryStream, Encoding.UTF8);
            var jsonData = await streamReader.ReadToEndAsync();
            var products = JsonSerializer.Deserialize<List<Product>>(jsonData);
            var productsDto = products.Select(p => new ProductDto()
            {
                Brand = p.Brand,
                Name = p.Name,
                Type = p.Type.ToString(),
                Id = p.Id,
                Image = CreateIFormImage(zipArchive.GetEntry("images/" + p.ImageName).Open(), p.ImageName)
            }).ToList();

            foreach (var productDto in productsDto)
            {
                Console.WriteLine(productDto);
                await PostProduct(productDto);
            }

            //Console.WriteLine("Adding products to database");
            //_context.Products.AddRange(products);
            //await _context.SaveChangesAsync();
        }

        Console.WriteLine("Returning ok");
        return Ok(new { message = "Products uploaded successfully" });
    }

    public static IFormFile CreateIFormImage(Stream fileStream, string imageName)
    {
        var memoryStream = new MemoryStream();
        fileStream.CopyTo(memoryStream);
        memoryStream.Position = 0;
        return new FormFile(memoryStream, 0, memoryStream.Length, "Image", imageName);
    }
}