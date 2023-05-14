using Microsoft.VisualBasic.ApplicationServices;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing.Drawing2D;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Markup;

namespace sudoku3
{
    public class Board
    {
        public Form1 form;

        public static int N = 9;
        public Cell[,] cells;
        public Cell active_cell = null;
        public int empty_cells_n = 0;

        public int width;
        public int cellwidth;
        public int X;
        public int Y;

        public Board(Form1 form)
        {
            this.form = form;
            this.width = Convert.ToInt32(form.ClientSize.Height * 0.7);
            this.cellwidth = width / N;

            this.X = (form.ClientSize.Width - this.width) / 2;
            this.Y = (form.ClientSize.Height - this.width) / 2;

            cells = new Cell[N, N];
            for (int i = 0; i < N; i++)
            {
                for (int j = 0; j < N; j++)
                {
                    cells[j, i] = new Cell("", this, X + cellwidth * j, Y + cellwidth * i, j, i);
                }
            }

            Generator.classic(cells);
            //ColorGenerator.color(cells);
        }

        public void draw(Graphics e)
        {
            foreach (Cell c in cells)
            {
                c.draw(e);
            }
            draw_add_grid(e);
        }

        public void draw_add_grid(Graphics e)
        {
            for(int i = 0; i < 4; i++)
            {
                //почему именно такие +1 -2 и прочее? просто потому что блять (мб проблема в width)
                e.DrawLine(new Pen(Color.FromArgb(0, 0, 0), 5), 
                           new Point(X - 2, Y + cellwidth*3*i), 
                           new Point(X + width + 3, Y + cellwidth * 3 * i));
                e.DrawLine(new Pen(Color.FromArgb(0, 0, 0), 5), 
                           new Point(X + cellwidth*3*i, Y), 
                           new Point(X + cellwidth*3*i, Y + width + 3));
            }
        }

        public Cell which_cell_clicked(Point coords)
        {
            int cx = coords.X - form.Location.X;
            int cy = coords.Y - form.Location.Y;
            foreach (Cell c in cells)
            {
                if(cx > c.X && cx < c.X + cellwidth && 
                   cy > c.Y && cy < c.Y + cellwidth)
                {
                    return c;
                }
            }
            return null;
        }

        public void check_all_cells_for_mistake()
        {
            foreach(Cell c in cells)
            {
                if(c.editable == false) { continue; }
                if(c.value == "") { continue; }
                c.correct = c.check_correctness();
            }
            form.Invalidate();
        }
    }
}
