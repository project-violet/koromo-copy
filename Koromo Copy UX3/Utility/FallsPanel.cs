/***

   Copyright (C) 2018. dc-koromo. All Rights Reserved.

   Author: Koromo Copy Developer

***/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace Koromo_Copy_UX3.Utility
{
    /// <summary>
    /// 이 판넬은 높이가 다른 하위 컨트롤들 사이의 빈공간을 채워 정렬하는 판넬입니다.
    /// 모든 하위 컨트롤의 너비는 같다고 가정하고 측정, 정렬합니다.
    /// </summary>
    public class FallsPanel : Panel
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
            var width_length = 0;
            var current_width = 0d;
            var desired_height = 0d;
            var lock_width = false;

            for (int i = 0; i < InternalChildren.Count; i++)
            {
                var child = InternalChildren[i];
                var x = current_width;
                var y = 0d;
                
                if (lock_width == false)
                {
                    if (current_width == 0d || current_width + child.DesiredSize.Width <= availableWidth)
                    {
                        width_length += 1;
                        current_width += child.DesiredSize.Width;
                    }
                    else
                    {
                        lock_width = true;
                    }
                }
                
                if (i >= width_length)
                {
                    x = positions[i - width_length].X;
                    y = positions[i - width_length].Y + InternalChildren[i - width_length].DesiredSize.Height;
                }

                desired_height = Math.Max(desired_height, child.DesiredSize.Height + y);
                positions[i] = new Point(x, y);
            }

            return desired_height;
        }
    }
}
