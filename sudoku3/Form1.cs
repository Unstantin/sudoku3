using System.Diagnostics;
using System.Drawing.Drawing2D;
using System.Drawing;
using System.Windows.Forms;
using System.Windows.Forms.VisualStyles;

namespace sudoku3
{
    public partial class Form1 : Form
    {
        public Stopwatch watch;
        public Pen pen;
        public Color board_color = Color.White;
        public Color editable_cells_color = Color.CadetBlue;
        public Color bg_color = Color.FromArgb(50, 50, 50);

        public MODULES_ACTIVE MODULE_ACTIVE;
        public enum MODULES_ACTIVE: int {
            MENU,
            KROSSVORD,
            END_OF_GAME
        }

        public Player player;
        public Board board;

        public Button button_start;
        public Button button_quit;

        public Button[] decision_buttons;
        public TableLayoutPanel decision_panel;

        public Form1()
        {
            InitializeComponent();

            watch = new Stopwatch();

            pen = new Pen(board_color, 2);
            player = new Player();
            MODULE_ACTIVE = MODULES_ACTIVE.MENU;
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            create_menu_ui();
            //create_end_ui();
        }

        private void Form1_Paint(object sender, PaintEventArgs e)
        {
            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
            e.Graphics.Clear(bg_color);
            if (MODULE_ACTIVE == MODULES_ACTIVE.KROSSVORD) { 
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

            if(MODULE_ACTIVE == MODULES_ACTIVE.END_OF_GAME)
            {
                if(e.KeyValue == 13)
                {
                    MODULE_ACTIVE = MODULES_ACTIVE.MENU;
                }
            }
        }

        private void button_start_click(object sender, EventArgs e)
        {
            board = new Board(this);
            button_start.Dispose();
            player.lives = 3;
            Invalidate();
            MODULE_ACTIVE = MODULES_ACTIVE.KROSSVORD;
            watch.Start();
        }

        private void button_quit_Click(object sender, EventArgs e)
        {
            this.Close();
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
                    if(board.active_cell.value == "")
                    {
                        board.empty_cells_n--;
                    }
                    board.active_cell.value = b.Text;
                    int mnb = board.mistake_cells_n;
                    board.check_all_cells_for_mistake();
                    if(mnb <= board.mistake_cells_n)
                    {
                        player.lives--;
                        if(player.lives == 0)
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
            int live_sizeW = (int)(18*s);

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
            MODULE_ACTIVE = MODULES_ACTIVE.END_OF_GAME;
            watch.Stop();
            player.solution_time_sec = watch.ElapsedMilliseconds;

            switch (result)
            {
                case 0: // ïîðàæåíèå
                    break;
                case 1: // ïîáåäà
                    break;
                case 2: // ñîõðàíåíèå
                    break;
            }

            create_end_ui(result);
        }

        public void create_end_ui(int result)
        {
            Color[] colors = {Color.DarkRed, Color.YellowGreen };
            Font res_font = new Font("Microsoft Sans Serif", 60, FontStyle.Bold);
            Font time_font = new Font("Microsoft Sans Serif", 25, FontStyle.Bold);
            Font instr_font = new Font("Microsoft Sans Serif", 20, FontStyle.Bold);

            Label res = new Label();
            res.AutoSize = true;
            res.Width = 350;
            res.Height = 90;
            res.BackColor = colors[result];
            res.Text = "ÈÒÎÃÈ";
            res.Font = res_font;
            res.Location = new Point((this.ClientSize.Width - res.Width) / 2,
                                       (int)(this.ClientSize.Height * 0.2));
            this.Controls.Add(res);

            Label time = new Label();
            long min = watch.ElapsedMilliseconds / 1000 / 60;
            long sec = watch.ElapsedMilliseconds / 1000 % 60;
            time.Text = $"ÂÀØÅ ÂÐÅÌß: {min} ìèí {sec} ñåê\n" +
                        $"ÊÎËÈ×ÅÑÒÂÎ ÎØÈÁÎÊ: {3 - player.lives}";
            time.AutoSize = true;
            time.Width = 200;
            time.Height = 300;
            time.Font = time_font;
            time.BackColor = Color.LightGray;
            time.Location = new Point((int)(this.ClientSize.Width * 0.2), 
                                     (int)(this.ClientSize.Height * 0.35));
            this.Controls.Add(time);

            Label instr = new Label();
            instr.Text = "ÍÀÆÌÈÒÅ ENTER ÄËß ÏÐÎÄÎËÆÅÍÈß";
            instr.AutoSize= true;
            instr.Width = 600;
            instr.ForeColor = Color.FromArgb(100,100,100);
            instr.BackColor = bg_color;
            instr.Font = instr_font;   
            instr.Location = new Point((int)((this.ClientSize.Width - instr.Width) / 2),
                                       (int)(this.ClientSize.Height * 0.8));
            this.Controls.Add(instr);
        }

        public void create_menu_ui()
        {
            button_quit = new Button();
            button_quit.Click += button_quit_Click;
            button_quit.Text = "ÂÛÉÒÈ";
            button_quit.Width = 100;
            button_quit.Height = 50;
            button_quit.Location = new Point(this.ClientSize.Width - button_quit.Width, 0);

            button_start = new Button();
            button_start.Left = (this.ClientSize.Width - button_start.Width) / 2;
            button_start.Top = (this.ClientSize.Height - button_start.Height) / 2;
            button_start.Width = 100;
            button_start.Height = 50;
            button_start.Text = "ÍÀ×ÀÒÜ";
            button_start.Click += button_start_click;

            this.Controls.Add(button_start);
            this.Controls.Add(button_quit);
        }
    }

    public class Player {
        public int lives;
        public long solution_time_sec;
    }
}