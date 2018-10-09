/***

   Copyright (C) 2018. dc-koromo. All Rights Reserved.

   Author: Koromo Copy Developer

***/

using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace Koromo_Copy.Component.MM
{
    class MMParser
    {
        /// <summary>
        /// ex: https://marumaru.in/b/manga/241501
        /// </summary>
        /// <param name="html"></param>
        public static List<string> ParseManga(string html)
        {
            List<string> result = new List<string>();
            {
                Regex regex = new Regex("/archives/(.*?)\\\"");
                Match match = regex.Match(html);

                while (match.Success)
                {
                    if (match.Groups[1].Value.Trim() != "")
                        result.Add("http://wasabisyrup.com/archives/" + match.Groups[1].Value);
                    match = match.NextMatch();
                }
            }

            return result;
        }

        /// <summary>
        /// ex: https://marumaru.in/b/manga/241501
        /// </summary>
        /// <param name="html"></param>
        /// <returns></returns>
        public static string GetTitle(string html)
        {
            string title_s = Regex.Split(Regex.Split(html, @"\<title\>")[1], @"\<\/title\>")[0];
            return title_s.Remove(0, "MARUMARU - 마루마루 - ".Length);
        }

        /// <summary>
        /// ex: https://marumaru.in/b/manga/54099
        /// </summary>
        /// <param name="html"></param>
        /// <returns></returns>
        public static string GetThumbnailAddress(string html)
        {
            string thumbnail = Regex.Split(Regex.Split(html, "https://marumaru.in/quickimage/")[1], "\"")[0];
            return "https://marumaru.in/quickimage/" + thumbnail;
        }

        /// <summary>
        /// ex: http://wasabisyrup.com/archives/xnc4I45ZMRI
        /// </summary>
        /// <param name="html"></param>
        /// <returns></returns>
        public static List<string> ParseArchives(string html)
        {
            Regex regex = new Regex("(/storage/gallery/.*?)\\\"");
            Match match = regex.Match(html);
            List<string> result = new List<string>();

            while (match.Success)
            {
                result.Add(match.Groups[1].Value);
                match = match.NextMatch();
            }

            return result;
        }

        /// <summary>
        /// ex: http://wasabisyrup.com/archives/xnc4I45ZMRI
        /// </summary>
        /// <param name="html"></param>
        /// <returns></returns>
        public static string GetArchive(string html)
        {
            string title_s = Regex.Split(Regex.Split(html, @"\<title\>")[1], @"\<\/title\>")[0];
            return Regex.Replace(title_s, " \\| WasabiSyrup", "");
        }
    }
}
