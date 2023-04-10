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

namespace QuadTree1
{
    public partial class MainWindow : Window
    {
        public List<Bod> Body;                                  // list bodov
        public Stack<Bod> Zasobnik;
        Random rnd;
        QuadTree qt;
        Rectangle R;

        bool IsPressed;                                         // podmienka pre tlacidlo na mysi
        Point OznacenyBod;
        Point PoziciaMyse;
        double startX, startY, endX, endY;
        int depth;                                              // hlbka stromu

        public MainWindow()
        {
            InitializeComponent();
            Body = new List<Bod>();
            rnd = new Random();
            qt = new QuadTree(depth);
            R = new Rectangle();
            IsPressed = false;
            depth = 0;
        }

        public void g_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            Bod V = new Bod(Mouse.GetPosition(g).X, Mouse.GetPosition(g).Y, g);    // ak kliknem do canvasu, vytvori sa novy bod a ten pridam do listu a zatriedim ho do struktury stromu
            Body.Add(V);
            Zasobnik = new Stack<Bod>(Body);
            qt.Zasobnik = Zasobnik;
            qt.SubdivideAndInsert(V);
            qt.DrawLines(g);
        }

        private void Reset_Click(object sender, RoutedEventArgs e)
        {
            Body.Clear();                                       // vymaze prvky listu
            g.Children.Clear();                                 // vymaze vsetko z canvasu
            if (Zasobnik != null)                               // vymaze cely strom
            {
                qt.Clear();
            }                                       
            PathText.Text = " ";                                // vymaze cestu
        }

        private void RandomPoints_Click(object sender, RoutedEventArgs e)
        {
            Reset_Click(this, e);
            int pocetNahodnychBodov = rnd.Next(10, 100);        // vygeneruje cislo medzi 10 az 100, ktore zodpovie vykreslenym bodom
            double randomX, randomY;
            for (int i = 0; i < pocetNahodnychBodov; i++)
            {
                randomX = rnd.Next(7, 650);                     // vyrata nahodne suradnice pre kazdy bod
                randomY = rnd.Next(4, 650);
                Bod V = new Bod(randomX, randomY, g);           // vytvori sa novy bod na nahodnej pozicii
                Body.Add(V);

                Zasobnik = new Stack<Bod>(Body);                // bod zatriedim do struktury stromu
                qt.Zasobnik = Zasobnik;
                qt.SubdivideAndInsert(V);
                qt.DrawLines(g);
            }
        }
        private void g_MouseRightButtonDown(object sender, MouseButtonEventArgs e)              
        {  
            OznacenyBod = new Point(Mouse.GetPosition(g).X, Mouse.GetPosition(g).Y);                    // kde som klikol
            for (int i = 1; i < Body.Count; i++)
            {
                if (Math.Abs(OznacenyBod.X - Body[i].X) < 5 & Math.Abs(OznacenyBod.Y - Body[i].Y) < 5)  // test: nachadza sa v tesnej blizkosti uz vykresleny bod stromu?
                {
                    PathText.Text = "Koreň - " + qt.WritePath(Body[i]) + "bod.";   // vypise cestu od korena az po vykresleny bod stromu
                }
            }

            IsPressed = true;
        }
        private void g_MouseMove(object sender, MouseEventArgs e)
        {
            PoziciaMyse = new Point(Mouse.GetPosition(g).X, Mouse.GetPosition(g).Y);                    // zistujem poziciu myse pri jej hybani 
            if (IsPressed == true)                                                                      // ak je navyse stlacene prave tlacidlo myse, vytvaram jej tahanim obdlznik 
            {
                g.Children.Clear();

                startX = Math.Min(OznacenyBod.X, PoziciaMyse.X);                                        // test: odkial kam taham obdlznik
                startY = Math.Min(OznacenyBod.Y, PoziciaMyse.Y);

                endX = Math.Max(OznacenyBod.X, PoziciaMyse.X);
                endY = Math.Max(OznacenyBod.Y, PoziciaMyse.Y);

                R.Width = endX - startX;
                R.Height = endY - startY;

                R.Stroke = new SolidColorBrush(Colors.MediumVioletRed);
                R.Fill = new SolidColorBrush(Color.FromArgb(70, 250, 152, 214));
                Canvas.SetLeft(R, startX);
                Canvas.SetTop(R, startY);

                g.Children.Add(R);                   
                ActivatePoints(R, startX, startY);                                                      // oznacim body nachadzajuce sa v obdlzniku
                DrawPoints();                                                                           // predtym som musel zmazat plochu, vykreslim body a strukturu este raz
                qt.DrawLines(g);
            } 
        }
           
        private void g_MouseRightButtonUp(object sender, MouseButtonEventArgs e)                        // pri pusteni praveho tlacidla myse odstranim obdlznik z canvasu a body prestanu byt oznacene
        {
            if (IsPressed == true)
            {
                IsPressed = false;
                g.Children.Remove(R);
                DeactivatePoints(R, startX, startY);
            }
        }

        public void DrawPoints()                                                                        // vykresli mi vsetky kliknute body 
        {
            for (int i = 0; i < Body.Count; i++)
            {
                Body[i].DrawPoint();
            }
        }
        public void ActivatePoints(Rectangle r, double x, double y)                  // vyznaci body inou farbou
        {
            for (int i = 0; i < Body.Count; i++)
            {
                if (Body[i].X > x & Body[i].X < x + r.Width & Body[i].Y > y & Body[i].Y < y + r.Height)
                {
                    Body[i].ChangeColor(Colors.MediumTurquoise);
                }
            } 
        }
        private void DeactivatePoints(Rectangle r, double x, double y)            // zmeni body na povodnu farbu
        {
            for (int i = 0; i < Body.Count; i++)
            {
                if (Body[i].X > x & Body[i].X < x + r.Width & Body[i].Y > y & Body[i].Y < y + r.Height)
                {
                    Body[i].ChangeColor(Colors.MediumVioletRed);
                }
            }
        }
    }
}
