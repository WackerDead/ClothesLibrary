using System.Drawing;
using Backend.Models;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.ColorSpaces;
using SixLabors.ImageSharp.ColorSpaces.Conversion;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using SixLabors.ImageSharp.Processing.Processors.Quantization;
using Color = SixLabors.ImageSharp.Color;
using Size = SixLabors.ImageSharp.Size;

namespace Backend;

public class ColorExtractor
{
    public static void ExtractColor(Image<Rgba32> image)
    {
        ExtractPalette(image, 1);
    }

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

    public static int maxColors = 256000;
    private static int distance = 8;

    public static void ExtractPaletteHue(Image<Rgba32> image, int paletteSize)
    {
        HueColor[] hues = new HueColor[256];

        image.Mutate(x => x
            .Resize(new ResizeOptions { Sampler = KnownResamplers.NearestNeighbor, Size = new Size(100, 0) })
            .Quantize(new OctreeQuantizer(new QuantizerOptions() { MaxColors = maxColors })));

        for (int i = 0; i < image.Width; i++)
        {
            for (int j = 0; j < image.Height; j++)
            {
                var pixel = image[i, j];
                if (pixel.A == 0) continue;

                var hsv = ColorSpaceConverter.ToHsv(pixel);
                int hue = (int)Math.Round(hsv.H) * hues.Length / 360;

                HueColor hc;
                if ((hc = hues[hue]) == null)
                {
                    hc = new HueColor(hue);
                }

                hc.count++;

                int saturation = (int)(hsv.S * (hc.saturations.Length - 1));
                hc.saturations[saturation]++;

                int value = (int)(hsv.V * (hc.values.Length - 1));
                hc.values[value]++;
                hues[hue] = hc;
            }
        }

        List<int> pal = FindPeaks(hues.Select(h => h == null ? 0 : h.count).ToList(), paletteSize);
        foreach (var i in pal)
        {
            Console.WriteLine("Hue {0}, Saturation {1}, Value {2}", i * 360 / 256,
                hues[i].saturations.Index().First(s => s.Item == hues[i].saturations.Max()).Index * 100 / 39,
                hues[i].values.Index().First(s => s.Item == hues[i].values.Max()).Index * 100 / 39);
        }
    }

    public static List<ProductColor> ExtractProductColors(Image<Rgba32> image)
    {
        var colors = ExtractPaletteHueCluster(image, 1000);
        return colors.Select(c => new ProductColor(c.Rgba)).ToList();
    }
    
    public static List<Rgba32> ExtractPaletteHueCluster(Image<Rgba32> image, int paletteSize)
    {
        HueColor[] hues = new HueColor[256];
        int[] hueClusters = new int[256];

        image.Mutate(x => x
                .Resize(new ResizeOptions { Sampler = KnownResamplers.NearestNeighbor, Size = new Size(200, 0) })
            /*.Quantize(new OctreeQuantizer(new QuantizerOptions() { MaxColors = maxColors }))*/);
        image.Save("./clothing_resized.png");

        for (int i = 0; i < image.Width; i++)
        {
            for (int j = 0; j < image.Height; j++)
            {
                var pixel = image[i, j];
                if (pixel.A == 0) continue;

                var hsv = ColorSpaceConverter.ToHsv(pixel);
                int hue = (int)Math.Round(hsv.H) * hues.Length / 360;

                HueColor hc;
                if ((hc = hues[hue]) == null)
                {
                    hc = new HueColor(hue);
                }

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

        List<int> huePalette = ClusterExtractor(hueClusters.ToList(), paletteSize);
        List<Rgba32> palette = new();
        
        foreach (var hue in huePalette)
        {
            int index = hue - 1;
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

            //Console.Write(Color);
            //Console.WriteLine(hue + " - " + index);

            //TODO: When saturation is 0, the value is the only one that matters, so make it so if saturation is 0 and value is the same, then its the same color
            int h = hue * 360 / 256;
            float s = (float)color.saturations.Index().First(s => s.Item == color.saturations.Max()).Index / 39;
            float v = (float)color.values.Index().First(s => s.Item == color.values.Max()).Index / 39;
            //Console.WriteLine("Hue {0}, Saturation {1}, Value {2}, Count {3}", h, s*100, v*100, color.count);
            var c = ColorSpaceConverter.ToRgb(new Hsv(h, s, v));
            var rgba32 = new Rgba32(c.R, c.G, c.B);
            Console.WriteLine("{0} - {1} : {2}", rgba32.ToHex(), GetClosestColor(c),
                hueClusters[hue]);
            palette.Add(rgba32);
        }

        return palette;
    }

    public static void ExtractPaletteColors(Image<Rgba32> image, int paletteSize)
    {
        List<(Color, int)> colors = new List<(Color, int)>();

        image.Mutate(x => x
            .Resize(new ResizeOptions { Sampler = KnownResamplers.NearestNeighbor, Size = new Size(100, 0) })
            .Quantize(new OctreeQuantizer(new QuantizerOptions() { MaxColors = maxColors })));

        for (int i = 0; i < image.Width; i++)
        {
            for (int j = 0; j < image.Height; j++)
            {
                var pixel = image[i, j];
                if (pixel.A == 0) continue;

                int index;
                if ((index = colors.FindIndex(c => c.Item1 == (Color)pixel)) != -1)
                {
                    var temp = colors[index];
                    temp.Item2++;
                    colors[index] = temp;
                }
                else colors.Add((pixel, 1));
            }
        }

        List<(Color, int)> peaks = new();

        for (int i = 1; i < colors.Count - 1; i++)
        {
            if (colors[i].Item2 > colors[i - 1].Item2 && colors[i].Item2 > colors[i + 1].Item2)
            {
                var color = ColorSpaceConverter.ToHsv(colors[i].Item1.ToPixel<Rgba32>());

                if (peaks.Any(p =>
                        Math.Abs(ColorSpaceConverter.ToHsv(p.Item1.ToPixel<Rgba32>()).H - color.H) < distance))
                    continue;
                peaks.Add(colors[i]);
            }
        }

        List<Color> palette = peaks.OrderByDescending(p => p.Item2).Select(p => p.Item1).ToList()
            .GetRange(0, Math.Min(paletteSize, peaks.Count));

        foreach (var i in palette)
        {
            var color = ColorSpaceConverter.ToHsv(i.ToPixel<Rgba32>());
            Console.WriteLine("Hue {0}, Saturation {1}, Value {2}", (int)color.H, (int)(color.S * 100), (int)(color
                .V * 100));
        }
    }

    public static void ExtractPalette(Image<Rgba32> image, int paletteSize)
    {
        HueColor[] hues = new HueColor[256];
        int[] hueClusters = new int[256];

        List<(Color, int)> colors = new List<(Color, int)>();


        image.Mutate(x => x
            .Resize(new ResizeOptions { Sampler = KnownResamplers.NearestNeighbor, Size = new Size(100, 0) })
            .Quantize(new OctreeQuantizer(new QuantizerOptions() { MaxColors = 1000 })));
        for (int i = 0; i < image.Width; i++)
        {
            for (int j = 0; j < image.Height; j++)
            {
                var pixel = image[i, j];
                if (pixel.A == 0) continue;

                int index = -1;
                if ((index = colors.FindIndex(c => c.Item1 == (Color)pixel)) != -1)
                {
                    var temp = colors[index];
                    temp.Item2++;
                    colors[index] = temp;
                }
                else colors.Add((pixel, 1));

                continue;

                var hsv = ColorSpaceConverter.ToHsv(pixel);
                int hue = ((int)Math.Round(hsv.H)) * hues.Length / 360;
                int saturation = (int)(hsv.S * 39);
                //Console.WriteLine(hsv.S);
                int value = (int)(hsv.V * 39);


                var hc = hues[hue];
                if (hc == null)
                {
                    hc = new HueColor(hue);
                }

                hc.count++;
                hc.saturations[saturation]++;
                hc.values[value]++;
                hues[hue] = hc;
                //Console.WriteLine("Hue {0}, Score {1}.", hue, hues[hue].count);

                /*hues[(int)Math.Round(hsv.H)].hue++;
                hues[(int)Math.Round(hsv.H)].saturations[saturation]++;;
                hues[(int)Math.Round(hsv.H)].values[value]++;*/
            }
        }

        /*colors = colors.OrderByDescending(dic => dic.Value).ToDictionary(dic => dic.Key, dic => dic.Value);

        for (int i = 0; i < paletteSize; i++)
        {
            Console.WriteLine("#{2} Color {0}, Score {1}.", colors.ElementAt(i).Key, colors.ElementAt(i).Value, i);
        }*/

        List<(Color, int)> peaks = new();

        for (int i = 1; i < colors.Count - 1; i++)
        {
            if (colors[i].Item2 > colors[i - 1].Item2 && colors[i].Item2 > colors[i + 1].Item2)
            {
                var color = ColorSpaceConverter.ToHsv(colors[i].Item1.ToPixel<Rgba32>());

                if (peaks.Any(p =>
                        Math.Abs(ColorSpaceConverter.ToHsv(p.Item1.ToPixel<Rgba32>()).H - color.H) < distance))
                    continue;
                peaks.Add(colors[i]);
            }
        }

        List<Color> pala = peaks.OrderByDescending(p => p.Item2).Select(p => p.Item1).ToList()
            .GetRange(0, Math.Min(paletteSize, peaks.Count));

        foreach (var i in pala)
        {
            var color = ColorSpaceConverter.ToHsv(i.ToPixel<Rgba32>());
            Console.WriteLine("Peak Hue {0}, Saturation {1}, Value {2}", (int)color.H, (int)(color.S * 100), (int)(color
                .V * 100));
        }

        //return;

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

        for (int i = 0; i < hues.Length; i++)
        {
            Console.WriteLine("Hue {0}, Score {1}.", i, hueClusters[i]);
        }

        Console.WriteLine("Max hue {0}", hueClusters.Index().First(tuple => tuple.Item == hueClusters.Max()));

        List<int> pal = FindPeaks(hues.Select(h => h == null ? 0 : h.count).ToList(), paletteSize);
        foreach (var i in pal)
        {
            Console.WriteLine("Peak Hue {0}, Saturation {1}, Value {2}", i * 360 / 256,
                hues[i].saturations.Index().First(s => s.Item == hues[i].saturations.Max()).Index * 100 / 39,
                hues[i].values.Index().First(s => s.Item == hues[i].values.Max()).Index * 100 / 39);
        }

        List<int> huePalette = ClusterExtractor(hueClusters.ToList(), paletteSize);

        foreach (var hue in huePalette)
        {
            int index = hue - 1;
            HueColor color;
            do
            {
                color = hues[++index];
            } while (color == null);

            //Console.Write(Color);
            Console.WriteLine(hue + " - " + index);
            Console.WriteLine("Hue {0}, Saturation {1}, Value {2}", hue * 360 / 256,
                color.saturations.Index().First(s => s.Item == color.saturations.Max()).Index * 100 / 39,
                color.values.Index().First(s => s.Item == color.values.Max()).Index * 100 / 39);
        }
    }

    private static List<int> ClusterExtractor(List<int> hueClusters, int clusters)
    {
        Dictionary<int, int> palette = new();
        int maxValue = 0;

        for (int i = 0; i < clusters; i++)
        {
            int hue = hueClusters.IndexOf(hueClusters.Max());
            if (hue == 0) break;
            int clusterStart = ((hue - 4) % hueClusters.Count + hueClusters.Count) % hueClusters.Count;
            int value = 0;
            for (int j = clusterStart; j < clusterStart + 9; j++)
            {
                value += hueClusters[j % hueClusters.Count];
                hueClusters[j % hueClusters.Count] = 0;
            }

            /*for (int j = 0; j < hueClusters.Count; j++)
            {
                Console.WriteLine("Hue {0}, Score {1}.", j, hueClusters[j]);
            }*/

            int minDistance = hueClusters.Count;
            foreach (var (key, _) in palette)
            {
                int distance = Math.Abs(hue - key);
                if (distance < minDistance)
                {
                    minDistance = distance;
                }
            }

            if (minDistance < distance)
            {
                i--;
                continue;
            }

            if (value > maxValue)
            {
                maxValue = value;
            }
            if (value < maxValue / 4)
            {
                break;
            }
            palette.Add(hue, value);
        }

        //palette = palette.OrderByDescending(dic => dic.Value).ToDictionary(dic => dic.Key, dic => dic.Value);
        /*foreach (var (hue, value) in palette)
        {
            Console.WriteLine("Hue {0}, Score {1}.", hue, value);
        }*/

        return palette.OrderByDescending(dic => dic.Value).Select(dic => dic.Key).ToList();
    }

    public static List<int> FindPeaks(List<int> list, int count)
    {
        // List<(hue,countValue)>
        List<(int, int)> peaks = new();

        for (int i = 1; i < list.Count - 1; i++)
        {
            if (list[i] > list[i - 1] && list[i] > list[i + 1])
            {
                if (peaks.Any(p => Math.Abs(p.Item1 - i) < distance)) continue;
                peaks.Add((i, list[i]));
            }
        }
        
        return peaks.OrderByDescending(p => p.Item2).Select(p => p.Item1).ToList()
            .GetRange(0, Math.Min(count, peaks.Count));
        }

    private static Dictionary<string, Rgb24> clothingColors = new Dictionary<string, Rgb24>
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
        { "Brown", new Rgb24(165, 42, 42) },
        { "DarkGray", new Rgb24(80, 80, 80) },
        { "LightGray", new Rgb24(200, 200, 200) },
    };

    public static string GetClosestColor(Rgb24 color)
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
        }*/

        return closest;
    }

    private static double ColorDistance(Rgb24 color, Rgb24 argValue)
    {
        var rgbDistance = Math.Pow(color.R - argValue.R, 2) + Math.Pow(color.G - argValue.G, 2) + Math.Pow(color.B - argValue.B, 2);
        
        var c1 = ColorSpaceConverter.ToHsv(color);
        var c2 = ColorSpaceConverter.ToHsv(argValue);
        var hsvDistance = Math.Pow(c1.H - c2.H, 2) + Math.Pow((c1.S - c2.S)*100, 2)/2 + Math.Pow((c1.V - c2.V)*100, 2);
        var colorDistance = rgbDistance + hsvDistance * 4;
        //Console.WriteLine("Color {0}, list {1}: {3}, distance {2}", c1, c2, hsvDistance, clothingColors.First(d => d.Value == argValue).Key);
        
        var c3 = new ColorSpaceConverter().ToCieLab(color);
        var c4 = new ColorSpaceConverter().ToCieLab(argValue);
        var lchDistance = Math.Pow(c3.L - c4.L, 2) + Math.Pow(c3.A - c4.A, 2) + Math.Pow(c3.B - c4.B, 2);
        return hsvDistance;
    }
}