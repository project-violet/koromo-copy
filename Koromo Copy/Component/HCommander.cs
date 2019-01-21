/***

   Copyright (C) 2018-2019. dc-koromo. All Rights Reserved.
   
   Author: Koromo Copy Developer

***/

using Koromo_Copy.Component.EH;
using Koromo_Copy.Component.Hitomi;
using Koromo_Copy.Component.Hiyobi;
using Koromo_Copy.Net;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Koromo_Copy.Component
{
    public enum HArticleType
    {
        Hitomi,
        EHentai,
        EXHentai,
        Hiyobi,
    }
    
    /// <summary>
    /// E Hentai 미러 웹 사이트들의 일반적인 작품 정보입니다.
    /// </summary>
    public struct HArticleModel
    {
        public HArticleType ArticleType;

        public string URL;
        public string Magic;
        public string Thumbnail;

        public string Title;
        public string SubTitle;

        public string Type;
        public string Uploader;

        public string Posted;
        public string Parent;
        public string Visible;
        public string Language;
        public string FileSize;
        public int Length;
        public int Favorited;
        public int RatingCount;
        public float RatingAverage;

        public string reclass;
        public string[] language;
        public string[] group;
        public string[] parody;
        public string[] character;
        public string[] artist;
        public string[] male;
        public string[] female;
        public string[] misc;
    }

    /// <summary>
    /// E Hentai 미러 웹 사이트들에 대한 지원을 제공합니다.
    /// </summary>
    public class HCommander
    {
        #region 퍼블릭

        /// <summary>
        /// E Hentai Magic Number를 이용해 작품 정보를 가져옵니다.
        /// </summary>
        /// <param name="magic_number"></param>
        /// <returns></returns>
        public static HArticleModel? GetArticleData(int magic_number)
        {
            string magic = magic_number.ToString();

            Monitor.Instance.Push($"[HCommander] [1] {magic}");

            //
            // 1. 히토미 데이터로 찾기
            //
            var search_hitomi = HitomiLegalize.GetMetadataFromMagic(magic);
            if (search_hitomi.HasValue)
            {
                // 히토미 데이터가 존재한다면 데이터의 유효 여부를 판단한다.
                try
                {
                    var url = $"https://hitomi.la/galleries/{magic}.html";
                    var request = WebRequest.Create(url);
                    using (var response = request.GetResponse())
                    {
                        using (var responseStream = response.GetResponseStream())
                        {
                            // 최종 승인
                            return ConvertTo(search_hitomi.Value, url);
                        }
                    }
                }
                catch
                {
                }
            }

            Monitor.Instance.Push($"[HCommander] [2] {magic}");

            //
            // 2. Hiyobi를 이용해 탐색한다
            //
            if (search_hitomi.HasValue && search_hitomi.Value.Language == "korean")
            {
                try
                {
                    var html = NetCommon.DownloadString(HiyobiCommon.GetInfoAddress(magic));
                    var article = HiyobiParser.ParseGalleryConents(html);
                    return ConvertTo(article, HiyobiCommon.GetInfoAddress(magic));
                }
                catch
                {
                }
            }

            Monitor.Instance.Push($"[HCommander] [3] {magic}");
            
            //
            // 9.3/4 샰쮘뽣?뛤3쇼뵀?gVA덲탭k융뷠킢쪳1SPS?XF퍵8C샜쁬
            //
            var f = ExHentaiData.data.AsParallel().Where(x => (x >> 40) == magic_number).ToList();
            if (f.Count > 0)
            {
                try
                {
                    var url = $"https://e-hentai.org/g/{magic}/{f[0] ^ 1L * magic_number << 40:x}/";
                    var html2 = NetCommon.DownloadExHentaiString(url);
                    var article = EHentaiParser.ParseArticleData(html2);
                    return ConvertTo(article, url);
                }
                catch
                {
                    var url = $"https://exhentai.org/g/{magic}/{f[0] ^ 1L * magic_number << 40:x}/";
                    var html2 = NetCommon.DownloadExHentaiString(url);
                    var article = ExHentaiParser.ParseArticleData(html2);
                    return ConvertTo(article, url);
                }
            }

            //
            // 3. GalleryBlock을 이용해 제목을 가져온다.
            //
            string title = "";
            try
            {
                var html = NetCommon.DownloadString($"{HitomiCommon.HitomiGalleryBlock}{magic}.html");
                var article = HitomiParser.ParseGalleryBlock(html);
                title = article.Title;
            }
            catch
            {
                Monitor.Instance.Push($"[HCommander] [0] {magic}");
                return null;
            }

            //
            // 4. 'Show Expunged Galleries' 옵션을 이용해 Ex-Hentai에서 검색한 후 정보를 가져온다.
            //
            try
            {
                var html = NetCommon.DownloadExHentaiString($"https://exhentai.org/?f_doujinshi=1&f_manga=1&f_artistcg=1&f_gamecg=1&f_western=1&f_non-h=1&f_imageset=1&f_cosplay=1&f_asianporn=1&f_misc=1&f_search={title}&page=0&f_apply=Apply+Filter&advsearch=1&f_sname=on&f_stags=on&f_sh=on&f_srdd=2");

                if (html.Contains($"/{magic}/"))
                {
                    var url = Regex.Match(html, $"(https://exhentai.org/g/{magic}/\\w+/)").Value;
                    var html2 = NetCommon.DownloadExHentaiString(url);
                    var article = ExHentaiParser.ParseArticleData(html2);
                    return ConvertTo(article, url);
                }
            }
            catch
            { }

            Monitor.Instance.Push($"[HCommander] [0] {magic}");
            return null;
        }
        
        /// <summary>
        /// 다운로드해야할 이미지 목록을 가져옵니다.
        /// </summary>
        /// <param name="article"></param>
        /// <returns></returns>
        public static string[] GetDownloadImageAddress(HArticleModel article)
        {
            return null;
        }
        
        #endregion

        #region 프라이빗 프리베이트

        private static HArticleModel ConvertTo(HitomiMetadata metadata, string url)
        {
            var article = new HArticleModel();
            article.ArticleType = HArticleType.Hitomi;
            article.URL = url;
            article.artist = metadata.Artists;
            article.group = metadata.Groups;
            article.parody = metadata.Parodies;
            article.misc = metadata.Tags;
            article.character = metadata.Characters;
            article.Language = metadata.Language;
            article.Title = metadata.Name;
            article.Type = metadata.Type;
            article.Magic = metadata.ID.ToString();
            return article;
        }

        private static HArticleModel ConvertTo(HitomiArticle article, string url)
        {
            return ConvertTo(HitomiLegalize.ArticleToMetadata(article), url);
        }

        private static HArticleModel ConvertTo(HiyobiArticle data, string url)
        {
            var article = new HArticleModel();
            article.ArticleType = HArticleType.Hiyobi;
            article.URL = url;
            article.artist = data.Artists;
            article.group = data.Groups;
            article.parody = data.Series;
            article.misc = data.Tags;
            article.character = data.Characters;
            article.Language = "korean";
            article.Title = data.Title;
            article.Type = data.Type;
            article.Magic = data.Magic;
            return article;
        }

        private static HArticleModel ConvertTo(EHentaiArticle data, string url)
        {
            var article = new HArticleModel();
            article.ArticleType = HArticleType.EXHentai;
            article.URL = url;
            article.Thumbnail = data.Thumbnail;
            article.Title = data.Title;
            article.SubTitle = data.SubTitle;
            article.Type = data.Type;
            article.Uploader = data.Uploader;
            article.Posted = data.Posted;
            article.Parent = data.Parent;
            article.Visible = data.Visible;
            article.Language = data.Language;
            article.FileSize = data.FileSize;
            article.Length = data.Length;
            article.Favorited = data.Favorited;
            article.reclass = data.reclass;
            article.language = data.language;
            article.group = data.group;
            article.parody = data.parody;
            article.character = data.character;
            article.artist = data.artist;
            article.male = data.male;
            article.female = data.female;
            article.misc = data.misc;
            return article;
        }

        #endregion
    }
}
