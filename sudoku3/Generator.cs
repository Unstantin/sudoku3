using Microsoft.VisualBasic.ApplicationServices;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.Intrinsics.Arm;
using System.Security.Cryptography.Xml;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms.VisualStyles;

namespace sudoku3
{
    public class ClassicGenerator
    {
        public int?[,] base_sudoku =
            {{1,2,3,4,5,6,7,8,9},
             {4,5,6,7,8,9,1,2,3},
             {7,8,9,1,2,3,4,5,6},
             {2,3,4,5,6,7,8,9,1},
             {5,6,7,8,9,1,2,3,4},
             {8,9,1,2,3,4,5,6,7},
             {3,4,5,6,7,8,9,1,2},
             {6,7,8,9,1,2,3,4,5},
             {9,1,2,3,4,5,6,7,8}};

        public int?[,] classic(Cell[,] cells)
        {
            int N = 15;
            Random random = new Random();
            for(int i = 0; i < N; i++)
            {
                int var = random.Next() % 2;
                if(var == 0)
                {
                    swap_colums_or_rows(random.Next() % 2);
                } else
                {
                    swap_blocks(random.Next() % 2);
                }
            }

            int?[,] solution = (int?[,])base_sudoku.Clone();

            clear_random_cells(cells);
            
            for(int i = 0; i < base_sudoku.GetLength(0); i++)
            {
                for(int j = 0; j < base_sudoku.GetLength(1); j++)
                {
                    if (base_sudoku[i,j] == null)
                    {
                        cells[i, j].value = "";
                    } else
                    {
                        cells[i, j].value = Convert.ToString(base_sudoku[i, j]);
                    }
                }
            }

            return solution;
        }

        private void swap_colums_or_rows(int mode)
        {
            Random r = new Random();
            int block =  r.Next() % 3;
            int cor1 = r.Next() % 3;
            int cor2 = r.Next() % 3;
            
            while(cor1 == cor2)
            {
                cor2 = r.Next() % 3;
            }

            for(int i = 0; i < base_sudoku.GetLength(0); i++)
            {
                if(mode == 0) {
                    //строки
                    (base_sudoku[i, block * 3 + cor1], base_sudoku[i, block * 3 + cor2]) = (base_sudoku[i, block * 3 + cor2], base_sudoku[i, block * 3 + cor1]);
                } else {
                    //столбцы
                    (base_sudoku[block * 3 + cor1, i], base_sudoku[block * 3 + cor2, i]) = (base_sudoku[block * 3 + cor2, i], base_sudoku[block * 3 + cor1, i]);
                }
            }
        }

        private void swap_blocks(int mode)
        {
            Random r = new Random();
            int block1 = r.Next() % 3;
            int block2 = r.Next() % 3;
            while(block1 == block2)
            {
                block2 = r.Next() % 3;
            }

            for (int i = 0; i < base_sudoku.GetLength(0); i++)
            {
                if (mode == 0)
                {
                    //строки
                    (base_sudoku[i, block1 * 3], base_sudoku[i, block2 * 3]) = (base_sudoku[i, block2 * 3], base_sudoku[i, block1 * 3]);
                    (base_sudoku[i, block1 * 3 + 1], base_sudoku[i, block2 * 3 + 1]) = (base_sudoku[i, block2 * 3 + 1], base_sudoku[i, block1 * 3 + 1]);
                    (base_sudoku[i, block1 * 3 + 2], base_sudoku[i, block2 * 3 + 2]) = (base_sudoku[i, block2 * 3 + 2], base_sudoku[i, block1 * 3 + 2]);
                }
                else
                {
                    //столбцы
                    (base_sudoku[block1 * 3, i], base_sudoku[block2 * 3, i]) = (base_sudoku[block2 * 3, i], base_sudoku[block1 * 3, i]);
                    (base_sudoku[block1 * 3 + 1, i], base_sudoku[block2 * 3 + 1, i]) = (base_sudoku[block2 * 3 + 1, i], base_sudoku[block1 * 3 + 1, i]);
                    (base_sudoku[block1 * 3 + 2, i], base_sudoku[block2 * 3 + 2, i]) = (base_sudoku[block2 * 3 + 2, i], base_sudoku[block1 * 3 + 2, i]);
                }
            }
        }

        private void clear_random_cells(Cell[,] cells)
        {
            int M = 0;
            if (cells[0,0].board.mode == Board.MODES.CLASSIC)
            {
                M = 25;
            } 
            else if(cells[0, 0].board.mode == Board.MODES.KILLER)
            {
                M = 30;
            }
            
            Random random = new Random();
            for(int i = 0; i < M; i++)
            {
                int x, y;
                (x, y) = (random.Next() % 9, random.Next() % 9);
                if (cells[x, y].editable == true)
                {
                    i--;
                    continue;
                }

                cells[x, y].board.empty_cells_n++;
                base_sudoku[x, y] = null;
                cells[x, y].editable = true;
            }
        }
    }

    public class KillerGenerator
    {
        List<List<(int, int)>> areas;
        List<(int, int)> free_cells;
        int?[,] area_id_cells = new int?[9, 9];
        int MAX_N_AREA = 32;
        int BASE_CELLS_N = 20;

        public void Init()
        {
            free_cells = new List<(int, int)>();
            for(int i = 0; i < 9; i++)
            {
                for(int j = 0; j < 9; j++)
                {
                    area_id_cells[i, j] = null;
                    free_cells.Add((i,j));
                }
            }

            areas = new List<List<(int, int)>>();
            Random r = new Random();
            for (int i = 0; i < BASE_CELLS_N; i++)
            {
                int x = r.Next(0, 9);
                int y = r.Next(0, 9);
                areas.Add(new List<(int, int)> { (x, y) });
                area_id_cells[x, y] = i;
                free_cells.Remove((x, y));
            }
        }

        public void part_base_place() {
            Random r = new Random();
            for (int i = 0; i < BASE_CELLS_N / 2; i++)
            {
                int x = r.Next(0, 9);
                int y = r.Next(0, 9);
                areas.Add(new List<(int, int)> { (x, y) });
                area_id_cells[x, y] = i;
                free_cells.Remove((x, y));
            }
        }

        public List<int> killer(Board board)
        {
            List<int> try_k = try_killer(board);
            while (try_k == null || try_k.Count > 81)
            {
                try_k = try_killer(board);
            }
            return try_k;
        }

        public List<int> try_killer(Board board)
        {
            Init();
            Random r = new Random();
            int cells_n = 0;
            while (free_cells.Count != 0)
            {
                int index = r.Next(free_cells.Count);
                int res = check_neighbors(free_cells[index], board);
                if (res == 0)
                {
                    areas.Add(new List<(int, int)> { free_cells[index] });
                    free_cells.RemoveAt(index);
                    cells_n++;
                }
                else if (res == 1)
                {
                    free_cells.RemoveAt(index);
                    cells_n++;
                }

                if (cells_n + 20 > 81) { return null; }
                if (areas.Count > MAX_N_AREA) { return null; }
               
            }
            Console.WriteLine(cells_n + 20);

            foreach (List<(int, int)> area in areas)
            {
                (int, int) highest = find_highest_in_area(area);
                board.cells[highest.Item1, highest.Item2].with_area_sum = true;
                foreach((int,int) cell in area)
                {
                    board.cells[cell.Item1, cell.Item2].area_id = areas.IndexOf(area); 
                }
            }

            return get_sums_of_areas(board.cells);
        }

        public List<int> get_sums_of_areas(Cell[,] cells)
        {
            List<int> res = new List<int>();
            int i = 0;
            foreach(List<(int,int)> area in areas)
            {
                res.Add(0);
                foreach((int,int) coords in area)
                {
                    res[i] += Convert.ToInt32(cells[0,0].board.solution[coords.Item1, coords.Item2]);
                }
                i++;
            }
            return res;
        }

        public int check_neighbors((int, int) coords, Board board)
        {
            List<int> mays = new List<int>();

            bool b0 = false;
            bool b1 = false;
            bool b2 = false;
            bool b3 = false;

            if (coords.Item1 + 1 < 9) 
            {
                b0 = area_id_cells[coords.Item1 + 1, coords.Item2] == null;
                if (!b0) 
                {
                    if (!inArea(coords, (coords.Item1 + 1, coords.Item2), board))
                    {
                        mays.Add(0);
                    }
                }
            }
            if (coords.Item2 + 1 < 9)
            {
                b1 = area_id_cells[coords.Item1, coords.Item2 + 1] == null;
                if (!b1)
                {
                    if (!inArea(coords, (coords.Item1, coords.Item2 + 1), board))
                    {
                        mays.Add(1);
                    }
                }
            }
            if (coords.Item1 - 1 >= 0) 
            {
                b2 = area_id_cells[coords.Item1 - 1, coords.Item2] == null;
                if (!b2)
                {
                    if (!inArea(coords, (coords.Item1 - 1, coords.Item2), board))
                    {
                        mays.Add(2);
                    }
                }
            }
            if (coords.Item2 - 1 >= 0) 
            {
                b3 = area_id_cells[coords.Item1, coords.Item2 - 1] == null;
                if (!b3)
                {
                    if (!inArea(coords, (coords.Item1, coords.Item2 - 1), board))
                    {
                        mays.Add(3);
                    }
                }
            }

            if(mays.Count == 0)
            {
                return 0;
            }

            if(b0 && b1 & b2 && b3) { return 2; }

            Random r = new Random();
            int index = r.Next(4);
            while(!mays.Contains(index))
            {
                index = r.Next(4);
            }
            if(index == 0) { 
                areas[(int)area_id_cells[coords.Item1 + 1, coords.Item2]].Add(coords);
            }
            else if(index == 1) { 
                areas[(int)area_id_cells[coords.Item1, coords.Item2 + 1]].Add(coords); 
            }
            else if(index == 2) { 
                areas[(int)area_id_cells[coords.Item1 - 1, coords.Item2]].Add(coords); 
            }
            else if(index == 3) { 
                areas[(int)area_id_cells[coords.Item1, coords.Item2 - 1]].Add(coords); 
            }

            return 1;
        }

        public bool inArea((int,int) coords, (int, int) coords_i, Board board)
        {
            List<(int, int)> area = areas[(int)area_id_cells[coords_i.Item1, coords_i.Item2]]; // область рассматриваемого соседа
            int? value = board.solution[coords.Item1, coords.Item2]; //значение рассматриваемой клетки
            foreach((int,int) coords_in_area in area)
            {
                int? value_in_area = board.solution[coords_in_area.Item1, coords_in_area.Item2]; //значение в области
                if (value_in_area == value)
                {
                    return true;
                } 
            }
            return false;
        }

        public (int,int) find_highest_in_area(List<(int,int)> area)
        {
            (int, int) res = area[0];
            foreach((int,int) coords in area)
            {
                if(res.Item2 > coords.Item2)
                {
                    res = coords;
                } else if (res.Item2 == coords.Item2)
                {
                    if(res.Item1 > coords.Item1)
                    {
                        res = coords;
                    }
                }
            }

            return res;
        }
    }

    //бесконечный цикл из-за того, что у блоков может и не быть рядом никаких клеток (см для программы)
    public class ColorGenerator
    {
        List<(int, int)>[] blocks;
        int Mi = 3;

        List<(int, int, int, int)> already_checked_pairs_cells = new List<(int, int, int, int)>();

        int block_integrity_n;
        List<(int, int)> already_checked_cells_integrity;

        public void color(Cell[,] cells)
        {
            Console.WriteLine("ГЕНЕРАЦИЯ НАЧАЛАСЬ!");
            Console.ReadLine();
            Init();
            Console.WriteLine("ИНИЦИАЛИЗАЦИЯ ПРОШЛА");
            Console.ReadLine();
            create_blocks();
            Console.WriteLine("БЛОКИ СОЗДАЛИСЬ");
            for (int i = 0; i < 9; i++)
            {
                for (int j = 0; j < 9; j++)
                {
                    bool find = false;
                    for (int k = 0; k < 9; k++)
                    {
                        for (int f = 0; f < 9; f++)
                        {
                            if (blocks[k][f] == (i, j))
                            {
                                cells[i, j].block_color = k;
                                find = true;
                                break;
                            }
                        }
                        if (find) { break; }
                    }
                }
            }
        }

        void Init()
        {
            blocks = new List<(int, int)>[9];
            for (int i = 0; i < 9; i++)
            {
                blocks[i] = new List<(int, int)>();
                for (int j = 0; j < 3; j++)
                {
                    blocks[i].Add(((i % 3) * 3, (i / 3) * 3 + j));
                    blocks[i].Add(((i % 3) * 3 + 1, (i / 3) * 3 + j));
                    blocks[i].Add(((i % 3) * 3 + 2, (i / 3) * 3 + j));
                }
            }
        }

        void create_blocks()
        {
            int[] blocks_which_will_mix = new int[] { 0, 2, 4, 6, 8 };
            foreach (int i in blocks_which_will_mix)
            {
                Console.WriteLine($"РАБОТАЮ НАД БЛОКОМ {i}");
                Console.ReadLine();
                for (int k = 0; k < Mi; k++) {
                    int n2;
                    n2 = mix(i);
                    //Console.WriteLine("Щас проверим можно ли так поменять");
                    if (!this.check_integrity(blocks[i]) || !this.check_integrity(blocks[n2]))
                    {
                        Console.WriteLine("Нельзя так поменять");

                        blocks[i].Add(blocks[n2][^1]);
                        blocks[n2].Remove(blocks[n2][^1]);
                        blocks[n2].Add(blocks[i][^2]);
                        blocks[i].Remove(blocks[i][^2]);
                        
                        k--;
                    } else
                    {
                        Console.WriteLine("                                 Можно так поменять!");
                        already_checked_pairs_cells = new List<(int, int, int, int)>();
                    }
                }
            }
        }

        int mix(int n)
        {
            Random random = new Random();
            int r;
            int n2 = 0;
            switch (n)
            {
                case 0:
                    switch (random.Next(0, 2))
                    {
                        case 0:
                            n2 = 1;
                            break;
                        case 1:
                            n2 = 3;
                            break;
                    }
                    break;
                case 2:
                    switch (random.Next(0, 2))
                    {
                        case 0:
                            n2 = 1;
                            break;
                        case 1:
                            n2 = 5;
                            break;
                    }
                    break;
                case 4:
                    switch (random.Next(0, 4))
                    {
                        case 0:
                            n2 = 1;
                            break;
                        case 1:
                            n2 = 3;
                            break;
                        case 2:
                            n2 = 5;
                            break;
                        case 3:
                            n2 = 7;
                            break;
                    }
                    break;
                case 6:
                    switch (random.Next(0, 2))
                    {
                        case 0:
                            n2 = 3;
                            break;
                        case 1:
                            n2 = 7;
                            break;
                    }
                    break;
                case 8:
                    switch (random.Next(0, 2))
                    {
                        case 0:
                            n2 = 5;
                            break;
                        case 1:
                            n2 = 7;
                            break;
                    }
                    break;
            }
            mix_two(n, n2);
            return n2;
        }

        void mix_two(int n1, int n2)
        {
            Console.WriteLine($"ПЕРЕМЕШИВАЮ БЛОК {n1} и {n2}");
            //Console.ReadLine();
            Random random = new Random();
            int c1 = random.Next(0, 9);
            int c2 = random.Next(0, 9);
            int x1 = blocks[n1][c1].Item1;
            int y1 = blocks[n1][c1].Item2;
            int x2 = blocks[n2][c2].Item1;
            int y2 = blocks[n2][c2].Item2;
            if(already_checked_pairs_cells.Contains((x1,y1,x2,y2)))
            {
                return;
            }
            already_checked_pairs_cells.Add((x1,y1,x2,y2));
            while (!(
               (
                blocks[n1].Contains((x2 + 1, y2)) ||
                blocks[n1].Contains((x2 - 1, y2)) ||
                blocks[n1].Contains((x2, y2 + 1)) ||
                blocks[n1].Contains((x2, y2 - 1))
               ) && (
                blocks[n2].Contains((x1 + 1, y1)) ||
                blocks[n2].Contains((x1 - 1, y1)) ||
                blocks[n2].Contains((x1, y1 + 1)) ||
                blocks[n2].Contains((x1, y1 - 1))
               )
                  )) { 
                //Console.WriteLine("НЕ ПОДХОДИТ!");
                c1 = random.Next(0, 9);
                c2 = random.Next(0, 9);
                x1 = blocks[n1][c1].Item1;
                y1 = blocks[n1][c1].Item2;
                x2 = blocks[n2][c2].Item1;
                y2 = blocks[n2][c2].Item2;
                //Console.WriteLine($"ПРОБУЮ {x1},{y1} и {x2},{y2}");
            }

            blocks[n2].Add(blocks[n1][c1]);
            blocks[n1].Add(blocks[n2][c2]);
            blocks[n1].Remove(blocks[n1][c1]);
            blocks[n2].Remove(blocks[n2][c2]);

            Console.WriteLine($"ПОМЕНЯЛ МЕСТАМИ {x1},{y1} и {x2},{y2}");
        }

        bool check_integrity(List<(int,int)> block)
        {
            block_integrity_n = 0;
            already_checked_cells_integrity = new List<(int, int)>();
            check_neighbors(block[0], block);

            //Console.WriteLine($"Я нашел только {block_integrity_n} в этом блоке");

            return block_integrity_n == 9;
        }

        void check_neighbors((int,int) cell, List<(int,int)> block)
        {
            if(already_checked_cells_integrity.Contains(cell))
            {
                return;
            }
            if(block.Contains((cell.Item1,cell.Item2)))
            {
                block_integrity_n++;
                already_checked_cells_integrity.Add(cell);
                if (block_integrity_n == 9)
                {
                    return;
                }
                check_neighbors((cell.Item1 + 1, cell.Item2), block);
                check_neighbors((cell.Item1 - 1, cell.Item2), block);
                check_neighbors((cell.Item1, cell.Item2 + 1), block);
                check_neighbors((cell.Item1, cell.Item2 - 1), block);
            } 
        }
    }
}
