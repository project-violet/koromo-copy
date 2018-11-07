/***

   Copyright (C) 2018. dc-koromo. All Rights Reserved.
   
   Author: Koromo Copy Developer

***/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;

namespace Koromo_Copy_UX3.Controls.AutoComplete
{
    public class AutoCompleteTextBox : TextBox
    {
        Popup popup;
        ListBox listBox;

        static AutoCompleteTextBox()
        {
            DefaultStyleKeyProperty.OverrideMetadata(
                typeof(AutoCompleteTextBox),
                new FrameworkPropertyMetadata(typeof(AutoCompleteTextBox)));
        }

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            popup = Template.FindName("ACTB_Popup", this) as Popup;
            listBox = Template.FindName("ACTB_ListBox", this) as ListBox;
        }

        private void OnItemTemplateChanged(DataTemplate dt)
        {
            listBox.ItemTemplate = dt;
        }

        protected override void OnTextChanged(TextChangedEventArgs e)
        {
            //base.OnTextChanged(e);
            //popup.IsOpen = true;
            //listBox.SelectedIndex = -1;
        }
    }
}
