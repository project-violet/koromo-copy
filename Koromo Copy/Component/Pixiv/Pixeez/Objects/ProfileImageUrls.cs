using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pixeez.Objects
{
    public class ProfileImageUrls
    {

        [JsonProperty("px_16x16")]
        public string Px16x16 { get; set; }

        [JsonProperty("px_50x50")]
        public string Px50x50 { get; set; }

        [JsonProperty("px_170x170")]
        public string Px170x170 { get; set; }
    }
}
