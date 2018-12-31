/***

   Copyright (C) 2018-2019. dc-koromo. All Rights Reserved.
   
   Author: Koromo Copy Developer

***/

using Koromo_Copy;
using Koromo_Copy.Console;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;

namespace Koromo_Copy_UX3.Domain
{
    static class GlobalImpl
    {
        public static void InitGlobal()
        {
            Global.UXInvoke = (Action x) =>
            {
                Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Normal,
                    new Action(() =>
                    {
                        x();
                    }));
            };
            Global.UXWaitInvoke = async (Action x) =>
            {
                await Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Normal,
                    new Action(() =>
                    {
                        x();
                    }));
            };
            Global.ShowArtistView = (string x) =>
            {
                Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Normal,
                    new Action(() =>
                    {
                        (new ArtistViewerWindow(x)).Show();
                    }));
            };
            InternalConsole.get_windows = async () =>
            {
                object[] result = null;

                await Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Normal,
                    new Action(() =>
                    {
                        try
                        {
                            result = App.Current.Windows.OfType<Window>().ToArray();
                        }
                        catch (Exception e)
                        {
                            Koromo_Copy.Console.Console.Instance.WriteLine(e.Message);
                            Koromo_Copy.Console.Console.Instance.WriteLine(e.StackTrace);
                        }
                    }));

                return result;
            };
            InternalConsole.get_window = async (string name) =>
            {
                object result = null;

                await Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Normal,
                    new Action(() =>
                    {
                        try
                        {
                            result = App.Current.Windows.OfType<Window>().Where(x => x.GetType().Name == name).ElementAt(0);
                        }
                        catch (Exception e)
                        {
                            Koromo_Copy.Console.Console.Instance.WriteLine(e.Message);
                            Koromo_Copy.Console.Console.Instance.WriteLine(e.StackTrace);
                        }
                    }));

                return result;
            };
        }
    }
}
