using System.Windows;
using System.Windows.Controls;

namespace Koromo_Copy_UX2.Domain
{
    public class SimpleDataTemplateSelector : DataTemplateSelector
    {
        public DataTemplate FixedTemplate { get; set; }

        public override DataTemplate SelectTemplate(object item, DependencyObject container)
        {
            return FixedTemplate;
        }
    }
}