/***

   Copyright (C) 2018-2019. dc-koromo. All Rights Reserved.

   Author: Koromo Copy Developer

***/

namespace Koromo_Copy.Interface
{
    /// <summary>
    /// 콘솔 최상위 명령의 인터페이스입니다.
    /// </summary>
    public interface IConsole
    {
        /// <summary>
        /// 리다이렉트 함수입니다.
        /// </summary>
        /// <param name="arguments"></param>
        /// <returns>리다이렉트가 성공적으로 수행되었는지의 여부입니다.</returns>
        bool Redirect(string[] arguments, string contents);
    }
    
    /// <summary>
    /// 반드시 포함되어야하는 콘솔 옵션입니다.
    /// </summary>
    public class IConsoleOption
    {
        public bool Error;
        public string ErrorMessage;
        public string HelpMessage;
    }
}
