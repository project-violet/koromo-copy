/***

   Copyright (C) 2018-2019. dc-koromo. All Rights Reserved.
   
   Author: Koromo Copy Developer

***/

using Koromo_Copy;
using Koromo_Copy.Component.Pixiv;
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
    /// SettingViewPixiv.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class SettingViewPixiv : UserControl
    {
        public SettingViewPixiv()
        {
            InitializeComponent();

            Path.Text = Settings.Instance.Pixiv.Path;
            Id.Text = Settings.Instance.Pixiv.Id;
            Password.Password = Settings.Instance.Pixiv.Password;
        }

        private async void Button_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                await PixivTool.Instance.Login(Id.Text, Password.Password);
                MainWindow.Instance.FadeOut_MiddlePopup("아이디/비밀번호가 유효합니다!", false);
                Settings.Instance.Pixiv.Id = Id.Text;
                Settings.Instance.Pixiv.Password = Password.Password;
                Settings.Instance.Save();
            }
            catch (Exception ex)
            {
                MainWindow.Instance.FadeOut_MiddlePopup("로그인 오류! 아이디/비밀번호를 확인하세요!", false);
                Koromo_Copy.Monitor.Instance.Push(ex.Message + "\r\n" + ex.StackTrace);
            }
        }

        private void Path_LostFocus(object sender, RoutedEventArgs e)
        {
            Settings.Instance.Pixiv.Path = Path.Text;
            Settings.Instance.Save();
        }
    }
}
