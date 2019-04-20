/***

   Copyright (C) 2018-2019. dc-koromo. All Rights Reserved.

   Author: Koromo Copy Developer

***/

using Hitomi_Copy_3;
using Hitomi_Copy_3._403;

namespace Koromo_Copy.Utility
{
    public class UtilityDelegator
    {
        public static void Run(string toch)
        {
            switch (toch)
            {
                case "HitomiExplorer":
                    (new HitomiExplorer()).Show();
                    break;
                case "FsEnumerator":
                    (new FsEnumerator()).Show();
                    break;
                case "RelatedTagsTest":
                    (new RelatedTagsTest()).Show();
                    break;
                case "StringTools":
                    (new StringTools()).Show();
                    break;
                case "GalleryBlockTester":
                    (new GalleryBlockTester()).Show();
                    break;
                case "Record":
                    (new Record()).Show();
                    break;
            }
        }
    }
}
