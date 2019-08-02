/***

   Copyright (C) 2018-2019. dc-koromo. All Rights Reserved.

   Author: Koromo Copy Developer

***/

using MediaDevices;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Koromo_Copy.Devices
{
    public class DeviceManager
    {
        public static void EnumerateDevice()
        {
            var devices = MediaDevice.GetDevices();
        }
    }
}
