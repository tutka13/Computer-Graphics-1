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
    public class QuadTree
    {
        public double x1 = 0, y1 = 0;               // lavy horny roh
        public double x2 = 660, y2 = 660;           // pravy dolny roh
        public int depth = 0;                       // aktualna hlbka stromu
        public int maxDepth = 5;                    // maximalna hlbka stromu

        public Stack<Bod> Zasobnik;
        public QuadTree northEast, southEast, southWest, northWest;         // deti

        public QuadTree(int depth)                  // konstruktor ma jeden parameter: hlbku stromu
        {
            this.depth = depth;
        }

        public void CreateChildren()                // vytvori nove stromy s hlbkou o jeden stupen vyssou
        {
            northEast = new QuadTree(depth + 1);
            southEast = new QuadTree(depth + 1);
            southWest = new QuadTree(depth + 1);
            northWest = new QuadTree(depth + 1);

            northEast.x1 = (x2 + x1) / 2;          // pre kazde dieta sa vyrataju nove suradnice laveho horneho rohu a praveho dolnehu rohu
            northEast.y1 = y1;
            northEast.x2 = x2;
            northEast.y2 = (y1 + y2) / 2;

            southEast.x1 = (x2 + x1) / 2;
            southEast.y1 = (y2 + y1) / 2;
            southEast.x2 = x2;
            southEast.y2 = y2;

            southWest.x1 = x1;
            southWest.y1 = (y2 + y1) / 2;
            southWest.x2 = (x1 + x2) / 2;
            southWest.y2 = y2;

            northWest.x1 = x1;
            northWest.y1 = y1;
            northWest.x2 = (x1 + x2) / 2;
            northWest.y2 = (y1 + y2) / 2;
        }
        public void SubdivideAndInsert(Bod B)
        {
            if (Zasobnik.Count > 1)                     // podmienka: strom nie je prazdny
            {
                if (depth <= maxDepth)                  // podmienka: aktualna hlbka je mensia (rovna) ako maximalna mozna hlbka
                {
                    CreateChildren();
                    Insert(B);                          // vlozim bod podla jeho suradnic

                    northEast.SubdivideAndInsert(B);    // zopakujem pre deti
                    southEast.SubdivideAndInsert(B);
                    southWest.SubdivideAndInsert(B);
                    northWest.SubdivideAndInsert(B);
                }
            }
        }

        public void Insert(Bod B)
        {
            Stack<Bod> ne = new Stack<Bod>();               // pre kazde dieta vytvorim zasobnik
            Stack<Bod> se = new Stack<Bod>();
            Stack<Bod> sw = new Stack<Bod>();
            Stack<Bod> nw = new Stack<Bod>();

            while (Zasobnik.Count != 0)
            {
                B = Zasobnik.Pop();                        // odstranim bod z povodneho zasobnika

                if (B.X >= northEast.x1 & B.X < northEast.x2 & B.Y >= northEast.y1 & B.Y < northEast.y2)      // kontrola do ktoreho kvadranru patri bod, podla suradnic
                {
                    ne.Push(B);                                                                               // pridam do prislusneho zasobnika
                }
                else if (B.X >= southEast.x1 & B.X < southEast.x2 & B.Y >= southEast.y1 & B.Y < southEast.y2)
                {
                    se.Push(B);
                }
                else if (B.X >= southWest.x1 & B.X < southWest.x2 & B.Y >= southWest.y1 & B.Y < southWest.y2)
                {
                    sw.Push(B);
                }
                else if (B.X >= northWest.x1 & B.X < northWest.x2 & B.Y >= northWest.y1 & B.Y < northWest.y2)
                {
                    nw.Push(B);
                }

                northEast.Zasobnik = ne;            // ku kazdemu dietatu mam vytvoreny prazdny zasobnik, tam pridam ten vytvoreny
                southEast.Zasobnik = se;
                southWest.Zasobnik = sw;
                northWest.Zasobnik = nw;
            }
        }

        public void DrawLines(Canvas C)
        {
            if (PointCount() > 1)                                            // ak strom nie je prazdny
            {
                Line V = new Line();                                               // vertikalna usecka 
                V.Stroke = new SolidColorBrush(Colors.DarkBlue);
                V.StrokeThickness = 1;

                Line H = new Line();                                               // horizonatalna usecka
                H.Stroke = new SolidColorBrush(Colors.DarkBlue);
                H.StrokeThickness = 1;

                V.X1 = (x2 + x1) / 2;                                              // prepocitaju sa suradnice
                V.Y1 = y1;
                V.X2 = (x2 + x1) / 2;
                V.Y2 = y2;

                H.X1 = x1;
                H.Y1 = (y2 + y1) / 2;
                H.X2 = x2;
                H.Y2 = (y2 + y1) / 2;

                Canvas.SetZIndex(V, 3);
                Canvas.SetZIndex(H, 3);
                C.Children.Add(V);
                C.Children.Add(H);

                if (northEast != null)                                              // vsetko sa zopakuje pre kazde dieta, kym nie je prazdny
                {
                    northEast.DrawLines(C);
                }
                if (southEast != null)
                {
                    southEast.DrawLines(C);
                }
                if (southWest != null)
                {
                    southWest.DrawLines(C);
                }
                if (northWest != null)
                {
                    northWest.DrawLines(C);
                }
            }
        }
        public int PointCount()           // rekurzivne sa zrata pocet bodov vnutri stromu
        {
            int count = 0;
            if (Zasobnik != null)
            {
                count = count + Zasobnik.Count;
            }
            if (northEast != null)
            {
                count = count + northEast.PointCount();
            }
            if (northEast != null)
            {
                count = count + southEast.PointCount();
            }
            if (southWest != null)
            {
                count = count + southWest.PointCount();
            }
            if (northWest != null)
            {
                count = count + northWest.PointCount();
            }
            return count;
        }
        public void Clear()
        {
            northEast = null;                              // vymazu sa deti
            southEast = null;
            southWest = null;
            northWest = null;

            x1 = 0;                                       // nastavia sa znova povodne suradnice
            y1 = 0;
            x2 = 660;
            y2 = 660;

            Zasobnik.Clear();                            // vymaze sa zasobnik a hlbka sa nastavi na 0
            depth = 0;
        }

        public string WritePath(Bod B)                      // vypise cestu pre kazdy bod
        {
            if (northEast != null)                          // ak nejake dieta nie je prazdne (vytvaraju sa vsetky naraz)
            {
                if (B.X >= northEast.x1 & B.X < northEast.x2 & B.Y >= northEast.y1 & B.Y < northEast.y2)        // urcim, do ktoreho kvadrantu patri bod
                {
                    return "I. - " + northEast.WritePath(B);                                                    // zapisem si cislo kvadrantu a posuniem sa o uroven vyssie k dietatu
                }
                else if (B.X >= southEast.x1 & B.X < southEast.x2 & B.Y >= southEast.y1 & B.Y < southEast.y2)
                {
                    return "II. - " + southEast.WritePath(B);
                }
                else if (B.X >= southWest.x1 & B.X < southWest.x2 & B.Y >= southWest.y1 & B.Y < southWest.y2)
                {
                    return "III. - " + southWest.WritePath(B);
                }
                else if (B.X >= northWest.x1 & B.X < northWest.x2 & B.Y >= northWest.y1 & B.Y < northWest.y2)
                {
                    return "IV. - " + northWest.WritePath(B);
                }
            }
            return "";                                  // inak sa nevypise nic
        }
    }
}
