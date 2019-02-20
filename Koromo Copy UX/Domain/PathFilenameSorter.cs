/***

   Copyright (C) 2018-2019. dc-koromo. All Rights Reserved.
   
   Author: Koromo Copy Developer

***/

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Koromo_Copy_UX.Domain
{
    public class PathFilenameSorter
    {
        public static string[] Sort(string[] paths)
        {
            List<string> result = new List<string>();
            int count = 1;
            int pad = paths.Length > 1000 ? 4 : 3;
            foreach (var path in paths)
            {
                result.Add(Path.Combine(Path.GetDirectoryName(path), count.ToString().PadLeft(pad, '0') + Path.GetExtension(path)));
                count++;
            }
            return result.ToArray();
        }

        public static string[] SortOnlyFilename(string[] filenames)
        {
            List<string> result = new List<string>();
            int count = 1;
            int pad = filenames.Length > 1000 ? 4 : 3;
            foreach (var path in filenames)
            {
                result.Add(count.ToString().PadLeft(pad, '0') + Path.GetExtension(path));
                count++;
            }
            return result.ToArray();
        }
    }
}
