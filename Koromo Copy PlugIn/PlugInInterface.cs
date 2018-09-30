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
        /// <summary>
        /// 코로모 카피로 메세지를 보냅니다.
        /// </summary>
        /// <param name="plugin"></param>
        /// <param name="message"></param>
        /// <param name="err"></param>
        void Send(KoromoCopyPlugIn plugin, string message, bool err = true);
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
    /// 아무런 목적이 정해지지않은 빈 플러그인입니다.
    /// </summary>
    public interface INonePlugin : KoromoCopyPlugIn
    {
        /// <summary>
        /// 초기화시 코로모 카피의 버전텍스트가 전달됩니다.
        /// </summary>
        /// <param name="user_input"></param>
        void Send(string user_input);
    }
    
    /// <summary>
    /// 다운로드 플러그인을 만들때 구현해야 할 정보들입니다.
    /// </summary>
    public interface IDownloadPlugIn : KoromoCopyPlugIn
    {
        /// <summary>
        /// 플러그인에게 전달할 정보입니다.
        /// </summary>
        /// <param name="user_input"></param>
        void Send(string user_input);

        /// <summary>
        /// URL의 형태가 다운로더가 제공하는 형태에 맞는지 확인합니다.
        /// </summary>
        /// <param name="url"></param>
        /// <returns></returns>
        bool SpecifyUrl(string url);

        /// <summary>
        /// 작품 정보를 가져옵니다.
        /// </summary>
        /// <returns></returns>
        IArticle GetArticle();

        /// <summary>
        /// 이미지 링크 정보가 포함된 작품 정보를 가져옵니다.
        /// </summary>
        /// <returns></returns>
        IArticle GetImageLink();
    }
    
    /// <summary>
    /// 유틸리티 플러그인을 만들때 구현해야 할 정보들입니다.
    /// </summary>
    public interface UtilityPlugIn : KoromoCopyPlugIn
    {
        /// <summary>
        /// 플러그인에게 전달할 정보입니다.
        /// </summary>
        /// <param name="user_input"></param>
        void Send(string user_input);

        /// <summary>
        /// 유틸리티 창을 보여줍니다.
        /// </summary>
        void Show();

        /// <summary>
        /// 유틸리티 창을 다이얼로그로 보여줍니다.
        /// </summary>
        void ShowDialog();

        /// <summary>
        /// 유틸리티 창을 숨깁니다.
        /// </summary>
        void Hide();

        /// <summary>
        /// 유틸리티를 닫습니다.
        /// </summary>
        void Close();
    }

    /// <summary>
    /// 헬퍼 플러그인을 만들때 구현해야 할 정보들입니다.
    /// </summary>
    public interface HelperPlugIn : KoromoCopyPlugIn
    {
    }
}
