using System.Windows.Forms;

namespace sudoku3
{
    public partial class Form1 : Form
    {
        public Pen pen;

        public string MODULE_ACTIVE;

        public Board board;

        public Button button_start;
        public Button button_quit;

        public Button[] decision_buttons;
        public TableLayoutPanel decision_panel;

        public Form1()
        {
            InitializeComponent();

            pen = new Pen(Color.FromArgb(0, 0, 0));
            board = new Board(this);
            MODULE_ACTIVE = "menu";
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            button_quit = new Button();
            button_quit.Click += button_quit_Click;
            button_quit.Text = "¬€…“»";
            button_quit.Width = 100;
            button_quit.Height = 50;
            button_quit.Location = new Point(this.ClientSize.Width - button_quit.Width, 0);

            button_start = new Button();
            button_start.Left = (this.ClientSize.Width - button_start.Width) / 2;
            button_start.Top = (this.ClientSize.Height - button_start.Height) / 2;
            button_start.Width = 100;
            button_start.Height = 50;
            button_start.Text = "Õ¿◊¿“‹";
            button_start.Click += button_start_click;

            this.Controls.Add(button_start);
            this.Controls.Add(button_quit);
        }

        private void Form1_Paint(object sender, PaintEventArgs e)
        {
            if (MODULE_ACTIVE == "krossvord") { board.draw(e.Graphics); };
        }

        private void Form1_MouseClick(object sender, MouseEventArgs e)
        {
            if(MODULE_ACTIVE == "krossvord")
            {
                Cell c = board.which_cell_clicked(Control.MousePosition);
                if(c != null)
                {
                    if(c.editable == false)
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
            if(MODULE_ACTIVE == "menu")
            {

            }
        }

        private void Form1_KeyDown(object sender, KeyEventArgs e)
        {
            if (MODULE_ACTIVE == "krossvord")
            {
                if (e.KeyValue == 27 && !decision_panel.IsDisposed)
                {
                    decision_panel.Dispose();
                }

                if(!decision_panel.Disposing)
                {
                    if(e.KeyValue == 46 || e.KeyValue == 8)
                    {
                        board.active_cell.value = "";
                        board.active_cell.correct = true;
                        decision_panel.Dispose();
                    }
                }
            }
        }

        private void button_start_click(object sender, EventArgs e)
        {
            button_start.Dispose();
            Invalidate();
            MODULE_ACTIVE = "krossvord";
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
            decision_panel.BackColor = Color.Gray;

            int N = 10;
            decision_buttons = new Button[N];
            for (int i = 1; i < N; i++)
            {
                decision_buttons[i] = new Button();
                decision_buttons[i].ForeColor = Color.Black;
                decision_buttons[i].Text = Convert.ToString(i);
                decision_buttons[i].Margin = new Padding(0);
                decision_buttons[i].FlatStyle= FlatStyle.Flat;
                decision_buttons[i].Width = board.cellwidth / 3;
                decision_buttons[i].Height = board.cellwidth / 3;
                decision_buttons[i].BackColor = Color.LightBlue;
                decision_buttons[i].FlatAppearance.BorderColor = Color.Black;
                decision_buttons[i].Click += decision_button_click;

                decision_panel.Controls.Add(decision_buttons[i]);
            }

            this.Controls.Add(decision_panel);
        }

        private void decision_button_click(object sender, EventArgs e)
        {
            foreach(Button b in decision_buttons)
            {
                if (sender == b)
                {
                    board.active_cell.value = b.Text;
                    board.empty_cells_n--;
                    board.active_cell.correct = board.active_cell.check_correctness();
                }
            }

            decision_panel.Dispose();
        }
    }
}