using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace Koromo_Copy_UX.Utility
{
    public class TableLayoutStrategy
    {
        private int _columnCount;
        private double[] _colWidths;
        private readonly List<double> _rowHeights = new List<double>();
        private int _elementCount;

        public Size ResultSize
        {
            get { return _colWidths != null && _rowHeights.Any() ? new Size(_colWidths.Sum(), _rowHeights.Sum()) : new Size(0, 0); }
        }

        public void Calculate(Size availableSize, Size[] measures)
        {
            BaseCalculation(availableSize, measures);
            AdjustEmptySpace(availableSize);
        }

        private void BaseCalculation(Size availableSize, Size[] measures)
        {
            _elementCount = measures.Length;
            _columnCount = GetColumnCount(availableSize, measures);
            if (_colWidths == null || _colWidths.Length < _columnCount)
                _colWidths = new double[_columnCount];
            var calculating = true;
            while (calculating)
            {
                calculating = false;
                ResetSizes();
                int row;
                for (row = 0; row * _columnCount < measures.Length; row++)
                {
                    var rowHeight = 0.0;
                    int col;
                    for (col = 0; col < _columnCount; col++)
                    {
                        int i = row * _columnCount + col;
                        if (i >= measures.Length) break;
                        _colWidths[col] = Math.Max(_colWidths[col], measures[i].Width);
                        rowHeight = Math.Max(rowHeight, measures[i].Height);
                    }

                    if (_columnCount > 1 && _colWidths.Sum() > availableSize.Width)
                    {
                        _columnCount--;
                        calculating = true;
                        break;
                    }
                    _rowHeights.Add(rowHeight);
                }
            }
        }

        public Rect GetPosition(int index)
        {
            var columnIndex = index % _columnCount;
            var rowIndex = index / _columnCount;
            var x = 0d;
            for (int i = 0; i < columnIndex; i++)
            {
                x += _colWidths[i];
            }
            var y = 0d;
            for (int i = 0; i < rowIndex; i++)
            {
                y += _rowHeights[i];
            }
            return new Rect(new Point(x, y), new Size(_colWidths[columnIndex], _rowHeights[rowIndex]));
        }

        public int GetIndex(Point position)
        {
            var col = 0;
            var x = 0d;
            while (x < position.X && _columnCount > col)
            {
                x += _colWidths[col];
                col++;
            }
            col--;
            var row = 0;
            var y = 0d;
            while (y < position.Y && _rowHeights.Count > row)
            {
                y += _rowHeights[row];
                row++;
            }
            row--;
            if (row < 0) row = 0;
            if (col < 0) col = 0;
            if (col >= _columnCount) col = _columnCount - 1;
            var result = row * _columnCount + col;
            if (result > _elementCount) result = _elementCount - 1;
            return result;
        }

        private void AdjustEmptySpace(Size availableSize)
        {
            var width = _colWidths.Sum();
            if (!double.IsNaN(availableSize.Width) && availableSize.Width > width)
            {
                var dif = (availableSize.Width - width) / _columnCount;

                for (var i = 0; i < _columnCount; i++)
                {
                    _colWidths[i] += dif;
                }
            }
        }

        private void ResetSizes()
        {
            _rowHeights.Clear();
            for (var j = 0; j < _colWidths.Length; j++)
            {
                _colWidths[j] = 0;
            }
        }

        private static int GetColumnCount(Size availableSize, Size[] measures)
        {
            double width = 0;
            for (int colCnt = 0; colCnt < measures.Length; colCnt++)
            {
                var nwidth = width + measures[colCnt].Width;
                if (nwidth > availableSize.Width)
                    return Math.Max(1, colCnt);
                width = nwidth;
            }
            return measures.Length;
        }
    }

    public class ArrangePanel : Panel
    {
        private UIElement _draggingObject;
        private Vector _delta;
        private Point _startPosition;
        private readonly TableLayoutStrategy _strategy = new TableLayoutStrategy();

        protected override void OnPreviewMouseLeftButtonDown(MouseButtonEventArgs e)
        {
            StartReordering(e);
        }

        protected override void OnMouseLeftButtonUp(MouseButtonEventArgs e)
        {
            StopReordering();
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            if (_draggingObject != null)
            {
                if (e.LeftButton == MouseButtonState.Released)
                    StopReordering();
                else
                    DoReordering(e);
            }
        }

        protected override void OnMouseLeave(MouseEventArgs e)
        {
            StopReordering();
            base.OnMouseLeave(e);
        }

        private void StartReordering(MouseEventArgs e)
        {
            _startPosition = e.GetPosition(this);
            _draggingObject = GetMyChildOfUiElement((UIElement)e.OriginalSource);
            _draggingObject.SetValue(ZIndexProperty, 100);
            var position = GetPosition(_draggingObject);
            _delta = position.TopLeft - _startPosition;
            _draggingObject.BeginAnimation(PositionProperty, null);
            SetPosition(_draggingObject, position);
        }

        private void DoReordering(MouseEventArgs e)
        {
            e.Handled = true;
            Point mousePosition = e.GetPosition(this);
            var index = _strategy.GetIndex(mousePosition);
            SetOrder(_draggingObject, index);
            var topLeft = mousePosition + _delta;
            var newPosition = new Rect(topLeft, GetPosition(_draggingObject).Size);
            SetPosition(_draggingObject, newPosition);
        }

        private void StopReordering()
        {
            if (_draggingObject == null) return;

            _draggingObject.ClearValue(ZIndexProperty);
            InvalidateMeasure();
            AnimateToPosition(_draggingObject, GetDesiredPosition(_draggingObject));
            _draggingObject = null;
        }

        private UIElement GetMyChildOfUiElement(UIElement e)
        {
            var obj = e;
            var parent = (UIElement)VisualTreeHelper.GetParent(obj);
            while (parent != null && parent != this)
            {
                obj = parent;
                parent = (UIElement)VisualTreeHelper.GetParent(obj);
            }
            return obj;
        }

        protected override Size MeasureOverride(Size availableSize)
        {
            InitializeEmptyOrder();
            if (_draggingObject != null)
            {
                ReorderOthers();
            }

            var measures = MeasureChildren();

            _strategy.Calculate(availableSize, measures);

            var index = -1;
            foreach (var child in Children.OfType<UIElement>().OrderBy(GetOrder))
            {
                index++;
                if (child == _draggingObject) continue;
                SetDesiredPosition(child, _strategy.GetPosition(index));
            }

            return _strategy.ResultSize;
        }

        protected override Size ArrangeOverride(Size finalSize)
        {
            foreach (var child in Children.OfType<UIElement>().OrderBy(GetOrder))
            {
                var position = GetPosition(child);
                if (double.IsNaN(position.Top))
                    position = GetDesiredPosition(child);
                child.Arrange(position);
            }
            return _strategy.ResultSize;
        }


        private Size[] MeasureChildren()
        {
            //if (_measures == null || Children.Count != _measures.Length)
            {
                _measures = new Size[Children.Count];

                var infinitSize = new Size(Double.PositiveInfinity, Double.PositiveInfinity);

                foreach (UIElement child in Children)
                {
                    child.Measure(infinitSize);
                }


                var i = 0;
                foreach (var measure in Children.OfType<UIElement>().OrderBy(GetOrder).Select(ch => ch.DesiredSize))
                {
                    _measures[i] = measure;
                    i++;
                }
            }
            return _measures;
        }

        private void ReorderOthers()
        {
            var s = GetOrder(_draggingObject);
            var i = 0;
            foreach (var child in Children.OfType<UIElement>().OrderBy(GetOrder))
            {
                if (i == s) i++;
                if (child == _draggingObject) continue;
                var current = GetOrder(child);
                if (i != current)
                {
                    SetOrder(child, i);
                }
                i++;
            }
        }

        private void InitializeEmptyOrder()
        {
            if (Children.Count > 0)
            {
                var next = Children.OfType<UIElement>().Max(ch => GetOrder(ch)) + 1;
                foreach (var child in Children.OfType<UIElement>().Where(child => GetOrder(child) == -1))
                {
                    SetOrder(child, next);
                    next++;
                }
            }
        }


        public static readonly DependencyProperty OrderProperty;
        public static readonly DependencyProperty PositionProperty;
        public static readonly DependencyProperty DesiredPositionProperty;
        private Size[] _measures;

        static ArrangePanel()
        {
            PositionProperty = DependencyProperty.RegisterAttached(
                "Position",
                typeof(Rect),
                typeof(ArrangePanel),
                new FrameworkPropertyMetadata(
                    new Rect(double.NaN, double.NaN, double.NaN, double.NaN),
                    FrameworkPropertyMetadataOptions.AffectsParentArrange));

            DesiredPositionProperty = DependencyProperty.RegisterAttached(
                "DesiredPosition",
                typeof(Rect),
                typeof(ArrangePanel),
                new FrameworkPropertyMetadata(
                    new Rect(double.NaN, double.NaN, double.NaN, double.NaN),
                    OnDesiredPositionChanged));

            OrderProperty = DependencyProperty.RegisterAttached(
                "Order",
                typeof(int),
                typeof(ArrangePanel),
                new FrameworkPropertyMetadata(
                    -1,
                    FrameworkPropertyMetadataOptions.AffectsParentMeasure));
        }

        private static void OnDesiredPositionChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var desiredPosition = (Rect)e.NewValue;
            AnimateToPosition(d, desiredPosition);
        }

        private static void AnimateToPosition(DependencyObject d, Rect desiredPosition)
        {
            var position = GetPosition(d);
            if (double.IsNaN(position.X))
            {
                SetPosition(d, desiredPosition);
                return;
            }

            var distance = Math.Max(
                (desiredPosition.TopLeft - position.TopLeft).Length,
                (desiredPosition.BottomRight - position.BottomRight).Length);

            var animationTime = TimeSpan.FromMilliseconds(distance * 2);
            var animation = new RectAnimation(position, desiredPosition, new Duration(animationTime));
            animation.DecelerationRatio = 1;
            ((UIElement)d).BeginAnimation(PositionProperty, animation);
        }

        public static int GetOrder(DependencyObject obj)
        {
            return (int)obj.GetValue(OrderProperty);
        }

        public static void SetOrder(DependencyObject obj, int value)
        {
            obj.SetValue(OrderProperty, value);
        }

        public static Rect GetPosition(DependencyObject obj)
        {
            return (Rect)obj.GetValue(PositionProperty);
        }

        public static void SetPosition(DependencyObject obj, Rect value)
        {
            obj.SetValue(PositionProperty, value);
        }

        public static Rect GetDesiredPosition(DependencyObject obj)
        {
            return (Rect)obj.GetValue(DesiredPositionProperty);
        }

        public static void SetDesiredPosition(DependencyObject obj, Rect value)
        {
            obj.SetValue(DesiredPositionProperty, value);
        }
    }
}
