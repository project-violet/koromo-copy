using Koromo_Copy.Plugin;
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
    /// SettingViewPlugin.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class SettingViewPlugin : UserControl
    {
        public SettingViewPlugin()
        {
            InitializeComponent();

            Loaded += SettingViewPlugin_Loaded;
        }

        private void SettingViewPlugin_Loaded(object sender, RoutedEventArgs e)
        {
            PlugInList.Text = string.Join("\r\n", PlugInManager.Instance.GetLoadedPlugins().ToArray());
        }
    }
}
