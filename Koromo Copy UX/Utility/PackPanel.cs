using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace Koromo_Copy_UX.Utility
{
    public class PackPanel : Panel
    {
        protected override Size MeasureOverride(Size availableSize)
        {
            foreach (UIElement child in InternalChildren)
            {
                child.Measure(availableSize);
            }
            
            var positions = new Point[InternalChildren.Count];
            var desiredHeight = ArrangeChildren(positions, availableSize.Width);

            return new Size(availableSize.Width, desiredHeight);
        }

        protected override Size ArrangeOverride(Size finalSize)
        {
            var positions = new Point[InternalChildren.Count];
            ArrangeChildren(positions, finalSize.Width);

            for (int i = 0; i < InternalChildren.Count; i++)
            {
                var child = InternalChildren[i];
                child.Arrange(new Rect(positions[i], child.DesiredSize));
            }

            return finalSize;
        }

        private double ArrangeChildren(Point[] positions, double availableWidth)
        {
            var lastRowStartIndex = -1;
            var lastRowEndIndex = 0;
            var currentWidth = 0d;
            var desiredHeight = 0d;

            for (int childIndex = 0; childIndex < InternalChildren.Count; childIndex++)
            {
                var child = InternalChildren[childIndex];
                var x = 0d;
                var y = 0d;

                if (currentWidth == 0d || currentWidth + child.DesiredSize.Width <= availableWidth)
                {
                    x = currentWidth;
                    currentWidth += child.DesiredSize.Width;
                }
                else
                {
                    currentWidth = child.DesiredSize.Width;
                    lastRowStartIndex = lastRowEndIndex;
                    lastRowEndIndex = childIndex;
                }

                if (lastRowStartIndex >= 0)
                {
                    int i = lastRowStartIndex;

                    while (i < lastRowEndIndex - 1 && positions[i + 1].X < x)
                    {
                        i++;
                    }

                    while (i < lastRowEndIndex && positions[i].X < x + child.DesiredSize.Width)
                    {
                        y = Math.Max(y, positions[i].Y + InternalChildren[i].DesiredSize.Height);
                        i++;
                    }
                }

                positions[childIndex] = new Point(x, y);
                desiredHeight = Math.Max(desiredHeight, y + child.DesiredSize.Height);
            }

            return desiredHeight;
        }
    }
}
