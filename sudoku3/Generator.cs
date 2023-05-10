using Microsoft.VisualBasic.ApplicationServices;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace sudoku3
{
    internal class Generator
    {
        public static int?[,] BASE =
            {{1,2,3,4,5,6,7,8,9},
             {4,5,6,7,8,9,1,2,3},
             {7,8,9,1,2,3,4,5,6},
             {2,3,4,5,6,7,8,9,1},
             {5,6,7,8,9,1,2,3,4},
             {8,9,1,2,3,4,5,6,7},
             {3,4,5,6,7,8,9,1,2},
             {6,7,8,9,1,2,3,4,5},
             {9,1,2,3,4,5,6,7,8}};

        static int?[,] base_sudoku = BASE;

        static public void classic(Cell[,] cells)
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

        static private void swap_colums_or_rows(int mode)
        {
            Random r = new Random();
            int block =  r.Next() % 3;
            int cor1 = r.Next() % 3;
            int cor2 = r.Next() % 3;
            
            while(cor1 == cor2)
            {
                cor2 = r.Next() % 3;
            }
/*
            Console.WriteLine(mode);
            Console.WriteLine(block);
            Console.WriteLine(cor1);
            Console.WriteLine(cor2);*/

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

        static private void swap_blocks(int mode)
        {
            Random r = new Random();
            int block1 = r.Next() % 3;
            int block2 = r.Next() % 3;
            while(block1 == block2)
            {
                block2 = r.Next() % 3;
            }
            /*
            Console.WriteLine(block1);
            Console.WriteLine(block2);*/

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

        static private void clear_random_cells(Cell[,] cells)
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
}
