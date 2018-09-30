/***

   Copyright (C) 2018. dc-koromo. All Rights Reserved.
   
   Author: Koromo Copy Developer

***/

namespace Koromo_Copy
{
    public class Version
    {
        public const string Name = "Koromo Copy";
        public static string Text { get; } = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString();
    }
}
