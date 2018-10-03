/***

   Copyright (C) 2018. dc-koromo. All Rights Reserved.
   
   Author: Koromo Copy Developer

***/

using Koromo_Copy.Interface;
using System;
using System.Collections.Generic;

namespace Koromo_Copy.Fs
{
    public enum JournalType
    {
        None,
        Rename,
        Copy,
        Move
    }

    /// <summary>
    /// 파일의 변경사항을 추적, 기록하는 클래스입니다.
    /// </summary>
    public class Journal : ILazy<Journal>
    {
        Queue<Tuple<JournalType, string, string>> journal_log;

        public bool Verify()
        {
            return true;
        }

    }
}
