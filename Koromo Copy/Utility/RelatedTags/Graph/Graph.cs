/* Copyright (C) 2018-2019. Hitomi Parser Developers */

using Koromo_Copy.Component.Hitomi.Analysis;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace Hitomi_Copy_3.Graph
{
    public partial class Graph : UserControl
    {
        ViewManager vm;
        float zoom = 1.0F;

        public Graph()
        {
            InitializeComponent();

            ResizeRedraw = true;
            DoubleBuffered = true;

            MouseDown += new MouseEventHandler(OnMouseDown);
            MouseUp += new MouseEventHandler(OnMouseUp);
            MouseMove += new MouseEventHandler(OnMouseMove);
            MouseClick += new MouseEventHandler(OnMouseClick);
            MouseWheel += new MouseEventHandler(OnMouseWheel);
            MouseDoubleClick += new MouseEventHandler(OnMouseDoubleClick);

            KeyDown += new KeyEventHandler(OnKeyDown);
            KeyUp += new KeyEventHandler(OnKeyUp);

            vm = new ViewManager(Font);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            vm.Render(e.Graphics, this.Size, PointToClient(Cursor.Position), zoom, GetStaticState());
            base.OnPaint(e);
        }

        public GraphNodeManager GetGNM()
        {
            return vm.gnm;
        }

        private List<FixedString> GetStaticState()
        {
            List<FixedString> fx = new List<FixedString>();
            string message = $"Base Point : {vm.BasePoint.ToString()}\n" +
                $"Mouse Point : {PointToClient(Cursor.Position).ToString()}\n"+
                $"Zoom : {zoom.ToString()}\n";
            if (key_control)
                message += "\nControl Key Pressed";
            fx.Add(new FixedString(message, new Point(10, 10), Font, Brushes.Aqua));
            return fx;
        }

        #region Mouse Event

        bool ml_down = false;
        bool mr_down = false;
        Point mr_pos;
        Point ml_pos;

        private void OnMouseDown(object sender, MouseEventArgs e)
        {
            if (ml_down == false && e.Button == MouseButtons.Left)
            {
                ml_down = true;
                ml_pos = e.Location;

                if (key_control)
                {
                    vm.DragBoxEndPoint = vm.DragBoxStartPoint = PointToClient(Cursor.Position);
                    vm.IsDrawDragBox = true;
                }

                Invalidate();
            }
        }
        private void OnMouseUp(object sender, MouseEventArgs e)
        {
            if (ml_down == true)
            {
                ml_down = false;

                if (key_control)
                {
                    vm.IsDrawDragBox = false;
                }

                vm.SelectNode(e.Location, zoom);
                Invalidate();
            }
            else if (mr_down == true)
            {
                mr_down = false;
            }
        }
        private void OnMouseMove(object sender, MouseEventArgs e)
        {
            if (ml_down)
            {
                if (!key_control)
                {
                    Cursor = Cursors.Hand;
                    
                    int dx = ml_pos.X - e.Location.X;
                    int dy = ml_pos.Y - e.Location.Y;

                    vm.Move((int)(dx / zoom), (int)(dy / zoom));
                }
                else
                {
                    vm.DragBoxEndPoint = e.Location;
                }
                ml_pos = e.Location;
                Invalidate();
            }
            else if (mr_down)
            {
                mr_pos = e.Location;

                Invalidate();
            }
        }
        private void OnMouseClick(object sender, MouseEventArgs e)
        {
        }
        private void OnMouseWheel(object sender, MouseEventArgs e)
        {
            Point p = PointToClient(e.Location);
            float prev_zoom = zoom;
            
            if (e.Delta > 0)
                zoom += 0.05F;
            else
                zoom -= 0.05F;

            if (zoom < 0.05F)
                zoom = 0.05F;

            int dx = (int)(p.X - p.X * zoom / prev_zoom);
            int dy = (int)(p.Y - p.Y * zoom / prev_zoom);
            vm.Move(-dx, -dy);

            Invalidate();
        }
        private void OnMouseDoubleClick(object sender, MouseEventArgs e)
        {
            string tag = vm.GetSelectedNodeText(e.Location, zoom);
            if (tag != "")
            {
                Point bp = vm.BasePoint;
                vm = new ViewManager(Font);
                init_graph(tag);
                vm.BasePoint = bp;
                Invalidate();
            }
        }

        #region Mouse Clipping

        public void ClipStart()
        {
            Cursor.Clip = new Rectangle(PointToScreen(Location), Size);
        }

        public void ClipEnd()
        {
            Cursor.Clip = new Rectangle();
        }

        #endregion

        #endregion

        #region Keyboard Event

        bool key_control = false;

        private void OnKeyDown(object sender, KeyEventArgs e)
        {
            switch (e.KeyCode)
            {
                case Keys.Escape:
                    Cursor.Clip = Rectangle.Empty;
                    break;
                default:
                    if (e.Control && !key_control)
                    {
                        key_control = true;
                    }
                    break;
            }
            Invalidate();
        }
        private void OnKeyUp(object sender, KeyEventArgs e)
        {
            key_control = false;
            vm.IsDrawDragBox = false;
            Invalidate();
        }

        #endregion

        #region Initialize

        public void init_graph(string tag)
        {
            if (tag != "")
            {
                var result = HitomiAnalysisRelatedTags.Instance.result[tag];

                int index = 0;
                int eindex = 0;

                GraphVertex v = new GraphVertex();
                v.Index = index++;
                v.Color = Color.White;
                v.InnerText = tag;
                v.OuterText = "";
                v.Radius = 100.0F;
                v.Nodes = new List<Tuple<GraphVertex, GraphEdge>>();

                foreach (var ld in result)
                {
                    GraphVertex vt = new GraphVertex();
                    GraphEdge et = new GraphEdge();
                    vt.Index = index++;
                    vt.Color = Color.WhiteSmoke;
                    vt.InnerText = ld.Item1;
                    vt.Radius = 100.0F;
                    vt.OuterText = "";

                    et.StartsIndex = 0;
                    et.EndsIndex = vt.Index;
                    et.Index = eindex++;
                    et.Text = ld.Item2.ToString().Substring(0, Math.Min(5, ld.Item2.ToString().Length));
                    et.SelectionText = "";
                    et.Thickness = 6.0F;
                    et.Color = Color.Black;

                    v.Nodes.Add(new Tuple<GraphVertex, GraphEdge>(vt, et));
                }
                List<Tuple<Point, double>> edges = new List<Tuple<Point, double>>();
                for (int i = 0; i < v.Nodes.Count; i++)
                {
                    for (int j = i + 1; j < v.Nodes.Count; j++)
                    {
                        var list = HitomiAnalysisRelatedTags.Instance.result[v.Nodes[i].Item1.InnerText].Where(x => x.Item1 == v.Nodes[j].Item1.InnerText);

                        if (list.Count() > 0)
                        {
                            edges.Add(new Tuple<Point, double>(new Point(i + 1, j + 1), list.ToList()[0].Item2));
                        }
                    }
                }
                GetGNM().Nomalize(v, edges);
            }
        }

        #endregion
    }
}
