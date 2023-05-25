using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Configuration;
using System.Drawing;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using static sudoku3.Board;

namespace sudoku3
{
    [Serializable]
    abstract public class CellType {
        public bool editable = false;
        public bool correct = true;
        public string value;

        public int X;
        public int Y;
        public int xb;
        public int yb;

        abstract public void draw(Graphics e);
        abstract public bool check_correctness();
    }

    [Serializable]
    public class Cell : CellType
    {
        [NonSerialized] Font drawFont = new Font("Batang", 20);
        [NonSerialized] Font drawFontSums = new Font("Batang", 16);
        [NonSerialized] SolidBrush drawBrush_non_edit = new SolidBrush(Color.White);
        [NonSerialized] SolidBrush drawBrush_edit = new SolidBrush(Color.Black);
        [NonSerialized] StringFormat drawFormat = new StringFormat();

        [NonSerialized] public Form1 form;
        [NonSerialized] public Board board;

        public int block_color; // для нерегулярного судоку

        //killer
        public int area_id; 
        public bool with_area_sum = false;

        public Cell(Cell saved_cell, Form1 form, Board board)
        {
            this.form = form;
            this.board = board;
            editable = saved_cell.editable;
            correct = saved_cell.correct;
            value = saved_cell.value;
            X = saved_cell.X;
            Y = saved_cell.Y;
            xb = saved_cell.xb;
            yb = saved_cell.yb;
            block_color = saved_cell.block_color;
            area_id = saved_cell.area_id;
            with_area_sum = saved_cell.with_area_sum;
        }

        public Cell(string value, Board board, int X, int Y, int xb, int yb) { 
            this.value = value;

            this.board = board;
            this.form = board.form;

            this.X = X;
            this.Y = Y;

            this.xb = xb;
            this.yb = yb;
        }

        public override void draw(Graphics e)
        {
            Point[] p = new Point[4] { new Point(X, Y), 
                                       new Point(X + board.cellwidth, Y),
                                       new Point(X +  board.cellwidth, Y +  board.cellwidth), 
                                       new Point(X, Y +  board.cellwidth) };

            if(board.mode == Board.MODES.CLASSIC)
            {
                draw_classic(e, p);
            } 
            else if(board.mode ==Board.MODES.KILLER) 
            { 
                draw_killer(e, p);
            }
        }

        public void draw_classic(Graphics e, Point[] p)
        {
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

            //почему именно такие +7 и прочее? просто потому что (мб проблема в width)
            if (!this.editable)
            {
                e.DrawString(value, drawFont, drawBrush_non_edit, X + board.cellwidth / 4 + 9,
                    Y + board.cellwidth / 4 + 4, drawFormat);
            }
            else
            {
                e.DrawString(value, drawFont, drawBrush_edit, X + board.cellwidth / 4 + 9,
                    Y + board.cellwidth / 4 + 4, drawFormat);
            }
        }

        public void draw_killer(Graphics e, Point[] p)
        {
            Pen pen_incorrect = new Pen(Color.FromArgb(0, 0, 0), 4);
            Pen non_editable = new Pen(Color.FromArgb(0, 0, 0), 3);
            color_by_area_id(e, p);
            if(!this.correct && this.editable)
            {
                e.DrawLine(pen_incorrect, p[0], p[2]);
                e.DrawLine(pen_incorrect, p[1], p[3]);
            }
            else if (!this.editable)
            {
                Point[] p1 = new Point[4];
                p1[0] = new Point(p[0].X + 6, p[0].Y + 6);
                p1[1] = new Point(p[1].X - 6, p[1].Y + 6);
                p1[2] = new Point(p[2].X - 6, p[2].Y - 6);
                p1[3] = new Point(p[3].X + 6, p[3].Y - 6);
                e.DrawPolygon(non_editable, p1);
            }

            e.DrawPolygon(form.pen, p);

            e.DrawString(value, drawFont, drawBrush_edit, X + board.cellwidth / 4 + 9,
                    Y + board.cellwidth / 4 + 4, drawFormat);

            if (this.with_area_sum)
            {
                e.DrawString(Convert.ToString(board.sums_of_areas[area_id]),
                    drawFontSums, drawBrush_edit, X+4, Y+4, drawFormat);
            }
        }

        public override bool check_correctness()
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
                if (board.mode == Board.MODES.KILLER)
                {
                    if (!check_area_and_sum())
                    {
                        return false;
                    }
                }
            }
            
            return true;
        }

        public bool check_area_and_sum() 
        {
            int sum = 0;
            foreach(Cell c in board.cells)
            {
                if(area_id == c.area_id)
                {
                    if(c.value != "")
                    {
                        sum += Convert.ToInt32(c.value);
                    }
                    if(sum > board.sums_of_areas[area_id])
                    {
                        return false;
                    }
                    if(value == c.value && !((xb == c.xb) && (yb == c.yb)))
                    {
                        return false;
                    }
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

        public void color_by_area_id(Graphics e, Point[] p)
        {
            StreamReader sr = new StreamReader("killercolors.txt");
            List<Color> colors = new List<Color>();
            for(int i = 0; i < 35; i++)
            {
                string[] clrs = sr.ReadLine().Split(" ");
                colors.Add(Color.FromArgb(
                    Convert.ToInt32(clrs[0]),
                    Convert.ToInt32(clrs[1]),
                    Convert.ToInt32(clrs[2])));
            }
            sr.Close();
                    
            SolidBrush brush = new SolidBrush(colors[this.area_id % 35]);
            e.FillPolygon(brush, p);
        } //киллер

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
        } //фигурное
    }

    [Serializable]
    public class TriangularCell : CellType
    {
        [NonSerialized] Font drawFont = new Font("Batang", 20);
        [NonSerialized] Font drawFontSums = new Font("Batang", 16);
        [NonSerialized] SolidBrush drawBrush_non_edit = new SolidBrush(Color.White);
        [NonSerialized] SolidBrush drawBrush_edit = new SolidBrush(Color.Black);
        [NonSerialized] StringFormat drawFormat = new StringFormat();

        [NonSerialized] public Form1 form;
        [NonSerialized] public TriangularBoard board;

        public bool isUp;

        public TriangularCell(TriangularCell saved_cell, Form1 form, TriangularBoard board)
        {
            this.form = form;
            this.board = board;
            this.X = saved_cell.X;
            this.Y = saved_cell.Y;
            this.xb = saved_cell.xb;
            this.yb = saved_cell.yb;
            this.editable = saved_cell.editable;
            this.correct = saved_cell.correct;
            this.value = saved_cell.value;
        }

        public TriangularCell(string value, TriangularBoard board, int X, int Y, int xb, int yb)
        {
            this.form = board.form;
            this.board = board;
            this.X = X;
            this.Y = Y;
            this.xb = xb;
            this.yb = yb;
            this.value = value;

            this.isUp = false;
            if (this.yb % 2 == this.xb % 2)
            {
                isUp = true;
            }
        }

        public override void draw(Graphics e)
        {
            Point[] p;
            if (!isUp)
            {
                p = new Point[]
                {
                    new Point(X, Y + board.triangular_hight / 2),
                    new Point(X + board.cellwidth / 2, Y - board.triangular_hight / 2),
                    new Point(X - board.cellwidth / 2, Y - board.triangular_hight / 2)
                };
            } 
            else 
            {
                p = new Point[]
                {
                    new Point(X, Y - board.triangular_hight / 2),
                    new Point(X + board.cellwidth / 2, Y + board.triangular_hight / 2),
                    new Point(X - board.cellwidth / 2, Y + board.triangular_hight / 2)
                };
            }
            if(!this.correct)
            {
                e.FillPolygon(Brushes.OrangeRed, p);
            }
            else if(this.editable)
            {
                e.FillPolygon(new SolidBrush(form.editable_cells_color), p);
            }

            e.DrawPolygon(form.pen, p);

            Font drawFont = new Font("Batang", 15);
            SolidBrush drawBrush_edit = new SolidBrush(Color.White);
            StringFormat drawFormat = new StringFormat();
            if(isUp)
            {
                e.DrawString(value, drawFont, drawBrush_edit, X - 8, Y - 5, drawFormat);
            } 
            else
            {
                e.DrawString(value, drawFont, drawBrush_edit, X - 8, Y - 12, drawFormat);
            }
        }

        public override bool check_correctness()
        {
            return !inBlock() && check_diagonals((xb, yb));
        }

        public bool inBlock()
        {
            int block_id = 0;
            foreach ((int, int)[] list in TriangularGenerator.blocks_coords)
            {
                bool find = false;
                foreach ((int, int) c in list)
                {
                    if (c.Item1 == xb && c.Item2 == yb)
                    {
                        find = true;
                        break;
                    }
                }

                if (find) { break; } else { block_id++; }
            }

            foreach ((int, int) coords in TriangularGenerator.blocks_coords[block_id])
            {
                if (coords == (xb, yb)) { continue; }
                if (board.cells[coords.Item1, coords.Item2].value == Convert.ToString(value))
                {
                    return true;
                }
            }
            return false;
        }

        public bool check_diagonals((int, int) coords)
        {
            return
                !check_direction(coords, 1, 1) &&
                !check_direction(coords, 1, -1) &&
                !check_direction(coords, -1, 1) &&
                !check_direction(coords, -1, -1) &&
                !check_row(coords);
        }

        public bool check_direction((int, int) coords, int sx, int sy)
        {
            int step_x = 0;
            int step_y = 0;

            int i;
            if (board.cells[coords.Item1, coords.Item2].isUp)
            { i = 0; } else { i = 1; }

            while (true)
            {
                if (sy > 0)
                {
                    if (i % 2 == 0) { step_y += sy; }
                    else { step_x += sx; }
                }
                else
                {
                    if (i % 2 == 0) { step_x += sx; }
                    else { step_y += sy; }
                }
                i++;

                if (
                    coords.Item1 + step_x < 0 || coords.Item2 + step_y < 0 ||
                    coords.Item1 + step_x > 12 || coords.Item2 + step_y > 7
                   )
                { break; }

                if (board.cells[coords.Item1 + step_x, coords.Item2 + step_y] != null)
                {
                    if (board.cells[coords.Item1 + step_x, coords.Item2 + step_y].value == Convert.ToString(value))
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        public bool check_row((int, int) coords)
        {
            for (int i = 0; i < 13; i++)
            {
                if (board.cells[i, coords.Item2] == null) { continue; }
                if ((i, coords.Item2) == (xb,yb)) { continue; }
                if (board.cells[i, coords.Item2].value == Convert.ToString(value))
                {
                    return true;
                }
            }

            return false;
        }
    }
}
