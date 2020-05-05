using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Linq;

namespace kohonen.ui
{
    class ObjectClassVisual : FrameworkElement, IClassObjectGroupListener
    {
        private readonly VisualCollection children;
        public int SelectedClass { get; set; }
        public List<ClassObject> Objects { get; set; }
        private List<DrawingVisual> HighlightedObjects { get; set; }
        public ObjectClassVisual()
        {
            this.SelectedClass = 0;
            this.Objects = new List<ClassObject>();
            this.HighlightedObjects = new List<DrawingVisual>();
            children = new VisualCollection(this) {
                DrawField(),
                DrawAxes()
            };
            MouseLeftButtonDown += ObjectClassVisual_MouseLeftButtonDown;
        }

        private Color FromClass(int cls)
        {
            switch (cls)
            {
                case 1:
                    return Colors.OrangeRed;
                case 2:
                    return Colors.SteelBlue;
                default:
                    return Colors.LawnGreen;
            }
        }
        private Color FromSelectedClass()
        {
            return FromClass(SelectedClass);
        }

        public void Clear()
        {
            children.RemoveRange(2, children.Count - 2);
            this.Objects.Clear();
        }

        private void ObjectClassVisual_MouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            Point pt = e.GetPosition((UIElement)sender);
            this.Objects.Add(new ClassObject() { C = SelectedClass, X = pt.X, Y = pt.Y });
            DrawingVisual visual = new DrawingVisual();
            DrawingContext context = visual.RenderOpen();

            context.DrawRectangle(new SolidColorBrush(Colors.White), new Pen(new SolidColorBrush(FromSelectedClass()), 2),
                new Rect(pt.X - 2, pt.Y - 2, 4, 4));
            context.Close();
            this.children.Add(visual);
        }

        DrawingVisual DrawAxes()
        {
            DrawingVisual visual = new DrawingVisual();
            DrawingContext context = visual.RenderOpen();
            context.DrawLine(new Pen(new SolidColorBrush(Colors.Black), 1), new Point(10, 10), new Point(10, 280));
            context.DrawLine(new Pen(new SolidColorBrush(Colors.Black), 1), new Point(10, 280), new Point(280, 280));

            context.DrawText(new FormattedText("y", System.Globalization.CultureInfo.InvariantCulture,
                FlowDirection.LeftToRight, new Typeface("Arial"), 16, new SolidColorBrush(Colors.Black), 96), new Point(17, 12));
            context.DrawText(new FormattedText("x", System.Globalization.CultureInfo.InvariantCulture,
                FlowDirection.LeftToRight, new Typeface("Arial"), 16, new SolidColorBrush(Colors.Black), 96), new Point(260, 255));
            context.Close();
            return visual;
        }

        DrawingVisual DrawField()
        {
            DrawingVisual visual = new DrawingVisual();
            DrawingContext context = visual.RenderOpen();
            context.DrawRectangle(new SolidColorBrush(Colors.White), null, new Rect(0, 0, 300, 300));
            context.Close();
            return visual;
        }

        protected override int VisualChildrenCount => children.Count;
        protected override Visual GetVisualChild(int index)
        {
            if (index < 0 || index >= children.Count)
            {
                throw new ArgumentOutOfRangeException();
            }

            return children[index];
        }

        public void Highlight(List<ClassObject> list)
        {
            HighlightedObjects.ForEach(v => children.Remove(v));
            HighlightedObjects.Clear();
            if (list != null) {
                list.ForEach(o =>
                {
                    DrawingVisual visual = new DrawingVisual();
                    DrawingContext context = visual.RenderOpen();
                    Color fill =  FromClass(o.C);
                    fill.A = 200;
                    context.DrawRectangle(new SolidColorBrush(FromClass(o.C)), new Pen(new SolidColorBrush(fill), 3),
                        new Rect(o.X - 3, o.Y - 3, 6, 6));
                    context.Close();
                    this.children.Add(visual);
                    this.HighlightedObjects.Add(visual);
                });
            }
        }
    }
}
