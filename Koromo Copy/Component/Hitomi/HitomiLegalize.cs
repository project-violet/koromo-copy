/***

   Copyright (C) 2018-2019. dc-koromo. All Rights Reserved.
   
   Author: Koromo Copy Developer

***/

using System;
using System.Collections.Generic;
using System.Linq;

namespace Koromo_Copy.Component.Hitomi
{
    public class HitomiLegalize
    {
        public static HitomiArticle MetadataToArticle(HitomiMetadata metadata)
        {
            HitomiArticle article = new HitomiArticle();
            article.Artists = metadata.Artists;
            article.Characters = metadata.Characters;
            article.Groups = metadata.Groups;
            article.Language = metadata.Language;
            article.Magic = metadata.ID.ToString();
            article.Series = metadata.Parodies;
            article.Tags = metadata.Tags;
            article.Title = metadata.Name;
            article.Type = metadata.Type;
            return article;
        }

        public static HitomiMetadata ArticleToMetadata(HitomiArticle article)
        {
            HitomiMetadata metadata = new HitomiMetadata();
            metadata.Artists = article.Artists;
            metadata.Characters = article.Characters;
            metadata.Groups = article.Groups;
            metadata.ID = Convert.ToInt32(article.Magic);
            metadata.Language = LegalizeLanguage(article.Language);
            metadata.Name = article.Title;
            metadata.Parodies = article.Series;
            metadata.Tags = article.Tags.Select(x => LegalizeTag(x)).ToArray();
            metadata.Type = article.Type;
            return metadata;
        }

        public class CompareMetadata : IComparer<HitomiMetadata> { public int Compare(HitomiMetadata x, HitomiMetadata y) { return y.ID.CompareTo(x.ID); } }
        public static HitomiMetadata? GetMetadataFromMagic(string magic)
        {
            HitomiMetadata tmp = new HitomiMetadata() { ID = Convert.ToInt32(magic) };
            var pos = HitomiData.Instance.metadata_collection.BinarySearch(tmp, new CompareMetadata());
            if (pos < 0) return null;
            return HitomiData.Instance.metadata_collection[pos];
        }

        public static string LegalizeTag(string tag)
        {
            if (tag.Trim().EndsWith("♀")) return "female:" + tag.Trim('♀').Trim();
            if (tag.Trim().EndsWith("♂")) return "male:" + tag.Trim('♂').Trim();
            return tag.Trim();
        }

        public static string LegalizeLanguage(string lang)
        {
            switch (lang)
            {
                case "모든 언어": return "all";
                case "한국어": return "korean";
                case "N/A": return "n/a";
                case "日本語": return "japanese";
                case "English": return "english";
                case "Español": return "spanish";
                case "ไทย": return "thai";
                case "Deutsch": return "german";
                case "中文": return "chinese";
                case "Português": return "portuguese";
                case "Français": return "french";
                case "Tagalog": return "tagalog";
                case "Русский": return "russian";
                case "Italiano": return "italian";
                case "polski": return "polish";
                case "tiếng việt": return "vietnamese";
                case "magyar": return "hungarian";
                case "Čeština": return "czech";
                case "Bahasa Indonesia": return "indonesian";
                case "العربية": return "arabic";
            }

            return lang;
        }

        public static string DeLegalizeLanguage(string lang)
        {
            switch (lang)
            {
                case "all": return "모든 언어";
                case "korean": return "한국어";
                case "n/a": return "N/A";
                case "japanese": return "日本語";
                case "english": return "English";
                case "spanish": return "Español";
                case "thai": return "ไทย";
                case "german": return "Deutsch";
                case "chinese": return "中文";
                case "portuguese": return "Português";
                case "french": return "Français";
                case "tagalog": return "Tagalog";
                case "russian": return "Русский";
                case "italian": return "Italiano";
                case "polish": return "polski";
                case "vietnamese": return "tiếng việt";
                case "hungarian": return "magyar";
                case "czech": return "Čeština";
                case "indonesian": return "Bahasa Indonesia";
                case "arabic": return "العربية";
            }

            return lang;
        }
    }
}
