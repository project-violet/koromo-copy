/***

   Copyright (C) 2019. dc-koromo. All Rights Reserved.

   Author: Koromo Copy Developer

***/

using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Koromo_Copy.Component.DC
{
    public class DCCommon
    {
        public static DCComment GetComments(DCArticle article, string page)
        {
            var wc = Net.NetCommon.GetDefaultClient();
            wc.Headers.Add("X-Requested-With", "XMLHttpRequest");
            wc.QueryString.Add("id", article.OriginalGalleryName);
            wc.QueryString.Add("no", article.Id);
            wc.QueryString.Add("cmt_id", article.OriginalGalleryName);
            wc.QueryString.Add("cmt_no", article.Id);
            wc.QueryString.Add("e_s_n_o", article.ESNO);
            wc.QueryString.Add("comment_page", page);
            return JsonConvert.DeserializeObject<DCComment>(Encoding.UTF8.GetString(wc.UploadValues("https://gall.dcinside.com/board/comment/", "POST", wc.QueryString)));
        }
    }
}
