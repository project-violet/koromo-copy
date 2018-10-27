using Koromo_Copy_UX2.Domain;
using MaterialDesignColors;
using MaterialDesignThemes.Wpf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Koromo_Copy_UX2
{
    public class SettingViewModel
    {
        public SettingViewModel()
        {
            var swatch = new List<Swatch>();
            foreach(var sw in new SwatchesProvider().Swatches)
            {
                swatch.Add(new Swatch(TranslateSwatchName(sw.Name), sw.PrimaryHues, sw.AccentHues));
            }
            Swatches = swatch;
        }

        private string TranslateSwatchName(string name)
        {
            switch (name)
            {
                case "yellow": return "노랑색";
                case "amber": return "호박색";
                case "deeporange": return "진한 주황색";
                case "lightblue": return "밝은 청색";
                case "teal": return "암록색";
                case "cyan": return "청록색";
                case "pink": return "분홍색";
                case "green": return "녹색";
                case "deeppurple": return "진한 보라색";
                case "indigo": return "남색";
                case "lightgreen": return "연한 초록색";
                case "blue": return "파란색";
                case "lime": return "라임색";
                case "red": return "빨간색";
                case "orange": return "주황색";
                case "purple": return "보라색";
                case "bluegrey": return "푸른 회색";
                case "grey": return "회색";
                case "brown": return "갈색";
            }
            return name;
        }

        public ICommand ToggleBaseCommand { get; } = new AnotherCommandImplementation(o => ApplyBase((bool)o));

        private static void ApplyBase(bool isDark)
        {
            new PaletteHelper().SetLightDark(isDark);
        }

        public IEnumerable<Swatch> Swatches { get; }

        public ICommand ApplyPrimaryCommand { get; } = new AnotherCommandImplementation(o => ApplyPrimary((Swatch)o));

        private static void ApplyPrimary(Swatch swatch)
        {
            new PaletteHelper().ReplacePrimaryColor(swatch);
        }

        public ICommand ApplyAccentCommand { get; } = new AnotherCommandImplementation(o => ApplyAccent((Swatch)o));

        private static void ApplyAccent(Swatch swatch)
        {
            new PaletteHelper().ReplaceAccentColor(swatch);
        }
    }
}
