namespace Backend.Models;

public class Outfit
{
    public int Id { get; set; }
    public string ImageName { get; set; }
    public ICollection<Product> Clothes { get; set; } = new List<Product>();
    
    public Outfit(string imageName, ICollection<Product> clothes)
    {
        ImageName = imageName;
        Clothes = clothes;
    }
    
    public Outfit()
    {
    }
}