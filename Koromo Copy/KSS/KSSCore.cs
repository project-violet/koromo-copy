/***

   Copyright (C) 2018-2019. dc-koromo. All Rights Reserved.

   Author: Koromo Copy Developer

***/

using Koromo_Copy.Interface;
using Quobject.SocketIoClientDotNet.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Koromo_Copy.KSS
{
    public class KSSCore : ILazy<KSSCore>
    {
        public void Start()
        {
#if !DEBUG
            if (Settings.Instance.KSS.AcceptTerms)
#endif
            {
                if (socket == null)
                {
                    var ping = new System.Net.NetworkInformation.Ping();
                    var result = ping.Send("koromo.duckdns.org");
                    if (result.Status != System.Net.NetworkInformation.IPStatus.Success)
                        return;

                    socket = IO.Socket(KSSCommon.URL);
                    
                    // 동시 접속자
                    socket.On("hconn", (data) =>
                    {
                        OnHConnReceive?.Invoke(null, (int)data);
                    });

                    // 누적 접속자
                    socket.On("aconn", (data) =>
                    {
                        OnAConnReceive?.Invoke(null, (int)data);
                    });

                    // 내 아이피 물어보기
                    socket.On("myip", (data) =>
                    {
                        Console.Console.Instance.WriteLine(data);
                    });

                    // 리폿 성공여부 묻기
                    socket.On("report", (data) =>
                    {
                        OnReportReceive?.Invoke(null, (int)data);
                    });

                    Opened = true;
                }
            }
        }
        
        public void Stop() => socket.Disconnect();
        public bool Opened { get; set; }

        public void MyIP() => socket.Emit("myip");

        public Socket socket;

        public event EventHandler<int> OnReportReceive;
        public event EventHandler<int> OnHConnReceive;
        public event EventHandler<int> OnAConnReceive;
    }
}
