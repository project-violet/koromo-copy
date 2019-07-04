/***

   Copyright (C) 2018-2019. dc-koromo. All Rights Reserved.
   
   Author: Koromo Copy Developer

***/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
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
using System.Windows.Threading;

namespace Koromo_Copy_UX.Utility.Bookmark
{
    /// <summary>
    /// BookmarkEditClass.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class BookmarkEditClass : UserControl
    {
        public BookmarkEditClass()
        {
            InitializeComponent();

            var builder = new StringBuilder();

            foreach (var root in BookmarkModelManager.Instance.Model.root_classes)
                recursion(builder, root, "");
            
            ((Paragraph)(TextEdit.Document.Blocks.FirstBlock)).Margin = new Thickness(0, 0, 0, 0);
            ((Paragraph)(TextEdit.Document.Blocks.FirstBlock)).Inlines.Add(new Run(builder.ToString()));
        }
        
        private void recursion(StringBuilder builder, string sub, string parent)
        {
            if (BookmarkModelManager.Instance.Model.sub_classes.All(x => x.Item1 != parent + sub))
            {
                builder.Append(parent + sub + "\r\n");
                return;
            }

            var bb = new StringBuilder();
            foreach (var subb in BookmarkModelManager.Instance.Model.sub_classes.Where(x => x.Item1 == parent + sub))
            {
                recursion(builder, "/" + subb.Item2, parent + sub);
            }
        }

        public string ClassifyRule = "";
        private void TextEdit_TextChanged(object sender, TextChangedEventArgs e)
        {
            ClassifyRule = new TextRange(TextEdit.Document.ContentStart, TextEdit.Document.ContentEnd).Text;
        }
    }
}
