using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using System.Collections.Generic;

namespace Core
{
    /// <summary>
    /// Options for image generation
    /// </summary>
    public class ImageOptions
    {
        /// <summary>
        /// Initial width of image
        /// </summary>
        public int InitialWidth { get; set; }
        
        /// <summary>
        /// Initial height of image
        /// </summary>
        public int InitialHeight { get; set; }
        
        /// <summary>
        /// Background color for generated image
        /// </summary>
        public Rgba32 BackgroundColor { get; set; } = new Rgba32(30, 30, 30, 255);
        
        /// <summary>
        /// Collection of colors to use in image
        /// </summary>
        public List<Color> Colors { get; set; } = new List<Color>();
        
        /// <summary>
        /// Vertical spacing (in pixels) between branches. Defaults to 100 px
        /// </summary>
        public int VerticalBranchSpacing { get; set; } = 100;

        /// <summary>
        /// Whether the image can be dynamically resized. Defaults to true
        /// </summary>
        public bool CanResize { get; set; } = true;
        
        /// <summary>
        /// Minimum horizontal distance between points before image is scaled up along the X axis. Ignored if <see cref="CanResize"/> is false
        /// </summary>
        public int MinimumHorizontalClearance { get; set; }
        
        /// <summary>
        /// Minimum vertical distance between points and the upper/lower edges of the image before image is scaled up along the y axis. Ignored if <see cref="CanResize"/> is false
        /// </summary>
        public int MinimumVerticalClearance { get; set; }
        
        /// <summary>
        /// Sets whether image processor can scale values such as title font size along with the image itself. Defaults to true
        /// </summary>
        public bool ScaleValuesOnResize { get; set; } = true;
        
        /// <summary>
        /// Sets whether axis scales are coupled or indepentent. Defaults to true
        /// </summary>
        public bool ScaleImageAxesIndepentently { get; set; } = true;
    }
}
