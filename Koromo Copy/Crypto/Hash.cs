/***

   Copyright (C) 2018-2019. dc-koromo. All Rights Reserved.

   Author: Koromo Copy Developer

***/

using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace Koromo_Copy.Crypto
{
    public static class Hash
    {
        public static string GetFileHash(this string file)
        {
            using (FileStream stream = File.OpenRead(file))
            {
                SHA512Managed sha = new SHA512Managed();
                byte[] hash = sha.ComputeHash(stream);
                return BitConverter.ToString(hash).Replace("-", String.Empty);
            }
        }

        public static string GetHashSHA1(this string str)
        {
            SHA1Managed sha = new SHA1Managed();
            byte[] hash = sha.ComputeHash(Encoding.UTF8.GetBytes(str));
            return BitConverter.ToString(hash).Replace("-", String.Empty);
        }
    }
}
