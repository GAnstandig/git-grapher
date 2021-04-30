using SixLabors.Fonts;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Drawing.Processing;

namespace PrettyGit
{
    public class TitleOptions : TextOptions
    {
        public Font Font { get; private set; }
        public Color Color { get; set; }

        public Location Position { get; set; }
        public float XOffset { get; set; }
        public float YOffset { get; set; }

        public TitleOptions(Font font) : base()
        {
            Font = font;
        }

        public enum Location
        {
            TopLeft,
            TopCenter,
            TopRight,

            RightEdge,

            BottomRight,
            BottomCenter,
            BottomLeft,

            LeftEdge
        }
    }
}
