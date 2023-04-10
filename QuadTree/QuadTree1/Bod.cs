using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

namespace QuadTree1
{
    public class Bod
    {
        private Canvas g;
        public double X, Y;                             // suradnice
        public double radius = 3;

        public Bod(double X, double Y, Canvas C)
        {
            this.X = X;
            this.Y = Y;
            g = C;
            DrawPoint();                                 // kresli body
        }

        public Ellipse DrawPoint()
        {
            Ellipse E = new Ellipse();
            E.Fill = new SolidColorBrush(Colors.MediumVioletRed);
            E.Width = 2 * radius;
            E.Height = 2 * radius;
            Canvas.SetLeft(E, X - radius);
            Canvas.SetTop(E, Y - radius);
            Canvas.SetZIndex(E, 1);                     // z-index 1-body, 2-ciary
            g.Children.Add(E);
            return E;
        }

        public Ellipse ChangeColor(Color C)             // vykresli bod farbou v parametri 
        {
            Ellipse E = new Ellipse();
            E.Fill = new SolidColorBrush(C);
            E.Width = 2 * radius;
            E.Height = 2 * radius;
            Canvas.SetLeft(E, X - radius);
            Canvas.SetTop(E, Y - radius);
            Canvas.SetZIndex(E, 2);
            g.Children.Add(E);
            return E;
        }
    }
}
