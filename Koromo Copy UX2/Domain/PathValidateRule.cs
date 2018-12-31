/***

   Copyright (C) 2018-2019. dc-koromo. All Rights Reserved.
   
   Author: Koromo Copy Developer

***/

using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows.Controls;

namespace Koromo_Copy_UX2.Domain
{
    public class PathValidateRule : ValidationRule
    {
        public override ValidationResult Validate(object value, CultureInfo cultureInfo)
        {
            if (string.IsNullOrWhiteSpace((value ?? "").ToString()))
                return new ValidationResult(false, "경로는 비어있으면 안됩니다.");
            
            var ss = "";
            foreach (var s in value.ToString().Split('\\'))
                if (s.StartsWith("{")) break;
                else ss += s + "\\";
            if (!Directory.Exists(ss))
                return new ValidationResult(false, "유효하지 않은 경로입니다.");

            if (!(value.ToString().ToLower().Contains("{id}") || value.ToString().ToLower().Contains("{title}")))
                return new ValidationResult(false, "{Id} 또는 {Title}를 반드시 하나이상 포함해야합니다.");

            var valid_tokens = new string[]
            {
                "{title}",
                "{artists}",
                "{id}", 
                "{type}",
                "{date}",
                "{series}",
                "{search}"
            };
            var regex = Regex.Matches(value.ToString(), @"(\{.*?\})");
            if (!regex.OfType<Match>().All(x => valid_tokens.Contains(x.Value.ToLower())))
                return new ValidationResult(false, "유효하지 않은 토큰이 있습니다. " + string.Join(", ", regex.OfType<Match>().Where(x => !valid_tokens.Contains(x.Value.ToLower()))));

            return ValidationResult.ValidResult;
        }
    }
}
