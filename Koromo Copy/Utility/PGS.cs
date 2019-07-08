/***

   Copyright (C) 2018-2019. dc-koromo. All Rights Reserved.
   
   Author: Koromo Copy Developer

***/

using Koromo_Copy.LP;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Koromo_Copy.Utility
{
    public partial class PGS : Form
    {
        public PGS()
        {
            InitializeComponent();
        }

        Scanner scanner;

        private void bLG_Click(object sender, EventArgs e)
        {
            var sg = new ScannerGenerator();

            try
            {
                foreach (var line in rtbLLD.Lines)
                    sg.PushRule(line.Split(new[] { "=>" }, StringSplitOptions.None)[1].Replace("\"", "").Trim(), line.Split(new[] { "=>" }, StringSplitOptions.None)[0].Trim());
                sg.Generate();
                scanner = sg.CreateScannerInstance();
                rtbLS.AppendText("New scanner instance generated!\r\n" + sg.PrintDiagram());
            }
            catch (Exception ex)
            {
                rtbLS.Text = $"Scanner build error!\r\n{ex.Message}\r\n{ex.StackTrace}";
            }
        }

        private void rtbLS_TextChanged(object sender, EventArgs e)
        {
            rtbLS.SelectionStart = rtbLS.Text.Length;
            rtbLS.ScrollToCaret();
        }

        private void bLT_Click(object sender, EventArgs e)
        {
            if (scanner == null)
            {
                rtbLS.AppendText("Create scanner instance before testing!\r\n");
                return;
            }

            rtbLS.AppendText(" ----------- Start Lexing -----------\r\n");
            scanner.AllocateTarget(rtbLT.Text);

            try
            {
                while (scanner.Valid())
                {
                    var ss = scanner.Next();
                    if (scanner.Error())
                        rtbLS.AppendText("Error!\r\n");
                    rtbLS.AppendText($"{ss.Item1},".PadRight(10) + $" {ss.Item2}\r\n");
                }
            }
            catch (Exception ex)
            {
                rtbLS.AppendText("Error!\r\nCheck test case!\r\n");
            }
            rtbLS.AppendText(" ------------ End Lexing ------------\r\n");
        }

        ShiftReduceParser srparser;

        private void bPGG_Click(object sender, EventArgs e)
        {
            try
            {
                var gen = ParserGenerator.GetGenerator(rtbPGNT.Text.Split(','), rtbPGT.Lines.Select(x => x.Trim()).Select(x => 
                            new Tuple<string, string> (x.Split(',')[0], x.Substring(x.Split(',')[0].Length + 1).Trim())).ToArray(), 
                            rtbPGPR.Lines.Select(x => x.Trim()).ToArray(), rtbPGC.Lines.Select(x => x.Trim()).ToArray());
                
                rtbPGS.Clear();
                gen.GlobalPrinter.Clear();
                gen.PrintProductionRules();
                if (rbSLR.Checked == true)
                {
                    gen.Generate();
                }
                else if (rbLALR.Checked == true)
                {
                    gen.GenerateLALR();
                }
                else
                {
                    gen.GenerateLR1();
                }
                gen.PrintStates();
                gen.PrintTable();
                rtbPGS.AppendText(gen.GlobalPrinter.ToString());
                srparser = gen.CreateShiftReduceParserInstance();
            }
            catch (Exception ex)
            {
                rtbPGS.AppendText("Generate Error!\r\n" + ex.Message + "\r\n" + ex.StackTrace);
            }
        }

        private void rtbPGS_TextChanged(object sender, EventArgs e)
        {
            rtbPGS.SelectionStart = rtbPGS.Text.Length;
            rtbPGS.ScrollToCaret();
        }

        private void bPGT_Click(object sender, EventArgs e)
        {
            if (scanner == null)
            {
                rtbPGS.AppendText("Create scanner instance before testing!\r\n");
                return;
            }

            if (srparser == null)
            {
                rtbPGS.AppendText("Create parser instance before testing!\r\n");
                return;
            }

            foreach (var line in rtbPGTEST.Lines)
            {
                rtbPGS.AppendText(" ------ TEST: " + line + "\r\n");

                srparser.Clear();
                Action<string, string> insert = (string x, string y) =>
                {
                    srparser.Insert(x, y);
                    if (srparser.Error())
                        rtbPGS.AppendText("PARSING ERROR" + "\r\n");
                    while (srparser.Reduce())
                    {
                        rtbPGS.AppendText(srparser.Stack() + "\r\n");
                        var l = srparser.LatestReduce();
                        rtbPGS.AppendText(l.Production.PadLeft(8) + " => ");
                        rtbPGS.AppendText(string.Join(" ", l.Childs.Select(z => z.Production)) + "\r\n");
                        rtbPGS.AppendText(l.Production.PadLeft(8) + " => ");
                        rtbPGS.AppendText(string.Join(" ", l.Childs.Select(z => z.Contents)) + "\r\n");
                        srparser.Insert(x, y);
                        if (srparser.Error())
                            rtbPGS.AppendText("PARSING ERROR" + "\r\n");
                    }
                    rtbPGS.AppendText(srparser.Stack() + "\r\n");
                };

                scanner.AllocateTarget(line);

                try
                {
                    while (scanner.Valid())
                    {
                        var ss = scanner.Next();
                        insert(ss.Item1, ss.Item2);
                    }
                    insert("$", "$");
                }
                catch (Exception ex)
                {
                    rtbPGS.AppendText("Error!\r\nCheck test case!\r\n");
                }

                try
                {
                    var builder = new StringBuilder();

                    srparser.Tree.Print(builder);
                    rtbPGS.AppendText(builder.ToString());
                }
                catch
                {

                }

                rtbPGS.AppendText(" ------ END TEST ------\r\n");
            }
        }
    }
}
