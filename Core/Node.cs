
using SixLabors.ImageSharp;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace Core
{
    public class Node : IEquatable<Node>
    {
        /// <summary>
        /// ID of the point
        /// </summary>
        public string ID { get; set; }

        /// <summary>
        /// Location in space where the commit appears on the graph
        /// </summary>
        public PointF Location { get { return new(xPosition, yPosition); } }

        public float xPosition { get; set; }
        public float yPosition { get; set; }

        public int xOffset { get; set; } = -1;
        public int yOffset { get; set; } = -1;

        public Color Color { get; set; }

        /// <summary>
        /// List of point objects that this object is a child of
        /// </summary>
        public List<Node> Parents { get; } = new();
        public List<Node> Children { get; } = new();

        public Node(string id)
        {
            ID = id;
        }

        public override int GetHashCode()
        {
            return ID.GetHashCode();
        }

        public override string ToString()
        {
            return ID;
        }

        public List<Node> GetDescendants()
        {
            List<Node> descendants = new(Children);

            foreach (var child in Children)
            {
                descendants.AddRange(child.GetDescendants());
            }

            return descendants;
        }

        public bool Equals([AllowNull] Node other)
        {
            if (other is null)
            {
                return false;
            }

            return ID.Equals(other.ID);
        }
    }
}
