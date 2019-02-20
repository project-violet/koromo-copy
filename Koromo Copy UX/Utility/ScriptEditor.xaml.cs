/***

   Copyright (C) 2018-2019. dc-koromo. All Rights Reserved.
   
   Author: Koromo Copy Developer

***/

using ICSharpCode.AvalonEdit.CodeCompletion;
using ICSharpCode.AvalonEdit.Folding;
using ICSharpCode.AvalonEdit.Highlighting;
using ICSharpCode.AvalonEdit.Indentation.CSharp;
using Koromo_Copy;
using Koromo_Copy.Script;
using Koromo_Copy.Script.SRCAL;
using Koromo_Copy_UX.Properties;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Resources;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Windows.Threading;
using System.Xml;
using static Koromo_Copy.Script.SRCAL.SRCALEngine;

namespace Koromo_Copy_UX.Utility
{
    /// <summary>
    /// ScriptEditor.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class ScriptEditor : Window
    {
        static bool init = false;

        static void inits()
        {
            if (init) return;
            init = true;
            ICSharpCode.AvalonEdit.Highlighting.Xshd.XshdSyntaxDefinition xshd;
            var v = Assembly.GetExecutingAssembly().GetManifestResourceNames();

            using (Stream s = Assembly.GetExecutingAssembly().GetManifestResourceStream(Assembly.GetExecutingAssembly().GetManifestResourceNames().Single(str => str.EndsWith("SRCAL.xshd"))))
            {
                using (XmlTextReader reader = new XmlTextReader(s))
                {
                    xshd = ICSharpCode.AvalonEdit.Highlighting.Xshd.HighlightingLoader.LoadXshd(reader);
                }
            }
            HighlightingManager.Instance.RegisterHighlighting("SRCAL", new[] { ".srcal" }, ICSharpCode.AvalonEdit.Highlighting.Xshd.HighlightingLoader.Load(xshd, HighlightingManager.Instance));
        }

        FoldingManager mgr;
        ScriptEditorBraceFolding folding;
        public ScriptEditor(string code = null)
        {
            inits();
            InitializeComponent();

            textEditor.TextArea.TextEntered += TextArea_TextEntered;
            textEditor.TextArea.TextEntering += TextArea_TextEntering;
            textEditor.PreviewMouseLeftButtonDown += TextEditor_PreviewMouseLeftButtonDown; ;

            textEditor.Options.EnableHyperlinks = false;
            textEditor.Options.HighlightCurrentLine = true;
            textEditor.Options.ConvertTabsToSpaces = true;
            mgr = FoldingManager.Install(textEditor.TextArea);
            folding = new ScriptEditorBraceFolding();
            DispatcherTimer foldingUpdateTimer = new DispatcherTimer();
            foldingUpdateTimer.Interval = TimeSpan.FromSeconds(2);
            foldingUpdateTimer.Tick += delegate { UpdateFoldings(); };
            foldingUpdateTimer.Start();

            if (code != null)
            {
                textEditor.Text = code;
            }
        }

        private void TextEditor_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            var pos = textEditor.GetPositionFromPoint(e.GetPosition(textEditor));
            if (pos.HasValue)
            {
                LC.Text = $"줄:{pos.Value.Line} 열:{pos.Value.Column}";
            }
        }
        
        CompletionWindow completionWindow;

        static Tuple<string, string>[] tokens =
        {
            Tuple.Create("$ScriptName","스크립트의 이름입니다."),
            Tuple.Create("$ScriptVersion","스크립트의 버전입니다."),
            Tuple.Create("$ScriptAuthor","스크립트의 작성자입니다."),
            Tuple.Create("$ScriptFolderName","스크립트의 기본 생성 폴더 이름입니다."),
            Tuple.Create("$ScriptRequestName","다운로드 상태에 표시될 접두사입니다."),
            Tuple.Create("$URLSpecifier","스크립트 식별용 URL입니다."),
            Tuple.Create("$UsingDriver","Selenium Driver를 사용할지에 대한 여부입니다."),
            Tuple.Create("$LatestImagesCount","마지막으로 요청한 다운로드 이미지의 개수입니다."),
            Tuple.Create("$Infinity","무한대입니다. 기본적으로 int.Max값을 가집니다."),
            Tuple.Create("$RequestURL","현재 분석하고있는 URL을 가져옵니다."),
            Tuple.Create("$RequestHtml","현재 분석하고있는 HTML을 가져옵니다."),
            Tuple.Create("$LoadPage","특정 URL을 다운로드하고, 현재 분석하는 문서를 다운로드한 문서로 바꿉니다."),
            Tuple.Create("$AppendImage","다운로드할 이미지와 이미지의 저장 경로를 저장합니다."),
            Tuple.Create("$RequestDownload","모든 요청을 끝내고 저장된 URL을 모두 다운로드합니다."),
            Tuple.Create("$ExitLoop","특정 loop나 foreach를 탈출 합니다."),
            Tuple.Create("$ClearImagesCount","이미지 카운트를 0으로 합니다."),
            Tuple.Create("$ConsolePrint","콘솔에 str를 출력합니다."),
            Tuple.Create("$ConsolePrintln","콘솔에 str를 출력하고, 다음줄로 옮깁니다."),
            Tuple.Create("$MonitorPrint","Monitor를 통해 str을 출력합니다. 이는 로그에 기록됩니다."),
            Tuple.Create("$DriverNew","기존 드라이버를 닫고, 새로운 드라이버를 실행합니다."),
            Tuple.Create("$DriverLoadPage","특정 URL로 Navigate합니다."),
            Tuple.Create("$DriverClickByXPath","XPath를 이용해 특정 버튼을 클릭합니다."),
            Tuple.Create("$DriverClickByName","Name을 이용해 특정 버튼을 클릭합니다."),
            Tuple.Create("$DriverSendKey","특정 id 요소에 text를 씁니다."),
            Tuple.Create("$DriverGetScrollHeight","스크롤의 높이를 가져옵니다."),
            Tuple.Create("$DriverScrollTo","스크롤의 높이를 설정합니다."),
            Tuple.Create("$DriverScrollBottom","스크롤을 최하단으로 내립니다."),
            Tuple.Create("$MessageFadeOn","메인창에 메세지를 나타냅니다."),
            Tuple.Create("$MessageText","메세지의 내용을 바꿉니다."),
            Tuple.Create("$MessageFadeOff","메인창에서 메세지를 사라지게합니다."),
            Tuple.Create("$MessageFadeInFadeOut","일시적으로 나타났다 사라지는 메세지를 출력합니다."),
            Tuple.Create("add","두 값을 더합니다."),
            Tuple.Create("mul","두 값을 곱합니다."),
            Tuple.Create("mod","x mod y의 값을 구합니다."),
            Tuple.Create("div","x / y의 값을 구합니다."),
            Tuple.Create("cal","CAL 문법을 이용해 계산합니다. 반환값은 string-list입니다."),
            Tuple.Create("equal","두 값이 같은지 확인합니다. 반환값은 boolean입니다."),
            Tuple.Create("split","src를 기준으로 문자열을 자릅니다. 반환값은 string-list입니다."),
            Tuple.Create("count","List의 요소 개수를 가져옵니다. 반환값은 정수입니다."),
            Tuple.Create("concat","여러 문자열을 하나로 합칩니다."),
            Tuple.Create("url_parameter","URL의 특정 매개변수를 value로 설정합니다."),
            Tuple.Create("url_parameter_tidy","URL의 특정 매개변수를 삭제합니다."),
            Tuple.Create("int","불리안값이나 정수형, 문자열을 정수형으로 바꿉니다."),
            Tuple.Create("string","불리안값이나 정수형, 문자열을 문자열형으로 바꿉니다."),
            Tuple.Create("regex_exists","regex pattern과 매칭되는 항목이 존재하는지의 여부를 가져옵니다."),
            Tuple.Create("regex_match","regex pattern과 매칭되는 항목의 첫 번째 요소를 가져옵니다."),
            Tuple.Create("regex_matches","regex pattern과 매칭되는 모든 항목을 가져옵니다"),
            Tuple.Create("type","인자의 타입을 문자열형태로 가져옵니다."),
            Tuple.Create("to_json","문자열을 json형태로 가져옵니다."),
            Tuple.Create("get_json","json의 특정 항목을 가져옵니다."),
            Tuple.Create("get_json_string","json의 특정 항목을 문자열 형태로 가져옵니다."),
            Tuple.Create("base64decode","base64 포맷을 디코딩합니다."),
            Tuple.Create("htmldecode","html 포맷을 디코딩합니다. 가령, &amp;와 같은 문자열이 치환됩니다."),
            Tuple.Create("urldecode","url 포맷을 디코딩합니다."),
        };
        bool opening = false;
        private void TextArea_TextEntered(object sender, TextCompositionEventArgs e)
        {
            if (opening == false && (e.Text == "$" || char.IsLetter(e.Text[0])))//e.Text == "$")
            {
                var list = new List<Tuple<string, string>>();
                tokens.ToList().ForEach(x =>
                {
                    if (x.Item1.StartsWith(e.Text))
                        list.Add(x);
                });
                if (list.Count > 0)
                {
                    opening = true;
                    completionWindow = new CompletionWindow(textEditor.TextArea) { Height = 150, Width = 300 };
                    IList<ICompletionData> data = completionWindow.CompletionList.CompletionData;
                    list.ForEach(x => data.Add(new MyCompletionData(x.Item1.Substring(1), x.Item1, x.Item2)));
                    completionWindow.Show();
                    completionWindow.Closed += delegate
                    {
                        completionWindow = null;
                        opening = false;
                    };
                }
            }
        }

        private void TextArea_TextEntering(object sender, TextCompositionEventArgs e)
        {
            if (e.Text.Length > 0 && completionWindow != null)
            {
                if (!char.IsLetterOrDigit(e.Text[0]))
                {
                    completionWindow.CompletionList.RequestInsertion(e);
                }
            }
        }

        private void UpdateFoldings()
        {
            folding.UpdateFoldings(mgr, textEditor.Document);
        }

        SRCALParser.CDLScript script;
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            var tag = (sender as Button).Tag.ToString();
            DebugMonitor.Text = "";
            var raw_script = textEditor.Text.Split(
                new[] { '\n' },
                StringSplitOptions.None
                ).ToList();
            if (tag == "Open")
            {
                var ofd = new OpenFileDialog();
                ofd.InitialDirectory = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "script");
                ofd.Filter = "SRCAL 스크립트 파일 (*.srcal)|*.srcal";
                if (ofd.ShowDialog() == true)
                {
                    textEditor.Text = File.ReadAllText(ofd.FileName);
                }
            }
            else if (tag == "Save")
            {
                string default_filename = "";
                try
                {
                    var parser = new SRCALParser();
                    script = parser.Parse(raw_script);
                    default_filename = parser.attributes["$ScriptName"];
                } catch { }
                var sfd = new SaveFileDialog();
                sfd.InitialDirectory = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "script");
                sfd.FileName = default_filename;
                sfd.Filter = "SRCAL 스크립트 파일 (*.srcal)|*.srcal";
                if (sfd.ShowDialog() == true)
                {
                    File.WriteAllText(sfd.FileName, string.Join("\r\n", raw_script));
                }
            }
            else if (tag == "Parse")
            {

                bool err = false;
                var parser = new SRCALParser();
                try
                {
                    script = parser.Parse(raw_script);

                    var attribute = new SRCALAttribute();
                    attribute.ScriptName = parser.attributes["$ScriptName"];
                    attribute.ScriptVersion = parser.attributes["$ScriptVersion"];
                    attribute.ScriptAuthor = parser.attributes["$ScriptAuthor"];
                    attribute.ScriptFolderName = parser.attributes["$ScriptFolderName"];
                    attribute.ScriptRequestName = parser.attributes["$ScriptRequestName"];
                    attribute.URLSpecifier = parser.attributes["$URLSpecifier"];
                    int v;
                    if (int.TryParse(parser.attributes["$UsingDriver"], out v))
                    {
                        attribute.UsingDriver = v == 0 ? false : true;
                    }
                    else
                    {
                        err = true;
                        DebugMonitor.Text = "Using driver must be integer type.\r\n";
                    }
                    DebugMonitor.Text = Monitor.SerializeObject(attribute) + "\r\n";
                }
                catch (Exception ex)
                {
                    DebugMonitor.Text += $"Script parsing error. {ex.Message}\r\n{ex.StackTrace}\r\n";
                    err = true;
                }

                if (parser.errors.Count > 0)
                {
                    DebugMonitor.Text += $"Occurred some errors when parsing script ...\r\n";
                    for (int i = 0; i < parser.errors.Count; i++)
                    {
                        DebugMonitor.Text += $"[{parser.errors[i].Item1.Line + 1}, {parser.errors[i].Item1.Column + 1}] {parser.errors[i].Item2}\r\n";
                    }
                    err = true;
                }

                if (!err)
                {
                    DebugMonitor.Text += "Complete parsing.";
                }
                else
                {
                    DebugMonitor.Text += "Error occured when parse script.";
                }
                DebugMonitor.ScrollToEnd();
            }
            else if (tag == "Inject")
            {
                try
                {
                    if (!ScriptManager.Instance.Subscribe(string.Join("\r\n", raw_script)))
                    {
                        MessageBox.Show("인젝션에 성공했습니다!\r\n메인창에서 스크립트를 실행하고, 콘솔에서 상태를 점검하세요.\r\n인젝션을 재시도하기 전에 반드시 이젝트해야합니다.", Title, MessageBoxButton.OK, MessageBoxImage.Information);
                        return;
                    }
                }
                catch (Exception ex)
                {
                    Monitor.Instance.Push($"[Script Editor] Fail to inject. {ex.Message}\r\n{ex.StackTrace}");
                }
                MessageBox.Show("인젝션에 실패했습니다. 자세한 내용은 콘솔을 참고해주세요.", Title, MessageBoxButton.OK, MessageBoxImage.Error);
            }
            else if (tag == "Eject")
            {
                var parser = new SRCALParser();
                try
                {
                    script = parser.Parse(raw_script);
                    if (ScriptManager.Instance.Unsubscribe(parser.attributes["$ScriptName"]) >= 1)
                    {
                        MessageBox.Show("이젝션 완료!", Title, MessageBoxButton.OK, MessageBoxImage.Information);
                        return;
                    }
                    else
                    {
                        MessageBox.Show("이젝션할 내용이 없습니다.", Title, MessageBoxButton.OK, MessageBoxImage.Exclamation);
                    }
                }
                catch (Exception ex)
                {
                    Monitor.Instance.Push($"[Script Editor] Fail to eject. {ex.Message}\r\n{ex.StackTrace}");
                    MessageBox.Show("이젝션을 실패했습니다. 자세한 내용은 콘솔을 참고해주세요.", Title, MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            else if (tag == "CC")
            {
                (new CustomCrawler()).Show();
            }
            else if (tag == "Help")
            {
                Process.Start("https://github.com/dc-koromo/koromo-copy/blob/master/Document/SRCAL.md");
            }
        }
    }
}
