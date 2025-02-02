using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace Backend.Models;

public class ProductColor
{
    [JsonIgnore]public int ProductId { get; set; }
    [JsonIgnore]public Product Product { get; set; }
    public uint Color { get; set; } //rgba
    public string ColorName { get; set; }

    public ProductColor(uint color, string colorName)
    {
        Color = color;
        ColorName = colorName;
    }
}