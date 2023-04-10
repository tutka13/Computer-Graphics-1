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

namespace Z_buffer
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        List<Point> Points;
        List<Point> GrayPoints;
        List<Line> Lines;
        Line SivaUsecka;
        List<Point> PointsOnGrayLine;
        List<Point> Middle;
        List<Brush> FarbyUseciek;

        Random rnd;
        int clickLeftCount, clickRightCount;
        double resolution;
        Point start, end;
        Vector OrthogonalVector;
        bool priemetna;
        double t, s;

        public MainWindow()
        {
            InitializeComponent();
            Points = new List<Point>();
            GrayPoints = new List<Point>();
            Lines = new List<Line>();
            FarbyUseciek = new List<Brush>();
            rnd = new Random();

            PointsOnGrayLine = new List<Point>();
            Middle = new List<Point>();
            OrthogonalVector = new Vector();

            numberOfLines.Content = 0;
            resolution = Convert.ToDouble(ParameterPX.Text);
            clickLeftCount = 0;
            clickRightCount = 0;
            priemetna = false;
        }
        private SolidColorBrush NahodnaFarba() // vygeneruje nahodnu farbu
        {
            byte r, g, b;
            r = Convert.ToByte(rnd.Next(256));
            g = Convert.ToByte(rnd.Next(256));
            b = Convert.ToByte(rnd.Next(256));
            SolidColorBrush Farba = new SolidColorBrush(Color.FromRgb(r, g, b));
            return Farba;
        }
        private double CrossProduct(Vector u, Vector v) // vektorovy sucin pre vektory
        {
            double cross;
            cross = u.X * v.Y - u.Y * v.X;
            return cross;
        } 
        private bool FindParameter(Point A, Point B, Vector u, Vector v) // test, ci maju usecky spolocny prienik 
        {
            t = CrossProduct(B - A, v) / CrossProduct(u, v);             // zrataju sa parametre
            s = CrossProduct(A - B, u) / CrossProduct(v, u);
            if (t <= 1 && t >= 0 && s >= 0 && s <= 1)                   // ak su v spravnom intervale, tak mame prienik
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        private void Zbuffer_Click(object sender, RoutedEventArgs e) // z-buffer 
        {
            if (Points.Count % 2 == 1)                              // podmienka pre spravny beh programu
            {
                MessageBox.Show("Zadaj párny počet bodov!");
            }
            if (Points.Count % 2 == 0)                              // ak mame usecky, rozdelime sivu usecku, vysleme luce a hladame prieniky
            {
                DivideGrayLine();
                RayCast();
                FindIntersection();
            }
        }
        private void FindIntersection()
        {
            List<Brush> LinesColor = new List<Brush>();      // tento list ma rovnaky pocet prvkov ako List Middle
            for (int j = 0; j < Middle.Count; j++)           // pre kazdy luc vytvorime list prienikov a ich farieb
            {
                List<Point> Intersections = new List<Point>();
                List<Brush> IntersectionsColors = new List<Brush>();

                double d;
                double min = 10000;
                int remember = 0;

                for (int i = 0; i < Points.Count; i = i + 2)    // najdeme vsetky prieniky
                {
                    if (FindParameter(Points[i], Middle[j], Points[i + 1] - Points[i], OrthogonalVector) == true) // vyrata vsetky prieniky
                    {
                        Point I = new Point();
                        I.X = Middle[j].X + s * OrthogonalVector.X;
                        I.Y = Middle[j].Y + s * OrthogonalVector.Y;
                        Intersections.Add(I);

                        //DrawPoint(Intersections, g, Colors.Red);     // vykresli vsetky prieniky
                        IntersectionsColors.Add(Lines[i / 2].Stroke);
                    }
                }
                                                                                                  
                for (int k = 0; k < Intersections.Count; k++)           // najde najblizsi prienik na luci
                {
                    d = Math.Sqrt(Math.Pow(Middle[j].X - Intersections[k].X, 2) + Math.Pow(Middle[j].Y - Intersections[k].Y, 2));
                    if (d < min)
                    {
                        min = d;
                        remember = k;
                    }
                }

                if (Intersections.Count > 0)                            // ak ma luc prienik s useckou, priradi farbu
                {
                    /*Ellipse e = new Ellipse();                        // toto by vykreslilo bod prieniku s farbou usecky
                    e.Fill = Brushes[remember];
                    e.Width = 4;
                    e.Height = 4;
                    Canvas.SetLeft(e, Intersections[remember].X - 2);
                    Canvas.SetTop(e, Intersections[remember].Y - 2);
                    Canvas.SetZIndex(e, 5);
                    g.Children.Add(e);*/
                    LinesColor.Add(IntersectionsColors[remember]);      // zapamatame si farbu najblizsieho prieniku
                }
                else
                {
                    LinesColor.Add(new SolidColorBrush(Colors.Gray));     // ak nema prienik, priradi znova sivu farbu
                }

                Line L = new Line();                                     // rozkuskuje sivu usecku a priradi usekom farbu
                L.Stroke = LinesColor[j];
                L.StrokeThickness = 3;

                L.X1 = PointsOnGrayLine[j].X;
                L.Y1 = PointsOnGrayLine[j].Y;
                L.X2 = PointsOnGrayLine[j + 1].X;
                L.Y2 = PointsOnGrayLine[j + 1].Y;

                Canvas.SetZIndex(L, 3);
                g.Children.Add(L);
            }
        }

        private void RayCast()
        {
            start.X = GrayPoints[0].X;
            start.Y = GrayPoints[0].Y;
            end.X = GrayPoints[1].X;
            end.Y = GrayPoints[1].Y;

            OrthogonalVector.X = 10 * (end.Y - start.Y);      // vypocita kolmy vektor na priamku a vynasobu dostatocne velkym skalarom na ratanie prienikov v celom canvase
            OrthogonalVector.Y = 10 * (start.X - end.X);      // x = end.X - start.X, y = end.Y - start.Y, (y, -x) je kolmy vektor

            for (int i = 0; i < Middle.Count; i++)            // vsetky luce maju rovnaky smerovy vektor OrthogonalVector
            {
                Line L = new Line();
                L.X1 = Middle[i].X;
                L.Y1 = Middle[i].Y;
                L.X2 = Middle[i].X + OrthogonalVector.X;
                L.Y2 = Middle[i].Y + OrthogonalVector.Y;
                //Rays.Add(L);
                //L.Stroke = new SolidColorBrush(Colors.LightGray);             // nechceme ich vykreslovat
                //L.StrokeThickness = 1;
                //Canvas.SetZIndex(L, 2);
                //g.Children.Add(L);
            }
        }

        public void DrawPoint(List<Point> CV, Canvas C, Color Color)
        {
            Ellipse e = new Ellipse();                          // kresli body
            e.Fill = new SolidColorBrush(Color);
            e.Width = 4;
            e.Height = 4;
            Canvas.SetLeft(e, CV[CV.Count - 1].X - 2);
            Canvas.SetTop(e, CV[CV.Count - 1].Y - 2);
            Canvas.SetZIndex(e, 5);
            C.Children.Add(e);
        }

        public void DrawLines(List<Point> CV, Canvas C, SolidColorBrush Farba, List<Line> Lines)
        {
            Line L = new Line();            // kresli usecky
            L.Stroke = Farba;
            FarbyUseciek.Add(Farba);
            L.StrokeThickness = 2;

            L.X1 = CV[CV.Count - 2].X;
            L.Y1 = CV[CV.Count - 2].Y;
            L.X2 = CV[CV.Count - 1].X;
            L.Y2 = CV[CV.Count - 1].Y;

            Canvas.SetZIndex(L, 2);
            Lines.Add(L);
            C.Children.Add(L);
        }

        private void g_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            clickLeftCount = Points.Count % 2;              // ak je pocet bodov parny, vyklesli priamku

            Points.Add(e.GetPosition(g));
            DrawPoint(Points, g, Colors.DarkBlue);

            if (clickLeftCount == 1)
            {
                DrawLines(Points, g, NahodnaFarba(), Lines);
            }
            numberOfLines.Content = Lines.Count;
        }

        private void g_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            clickRightCount++;
            if (clickRightCount == 1)               // pocet klikov
            {
                g.Children.Clear();                 // canvas sa vymaze a znova sa nakreslia body a usecky
                GrayPoints.Clear();
                PointsOnGrayLine.Clear();
                Middle.Clear();

                for (int i = 0; i < Points.Count; i++)
                {
                    Ellipse E = new Ellipse();
                    E.Fill = new SolidColorBrush(Colors.DarkBlue);
                    E.Width = 4;
                    E.Height = 4;
                    Canvas.SetLeft(E, Points[i].X - 2);
                    Canvas.SetTop(E, Points[i].Y - 2);
                    Canvas.SetZIndex(E, 5);
                    g.Children.Add(E);
                }

                for (int i = 0; i < Points.Count; i = i + 2)
                {
                    Line L = new Line();
                    L.Stroke = FarbyUseciek[i/2];
                    L.StrokeThickness = 2;

                    L.X1 = Points[i].X;
                    L.Y1 = Points[i].Y;
                    L.X2 = Points[i + 1].X;
                    L.Y2 = Points[i + 1].Y;

                    Canvas.SetZIndex(L, 2);
                    g.Children.Add(L);
                }
                
                GrayPoints.Add(e.GetPosition(g));
                DrawPoint(GrayPoints, g, Colors.Red);
            }
            else if (clickRightCount == 2)                  // ak mame dva body, vykresli sa siva usecka
            {
                GrayPoints.Add(e.GetPosition(g));
                DrawPoint(GrayPoints, g, Colors.Red);

                SivaUsecka = new Line();
                SivaUsecka.Stroke = new SolidColorBrush(Colors.Gray);
                SivaUsecka.StrokeThickness = 2;

                SivaUsecka.X1 = GrayPoints[0].X;
                SivaUsecka.Y1 = GrayPoints[0].Y;
                SivaUsecka.X2 = GrayPoints[1].X;
                SivaUsecka.Y2 = GrayPoints[1].Y;

                Canvas.SetZIndex(SivaUsecka, 1);
                g.Children.Add(SivaUsecka);

                priemetna = true;
                clickRightCount = 0;
            }   
        }

        private void Reset_Click(object sender, RoutedEventArgs e)      // vsetko sa vymaze
        {
            g.Children.Clear();

            Points.Clear();
            Lines.Clear();
            GrayPoints.Clear();
            FarbyUseciek.Clear();

            PointsOnGrayLine.Clear();
            Middle.Clear();

            priemetna = false;
            numberOfLines.Content = 0;
        }

        private void DivideGrayLine()
        {
            double dx, dy;
            if (priemetna == true)
            {
                start.X = GrayPoints[0].X;
                start.Y = GrayPoints[0].Y;
                end.X = GrayPoints[1].X;
                end.Y = GrayPoints[1].Y;
                PointsOnGrayLine.Add(start);

                dx = (end.X - start.X) / resolution;
                dy = (end.Y - start.Y) / resolution;

                for (int i = 0; i < resolution; i++)            // vyrata mi konce useciek PointsOnGrayLine
                {
                    Point P = new Point();
                    P.X = start.X + dx;
                    P.Y = start.Y + dy;

                    PointsOnGrayLine.Add(P);
                    //DrawPoint(PointsOnGrayLine, g, Colors.Red);       // mozno odkomentovat pre vizualizaciu segmentov na GrayLine
                    start.X = P.X;
                    start.Y = P.Y;
                }

                for (int i = 0; i < resolution; i++)        // stredy useciek Middle
                {
                    Point P = new Point();
                    P.X = (PointsOnGrayLine[i].X + PointsOnGrayLine[i + 1].X) / 2;
                    P.Y = (PointsOnGrayLine[i].Y + PointsOnGrayLine[i + 1].Y) / 2;
                    Middle.Add(P);
                    //DrawPoint(Middle, g, Colors.Blue);                // odkomentovat pre vizualizaciu bodov, z ktorych vysielame luc
                }
            }  
        }
        private void DecreaseIter_Click(object sender, RoutedEventArgs e)       // znizi sa pocet usekov na sivej priamke
        {
            if (resolution > 1)
            {
                resolution--;
                ParameterPX.Text = Convert.ToString(resolution);
                PointsOnGrayLine.Clear();
                Middle.Clear();
            }
            else if (resolution == 1)
            {
                MessageBox.Show("Dosiahol si minimálne rozlíšenie!");
            }
        }

        private void IncreaseIter_Click(object sender, RoutedEventArgs e)      // zvysi sa pocet usekov na sivej priamke
        {
            resolution++;
            ParameterPX.Text = Convert.ToString(resolution);
            PointsOnGrayLine.Clear();
            Middle.Clear();
        }
    }
}
