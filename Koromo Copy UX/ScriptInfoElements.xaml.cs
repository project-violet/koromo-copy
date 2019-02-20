/***

   Copyright (C) 2018-2019. dc-koromo. All Rights Reserved.
   
   Author: Koromo Copy Developer

***/

using Koromo_Copy.Script;
using Koromo_Copy_UX.Domain;
using Koromo_Copy_UX.Utility;
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
    /// ScriptInfoElemetns.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class ScriptInfoElements : UserControl
    {
        public ScriptInfoElements()
        {
            InitializeComponent();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            var index = ((ScriptInfoViewModel)DataContext).Index;
            (new ScriptEditor(string.Join("\n", ScriptManager.Instance.Scripts[index].RawScript))).Show();
        }
    }
}
