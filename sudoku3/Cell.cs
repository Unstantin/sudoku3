using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace sudoku3
{
    public class Cell
    {
        static Font drawFont = new Font("Arial", 20);
        static SolidBrush drawBrush = new SolidBrush(Color.Black);
        static StringFormat drawFormat = new StringFormat();

        public Form1 form;
        public Board board;

        public bool editable = false;
        public bool correct = true;
        public string value;

        public int X;
        public int Y;

        public int xb;
        public int yb;

        public Cell(string value, Board board, int X, int Y, int xb, int yb) { 
            this.value = value;

            this.board = board;
            this.form = board.form;

            this.X = X;
            this.Y = Y;

            this.xb = xb;
            this.yb = yb;
        }

        public void draw(Graphics e)
        {
            Point[] p = new Point[4] { new Point(X, Y), 
                                       new Point(X + board.cellwidth, Y),
                                       new Point(X +  board.cellwidth, Y +  board.cellwidth), 
                                       new Point(X, Y +  board.cellwidth) };

            if (!this.correct)
            {
                e.FillPolygon(Brushes.OrangeRed, p);
            }
            else if (this.editable)
            {
                e.FillPolygon(Brushes.LightBlue, p);
            }
            

            e.DrawPolygon(form.pen, p);

            //почему именно такие +7 и прочее? просто потому что блять (мб проблема в width)
            e.DrawString(value, drawFont, drawBrush, X + board.cellwidth / 4 + 9, Y + board.cellwidth / 4 + 4, drawFormat);
        }

        public bool check_correctness()
        {
            for(int i = 0; i < Board.N; i++)
            {

                int x = (xb / 3) * 3 + i % 3;
                int y = (yb / 3) * 3 + i / 3;
                //board.cells[x, y].value = "A";
                //form.Invalidate();
                Console.WriteLine(x + " " + y);
                if (x == xb || y == yb)
                {
                    continue;
                }
                if (board.cells[y,x].value == this.value)
                {
                    Console.WriteLine("BBBBBB");
                    Console.WriteLine(x + " " + y);
                    return false;
                }

                /*if (i == xb || i == yb)
                {
                    continue;
                }
                if (board.cells[i, yb].value == this.value)
                {
                    Console.WriteLine("AAAAAA");
                    Console.WriteLine(i + " " + yb);
                    return false;
                }
                if (board.cells[xb, i].value == this.value)
                {
                    Console.WriteLine("CCCCCC");
                    Console.WriteLine(xb + " " + i);
                    return false;
                }*/
            }
            //Console.WriteLine(xb + " " + yb);
            Console.WriteLine();
            return true;
        }
    }
}
