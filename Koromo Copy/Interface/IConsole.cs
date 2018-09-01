/***

   Copyright (C) 2018. dc-koromo. All Rights Reserved.

   Author: Koromo Copy Developer

***/

namespace Koromo_Copy.Interface
{
    /// <summary>
    /// 콘솔 최상위 명령의 인터페이스입니다.
    /// </summary>
    public interface IConsole
    {
        void Redirect(string[] arguments);
    }

    /// <summary>
    /// 반드시 포함되어야하는 콘솔 옵션입니다.
    /// </summary>
    public class IConsoleOption
    {
        public bool Error;
        public string ErrorMessage;
    }
}
