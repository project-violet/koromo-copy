using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pixeez.Objects
{
    public class ImageUrls
    {
        [JsonProperty("px_128x128")]
        public string Px128x128 { get; set; }

        [JsonProperty("small")]
        public string Small { get; set; }

        [JsonProperty("medium")]
        public string Medium { get; set; }

        [JsonProperty("large")]
        public string Large { get; set; }

        [JsonProperty("px_480mw")]
        public string Px480mw { get; set; }
    }

    public class FavoritedCount
    {
        [JsonProperty("public")]
        public int? Public { get; set; }

        [JsonProperty("private")]
        public int? Private { get; set; }
    }

    public class WorkStats
    {
        [JsonProperty("scored_count")]
        public int? ScoredCount { get; set; }

        [JsonProperty("score")]
        public int? Score { get; set; }

        [JsonProperty("views_count")]
        public int? ViewsCount { get; set; }

        [JsonProperty("favorited_count")]
        public FavoritedCount FavoritedCount { get; set; }

        [JsonProperty("commented_count")]
        public int? CommentedCount { get; set; }
    }

    public class Page
    {

        [JsonProperty("image_urls")]
        public ImageUrls ImageUrls { get; set; }
    }

    public class Metadata
    {

        [JsonProperty("pages")]
        public IList<Page> Pages { get; set; }
    }

    public class Work
    {

        [JsonProperty("id")]
        public long? Id { get; set; }

        [JsonProperty("title")]
        public string Title { get; set; }

        [JsonProperty("caption")]
        public string Caption { get; set; }

        [JsonProperty("tags")]
        public IList<string> Tags { get; set; }

        [JsonProperty("tools")]
        public IList<string> Tools { get; set; }

        [JsonProperty("image_urls")]
        public ImageUrls ImageUrls { get; set; }

        [JsonProperty("width")]
        public int? Width { get; set; }

        [JsonProperty("height")]
        public int? Height { get; set; }

        [JsonProperty("stats")]
        public WorkStats Stats { get; set; }

        [JsonProperty("publicity")]
        public int? Publicity { get; set; }

        [JsonProperty("age_limit")]
        public string AgeLimit { get; set; }

        [JsonProperty("created_time")]
        public DateTimeOffset CreatedTime { get; set; }

        [JsonProperty("reuploaded_time")]
        public string ReuploadedTime { get; set; }

        [JsonProperty("user")]
        public User User { get; set; }

        [JsonProperty("is_manga")]
        public bool? IsManga { get; set; }

        [JsonProperty("is_liked")]
        public bool? IsLiked { get; set; }

        [JsonProperty("favorite_id")]
        public long? FavoriteId { get; set; }

        [JsonProperty("page_count")]
        public int? PageCount { get; set; }

        [JsonProperty("book_style")]
        public string BookStyle { get; set; }

        [JsonProperty("type")]
        public string Type { get; set; }

        [JsonProperty("metadata")]
        public Metadata Metadata { get; set; }

        [JsonProperty("content_type")]
        public string ContentType { get; set; }
    }
}
