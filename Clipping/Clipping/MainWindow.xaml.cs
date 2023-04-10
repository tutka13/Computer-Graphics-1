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

namespace Clipping
{
    public partial class MainWindow : Window
    {
        List<Point> Points;
        List<Point> Polygone;
        List<Point> ClippedPoints;

        List<Line> Lines;
        List<Line> PolygoneLines;

        Point PoziciaMyse;
        Point OznacenyBod;

        double startX, startY, endX, endY;
        Random rnd;
        Rectangle R;
        bool isPressed, isClosed, inicializedWindow;
        int clickLeftCount;

        public MainWindow()
        {
            InitializeComponent();
            Points = new List<Point>();
            ClippedPoints = new List<Point>();
            Polygone = new List<Point>();

            Lines = new List<Line>();
            PolygoneLines = new List<Line>();

            rnd = new Random();
            R = new Rectangle();
            isPressed = false;
            isClosed = false;
            inicializedWindow = false;

            clickLeftCount = 0;
        }
        private SolidColorBrush NahodnaFarba() // vygeneruje nahodnu farbu
        {
            byte r, g, b;
            r = Convert.ToByte(rnd.Next(256));
            g = Convert.ToByte(rnd.Next(256));
            b = Convert.ToByte(rnd.Next(256));
            SolidColorBrush LineColor = new SolidColorBrush(Color.FromRgb(r, g, b));
            return LineColor;
        }
        public void DrawPoint(List<Point> List, SolidColorBrush Color)  // kresli body
        {
            Ellipse e = new Ellipse();                          
            e.Fill = Color;
            e.Width = 4;
            e.Height = 4;
            Canvas.SetLeft(e, List[List.Count - 1].X - 2);
            Canvas.SetTop(e, List[List.Count - 1].Y - 2);
            Canvas.SetZIndex(e, 5);
            g.Children.Add(e);
        }
        public void DrawAllPoints(List<Point> List, SolidColorBrush Color)  // vykresli vsetky body v Liste
        {
            for (int i = 0; i < List.Count; i++)
            {
                Ellipse e = new Ellipse();                          
                e.Fill = Color;
                e.Width = 4;
                e.Height = 4;
                Canvas.SetLeft(e, List[i].X - 2);
                Canvas.SetTop(e, List[i].Y - 2);
                Canvas.SetZIndex(e, 5);
                g.Children.Add(e);
            }
        }
        public void DrawLine(Point A, Point B, SolidColorBrush Color, double thickness, List<Line> List)  // vykreslenie usecky medzi dvoma bodmi s pridanim usecky do listu
        {
            Line L = new Line();
            L.Stroke = Color;
            L.StrokeThickness = thickness;
            L.X1 = A.X;
            L.Y1 = A.Y;
            L.X2 = B.X;
            L.Y2 = B.Y;
            Canvas.SetZIndex(L, 2);
            List.Add(L);
            g.Children.Add(L);
        }
        public byte[] Code(Point A)  // vytvori kod pre bod v poradi LRDU
        {
            byte[] code = new byte[4];
            if (A.X < startX)
            {
                code[0] = 1;
            }
            else
            {
                code[0] = 0;
            }
            if (A.X > endX)
            {
                code[1] = 1;
            }
            else
            {
                code[1] = 0;
            }
            if (A.Y > endY)
            {
                code[2] = 1;
            }
            else
            {
                code[2] = 0;
            }
            if (A.Y < startY)
            {
                code[3] = 1;
            }
            else
            {
                code[3] = 0;
            }
            return code;
        }
        public void Accept(Point A, Point B)    // prida body do listu na dalsie vykreslovanie
        {
            ClippedPoints.Add(A);
            ClippedPoints.Add(B);
        }
        public void Clipping(Line L, Point A, Point B)     // metodu bude volat len ked bude inicializedWindow
        {
            double k = (B.Y - A.Y) / (B.X - A.X);   // smernica pre y
            double reck = (B.X - A.X) / (B.Y - A.Y); // obratena smernica pre x
                                                        
            //L
            if (Code(A)[0] == 1)                // ak je na nejakom mieste z code cislica 1, idem podla toho orezavat a priradit bodu nove suradnice
            {
                A.Y = A.Y + k * (startX - A.X);
                A.X = startX;
            }
            if (Code(B)[0] == 1)
            {
                B.Y = B.Y + k * (startX - B.X);
                B.X = startX;  
            }

            //R
            if (Code(A)[1] == 1)
            {
                A.Y = A.Y + k * (endX - A.X);
                A.X = endX;
            }
            if (Code(B)[1] == 1)
            {
                B.Y = B.Y + k * (endX - B.X);
                B.X = endX;
            }
            
            //D
            if (Code(A)[2] == 1)
            {
                A.X = A.X + reck * (endY - A.Y);
                A.Y = endY;
            }
            if (Code(B)[2] == 1)
            {
                B.X = B.X + reck * (endY - B.Y);
                B.Y = endY;
            }

            //U
            if (Code(A)[3] == 1)
            {
                A.X = A.X + reck * (startY - A.Y);
                A.Y = startY;
            }
            if (Code(B)[3] == 1)
            {
                B.X = B.X + reck * (startY - B.Y);
                B.Y = startY;
            }

            ClippedPoints.Add(A);
            ClippedPoints.Add(B);
            L.X1 = A.X;
            L.Y1 = A.Y;
            L.X2 = B.X;
            L.Y2 = B.Y;
        }
        public byte[] LogicOR(byte[] pole1, byte[] pole2)    // operacia alebo po bytoch
        {
            byte[] code = new byte[4];                      
            for (int i = 0; i < code.Length; i++)
            {
                if (pole1[i] == 0 && pole2[i] == 0)
                {
                    code[i] = 0;
                }
                else if (pole1[i] == 1 && pole2[i] == 1)
                {
                    code[i] = 1;
                }
                else if (pole1[i] == 1 && pole2[i] == 0)
                {
                    code[i] = 1;
                }
                else if (pole1[i] == 0 && pole2[i] == 1)
                {
                    code[i] = 1;
                }
            }
            return code;
        }   
        public byte[] LogicAND(byte[] pole1, byte[] pole2)  // operacia a po bytoch
        {
            byte[] code = new byte[4];
            for (int i = 0; i < code.Length; i++)
            {
                if (pole1[i] == 0 && pole2[i] == 0)
                {
                    code[i] = 0;
                }
                else if (pole1[i] == 1 && pole2[i] == 1)
                {
                    code[i] = 1;
                }
                else if (pole1[i] == 1 && pole2[i] == 0)
                {
                    code[i] = 0;
                }
                else if (pole1[i] == 0 && pole2[i] == 1)
                {
                    code[i] = 0;
                }
            }
            return code;
        }   
        public void CSAlgorithm()   // Cohen-Sutherland algoritmus
        {
            byte[] zeros = new byte[4];     // pole nul, s ktorym budeme porovnavat
            byte[] A, B, C, D;
            g.Children.Clear();             // predtym vymazeme canvas

            foreach (Line L in Lines)
            {
                Point P = new Point();
                P.X = L.X1;
                P.Y = L.Y1;
                Point R = new Point();
                R.X = L.X2;
                R.Y = L.Y2;

                A = Code(P);    // ulozene kody a operacie s nimi 
                B = Code(R);
                C = LogicOR(A, B);
                D = LogicAND(A, B);

                // if LogicOR == zeros => accept line
                if (C[0] == zeros[0] && C[1] == zeros[1] && C[2] == zeros[2] && C[3] == zeros[3])     // lezi v okne
                {
                    Accept(P, R);
                    g.Children.Add(L);                                                                // prida povodnu usecku do canvasu
                }
                // else if LogicAND == zeros => clip line
                else if (D[0] == zeros[0] && D[1] == zeros[1] && D[2] == zeros[2] && D[3] == zeros[3])
                {
                    Clipping(L, P, R);
                    g.Children.Add(L);                                                               // prida orezanu usecku do canvasu
                }
                // if LogicAND != zeros => reject line
                else
                {
                                                                                                     // neprida nic
                }
            }
            DrawAllPoints(ClippedPoints, new SolidColorBrush(Colors.DarkBlue));                   // prida do canvasu nove body
            g.Children.Add(R);                                                                   // nakoniec prida do canvasu okno
        }
        private void g_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)   //pomocou nej klikame body, za splnenych podmienok spusti oba algoritmy
        {
            clickLeftCount = Points.Count % 2;              // ak je pocet bodov parny, vykresli usecku
            Points.Add(e.GetPosition(g));                   // kliknutim prida bod do listu a do canvase
            DrawPoint(Points, new SolidColorBrush(Colors.DarkBlue));

            if (clickLeftCount == 1 && Points.Count > 1)    // kresli usecky, ktore budeme orezavat
            {
                DrawLine(Points[Points.Count - 2], Points[Points.Count - 1], NahodnaFarba(), 2, Lines);
            }            

            if (inicializedWindow)      // ak mame okno, urobi Cohen-Sutherland
            {
                CSAlgorithm();
            }
            if (isClosed)               // ak mame polygon, urobi Cyrus-Beck
            {
                CBAlgorithm();
            }
        }
        private void g_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (CohenSutherland.IsChecked == true)                                              // kreslenie okna
            {
                OznacenyBod = new Point(Mouse.GetPosition(g).X, Mouse.GetPosition(g).Y);                    // kde som klikol
                isPressed = true;

                if (inicializedWindow)
                {
                    g.Children.Clear();
                    Lines.Clear();
                    Points.Clear();
                    ClippedPoints.Clear();
                }
            }
            if (CyrusBeck.IsChecked == true)                                                        // kreslenie polygonu
            {
                if (isClosed == false)
                {
                    Polygone.Add(e.GetPosition(g));
                    DrawPoint(Polygone, new SolidColorBrush(Colors.Red));

                    if (Polygone.Count > 1)
                    {
                        DrawLine(Polygone[Polygone.Count - 2], Polygone[Polygone.Count - 1], new SolidColorBrush(Colors.Red), 3, PolygoneLines);
                    } 
                }
            }
        }
        private void CohenSutherland_Checked(object sender, RoutedEventArgs e)
        {
            g.Children.Clear();     // nastavi povodne hodnoty
            if (Points != null)
            {
                Points.Clear();
                ClippedPoints.Clear();
                Polygone.Clear();

                Lines.Clear();
                PolygoneLines.Clear();

                isClosed = false;
                isPressed = false;
                inicializedWindow = false;

                clickLeftCount = 0;
            }
        }

        private void CyrusBeck_Checked(object sender, RoutedEventArgs e)
        {
            g.Children.Clear(); // nastavi povodne hodnoty
            if (Points != null)
            {
                Points.Clear();
                ClippedPoints.Clear();
                Lines.Clear();

                isClosed = false;
                isPressed = false;
                inicializedWindow = false;

                clickLeftCount = 0;
            }
        }

        private void g_MouseDown(object sender, MouseButtonEventArgs e) //uzatvori polygon a spusti CB
        {
            if (e.ChangedButton == MouseButton.Middle && e.ButtonState == MouseButtonState.Pressed) // ak je stlacene stredne tlacidlo myse
            {
                if (isClosed == false && CyrusBeck.IsChecked == true && Polygone.Count > 2) // a polygon nie je uzavrety
                {
                    DrawLine(Polygone[Polygone.Count - 1], Polygone[0], new SolidColorBrush(Colors.Red), 3, PolygoneLines);     // uzatvori polygon
                    isClosed = true;
                    CBAlgorithm();
                }
            }
        }

        private void g_MouseMove(object sender, MouseEventArgs e)   //kresli okno
        {
            PoziciaMyse = new Point(Mouse.GetPosition(g).X, Mouse.GetPosition(g).Y);                    // zistujem poziciu myse pri jej hybani 
            if (isPressed)                                                                      // ak je navyse stlacene prave tlacidlo myse, vytvaram jej tahanim obdlznik 
            {
                g.Children.Clear();

                startX = Math.Min(OznacenyBod.X, PoziciaMyse.X);                                        // test: odkial kam taham obdlznik
                startY = Math.Min(OznacenyBod.Y, PoziciaMyse.Y);

                endX = Math.Max(OznacenyBod.X, PoziciaMyse.X);
                endY = Math.Max(OznacenyBod.Y, PoziciaMyse.Y);

                R.Width = endX - startX;
                R.Height = endY - startY;

                R.Stroke = new SolidColorBrush(Colors.DarkBlue);
                R.Fill = new SolidColorBrush(Color.FromArgb(15, 15, 15, 255));
                Canvas.SetLeft(R, startX);
                Canvas.SetTop(R, startY);

                g.Children.Add(R);

                if (inicializedWindow == false)     // ak este nemame okno, kreslia sa povodne usecky
                {
                    DrawAllPoints(Points, new SolidColorBrush(Colors.DarkBlue));

                    foreach (Line L in Lines)
                    {
                        g.Children.Add(L);
                    }
                }
            }
        }

        private void g_MouseRightButtonUp(object sender, MouseButtonEventArgs e)    // po dokresleni okna spusti CS
        {
            if (CohenSutherland.IsChecked == true && isPressed)
            {
                isPressed = false;
                inicializedWindow = true;
                CSAlgorithm();
            }
        }

        private void Generator_Click(object sender, RoutedEventArgs e)
        {
            for (int i = 0; i < 10; i++)       // vykresli 10 useciek
            {
                Point P = new Point();
                P.X = rnd.NextDouble() * g.Width;
                P.Y = rnd.NextDouble() * g.Height;
                Points.Add(P);

                Point R = new Point();
                R.X = rnd.NextDouble() * g.Width;
                R.Y = rnd.NextDouble() * g.Height;
                Points.Add(R);

                DrawLine(P, R, NahodnaFarba(), 2, Lines);
            }

            DrawAllPoints(Points, new SolidColorBrush(Colors.DarkBlue));

            if (inicializedWindow)      // ak mame okno, urobi sa CS
            {
                CSAlgorithm();
            }
            if (isClosed)               // ak mame polygon, urobi sa CB
            {
                CBAlgorithm();
            }
        }

        private void Reset_Click(object sender, RoutedEventArgs e)  // vymaze vsetko potrebne
        {
            g.Children.Clear();

            Points.Clear();
            ClippedPoints.Clear();
            Polygone.Clear();

            Lines.Clear();
            PolygoneLines.Clear();

            isClosed = false;
            isPressed = false;
            inicializedWindow = false;
            clickLeftCount = 0;
        }
        private bool Orientation(List<Point> List)  // metoda urci orientaciu pomocou 3 bodov
        {
            double value = (List[1].Y - List[0].Y) * (List[2].X - List[1].X) - (List[1].X - List[0].X) * (List[2].Y - List[1].Y);
            if (value == 0) // body su kolinearne
            {
                value = (List[2].Y - List[1].Y) * (List[3].X - List[2].X) - (List[2].X - List[1].X) * (List[3].Y - List[2].Y);  // vezmeme dalsiu trojicu
            }

            if (value < 0) //clockwise
            {
                return false;
            }
            else  //anticlockwise - tuto chceme
            {
                return true;
            }
        }
        private void CBAlgorithm()  // Cyrus-Beck algoritmus
        {
            g.Children.Clear();

            foreach (Line L in PolygoneLines)   // vykresli polygon
            {
                g.Children.Add(L);
            }

            foreach (Line K in Lines)
            {
                double t_e = 0;
                double t_l = 1;
                Vector s = new Vector(); // smerovy vektor usecky K

                s.X = K.X2 - K.X1;
                s.Y = K.Y2 - K.Y1;

                foreach (Line L in PolygoneLines)
                {
                    Vector n = new Vector();    // normalovy vektor hrany L
                    Vector m = new Vector();    // vektor (stred hrany L - prvy bod usecky K)

                    Point Middle = new Point();
                    Middle.X = (L.X1 + L.X2) / 2;
                    Middle.Y = (L.Y1 + L.Y2) / 2;

                    // podla orientacie polygonu, vyratame normaly k hranam
                    if (Orientation(Polygone))
                    {
                        n.X = L.Y2 - L.Y1;
                        n.Y = -L.X2 + L.X1;
                    }
                    else
                    {
                        n.X = - L.Y2 + L.Y1;
                        n.Y = L.X2 - L.X1;
                    }
                    
                    m.X = Middle.X - K.X1;
                    m.Y = Middle.Y - K.Y1;

                    double d1 = DotProduct(s, n);
                    double d2 = DotProduct(m, n);

                    if (d1 > 0)  // t je kandidat na t_e
                    {
                        double t = d2 / d1;
                        t_e = Math.Max(t, t_e);
                    }
                    else if (d1 < 0)  // t je kandidat na t_l
                    {
                        double t = d2 / d1;
                        t_l = Math.Min(t, t_l);
                    }
                    else if (d1 == 0 && d2 == 0)    // ak su skalarne suciny nulove, vykresli sa povodna usecka
                    {
                        g.Children.Add(K);
                    }
                    else if (d1 == 0 && d2 != 0)
                    {
                        g.Children.Add(K);
                    }
                }

                if (0 <= t_e && t_e < t_l && t_l <= 1)
                {
                    double oldx = K.X1;         // ulozi si hodnotu
                    double oldy = K.Y1;

                    K.X1 = K.X1 + s.X * t_e;    // vypocita zaciactocny a koncovy bod usecky K
                    K.Y1 = K.Y1 + s.Y * t_e;
                    K.X2 = oldx + s.X * t_l;
                    K.Y2 = oldy + s.Y * t_l;

                    g.Children.Add(K);          // vykresli usecku K

                    Point P = new Point();      // kvoli vykresleniu bodov 
                    P.X = K.X1;
                    P.Y = K.Y1;
                    ClippedPoints.Add(P);

                    Point R = new Point();
                    R.X = K.X2;
                    R.Y = K.Y2;
                    ClippedPoints.Add(R);

                    DrawAllPoints(ClippedPoints, new SolidColorBrush(Colors.DarkBlue));
                }
            }
        }
        private double DotProduct(Vector a, Vector b)   // skalarny sucin
        {
            double d = a.X * b.X + a.Y * b.Y;
            return d;
        }
    }
}
