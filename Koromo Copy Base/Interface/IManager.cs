/***

   Copyright (C) 2018-2019. dc-koromo. All Rights Reserved.
   
   Author: Koromo Copy Developer

***/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Koromo_Copy.Interface
{
    /// <summary>
    /// 인터페이스의 타입을 결정합니다.
    /// </summary>
    public enum ManagerType
    {
        /// <summary>
        /// 이미지 한 장을 다운로드할 때 사용합니다.
        /// </summary>
        SingleArticleSingleImage,

        /// <summary>
        /// 동영상 한 편을 다운로드할 때 사용합니다.
        /// </summary>
        SingleArticleSingleMovie,

        /// <summary>
        /// 작품 하나를 다운로드할 때 사용합니다.
        /// </summary>
        SingleArticleMultipleImages,
        
        /// <summary>
        /// 작품이 여러개 포함된 시리즈를 다운로드할 때 사용합니다.
        /// </summary>
        SingleSeriesMultipleArticles,
    }

    public enum ManagerEngineType
    {
        /// <summary>
        /// 모든 과정에서 WebClient를 사용하여 정보를 가져옵니다.
        /// </summary>
        None,

        /// <summary>
        /// 모든 과정에서 셀레니움 등의 드라이버를 통해 정보를 가져옵니다.
        /// </summary>
        UsingDriver,
    }

    /// <summary>
    /// 하나의 사이트를 총괄하는 인터페이스입니다.
    /// </summary>
    public interface IManager
    {
        ManagerType Type { get; }
        ManagerEngineType EngineType { get; }

        /// <summary>
        /// 매니저의 이름입니다.
        /// </summary>
        string Name { get; }
        
        /// <summary>
        /// URL의 형태가 다운로더가 제공하는 형태에 맞는지 확인합니다.
        /// </summary>
        /// <param name="url"></param>
        /// <returns></returns>
        bool SpecifyUrl(string url);

        /// <summary>
        /// 고유 WebClient가 필요하다면 해당 WebClient를 반환합니다.
        /// 그렇지않을 경우엔 null이 반환됩니다.
        /// </summary>
        /// <returns></returns>
        WebClient GetWebClient();

        /// <summary>
        /// 가장 기본적인 시리즈 정보를 가져옵니다.
        /// </summary>
        /// <param name="html"></param>
        /// <returns></returns>
        ISeries ParseSeries(string html);
        
        /// <summary>
        /// 가장 기본적인 아티클 정보를 가져옵니다.
        /// </summary>
        /// <param name="html"></param>
        /// <returns></returns>
        IArticle ParseArticle(string html);

        /// <summary>
        /// 이미지 정보를 가져옵니다.
        /// </summary>
        /// <param name="html"></param>
        /// <returns></returns>
        List<string> ParseImages(string html, IArticle article);

        /// <summary>
        /// 다운로드할 파일들이 저장될 이름을 가져옵니다. 확장자도 같이 포함해야합니다.
        /// </summary>
        /// <param name="article"></param>
        /// <returns></returns>
        List<string> GetDownloadFileNames(IArticle article);
    }
}
