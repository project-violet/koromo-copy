/***

   Copyright (C) 2018-2019. dc-koromo. All Rights Reserved.
   
   Author: Koromo Copy Developer

***/

using Koromo_Copy;
using Koromo_Copy.Component.Hitomi;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
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

namespace Koromo_Copy_UX
{
    /// <summary>
    /// RecommendArtistElements.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class RecommendArtistElements : UserControl
    {
        public RecommendArtistElements()
        {
            InitializeComponent();
            Koromo_Copy_UX.Language.Lang.ApplyLanguageDictionary(this);
        }

        public string Artist;

        public RecommendArtistElements(string artist, string score, string detail)
        {
            InitializeComponent();
            Koromo_Copy_UX.Language.Lang.ApplyLanguageDictionary(this);

            ArtistTextBox.Text = artist;
            ScoreTextBox.Text = detail;

            if (score.Length > 8)
                ScoreLabel.Text = $"{FindResource("score")} : {score.Remove(8)} {FindResource("score_postfix")}";
            else
                ScoreLabel.Text = $"{FindResource("score")} : {score} {FindResource("score_postfix")}";
            Artist = artist;

            Loaded += RecommendArtistElements_Loaded;
        }

        bool IsDataLoaded = false;
        private void RecommendArtistElements_Loaded(object sender, RoutedEventArgs e)
        {
            if (IsDataLoaded) return;
            IsDataLoaded = true;
            Task.Run(() =>
            {
                List<string> titles = new List<string>();
                List<string> magics = new List<string>();

                for (int i = 0, j = 0; i < 5 && j < HitomiIndex.Instance.metadata_collection.Count; j++)
                {
                    if (HitomiIndex.Instance.metadata_collection[j].Artists != null &&
                       ((HitomiIndex.Instance.metadata_collection[j].Language >= 0 ? Settings.Instance.Hitomi.Language == HitomiIndex.Instance.index.Languages[HitomiIndex.Instance.metadata_collection[j].Language] : Settings.Instance.Hitomi.Language == "n/a") || Settings.Instance.Hitomi.Language == "all") &&
                        HitomiIndex.Instance.metadata_collection[j].Artists.Select(x => HitomiIndex.Instance.index.Artists[x]).Contains(Artist))
                    {
                        string ttitle = HitomiIndex.Instance.metadata_collection[j].Name.Split('|')[0];
                        if (titles.Count > 0 && !titles.TrueForAll((title) => Strings.ComputeLevenshteinDistance(ttitle, title) > Settings.Instance.Hitomi.TextMatchingAccuracy)) continue;

                        titles.Add(ttitle);
                        magics.Add(HitomiIndex.Instance.metadata_collection[j].ID.ToString());
                        i++;
                    }
                }
                require_count = magics.Count;
                loaded_count = 0;
                Image[] images = { Image1, Image2, Image3, Image4, Image5 };
                for (int i = 0; i < magics.Count; i++)
                {
                    var thumbnail = HitomiCommon.HitomiThumbnail + HitomiParser.ParseGalleryBlock(Koromo_Copy.Net.NetCommon.DownloadString(
                        $"{HitomiCommon.HitomiGalleryBlock}{magics[i]}.html")).Thumbnail;

                    int j = i;
                    Application.Current.Dispatcher.BeginInvoke(new Action(
                    delegate
                    {
                        BitmapImage[j] = new BitmapImage();
                        BitmapImage[j].BeginInit();
                        BitmapImage[j].UriSource = new Uri(thumbnail);
                        if (Settings.Instance.Model.LowQualityImage)
                            BitmapImage[j].DecodePixelWidth = 100;
                        BitmapImage[j].EndInit();
                        BitmapImage[j].DownloadCompleted += BitmapImage_DownloadCompleted;
                    }));
                }
            });
        }

        int require_count;
        int loaded_count;

        BitmapImage[] BitmapImage = new BitmapImage[5];
        public Action LoadComplete = null;

        private void BitmapImage_DownloadCompleted(object sender, EventArgs e)
        {
            Application.Current.Dispatcher.BeginInvoke(new Action(
            delegate
            {
                Image[] images = { Image1, Image2, Image3, Image4, Image5 };

                for (int i = 0; i < 5; i++)
                    if (BitmapImage[i] == sender)
                    {
                        images[i].Source = BitmapImage[i];
                        break;
                    }

                if (Interlocked.Increment(ref loaded_count) == require_count)
                {
                    LoadComplete();
                }
            }));
        }

        private void Image_MouseMove(object sender, MouseEventArgs e)
        {
            var image = sender as Image;
            var tooltip = image.ToolTip as ToolTip;
            tooltip.Placement = System.Windows.Controls.Primitives.PlacementMode.Relative;
            tooltip.HorizontalOffset = e.GetPosition((IInputElement)sender).X + 10;
            tooltip.VerticalOffset = e.GetPosition((IInputElement)sender).Y;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            (new ArtistViewerWindow(Artist)).Show();
        }
    }
}
