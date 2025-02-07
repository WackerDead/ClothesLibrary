using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace Backend.Models;

public class ProductColor
{
    [JsonIgnore]public int ProductId { get; set; }
    [JsonIgnore]public Product Product { get; set; }
    [JsonIgnore]public int ColorId { get; set; }
    [JsonIgnore]public Color Color { get; set; }
    public uint ExactColor { get; set; } //rgba

    public ProductColor(uint exactColor, Color color)
    {
        ExactColor = exactColor;
        Color = color;
    }
    
    public ProductColor()
    {
    }
}