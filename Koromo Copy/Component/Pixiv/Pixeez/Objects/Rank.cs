using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pixeez.Objects
{
    public class Rank
    {

        [JsonProperty("content")]
        public string Content { get; set; }

        [JsonProperty("mode")]
        public string Mode { get; set; }

        [JsonProperty("date")]
        public DateTimeOffset Date { get; set; }

        [JsonProperty("works")]
        public IList<RankWork> Works { get; set; }
    }

    public class RankWork
    {

        [JsonProperty("rank")]
        public int? Rank { get; set; }

        [JsonProperty("previous_rank")]
        public int? PreviousRank { get; set; }

        [JsonProperty("work")]
        public Work Work { get; set; }
    }
}
