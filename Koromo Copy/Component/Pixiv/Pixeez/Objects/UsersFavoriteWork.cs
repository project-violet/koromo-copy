using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pixeez.Objects
{
    public class UsersFavoriteWork
    {

        [JsonProperty("id")]
        public long? Id { get; set; }

        [JsonProperty("comment")]
        public string Comment { get; set; }

        [JsonProperty("tags")]
        public IList<string> Tags { get; set; }

        [JsonProperty("publicity")]
        public string Publicity { get; set; }

        [JsonProperty("work")]
        public Work Work { get; set; }
    }

}
