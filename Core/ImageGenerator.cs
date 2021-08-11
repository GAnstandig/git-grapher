using SixLabors.Fonts;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Drawing;
using SixLabors.ImageSharp.Drawing.Processing;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Core
{
    public class ImageGenerator
    {
        internal ImageOptions imageOptions;
        internal TitleOptions? titleOptions;

        private int widthMultiplier = 1;
        private int heightMultiplier = 1;

        private int titlePadding = 0;

        private float pointOffsetX = 0;

        private List<List<Node>> pointGroups = new();
        private List<Node> imagePoints = new();

        public ImageGenerator(ImageOptions imageOpts, TitleOptions? titleOpts)
        {
            imageOptions = imageOpts;
            titleOptions = titleOpts;
        }

        public Image GetImage(List<Node> points, string title = "", bool WriteIDs = false)
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

            DrawGraph(image, imagePoints, WriteIDs);

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

        private void SetColors(List<List<Node>> pointGroups)
        {
            Random rng = new();
            Dictionary<Color, int> occurrencesByColor = new();

            foreach (Color color in imageOptions.Colors)
            {
                occurrencesByColor.Add(color, 0);
            }

            foreach (List<Node> group in pointGroups)
            {
                List<Color> rarestColors = occurrencesByColor
                    .Where(x => x.Value == occurrencesByColor[occurrencesByColor.OrderByDescending(y => y.Value).Last().Key])
                    .Select(x => x.Key)
                    .ToList();

                Color nextColor = rarestColors.ElementAt(rng.Next(0, rarestColors.Count - 1));

                occurrencesByColor[nextColor]++;

                foreach (Node p in group)
                {
                    p.Color = nextColor;
                }
            }
        }

        /// <summary>
        /// Converts point offset data to point location data
        /// </summary>
        /// <param name="points">List of points to generate location data for</param>
        private void Plot(List<Node> points, bool accountForTitle)
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

            foreach (Node p in points)
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

        private void SetHorizontalOffsets(List<Node> points)
        {
            for (int i = 0; i < points.Count; i++)
            {
                Node current = points[i];

                if (current.xPosition == 0)
                {
                    current.xOffset = i + 1;
                }
            }
        }

        private void SetVerticalOffsets(List<Node> points)
        {
            Node originPoint = points.First();
            Node finalPoint = points.Last();

            List<Node> endpoints = points.Where(x => !x.Children.Any()).Except(new List<Node>() { points.Last() }).ToList();
            List<Node> unassignedPoints = new(points);
            List<Node> assignedPoints = new() { finalPoint };
            List<List<Node>> branches = new();

            while (unassignedPoints.Any())
            {
                Console.Write($"Grouped {points.Count - unassignedPoints.Count} out of {points.Count} points \t\t\r");

                List<Node> branch;

                if (Utilities.GetShortestPath(unassignedPoints.First(), assignedPoints, points) is List<Node> values)
                {
                    branch = values;
                }
                else if (unassignedPoints.First().Children.Any())
                {
                    branch = Utilities.GetShortestPath(unassignedPoints.First(), endpoints, points) ?? new List<Node>();
                }
                else
                {
                    branch = new List<Node>() { unassignedPoints.First() };
                }

                assignedPoints.AddRange(branch);
                unassignedPoints.RemoveRange(branch);
                branches.Add(branch);
            }

            List<Range> knownRanges = new();

            Console.WriteLine("Setting vertical offsets");

            foreach (List<Node> branch in branches)
            {
                List<Node> newBranch = new(branch.Where(x => x.yOffset < 0));

                pointGroups.Add(newBranch);

                int count = newBranch.Count();

                int endPosition;

                if (count >= branch.Count - 1)
                {
                    endPosition = branch.Last().xOffset;
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

                if (knownRanges.GetIntersectingRanges(branchRange) is List<Range> ranges && ranges.Any())
                {
                    int parentOffset;
                    if (branch.First().Parents.Any())
                    {
                        parentOffset = branch.First().Parents?.Min(x => x.yOffset) ?? 0;
                    }
                    else
                    {
                        parentOffset = 0;
                    }

                    int lowestPoint = Enumerable.Range(parentOffset, ranges.Max(x => x.YOffset)+5).First(x=> !ranges.Any(y=>y.YOffset == x));
                    yOffset = lowestPoint;
                }

                else
                {
                    yOffset = 0;
                }

                foreach (Node pt in newBranch)
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
            
            DrawingOptions drawOpts = new()
            {
                TextOptions = titleOptions
            };

            image.Mutate(x => x.DrawText(drawOpts, title, titleOptions.Font, titleOptions.Color, new PointF(xOffset, yOffset)));
        }

        private void DrawGraph(Image image, List<Node> points, bool drawIDs = false)
        {
            for (int i = 0; i < points.Count; i++)
            {
                Node point = points[i];

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
                            new PointF(offsetLocation.X + (2 * pointOffsetX), offsetLocation.Y),
                            new PointF(x.xPosition - (2 * pointOffsetX), x.yPosition),
                            x.Location));
                        }
                    }
                    else
                    {
                        image.Mutate(y => y.DrawBeziers(Pens.Solid(lineColor, 12),
                        point.Location,
                        new PointF(point.xPosition + (2 * pointOffsetX), point.yPosition),
                        new PointF(x.xPosition - (2 * pointOffsetX), x.yPosition),
                        x.Location));
                    }
                });

                image.Mutate(x => x.Fill(point.Color, new EllipsePolygon(point.Location, 20)));

                if (drawIDs)
                {
                    Font font = new(SystemFonts.Find("consolas"), 22, FontStyle.Italic);
                    image.Mutate(x => x
                        .SetDrawingTransform(Matrix3x2Extensions.CreateRotationDegrees(270, new PointF(point.xPosition, point.yPosition)))
                        .DrawText(
                            point.ID, 
                            font, 
                            new Rgb24(255, 0, 255), 
                            new PointF(point.xPosition, point.yPosition-10)));
                }
            }
        }
    }
}
