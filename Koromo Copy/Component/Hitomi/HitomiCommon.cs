/***

   Copyright (C) 2018. dc-koromo. All Rights Reserved.
   
   Author: Koromo Copy Developer

***/

using System;
using System.Linq;

namespace Koromo_Copy.Component.Hitomi
{
    /// <summary>
    /// 히토미 다운로더 구현에 필요한 모든 정보를 가지고 있습니다.
    /// </summary>
    public class HitomiCommon
    {
        /// <summary>
        /// 히토미 기본 경로입니다.
        /// </summary>
        public const string HitomiAddress = @"https://hitomi.la/";

        /// <summary>
        /// 갤러리의 주소입니다.
        /// 이 주소는 작품의 이미지 정보를 얻어오는데 사용합니다.
        /// </summary>
        public const string HitomiGalleryAddress = @"https://ltn.hitomi.la/galleries/";

        /// <summary>
        /// Gallery Block의 주소입니다.
        /// Gallery Block은 작품의 썸네일, 태그 정보, 언어 정보, 제목을 담고있습니다.
        /// </summary>
        public const string HitomiGalleryBlock = @"https://ltn.hitomi.la/galleryblock/";

        /// <summary>
        /// 히토미 Reader의 주소입니다.
        /// 이 주소는 현재 사용하지 않습니다.
        /// </summary>
        public const string HitomiReaderAddress = @"https://hitomi.la/reader/";

        /// <summary>
        /// 히토미 썸네일 경로입니다.
        /// 모든 썸네일은 이 경로를 통해 가져오게됩니다.
        /// </summary>
        public const string HitomiThumbnail = @"https://tn.hitomi.la/";
        public const string HitomiThumbnailBig = @"https://tn.hitomi.la/bigtn/";
        public const string HitomiThumbnailSmall = @"https://tn.hitomi.la/smalltn/";

        /// <summary>
        /// 다운로드할 정규화된 이미지 Url을 가져옵니다.
        /// </summary>
        /// <param name="gallery">히토미 작품의 고유번호입니다.</param>
        /// <param name="page_with_extension">이미지 파일의 이름입니다.</param>
        /// <returns></returns>
        public static string GetDownloadImageAddress(
            string gallery,
            string page_with_extension)
        {
            // download.js
            var number_of_frontends = 2;
            char subdomain = Convert.ToChar(97 + (Convert.ToInt32(gallery.Last()) % number_of_frontends));
            if (gallery.Last() == '1')
                subdomain = 'a';
            return $"https://{subdomain}a.hitomi.la/galleries/{gallery}/{page_with_extension}";
        }

        /// <summary>
        /// 이미지들의 이름이 포함된 JSon 파일의 Url을 가져옵니다.
        /// </summary>
        /// <param name="gallery"></param>
        /// <returns></returns>
        public static string GetImagesLinkAddress(string gallery)
            => $"{HitomiGalleryAddress}{gallery}.js";
    }
}
