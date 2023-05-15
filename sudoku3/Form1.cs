using System.Diagnostics;
using System.Drawing.Drawing2D;
using System.IO;
using System.Drawing;
using System.Windows.Forms;
using System.Windows.Forms.VisualStyles;
using System.Runtime.Serialization.Formatters.Binary;
using System;
using System.Runtime.Serialization;
using Microsoft.VisualBasic;

namespace sudoku3
{
    public partial class Form1 : Form
    {
        public Stopwatch watch;
        public Pen pen;
        public Color board_color = Color.White;
        public Color editable_cells_color = Color.CadetBlue;
        public Color bg_color = Color.FromArgb(50, 50, 50);
        public Color list_saved_sudoku_bg = Color.DarkGray;

        public MODULES_ACTIVE MODULE_ACTIVE;
        public enum MODULES_ACTIVE : int
        {
            MENU,
            KROSSVORD,
            END_OF_GAME,
            SAVED_SUDOKU,
            STATISTICS
        }

        public Player player;
        public Board board;
        public Dictionary<int, Board> saved_boards;
        public Generator generator;

        public Button button_start;
        public Button button_start_in_saved_sudoku;
        public Button_quit button_quit;
        public Button button_escape;
        public Button button_save_sudoku;
        public Button button_statistics;
        public Button button_saved_sudoku;
        public ListBox listbox_saved_sudoku;

        public Button[] decision_buttons;
        public TableLayoutPanel decision_panel;

        public Label label_res;
        public Label label_time;
        public Label label_instr;

        public Form1()
        {
            InitializeComponent();
            BinaryFormatter formatter = new BinaryFormatter();

            watch = new Stopwatch();

            pen = new Pen(board_color, 2);
            if(!File.Exists("player.dat"))
            {
                player = new Player();
            } else
            {
                FileStream fs = new FileStream($"player.dat", FileMode.Open, FileAccess.Read);
                player = (Player)formatter.Deserialize(fs);
                fs.Close();
            }
            MODULE_ACTIVE = MODULES_ACTIVE.MENU;

            if (!Directory.Exists("savings"))
            {
                Directory.CreateDirectory("savings");
            }
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            button_quit = new Button_quit();
            button_quit.Click += button_quit_click;
            button_quit.Text = "¬€…“»";
            button_quit.Width = 100;
            button_quit.Height = 50;
            button_quit.Location = new Point(this.ClientSize.Width - button_quit.Width, 0);

            this.Controls.Add(button_quit);

            load_savings();
            create_menu_ui();
        }

        private void Form1_Paint(object sender, PaintEventArgs e)
        {
            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
            e.Graphics.Clear(bg_color);
            if (MODULE_ACTIVE == MODULES_ACTIVE.KROSSVORD)
            {
                board.draw(e.Graphics);
                draw_lives(e.Graphics);
            };
        }

        private void Form1_MouseClick(object sender, MouseEventArgs e)
        {
            if (MODULE_ACTIVE == MODULES_ACTIVE.KROSSVORD)
            {
                Cell c = board.which_cell_clicked(Control.MousePosition);
                if (c != null)
                {
                    if (c.editable == false)
                    {
                        return;
                    }
                    if (board.active_cell != null)
                    {
                        decision_panel.Dispose();
                    }
                    board.active_cell = c;
                    decision_panel_add(c);
                    Invalidate();
                }
            }
        }

        private void Form1_KeyDown(object sender, KeyEventArgs e)
        {
            if (MODULE_ACTIVE == MODULES_ACTIVE.KROSSVORD)
            {
                if (e.KeyValue == 27 && !decision_panel.IsDisposed)
                {
                    decision_panel.Dispose();
                }

                if (decision_panel != null)
                {
                    if (!decision_panel.IsDisposed)
                    {
                        if (e.KeyValue == 46 || e.KeyValue == 8)
                        {
                            board.active_cell.value = "";
                            board.empty_cells_n++;
                            decision_panel.Dispose();
                            board.check_all_cells_for_mistake();
                            Invalidate();
                        }
                    }
                }
            }

            if (MODULE_ACTIVE == MODULES_ACTIVE.END_OF_GAME)
            {
                if (e.KeyValue == 13)
                {
                    MODULE_ACTIVE = MODULES_ACTIVE.MENU;
                    destroy_end_of_game_ui();
                    create_menu_ui();
                }
            }
        }

        private void button_start_click(object sender, EventArgs e)
        {
            generator = new Generator();
            board = new Board(this, Board.MODES.CLASSIC);
            destroy_menu_ui();
            create_krossvord_ui();
            player.lives = 3;
            Invalidate();
            MODULE_ACTIVE = MODULES_ACTIVE.KROSSVORD;
            watch.Start();
        }

        public void button_statistics_click(object sender, EventArgs e)
        {

        }

        public void button_saved_sudoku_click(object sender, EventArgs e)
        {
            destroy_menu_ui();
            MODULE_ACTIVE = MODULES_ACTIVE.SAVED_SUDOKU;
            create_saved_sudoku_ui();
        }

        public void button_save_sudoku_click(object sender, EventArgs e)
        {
            end_of_game(2);
        }

        public void button_start_in_saved_sudoku_click(object sender, EventArgs e)
        {
            if(listbox_saved_sudoku.SelectedItem != null)
            {
                this.board = new Board(saved_boards[(int)listbox_saved_sudoku.SelectedItem], this);
                destroy_saved_sudoku_ui();
                create_krossvord_ui();
                Invalidate();
                MODULE_ACTIVE = MODULES_ACTIVE.KROSSVORD;
            }
        }

        private void button_quit_click(object sender, EventArgs e)
        {
            BinaryFormatter formatter = new BinaryFormatter();
            using (FileStream fs = new FileStream($"player.dat", FileMode.OpenOrCreate, FileAccess.Write))
            {
                formatter.Serialize(fs, player);
            }
            this.Close();
        }

        private void button_escape_click(object sender, EventArgs e)
        {
            if(MODULE_ACTIVE == MODULES_ACTIVE.SAVED_SUDOKU)
            {
                destroy_saved_sudoku_ui();
            }
            if(MODULE_ACTIVE == MODULES_ACTIVE.STATISTICS)
            {

            }
            MODULE_ACTIVE = MODULES_ACTIVE.MENU;
            create_menu_ui();
        }

        public void decision_panel_add(Cell c)
        {
            decision_panel = new TableLayoutPanel();
            decision_panel.ColumnCount = 3;
            decision_panel.RowCount = 3;
            decision_panel.Width = board.cellwidth;
            decision_panel.Height = board.cellwidth;
            decision_panel.Location = new Point(c.X, c.Y);
            decision_panel.BackColor = bg_color;

            int N = 10;
            decision_buttons = new Button[N];
            for (int i = 1; i < N; i++)
            {
                decision_buttons[i] = new Button();
                decision_buttons[i].ForeColor = bg_color;
                decision_buttons[i].Text = Convert.ToString(i);
                decision_buttons[i].Margin = new Padding(0);
                decision_buttons[i].FlatStyle = FlatStyle.Flat;
                decision_buttons[i].Width = board.cellwidth / 3;
                decision_buttons[i].Height = board.cellwidth / 3;
                decision_buttons[i].BackColor = editable_cells_color;
                decision_buttons[i].FlatAppearance.BorderColor = Color.Black;
                decision_buttons[i].Click += decision_button_click;

                decision_panel.Controls.Add(decision_buttons[i]);
            }

            this.Controls.Add(decision_panel);
        }

        private void decision_button_click(object sender, EventArgs e)
        {
            foreach (Button b in decision_buttons)
            {
                if (sender == b)
                {
                    if (board.active_cell.value == "")
                    {
                        board.empty_cells_n--;
                    }
                    board.active_cell.value = b.Text;
                    int mnb = board.mistake_cells_n;
                    bool cor = board.active_cell.correct;
                    board.check_all_cells_for_mistake();
                    if ((mnb < board.mistake_cells_n) || (cor == false && board.active_cell.correct == false))
                    {
                        player.lives--;
                        if (player.lives == 0)
                        {
                            end_of_game(0);
                        }
                    }

                    if (board.empty_cells_n == 0 && board.mistake_cells_n == 0)
                    {
                        end_of_game(1);
                    }
                }
            }
            decision_panel.Dispose();
        }

        public void draw_lives(Graphics e)
        {
            int s = 5;
            int offsetW = (int)(this.ClientSize.Width * 0.405);
            int paddingW = (int)(this.ClientSize.Width * 0.025);
            int live_sizeW = (int)(18 * s);

            int offsetH = (int)(this.ClientSize.Height * 0.075);

            for (int i = 0; i < player.lives; i++)
            {
                int X = offsetW + i * (paddingW + live_sizeW);
                int Y = offsetH;
                Point[] p =
                {
                    new Point(X, Y),
                    new Point(X + 2 * s, Y - 5 * s),
                    new Point(X + 7 * s, Y - 5 * s),
                    new Point(X + 9 * s, Y),
                    new Point(X + 11 * s, Y - 5 * s),
                    new Point(X + 16 * s, Y - 5 * s),
                    new Point(X + 18 * s, Y),
                    new Point(X + 9 * s, Y + 8 * s)
                };

                e.FillPolygon(Brushes.DarkRed, p);
                e.DrawPolygon(new Pen(Color.White, 3), p);
            }
        }

        public void end_of_game(int result)
        {
            destroy_krossvord_ui();
            if(decision_panel != null && !decision_panel.IsDisposed)
            {
                decision_panel.Dispose();
            }
            Invalidate();
            MODULE_ACTIVE = MODULES_ACTIVE.END_OF_GAME;
            watch.Stop();
            player.solution_time_sec = watch.ElapsedMilliseconds;

            switch (result)
            {
                case 0: // ÔÓ‡ÊÂÌËÂ
                    break;
                case 1: // ÔÓ·Â‰‡
                    break;
                case 2: // ÒÓı‡ÌÂÌËÂ
                    BinaryFormatter formatter = new BinaryFormatter();
                    if(board.saved == true)
                    {
                        using (FileStream fs = new FileStream($"savings/save{board.save_index}.dat", FileMode.Create, FileAccess.Write))
                        {
                            formatter.Serialize(fs, board);
                        }
                    } else
                    {
                        board.save_index = player.saves_n;
                        board.saved = true;
                        using (FileStream fs = new FileStream($"savings/save{player.saves_n}.dat", FileMode.Create, FileAccess.Write))
                        {
                            formatter.Serialize(fs, board);
                        }
                        player.saves_n++;
                    }
                    load_savings();
                    break;
            }

            create_end_ui(result);
        }

        public void load_savings()
        {
            BinaryFormatter formatter = new BinaryFormatter();
            saved_boards = new Dictionary<int,Board>();
            for (int i = 0; i < player.saves_n; i++)
            {
                FileStream savings = new FileStream($"savings/save{i}.dat", FileMode.Open, FileAccess.Read);
                saved_boards.Add(i, (Board)formatter.Deserialize(savings));
                savings.Close();
            }
        }

        public void create_krossvord_ui()
        {
            button_save_sudoku = new Button();
            button_save_sudoku.Width = 100;
            button_save_sudoku.Height = 50;
            button_save_sudoku.Text = "—Œ’–¿Õ»“‹";
            button_save_sudoku.Location = new Point(0, 0);
            button_save_sudoku.Click += button_save_sudoku_click;

            this.Controls.Add(button_save_sudoku);
        }

        public void create_saved_sudoku_ui()
        {
            listbox_saved_sudoku = new ListBox();
            listbox_saved_sudoku.Width = 400;
            listbox_saved_sudoku.Height = 400;
            listbox_saved_sudoku.BackColor = list_saved_sudoku_bg;
            //listbox_saved_sudoku.DrawItem += listbox_draw;
            listbox_saved_sudoku.Left = (this.ClientSize.Width - listbox_saved_sudoku.Width) / 2;
            listbox_saved_sudoku.Top = (int)(this.ClientSize.Height * 0.3);

            foreach(KeyValuePair<int,Board> save in saved_boards)
            {
                listbox_saved_sudoku.Items.Add(save.Value.save_index);
            }

            this.Controls.Add(listbox_saved_sudoku);

            button_start_in_saved_sudoku = new Button();
            button_start_in_saved_sudoku.Width = 100;
            button_start_in_saved_sudoku.Height = 50;
            button_start_in_saved_sudoku.Text = "Õ¿◊¿“‹";
            button_start_in_saved_sudoku.Click += button_start_in_saved_sudoku_click;
            button_start_in_saved_sudoku.Location = new Point(
                (int)(this.ClientSize.Width * 0.7),
                (this.ClientSize.Height - button_start_in_saved_sudoku.Height) / 2
            );

            this.Controls.Add(button_start_in_saved_sudoku);

            button_escape = new Button();
            button_escape.Width = 100;
            button_escape.Height = 50;
            button_escape.Location = new Point(this.ClientSize.Width - button_quit.Width, 50);
            button_escape.Text = "Õ¿«¿ƒ";
            button_escape.Click += button_escape_click;

            this.Controls.Add(button_escape);
        }

        public void create_end_ui(int result)
        {
            Color[] colors = { Color.DarkRed, Color.YellowGreen, Color.DimGray };
            Font res_font = new Font("Microsoft Sans Serif", 60, FontStyle.Bold);
            Font time_font = new Font("Microsoft Sans Serif", 25, FontStyle.Bold);
            Font instr_font = new Font("Microsoft Sans Serif", 20, FontStyle.Bold);

            label_res = new Label();
            label_res.AutoSize = true;
            label_res.Width = 350;
            label_res.Height = 90;
            label_res.BackColor = colors[result];
            label_res.Text = "»“Œ√»";
            label_res.Font = res_font;
            label_res.Location = new Point((this.ClientSize.Width - label_res.Width) / 2,
                                       (int)(this.ClientSize.Height * 0.2));
            this.Controls.Add(label_res);

            label_time = new Label();
            long min = watch.ElapsedMilliseconds / 1000 / 60;
            long sec = watch.ElapsedMilliseconds / 1000 % 60;
            label_time.Text = $"¬¿ÿ≈ ¬–≈Ãﬂ: {min} ÏËÌ {sec} ÒÂÍ\n" +
                        $" ŒÀ»◊≈—“¬Œ Œÿ»¡Œ : {3 - player.lives}";
            label_time.AutoSize = true;
            label_time.Width = 200;
            label_time.Height = 300;
            label_time.Font = time_font;
            label_time.BackColor = Color.LightGray;
            label_time.Location = new Point((int)(this.ClientSize.Width * 0.2),
                                     (int)(this.ClientSize.Height * 0.35));
            this.Controls.Add(label_time);

            label_instr = new Label();
            label_instr.Text = "Õ¿∆Ã»“≈ ENTER ƒÀﬂ œ–ŒƒŒÀ∆≈Õ»ﬂ";
            label_instr.AutoSize = true;
            label_instr.Width = 600;
            label_instr.ForeColor = Color.FromArgb(100, 100, 100);
            label_instr.BackColor = bg_color;
            label_instr.Font = instr_font;
            label_instr.Location = new Point((int)((this.ClientSize.Width - label_instr.Width) / 2),
                                       (int)(this.ClientSize.Height * 0.8));
            this.Controls.Add(label_instr);
        }

        public void create_menu_ui()
        {
            button_start = new Button();
            button_start.Left = (this.ClientSize.Width - button_start.Width) / 2;
            button_start.Top = (int)(this.ClientSize.Height * 0.3);
            button_start.Width = 170;
            button_start.Height = 60;
            button_start.Text = "Õ¿◊¿“‹ ÕŒ¬Œ≈ —”ƒŒ ”";
            button_start.Click += button_start_click;

            this.Controls.Add(button_start);

            button_statistics = new Button();
            button_statistics.Left = (this.ClientSize.Width - button_statistics.Width) / 2;
            button_statistics.Top = (int)(this.ClientSize.Height * 0.4);
            button_statistics.Width = 170;
            button_statistics.Height = 60;
            button_statistics.Text = "—“¿“»—“» ¿";
            button_statistics.Click += button_statistics_click;

            this.Controls.Add(button_statistics);

            button_saved_sudoku = new Button();
            button_saved_sudoku.Left = (this.ClientSize.Width - button_saved_sudoku.Width) / 2;
            button_saved_sudoku.Top = (int)(this.ClientSize.Height * 0.5);
            button_saved_sudoku.Width = 170;
            button_saved_sudoku.Height = 60;
            button_saved_sudoku.Text = "œ–ŒƒŒÀ∆»“‹ —Œ’–¿Õ≈ÕÕŒ≈ —”ƒŒ ”";
            button_saved_sudoku.Click += button_saved_sudoku_click;

            this.Controls.Add(button_saved_sudoku);
        }

        public void destroy_menu_ui()
        {
            button_start.Dispose();
            button_statistics.Dispose();
            button_saved_sudoku.Dispose();
        }

        public void destroy_krossvord_ui()
        {
            button_save_sudoku.Dispose();
        }

        public void destroy_end_of_game_ui()
        {
            label_instr.Dispose();
            label_res.Dispose();
            label_time.Dispose();
        }

        public void destroy_saved_sudoku_ui()
        {
            listbox_saved_sudoku.Dispose();
            button_saved_sudoku.Dispose();
            button_escape.Dispose();
            button_start_in_saved_sudoku.Dispose();
        }
    }

    [Serializable]
    public class Player
    {
        [NonSerialized] public int lives;
        [NonSerialized] public long solution_time_sec;
        public int saves_n = 0;
    }

    public class Button_quit : Button
    {
        protected override bool IsInputKey(Keys keyData)
        {
            if (keyData == Keys.Space)
            {
                return true;
            }
            if (keyData == Keys.Enter)
            {
                return true;
            }
            return base.IsInputKey(keyData);
        }
    }
}