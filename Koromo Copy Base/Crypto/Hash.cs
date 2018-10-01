/***

   Copyright (C) 2018. dc-koromo. All Rights Reserved.

   Author: Koromo Copy Developer

***/

using System;
using System.IO;
using System.Security.Cryptography;

namespace Koromo_Copy_Base.Crypto
{
    public static class Hash
    {
        public static string GetFileHash(string file)
        {
            using (FileStream stream = File.OpenRead(file))
            {
                SHA512Managed sha = new SHA512Managed();
                byte[] hash = sha.ComputeHash(stream);
                return BitConverter.ToString(hash).Replace("-", String.Empty);
            }
        }
    }
}
