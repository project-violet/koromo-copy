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
        public static HitomiArticle MetadataToArticle(HitomiIndexMetadata metadata)
        {
            HitomiArticle article = new HitomiArticle();
            if (metadata.Artists != null) article.Artists = metadata.Artists.Select(x => HitomiIndex.Instance.index.Artists[x]).ToArray();
            if (metadata.Characters != null) article.Characters = metadata.Characters.Select(x => HitomiIndex.Instance.index.Characters[x]).ToArray();
            if (metadata.Groups != null) article.Groups = metadata.Groups.Select(x => HitomiIndex.Instance.index.Groups[x]).ToArray();
            if (metadata.Language >= 0) article.Language = HitomiIndex.Instance.index.Languages[metadata.Language];
            article.Magic = metadata.ID.ToString();
            if (metadata.Parodies != null) article.Series = metadata.Parodies.Select(x => HitomiIndex.Instance.index.Series[x]).ToArray();
            if (metadata.Tags != null) article.Tags = metadata.Tags.Select(x => HitomiIndex.Instance.index.Tags[x]).ToArray();
            article.Title = metadata.Name;
            if (metadata.Type >= 0) article.Type = HitomiIndex.Instance.index.Types[metadata.Type];
            return article;
        }

        public static HitomiMetadata ArticleToMetadata(HitomiArticle article)
        {
            HitomiMetadata metadata = new HitomiMetadata();
            if (article.Artists != null) metadata.Artists = article.Artists;
            if (article.Characters != null) metadata.Characters = article.Characters;
            if (article.Groups != null) metadata.Groups = article.Groups;
            metadata.ID = Convert.ToInt32(article.Magic);
            metadata.Language = LegalizeLanguage(article.Language);
            metadata.Name = article.Title;
            if (article.Series != null) metadata.Parodies = article.Series;
            if (article.Tags != null) metadata.Tags = article.Tags.Select(x => LegalizeTag(x)).ToArray();
            metadata.Type = article.Type;
            return metadata;
        }

        public static HitomiIndexMetadata? GetMetadataFromMagic(string magic)
        {
            HitomiIndexMetadata tmp = new HitomiIndexMetadata() { ID = Convert.ToInt32(magic) };
            var pos = HitomiIndex.Instance.metadata_collection.BinarySearch(tmp, Comparer<HitomiIndexMetadata>.Create((x,y) => y.ID.CompareTo(x.ID)));
            if (pos < 0) return null;
            return HitomiIndex.Instance.metadata_collection[pos];
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
