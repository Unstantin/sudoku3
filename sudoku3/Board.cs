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
using static sudoku3.Board;

namespace sudoku3
{
    [Serializable]
    public abstract class BoardType
    {
        public int empty_cells_n = 0;
        public int mistake_cells_n = 0;

        public int cellwidth;

        public CellType active_cell = null;

        public int lives = 3;
        public long solution_time = 0;

        public int save_index;
        public bool saved = false;

        public enum MODES
        {
            CLASSIC,
            KILLER,
            TRIANGLE
        }
        public MODES mode;

        abstract public void draw(Graphics e);
        abstract public CellType which_cell_clicked(Point coords);
        abstract public void check_all_cells_for_mistake();
    }

    [Serializable]
    public class Board : BoardType
    {
        [NonSerialized] public Form1 form;
        [NonSerialized] public static int N = 9;
        
        public Cell[,] cells;
        public int?[,] solution;
        public List<int> sums_of_areas; //killer

        public int width;
        public int X;
        public int Y;

        public Board(Board saved_board, Form1 form)
        {
            this.mode = saved_board.mode;
            this.form = form;
            this.width = saved_board.width;
            this.cellwidth = saved_board.cellwidth;
            this.active_cell = saved_board.active_cell;
            this.saved = saved_board.saved;
            this.save_index = saved_board.save_index;
            this.empty_cells_n = saved_board.empty_cells_n;
            this.mistake_cells_n = saved_board.mistake_cells_n;
            this.lives = saved_board.lives;
            this.solution_time = saved_board.solution_time;
            this.sums_of_areas = saved_board.sums_of_areas;
            this.solution = saved_board.solution;

            this.X = saved_board.X;
            this.Y = saved_board.Y;

            cells = new Cell[N, N];
            for (int i = 0; i < N; i++)
            {
                for (int j = 0; j < N; j++)
                {
                    cells[j, i] = new Cell(
                        saved_board.cells[j, i],
                        form, this
                    );
                }
            }

            form.saved_boards.Remove(save_index);
            form.saved_boards.Add(save_index, this);
        }

        public Board(Form1 form, MODES mode)
        {
            this.mode = mode;
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

            solution = form.classicGenerator.classic(cells);
            if (mode == MODES.KILLER) { sums_of_areas = form.killerGenetaror.killer(this); }
            //ColorGenerator.color(cells);
        }

        public override void draw(Graphics e)
        {
            foreach (Cell c in cells)
            {
                c.draw(e);
            }
            draw_add_grid(e);
        }

        public void draw_add_grid(Graphics e)
        {
            for (int i = 0; i < 4; i++)
            {
                //почему именно такие +1 -2 и прочее? просто потому что (мб проблема в width)
                e.DrawLine(new Pen(form.board_color, 7),
                           new Point(X - 4, Y + cellwidth * 3 * i),
                           new Point(X + width + 4, Y + cellwidth * 3 * i));
                e.DrawLine(new Pen(form.board_color, 7),
                           new Point(X + cellwidth * 3 * i, Y),
                           new Point(X + cellwidth * 3 * i, Y + width + 3));
            }
        }

        public override Cell which_cell_clicked(Point coords)
        {
            int cx = coords.X - form.Location.X;
            int cy = coords.Y - form.Location.Y;
            foreach (Cell c in cells)
            {
                if (cx > c.X && cx < c.X + cellwidth &&
                   cy > c.Y && cy < c.Y + cellwidth)
                {
                    return c;
                }
            }
            return null;
        }

        public override void check_all_cells_for_mistake()
        {
            foreach (Cell c in cells)
            {
                if (c.editable == false) { continue; }
                if ((c.value == "") && (c.correct == false))
                {
                    mistake_cells_n--;
                    c.correct = true;
                    continue;
                }
                if (c.value == "")
                {
                    continue;
                }

                bool c_cor = c.correct;
                c.correct = c.check_correctness();
                if ((c_cor == false) && (c.correct != false))
                {
                    mistake_cells_n--;
                }
                if ((c_cor == true) && (c.correct != true))
                {
                    mistake_cells_n++;
                }
            }
            form.Invalidate();
        }
    }

    [Serializable]
    public class TriangularBoard : BoardType
    {
        [NonSerialized] public Form1 form;

        public TriangularCell[,] cells;
        public int?[,] solution;
        public int width;
        public int triangular_hight;
        public int X;
        public int Y;

        public TriangularBoard(TriangularBoard board, Form1 form)
        {
            this.form = form;
        }

        public TriangularBoard(Form1 form)
        {
            this.form = form;
            this.mode = MODES.TRIANGLE;
            this.width = Convert.ToInt32(form.ClientSize.Height * 0.875);
            this.cellwidth = width / N;
            this.triangular_hight = (int)(cellwidth * Math.Sin(Math.PI * 60 / 180.0));

            this.X = (int)(form.ClientSize.Width * 0.35);
            this.Y = (int)(form.ClientSize.Height * 0.2);

            cells = new TriangularCell[13, 8];
            StreamReader file_with_coords = new StreamReader("triangular_coords.txt");
            (int, int)[] coords_from_file = new (int, int)[54];
            for (int i = 0; i < 54; i++)
            {
                string[] str_split = file_with_coords.ReadLine().Split(",");
                coords_from_file[i] = (Convert.ToInt32(str_split[0]), Convert.ToInt32(str_split[1]));
            }
            for (int i = 0; i < 13; i++)
            {
                for (int j = 0; j < 8; j++)
                {
                    if (coords_from_file.Contains((i, j)))
                    {
                        cells[i, j] = new TriangularCell(
                            "", this,
                            X + (cellwidth / 2) * i,
                            Y + triangular_hight * j,
                            i, j
                        );
                    }
                    else
                    {
                        cells[i, j] = null;
                    }
                }
            }

            //solution = form.classicGenerator.classic(cells);
        }

        public override void draw(Graphics e)
        {
            foreach (TriangularCell cell in cells)
            {
                if (cell != null) { cell.draw(e); }
            }
        }

        public override TriangularCell which_cell_clicked(Point coords)
        {
            int cx = coords.X - form.Location.X;
            int cy = coords.Y - form.Location.Y;
            int h = triangular_hight;
            int w = cellwidth;
            foreach (TriangularCell c in cells)
            {
                if(c == null) { continue; }
                int x = c.X;
                int y = c.Y;
                if (c.isUp) {
                    if (
                        (cy < y + h / 2) &&
                        (w/2 * (cy-y) + h*(cx-x) + w*h/4 > 0) &&
                        (w/2 * (cy-y) - h*(cx-x) + w*h/4 > 0)
                       )
                    {
                        return c;
                    }
                }
                else
                {
                    if (
                        (cy > y - h / 2) &&
                        (w/2 * (cy-y) - h*(cx-x) - w*h/4 < 0) &&
                        (w/2 * (cy-y) + h*(cx-x) - w*h/4 < 0)
                      )
                    {
                        return c;
                    }
                }               
            }
            return null;
        }

        public override void check_all_cells_for_mistake()
        {
           
        }
    }
}

