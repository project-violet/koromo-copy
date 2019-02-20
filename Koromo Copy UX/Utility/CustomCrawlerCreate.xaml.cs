/***

   Copyright (C) 2018-2019. dc-koromo. All Rights Reserved.
   
   Author: Koromo Copy Developer

***/

using Koromo_Copy.Script;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
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
using System.Windows.Shapes;

namespace Koromo_Copy_UX.Utility
{
    /// <summary>
    /// CustomCrawlerCreate.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class CustomCrawlerCreate : Window
    {
        public CustomCrawlerCreate()
        {
            InitializeComponent();
        }

        private ScriptModel intergrate()
        {
            return new ScriptModel
            {
                ScriptName = ScriptName.Text,
                ScriptVersion = ScriptVersion.Text,
                ScriptAuthor = ScriptAuthor.Text,
                ScriptFolderName = ScriptFolderName.Text,
                ScriptRequestName = ScriptRequestName.Text,
                PerDelay = Convert.ToInt32(PerDelay.Text),
                UsingDriver = Convert.ToInt32(UsingDriver.Text) == 1 ? true : false,
                
                URLSpecifier = URLSpecifier.Text,
                TitleCAL = TitleCAL.Text,
                ImagesCAL = ImagesCAL.Text,
                FileNameCAL = FileNameCAL.Text,

                UsingSub = Convert.ToInt32(UsingSub.Text) == 1 ? true : false,
                SubURLCAL = SubURLCAL.Text,
                SubURLTitleCAL = SubURLTitleCAL.Text,
                SubTitleCAL = SubTitleCAL.Text,
                SubImagesCAL = SubImagesCAL.Text,
                SubFileNameCAL = SubFileNameCAL.Text,
            };
        }

        private void derivative(ScriptModel model)
        {
            ScriptName.Text = model.ScriptName;
            ScriptVersion.Text = model.ScriptVersion;
            ScriptAuthor.Text = model.ScriptAuthor;
            ScriptFolderName.Text = model.ScriptFolderName;
            ScriptRequestName.Text = model.ScriptRequestName;
            PerDelay.Text = model.PerDelay.ToString();
            UsingDriver.Text = model.UsingDriver ? "1" : "0";

            URLSpecifier.Text = model.URLSpecifier;
            TitleCAL.Text = model.TitleCAL;
            ImagesCAL.Text = model.ImagesCAL;
            FileNameCAL.Text = model.FileNameCAL;

            UsingSub.Text = model.UsingSub ? "1" : "0";
            SubURLCAL.Text = model.SubURLCAL;
            SubURLTitleCAL.Text = model.SubURLTitleCAL;
            SubTitleCAL.Text = model.SubTitleCAL;
            SubImagesCAL.Text = model.SubImagesCAL;
            SubFileNameCAL.Text = model.SubFileNameCAL;
        }
        
        private void Import_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                ScriptEngine.Instance.Extract(ScriptName.Text);
                ScriptEngine.Instance.AddScript(intergrate());

                MessageBox.Show("Import Complete!", Title, MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, Title, MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void Save_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var dlg = new Microsoft.Win32.SaveFileDialog();
                dlg.FileName = $"{ScriptName.Text}-{ScriptVersion.Text}";
                dlg.DefaultExt = ".json";
                dlg.Filter = "Script File (.json)|*.json";

                if (dlg.ShowDialog().Value)
                {
                    string json = JsonConvert.SerializeObject(intergrate(), Formatting.Indented);
                    using (var fs = new StreamWriter(new FileStream(dlg.FileName, FileMode.Create, FileAccess.Write)))
                    {
                        fs.Write(json);
                    }

                    MessageBox.Show("Save Complete!", Title, MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, Title, MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void Open_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var dlg = new Microsoft.Win32.OpenFileDialog();
                dlg.DefaultExt = ".json";
                dlg.Filter = "Script File (.json)|*.json";

                if (dlg.ShowDialog().Value)
                {
                    derivative(JsonConvert.DeserializeObject<ScriptModel>(File.ReadAllText(dlg.FileName)));

                    MessageBox.Show("Open Complete!", Title, MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, Title, MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
