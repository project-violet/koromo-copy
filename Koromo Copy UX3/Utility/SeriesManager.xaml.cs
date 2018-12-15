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

namespace Koromo_Copy_UX3.Utility
{
    /// <summary>
    /// SeriesManager.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class SeriesManager : Window
    {
        public SeriesManager()
        {
            InitializeComponent();

            Loaded += SeriesManager_Loaded;
        }

        private void SeriesManager_Loaded(object sender, RoutedEventArgs e)
        {
            throw new NotImplementedException();
        }
    }
}
