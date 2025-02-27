using Backend.Data;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.ColorSpaces;
using SixLabors.ImageSharp.ColorSpaces.Conversion;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using Size = SixLabors.ImageSharp.Size;
using Color = Backend.Models.Color;

namespace Backend;

public class ColorExtractor
{
    class HueColor
    {
        public int hue;
        public int[] saturations;
        public int[] values;
        public int count;

        public HueColor(int hue)
        {
            this.hue = hue;
            saturations = new int[40];
            values = new int[40];
            count = 0;
        }

        public override string ToString()
        {
            Console.Write("Hue " + hue);
            for (int i = 0; i < saturations.Length; i++)
            {
                Console.Write(", Saturation " + i * 10 + " " + saturations[i]);
            }

            for (int i = 0; i < values.Length; i++)
            {
                Console.Write(", Value " + i * 10 + " " + values[i]);
            }

            return "Hue: " + hue + ", Saturation: " + saturations + ", Value: " + values;
        }
    }

    private static int distance = 8;

    public static List<uint> ExtractColors(Image<Rgba32> image)
    {
        var colors = ExtractPaletteHueCluster(image, 6);
        return colors.Select(GetRGBValue).ToList();
    }

    public static uint GetRGBValue(Rgba32 color)
    {
        return (uint)(color.R << 16 | color.G << 8 | color.B);
    }

    /// <summary>
    /// Extracts a color palette from an image by clustering hues.
    /// </summary>
    /// <param name="image">The image from which to extract the color palette.</param>
    /// <param name="paletteSize">The maximum number of colors to include in the palette.</param>
    /// <returns>A list of colors representing the extracted palette.</returns>
    public static List<Rgba32> ExtractPaletteHueCluster(Image<Rgba32> image, int paletteSize)
    {
        HueColor[] hues = new HueColor[256];
        int[] hueClusters = new int[256];

        // Resize the image to a fixed width of 200 pixels, maintaining the aspect ratio
        image.Mutate(x => x
            .Resize(new ResizeOptions { Sampler = KnownResamplers.NearestNeighbor, Size = new Size(200, 0) }));

        // Iterate over each pixel in the image
        for (int i = 0; i < image.Width; i++)
        {
            for (int j = 0; j < image.Height; j++)
            {
                var pixel = image[i, j];
                if (pixel.A == 0) continue;

                var hsv = ColorSpaceConverter.ToHsv(pixel);
                int hue = (int)Math.Round(hsv.H) * hues.Length / 360;

                // Structure to store the saturation and value for each hue
                HueColor hc;
                if ((hc = hues[hue]) == null) // If the hue is not in the array, create a new one
                {
                    hc = new HueColor(hue);
                }

                // Increment the count of the hue
                hc.count++;

                int saturation = (int)(hsv.S * (hc.saturations.Length - 1));
                hc.saturations[saturation]++;

                int value = (int)(hsv.V * (hc.values.Length - 1));
                hc.values[value]++;
                hues[hue] = hc;
            }
        }

        // Create clusters of 9 hues
        for (int i = 0; i < hues.Length; i++)
        {
            int hueCluster = 0;

            for (int count = 0, j = i; count < 9; count++, j++)
            {
                var hc = hues[j % hues.Length];
                if (hc == null) continue;
                hueCluster += hc.count;
            }

            hueClusters[(i + 5) % hues.Length] = hueCluster;
        }

        (int recommendedSize, List<int> huePalette) = ClusterExtractor(hueClusters.ToList(), paletteSize);
        List<Rgba32> palette = new();

        for (int i = 0; i < huePalette.Count; i++)
        {
            int hue = huePalette[i];
            
            //int index = hue - 1;
            int up = hue, down = hue;
            HueColor color;
            while (true)
            {
                // color = hues[++index];
                color = hues[up++ % hues.Length];
                if (color != null) break;
                color = hues[down-- % hues.Length];
                if (color != null) break;
            }

            //TODO: When saturation is 0, the value is the only one that matters, so make it so if saturation is 0 and value is the same, then its the same color
            int h = hue * 360 / 256;
            float s = (float)color.saturations.Index().First(s => s.Item == color.saturations.Max()).Index / 39;
            float v = (float)color.values.Index().First(s => s.Item == color.values.Max()).Index / 39;
            var c = ColorSpaceConverter.ToRgb(new Hsv(h, s, v));
            var rgba32 = new Rgba32(c.R, c.G, c.B);
            Console.WriteLine("#{0} : {1}", rgba32.ToHex(),
                hueClusters[hue]);
            if(i < recommendedSize && !palette.Contains(rgba32)) palette.Add(rgba32);
        }

        return palette;
    }

    /// <summary>
    /// Extracts clusters of hues from a list of hue counts.
    /// </summary>
    /// <param name="hueClusters">A list of hue counts.</param>
    /// <param name="clusters">The number of clusters to extract.</param>
    /// <returns>A list of hue indices representing the extracted clusters.</returns>
    private static (int, List<int>) ClusterExtractor(List<int> hueClusters, int clusters)
    {
        Dictionary<int, int> palette = new();
        int recommendedSize = clusters;
        int maxValue = 0;

        for (int i = 0, skipCount = 0; i < clusters && skipCount < 5; i++)
        {
            // Find the most common hue in the list
            int hue = hueClusters.IndexOf(hueClusters.Max());
            //if (hue == 0) break;

            // Calculate the start index for the cluster
            int clusterStart = ((hue - 4) % hueClusters.Count + hueClusters.Count) % hueClusters.Count;
            int value = 0;

            // Get the total value of the cluster and erase the cluster from the list
            for (int j = clusterStart; j < clusterStart + 9; j++)
            {
                value += hueClusters[j % hueClusters.Count];
                hueClusters[j % hueClusters.Count] = 0;
            }

            // Ensure the cluster is sufficiently spaced from existing clusters
            int minDistance = hueClusters.Count;
            foreach (var (key, _) in palette)
            {
                int distance = Math.Abs(hue - key);
                if (distance < minDistance)
                {
                    minDistance = distance;
                }
            }

            // If the cluster is too close to an existing cluster, skip it
            if (minDistance < distance)
            {
                i--;
                skipCount++;
                continue;
            }
            skipCount = 0;

            // Update the maximum value and add the cluster to the palette
            if (value > maxValue)
            {
                maxValue = value;
            }

            // If the cluster has a value less than a quarter of the maximum value, stop adding clusters
            if (value < maxValue / 4)
            {
                Console.WriteLine("3 - {0}", i);
                if (i < recommendedSize)
                {
                    recommendedSize = i;
                }
            }

            palette.Add(hue, value);
        }
        // Return the list of hue indices representing the extracted clusters
        return (recommendedSize, palette.OrderByDescending(dic => dic.Value).Select(dic => dic.Key).ToList());
    }

    public static Dictionary<string, Rgb24> clothingColors = new Dictionary<string, Rgb24>
    {
        { "Black", new Rgb24(0, 0, 0) },
        { "White", new Rgb24(255, 255, 255) },
        { "Red", new Rgb24(255, 0, 0) },
        { "Green", new Rgb24(0, 128, 0) },
        { "Blue", new Rgb24(0, 0, 255) },
        { "Yellow", new Rgb24(255, 220, 0) },
        { "Orange", new Rgb24(255, 165, 0) },
        { "Purple", new Rgb24(128, 0, 128) },
        { "Pink", new Rgb24(255, 192, 203) },
        { "Brown", new Rgb24(137, 81, 41) },
        { "DarkGray", new Rgb24(80, 80, 80) },
        { "LightGray", new Rgb24(200, 200, 200) },
    };

    /*public static string GetClosestColor(Rgb24 color)
    {
        string closest = "";
        float distance = float.MaxValue;

        return clothingColors
            .OrderBy(c => ColorDistance(color, c.Value))
            .First()
            .Key;

        /*foreach (var clothingColor in clothingColors)
        {
            float temp = (clothingColor.Value.R - color.R) * (clothingColor.Value.R - color.R) +
                         (clothingColor.Value.G - color.G) * (clothingColor.Value.G - color.G) +
                         (clothingColor.Value.B - color.B) * (clothingColor.Value.B - color.B);

            if (temp < distance)
            {
                distance = temp;
                closest = clothingColor.Key;
            }
        }*/ /*

        return closest;
    }*/

    public static Color GetClosestColor(List<Color> colors, uint color)
    {
        return colors.OrderBy(c => ColorDistance(color, c.Value)).First();
    }

    private static double ColorDistance(uint color, uint argValue)
    {
        /*var rgbDistance = Math.Pow(color.R - argValue.R, 2) + Math.Pow(color.G - argValue.G, 2) +
                          Math.Pow(color.B - argValue.B, 2);*/

        var c1 = UIntToHsv(color);
        var c2 = UIntToHsv(argValue);
        var hsvDistance = Math.Pow(c1.H - c2.H, 2) + Math.Pow((c1.S - c2.S) * 100, 2) / 2 +
                          Math.Pow((c1.V - c2.V) * 100, 2);
        /*var colorDistance = rgbDistance + hsvDistance * 4;
        //Console.WriteLine("Color {0}, list {1}: {3}, distance {2}", c1, c2, hsvDistance, clothingColors.First(d => d.Value == argValue).Key);

        var c3 = new ColorSpaceConverter().ToCieLab(color);
        var c4 = new ColorSpaceConverter().ToCieLab(argValue);
        var lchDistance = Math.Pow(c3.L - c4.L, 2) + Math.Pow(c3.A - c4.A, 2) + Math.Pow(c3.B - c4.B, 2);*/
        return hsvDistance;
    }

    public static Hsv UIntToHsv(uint color)
    {
        byte r = (byte)((color & 0xFF0000) >> 16);
        byte g = (byte)((color & 0x00FF00) >> 8);
        byte b = (byte)(color & 0x0000FF);
        return ColorSpaceConverter.ToHsv(new Rgb24(r, g, b));
    }
}