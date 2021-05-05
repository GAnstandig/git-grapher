using SixLabors.Fonts;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Drawing;
using SixLabors.ImageSharp.Drawing.Processing;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using System;
using System.Collections.Generic;
using System.Linq;

namespace PrettyGit
{
    public class ImageGenerator
    {
        internal ImageOptions imageOptions;
        internal TitleOptions? titleOptions;

        private int widthMultiplier = 1;
        private int heightMultiplier = 1;

        private int titlePadding = 0;

        private float pointOffsetX = 0;

        private List<List<Point>> pointGroups = new();
        private List<Point> imagePoints = new();

        public ImageGenerator(ImageOptions imageOpts, TitleOptions? titleOpts)
        {
            imageOptions = imageOpts;
            titleOptions = titleOpts;
        }

        public Image GetImage(List<Point> points, string title = "")
        {
            imagePoints = points;
            pointOffsetX = imageOptions.InitialWidth / (imagePoints.Count * 2);

            SetHorizontalOffsets(imagePoints);
            SetVerticalOffsets(imagePoints);

            if (imageOptions.CanResize)
            {
                ScaleImage(imageOptions.ScaleImageAxesIndepentently, !string.IsNullOrEmpty(title));
            }

            Plot(imagePoints, !string.IsNullOrEmpty(title));

            SetColors(pointGroups);

            Image image = new Image<Rgba32>(
                    imageOptions.InitialWidth * widthMultiplier,
                    imageOptions.InitialHeight * heightMultiplier,
                    imageOptions.BackgroundColor
                );

            DrawGraph(image, imagePoints);

            try
            {
                DrawTitle(image, title);
            }
            catch (InvalidOperationException e)
            {
                Console.WriteLine($"Unable to add title to image - Title Options not specified.\n{e.StackTrace}");
            }

            return image;
        }

        private void SetColors(List<List<Point>> pointGroups)
        {
            Random rng = new();
            Dictionary<Color, int> occurrencesByColor = new();

            foreach (Color color in imageOptions.Colors)
            {
                occurrencesByColor.Add(color, 0);
            }

            foreach (List<Point> group in pointGroups)
            {
                List<Color> rarestColors = occurrencesByColor
                    .Where(x => x.Value == occurrencesByColor[occurrencesByColor.OrderByDescending(y => y.Value).Last().Key])
                    .Select(x => x.Key)
                    .ToList();

                Color nextColor = rarestColors.ElementAt(rng.Next(0, rarestColors.Count - 1));

                occurrencesByColor[nextColor]++;
                
                foreach (Point p in group)
                {
                    p.Color = nextColor;
                }
            }
        }

        /// <summary>
        /// Converts point offset data to point location data
        /// </summary>
        /// <param name="points">List of points to generate location data for</param>
        private void Plot(List<Point> points, bool accountForTitle)
        {
            int usableSpace;
            if (accountForTitle)
            {
                usableSpace = (imageOptions.InitialHeight * heightMultiplier) - titlePadding;
            }
            else
            {
                usableSpace = (imageOptions.InitialHeight * heightMultiplier);
            }

            int usedSpace = (points.Max(x => x.yOffset) * imageOptions.VerticalBranchSpacing);

            int emptySpace = usableSpace - usedSpace;

            int upperPadding = emptySpace / 2;

            int bottomLine = upperPadding + usedSpace;

            foreach (Point p in points)
            {
                //assign X axis value
                p.xPosition = p.xOffset / (float)points.Count * (imageOptions.InitialWidth * widthMultiplier) - pointOffsetX;

                //assign Y axis value
                p.yPosition = bottomLine - ((p.yOffset * imageOptions.VerticalBranchSpacing)); //subtract to move upward
            }
        }

        private void ScaleImage(bool scaleImageAxesIndepentently, bool accountForTitle)
        {
            //get the spacing between nodes
            if (pointOffsetX < imageOptions.MinimumHorizontalClearance)
            {
                widthMultiplier = (int)Math.Ceiling((imageOptions.MinimumHorizontalClearance * imagePoints.Count * 2) / (double)imageOptions.InitialWidth);
                pointOffsetX = (imageOptions.InitialWidth * widthMultiplier) / (imagePoints.Count * 2);
            }

            //get the height of highest branch
            int imageHeight = imageOptions.InitialHeight;
            if (titleOptions is not null && accountForTitle)
            {
                titlePadding = (int)(Math.Ceiling(titleOptions.Font.Size) + (titleOptions.YOffset * 1.5));
                imageHeight -= titlePadding;
            }

            int distanceToEdge = imageHeight - (imagePoints.Max(x => x.yOffset) * imageOptions.VerticalBranchSpacing);

            if (distanceToEdge < imageOptions.MinimumVerticalClearance)
            {
                heightMultiplier = (int)Math.Ceiling((imagePoints.Max(x => x.yOffset) * imageOptions.VerticalBranchSpacing + imageOptions.MinimumVerticalClearance) / (double)imageOptions.InitialHeight);
            }

            if (!scaleImageAxesIndepentently)
            {
                widthMultiplier = Math.Max(widthMultiplier, heightMultiplier);
                heightMultiplier = Math.Max(widthMultiplier, heightMultiplier);
            }
        }

        private void SetHorizontalOffsets(List<Point> points)
        {
            for (int i = 0; i < points.Count; i++)
            {
                Point current = points[i];

                if (current.xPosition == 0)
                {
                    current.xOffset = i + 1;
                }
            }
        }

        private void SetVerticalOffsets(List<Point> points)
        {
            Point originPoint = points.First();
            Point finalPoint = points.Last();

            List<Point> unassignedPoints = new(points);

            List<List<Point>> branches = new();

            while (unassignedPoints.Any())
            {
                List<Point> branch;

                if (Utilities.GetShortestPath(unassignedPoints.First(), finalPoint) is List<Point> values)
                {
                    branch = values;
                }
                else
                {
                    branch = new List<Point>() { unassignedPoints.First() };
                    unassignedPoints.RemoveAt(0);
                }

                unassignedPoints.RemoveRange(branch);
                branches.Add(branch);

                Console.Write($"Plotted {points.Count - unassignedPoints.Count} out of {points.Count} points \t\t\r");
            }

            List<Range> knownRanges = new();

            foreach (List<Point> branch in branches)
            {
                List<Point> newBranch = new(branch.Where(x => x.yOffset < 0));

                pointGroups.Add(newBranch);

                int count = newBranch.Count();

                int endPosition;

                if (count >= branch.Count - 1)
                {
                    endPosition = newBranch.Last().xOffset;
                }
                else
                {
                    endPosition = branch[count].xOffset;
                }

                Range branchRange;

                endPosition -= 1;

                if (newBranch.First().Parents.Any())
                {
                    branchRange = new(newBranch.First().Parents.OrderBy(x => x.xOffset).Last().xOffset, endPosition, 0);
                }
                else
                {
                    branchRange = new(newBranch.First().xOffset, endPosition, 0);
                }

                int yOffset;

                if (knownRanges.GetBaseRange(branchRange) is Range range)
                {
                    yOffset = range.YOffset + 1;
                }
                else
                {
                    yOffset = 0;
                }

                foreach (Point pt in newBranch)
                {
                    pt.yOffset = yOffset;
                }

                branchRange.YOffset = yOffset;

                knownRanges.Add(branchRange);
            }
        }

        private void DrawTitle(Image image, string title)
        {
            if (titleOptions is null)
            {
                throw new InvalidOperationException("Cannot create a title when Title Options is null!");
            }

            float xOffset;
            float yOffset;

            int scaledHeight = imageOptions.InitialHeight * heightMultiplier;
            int scaledWidth = imageOptions.InitialWidth * widthMultiplier;

            switch (titleOptions.Position)
            {
                case TitleOptions.Location.TopLeft:
                    xOffset = 0 + titleOptions.XOffset;
                    yOffset = 0 + titleOptions.YOffset;
                    titleOptions.HorizontalAlignment = HorizontalAlignment.Left;
                    titleOptions.VerticalAlignment = VerticalAlignment.Top;
                    break;
                case TitleOptions.Location.TopCenter:
                    xOffset = scaledWidth / 2;
                    yOffset = 0 + titleOptions.YOffset;
                    titleOptions.HorizontalAlignment = HorizontalAlignment.Center;
                    titleOptions.VerticalAlignment = VerticalAlignment.Top;
                    break;
                case TitleOptions.Location.TopRight:
                    xOffset = scaledWidth - titleOptions.XOffset;
                    yOffset = 0 + titleOptions.YOffset;
                    titleOptions.HorizontalAlignment = HorizontalAlignment.Right;
                    titleOptions.VerticalAlignment = VerticalAlignment.Top;
                    break;
                case TitleOptions.Location.RightEdge:
                    xOffset = scaledWidth - titleOptions.XOffset;
                    yOffset = scaledHeight / 2;
                    titleOptions.HorizontalAlignment = HorizontalAlignment.Right;
                    titleOptions.VerticalAlignment = VerticalAlignment.Center;
                    break;
                case TitleOptions.Location.BottomRight:
                    xOffset = scaledWidth - titleOptions.XOffset;
                    yOffset = scaledHeight - titleOptions.YOffset;
                    titleOptions.HorizontalAlignment = HorizontalAlignment.Right;
                    titleOptions.VerticalAlignment = VerticalAlignment.Bottom;
                    break;
                case TitleOptions.Location.BottomCenter:
                    xOffset = scaledWidth / 2;
                    yOffset = scaledHeight - titleOptions.YOffset;
                    titleOptions.HorizontalAlignment = HorizontalAlignment.Center;
                    titleOptions.VerticalAlignment = VerticalAlignment.Bottom;
                    break;
                case TitleOptions.Location.BottomLeft:
                    xOffset = 0 + titleOptions.XOffset;
                    yOffset = scaledHeight - titleOptions.YOffset;
                    titleOptions.HorizontalAlignment = HorizontalAlignment.Left;
                    titleOptions.VerticalAlignment = VerticalAlignment.Bottom;
                    break;
                case TitleOptions.Location.LeftEdge:
                    xOffset = 0 + titleOptions.XOffset;
                    yOffset = scaledHeight / 2;
                    titleOptions.HorizontalAlignment = HorizontalAlignment.Left;
                    titleOptions.VerticalAlignment = VerticalAlignment.Center;
                    break;
                default:
                    throw new InvalidOperationException("Could not recognize title location selection!");
            }

            TextGraphicsOptions tgo = new()
            {
                TextOptions = titleOptions
            };

            image.Mutate(x => x.DrawText(tgo, title, titleOptions.Font, titleOptions.Color, new PointF(xOffset, yOffset)));
        }

        private void DrawGraph(Image image, List<Point> points)
        {
            for (int i = 0; i < points.Count; i++)
            {
                Point point = points[i];

                Color lineColor;

                point.Children.OrderBy(x => x.yPosition).ToList().ForEach(x =>
                {
                    if (point.yPosition > x.yPosition)
                    {
                        lineColor = x.Color;
                    }
                    else
                    {
                        lineColor = point.Color;
                    }

                    if (Math.Sqrt(Math.Pow(point.xPosition - x.xPosition, 2)) > pointOffsetX * 2 + 1 && point.yPosition != x.yPosition)
                    {
                        if (x.yPosition <= point.yPosition)
                        {
                            //uphill
                            PointF horizontalStart = new(point.xPosition + (2 * pointOffsetX), x.yPosition);

                            image.Mutate(y => y.DrawLines(Pens.Solid(lineColor, 12), horizontalStart, x.Location));

                            image.Mutate(y => y.DrawBeziers(Pens.Solid(lineColor, 12),
                                point.Location,
                                new PointF(horizontalStart.X, point.yPosition),
                                new PointF(point.xPosition, horizontalStart.Y),
                                horizontalStart
                                ));
                        }
                        else
                        {
                            //downhill

                            PointF offsetLocation = new(x.xPosition - 2 * pointOffsetX, point.yPosition);

                            image.Mutate(y => y.DrawLines(Pens.Solid(lineColor, 12), point.Location, offsetLocation));

                            image.Mutate(y => y.DrawBeziers(Pens.Solid(lineColor, 12),
                            offsetLocation,
                            new PointF(offsetLocation.X + pointOffsetX, offsetLocation.Y),
                            new PointF(x.xPosition - pointOffsetX, x.yPosition),
                            x.Location));
                        }
                    }
                    else
                    {
                        image.Mutate(y => y.DrawBeziers(Pens.Solid(lineColor, 12),
                        point.Location,
                        new PointF(point.xPosition + pointOffsetX, point.yPosition),
                        new PointF(x.xPosition - pointOffsetX, x.yPosition),
                        x.Location));
                    }
                });

                image.Mutate(x => x.Fill(point.Color, new EllipsePolygon(point.Location, 20)));
            }
        }
    }
}
