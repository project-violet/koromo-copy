/***

   Copyright (C) 2018-2019. dc-koromo. All Rights Reserved.
   
   Author: Koromo Copy Developer

***/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace Koromo_Copy_UX.Language
{
    public static class Lang
    {
        public static void ApplyLanguageDictionary(Control w)
        {
            var dict = new ResourceDictionary();
            var lang = Thread.CurrentThread.CurrentCulture.ToString();
            switch (lang)
            {
                case "ja-JP":
                case "ko-KR":
                    dict.Source = new Uri($@"..\Language\Lang.{lang}.xaml", UriKind.Relative);
                    //dict.Source = new Uri($@"..\Language\Lang.en-US.xaml", UriKind.Relative);
                    //dict.Source = new Uri($@"..\Language\Lang.ja-JP.xaml", UriKind.Relative);
                    break;
                default:
                    dict.Source = new Uri($@"..\Language\Lang.en-US.xaml", UriKind.Relative);
                    break;
            }
            w.Resources.MergedDictionaries.Add(dict);
        }
    }
}
