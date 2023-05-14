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
        Font drawFont = new Font("Arial", 20);
        SolidBrush drawBrush_non_edit = new SolidBrush(Color.White);
        SolidBrush drawBrush_edit = new SolidBrush(Color.Black);
        StringFormat drawFormat = new StringFormat();

        public Form1 form;
        public Board board;

        public bool editable = false;
        public bool correct = true;
        public string value;

        public int X;
        public int Y;

        public int xb;
        public int yb;

        public int block_color;

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

            //color_cells(e, p);

            if (!this.correct && this.editable)
            {
                e.FillPolygon(Brushes.OrangeRed, p);
            }
            else if (this.editable)
            {
                Brush brush = new SolidBrush(form.editable_cells_color);
                e.FillPolygon(brush, p);
            }
            
            e.DrawPolygon(form.pen, p);

            //почему именно такие +7 и прочее? просто потому что блять (мб проблема в width)
            if(!this.editable)
            {
                e.DrawString(value, drawFont, drawBrush_non_edit, X + board.cellwidth / 4 + 9,
                    Y + board.cellwidth / 4 + 4, drawFormat);
            } else
            {
                e.DrawString(value, drawFont, drawBrush_edit, X + board.cellwidth / 4 + 9, 
                    Y + board.cellwidth / 4 + 4, drawFormat);
            }
        }

        public bool check_correctness()
        {
            for (int i = 0; i < Board.N; i++)
            {
                if(!check_block(i))
                {
                    return false;
                }
                if(!check_lines(i))
                {
                    return false;
                }
            }
            return true;
        }

        public bool check_block(int i)
        {
            int x = (xb / 3) * 3 + i % 3;
            int y = (yb / 3) * 3 + i / 3;
            if (x == xb && y == yb)
            {
                return true;
            }
            if (board.cells[x, y].value == this.value)
            {
                return false;
            }

            return true;
        }

        public bool check_lines(int i)
        {
            if (i == xb || i == yb)
            {
                return true;
            }

            return !(board.cells[i, yb].value == this.value) && !(board.cells[xb, i].value == this.value);
        }

        public void color_cells(Graphics e, Point[] p)
        {
            switch (block_color)
            {
                case 0:
                    e.FillPolygon(Brushes.Gold, p);
                    break;
                case 1:
                    e.FillPolygon(Brushes.Orange, p);
                    break;
                case 2:
                    e.FillPolygon(Brushes.LawnGreen, p);
                    break;
                case 3:
                    e.FillPolygon(Brushes.LightPink, p);
                    break;
                case 4:
                    e.FillPolygon(Brushes.Tan, p);
                    break;
                case 5:
                    e.FillPolygon(Brushes.Thistle, p);
                    break;
                case 6:
                    e.FillPolygon(Brushes.Plum, p);
                    break;
                case 7:
                    e.FillPolygon(Brushes.OliveDrab, p);
                    break;
                case 8:
                    e.FillPolygon(Brushes.MintCream, p);
                    break;
            }
        }
    }
}
