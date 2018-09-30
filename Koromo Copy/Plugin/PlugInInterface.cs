/***

   Copyright (C) 2018. dc-koromo. All Rights Reserved.

   Author: Koromo Copy Developer

***/

using Hik.Sps;
using Koromo_Copy.Interface;

namespace Koromo_Copy.Plugin
{
    /// <summary>
    /// 코로모 카피가 제공하는 플러그인 전용 서비스입니다.
    /// </summary>
    public interface KoromoCopyPlugInBasedApplication : IPlugInBasedApplication
    {
        void Send(string message, bool err = true);
    }

    public enum KoromoCopyPlugInType
    {
        /// <summary>
        /// 아무동작도 하지않는 플러그인
        /// </summary>
        None,

        /// <summary>
        /// 다운로드 작업을 수행하는 플러그인
        /// </summary>
        Download,

        /// <summary>
        /// 유틸리티 플러그인입니다.
        /// </summary>
        Utility,

        /// <summary>
        /// 코로모 카피의 기능을 변경하는데 사용할 플러그인입니다.
        /// </summary>
        Helper,
    }

    /// <summary>
    /// 플러그인이 구현해야할 정보입니다.
    /// </summary>
    public interface KoromoCopyPlugIn : IPlugIn
    {
        KoromoCopyPlugInType Type { get; }
    }

    /// <summary>
    /// 다운로드 플러그인을 만들때 구현해야 할 정보들입니다.
    /// </summary>
    public abstract class DownloadPlugIn : KoromoCopyPlugIn
    {
        public KoromoCopyPlugInType Type { get; } = KoromoCopyPlugInType.Download;
        public abstract string Name { get; }

        /// <summary>
        /// 플러그인에게 전달할 정보입니다.
        /// </summary>
        /// <param name="user_input"></param>
        public abstract void Send(string user_input);

        /// <summary>
        /// 작품 정보를 가져옵니다.
        /// </summary>
        /// <returns></returns>
        public abstract IArticle GetArticle();

        /// <summary>
        /// 이미지 링크 정보가 포함된 작품 정보를 가져옵니다.
        /// </summary>
        /// <returns></returns>
        public abstract IArticle GetImageLink();
    }

    /*
    [PlugIn("Division")]
    public class DivisionOperation : PlugIn<ICalculatorApplication>, ICalculatorOperationPlugIn
    {
        public string OperationSign
        {
            get { return "/"; }
        }

        public double DoOperation(double number1, double number2)
        {
            if (number2 == 0.0)
            {
                Application.ApplicationProxy.ShowMessage("Second number can not be zero in division!");
                return 0.0;
            }

            return number1 / number2;
        }
    }
     */

    /// <summary>
    /// 유틸리티 플러그인을 만들때 구현해야 할 정보들입니다.
    /// </summary>
    public abstract class UtilityPlugIn : KoromoCopyPlugIn
    {
        public KoromoCopyPlugInType Type { get; } = KoromoCopyPlugInType.Utility;
        public abstract string Name { get; }
    }
    
}
