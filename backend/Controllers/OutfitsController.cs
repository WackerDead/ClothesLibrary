using Backend.Data;
using Backend.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;

namespace Backend;

[ApiController]
[Route("[controller]")]
public class OutfitsController : ControllerBase
{
    private readonly ClothingDbContext _context;
    private readonly IWebHostEnvironment _environment;

    public OutfitsController(ClothingDbContext context, IWebHostEnvironment environment)
    {
        _context = context;
        _environment = environment;
    }
    
    [HttpGet]
    public async Task<IEnumerable<Outfit>> GetOutfits()
    {
        return await _context.Outfits.Include(o => o.Clothes).ThenInclude(p => p.ProductColors).ThenInclude(pc => pc.Color).ToListAsync();
    }

    public class OutfitDto
    {
        public int? Id {get; set;}
        public string Name { get; set; }
        public int[] Clothes { get; set; }
    }

    [HttpPost]
    public async Task<IActionResult> PostOutfit(int[] clothes)
    {
        /*Console.WriteLine(outfit.Name);
        Console.WriteLine(outfit.Clothes);
        foreach (var outfitClothe in outfit.Clothes)
        {
            Console.WriteLine(outfitClothe);
        }
        List<Product> clothes = outfit.Clothes.Select(id => _context.Products.First(p => p.Id == id)).ToList();
        
        var image = await GenerateOutfitImage(clothes);
        string fileName = DateTime.Now.Ticks + ".png";
        var path = Path.Combine(_environment.ContentRootPath, "Upload/Outfits", fileName);
        await image.SaveAsPngAsync(path);

        Outfit newOutfit = new Outfit(fileName, clothes);
        _context.Outfits.Add(newOutfit);
        await _context.SaveChangesAsync();
        return Ok(new { message = "Outfit created successfully" });*/
        
        foreach (var clothe in clothes)
        {
            Console.WriteLine(clothe);
        }
        
        List<Product> clothesList = await _context.Products.Where(p => clothes.AsQueryable().Contains(p.Id)).ToListAsync();
        
        Console.WriteLine("aeda");
        var image = await GenerateOutfitImage(clothesList);
        string fileName = DateTime.Now.Ticks + ".png";
        var path = Path.Combine(_environment.ContentRootPath, "Upload/Outfits", fileName);
        await image.SaveAsPngAsync(path);

        Outfit newOutfit = new Outfit(fileName, clothesList);
        _context.Outfits.Add(newOutfit);
        await _context.SaveChangesAsync();
        return Ok(new { message = "Outfit created successfully" });
    }
    
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteOutfit(int id)
    {
        var outfit = await _context.Outfits.FindAsync(id);
        if (outfit == null)
        {
            return NotFound();
        }

        _context.Outfits.Remove(outfit);
        ClothesController.DeleteImage(Path.Combine(_environment.ContentRootPath, "Upload/Outfits", outfit.ImageName));
        await _context.SaveChangesAsync();
        return NoContent();
    }

    private static async Task<Image<Rgba32>> GenerateOutfitImage(List<Product> clothes)
    {
        Product pants = clothes.First(c => c.Type == ProductType.Pants);
        Product hoodie = clothes.First(c => c.Type == ProductType.Hoodie);
        Product shoes = clothes.First(c => c.Type == ProductType.Shoes);
    
        Console.WriteLine(pants.Name);
        Console.WriteLine(hoodie.Name);
        Console.WriteLine(shoes.Name);
    
        Image<Rgba32> hoodieImage = Image.Load<Rgba32>($"Upload/{hoodie.ImageName}");
        Image<Rgba32> pantsImage = Image.Load<Rgba32>($"Upload/{pants.ImageName}");
        pantsImage.Mutate(x => x.Resize(hoodieImage.Width  * 3/4, 0));
        Image<Rgba32> shoesImage = Image.Load<Rgba32>($"Upload/{shoes.ImageName}");
        shoesImage.Mutate(x => x.Resize(pantsImage.Width, 0));
    
        int width = int.Max(hoodieImage.Width, int.Max(pantsImage.Width, shoesImage.Width));
        int height = pantsImage.Height + hoodieImage.Height + shoesImage.Height;
    
        Image<Rgba32> fit = new Image<Rgba32>(width, height);
        fit.Mutate(x => x 
            .DrawImage(hoodieImage, new Point(0, 0), 1)
            .DrawImage(pantsImage, new Point(hoodieImage.Width  /6, hoodieImage.Height ), 1)
            .DrawImage(shoesImage, new Point(int.Max(0, hoodieImage.Width /4 - shoesImage.Width/6), hoodieImage.Height + pantsImage.Height/*hoodieImage.Width, height - shoesImage.Height*/), 1));
        //fit.Save("Upload/fit.png");
        return fit;
    }
}