/***

   Copyright (C) 2018-2019. dc-koromo. All Rights Reserved.
   
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
using System.Windows.Media;

namespace Koromo_Copy_UX.Domain
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
#if DEBUG
                                        new PropertyMetadata(1.0));
#else
                                        new PropertyMetadata(Settings.Instance.UXSetting.SearchSpaceWheelSpeed));
#endif

        public static readonly DependencyProperty ArtistViewerWheelSpeedProperty =
            DependencyProperty.Register(nameof(ArtistViewerWheelSpeed),
                                        typeof(Double),
                                        typeof(SettingWrap),
#if DEBUG
                                        new PropertyMetadata(1.0));
#else
                                        new PropertyMetadata(Settings.Instance.UXSetting.ArtistViewerWheelSpeed));
#endif

        public static readonly DependencyProperty DoNotHightlightAutoCompleteResultsProperty =
            DependencyProperty.Register(nameof(DoNotHightlightAutoCompleteResults),
                                        typeof(bool),
                                        typeof(SettingWrap),
#if DEBUG
                                        new PropertyMetadata(false));
#else
                                        new PropertyMetadata(Settings.Instance.UXSetting.DoNotHightlightAutoCompleteResults));
#endif
        
        public static readonly DependencyProperty MaxCountOfAutoCompleteResultProperty =
            DependencyProperty.Register(nameof(MaxCountOfAutoCompleteResult),
                                        typeof(int),
                                        typeof(SettingWrap),
#if DEBUG
                                        new PropertyMetadata(100));
#else
                                        new PropertyMetadata(Settings.Instance.UXSetting.MaxCountOfAutoCompleteResult));
#endif

        public static readonly DependencyProperty ThemeColorProperty =
            DependencyProperty.Register(nameof(ThemeColorProperty),
                                        typeof(Color),
                                        typeof(SettingWrap),
#if DEBUG
                                        new PropertyMetadata(Colors.Pink));
#else
                                        new PropertyMetadata(Color.FromArgb(Settings.Instance.UXSetting.ThemeColor.A,
                                                                            Settings.Instance.UXSetting.ThemeColor.R,
                                                                            Settings.Instance.UXSetting.ThemeColor.G,
                                                                            Settings.Instance.UXSetting.ThemeColor.B)));
#endif

        public static readonly DependencyProperty ImageQualityProperty =
            DependencyProperty.Register(nameof(ImageQualityProperty),
                                        typeof(int),
                                        typeof(SettingWrap),
#if DEBUG
                                        new PropertyMetadata(0));
#else
                                        new PropertyMetadata(Settings.Instance.Model.ImageQuality));
#endif
        
        private static readonly SettingWrap _instance = new SettingWrap();
        public static SettingWrap Instance { get { return _instance; } }
        
        /// <summary>
        /// 검색창의 휠 스피드를 조정합니다.
        /// </summary>
        public Double SearchSpaceWheelSpeed
        {
            get
            {
                return (Double)GetValue(SearchSpaceWheelSpeedProperty);
            }
            set
            {
                SetValue(SearchSpaceWheelSpeedProperty, value);
                Settings.Instance.UXSetting.SearchSpaceWheelSpeed = value;
                Settings.Instance.Save();
            }
        }

        /// <summary>
        /// 작가창의 휠 스피드를 조정합니다.
        /// </summary>
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

        /// <summary>
        /// 자동완성결과에 하이라이트를 적용할지의 여부를 설정합니다.
        /// </summary>
        public bool DoNotHightlightAutoCompleteResults
        {
            get
            {
                return (bool)GetValue(DoNotHightlightAutoCompleteResultsProperty);
            }
            set
            {
                SetValue(DoNotHightlightAutoCompleteResultsProperty, value);
                Settings.Instance.UXSetting.DoNotHightlightAutoCompleteResults = value;
                Settings.Instance.Save();
            }
        }

        /// <summary>
        /// 자동완성결과에 하이라이트를 적용할지의 여부를 설정합니다.
        /// </summary>
        public int MaxCountOfAutoCompleteResult
        {
            get
            {
                return (int)GetValue(MaxCountOfAutoCompleteResultProperty);
            }
            set
            {
                SetValue(MaxCountOfAutoCompleteResultProperty, value);
                Settings.Instance.UXSetting.MaxCountOfAutoCompleteResult = value;
                Settings.Instance.Save();
            }
        }

        /// <summary>
        /// 테마 색상입니다.
        /// </summary>
        public Color ThemeColor
        {
            get
            {
                return (Color)GetValue(ThemeColorProperty);
            }
            set
            {
                SetValue(ThemeColorProperty, value);
                Settings.Instance.UXSetting.ThemeColor = System.Drawing.Color.FromArgb(value.A, value.R, value.G, value.B);
                Settings.Instance.Save();
            }
        }

        /// <summary>
        /// 이미지의 화질입니다.
        /// </summary>
        public BitmapScalingMode ImageQuality
        {
            get
            {
                int val = (int)GetValue(ImageQualityProperty);
                return (BitmapScalingMode)(2 - val);
            }
            set
            {
                SetValue(ImageQualityProperty, (int)value);
                Settings.Instance.Model.ImageQuality = (int)value;
                Settings.Instance.Save();
            }
        }
    }
}
