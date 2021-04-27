using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;

namespace PrettyGit
{
    public static class ColorManager
    {
        internal static List<List<Color>> Presets = new()
        {
            new List<Color>()
            {
                Color.DarkRed,
                Color.Red,
                Color.Yellow,
                Color.YellowGreen,
                Color.GreenYellow,
                Color.DarkGreen,
                Color.DarkCyan,
                Color.Cyan,
                Color.Blue,
                Color.DarkBlue,
                Color.BlueViolet,
                Color.Violet,
                Color.RebeccaPurple,
                Color.Purple,
                Color.MediumVioletRed,
            },

            new List<Color>()
            {
                new Color(new Rgb24(50, 50, 50)),
                new Color(new Rgb24(70, 70, 70)),
                new Color(new Rgb24(90, 90, 90)),
                new Color(new Rgb24(110, 110, 110)),
                new Color(new Rgb24(130, 130, 130)),
                new Color(new Rgb24(150, 150, 150)),
                new Color(new Rgb24(170, 170, 170)),
                new Color(new Rgb24(190, 190, 190))
            },

            new List<Color>()
            {
                new Rgb24(170,57,57),
                new Rgb24(255,170,170),
                new Rgb24(212,106,106),
                new Rgb24(128,21,21),
                new Rgb24(85,0,0),
                new Rgb24(170,108,57),
                new Rgb24(255,209,170),
                new Rgb24(212,154,106),
                new Rgb24(128,69,21),
                new Rgb24(85,39,0),
                new Rgb24(34,102,102),
                new Rgb24(102,153,153),
                new Rgb24(64,127,127),
                new Rgb24(13,77,77),
                new Rgb24(0,51,51),
                new Rgb24(45,136,45),
                new Rgb24(136,204,136),
                new Rgb24(85,170,85),
                new Rgb24(17,102,17),
                new Rgb24(0,68,0)
            },

            new List<Color>()
            {
                new Rgb24( 103,0,0),
                new Rgb24( 65,2,2),
                new Rgb24( 79,0,0),
                new Rgb24( 155,0,0),
                new Rgb24( 211,0,0),
                new Rgb24( 103,46,0),
                new Rgb24( 65,30,2),
                new Rgb24( 79,36,0),
                new Rgb24( 155,70,0),
                new Rgb24( 211,96,0),
                new Rgb24( 0,62,62),
                new Rgb24( 1,39,39),
                new Rgb24( 0,47,47),
                new Rgb24( 0,93,93),
                new Rgb24( 0,127,127),
                new Rgb24( 0,82,0),
                new Rgb24( 1,52,1),
                new Rgb24( 0,63,0),
                new Rgb24( 0,124,0),
                new Rgb24( 0,169,0)
            }
        };

        public static List<Color> GetColors(XDocument colorDocument)
        {
            List<Color> colors = new();
            foreach (XElement color in colorDocument.Root.Descendants("color"))
            {
                colors.Add(new Color(new Rgb24(color.Attribute("r").Value.ToByte(), color.Attribute("g").Value.ToByte(), color.Attribute("b").Value.ToByte())));
            }

            return colors;
        }

        public static List<Color> GetPreset(int idx)
        {
            return Presets.ElementAt(Math.Max(idx, 0) % Presets.Count);
        }
    }
}
