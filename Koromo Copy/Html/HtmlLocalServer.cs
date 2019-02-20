/***

   Copyright (C) 2018-2019. dc-koromo. All Rights Reserved.
   
   Author: Koromo Copy Developer

***/

using Koromo_Copy.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Koromo_Copy.Html
{
    public class HtmlLocalServer : ILazy<HtmlLocalServer>
    {
        HttpListener listener;
        Thread thread;
        HtmlManager manager;

        public void Start()
        {
            listener = new HttpListener();
            manager = new HtmlManager();
            listener.Prefixes.Add("http://+:7777/");
            listener.Start();
            thread = new Thread(ResponseThread);
            thread.Start();
        }
        
        public void ResponseThread()
        {
            while (true)
            {
                HttpListenerContext context = listener.GetContext();
                
                //byte[] _responseArray = Encoding.UTF8.GetBytes("<html><head><title>Localhost server -- port 7777</title></head>" +
                //    "<body>Welcome to the <strong>Localhost server</strong> -- <em>port 7777!</em></body></html>");

                byte[] _responseArray = Encoding.UTF8.GetBytes(manager.getHtml());
                context.Response.OutputStream.Write(_responseArray, 0, _responseArray.Length);
                context.Response.KeepAlive = false;
                context.Response.Close();
            }
        }

        public void CreateImageServer(string title, string[] imgs)
        {
            //manager.Title = title;
            //
            //var builder = new StringBuilder();
            //foreach (var img in imgs)
            //{
            //    builder.Append($"<img src=\"{img}\">\r\n");
            //}
            //
            //manager.Body = builder.ToString();
        }
    }
}
