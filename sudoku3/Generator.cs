using Microsoft.VisualBasic.ApplicationServices;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.Contracts;
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

    public class TriangularGenerator
    {
        public int?[,] gener_cells;
        static public (int, int)[][] blocks_coords =
        {
            new (int,int)[] { (2,1), (3,1), (4,1), (5,1), (6,1), (3,2), (4,2), (5,2), (4,3) },
            new (int,int)[] { (2,2), (1,3), (2,3), (3,3), (0,4), (1,4), (2,4), (3,4), (4,4) },
            new (int,int)[] { (2,5), (3,5), (4,5), (5,5), (6,5), (3,6), (4,6), (5,6), (4,7) },
            new (int,int)[] { (8,4), (7,5), (8,5), (9,5), (6,6), (7,6), (8,6), (9,6), (10,6) },
            new (int,int)[] { (8,3), (9,3), (10,3), (11,3), (12,3), (9,4), (10,4), (11,4), (10,5) },
            new (int,int)[] { (8,0), (7,1), (8,1), (9,1), (6,2), (7,2), (8,2), (9,2), (10,2) }
        };
        public TriangularCell[,] cells;
        public int M = 16;

        public void init(TriangularCell[,] cells)
        {
            this.cells = cells;
            gener_cells = new int?[13,8];
            for (int i = 0; i < 13; i++)
            {
                for (int j = 0; j < 8; j++)
                {
                    if (cells[i, j] == null)
                    {
                        gener_cells[i, j] = null;
                    } else
                    {
                        gener_cells[i, j] = 0;
                    }
                }
            }
        }

        public void init_base_block()
        {
            Random r = new Random();
            foreach ((int, int) coords in blocks_coords[0])
            {
                int v = r.Next() % 9 + 1;
                while (inBlock(v, 0))
                {
                    v = r.Next() % 9 + 1;
                }

                gener_cells[coords.Item1, coords.Item2] = v;
            }
        }

        public void triangular(TriangularCell[,] cells)
        {
            init(cells);
            init_base_block();
            bool res = try_triangilar();
            while(!res)
            {
                init(cells);
                init_base_block();
                res = try_triangilar();
                foreach(int? v in gener_cells)
                {
                    if(v == 0)
                    {
                        res = false;
                        break;
                    }
                }
            }

            gener_to_cells();
            clear_some_cells();
        }

        public bool try_triangilar()
        {
            int try_to_fill_n = 0;
            for (int i = 1; i < 6; i++) //заполнение оставшихся блоков
            {
                if (!fill_block(i))
                {
                    try_to_fill_n++;
                    if (try_to_fill_n == 10)
                    {
                        return false;
                    }
                    i -= 2;
                    if(i < 1) { return false; }
                }
            }

            return true;
        }

        public void gener_to_cells()
        {
            for (int i = 0; i < 13; i++)
            {
                for (int j = 0; j < 8; j++)
                {
                    if (gener_cells[i, j] == null) { continue; }
                    if (gener_cells[i, j] == 0) { continue; }
                    cells[i, j].value = Convert.ToString(gener_cells[i, j]);
                }
            }
        }

        public void clear_some_cells()
        {
            Random r = new Random();
            for(int i = 0; i < M; i++)
            {
                int rx = r.Next() % 13;
                int ry = r.Next() % 8;
                if (cells[rx, ry] != null && cells[rx,ry].value != "") 
                {
                    cells[rx, ry].value = "";
                    cells[rx, ry].editable = true;
                    cells[rx,ry].board.empty_cells_n++;
                } 
                else
                {
                    i--;
                }
            }
        }

        public bool fill_block(int block_id)
        {
            List<int>[] possibilities = generate_possibilities(block_id);

            bool[] values = new bool[9];
            for(int i1 = 0; i1 < 9; i1++) { values[i1] = false; }
            foreach (List<int> m in possibilities)
            {
                foreach (int gi in m)
                {
                    values[gi - 1] = true;
                }
            }

            List<int> already_putted = new List<int>();
            while (true) // проверяем на тупики и на единичные случаи
            {
                bool remove = false;
                int cell_id = 0;
                foreach (List<int> vars in possibilities) 
                {
                    if (vars.Count == 0)
                    {
                        if(!already_putted.Contains(cell_id)) {
                            //перегенерируем прошлый блок, тупик
                            clear_block(block_id);
                            clear_block(block_id - 1);
                            return false;
                        }
                    }
                    else if (vars.Count == 1) //случай если доступно только одно число
                    {
                        (int, int) c = blocks_coords[block_id][cell_id];
                        gener_cells[c.Item1, c.Item2] = vars[0];
                        foreach (List<int> vars_tmp in possibilities)
                        {
                            vars_tmp.Remove((int)gener_cells[c.Item1, c.Item2]);
                        }
                        already_putted.Add(cell_id);
                        remove = true;
                    }
                    cell_id++;
                }

                if(!remove)
                {
                    break;
                }
            }

            List<List<int>> steps = generate_steps(possibilities);

            Random r = new Random();
            foreach(List<int> step in steps) // выполняем шаги
            {
                while(step.Count != 0)
                {
                    int value_index = r.Next(step.Count);
                    int value = step[value_index];
                    List<int> var_cells_to_place = new List<int>();
                    int var_index = 0;
                    foreach (List<int> var in possibilities)
                    {
                        if (var.Contains(value))
                        {
                            var_cells_to_place.Add(var_index);
                        }
                        var_index++;
                    }
                    if(var_cells_to_place.Count == 0) { //клетка без вариантов что в нее ставить
                        clear_block(block_id);
                        clear_block(block_id - 1);
                        return false; 
                    }

                    bool possibleOrNot = false;
                    foreach(int var in var_cells_to_place)
                    {
                        (int, int) c_tmp = blocks_coords[block_id][var];
                        if (gener_cells[c_tmp.Item1, c_tmp.Item2] == 0) { possibleOrNot = true;  }
                    }
                    if(!possibleOrNot) {
                        clear_block(block_id);
                        clear_block(block_id - 1);
                        return false; 
                    }

                    int var_cell_index = r.Next(var_cells_to_place.Count);
                    (int, int) c = blocks_coords[block_id][var_cells_to_place[var_cell_index]];
                    if(gener_cells[c.Item1, c.Item2] != 0) { continue; }
                    gener_cells[c.Item1, c.Item2] = value;

                    foreach (List<int> var in possibilities)
                    {
                        var.Remove(value);
                    }
                    step.Remove(value);
                }
            }

            return true;
        }

        public void clear_block(int block_id)
        {
            foreach((int,int) c in blocks_coords[block_id])
            {
                gener_cells[c.Item1, c.Item2] = 0;
            }
        }

        public List<int>[] generate_possibilities(int block_id)
        {
            int cell_id = 0;
            List<int>[] possibilities = new List<int>[9];
            foreach ((int, int) coords in blocks_coords[block_id])
            {
                possibilities[cell_id] = new List<int>();
                for (int j = 1; j < 10; j++)
                {
                    if (check_diagonals(j, coords))
                    {
                        possibilities[cell_id].Add(j);
                    }
                }
                cell_id++;
            }

            return possibilities;
        }

        public List<List<int>> generate_steps(List<int>[] possibilities)
        {
            int[] rares_of_values = new int[9];
            for(int i = 0; i < 9; i++) { rares_of_values[i] = 0; }
            foreach(List<int> cell_var in possibilities)
            {
                foreach(int value in cell_var)
                {
                    rares_of_values[value - 1]++;
                }
            }

            List<List<int>> steps = new List<List<int>>();
            for(int i = 1; i < 10; i++)
            {
                List<int> step = new List<int>();
                int i1 = 1;
                foreach(int r in rares_of_values)
                {
                    if(r == i)
                    {
                        step.Add(i1);
                    }
                    i1++;
                }
                if(step.Count != 0)
                {
                    steps.Add(step);
                }
            }

            return steps;
        }

        public bool inBlock(int value, int block_id)
        {
            foreach ((int, int) coords_tmp in blocks_coords[block_id])
            {
                if (gener_cells[coords_tmp.Item1, coords_tmp.Item2] == value)
                {
                    return true;
                }
            }
            return false;
        }

        public bool check_diagonals(int value, (int,int) coords)
        {
            return 
                !check_direction(value, coords, 1, 1) &&
                !check_direction(value, coords, 1, -1) &&
                !check_direction(value, coords, -1, 1) &&
                !check_direction(value, coords, -1, -1) &&
                !check_row(value, coords);
        }

        public bool check_direction(int value, (int,int) coords, int sx, int sy)
        {
            int step_x = 0;
            int step_y = 0;

            int i;
            if (cells[coords.Item1, coords.Item2].isUp)
            { i = 0; } else { i = 1; }

            while(true)
            {
                if(sy > 0) 
                {
                    if (i % 2 == 0) { step_y += sy; }
                    else { step_x += sx; }
                } 
                else
                {
                    if (i % 2 == 0) { step_x += sx;  }
                    else { step_y += sy; }
                }
                i++;

                if (
                    coords.Item1 + step_x < 0 || coords.Item2 + step_y < 0 ||
                    coords.Item1 + step_x > 12 || coords.Item2 + step_y > 7
                   )
                { break; }

                if (gener_cells[coords.Item1 + step_x, coords.Item2 + step_y] != null)
                {
                    if (gener_cells[coords.Item1 + step_x, coords.Item2 + step_y] == value)
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        public bool check_row(int value, (int,int) coords)
        {
            for(int i = 0; i < 13; i++)
            {
                if(gener_cells[i, coords.Item2] == value)
                {
                    return true;
                }
            }

            return false;
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
