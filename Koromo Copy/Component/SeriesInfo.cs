/***

   Copyright (C) 2018-2019. dc-koromo. All Rights Reserved.

   Author: Koromo Copy Developer

***/

using Koromo_Copy.Component.Hiyobi;
using Koromo_Copy.Component.Manazero;
using Koromo_Copy.Component.Mangashow;
using Koromo_Copy.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Koromo_Copy.Component
{
    public class SeriesInfo : ILazy<SeriesInfo>
    {
        List<IManager> managers;

        public SeriesInfo()
        {
            managers = new List<IManager>();
            managers.Add(MangashowmeManager.Instance);
            managers.Add(HiyobiNonHManager.Instance);
            managers.Add(ManazeroManager.Instance);
        }

        /// <summary>
        /// URL 형식을 이용하여 매니져를 선택합니다.
        /// </summary>
        /// <param name="url"></param>
        /// <returns></returns>
        public IManager SelectManager(string url)
        {
            foreach (var manager in managers)
            {
                if (manager.SpecifyUrl(url))
                    return manager;
            }
            return null;
        }
    }
}
