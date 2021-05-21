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

            if (!args.Any())
            {

                return 0;
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

            string sourceFile = arguments.FilePathToReadFrom?.FullName ?? string.Empty;

            List<Point> points = GetPointsFromLog(sourceFile);
            points.Reverse();

            string title = arguments.ImageTitle ?? string.Empty;

            string colorChoice = arguments.ColorPalette ?? string.Empty;

            if (colorChoice.IsNumeric())
            {
                imageOptions.Colors = ColorManager.GetPreset(int.Parse(colorChoice));
            }
            else
            {
                using FileStream fs = new(colorChoice, FileMode.Open, FileAccess.Read);
                using StreamReader reader = new(fs);
                imageOptions.Colors = ColorManager.GetColors(reader);
            }

            ImageGenerator generator = new ImageGenerator(imageOptions, titleOptions);

            Image image = generator.GetImage(points, title);

            string outFilePath = string.Empty;
            if (!string.IsNullOrEmpty(arguments.OutputDirectory))
            {
                outFilePath = arguments.OutputDirectory;
            }
            
            string fileName = $"{DateTime.Now:yyyy_MM_dd_HHmmss}";
            if (!string.IsNullOrEmpty(title))
            {
                fileName = title;
            }
            
            image.SaveAsPng(Path.Combine(outFilePath, fileName)+".png");

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

                    Point[] currPts = matches.Where(x=>pointMap.ContainsKey(x.Value)).Select(x => pointMap[x.Value]).ToArray();

                    currPts.First().Parents.AddRange(currPts.Skip(1));
                    currPts.First().Parents.ForEach(x => x.Children.Add(currPts.First()));
                }
            }

            return pointMap.Values.ToList();
        }
    }
}
