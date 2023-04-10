using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Bresenham
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        List<Point> Points;     // list pre body
        int clickCount;         // premenna na ratanie klikov
        List<Line> Lines;
        bool sumernostXY, sumernostX, sumernostY;   // pomocne premenne pre transformacie
        double r;
        Point S;                // stred kruznice

        public MainWindow()
        {
            InitializeComponent();
            Points = new List<Point>();
            Lines = new List<Line>();
            Grid();
            clickCount = 0;
        }
        void Grid()      // vykresli sa mriezka
        {
            for (int i = 0; i <= g.Width; i = i + 10)
            {
                Line L = new Line();
                L.Stroke = new SolidColorBrush(Colors.DarkBlue);
                L.Stroke.Opacity = 0.25;
                L.X1 = i;
                L.Y1 = 0;
                L.X2 = i;
                L.Y2 = g.Height;
                Canvas.SetZIndex(L, 5);
                g.Children.Add(L);
            }

            for (int i = 0; i <= g.Height; i = i + 10)
            {
                Line L = new Line();
                L.Stroke = new SolidColorBrush(Colors.DarkBlue);
                L.Stroke.Opacity = 0.25;
                L.X1 = 0;
                L.Y1 = i;
                L.X2 = g.Width;
                L.Y2 = i;
                Canvas.SetZIndex(L, 5);
                g.Children.Add(L);
            }
        }
        void DrawPoint(Point P)  // vykreslenie bodu
        {
            Ellipse R = new Ellipse();
            R.Fill = new SolidColorBrush(Colors.DarkBlue);
            R.Width = 5;
            R.Height = 5;
            Canvas.SetLeft(R, P.X - 2.5);
            Canvas.SetTop(R, P.Y - 2.5);
            Canvas.SetZIndex(R, 7);
            g.Children.Add(R);
        }
        void DrawLine(Point A, Point B, List<Line> LineSegments)  // vykreslenie usecky medzi dvoma bodmi
        {
            Line L = new Line();
            L.Stroke = new SolidColorBrush(Colors.DarkBlue);
            L.StrokeThickness = 1;
            L.X1 = A.X;
            L.Y1 = A.Y;
            L.X2 = B.X;
            L.Y2 = B.Y;
            Canvas.SetZIndex(L, 6);
            g.Children.Add(L);
            LineSegments.Add(L);
        }
        void CreateLine(Point A, Point B, List<Line> LineSegments)  // vytvori usecku, ale nevykresluje ju
        {
            Line L = new Line();
            L.Stroke = new SolidColorBrush(Colors.DarkBlue);
            L.StrokeThickness = 1;
            L.X1 = A.X;
            L.Y1 = A.Y;
            L.X2 = B.X;
            L.Y2 = B.Y;
            Canvas.SetZIndex(L, 6);
            LineSegments.Add(L);
        }
        private void g_MouseDown(object sender, MouseButtonEventArgs e)
        {
            clickCount++;
            if (LineSegment.IsChecked == true && clickCount < 3)        // rezim usecka
            {
                if (clickCount == 1)                                    // pri prvom kliku inicializujeme plochu a vytvorime bod
                {
                    Reset();
                }

                Point P = e.GetPosition(g);
                P.X = Convert.ToInt32(Math.Floor(P.X / 10)) * 10 + 5;   // tymto zaokruhlovanim zabezpecim to, aby bol bod v strede stvorceka a teda aj dobre ratanie dalej
                P.Y = Convert.ToInt32(Math.Floor(P.Y / 10)) * 10 + 5;
                DrawPixel(P.X, P.Y);
                Points.Add(P);

                if (clickCount == 2)                                    // pri druhom kliku vytvorime usecku a raterizujeme ju
                {
                    CreateLine(Points[Points.Count - 2], Points[Points.Count - 1], Lines);
                    Bresenham();
                    clickCount = 0;
                }

            }
            if (Circle.IsChecked == true && clickCount < 3)             // rezim kruznica
            {
                if (clickCount == 1)
                {
                    Reset();

                    S = e.GetPosition(g);                                     // pri prvom kliku inicializujeme plochu a vytvorime stred
                    S.X = Convert.ToInt32(Math.Floor(S.X / 10)) * 10 + 5;     // tymto zaokruhlovanim zabezpecim to, aby bol bod v strede stvorceka a teda aj dobre ratanie dalej
                    S.Y = Convert.ToInt32(Math.Floor(S.Y / 10)) * 10 + 5;
                    DrawPoint(S);
                    Points.Add(S);
                }

                if (clickCount == 2)
                {
                    Point P = e.GetPosition(g);                                // tento bod udáva polomer
                    P.X = Convert.ToInt32(Math.Floor(P.X / 10)) * 10 + 5;
                    P.Y = Convert.ToInt32(Math.Floor(P.Y / 10)) * 10 + 5;

                    DrawPoint(P);
                    Points.Add(P);
                    DrawLine(Points[Points.Count - 2], Points[Points.Count - 1], Lines);    // vykreslenie polomeru a vypocet jeho hodnoty

                    r = Math.Sqrt(Math.Pow(Points[Points.Count - 2].X - Points[Points.Count - 1].X, 2) + Math.Pow(Points[Points.Count - 2].Y - Points[Points.Count - 1].Y, 2));

                    /*Ellipse E = new Ellipse();                                            // vykreslenie elipsy, ktoru rasterizujeme
                    E.Stroke = new SolidColorBrush(Colors.MediumVioletRed);
                    E.StrokeThickness = 2;
                    E.Width = 2 * r;
                    E.Height = 2 * r;
                    Canvas.SetLeft(E, Points[Points.Count - 2].X - r);
                    Canvas.SetTop(E, Points[Points.Count - 2].Y - r);
                    Canvas.SetZIndex(E, 3);
                    g.Children.Add(E);*/
                    BresenhamCircle();
                    clickCount = 0;
                }
            }

            if (PolygonalLine.IsChecked == true)                                    // rezim lomenej ciary
            {
                Point P = e.GetPosition(g);
                P.X = Convert.ToInt32(Math.Floor(P.X / 10)) * 10 + 5;              
                P.Y = Convert.ToInt32(Math.Floor(P.Y / 10)) * 10 + 5;
                DrawPixel(P.X, P.Y);
                Points.Add(P);

                if (clickCount > 1)                                                 // algoritmus je rovnaky ako pri usecke
                {
                    CreateLine(Points[Points.Count - 2], Points[Points.Count - 1], Lines);
                    Bresenham();
                }
            }

        }
        void ReverseTransformation(double x, double y)      // spatna transformacia pre pixely, operacie sa vykonavaju v opacnom poradi
        {
            double pomocnaPremenna;
            if (sumernostX == true)
            {
                y = g.Height - y;
            }
            if (sumernostY == true)
            {
                x = g.Width - x;
            }
            if (sumernostXY == true)        // x a y sa vymenia
            {
                pomocnaPremenna = x;        // ulozim si x
                x = y;
                y = pomocnaPremenna;
            }
            DrawPixel(x, y);                // stransformovany pixel sa vykresli
        }
        void Bresenham()
        {
            // najprv transformacia
            foreach (Line L in Lines)
            {
                double deltaY, deltaX, m;
                sumernostXY = false;
                sumernostY = false;
                sumernostX = false;

                deltaY = L.Y2 - L.Y1;
                deltaX = L.X2 - L.X1;
                m = deltaY / deltaX;

                Line L1 = new Line();
                Line L2 = new Line();
                Line L3 = new Line();

                //odkomentovanim mozno vidiet ako sa usecka transformuje
                /*L1.Stroke = new SolidColorBrush(Colors.DarkOrange);
                L1.StrokeThickness = 2;
                L2.Stroke = new SolidColorBrush(Colors.DarkGreen);
                L2.StrokeThickness = 2;
                L3.Stroke = new SolidColorBrush(Colors.Red);
                L3.StrokeThickness = 2;*/

                if (Math.Abs(m) > 1) // x a y sa vymenia
                {
                    L1.X1 = L.Y1;
                    L1.Y1 = L.X1;
                    L1.X2 = L.Y2;
                    L1.Y2 = L.X2;
                    sumernostXY = true;
                    deltaY = L1.Y2 - L1.Y1;
                    deltaX = L1.X2 - L1.X1;
                }
                else
                {
                    L1.X1 = L.X1;
                    L1.Y1 = L.Y1;
                    L1.X2 = L.X2;
                    L1.Y2 = L.Y2;
                }
                /*Canvas.SetZIndex(L1, 2);
                g.Children.Add(L1);*/

                if (deltaX < 0)         // sumernost podla osi y v g.Width/2
                {
                    L2.X1 = g.Width - L1.X1;
                    L2.Y1 = L1.Y1;
                    L2.X2 = g.Width - L1.X2;
                    L2.Y2 = L1.Y2;
                    sumernostY = true;
                }
                else
                {
                    L2.X1 = L1.X1;
                    L2.Y1 = L1.Y1;
                    L2.X2 = L1.X2;
                    L2.Y2 = L1.Y2;
                }
                /*Canvas.SetZIndex(L2, 2);
                g.Children.Add(L2);*/

                if (deltaY < 0)     // sumernost podla osi x vo vyske g.Height/2
                {
                    L3.X1 = L2.X1;
                    L3.Y1 = g.Height - L2.Y1;
                    L3.X2 = L2.X2;
                    L3.Y2 = g.Height - L2.Y2;
                    sumernostX = true;
                }
                else
                {
                    L3.X1 = L2.X1;
                    L3.Y1 = L2.Y1;
                    L3.X2 = L2.X2;
                    L3.Y2 = L2.Y2;
                }
                /*Canvas.SetZIndex(L3, 2);
                g.Children.Add(L3);*/
                BresenhamLine(L3);
            }
        }   // transformacia a volanie algoritmu
        void DrawPixel(double x, double y)      // metoda na vykreslenie stvorceka
        {
            if (x < g.Width && y < g.Height)
            {
                Rectangle R = new Rectangle();
                R.Fill = new SolidColorBrush(Colors.HotPink);
                R.Width = 10;
                R.Height = 10;
                Canvas.SetLeft(R, Convert.ToInt32(x) - 5);
                Canvas.SetTop(R, Convert.ToInt32(y) - 5);
                Canvas.SetZIndex(R, 3);
                g.Children.Add(R);
            }
        }
        void BresenhamLine(Line L)      // Bresenhanmov algoritmus pre 1. oktant
        {
            double dx, dy, c1, c2, p, x, y;     // pociatocne hodnoty (musia by formalne double kvoli L.X a L.Y a v skutocnosti su int)
            x = L.X1;
            y = L.Y1;
            dx = L.X2 - L.X1;
            dy = L.Y2 - L.Y1;
            c1 = 2 * dy;
            c2 = 2 * (dy - dx);
            p = 2 * dy - dx;
            while (x < L.X2)                    // az kym nedosiahneme posledny bod
            {
                if (p > 0)                      // podla prirastku pripocitame c1 alebo c2
                {
                    p = p + c2;
                    y = y + 10;
                }
                else
                {
                    p = p + c1;
                }
                x = x + 10;
                ReverseTransformation(x, y);   // vykona sa spatna transformacia spolu s vykreslenim
            }
        }
        void BresenhamCircle()      // pre kruznicu so stredom v (0, 0) a 2. oktante
        {
            // nas stred je bod S, kazdy bod treba posunut o vektor S.X, S.Y
            double p, x, y;
            x = 0;
            y = Convert.ToInt32(Math.Floor(r / 10)) * 10;   // zaokruhlenie polmoeru

            DrawPixel(S.X + x, S.Y + y);                    // pociatocne pixely
            DrawPixel(S.X - x, S.Y - y);
            DrawPixel(S.X + y, S.Y - x);
            DrawPixel(S.X - y, S.Y + x);

            p = 3 - 2 * r;
            while (x <= y)
            {
                if (p < 0)
                {
                    p = p + 4 * x + 6;
                }
                else
                {
                    p = p + 4 * (x - y) + 10;
                    y = y - 10;
                }
                x = x + 10;

                DrawPixel(x + S.X, y + S.Y);        // vykreslenie vsetkych 8 oktantov pomocou symetrie
                DrawPixel(-x + S.X, y + S.Y);
                DrawPixel(x + S.X, -y + S.Y);
                DrawPixel(-x + S.X, -y + S.Y);

                DrawPixel(y + S.X, x + S.Y);
                DrawPixel(-y + S.X, x + S.Y);
                DrawPixel(y + S.X, -x + S.Y);
                DrawPixel(-y + S.X, -x + S.Y);
            }
        }

        private void ResetButton_Click(object sender, RoutedEventArgs e)      // vymaze canvas a listy
        {
            Reset();
            clickCount = 0;
        }

        private void LineSegment_Checked(object sender, RoutedEventArgs e)      // zmena rezimu
        {
            if (Points != null)
            {
                Reset();
            }
            clickCount = 0;
        }

        private void Circle_Checked(object sender, RoutedEventArgs e)   // zmena rezimu
        {
            Reset();
            clickCount = 0;
        }

        private void PolygonalLine_Checked(object sender, RoutedEventArgs e) // zmena rezim
        {
            Reset();
            clickCount = 0;
        }
        void Reset()
        {
            g.Children.Clear();
            Grid();
            Lines.Clear();
            Points.Clear();
        }
    }
}
