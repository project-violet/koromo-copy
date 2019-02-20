using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Koromo_Copy_UX.Utility
{
    // https://stackoverflow.com/questions/5395957/wpf-slider-with-two-thumbs

    /// <summary>
    /// DoubleThumbSlider.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class DoubleThumbSlider : UserControl
    {
        public DoubleThumbSlider()
        {
            InitializeComponent();
        }

        private static object LowerValueCoerceValueCallback(DependencyObject target, object valueObject)
        {
            DoubleThumbSlider targetSlider = (DoubleThumbSlider)target;
            double value = (double)valueObject;

            return Math.Min(value, targetSlider.UpperValue);
        }

        private static object UpperValueCoerceValueCallback(DependencyObject target, object valueObject)
        {
            DoubleThumbSlider targetSlider = (DoubleThumbSlider)target;
            double value = (double)valueObject;

            return Math.Max(value, targetSlider.LowerValue);
        }

        public static readonly DependencyProperty MinimumProperty =
    DependencyProperty.Register("Minimum", typeof(double), typeof(DoubleThumbSlider), new UIPropertyMetadata(0d));
        public static readonly DependencyProperty LowerValueProperty =
            DependencyProperty.Register("LowerValue", typeof(double), typeof(DoubleThumbSlider), new UIPropertyMetadata(0d, null, LowerValueCoerceValueCallback));
        public static readonly DependencyProperty UpperValueProperty =
            DependencyProperty.Register("UpperValue", typeof(double), typeof(DoubleThumbSlider), new UIPropertyMetadata(1d, null, UpperValueCoerceValueCallback));
        public static readonly DependencyProperty MaximumProperty =
            DependencyProperty.Register("Maximum", typeof(double), typeof(DoubleThumbSlider), new UIPropertyMetadata(1d));
        public static readonly DependencyProperty IsSnapToTickEnabledProperty =
            DependencyProperty.Register("IsSnapToTickEnabled", typeof(bool), typeof(DoubleThumbSlider), new UIPropertyMetadata(false));
        public static readonly DependencyProperty TickFrequencyProperty =
            DependencyProperty.Register("TickFrequency", typeof(double), typeof(DoubleThumbSlider), new UIPropertyMetadata(0.1d));
        public static readonly DependencyProperty TickPlacementProperty =
            DependencyProperty.Register("TickPlacement", typeof(TickPlacement), typeof(DoubleThumbSlider), new UIPropertyMetadata(TickPlacement.None));
        public static readonly DependencyProperty TicksProperty =
            DependencyProperty.Register("Ticks", typeof(DoubleCollection), typeof(DoubleThumbSlider), new UIPropertyMetadata(null));

        public double Minimum
        {
            get { return (double)GetValue(MinimumProperty); }
            set { SetValue(MinimumProperty, value); }
        }

        public double LowerValue
        {
            get { return (double)GetValue(LowerValueProperty); }
            set { SetValue(LowerValueProperty, value); }
        }

        public double UpperValue
        {
            get { return (double)GetValue(UpperValueProperty); }
            set { SetValue(UpperValueProperty, value); }
        }

        public double Maximum
        {
            get { return (double)GetValue(MaximumProperty); }
            set { SetValue(MaximumProperty, value); }
        }

        public bool IsSnapToTickEnabled
        {
            get { return (bool)GetValue(IsSnapToTickEnabledProperty); }
            set { SetValue(IsSnapToTickEnabledProperty, value); }
        }

        public double TickFrequency
        {
            get { return (double)GetValue(TickFrequencyProperty); }
            set { SetValue(TickFrequencyProperty, value); }
        }

        public TickPlacement TickPlacement
        {
            get { return (TickPlacement)GetValue(TickPlacementProperty); }
            set { SetValue(TickPlacementProperty, value); }
        }

        public DoubleCollection Ticks
        {
            get { return (DoubleCollection)GetValue(TicksProperty); }
            set { SetValue(TicksProperty, value); }
        }
    }
}
