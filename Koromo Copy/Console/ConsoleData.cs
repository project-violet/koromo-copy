/***

   Copyright (C) 2018-2019. dc-koromo. All Rights Reserved.

   Author: Koromo Copy Developer

***/

using Koromo_Copy.Interface;
using System.Collections.Generic;

namespace Koromo_Copy.Console
{
    /// <summary>
    /// 콘솔에서 사용하는 모든 데이터를 관리합니다.
    /// </summary>
    public class ConsoleData : ILazy<ConsoleData>
    {
        /// <summary>
        /// 사용되는 데이터가 저장되는 Dictionary입니다.
        /// </summary>
        Dictionary<string, object> data_dictionary = new Dictionary<string, object>();

        public ConsoleData()
        {
            data_dictionary.Add("grep_hitomi", @"(?<=\\)\[\d+\][^\\]+$");
        }

        /// <summary>
        /// 데이터 존재여부를 확인합니다.
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public bool Exists(string key)
        {
            return data_dictionary.ContainsKey(key);
        }
    }
}
