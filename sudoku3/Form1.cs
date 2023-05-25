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
using System.Drawing.Configuration;

namespace sudoku3
{
    public partial class Form1 : Form
    {
        public Stopwatch watch;
        public Pen pen;
        public Color board_color = Color.White;
        public Color editable_cells_color = Color.CadetBlue;
        public Color decision_panel_killer_color = Color.White;
        public Color bg_color = Color.FromArgb(50, 50, 50);
        public Color additable_color = Color.DarkGray;

        public MODULES_ACTIVE MODULE_ACTIVE;
        public enum MODULES_ACTIVE : int
        {
            MENU,
            CHOICE_TYPE,
            KROSSVORD,
            END_OF_GAME,
            SAVED_SUDOKU,
            STATISTICS
        }

        public Player player;
        //public Board board;
        //public TriangularBoard tr_board;
        public BoardType mainboard;
        //public Dictionary<int, Board> saved_boards;
        public Dictionary<int, BoardType> saved_boards;
        public ClassicGenerator classicGenerator;
        public KillerGenerator killerGenetaror;
        public TriangularGenerator triangularGenerator;

        public Button button_start;
        public Button button_classic;
        public Button button_killer;
        public Button button_hoshi;
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
        public Label label_index_save;

        public Label label_statistics;

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
                mainboard.draw(e.Graphics);
                draw_lives(e.Graphics);
            }
        }

        private void Form1_MouseClick(object sender, MouseEventArgs e)
        {
            if (MODULE_ACTIVE == MODULES_ACTIVE.KROSSVORD)
            {
                CellType c = mainboard.which_cell_clicked(Control.MousePosition);
                if (c != null)
                {
                    if (c.editable == false)
                    {
                        return;
                    }
                    if (mainboard.active_cell != null && decision_panel != null)
                    {
                        decision_panel.Dispose();
                    }
                    mainboard.active_cell = c;
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
                            mainboard.active_cell.value = "";
                            mainboard.empty_cells_n++;
                            decision_panel.Dispose();
                            mainboard.check_all_cells_for_mistake();
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
            destroy_menu_ui();
            create_choice_type_sudoku_ui();
            MODULE_ACTIVE = MODULES_ACTIVE.CHOICE_TYPE;
        }

        public void button_classic_click(object sender, EventArgs e)
        {
            classicGenerator = new ClassicGenerator();
            mainboard = new Board(this, Board.MODES.CLASSIC);
            destroy_choice_type_sudoku_ui();
            create_krossvord_ui();
            mainboard.lives = 3;
            Invalidate();
            MODULE_ACTIVE = MODULES_ACTIVE.KROSSVORD;
            watch.Start();
        }

        public void button_killer_click(object sender, EventArgs e)
        {
            classicGenerator = new ClassicGenerator();
            killerGenetaror = new KillerGenerator();
            mainboard = new Board(this, Board.MODES.KILLER);
            destroy_choice_type_sudoku_ui();
            create_krossvord_ui();
            mainboard.lives = 3;
            Invalidate();
            MODULE_ACTIVE = MODULES_ACTIVE.KROSSVORD;
            watch.Start();
        }

        public void button_hoshi_click(object sender, EventArgs e)
        {
            triangularGenerator = new TriangularGenerator();
            mainboard = new TriangularBoard(this);
            destroy_choice_type_sudoku_ui();
            create_krossvord_ui();
            mainboard.lives = 3;
            Invalidate();
            MODULE_ACTIVE = MODULES_ACTIVE.KROSSVORD;
            watch.Start();
        }

        public void button_statistics_click(object sender, EventArgs e)
        {
            if(player.playes_n < 3)
            {
                MessageBox.Show("«¿ ŒÕ◊»“≈ ’Œ“ﬂ ¡€ 3 »√–€, ◊“Œ¡€ œŒ—ÃŒ“–≈“‹ —“¿“»—“» ”!");
                return;
            }
            MODULE_ACTIVE = MODULES_ACTIVE.STATISTICS;
            destroy_menu_ui();
            create_statistics_ui();
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
                string number = "";
                bool search = true;
                foreach(char c in (string)listbox_saved_sudoku.SelectedItem)
                {
                    if(search)
                    {
                        if (c == '#')
                        {
                            search = false;
                        }
                    } else
                    {
                        if(c == '#')
                        {
                            break;
                        }
                        number += c;
                    }
                }
                if(saved_boards[Convert.ToInt32(number)] is Board)
                {
                    this.mainboard = new Board((Board)saved_boards[Convert.ToInt32(number)], this);
                } else
                {
                    this.mainboard = new TriangularBoard((TriangularBoard)saved_boards[Convert.ToInt32(number)], this);
                }
                
                destroy_saved_sudoku_ui();
                create_krossvord_ui();
                Invalidate();
                MODULE_ACTIVE = MODULES_ACTIVE.KROSSVORD;
                watch.Start();
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
                destroy_statistics_ui();
            }
            if(MODULE_ACTIVE == MODULES_ACTIVE.CHOICE_TYPE)
            {
                destroy_choice_type_sudoku_ui();
            }
            MODULE_ACTIVE = MODULES_ACTIVE.MENU;
            create_menu_ui();
        }

        public void decision_panel_add(CellType c)
        {
            decision_panel = new TableLayoutPanel();
            decision_panel.ColumnCount = 3;
            decision_panel.RowCount = 3;
            decision_panel.Width = mainboard.cellwidth;
            decision_panel.Height = mainboard.cellwidth;
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
                decision_buttons[i].Width = mainboard.cellwidth / 3;
                decision_buttons[i].Height = mainboard.cellwidth / 3;
                 
                if (mainboard.mode == BoardType.MODES.CLASSIC)
                {
                    decision_buttons[i].BackColor = editable_cells_color;
                }
                else if (mainboard.mode == BoardType.MODES.KILLER)
                {
                    decision_buttons[i].BackColor = decision_panel_killer_color;
                }
                
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
                    if (mainboard.active_cell.value == "")
                    {
                        mainboard.empty_cells_n--;
                    }
                    mainboard.active_cell.value = b.Text;
                    int mnb = mainboard.mistake_cells_n;
                    bool cor = mainboard.active_cell.correct;
                    mainboard.check_all_cells_for_mistake();
                    if ((mnb < mainboard.mistake_cells_n) || (cor == false && mainboard.active_cell.correct == false))
                    {
                        mainboard.lives--;
                        if (mainboard.lives == 0)
                        {
                            end_of_game(0);
                        }
                    }

                    if (mainboard.empty_cells_n == 0 && mainboard.mistake_cells_n == 0)
                    {
                        end_of_game(1);
                    }
                }
            }
            decision_panel.Dispose();
            Invalidate();
        }

        public void draw_lives(Graphics e)
        {
            int s = 5;
            int offsetW = (int)(this.ClientSize.Width * 0.405);
            int paddingW = (int)(this.ClientSize.Width * 0.025);
            int live_sizeW = (int)(18 * s);

            int offsetH = (int)(this.ClientSize.Height * 0.075);

            for (int i = 0; i < mainboard.lives; i++)
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
            watch.Stop();
            destroy_krossvord_ui();
            if(decision_panel != null && !decision_panel.IsDisposed)
            {
                decision_panel.Dispose();
            }
            Invalidate();
            MODULE_ACTIVE = MODULES_ACTIVE.END_OF_GAME;
            mainboard.solution_time += watch.ElapsedMilliseconds;
            watch.Reset();

            switch (result)
            {
                case 0: // ÔÓ‡ÊÂÌËÂ
                    saved_boards.Remove(mainboard.save_index);
                    File.Delete($"savings/save{mainboard.save_index}.dat");
                    player.saves_n--;
                    add_statistics(result);
                    break;
                case 1: // ÔÓ·Â‰‡
                    saved_boards.Remove(mainboard.save_index);
                    File.Delete($"savings/save{mainboard.save_index}.dat");
                    player.saves_n--;
                    add_statistics(result);
                    break;
                case 2: // ÒÓı‡ÌÂÌËÂ
                    BinaryFormatter formatter = new BinaryFormatter();
                    if(mainboard.saved == true)
                    {
                        using (FileStream fs = new FileStream($"savings/save{mainboard.save_index}.dat", FileMode.Create, FileAccess.Write))
                        {
                            formatter.Serialize(fs, mainboard);
                        }
                    } else
                    {
                        int index = 0;
                        while(File.Exists($"savings/save{index}.dat"))
                        {
                            index++;
                        }

                        mainboard.save_index = index;
                        mainboard.saved = true;
                        using (FileStream fs = new FileStream($"savings/save{mainboard.save_index}.dat", FileMode.Create, FileAccess.Write))
                        {
                            formatter.Serialize(fs, mainboard);
                        }
                        player.saves_n++;
                    }
                    break;
            }
            load_savings();
            create_end_ui(result);
        }

        public void add_statistics(int result)
        {
            player.win_n += result;
            player.playes_n++;
            if(mainboard.solution_time < player.best_time && result==1)
            {
                player.best_time = (int)mainboard.solution_time;
            }
            if(mainboard.lives == 3)
            {
                player.win_without_mistakes_n++;
            }
        }

        public void load_savings()
        {
            BinaryFormatter formatter = new BinaryFormatter();
            saved_boards = new Dictionary<int,BoardType>();
            foreach(string file in Directory.EnumerateFiles("savings", "*", SearchOption.AllDirectories))
            {
                FileStream saving = new FileStream(file, FileMode.Open, FileAccess.Read);
                BoardType save_b = (BoardType)formatter.Deserialize(saving);
                if(save_b.mode != BoardType.MODES.TRIANGLE)
                {
                    Board board = (Board)save_b;
                    saved_boards.Add(board.save_index, board);
                } else
                {
                    TriangularBoard board = (TriangularBoard)save_b;
                    saved_boards.Add(board.save_index, board);
                }
                saving.Close();
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

        public void create_choice_type_sudoku_ui()
        {
            button_classic = new Button();
            button_classic.Width = 400;
            button_classic.Height = 200;
            button_classic.BackgroundImage = Image.FromFile("classic.png");
            button_classic.Location = new Point((int)(ClientSize.Width * 0.1),
                                                (ClientSize.Height - button_classic.Height) / 2);
            button_classic.Click += button_classic_click;

            this.Controls.Add(button_classic);

            button_killer = new Button();
            button_killer.Width = 400;
            button_killer.Height = 200;
            button_killer.Image = Image.FromFile("killer.png");
            button_killer.Location = new Point((int)(ClientSize.Width * 0.7),
                                                (ClientSize.Height - button_killer.Height) / 2);
            button_killer.Click += button_killer_click;

            this.Controls.Add(button_killer);

            button_hoshi = new Button();
            button_hoshi.Width = 400;
            button_hoshi.Height = 200;
            button_hoshi.Image = Image.FromFile("hoshi.png");
            button_hoshi.Location = new Point((int)(ClientSize.Width * 0.4),
                                                (ClientSize.Height - button_hoshi.Height) / 2);
            button_hoshi.Click += button_hoshi_click;

            this.Controls.Add(button_hoshi);

            button_escape = new Button();
            button_escape.Width = 100;
            button_escape.Height = 50;
            button_escape.Location = new Point(this.ClientSize.Width - button_quit.Width, 50);
            button_escape.Text = "Õ¿«¿ƒ";
            button_escape.Click += button_escape_click;

            this.Controls.Add(button_escape);
        }

        public void create_saved_sudoku_ui()
        {
            listbox_saved_sudoku = new ListBox();
            listbox_saved_sudoku.Width = 400;
            listbox_saved_sudoku.Height = 400;
            listbox_saved_sudoku.BackColor = additable_color;
            listbox_saved_sudoku.Left = (this.ClientSize.Width - listbox_saved_sudoku.Width) / 2;
            listbox_saved_sudoku.Top = (int)(this.ClientSize.Height * 0.3);

            foreach(KeyValuePair<int,BoardType> save in saved_boards)
            {
                listbox_saved_sudoku.Items.Add($"—Œ’–¿Õ≈Õ»≈ #{save.Value.save_index}# | –≈∆»Ã: {save.Value.mode}");
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

        public void create_statistics_ui()
        {
            Font font = new Font("Microsoft Sans Serif", 20, FontStyle.Bold);
            label_statistics = new Label();
            label_statistics.Text = "";
            label_statistics.Text += $"œŒ¡≈ƒ ¬—≈√Œ: {player.win_n}\n";
            label_statistics.Text += $"œ–Œ÷≈Õ“ œŒ¡≈ƒ: {player.win_n / player.playes_n * 100} %\n";
            label_statistics.Text += $"œ–Œ÷≈Õ“ œŒ¡≈ƒ: ¬€ —€√–¿À» 0 –¿«\n";
            label_statistics.Text += $"À”◊ÿ≈≈ ¬–≈Ãﬂ –≈ÿ≈Õ»ﬂ: {player.best_time / 1000 / 60} ÏËÌ {player.best_time / 1000 % 60} ÒÂÍ\n";
            label_statistics.Text += $" ŒÀ»◊≈—“¬Œ œŒ¡≈ƒ ¡≈« Œÿ»¡Œ : {player.win_without_mistakes_n}\n";
            label_statistics.Width = 650;
            label_statistics.Height = 400;
            label_statistics.Font = font;
            label_statistics.BackColor = additable_color;
            if (player.win_n == 0)
            {
                label_statistics.Text += $"œ–Œ÷≈Õ“ œŒ¡≈ƒ ¡≈« Œÿ»¡Œ : ¬€ ≈Ÿ≈ Õ»–¿«” Õ≈ ¬€…√–¿À»\n";
            }
            else
            {
                label_statistics.Text += $"œ–Œ÷≈Õ“ œŒ¡≈ƒ ¡≈« Œÿ»¡Œ : {player.win_without_mistakes_n / player.win_n}\n";
            }
            label_statistics.Location = new Point(
                (this.ClientSize.Width - label_statistics.Width) / 2,
                (int)(this.ClientSize.Height * 0.3)
                );
            

            this.Controls.Add(label_statistics);

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
            long min = mainboard.solution_time / 1000 / 60;
            long sec = mainboard.solution_time / 1000 % 60;
            label_time.Text = $"¬¿ÿ≈ ¬–≈Ãﬂ: {min} ÏËÌ {sec} ÒÂÍ\n" +
                        $" ŒÀ»◊≈—“¬Œ Œÿ»¡Œ : {3 - mainboard.lives}";
            label_time.AutoSize = true;
            label_time.Width = 200;
            label_time.Height = 300;
            label_time.Font = time_font;
            label_time.BackColor = Color.LightGray;
            label_time.Location = new Point((int)(this.ClientSize.Width * 0.2),
                                     (int)(this.ClientSize.Height * 0.35));
            this.Controls.Add(label_time);

            if(result == 2)
            {
                label_index_save = new Label();
                label_index_save.Text = $"—Œ’–¿Õ≈ÕŒ œŒƒ ÕŒÃ≈–ŒÃ {mainboard.save_index}";
                label_index_save.AutoSize = true;
                label_index_save.Width = 500;
                label_index_save.BackColor = bg_color;
                label_index_save.ForeColor = Color.FromArgb(100, 100, 100);
                label_index_save.Font = instr_font;
                label_index_save.Location = new Point(
                    (int)((this.ClientSize.Width - label_index_save.Width) / 2),
                    (int)(this.ClientSize.Height * 0.68)
                    );

                this.Controls.Add(label_index_save);
            }

            label_instr = new Label();
            label_instr.Text = "Õ¿∆Ã»“≈ ENTER ƒÀﬂ œ–ŒƒŒÀ∆≈Õ»ﬂ";
            label_instr.AutoSize = true;
            label_instr.Width = 600;
            label_instr.ForeColor = Color.FromArgb(100, 100, 100);
            label_instr.BackColor = bg_color;
            label_instr.Font = instr_font;
            label_instr.Location = new Point(
                  (int)((this.ClientSize.Width - label_instr.Width) / 2),
                  (int)(this.ClientSize.Height * 0.8)
                  );
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

        public void destroy_choice_type_sudoku_ui()
        {
            button_classic.Dispose();
            button_killer.Dispose();
            button_hoshi.Dispose();
            button_escape.Dispose();
        }

        public void destroy_statistics_ui()
        {
            label_statistics.Dispose();
            button_escape.Dispose();
        }

        public void destroy_end_of_game_ui()
        {
            label_instr.Dispose();
            label_res.Dispose();
            label_time.Dispose();
            if(label_index_save != null && !label_index_save.IsDisposed)
            {
                label_index_save.Dispose();
            }
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
        public int saves_n = 0;
        public int win_n = 0;
        public int playes_n = 0;
        public int best_time = 0;
        public int win_without_mistakes_n = 0;
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