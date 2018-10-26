/***

   Copyright (C) 2018. dc-koromo. All Rights Reserved.

   Author: Koromo Copy Developer

***/

using Koromo_Copy.Interface;
using System.Threading.Tasks;

namespace Koromo_Copy.Component
{
    public interface IDispatchable
    {
        Task<IArticle> Collect(string uri);
    }
}
