/***

   Copyright (C) 2018-2019. dc-koromo. All Rights Reserved.
   
   Author: Koromo Copy Developer

***/

using Koromo_Copy.Interface;
using Newtonsoft.Json;
using System;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

namespace Koromo_Copy
{
    /// <summary>
    /// 모든 IO와 작업 현황을 보고합니다.
    /// </summary>
    public class Monitor : ILazy<Monitor>
    {
        /// <summary>
        /// 오브젝트를 직렬화합니다.
        /// </summary>
        /// <param name="toSerialize"></param>
        /// <returns></returns>
        public static string SerializeObject(object toSerialize)
        {
            try
            {
                return JsonConvert.SerializeObject(toSerialize, Formatting.Indented, new JsonSerializerSettings
                {
                    ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore
                });
            }
            catch
            {
                try
                {
                    XmlSerializer xmlSerializer = new XmlSerializer(toSerialize.GetType());

                    using (StringWriter textWriter = new StringWriter())
                    {
                        xmlSerializer.Serialize(textWriter, toSerialize);
                        return textWriter.ToString();
                    }
                }
                catch (Exception e)
                {
                    return toSerialize.ToString();
                }
            }
        }

        /// <summary>
        /// 모니터 초기화
        /// </summary>
        public Monitor()
        {
            log.CollectionChanged += Monitor_Notify;

            if (controlEnable)
                showMonitorControl();
        }

        /// <summary>
        /// 프롬프트를 초기화합니다.
        /// </summary>
        public void Start()
        {
            Console.Console.Instance.Start();
        }

        /// <summary>
        /// 처음시작시엔 디버그 모드에서만 모니터를 사용합니다.
        /// </summary>
#if DEBUG
        public bool controlEnable = true;
#else
        public bool controlEnable = false;
#endif
        public bool ControlEnable
        {
            get
            {
                return controlEnable;
            }
            set
            {
                if (value && !controlEnable)
                    showMonitorControl();
                else if (!value && controlEnable)
                    hideMonitorControl();
                controlEnable = value;
            }
        }

        /// <summary>
        /// 모든 진행사항이 기록되는 큐입니다.
        /// </summary>
        ObservableCollection<Tuple<DateTime, string, bool>> log = new ObservableCollection<Tuple<DateTime, string, bool>>();

        /// <summary>
        /// 문자열을 로그에 Push합니다.
        /// </summary>
        /// <param name="str"></param>
        public void Push(string str)
        {
            lock (log)
            {
                log.Add(Tuple.Create(DateTime.Now, str, false));
            }
        }

        /// <summary>
        /// 객체를 로그에 Push합니다.
        /// </summary>
        /// <param name="obj"></param>
        public void Push(object obj)
        {
            lock (log)
            {
                log.Add(Tuple.Create(DateTime.Now, obj.ToString(), false));
                log.Add(Tuple.Create(DateTime.Now, SerializeObject(obj), true));
            }
        }

        /// <summary>
        /// 로그를 저장합니다.
        /// </summary>
        public void Save()
        {
            CultureInfo en = new CultureInfo("en-US");
            StringBuilder build = new StringBuilder();
            log.ToList().Where(x => x != null).ToList().ForEach(x => build.Append($"[{x.Item1.ToString(en)}] {x.Item2}\r\n"));
            File.AppendAllText("log.txt", build.ToString());
        }
        
        private void Monitor_Notify(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (controlEnable)
            {
                lock (log)
                {
                    Console.Console.Instance.Push(log.Last().Item1, log.Last().Item2);
                }
            }
        }

        private void showMonitorControl()
        {
            Console.Console.Instance.Show();
        }

        private void hideMonitorControl()
        {
            Console.Console.Instance.Hide();
        }
    }
}
