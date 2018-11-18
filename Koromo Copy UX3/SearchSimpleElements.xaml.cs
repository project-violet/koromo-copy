using Koromo_Copy.Component.Hitomi;
using Koromo_Copy.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Koromo_Copy_UX3
{
    /// <summary>
    /// SearchSimpleElements.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class SearchSimpleElements : UserControl
    {
        public SearchSimpleElements()
        {
            InitializeComponent();
        }

        private void SearchSimpleElements_MouseLeave(object sender, MouseEventArgs e)
        {
            if (!_Select)
            {
                PinkRectangle.Fill = Brushes.Transparent;
                Border.BorderBrush = Brushes.Transparent;
            }
        }

        private void SearchSimpleElements_MouseEnter(object sender, MouseEventArgs e)
        {
            if (!_Select)
            {
                PinkRectangle.Fill = new SolidColorBrush(Color.FromArgb(100, 234, 202, 233));
                Border.BorderThickness = new Thickness(1);
                //Border.BorderBrush = Brushes.LightPink;
                Border.BorderBrush = new SolidColorBrush(Color.FromRgb(255, 174, 201));
                BorderCollapsed.BorderThickness = new Thickness(1);
            }
        }

        private bool _Select = false;
        public bool Select
        {
            get { return _Select; }
            set
            {
                _Select = value;
                if (value)
                {
                    PinkRectangle.Fill = new SolidColorBrush(Color.FromArgb(200, 234, 202, 233));
                    //PinkRectangle.Fill = Brushes.Pink;
                    Border.BorderThickness = new Thickness(2);
                    Border.BorderBrush = Brushes.LightPink;
                    BorderCollapsed.BorderThickness = new Thickness(0);
                }
                else
                {
                    PinkRectangle.Fill = new SolidColorBrush(Color.FromArgb(100, 234, 202, 233));
                    Border.BorderThickness = new Thickness(1);
                    //Border.BorderBrush = Brushes.LightPink;
                    Border.BorderBrush = new SolidColorBrush(Color.FromRgb(255, 174, 201));
                    BorderCollapsed.BorderThickness = new Thickness(1);
                }
            }
        }

        public void Transparent()
        {
            PinkRectangle.Fill = Brushes.Transparent;
            Border.BorderBrush = Brushes.Transparent;
        }

        private void SearchSimpleElements_MouseUp(object sender, MouseButtonEventArgs e)
        {
            Select = !Select;
        }

        public IArticle Article;

        public SearchSimpleElements(IArticle article)
        {
            InitializeComponent();

            Article = article;
            
            Loaded += SearchSimpleElements_Loaded;

            if (article is HitomiArticle ha)
                if (HitomiLog.Instance.Contains(ha.Magic))
                    DownloadMark.Visibility = Visibility.Visible;
        }

        bool init = false;
        private void SearchSimpleElements_Loaded(object sender, EventArgs ex)
        {
            if (init) return;
            init = true;
            Task.Run(() =>
            {
                HitomiArticle ha = Article as HitomiArticle;
                ha.Thumbnail = HitomiCommon.HitomiThumbnail + HitomiParser.ParseGalleryBlock(Koromo_Copy.Net.NetCommon.DownloadString(
                    $"{HitomiCommon.HitomiGalleryBlock}{ha.Magic}.html")).Thumbnail;
                ha.ImagesLink = HitomiParser.GetImageLink(Koromo_Copy.Net.NetCommon.DownloadString(HitomiCommon.GetImagesLinkAddress(ha.Magic)));

                Application.Current.Dispatcher.BeginInvoke(new Action(
                delegate
                {
                    BitmapImage.BeginInit();
                    BitmapImage.UriSource = new Uri(ha.Thumbnail);
                    BitmapImage.EndInit();
                    Title.Text = ha.Title;
                    Image.Source = BitmapImage;
                    //b.DownloadCompleted += B_DownloadCompleted;
                }));
            });
        }

        //private void B_DownloadCompleted(object sender, EventArgs e)
        //{
        //    Image.Source = bitmapto(b, b.Width, b.Height);
        //}

        //System.Drawing.Bitmap GetBitmap(BitmapSource source)
        //{
        //    System.Drawing.Bitmap bmp = new System.Drawing.Bitmap(
        //      source.PixelWidth,
        //      source.PixelHeight, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
        //    System.Drawing.Imaging.BitmapData data = bmp.LockBits(
        //      new System.Drawing.Rectangle(System.Drawing.Point.Empty, bmp.Size),
        //      System.Drawing.Imaging.ImageLockMode.WriteOnly,
        //      System.Drawing.Imaging.PixelFormat.Format32bppPArgb);
        //    source.CopyPixels(
        //      Int32Rect.Empty,
        //      data.Scan0,
        //      data.Height * data.Stride,
        //      data.Stride);
        //    bmp.UnlockBits(data);
        //    return bmp;
        //}

        //public static BitmapSource ConvertBitmap(System.Drawing.Bitmap source)
        //{
        //    return System.Windows.Interop.Imaging.CreateBitmapSourceFromHBitmap(
        //                  source.GetHbitmap(),
        //                  IntPtr.Zero,
        //                  Int32Rect.Empty,
        //                  BitmapSizeOptions.FromEmptyOptions());
        //}


        //public BitmapSource bitmapto(BitmapSource bitmap_source, double ActualWidth, double ActualHeight)
        //{
        //    PresentationSource source = PresentationSource.FromVisual(this);

        //    double dpiX, dpiY;
        //    dpiX = 96.0 * source.CompositionTarget.TransformToDevice.M11;
        //    dpiY = 96.0 * source.CompositionTarget.TransformToDevice.M22;

        //    float dpi = (float)dpiX;
        //    float factor = dpi / 96f * 2;
        //    int width = (int)Math.Round(ActualWidth * factor);
        //    int height = (int)Math.Round(ActualHeight * factor);

        //    // Create bitmaps.
        //    System.Drawing.Bitmap oldBitmap = GetBitmap(bitmap_source);
        //    System.Drawing.Bitmap newBitmap = new System.Drawing.Bitmap(width, height, System.Drawing.Imaging.PixelFormat.Format32bppArgb);

        //    // Draw the new bitmap. Use high-quality interpolation mode.
        //    using (System.Drawing.Graphics g = System.Drawing.Graphics.FromImage(newBitmap))
        //    {
        //        g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;

        //        g.Clear(System.Drawing.Color.Transparent);
        //        g.DrawImage(oldBitmap, 0, 0, newBitmap.Width, newBitmap.Height);
        //    }

        //    // Set the image source to the resized bitmap.
        //    return ConvertBitmap(newBitmap);
        //}

        public BitmapImage BitmapImage = new BitmapImage();

        private void Image_MouseMove(object sender, MouseEventArgs e)
        {
            ImageToolTip.Placement = System.Windows.Controls.Primitives.PlacementMode.Relative;
            ImageToolTip.HorizontalOffset = e.GetPosition((IInputElement)sender).X + 10;
            ImageToolTip.VerticalOffset = e.GetPosition((IInputElement)sender).Y;
        }
    }
}
