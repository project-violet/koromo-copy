/***

   Copyright (C) 2018-2019. dc-koromo. All Rights Reserved.
   
   Author: Koromo Copy Developer

***/

using DCGallery;
using Koromo_Copy.Console;
using Koromo_Copy.Interface;
using Koromo_Copy_UX.Controls;
using Koromo_Copy_UX.Utility;
using Koromo_Copy_UX.Utility.ZipArtists;
using System.Windows;

namespace Koromo_Copy_UX.Domain
{
    public class UXConsoleOption : IConsoleOption
    {
        [CommandLine("--help", CommandType.OPTION, Default = true)]
        public bool Help;

        [CommandLine("-window", CommandType.ARGUMENTS, Help = "use -window <Code>")]
        public string[] Window;

        [CommandLine("-cmd", CommandType.ARGUMENTS, Help = "use -cmd <Code>")]
        public string[] Command;
    }
    
    public class UXConsole : ILazy<UXConsole>, IConsole
    {
        public static void Register()
        {
            Console.Instance.redirections.Add("ux", new UXConsole());
        }

        static bool Redirect(string[] arguments, string contents)
        {
            UXConsoleOption option = CommandLineParser<UXConsoleOption>.Parse(arguments);

            if (option.Error)
            {
                Console.Instance.WriteLine(option.ErrorMessage);
                if (option.HelpMessage != null)
                    Console.Instance.WriteLine(option.HelpMessage);
                return false;
            }
            else if (option.Help)
            {
                PrintHelp();
            }
            else if (option.Window != null)
            {
                ProcessWindow(option.Window);
            }
            else if (option.Command != null)
            {
                ProcessCommand(option.Command);
            }

            return true;
        }

        bool IConsole.Redirect(string[] arguments, string contents)
        {
            return Redirect(arguments, contents);
        }

        static void PrintHelp()
        {
            Console.Instance.WriteLine(
                "UX Console Core\r\n" +
                "\r\n" +
                " -window <Code> : Open specific window.\r\n"
                );
        }

        /// <summary>
        /// 새로운 창을 실행합니다.
        /// </summary>
        /// <param name="args"></param>
        static void ProcessWindow(string[] args)
        {
            switch (args[0])
            {
                case "artist_viewer":

                    Application.Current.Dispatcher.BeginInvoke(new System.Action(
                    delegate
                    {
                        ArtistViewerWindow avw = new ArtistViewerWindow();
                        avw.Show();
                    }));
                    break;

                case "test":

                    Application.Current.Dispatcher.BeginInvoke(new System.Action(
                    delegate
                    {
                        TestWindow tw = new TestWindow();
                        tw.Show();
                    }));
                    break;

                case "finder":

                    Application.Current.Dispatcher.BeginInvoke(new System.Action(
                    delegate
                    {
                        FinderWindow fw = new FinderWindow();
                        fw.Show();
                    }));
                    break;

                case "article_info":

                    Application.Current.Dispatcher.BeginInvoke(new System.Action(
                    delegate
                    {
                        ArticleInfoWindow aiw = new ArticleInfoWindow();
                        aiw.Show();
                    }));
                    break;

                case "patch_note":

                    Application.Current.Dispatcher.BeginInvoke(new System.Action(
                    delegate
                    {
                        PatchNoteWindow pnw = new PatchNoteWindow();
                        pnw.Show();
                    }));
                    break;

                case "car":

                    Application.Current.Dispatcher.BeginInvoke(new System.Action(
                    delegate
                    {
                        CustomArtistsRecommendWindow carw = new CustomArtistsRecommendWindow();
                        carw.Show();
                    }));
                    break;

                case "zip_viewer":

                    Application.Current.Dispatcher.BeginInvoke(new System.Action(
                    delegate
                    {
                        ZipViewer zv = new ZipViewer();
                        zv.Show();
                    }));
                    break;

                case "series_manager":

                    Application.Current.Dispatcher.BeginInvoke(new System.Action(
                    delegate
                    {
                        SeriesManager sm = new SeriesManager();
                        sm.Show();
                    }));
                    break;

                case "crawler":

                    Application.Current.Dispatcher.BeginInvoke(new System.Action(
                    delegate
                    {
                        MangaCrawler mc = new MangaCrawler();
                        mc.Show();
                    }));
                    break;

                case "zip-listing":

                    Application.Current.Dispatcher.BeginInvoke(new System.Action(
                    delegate
                    {
                        ZipListing zl = new ZipListing();
                        zl.Show();
                    }));
                    break;

                case "cc":

                    Application.Current.Dispatcher.BeginInvoke(new System.Action(
                    delegate
                    {
                        CustomCrawler cc = new CustomCrawler();
                        cc.Show();
                    }));
                    break;

                case "editor":

                    Application.Current.Dispatcher.BeginInvoke(new System.Action(
                    delegate
                    {
                        ScriptEditor ed = new ScriptEditor();
                        ed.Show();
                    }));
                    break;

                case "elo":

                    Application.Current.Dispatcher.BeginInvoke(new System.Action(
                    delegate
                    {
                        RankSimulator rs = new RankSimulator();
                        rs.Show();
                    }));
                    break;

                case "ft":

                    Application.Current.Dispatcher.BeginInvoke(new System.Action(
                    delegate
                    {
                        ForbiddenTester ft = new ForbiddenTester();
                        ft.Show();
                    }));
                    break;

                case "zip-artists":

                    Application.Current.Dispatcher.BeginInvoke(new System.Action(
                    delegate
                    {
                        ZipArtists za = new ZipArtists();
                        za.Show();
                    }));
                    break;

                case "dctools":

                    Application.Current.Dispatcher.BeginInvoke(new System.Action(
                    delegate
                    {
                        DCTools dct = new DCTools();
                        dct.Show();
                    }));
                    break;

                case "gex":

                    Application.Current.Dispatcher.BeginInvoke(new System.Action(
                    delegate
                    {
                        GalleryExplorer ge = new GalleryExplorer();
                        ge.Show();
                    }));
                    break;

                case "valid":

                    Application.Current.Dispatcher.BeginInvoke(new System.Action(
                    delegate
                    {
                        ZipIntegrity zi = new ZipIntegrity();
                        zi.Show();
                    }));
                    break;

                default:
                    Console.Instance.WriteLine($"'{args[0]}' window is not found.");
                    break;
            }
        }

        static void ProcessCommand(string[] args)
        {
            switch (args[0])
            {
                case "1":

                    Application.Current.Dispatcher.BeginInvoke(new System.Action(
                    delegate
                    {
                        SettingWrap.Instance.SearchSpaceWheelSpeed = 0.1;
                    }));
                    break;
            }
        }
    }
}
