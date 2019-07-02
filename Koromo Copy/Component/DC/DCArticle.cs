/***

   Copyright (C) 2018-2019. dc-koromo. All Rights Reserved.

   Author: Koromo Copy Developer

***/

using Koromo_Copy.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Koromo_Copy.Component.DC
{
    public class DCArticle : IArticle
    {
        public string Id { get; set; }
        public string GalleryName { get; set; }
        public string OriginalGalleryName { get; set; }
        public string Thumbnail { get; set; }
        public string Class { get; set; }
        public string Title { get; set; }
        public string Contents { get; set; }
        public List<string> ImagesLink { get; set; }
        public List<string> FilesName { get; set; }
        public string Archive { get; set; }
        public string ESNO { get; set; }
    }

    public class DCPageArticle
    {
        public string no;
        public string classify;
        public string type;
        public string title;
        public string replay_num;
        public string nick;
        public string uid;
        public string ip;
        public bool islogined;
        public bool isfixed;
        public DateTime date;
        public string count;
        public string recommend;
    }

    public class DCGallery
    {
        public string id;
        public string name;
        public string esno;
        public string cur_page;
        public string max_page;
        public DCPageArticle[] articles;
    }

    public class DCCommentElement
    {
        public string no;
        public string parent;
        public string user_id;
        public string name;
        public string ip;
        public string reg_date;
        public string nicktype;
        public string t_ch1;
        public string t_ch2;
        public string vr_type;
        public string voice;
        public string rcnt;
        public string c_no;
        public int depth;
        public string del_yn;
        public string is_delete;
        public string memo;
        public string my_cmt;
        public string del_btn;
        public string mod_btn;
        public string a_my_cmt;
        public string reply_w;
        public string gallog_icon;
        public bool vr_player;
        public string vr_player_tag;
        public int next_type;
    }

    public class DCComment
    {
        public int total_cnt;
        public int comment_cnt;
        public DCCommentElement[] comments;
    }

    public class DCGalleryModel
    {
        public bool is_minor_gallery;
        public string gallery_id;
        public DCPageArticle[] articles;
    }
}
