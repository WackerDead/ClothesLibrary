using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace Backend.Models;

public enum ProductType
{
    Tshirt,
    Pants,
    Shoes,
    Sweatshirt,
    Hoodie
}

public enum WeatherType
{
    Any,
    Cold,
    Hot,
    Rainy,
}

public class Product
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string Brand { get; set; }

    [JsonConverter(typeof(JsonStringEnumConverter))]
    public ProductType Type { get; set; }
    //public WeatherType Weather { get; set; } = WeatherType.Any;
    
    public bool Favorite { get; set; } = false;
    
    public string ImageName { get; set; }

    public ICollection<ProductColor> ProductColors = new List<ProductColor>();

    [NotMapped]
    public List<Color> Colors => ProductColors.Select(pc => pc.Color).ToList();

    public Product(string name, string brand, ProductType type, string imageName, ICollection<ProductColor> productColors)
    {
        Name = name;
        Brand = brand;
        Type = type;
        ImageName = imageName;
        ProductColors = productColors;
    }

    public Product()
    {
    }
}