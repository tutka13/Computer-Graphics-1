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

namespace Scanline
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        List<Point> Points;
        public List<PolyEdge> Edges; // hrany s polyedge strukturou
        public List<PolyEdge>[] TE; // tabulka hran

        int maximumY, minimumY;
        bool isClosed;
        public MainWindow()
        {
            InitializeComponent();
            Grid();
            Edges = new List<PolyEdge>();
            Points = new List<Point>();

            isClosed = false;
        }
        public void Grid()      // vykresli sa mriezka
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
                Canvas.SetZIndex(L, 0);
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
                Canvas.SetZIndex(L, 0);
                g.Children.Add(L);
            }
        }   
        public void DrawPoint(Point P)  // vykreslenie bodu
        {
            Rectangle R = new Rectangle();
            R.Fill = new SolidColorBrush(Colors.DarkBlue);
            R.Width = 4;
            R.Height = 4;
            Canvas.SetLeft(R, P.X - 2);
            Canvas.SetTop(R, P.Y - 2);
            Canvas.SetZIndex(R, 3);
            g.Children.Add(R);
        }   
        public void DrawLine(Point A, Point B)  // vykreslenie usecky medzi dvoma bodmi
        {
            Line L = new Line();
            L.Stroke = new SolidColorBrush(Colors.DarkBlue);
            L.StrokeThickness = 2;
            L.X1 = A.X;
            L.Y1 = A.Y;
            L.X2 = B.X;
            L.Y2 = B.Y;
            Canvas.SetZIndex(L, 2);
            g.Children.Add(L);
        }       
        public void FindExtremes() // najde extremy vzhladom na y
        {
            for (int i = 0; i < Edges.Count; i++)
            {
                if (maximumY < Edges[i].ymax)
                {
                    maximumY = Edges[i].ymax;
                }
                if (minimumY > Edges[i].ymin)
                {
                    minimumY = Edges[i].ymin;
                }
            }
        }
        private void ShortenEdges(PolyEdge e, PolyEdge f)   //skrati hrany
        {
            if (e.ymin == e.ymax)  // e je rovnobezna s x-ovou osou
            {
                if (e.ymin == f.ymin)  // f smeruje nahor, skrati f
                {
                    f.Shorten();
                }
            }
            else if (f.ymin == f.ymax)  // f je rovnobezna s x-ovou osou
            {
                if (e.ymin == f.ymin)  // e smeruje nahor, skrati e
                {
                    e.Shorten();
                }
            }
            else if (e.ymin == f.ymax)   // obe smeruju nadol, skrati e (tu hornu) 
            {
                e.Shorten();
            }
            else if (e.ymax == f.ymin)  // obe smeruju nahor, skrati f
            {
                f.Shorten();
            }
        }   
        public void PreProcessing()
        {
            for (int i = 1; i < Edges.Count; i++)   // skratime hrany
            {
                ShortenEdges(Edges[i - 1], Edges[i]);
            }
            ShortenEdges(Edges[Edges.Count - 1], Edges[0]);
            FindExtremes();                         // najdeme extremy

            int n = maximumY - minimumY + 1;
            TE = new List<PolyEdge>[n];

            for (int y = 0; y < n; y++)                     // naplnam po riadkoch TE hranami
            {
                TE[y] = new List<PolyEdge>();               // pre riadok vytvorim novy list

                for (int i = 0; i < Edges.Count; i++)       // vlozim do neho vsetky hrany, pre ktore sa ich ymin = minimumY + dane y 
                {
                    if (Edges[i].ymin == minimumY + y)
                    {
                        TE[y].Add(Edges[i]);
                    }
                }
            }
        }
        private void g_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)   // lavy klik
        {
            if (isClosed == false)
            {
                Point P = e.GetPosition(g);
                P.X = Convert.ToInt32(P.X - P.X % 10) + 5;              // tymto zaokruhlovanim zabezpecim to, aby bol bod v strede stvorceka a teda aj dobre ratanie dalej
                P.Y = Convert.ToInt32(P.Y - P.Y % 10) + 5;

                Points.Add(P);
                DrawPoint(P);

                if (Points.Count > 1)
                {
                    DrawLine(Points[Points.Count - 2], Points[Points.Count - 1]);
                    Edges.Add(new PolyEdge(Points[Points.Count - 2], Points[Points.Count - 1]));
                }      
            } 
        }   
        private void g_MouseRightButtonDown(object sender, MouseButtonEventArgs e)   // pravy klik
        {
            if (isClosed == false)
            {
                DrawLine(Points[Points.Count - 1], Points[0]);
                Edges.Add(new PolyEdge(Points[Points.Count - 1], Points[0]));
                isClosed = true;
            }
            Scanline();
        }
        private void Scanline()
        {
            PreProcessing();
            List<PolyEdge> TAE = new List<PolyEdge>();      // vytvorim list aktvinych hran

            for (int y = minimumY; y <= maximumY; y++)
            {
                foreach (PolyEdge e in TE[y - minimumY])    // naplnam list aktivnych hran pre indexy 0 az maximumY - minimumY
                {
                    TAE.Add(e); 
                }
                TAE = TAE.OrderBy(e => e.x).ToList();       // usporiadam hrany vzhladom na x

                for (int i = TAE.Count - 1; i >= 0; i--)    // hrany rovnobezne s x-ovou osou 
                {
                    if (TAE[i].rovnobezna)
                    {
                        for (int x = TAE[i].xmin; x <= TAE[i].xmax; x++)    // vykreslim rovnobeznu hranu (bez toho by chybali presne tie pixely, cez ktore usecka prechadza)
                        {
                            ColorIt(x, y);
                        }
                        TAE.RemoveAt(i);                                    // odstranim ju
                    }
                }

                for (int i = 0; i < TAE.Count; i += 2)      // prechadzam vsetky hrany v TAE po dvojiciach
                {
                    for (int x = (int)Math.Round(TAE[i].x); x <= (int)Math.Round(TAE[i + 1].x); x++)
                    {
                        ColorIt(x, y);
                    }
                }

                for (int i = TAE.Count - 1; i >= 0; i--)    // po spracovani 
                {
                    if (TAE[i].ymax == y)           // ak ymax danej hrany sa rovna prave spracovavanemu riadku, odstranim hranu z TAE
                    {
                        TAE.Remove(TAE[i]);
                    }
                    else
                    {
                        TAE[i].XShorten();         // ak nie, skratim x, teda priratam smernicu
                    }
                }
            }
        }
        public void ColorIt(int i, int j)         // vytvori obdlznik na pozicii (i, j)
        {
            Rectangle R = new Rectangle();
            R.Width = 10;
            R.Height = 10;
            R.Fill = new SolidColorBrush(Colors.HotPink);
            Canvas.SetLeft(R, i * 10);
            Canvas.SetTop(R, j * 10);
            Canvas.SetZIndex(R, 1);
            g.Children.Add(R);
        }
        private void Reset_Click(object sender, RoutedEventArgs e)  // tlacidlo reset, vymazanie canvasu a vyprazdnenie listov
        {
            g.Children.Clear();
            Points.Clear();
            Edges.Clear();
            
            Grid();
            isClosed = false;
        } 
    }

    public class PolyEdge
    {
        public int ymax, ymin;
        public double x;                    // x-ova hodnota pre ymin, musi byt double kvoli pricitavaniu obratenej smernice
        private double obratenaSmernica;

        public int xmin, xmax;
        public bool rovnobezna;

        public PolyEdge(Point A, Point B)
        {
            rovnobezna = false;
            if (A.Y > B.Y)
            {
                ymax = Convert.ToInt32(Math.Floor(A.Y / 10));
                ymin = Convert.ToInt32(Math.Floor(B.Y / 10));
                x = Convert.ToInt32(Math.Floor(B.X / 10));
            }
            else
            {
                ymax = Convert.ToInt32(Math.Floor(B.Y / 10));
                ymin = Convert.ToInt32(Math.Floor(A.Y / 10));
                x = Convert.ToInt32(Math.Floor(A.X / 10));
            }

            if (A.Y == B.Y)  // ak je hrana rovnobezna s osou x, potrebuejeme vediet xmin a xmax
            {
                if (A.X > B.X)
                {
                    xmax = Convert.ToInt32(Math.Floor(A.X / 10));
                    xmin = Convert.ToInt32(Math.Floor(B.X / 10));
                }
                else
                {
                    xmax = Convert.ToInt32(Math.Floor(B.X / 10));
                    xmin = Convert.ToInt32(Math.Floor(A.X / 10));
                }
                rovnobezna = true;
            }
            else
            {
                obratenaSmernica = (A.X - B.X) / (A.Y - B.Y);
            }
        }

        public void Shorten()       // skratenie hrany
        {
            x = x + obratenaSmernica;
            ymin++;
        }

        public void XShorten()     // skratenie x-ovej suradnice
        {
            x = x + obratenaSmernica;
        }
    }
}
