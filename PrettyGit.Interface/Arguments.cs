using CommandLineParser.Arguments;
using SixLabors.ImageSharp.PixelFormats;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace PrettyGit.Interface
{
    internal class Arguments
    {
        [FileArgument('f', "filePath", Description = @"Path to the file containing repository log data. {git log --all --date-order --pretty=""%h|%p|"" > [file name here]}", FileMustExist = true, Optional = false)]
        public FileInfo FilePathToReadFrom;

        [ValueArgument(typeof(string), 't', "title", Description = "Title of the image")]
        public string ImageTitle;

        [ValueArgument(typeof(string), 'o', "outputDirectory", Description = "Directory to save graphic to")]
        public string OutputDirectory;

        [ValueArgument(typeof(string), 'c', "colorPalette", Description = "path to the color palette file to use")]
        public string ColorPalette;

        //language=regex
        [RegexValueArgument("titleColor", @"(\d+\,?){4}", Description = "Color of the title for the image in RGBA format", SampleValue = "70,70,70,255")]
        public string TitleColor;

        [EnumeratedValueArgument(typeof(TitleOptions.Location), 'p', "titlePosition", Description ="Location in the image to draw the title", DefaultValue = TitleOptions.Location.BottomRight, AllowedValues = "TopLeft;TopCenter;TopRight;RightEdge;BottomRight;BottomCenter;BottomLeft;LeftEdge")]
        public TitleOptions.Location TitleLocation;

        //language=regex
        [RegexValueArgument("backgroundColor", @"(\d+\,?){4}", Description = "Color for the background of the image in RGBA format", SampleValue = "70,70,70,255")]
        public string BackgroundColor;

        [ValueArgument(typeof(int), 'w', "initialWidth", Description = "Starting width of the image.", DefaultValue = 1920)]
        public int ImageWidth;

        [ValueArgument(typeof(int), 'h', "initialHeight", Description = "Starting height of the image.", DefaultValue = 1080)]
        public int ImageHeight;
        
        [SwitchArgument('R', "noResize", true, Description = "Specify to disable dynamic resizing of the image" )]
        public bool CanImageBeResized;

        [SwitchArgument('k', "keepAspectRatio", true, Description = "Specify to lock the output image to the same aspect ratio as defined by initial height and width")]
        public bool DoAxesScaleIndependently;

        [ValueArgument(typeof(int), "horizontalPadding", Description = "Minimum horizontal distance between points before image is scaled up on the X axis.", DefaultValue = 20)]
        public int HorizontalDistanceBetweenPoints;
        
        [ValueArgument(typeof(int), "verticalPadding", Description = "Minimum vertical distance to edge of image before image is scaled up on the Y axis.", DefaultValue = 200)]
        public int VerticalDistanceToImageEdge;

        [ValueArgument(typeof(int), "horizontalTitleOffset", Description = "Distance from the left or right edge of the image to the title.", DefaultValue = 50)]
        public int HorizontalOffset;

        [ValueArgument(typeof(int), "verticalTitleOffset", Description = "Distance from the top or bottom edge of the image to the title.", DefaultValue = 50)]
        public int VerticalOffset;

        internal static void ApplyCustomizations(Arguments arguments, ref ImageOptions image, ref TitleOptions title)
        {
            Regex colorExtraction = new Regex(@"(\d+)");

            if (arguments.ImageWidth != image.InitialWidth)
            {
                image.InitialWidth = arguments.ImageWidth;
            }

            if (arguments.ImageHeight != image.InitialHeight)
            {
                image.InitialHeight = arguments.ImageHeight;
            }

            if (!string.IsNullOrEmpty(arguments.BackgroundColor))
            {
                byte[] colorVals = colorExtraction.Matches(arguments.BackgroundColor).Select(x =>
                    byte.Parse(x.Value)
                ).ToArray();
                image.BackgroundColor = new Rgba32(colorVals[0], colorVals[1], colorVals[2], colorVals[3]);
            }

            if (arguments.HorizontalDistanceBetweenPoints != image.MinimumHorizontalClearance)
            {
                image.MinimumHorizontalClearance = arguments.HorizontalDistanceBetweenPoints;
            }

            if (arguments.VerticalDistanceToImageEdge != image.MinimumVerticalClearance)
            {
                image.MinimumVerticalClearance = arguments.VerticalDistanceToImageEdge;
            }

            if (arguments.CanImageBeResized != image.CanResize)
            {
                image.CanResize = arguments.CanImageBeResized;
            }

            if (arguments.DoAxesScaleIndependently != image.ScaleImageAxesIndepentently)
            {
                image.ScaleImageAxesIndepentently = arguments.DoAxesScaleIndependently;
            }

            if (!string.IsNullOrEmpty(arguments.TitleColor))
            {
                byte[] colorVals = colorExtraction.Matches(arguments.TitleColor).Select(x =>
                    byte.Parse(x.Value)
                ).ToArray();
                title.Color = new Rgba32(colorVals[0], colorVals[1], colorVals[2], colorVals[3]);
            }

            if (arguments.TitleLocation != title.Position)
            {
                title.Position = arguments.TitleLocation;
            }

            if (arguments.HorizontalOffset != title.XOffset)
            {
                title.XOffset = arguments.HorizontalOffset;
            }

            if (arguments.VerticalOffset != title.YOffset)
            {
                title.YOffset = arguments.VerticalOffset;
            }
        }
    }

}
