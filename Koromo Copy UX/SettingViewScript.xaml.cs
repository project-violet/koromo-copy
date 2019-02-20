/***

   Copyright (C) 2018-2019. dc-koromo. All Rights Reserved.
   
   Author: Koromo Copy Developer

***/

using Koromo_Copy.Script;
using Koromo_Copy_UX.Domain;
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

namespace Koromo_Copy_UX
{
    /// <summary>
    /// SettingViewScript.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class SettingViewScript : UserControl
    {
        public static SettingViewScript Instance;
        public SettingViewScript()
        {
            InitializeComponent();

            Instance = this;
        }
        
        public void Init()
        {
            int index = 0;
            foreach (var script in ScriptManager.Instance.Scripts)
            {
                ScriptInfo.Children.Add(new Separator { Background = new SolidColorBrush(Colors.Gainsboro), Opacity = 0.8 });
                ScriptInfo.Children.Add(new ScriptInfoElements
                {
                    DataContext = new ScriptInfoViewModel
                    {
                        Version = $"{script.Attribute().ScriptName} {script.Attribute().ScriptVersion}",
                        Index = index++,
                        Content =
                        $"스크립트 이름 : {script.Attribute().ScriptName}\r\n" +
                        $"스크립트 버전 : {script.Attribute().ScriptVersion}\r\n" +
                        $"스크립트 작성자 : {script.Attribute().ScriptAuthor}\r\n" +
                        $"생성 하위 폴더 : {script.Attribute().ScriptFolderName}\r\n" + 
                        $"스크립트 식별자 : {script.Attribute().URLSpecifier}\r\n"
                    }
                });
            }
        }
    }
}
