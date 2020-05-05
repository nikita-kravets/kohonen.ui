using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Media;
using System.Linq;

namespace kohonen.ui
{
    class MapVisual : FrameworkElement
    {
        public IClassObjectGroupListener Listener { get; set; }
        private Dictionary<DrawingVisual, List<ClassObject>> HoverElements { get; set; }
        public Dictionary<int, List<ClassObject>> Map { get; set; }
        public int Dimension { get; set; }
        private List<Point> hexagonCenters;

        private readonly VisualCollection children;
        protected override int VisualChildrenCount => children.Count;
        public MapVisual()
        {
            children = new VisualCollection(this);
            this.Dimension = 2;
            this.Map = new Dictionary<int, List<ClassObject>>();
            this.HoverElements = new Dictionary<DrawingVisual, List<ClassObject>>();
            hexagonCenters = new List<Point>();
            this.MouseMove += MapVisual_MouseMove;
        }

        private void MapVisual_MouseMove(object sender, System.Windows.Input.MouseEventArgs e)
        {
            if (Listener != null)
            {
                Listener.Highlight(null);
            }

            VisualTreeHelper.HitTest(this,
                  null,  // No hit test filtering.
                  new HitTestResultCallback(HitTestResult),
                  new PointHitTestParameters(e.GetPosition(this)));

        }

        public HitTestResultBehavior HitTestResult(HitTestResult hr)
        {
            DrawingVisual visual = hr.VisualHit as DrawingVisual;
            if (visual != null && HoverElements.ContainsKey(visual))
            {
                if (Listener != null)
                {
                    Listener.Highlight(HoverElements[visual]);
                }
                return HitTestResultBehavior.Stop;
            }

            return HitTestResultBehavior.Continue;
        }
        public void DrawHexagons()
        {
            Clear();

            DrawingVisual visual = new DrawingVisual();
            DrawingContext context = visual.RenderOpen();

            context.DrawRectangle(new SolidColorBrush(Colors.White),
                null, new Rect(0, 0, 300, 300));

            var r = (300 / Dimension / 2) * Math.Sin(60 * Math.PI / 180f);

            double marginTop = ((300 / r) * Dimension) / 2;
            double marginLeft = ((300 / r) * Dimension) / 2;

            for (int i = 0; i < Dimension; i++)
            {
                for (int j = 0; j < Dimension; j++)
                {

                    //Get the center of hexagon
                    var x_0 = (r + r / 2) * (i + 1) + marginLeft;
                    var y_0 = (r * Math.Sin(60 * Math.PI / 180f) * 2) * (j + 1)
                        - (i % 2 != 0 ? r * Math.Sin(60 * Math.PI / 180f) : 0) + marginTop;

                    hexagonCenters.Add(new Point(x_0, y_0));

                    var shape = new Point[6];

                    //Create 6 points
                    for (int a = 0; a < 6; a++)
                    {
                        shape[a] = new Point(
                            x_0 + r * (float)Math.Cos(a * 60 * Math.PI / 180f),
                            y_0 + r * (float)Math.Sin(a * 60 * Math.PI / 180f));
                    }

                    //Draw 6 lines
                    for (int l = 0; l < 6; l++)
                    {
                        context.DrawLine(new Pen(new SolidColorBrush(Colors.Gray), 1),
                            shape[l], shape[l + 1 > 5 ? 0 : l + 1]);
                    }
                }
            }

            context.Close();
            children.Add(visual);
        }

        protected override Visual GetVisualChild(int index)
        {
            if (index < 0 || index >= children.Count)
            {
                throw new ArgumentOutOfRangeException();
            }

            return children[index];
        }

        public void Clear()
        {
            children.RemoveRange(0, children.Count);
            Map.Clear();
            hexagonCenters.Clear();
        }

        private Color GetColorByGroup(int group)
        {
            switch (group)
            {
                case 1:
                    return Colors.OrangeRed;
                case 2:
                    return Colors.SteelBlue;
                case 3:
                    return Colors.LawnGreen;
                default:
                    var colors = typeof(Colors)
                        .GetProperties()
                        .Where(p => p.PropertyType == typeof(Color))
                        .Select(p => { return p.GetValue(null); })
                        .ToList();
                    return
                        colors.OfType<Color>().Where(c =>
                        c != Colors.OrangeRed &&
                        c != Colors.SteelBlue &&
                        c != Colors.LawnGreen &&
                        c != Colors.Black &&
                        c != Colors.Gray
                        ).Take(Math.Min(group, colors.Count)).Last();
            }
        }

        public void DrawContents()
        {
            if (this.children.Count > 1)
            {
                this.children.RemoveRange(1, children.Count - 1);
                HoverElements.Clear();
            }

            var maxR = 300 / Dimension / 3.5;
            //Get Max count for 
            var maxWeight = Map.Values
                .Where(v => v != null)
                .GroupBy(c => c.Count)
                .Select(c => c.Key)
                .Max();

            foreach (var p in Map)
            {
                if (p.Value != null && p.Value.Count > 0)
                {
                    var r = p.Value.Count * maxR / maxWeight;

                    var groups = p.Value.GroupBy(v => v.C).OrderBy(g => g.Key);
                    var arcPoint = new Point(hexagonCenters[p.Key].X + r, hexagonCenters[p.Key].Y);
                    double angle = 0;

                    foreach (var g in groups)
                    {
                        DrawingVisual visual = new DrawingVisual();
                        DrawingContext context = visual.RenderOpen();
                        Color color = GetColorByGroup(g.Key);
                        double currentAngle = 360 * g.Count() / p.Value.Count;

                        if (currentAngle != 360.0)
                        {
                            angle += currentAngle;

                            context.DrawGeometry(new SolidColorBrush(color),
                                new Pen(new SolidColorBrush(Colors.White), 1),
                                Geometry.Parse(string.Format(System.Globalization.CultureInfo.InvariantCulture,
                                    "M {0},{1} L {2},{3} A {4},{5} {8} {9} {10} {6},{7} L {0},{1} Z",
                                    hexagonCenters[p.Key].X,
                                    hexagonCenters[p.Key].Y,
                                    arcPoint.X,
                                    arcPoint.Y,
                                    r, r,
                                    hexagonCenters[p.Key].X + r * Math.Cos(angle * Math.PI / 180f),
                                    hexagonCenters[p.Key].Y + r * Math.Sin(angle * Math.PI / 180f),
                                    0,
                                    currentAngle > 180 ? 1 : 0,
                                    1
                                    )));
                            arcPoint.X = hexagonCenters[p.Key].X + r * Math.Cos(angle * Math.PI / 180f);
                            arcPoint.Y = hexagonCenters[p.Key].Y + r * Math.Sin(angle * Math.PI / 180f);
                        }
                        else
                        {
                            context.DrawEllipse(new SolidColorBrush(color),
                                new Pen(new SolidColorBrush(Colors.White), 1), hexagonCenters[p.Key], r, r);
                        }
                        context.Close();
                        children.Add(visual);
                        HoverElements[visual] = new List<ClassObject>();
                        HoverElements[visual].AddRange(g.AsEnumerable());
                    }
                }
            }
        }
    }
}
