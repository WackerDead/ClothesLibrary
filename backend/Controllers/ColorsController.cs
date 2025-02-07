using Backend.Data;
using Backend.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SixLabors.ImageSharp.PixelFormats;

namespace Backend;

[ApiController]
[Route("[controller]")]
public class ColorsController : ControllerBase
{
    private readonly ClothingDbContext _context;
    private readonly IWebHostEnvironment _environment;

    public ColorsController(ClothingDbContext context, IWebHostEnvironment environment)
    {
        _context = context;
        _environment = environment;
    }
    
    [HttpGet]
    public async Task<IEnumerable<object>> GetColors()
    {
        return await _context.Colors.Select(c => new { value = c.Name, label = c.Name, color = $"#{c.Value:X6}" }).ToListAsync();
    }

    [HttpGet("seed")]
    public void SeedColors()
    {
        Console.WriteLine("Seeding colors");
        _context.Colors.RemoveRange(_context.Colors);
        _context.Colors.AddRange(ColorExtractor.clothingColors.Select(c => new Color(c.Key, ColorExtractor.GetRGBValue(new Rgba32(c.Value.R, c.Value.G, c.Value.B)))));
        _context.SaveChanges();
    }
}