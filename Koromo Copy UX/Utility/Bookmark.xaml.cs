/***

   Copyright (C) 2018-2019. dc-koromo. All Rights Reserved.
   
   Author: Koromo Copy Developer

***/

using MaterialDesignThemes.Wpf;
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
using System.Windows.Shapes;

namespace Koromo_Copy_UX.Utility
{
    /// <summary>
    /// Bookmark.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class Bookmark : Window
    {
        public Bookmark()
        {
            InitializeComponent();

            refresh();
        }

        private void ClassifyTree_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            var item = ClassifyTree.SelectedItem;
            if (item is TreeViewItem tvi)
            {
                if (tvi.DataContext != null)
                {
                    ContentControl.Content = tvi.DataContext;
                }
            }
        }

        private async void ClassButton_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new BookmarkEditClass();
            if ((bool)(await DialogHost.Show(dialog, "BookmarkDialog")))
            {
                var sr = dialog.ClassifyRule;
                var lines = sr.Split(
                    new[] { "\r\n", "\r", "\n" },
                    StringSplitOptions.None
                );

                var used = new HashSet<string>();
                var root_classes = new List<string>();
                // parent class, class name
                var sub_classes = new List<Tuple<string, string>>();

                foreach (var r0 in lines)
                {
                    var r1 = r0.Trim();
                    if (r1 != "")
                    {
                        var ss = r1.Split('/');
                        if (!used.Contains(ss[1]))
                        {
                            used.Add(ss[1]);
                            root_classes.Add(ss[1]);
                        }
                        for (int i = 2; i < ss.Length; i++)
                        {
                            if (!used.Contains(ss[i]))
                            {
                                used.Add(ss[i]);
                                sub_classes.Add(new Tuple<string, string>(ss[i - 1], ss[i]));
                            }
                        }
                    }
                }

                BookmarkModelManager.Instance.Model.root_classes = root_classes;
                BookmarkModelManager.Instance.Model.sub_classes = sub_classes;
                BookmarkModelManager.Instance.Save();

                refresh();
            }
        }

        private void refresh()
        {
            ClassifyTree.Items.Clear();

            var name_dict = new Dictionary<string, TreeViewItem>();

            foreach (var root in BookmarkModelManager.Instance.Model.root_classes)
            {
                var tvi = new TreeViewItem
                {
                    Header = root,
                    DataContext = new BookmarkPage("/" + root)
                };
                name_dict.Add(root, tvi);
                ClassifyTree.Items.Add(tvi);
            }

            // Child, Parent
            var indegree = new Dictionary<string, string>();

            foreach (var sub in BookmarkModelManager.Instance.Model.sub_classes)
                indegree.Add(sub.Item2, sub.Item1);

            foreach (var sub in BookmarkModelManager.Instance.Model.sub_classes)
            {
                var fullname = "/" + sub.Item2;
                var nname = sub.Item2;

                while (indegree.ContainsKey(nname))
                {
                    nname = indegree[nname];
                    fullname = "/" + nname + fullname;
                }

                var tvi = new TreeViewItem
                {
                    Header = sub.Item2,
                    DataContext = new BookmarkPage(fullname)
                };
                name_dict.Add(sub.Item2, tvi);
                name_dict[sub.Item1].Items.Add(tvi);
            }

            ContentControl.Content = (ClassifyTree.Items[0] as TreeViewItem).DataContext;
        }

        private void RefreshButton_Click(object sender, RoutedEventArgs e)
        {

        }

        private void ExtractButton_Click(object sender, RoutedEventArgs e)
        {

        }

        private void AddButton_Click(object sender, RoutedEventArgs e)
        {

        }
    }
}
