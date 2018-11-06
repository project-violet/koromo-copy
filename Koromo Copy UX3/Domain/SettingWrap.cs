/***

   Copyright (C) 2018. dc-koromo. All Rights Reserved.
   
   Author: Koromo Copy Developer

***/

using Koromo_Copy;
using Koromo_Copy.Interface;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Koromo_Copy_UX3.Domain
{
    /// <summary>
    /// UX Setting의 wrapping입니다. 이 오브젝트는 UX에 의존합니다.
    /// </summary>
    public class SettingWrap : DependencyObject
    {
        public static readonly DependencyProperty SearchSpaceWheelSpeedProperty =
            DependencyProperty.Register(nameof(SearchSpaceWheelSpeed),
                                        typeof(Double),
                                        typeof(SettingWrap),
                                        new PropertyMetadata(Settings.Instance.UXSetting.SearchSpaceWheelSpeed));

        public static readonly DependencyProperty ArtistViewerWheelSpeedProperty =
            DependencyProperty.Register(nameof(ArtistViewerWheelSpeed),
                                        typeof(Double),
                                        typeof(SettingWrap),
                                        new PropertyMetadata(Settings.Instance.UXSetting.ArtistViewerWheelSpeed));

        private static readonly SettingWrap _instance = new SettingWrap();
        public static SettingWrap Instance { get { return _instance; } }

        public Double SearchSpaceWheelSpeed
        {
            get {
                return (Double)GetValue(SearchSpaceWheelSpeedProperty);
            }
            set
            {
                SetValue(SearchSpaceWheelSpeedProperty, value);
                Settings.Instance.UXSetting.SearchSpaceWheelSpeed = value;
                Settings.Instance.Save();
            }
        }

        public Double ArtistViewerWheelSpeed
        {
            get
            {
                return (Double)GetValue(ArtistViewerWheelSpeedProperty);
            }
            set
            {
                SetValue(ArtistViewerWheelSpeedProperty, value);
                Settings.Instance.UXSetting.ArtistViewerWheelSpeed = value;
                Settings.Instance.Save();
            }
        }

    }
}
