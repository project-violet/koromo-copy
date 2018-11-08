/***

   Copyright (C) 2018. dc-koromo. All Rights Reserved.
   
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

        [JsonProperty]
        public string[] CustomAutoComplete;
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
