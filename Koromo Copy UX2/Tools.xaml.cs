using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Koromo_Copy_UX2
{
    /// <summary>
    /// Tools.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class Tools : System.Windows.Controls.UserControl
    {
        public Tools()
        {
            InitializeComponent();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            switch (((ButtonBase)sender).Tag.ToString())
            {
                case "1":
                    new Koromo_Copy.Utility.FsEnumerator().Show();
                    break;

                case "2":
                    new Koromo_Copy.Utility.HitomiExplorer().Show();
                    break;

                case "3":
                    new Koromo_Copy.Utility.MetadataDownloader().Show();
                    break;

                case "4":
                    new Koromo_Copy.Utility.StringTools().Show();
                    break;
            }
        }
    }
}
