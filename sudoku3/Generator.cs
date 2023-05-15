using Microsoft.VisualBasic.ApplicationServices;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Security.Cryptography.Xml;
using System.Text;
using System.Threading.Tasks;

namespace sudoku3
{
    public class Generator
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

        public void classic(Cell[,] cells)
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
            int M = 25;
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

    //бесконечный цикл из-за того, что у блоков может и не быть рядом никаких клеток (см для программы)
    internal class ColorGenerator
    {
        static List<(int, int)>[] blocks;
        static int Mi = 3;

        static List<(int, int, int, int)> already_checked_pairs_cells = new List<(int, int, int, int)>();

        static int block_integrity_n;
        static List<(int, int)> already_checked_cells_integrity;

        static public void color(Cell[,] cells)
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

        static public void Init()
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

        static public void create_blocks()
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
                    if (!ColorGenerator.check_integrity(blocks[i]) || !ColorGenerator.check_integrity(blocks[n2]))
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

        static public int mix(int n)
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

        static public void mix_two(int n1, int n2)
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

        static bool check_integrity(List<(int,int)> block)
        {
            block_integrity_n = 0;
            already_checked_cells_integrity = new List<(int, int)>();
            check_neighbors(block[0], block);

            //Console.WriteLine($"Я нашел только {block_integrity_n} в этом блоке");

            return block_integrity_n == 9;
        }

        static void check_neighbors((int,int) cell, List<(int,int)> block)
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
