/***

   Copyright (C) 2018-2019. dc-koromo. All Rights Reserved.

   Author: Koromo Copy Developer

***/

using Koromo_Copy.Interface;
using Pixeez;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Koromo_Copy.Component.Pixiv
{
    /// <summary>
    /// 픽시브 도구 클래스입니다.
    /// </summary>
    public class PixivTool : ILazy<PixivTool>
    {
        Tokens token;
        public bool IsLogin { get; private set; }

        public string GetAccessToken()
        {
            return token.AccessToken;
        }

        /// <summary>
        /// 로그인을 수행합니다.
        /// </summary>
        /// <param name="id"></param>
        /// <param name="pw"></param>
        public async Task Login(string id, string pw)
        {
            token = await Auth.AuthorizeAsync(id, pw);
            IsLogin = true;
        }

        /// <summary>
        /// 유저 정보를 가져옵니다.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public async Task<string> GetUserAsync(string id)
        {
            var user = await token.GetUsersAsync(Convert.ToInt32(id));
            return $"{user[0].Name} ({user[0].Account})";
        }

        /// <summary>
        /// 다운로드 이미지 링크를 가져옵니다.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public async Task<List<string>> GetDownloadUrlsAsync(string id)
        {
            var works = await token.GetUsersWorksAsync(Convert.ToInt32(id), 1, 10000000);
            return works.Select(x => x.ImageUrls.Large).ToList();
        }
    }
}
