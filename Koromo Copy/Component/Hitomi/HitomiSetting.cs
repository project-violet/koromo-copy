/***

   Copyright (C) 2018-2019. dc-koromo. All Rights Reserved.
   
   Author: Koromo Copy Developer

***/

using Newtonsoft.Json;

namespace Koromo_Copy.Component.Hitomi
{
    public class HitomiSetting
    {
        [JsonProperty]
        public string Path;

        /// <summary>
        /// 검색시 사용할 언어입니다.
        /// ALL 입력시 모든 언어를 사용하며, 언어가 없는 작품의 경우엔 N/A로 설정됩니다.
        /// </summary>
        [JsonProperty]
        public string Language;

        /// <summary>
        /// 프로그램 시작시 자동으로 동기화를 진행할지의 여부를 설정합니다.
        /// </summary>
        [JsonProperty]
        public bool AutoSync;

        /// <summary>
        /// 특정 언어를 가진 작품만 메모리에 남겨둠으로써 메모리를 절약합니다.
        /// </summary>
        [JsonProperty]
        public bool UsingOptimization;

        /// <summary>
        /// 퍼지 알고리즘을 이용해 Auto complete 목록을 가져올지의 여부를 설정합니다.
        /// </summary>
        [JsonProperty]
        public bool UsingFuzzy;

        /// <summary>
        /// 고급 검색 기능을 사용할지의 여부입니다.
        /// </summary>
        [JsonProperty]
        public bool UsingAdvancedSearch;

        /// <summary>
        /// 고급 검색시 설정된 언어를 기본으로 추가합니다.
        /// </summary>
        [JsonProperty]
        public bool UsingSettingLanguageWhenAdvanceSearch;

        /// <summary>
        /// Article 다운로드 폴더에 작품의 정보를 담은 json파일을 남깁니다.
        /// </summary>
        [JsonProperty]
        public bool SaveJsonFile;

        /// <summary>
        /// 작가추천에서 중복된 Article를 표시하지않도록 설정하는 고유값입니다.
        /// </summary>
        [JsonProperty]
        public int TextMatchingAccuracy;

        /// <summary>
        /// 스크롤 당 몇 개의 작가추천 목록을 표시할지 설정합니다.
        /// </summary>
        [JsonProperty]
        public int RecommendPerScroll;

        /// <summary>
        /// 작가창에서 비슷한 작가를 더이상 사용하지 않습니다.
        /// </summary>
        [JsonProperty]
        public bool DisableArtistViewToast;

        /// <summary>
        /// 더이상 마지막 다운로드 날짜를 표시하지 않습니다.
        /// </summary>
        [JsonProperty]
        public bool DisableArtistLastestDownloadDate;

        /// <summary>
        /// 원래 제목을 사용합니다.
        /// </summary>
        [JsonProperty]
        public bool UsingOriginalTitle;

        [JsonProperty]
        public string[] CustomAutoComplete;
        [JsonProperty]
        public string[] ExclusiveTag;
    }

    /// <summary>
    /// 히토미 분석에 관한 설정입니다.
    /// </summary>
    public class HitomiAnalysisSetting
    {
        /// <summary>
        /// 작가추천 시 female:, male: 태그만 사용합니다.
        /// </summary>
        [JsonProperty]
        public bool UsingOnlyFMTagsOnAnalysis;

        [JsonProperty]
        public bool UsingXiAanlysis;

        [JsonProperty]
        public bool UsingRMSAanlysis;
        
        [JsonProperty]
        public bool UsingCosineAnalysis;

        /// <summary>
        /// 작가추천 목록 생성시 해당 작가의 Article 수를 곱하지 않습니다.
        /// 이 설정은 단순한 작가추천 결과를 제공합니다.
        /// </summary>
        [JsonProperty]
        public bool RecommendNMultipleWithLength;

        /// <summary>
        /// 작가추천시 모든 언어를 기반으로 목록을 생성합니다.
        /// </summary>
        [JsonProperty]
        public bool RecommendLanguageALL;
    }
}
