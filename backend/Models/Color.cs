using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace Backend.Models;

public class Color
{
    public int Id { get; set; }
    public string Name { get; set; }
    public uint Value { get; set; }
    [JsonIgnore]public ICollection<ProductColor> ProductColors { get; set; } = new List<ProductColor>();
    
    public Color(string name, uint value)
    {
        Name = name;
        Value = value;
    }
}