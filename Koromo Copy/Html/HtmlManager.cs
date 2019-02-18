/***

   Copyright (C) 2018-2019. dc-koromo. All Rights Reserved.

   Author: Koromo Copy Developer

***/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Koromo_Copy.Html
{
    public class HtmlManager
    {
        public string Title { get; set; }
        public string Body { get; set; }

        public string getHtml()
        {
            return get_html();
        }

        private string get_html()
        {
            var builder = new StringBuilder();
            builder.Append("<html lang=\"ko\">\r\n");
            builder.Append(get_head());
            builder.Append(get_body());
            builder.Append("</html>\r\n");
            return builder.ToString();
        }

        #region HEAD

        private string get_head()
        {
            var builder = new StringBuilder();
            builder.Append("<head>\r\n");
            builder.Append(get_title());
            builder.Append("</head>\r\n");
            return builder.ToString();
        }

        private string get_title()
        {
            var builder = new StringBuilder();
            builder.Append("<meta charset=\"utf-8\">\r\n");
            builder.Append("<title>\r\n");
            builder.Append(Title + "\r\n");
            builder.Append("</title>\r\n");
            return builder.ToString();
        }

        #endregion

        #region BODY

        private string get_body()
        {
            var builder = new StringBuilder();
            builder.Append("<body>\r\n");
            builder.Append(Body);
            builder.Append("</body>\r\n");
            return builder.ToString();
        }
        
        #endregion
    }
}
