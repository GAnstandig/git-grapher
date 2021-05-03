using SixLabors.Fonts;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace PrettyGit.Interface
{
    public class Program
    {
        static void Main(string[] args)
        {
            string response = string.Empty;

            if (args.Length < 1 || string.IsNullOrEmpty(args[0]))
            {
                while (string.IsNullOrEmpty(response))
                {
                    Console.WriteLine(@"enter path for git log file file {git log --all --date-order --pretty=""%h|%p|""}: ");
                    response = Console.ReadLine();
                }
            }
            else
            {
                response = args[0];
            }

            List<Point> points = GetPointsFromLog(response);
            points.Reverse();

            Font font = new(SystemFonts.Find("consolas"), 55, FontStyle.BoldItalic);
            Console.Write(@"Enter a title for this work, or leave blank: ");
            string title = Console.ReadLine();

            Console.Write(@"Enter number for preset, or a path for a color file: ");
            string colorChoice = Console.ReadLine();
            if (string.IsNullOrEmpty(colorChoice))
            {
                colorChoice = "0";
            }
            ImageOptions imageOptions = new()
            {
                InitialWidth = 1920,
                InitialHeight = 1080,
                BackgroundColor = new Rgba32(30, 30, 30, 255),
                CanResize = true,
                MinimumHorizontalClearance = 20,
                MinimumVerticalClearance = 200,
                ScaleValuesOnResize = true,
                ScaleImageAxesIndepentently = true
            };

            if (colorChoice.IsNumeric())
            {
                imageOptions.Colors = ColorManager.GetPreset(int.Parse(colorChoice));
            }
            else
            {
                imageOptions.Colors = Path.GetExtension(colorChoice) switch
                {
                    ".xml" => ColorManager.GetColors(XmlManager.GetDocument(colorChoice)),
                    ".json" => ColorManager.GetColors(colorChoice)
                };
            }

            TitleOptions titleOptions = new TitleOptions(font)
            {
                Color = new Rgb24(170, 170, 170),
                Position = TitleOptions.Location.BottomRight,
                XOffset = 50,
                YOffset = 50
            };

            ImageGenerator generator = new ImageGenerator(imageOptions, titleOptions);

            Image image = generator.GetImage(points, title);

            if (string.IsNullOrEmpty(title))
            {
                image.SaveAsPng($"{DateTime.Now:yyyy_MM_dd_HHmmss}.png");
            }
            else
            {
                image.SaveAsPng($"{title}.png");
            }
        }

        private static List<Point> GetPointsFromLog(string logPath)
        {
            Regex hashExtraction = new("[a-f0-9]+(?=.*\\|)");

            Dictionary<string, Point> pointMap = new();

            using (FileStream fs = new(logPath, FileMode.Open, FileAccess.Read))
            {
                using StreamReader sr = new(fs);

                while (!sr.EndOfStream)
                {
                    //creating id-to-point map
                    string? line = sr.ReadLine();

                    MatchCollection matches = hashExtraction.Matches(line);

                    string key = matches.First().Value;
                    if (!pointMap.ContainsKey(key))
                    {
                        Point pt = new(key);
                        pointMap.Add(key, pt);
                    }
                }

                fs.Seek(0, SeekOrigin.Begin);
                sr.DiscardBufferedData();

                while (!sr.EndOfStream)
                {
                    //building inter-point relationships
                    string? line = sr.ReadLine();

                    MatchCollection matches = hashExtraction.Matches(line);

                    Point[] currPts = matches.Select(x => pointMap[x.Value]).ToArray();

                    currPts.First().Parents.AddRange(currPts.Skip(1));
                    currPts.First().Parents.ForEach(x => x.Children.Add(currPts.First()));
                }
            }

            return pointMap.Values.ToList();
        }
    }
}
