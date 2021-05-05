using CommandLineParser.Exceptions;

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
        static int Main(string[] args)
        {
            CommandLineParser.CommandLineParser parser = new()
            {
                IgnoreCase = true,
                AcceptEqualSignSyntaxForValueArguments = true,
                AcceptHyphen = true,
                ShowUsageOnEmptyCommandline = true,
            };

            Arguments arguments = new();

            try
            {
                parser.ExtractArgumentAttributes(arguments);
                parser.ParseCommandLine(args);
            }
            catch (CommandLineArgumentException e)
            {
                Console.WriteLine($"Error parsing argument \"{e.Argument}\": {e.Message}");
                return -1;
            }
            

            Font font = new(SystemFonts.Find("consolas"), 55, FontStyle.BoldItalic);

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

            TitleOptions titleOptions = new(font)
            {
                Color = new Rgb24(170, 170, 170),
                Position = TitleOptions.Location.BottomRight,
                XOffset = 50,
                YOffset = 50
            };

            Arguments.ApplyCustomizations(arguments, ref imageOptions, ref titleOptions);

            string response = arguments.FilePathToReadFrom?.FullName ?? string.Empty;

            while (string.IsNullOrEmpty(response))
            {
                Console.WriteLine(@"enter path for git log file file {git log --all --date-order --pretty=""%h|%p|""}: ");
                response = Console.ReadLine();
            }

            List<Point> points = GetPointsFromLog(response);
            points.Reverse();

            string title = arguments.ImageTitle ?? string.Empty;

            while (string.IsNullOrEmpty(title))
            {
                Console.Write(@"Enter a title for this work, or leave blank: ");
                title = Console.ReadLine();
            }

            string colorChoice = arguments.ColorPalette ?? string.Empty;

            if (string.IsNullOrEmpty(colorChoice))
            {
                Console.Write(@"Enter number for preset, or a path for a color file: ");
                colorChoice = Console.ReadLine();
                if (string.IsNullOrEmpty(colorChoice))
                {
                    colorChoice = "0";
                }
            }

            if (colorChoice.IsNumeric())
            {
                imageOptions.Colors = ColorManager.GetPreset(int.Parse(colorChoice));
            }
            else
            {
                imageOptions.Colors = Path.GetExtension(colorChoice) switch
                {
                    ".xml" => ColorManager.GetColors(XmlManager.GetDocument(colorChoice)),
                    ".json" => ColorManager.GetColors(colorChoice),
                    _ => throw new NotImplementedException()
                };
            }

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

            return 0;
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
