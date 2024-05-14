using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace Tetris_randome2
{
    public partial class Form1 : Form
    {
        public MyControl myControl;
        public Form1()
        {
            InitializeComponent();
            myControl = new MyControl(panel1.Width, panel1.Height);
            myControl.Dock = DockStyle.Fill;
            panel1.BackColor = Color.Transparent;
            panel1.Controls.Add(myControl);
            StartNewGame();  // Запуск новой игры при инициализации формы
        }

        private void StartNewGame()
        {
            if (myControl != null)
            {
                myControl.NewGameClicked -= MyControl_NewGameClicked;
                myControl.ExitClicked -= MyControl_ExitClicked;
                myControl.GameEnded -= MyControl_GameEnded;
                // panel1.Controls.Remove(myControl);
                //myControl.Dispose();
                myControl.Controls.Clear();
            }

            // myControl = new MyControl(panel1.Width, panel1.Height);
            //myControl.Dock = DockStyle.Fill;
            //panel1.BackColor = Color.Transparent;
            //panel1.Controls.Add(myControl);

            Random rnd = new Random();
            int rnd_n = rnd.Next(0, 6);
            myControl.RndomeFig(rnd_n);

            // Подписываемся на события из контрола MyControl
            myControl.NewGameClicked += MyControl_NewGameClicked;
            myControl.ExitClicked += MyControl_ExitClicked;
            myControl.GameEnded += MyControl_GameEnded;

            timer1.Interval = 10; // Интервал в миллисекундах (1000 миллисекунд = 1 секунда)
            timer1.Start();
        }

        private void MyControl_NewGameClicked(object sender, EventArgs e)
        {
            // Логика для начала новой игры
            StartNewGame();  // Перезапуск игры при нажатии "Новая игра"
        }

        private void MyControl_ExitClicked(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private void MyControl_GameEnded(object sender, EventArgs e)
        {
            timer1.Stop(); // Остановка таймера при окончании игры
           // MessageBox.Show("Игра окончена!");
        }
        private void timer1_Tick(object sender, EventArgs e)
        {
            myControl.MoveY_timer();
        }
    }
    public partial class MyControl : UserControl
    {    // Событие окончания игры
        public event EventHandler GameEnded;
        public Tetromino Figrnd;
        public Tetromino[] Figrnd_group;
        public Tetromino[][] Figs;
        public int width = 0;
        public int height = 0;
        public int current_figure = 0;
        public int current_mod = 0;
        public int scale = 3;
        public int speed_moveY = 2;
        public int speed_moveX = 22;
        public Tetromino[] Figrnd_out;
        public int Score = 0;
       
        public MyControl(int width_, int height_)
        {
            width = width_;
            height = height_;
            SetStyle(ControlStyles.OptimizedDoubleBuffer | ControlStyles.SupportsTransparentBackColor, true);
            BackColor = Color.Transparent;
            this.DoubleBuffered = true;
            Figs = new Tetromino[7][];
            Figs[0] = [
               TetrominoFactory.CreateFig1(),
                TetrominoFactory.CreateFig1_1()
               ];

            Figs[1] = [
                TetrominoFactory.CreateFig2(),
                TetrominoFactory.CreateFig2_1(),
                TetrominoFactory.CreateFig2_2(),
                TetrominoFactory.CreateFig2_3()
              ];

            Figs[2] = [
                TetrominoFactory.CreateFig3(),
                TetrominoFactory.CreateFig3_1(),
                TetrominoFactory.CreateFig3_2(),
                TetrominoFactory.CreateFig3_3()
                ];
            Figs[3] = [
                TetrominoFactory.CreateFig4(),
                TetrominoFactory.CreateFig4_1(),
                TetrominoFactory.CreateFig4_2(),
                TetrominoFactory.CreateFig4_3()
                ];
            Figs[4] = [
                TetrominoFactory.CreateFig5()
                ];
            Figs[5] = [

                TetrominoFactory.CreateFig6(),
                TetrominoFactory.CreateFig6_1()
                ];
            Figs[6] = [
                TetrominoFactory.CreateFig7(),
                TetrominoFactory.CreateFig7_1()
                ];
            Scale(scale, Figs);


            Figrnd_out = [];          
            decimal Ydec = Math.Round((decimal)height / (scale * 11), 0);
            int Y0 = (int)Ydec * (scale * 11);
            //Y0 = 0;
            int stepX = scale * 11;
            for (int j = 0; j <= width / (stepX * 1); j += 4)
            {
                Array.Resize(ref Figrnd_out, Figrnd_out.Length + 1);
                Figrnd_out[Figrnd_out.Length - 1] = new Tetromino(
                [new Point(j * stepX, Y0), new Point(j * stepX + scale * 10, Y0), new Point(j * stepX + scale * 10, Y0 + scale * 10), new Point(j * stepX, Y0 + scale * 10)],
                [new Point((j + 1) * stepX, Y0), new Point((j + 1) * stepX + scale * 10, Y0), new Point((j + 1) * stepX + scale * 10, Y0 + scale * 10), new Point((j + 1) * stepX, Y0 + scale * 10)],
                [new Point((j + 2) * stepX, Y0), new Point((j + 2) * stepX + scale * 10, Y0), new Point((j + 2) * stepX + scale * 10, Y0 + scale * 10), new Point((j + 2) * stepX, Y0 + scale * 10)],
                [new Point((j + 3) * stepX, Y0), new Point((j + 3) * stepX + scale * 10, Y0), new Point((j + 3) * stepX + scale * 10, Y0 + scale * 10), new Point((j + 3) * stepX, Y0 + scale * 10)]
                 );
            }


        }
        protected override void OnPaintBackground(PaintEventArgs e)
        {
            // Не вызываем базовую реализацию для предотвращения мерцания фона
        }

        protected override void OnPaint(PaintEventArgs e)
        {

            base.OnPaint(e);

            Graphics g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.FillRectangle(new SolidBrush(Color.Black), this.ClientRectangle);

            // Создаем кисть для заливки прямоугольника
            SolidBrush brushr = new SolidBrush(Color.Blue);
            // Заполняем прямоугольник
            if (Figrnd != null)
            {
                e.Graphics.FillPolygon(brushr, Figrnd.PointsRect1);
                e.Graphics.FillPolygon(brushr, Figrnd.PointsRect2);
                e.Graphics.FillPolygon(brushr, Figrnd.PointsRect3);
                e.Graphics.FillPolygon(brushr, Figrnd.PointsRect4);
            
          
            }

            if (Figrnd_out != null)
            {
                for (int i = 0; i < Figrnd_out.Length; i++)

                {
                    e.Graphics.FillPolygon(brushr, Figrnd_out[i].PointsRect1);
                    e.Graphics.FillPolygon(brushr, Figrnd_out[i].PointsRect2);
                    e.Graphics.FillPolygon(brushr, Figrnd_out[i].PointsRect3);
                    e.Graphics.FillPolygon(brushr, Figrnd_out[i].PointsRect4);
                }
            }

            g.DrawString("Score " + Score, new Font("Monotype Corsiva", 26), new SolidBrush(Color.White), new Point(10, 10));
        }


        public void RndomeFig(int fig)
        {

            current_figure = fig;
            int mod = Figs[fig].Length;
            Random rnd = new Random();
            current_mod = rnd.Next(0, mod - 1);
            

            Tetromino[] original_group = Figs[fig];
            Figrnd_group = new Tetromino[original_group.Length];

            // Для каждой фигуры в original_group создаем глубокую копию и добавляем ее в Figrnd_group
            for (int i = 0; i < original_group.Length; i++)
            {
                Tetromino original_tetromino = original_group[i];
                Point[] pointsRect1_copy = original_tetromino.PointsRect1.Select(p => new Point(p.X, p.Y)).ToArray();
                Point[] pointsRect2_copy = original_tetromino.PointsRect2.Select(p => new Point(p.X, p.Y)).ToArray();
                Point[] pointsRect3_copy = original_tetromino.PointsRect3.Select(p => new Point(p.X, p.Y)).ToArray();
                Point[] pointsRect4_copy = original_tetromino.PointsRect4.Select(p => new Point(p.X, p.Y)).ToArray();

                Figrnd_group[i] = new Tetromino(pointsRect1_copy, pointsRect2_copy, pointsRect3_copy, pointsRect4_copy);
            }

            int deltaX = ((width / 2) / (11 * scale)) * 11 * scale - 11 * scale;
            for (int i = 0; i < Figrnd_group.Length; i++)
            {
                MoveXY(Figrnd_group[i], deltaX, 0);
            }

            Figrnd  = Figrnd_group[current_mod];
           
            Invalidate();
        }

        public void Scale(int scale, Tetromino[][] Figs)
        {
            // Перебираем все фигуры в массиве Figs
            for (int i = 0; i < Figs.Length; i++)
            {
                Tetromino[] tetrominos = Figs[i]; // Получаем массив фигур

                // Перебираем каждую фигуру в массиве фигур
                for (int j = 0; j < tetrominos.Length; j++)
                {
                    Tetromino tetromino = tetrominos[j]; // Получаем фигуру

                    // Перебираем все точки в каждом прямоугольнике фигуры
                    for (int k = 0; k < tetromino.PointsRect1.Length; k++)
                    {
                        // Масштабируем координаты каждой точки на указанный масштаб
                        tetromino.PointsRect1[k] = new Point(tetromino.PointsRect1[k].X * scale, tetromino.PointsRect1[k].Y * scale);
                        tetromino.PointsRect2[k] = new Point(tetromino.PointsRect2[k].X * scale, tetromino.PointsRect2[k].Y * scale);
                        tetromino.PointsRect3[k] = new Point(tetromino.PointsRect3[k].X * scale, tetromino.PointsRect3[k].Y * scale);
                        tetromino.PointsRect4[k] = new Point(tetromino.PointsRect4[k].X * scale, tetromino.PointsRect4[k].Y * scale);
                    }
                }
            }
        }

        public void MoveXY(Tetromino t, int deltaX, int deltaY)
        {

            // Смещаем каждую точку прямоугольника на эту разницу
            for (int i = 0; i < t.PointsRect1.Length; i++)
            {
                t.PointsRect1[i] = new Point(t.PointsRect1[i].X + deltaX, t.PointsRect1[i].Y + deltaY);
                t.PointsRect2[i] = new Point(t.PointsRect2[i].X + deltaX, t.PointsRect2[i].Y + deltaY);
                t.PointsRect3[i] = new Point(t.PointsRect3[i].X + deltaX, t.PointsRect3[i].Y + deltaY);
                t.PointsRect4[i] = new Point(t.PointsRect4[i].X + deltaX, t.PointsRect4[i].Y + deltaY);
            }

        }

        public void MoveY_timer()
        {
            for (int i = 0; i < Figrnd_group.Length; i++)
            {
               

                if (
                    Moving( Figrnd,  Figrnd_out)
                   )
                {
                    MoveXY(Figrnd_group[i], 0, speed_moveY);
                }
                else
                {
                    int dY = maxFixY(Figrnd, height);
                    // выравнивание позиций Y
                    for (int j = 0; j < Figrnd.PointsRect1.Length; j++) 
                    {
                        Figrnd.PointsRect1[j].Y = Figrnd.PointsRect1[j].Y - dY;
                        Figrnd.PointsRect2[j].Y = Figrnd.PointsRect2[j].Y - dY; 
                        Figrnd.PointsRect3[j].Y = Figrnd.PointsRect3[j].Y - dY;
                        Figrnd.PointsRect4[j].Y = Figrnd.PointsRect4[j].Y - dY;
                    }

                    Array.Resize(ref Figrnd_out, Figrnd_out.Length + 1);
                    Tetromino original = Figrnd;
                   
                    Figrnd_out[Figrnd_out.Length - 1] = new Tetromino(
                        original.PointsRect1.Select(p => new Point(p.X, p.Y)).ToArray(),
                        original.PointsRect2.Select(p => new Point(p.X, p.Y)).ToArray(),
                        original.PointsRect3.Select(p => new Point(p.X, p.Y)).ToArray(),
                        original.PointsRect4.Select(p => new Point(p.X, p.Y)).ToArray()
                    );

                    SearchLines(ref Figrnd_out);



                    speed_moveY = 1;
                    Random rnd = new Random();
                    int rnd_n = rnd.Next(0, 6);
                    RndomeFig(rnd_n);
                }

            }

            Invalidate();
        }

        public void SearchLines(ref Tetromino[] Figrnd_out) 
        {


           
            int Ny = height / (11 * scale) + 1;
            int Nx = width / (11 * scale) ;

            // Создаем словарь для хранения количества заполненных клеток в каждой строке
            Dictionary<int, int> lines = new Dictionary<int, int>();

            // Инициализируем словарь значениями 0
            for (int i = 0; i < Ny; i++)
            {
                lines.Add(i, 0);
            }

            // Перебираем все элементы Figrnd_out
            foreach (Tetromino fig in Figrnd_out)
            {
                incrementYlines(ref lines, fig.PointsRect1);
                incrementYlines(ref lines, fig.PointsRect2);
                incrementYlines(ref lines, fig.PointsRect3);
                incrementYlines(ref lines, fig.PointsRect4);
            }
            List<int> fullLines = new List<int>();
            // Проверяем, какие строки полностью заполнены
            foreach (var line in lines)
            {
                // Если количество заполненных клеток в строке равно ширине поля, то строка полностью заполнена
                if (line.Value == Nx)
                {
                    // Добавляем номер строки в список полностью заполненных строк
                    fullLines.Add(line.Key*scale*11);
                }
            }

            if (lines[0] != 0)
            {
               
                GameOver();
            }

            int sc = 0;
            foreach (int coordY in fullLines)
            {
                foreach (Tetromino fig in Figrnd_out)
                {
                    DeleteLineRect(fig.PointsRect1, coordY);
                    DeleteLineRect(fig.PointsRect2, coordY);
                    DeleteLineRect(fig.PointsRect3, coordY);
                    DeleteLineRect(fig.PointsRect4, coordY);                   

                }
                sc++;
            }
            if (sc == 1) { Score += 100; }
            else if (sc == 2) { Score += 300; }
            else if (sc == 3) { Score += 500; }
            else if (sc == 4) { Score += 800; }

        }


        public void GameOver()
        {
            InitializeButtons();
            if (GameEnded != null)
            {
                GameEnded(this, EventArgs.Empty);
            }
        }
        private void InitializeButtons()  
        {
            // Создаем кнопку "New Game"
            Button newGameButton = new Button();
            newGameButton.Text = "New Game";
            newGameButton.Size = new Size(100, 30);
            newGameButton.Location = new Point(10, 60);
            newGameButton.Click += (sender, e) => {
                // Вызываем событие для начала новой игры
                OnNewGameClicked(EventArgs.Empty);
            };
            this.Controls.Add(newGameButton);

            // Создаем кнопку "Exit"
            Button exitButton = new Button();
            exitButton.Text = "Exit";
            exitButton.Size = new Size(100, 30);
            exitButton.Location = new Point(newGameButton.Right + 10, 60);
            exitButton.Click += (sender, e) => {
                // Вызываем событие для выхода из приложения
                OnExitClicked(EventArgs.Empty);
            };
            this.Controls.Add(exitButton);
        }

        // Событие для начала новой игры
        public event EventHandler NewGameClicked;
        protected virtual void OnNewGameClicked(EventArgs e)
        {
            NewGameClicked?.Invoke(this, e);
            Figrnd_out = [];
            decimal Ydec = Math.Round((decimal)height / (scale * 11), 0);
            int Y0 = (int)Ydec * (scale * 11);
            //Y0 = 0;
            int stepX = scale * 11;
            for (int j = 0; j <= width / (stepX * 1); j += 4)
            {
                Array.Resize(ref Figrnd_out, Figrnd_out.Length + 1);
                Figrnd_out[Figrnd_out.Length - 1] = new Tetromino(
                [new Point(j * stepX, Y0), new Point(j * stepX + scale * 10, Y0), new Point(j * stepX + scale * 10, Y0 + scale * 10), new Point(j * stepX, Y0 + scale * 10)],
                [new Point((j + 1) * stepX, Y0), new Point((j + 1) * stepX + scale * 10, Y0), new Point((j + 1) * stepX + scale * 10, Y0 + scale * 10), new Point((j + 1) * stepX, Y0 + scale * 10)],
                [new Point((j + 2) * stepX, Y0), new Point((j + 2) * stepX + scale * 10, Y0), new Point((j + 2) * stepX + scale * 10, Y0 + scale * 10), new Point((j + 2) * stepX, Y0 + scale * 10)],
                [new Point((j + 3) * stepX, Y0), new Point((j + 3) * stepX + scale * 10, Y0), new Point((j + 3) * stepX + scale * 10, Y0 + scale * 10), new Point((j + 3) * stepX, Y0 + scale * 10)]
                 );
            }
            Invalidate();
        }

        // Событие для выхода из приложения
        public event EventHandler ExitClicked;
        protected virtual void OnExitClicked(EventArgs e)
        {
            ExitClicked?.Invoke(this, e);
        }
        // Метод для начала новой игры
        private void StartNewGame()
    {
        // Здесь должна быть логика для начала новой игры
    }



    //834; 785
    public void DeleteLineRect(Point[] rect, int coordY) 
        {
            if (coordY == rect.Min(point => point.Y))
            {
                for (int i = 0; i < 4; i++)
                {
                    rect[i].X = 1000000;
                    rect[i].Y = 1000000;
                }
            }
            if (coordY > rect.Min(point => point.Y))
            {
                for (int i = 0; i < 4; i++)
                {                    
                    rect[i].Y = rect[i].Y+scale*11;
                }
            }

        }
        public void incrementYlines(ref Dictionary<int, int> lines, Point[] rect) 
        {
            // Получаем координаты Y точки
            int Ymin = rect.Min(point => point.Y);
            int y = Ymin / (scale * 11);
            if (Ymin < height)
            {
                // Увеличиваем количество заполненных клеток в соответствующей строке
                lines[y]++;
            }
        }


        public int minYfromOutbyX(Tetromino[] Figrnd_out, int X1) 
        {
            // Создаем список для хранения минимальных значений Y для каждого массива точек
            List<int> minYList = new List<int>();

            // Перебираем все элементы Figrnd_out
            foreach (Tetromino fig in Figrnd_out)
            {
                // Находим минимальное значение Y для каждого массива точек в текущем элементе Figrnd_out
                int aYt1 = fig.PointsRect1.Where(point => point.X == X1).Select(point => point.Y).DefaultIfEmpty(height).Min();
                int aYt2 = fig.PointsRect2.Where(point => point.X == X1).Select(point => point.Y).DefaultIfEmpty(height).Min();
                int aYt3 = fig.PointsRect3.Where(point => point.X == X1).Select(point => point.Y).DefaultIfEmpty(height).Min();
                int aYt4 = fig.PointsRect4.Where(point => point.X == X1).Select(point => point.Y).DefaultIfEmpty(height).Min();

                // Находим минимальное значение Y среди найденных значений
                int minY = new int[] { aYt1, aYt2, aYt3, aYt4 }.Min();

                // Добавляем найденное минимальное значение Y в список
                minYList.Add(minY);
            }

            // Находим минимальное значение Y среди всех найденных значений
            return minYList.Min();

        }

        public bool Moving(Tetromino Figrnd, Tetromino[] Figrnd_out)
        {
            int X1 = Figrnd.PointsRect1.Min(point => point.X);
            int Y1 = Figrnd.PointsRect1.Max(point => point.Y);
            int Y1min = minYfromOutbyX(Figrnd_out, X1);

            int X2 = Figrnd.PointsRect2.Min(point => point.X);
            int Y2 = Figrnd.PointsRect2.Max(point => point.Y);
            int Y2min = minYfromOutbyX(Figrnd_out, X2);

            int X3 = Figrnd.PointsRect3.Min(point => point.X);
            int Y3 = Figrnd.PointsRect3.Max(point => point.Y);
            int Y3min = minYfromOutbyX(Figrnd_out, X3);

            int X4 = Figrnd.PointsRect4.Min(point => point.X);
            int Y4 = Figrnd.PointsRect4.Max(point => point.Y);
            int Y4min = minYfromOutbyX(Figrnd_out, X4);
            if (
                Y1 + 1 < Y1min &&
                Y2 + 1 < Y2min &&
                Y3 + 1 < Y3min &&
                Y4 + 1 < Y4min 
               )
            { return true; }
            return false;
        }

        public int maxFixY(Tetromino Figrnd, int h) 
        {
            int maxY = int.MinValue; // Инициализируем переменную для хранения максимального значения Y

            // Перебираем все массивы точек в объекте Tetromino
            foreach (Point[] pointsRect in new Point[][] { Figrnd.PointsRect1, Figrnd.PointsRect2, Figrnd.PointsRect3, Figrnd.PointsRect4 })
            {
                // Перебираем все точки в текущем массиве точек
                foreach (Point point in pointsRect)
                {
                    // Обновляем значение maxY, если значение Y текущей точки больше текущего максимального значения maxY
                    if (point.Y > maxY)
                    {
                        maxY = point.Y;
                    }
                }
            }
            // maxY = 702;
            maxY = maxY - 10 * scale;
            int delta = h;
           
            for(int i = 0; i <= h/(11*scale); i++) 
            {
                int d = maxY - i * 11 * scale ;
                if (Math.Abs(d) < Math.Abs(delta)) {  delta = d; }
            }
            return delta ;
        }

        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            if (keyData == Keys.Up || keyData == Keys.W)
            {
                int mod_len = Figrnd_group.Length;
                if (current_mod < mod_len - 1)
                {
                    current_mod++;
                }
                else if (current_mod >= mod_len - 1)
                {
                    current_mod = 0;
                }
               
                Figrnd = Figrnd_group[current_mod];
                Invalidate();

                return true; // Возврат true указывает, что клавиша обработана
            }

            if (keyData == Keys.Down || keyData == Keys.S)
            {
                int mod_len = Figrnd_group.Length;
                if (current_mod > 0)
                {
                    current_mod--;
                }
                else if(current_mod <= 0)
                {
                    current_mod = mod_len - 1;
                }

                Figrnd = Figrnd_group[current_mod];
                Invalidate();

                return true; // Возврат true указывает, что клавиша обработана
            }
            if (keyData == Keys.Enter || keyData == Keys.Space)
            {
                speed_moveY = 10;
                Invalidate();

                return true; // Возврат true указывает, что клавиша обработана
            }



            if (keyData == Keys.Left || keyData == Keys.A)
            {
                
                    foreach (Tetromino t in Figrnd_group)
                    {
                        // Смещаем каждую точку прямоугольника на эту разницу

                        if (
                            t.PointsRect1.Min(point => point.X) - 11*scale >= 0 &&
                            t.PointsRect2.Min(point => point.X) - 11 * scale >= 0 &&
                            t.PointsRect3.Min(point => point.X) - 11 * scale >= 0 &&
                            t.PointsRect4.Min(point => point.X) - 11 * scale >= 0
                            )
                        {
                            for (int i = 0; i < t.PointsRect1.Length; i++)
                            {
                                t.PointsRect1[i] = new Point(t.PointsRect1[i].X - 11 * scale, t.PointsRect1[i].Y);
                                t.PointsRect2[i] = new Point(t.PointsRect2[i].X - 11 * scale, t.PointsRect2[i].Y);
                                t.PointsRect3[i] = new Point(t.PointsRect3[i].X - 11 * scale, t.PointsRect3[i].Y);
                                t.PointsRect4[i] = new Point(t.PointsRect4[i].X - 11 * scale, t.PointsRect4[i].Y);
                            }
                        }

                    }
               

                Invalidate();

                return true; // Возврат true указывает, что клавиша обработана
            }


            if (keyData == Keys.Right || keyData == Keys.D)
            {
                foreach (Tetromino t in Figrnd_group)
                {
                    // Смещаем каждую точку прямоугольника на эту разницу

                    if (
                            t.PointsRect1.Max(point => point.X) + 11 * scale < width &&
                            t.PointsRect2.Max(point => point.X) + 11 * scale < width &&
                            t.PointsRect3.Max(point => point.X) + 11 * scale < width &&
                            t.PointsRect4.Max(point => point.X) + 11 * scale < width
                            )
                        {
                            for (int i = 0; i < t.PointsRect1.Length; i++)
                            {
                                t.PointsRect1[i] = new Point(t.PointsRect1[i].X + 11 * scale, t.PointsRect1[i].Y);
                                t.PointsRect2[i] = new Point(t.PointsRect2[i].X + 11 * scale, t.PointsRect2[i].Y);
                                t.PointsRect3[i] = new Point(t.PointsRect3[i].X + 11 * scale, t.PointsRect3[i].Y);
                                t.PointsRect4[i] = new Point(t.PointsRect4[i].X + 11 * scale, t.PointsRect4[i].Y);
                            }
                        }

                    }
                

                Invalidate();

                return true; // Возврат true указывает, что клавиша обработана
            }




            // Если клавиша не та, передаем управление родительскому элементу управления
            return base.ProcessCmdKey(ref msg, keyData);
        }
    }   
    public class Tetromino
    {
        public Point[] PointsRect1 { get; private set; }
        public Point[] PointsRect2 { get; private set; }
        public Point[] PointsRect3 { get; private set; }
        public Point[] PointsRect4 { get; private set; }
        public Tetromino(Point[] pointsRect1, Point[] pointsRect2, Point[] pointsRect3, Point[] pointsRect4)
        {
            PointsRect1 = pointsRect1;
            PointsRect2 = pointsRect2;
            PointsRect3 = pointsRect3;
            PointsRect4 = pointsRect4;
        }

    }

    public class TetrominoFactory
    {
        public static Tetromino CreateFig1()
        {
            Point p1 = new Point(0, 0);
            Point p2 = new Point(10, 0);
            Point p3 = new Point(10, 10);
            Point p4 = new Point(0, 10);

            Point p5 = new Point(11, 0);
            Point p6 = new Point(21, 0);
            Point p7 = new Point(21, 10);
            Point p8 = new Point(11, 10);

            Point p9 = new Point(22, 0);
            Point p10 = new Point(32, 0);
            Point p11 = new Point(32, 10);
            Point p12 = new Point(22, 10);

            Point p13 = new Point(33, 0);
            Point p14 = new Point(43, 0);
            Point p15 = new Point(43, 10);
            Point p16 = new Point(33, 10);

            return new Tetromino(new Point[] { p1, p2, p3, p4 }, new Point[] { p5, p6, p7, p8 }, new Point[] { p9, p10, p11, p12 }, new Point[] { p13, p14, p15, p16 });
        }

        public static Tetromino CreateFig2()
        {
            Point p1 = new Point(0, 0);
            Point p2 = new Point(10, 0);
            Point p3 = new Point(10, 10);
            Point p4 = new Point(0, 10);

            Point p5 = new Point(11, 0);
            Point p6 = new Point(21, 0);
            Point p7 = new Point(21, 10);
            Point p8 = new Point(11, 10);

            Point p9 = new Point(22, 0);
            Point p10 = new Point(32, 0);
            Point p11 = new Point(32, 10);
            Point p12 = new Point(22, 10);

            Point p13 = new Point(22, 11);
            Point p14 = new Point(32, 11);
            Point p15 = new Point(32, 21);
            Point p16 = new Point(22, 21);

            return new Tetromino(new Point[] { p1, p2, p3, p4 }, new Point[] { p5, p6, p7, p8 }, new Point[] { p9, p10, p11, p12 }, new Point[] { p13, p14, p15, p16 });
        }

        public static Tetromino CreateFig3()
        {
            Point p1 = new Point(0, 0);
            Point p2 = new Point(10, 0);
            Point p3 = new Point(10, 10);
            Point p4 = new Point(0, 10);

            Point p5 = new Point(11, 0);
            Point p6 = new Point(21, 0);
            Point p7 = new Point(21, 10);
            Point p8 = new Point(11, 10);

            Point p9 = new Point(22, 0);
            Point p10 = new Point(32, 0);
            Point p11 = new Point(32, 10);
            Point p12 = new Point(22, 10);

            Point p13 = new Point(0, 11);
            Point p14 = new Point(10, 11);
            Point p15 = new Point(10, 21);
            Point p16 = new Point(0, 21);

            return new Tetromino(new Point[] { p1, p2, p3, p4 }, new Point[] { p5, p6, p7, p8 }, new Point[] { p9, p10, p11, p12 }, new Point[] { p13, p14, p15, p16 });
        }
        public static Tetromino CreateFig4()
        {
            Point p1 = new Point(0, 0);
            Point p2 = new Point(10, 0);
            Point p3 = new Point(10, 10);
            Point p4 = new Point(0, 10);

            Point p5 = new Point(11, 0);
            Point p6 = new Point(21, 0);
            Point p7 = new Point(21, 10);
            Point p8 = new Point(11, 10);

            Point p9 = new Point(22, 0);
            Point p10 = new Point(32, 0);
            Point p11 = new Point(32, 10);
            Point p12 = new Point(22, 10);

            Point p13 = new Point(11, 11);
            Point p14 = new Point(21, 11);
            Point p15 = new Point(21, 21);
            Point p16 = new Point(11, 21);

            return new Tetromino(new Point[] { p1, p2, p3, p4 }, new Point[] { p5, p6, p7, p8 }, new Point[] { p9, p10, p11, p12 }, new Point[] { p13, p14, p15, p16 });
        }
        public static Tetromino CreateFig5()
        {
            Point p1 = new Point(0, 0);
            Point p2 = new Point(10, 0);
            Point p3 = new Point(10, 10);
            Point p4 = new Point(0, 10);

            Point p5 = new Point(11, 0);
            Point p6 = new Point(21, 0);
            Point p7 = new Point(21, 10);
            Point p8 = new Point(11, 10);

            Point p9 = new Point(0, 11);
            Point p10 = new Point(10, 11);
            Point p11 = new Point(10, 21);
            Point p12 = new Point(0, 21);

            Point p13 = new Point(11, 11);
            Point p14 = new Point(21, 11);
            Point p15 = new Point(21, 21);
            Point p16 = new Point(11, 21);

            return new Tetromino(new Point[] { p1, p2, p3, p4 }, new Point[] { p5, p6, p7, p8 }, new Point[] { p9, p10, p11, p12 }, new Point[] { p13, p14, p15, p16 });
        }
        public static Tetromino CreateFig6()
        {
            Point p1 = new Point(0, 0);
            Point p2 = new Point(10, 0);
            Point p3 = new Point(10, 10);
            Point p4 = new Point(0, 10);

            Point p5 = new Point(11, 0);
            Point p6 = new Point(21, 0);
            Point p7 = new Point(21, 10);
            Point p8 = new Point(11, 10);

            Point p9 = new Point(11, 11);
            Point p10 = new Point(21, 11);
            Point p11 = new Point(21, 21);
            Point p12 = new Point(11, 21);

            Point p13 = new Point(22, 11);
            Point p14 = new Point(32, 11);
            Point p15 = new Point(32, 21);
            Point p16 = new Point(22, 21);

            return new Tetromino(new Point[] { p1, p2, p3, p4 }, new Point[] { p5, p6, p7, p8 }, new Point[] { p9, p10, p11, p12 }, new Point[] { p13, p14, p15, p16 });
        }
        public static Tetromino CreateFig7()
        {
            Point p1 = new Point(11, 0);
            Point p2 = new Point(21, 0);
            Point p3 = new Point(21, 10);
            Point p4 = new Point(11, 10);

            Point p5 = new Point(22, 0);
            Point p6 = new Point(32, 0);
            Point p7 = new Point(32, 10);
            Point p8 = new Point(22, 10);

            Point p9 = new Point(0, 11);
            Point p10 = new Point(10, 11);
            Point p11 = new Point(10, 21);
            Point p12 = new Point(0, 21);

            Point p13 = new Point(11, 11);
            Point p14 = new Point(21, 11);
            Point p15 = new Point(21, 21);
            Point p16 = new Point(11, 21);

            return new Tetromino(new Point[] { p1, p2, p3, p4 }, new Point[] { p5, p6, p7, p8 }, new Point[] { p9, p10, p11, p12 }, new Point[] { p13, p14, p15, p16 });
        }

        public static Tetromino CreateFig1_1()
        {
            Point p1 = new Point(0, 0);
            Point p2 = new Point(10, 0);
            Point p3 = new Point(10, 10);
            Point p4 = new Point(0, 10);

            Point p5 = new Point(0, 11);
            Point p6 = new Point(10, 11);
            Point p7 = new Point(10, 21);
            Point p8 = new Point(0, 21);

            Point p9 = new Point(0, 22);
            Point p10 = new Point(10, 22);
            Point p11 = new Point(10, 32);
            Point p12 = new Point(0, 32);

            Point p13 = new Point(0, 33);
            Point p14 = new Point(10, 33);
            Point p15 = new Point(10, 43);
            Point p16 = new Point(0, 43);

            return new Tetromino(new Point[] { p1, p2, p3, p4 }, new Point[] { p5, p6, p7, p8 }, new Point[] { p9, p10, p11, p12 }, new Point[] { p13, p14, p15, p16 });
        }

        public static Tetromino CreateFig2_1()
        {
            Point p1 = new Point(11, 0);
            Point p2 = new Point(21, 0);
            Point p3 = new Point(21, 10);
            Point p4 = new Point(11, 10);

            Point p5 = new Point(11, 11);
            Point p6 = new Point(21, 11);
            Point p7 = new Point(21, 21);
            Point p8 = new Point(11, 21);

            Point p9 = new Point(11, 22);
            Point p10 = new Point(21, 22);
            Point p11 = new Point(21, 32);
            Point p12 = new Point(11, 32);

            Point p13 = new Point(0, 22);
            Point p14 = new Point(10, 22);
            Point p15 = new Point(10, 32);
            Point p16 = new Point(0, 32);

            return new Tetromino(new Point[] { p1, p2, p3, p4 }, new Point[] { p5, p6, p7, p8 }, new Point[] { p9, p10, p11, p12 }, new Point[] { p13, p14, p15, p16 });
        }


        public static Tetromino CreateFig2_2()
        {
            Point p1 = new Point(0, 11);
            Point p2 = new Point(10, 11);
            Point p3 = new Point(10, 21);
            Point p4 = new Point(0, 21);

            Point p5 = new Point(11, 11);
            Point p6 = new Point(21, 11);
            Point p7 = new Point(21, 21);
            Point p8 = new Point(11, 21);

            Point p9 = new Point(22, 11);
            Point p10 = new Point(32, 11);
            Point p11 = new Point(32, 21);
            Point p12 = new Point(22, 21);

            Point p13 = new Point(0, 0);
            Point p14 = new Point(10, 0);
            Point p15 = new Point(10, 10);
            Point p16 = new Point(0, 10);

            return new Tetromino(new Point[] { p1, p2, p3, p4 }, new Point[] { p5, p6, p7, p8 }, new Point[] { p9, p10, p11, p12 }, new Point[] { p13, p14, p15, p16 });
        }

        public static Tetromino CreateFig2_3()
        {
            Point p1 = new Point(0, 0);
            Point p2 = new Point(10, 0);
            Point p3 = new Point(10, 10);
            Point p4 = new Point(0, 10);

            Point p5 = new Point(0, 11);
            Point p6 = new Point(10, 11);
            Point p7 = new Point(10, 21);
            Point p8 = new Point(0, 21);

            Point p9 = new Point(0, 22);
            Point p10 = new Point(10, 22);
            Point p11 = new Point(10, 32);
            Point p12 = new Point(0, 32);

            Point p13 = new Point(11, 0);
            Point p14 = new Point(21, 0);
            Point p15 = new Point(21, 10);
            Point p16 = new Point(11, 10);

            return new Tetromino(new Point[] { p1, p2, p3, p4 }, new Point[] { p5, p6, p7, p8 }, new Point[] { p9, p10, p11, p12 }, new Point[] { p13, p14, p15, p16 });
        }
        public static Tetromino CreateFig3_1()
        {

            Point p1 = new Point(11, 0);
            Point p2 = new Point(21, 0);
            Point p3 = new Point(21, 10);
            Point p4 = new Point(11, 10);

            Point p5 = new Point(11, 11);
            Point p6 = new Point(21, 11);
            Point p7 = new Point(21, 21);
            Point p8 = new Point(11, 21);

            Point p9 = new Point(11, 22);
            Point p10 = new Point(21, 22);
            Point p11 = new Point(21, 32);
            Point p12 = new Point(11, 32);

            Point p13 = new Point(0, 0);
            Point p14 = new Point(10, 0);
            Point p15 = new Point(10, 10);
            Point p16 = new Point(0, 10);

            return new Tetromino(new Point[] { p1, p2, p3, p4 }, new Point[] { p5, p6, p7, p8 }, new Point[] { p9, p10, p11, p12 }, new Point[] { p13, p14, p15, p16 });
        }

        public static Tetromino CreateFig3_2()
        {

            Point p1 = new Point(0, 11);
            Point p2 = new Point(10, 11);
            Point p3 = new Point(10, 21);
            Point p4 = new Point(0, 21);

            Point p5 = new Point(11, 11);
            Point p6 = new Point(21, 11);
            Point p7 = new Point(21, 21);
            Point p8 = new Point(11, 21);

            Point p9 = new Point(22, 11);
            Point p10 = new Point(32, 11);
            Point p11 = new Point(32, 21);
            Point p12 = new Point(22, 21);

            Point p13 = new Point(22, 0);
            Point p14 = new Point(32, 0);
            Point p15 = new Point(32, 10);
            Point p16 = new Point(22, 10);

            return new Tetromino(new Point[] { p1, p2, p3, p4 }, new Point[] { p5, p6, p7, p8 }, new Point[] { p9, p10, p11, p12 }, new Point[] { p13, p14, p15, p16 });
        }

        public static Tetromino CreateFig3_3()
        {

            Point p1 = new Point(0, 0);
            Point p2 = new Point(10, 0);
            Point p3 = new Point(10, 10);
            Point p4 = new Point(0, 10);

            Point p5 = new Point(0, 11);
            Point p6 = new Point(10, 11);
            Point p7 = new Point(10, 21);
            Point p8 = new Point(0, 21);

            Point p9 = new Point(0, 22);
            Point p10 = new Point(10, 22);
            Point p11 = new Point(10, 32);
            Point p12 = new Point(0, 32);

            Point p13 = new Point(11, 22);
            Point p14 = new Point(21, 22);
            Point p15 = new Point(21, 32);
            Point p16 = new Point(11, 32);


            return new Tetromino(new Point[] { p1, p2, p3, p4 }, new Point[] { p5, p6, p7, p8 }, new Point[] { p9, p10, p11, p12 }, new Point[] { p13, p14, p15, p16 });
        }

        public static Tetromino CreateFig4_1()
        {
            Point p1 = new Point(11, 0);
            Point p2 = new Point(21, 0);
            Point p3 = new Point(21, 10);
            Point p4 = new Point(11, 10);

            Point p5 = new Point(11, 11);
            Point p6 = new Point(21, 11);
            Point p7 = new Point(21, 21);
            Point p8 = new Point(11, 21);

            Point p9 = new Point(11, 22);
            Point p10 = new Point(21, 22);
            Point p11 = new Point(21, 32);
            Point p12 = new Point(11, 32);

            Point p13 = new Point(0, 11);
            Point p14 = new Point(10, 11);
            Point p15 = new Point(10, 21);
            Point p16 = new Point(0, 21);

            return new Tetromino(new Point[] { p1, p2, p3, p4 }, new Point[] { p5, p6, p7, p8 }, new Point[] { p9, p10, p11, p12 }, new Point[] { p13, p14, p15, p16 });
        }
        public static Tetromino CreateFig4_2()
        {
            Point p1 = new Point(0, 11);
            Point p2 = new Point(10, 11);
            Point p3 = new Point(10, 21);
            Point p4 = new Point(0, 21);

            Point p5 = new Point(11, 11);
            Point p6 = new Point(21, 11);
            Point p7 = new Point(21, 21);
            Point p8 = new Point(11, 21);

            Point p9 = new Point(22, 11);
            Point p10 = new Point(32, 11);
            Point p11 = new Point(32, 21);
            Point p12 = new Point(22, 21);

            Point p13 = new Point(11, 0);
            Point p14 = new Point(21, 0);
            Point p15 = new Point(21, 10);
            Point p16 = new Point(11, 10);


            return new Tetromino(new Point[] { p1, p2, p3, p4 }, new Point[] { p5, p6, p7, p8 }, new Point[] { p9, p10, p11, p12 }, new Point[] { p13, p14, p15, p16 });
        }

        public static Tetromino CreateFig4_3()
        {
            Point p1 = new Point(0, 0);
            Point p2 = new Point(10, 0);
            Point p3 = new Point(10, 10);
            Point p4 = new Point(0, 10);

            Point p5 = new Point(0, 11);
            Point p6 = new Point(10, 11);
            Point p7 = new Point(10, 21);
            Point p8 = new Point(0, 21);

            Point p9 = new Point(0, 22);
            Point p10 = new Point(10, 22);
            Point p11 = new Point(10, 32);
            Point p12 = new Point(0, 32);

            Point p13 = new Point(11, 11);
            Point p14 = new Point(21, 11);
            Point p15 = new Point(21, 21);
            Point p16 = new Point(11, 21);

            return new Tetromino(new Point[] { p1, p2, p3, p4 }, new Point[] { p5, p6, p7, p8 }, new Point[] { p9, p10, p11, p12 }, new Point[] { p13, p14, p15, p16 });
        }

        public static Tetromino CreateFig6_1()
        {

            Point p1 = new Point(11, 0);
            Point p2 = new Point(21, 0);
            Point p3 = new Point(21, 10);
            Point p4 = new Point(11, 10);

            Point p5 = new Point(11, 11);
            Point p6 = new Point(21, 11);
            Point p7 = new Point(21, 21);
            Point p8 = new Point(11, 21);

            Point p9 = new Point(0, 11);
            Point p10 = new Point(10, 11);
            Point p11 = new Point(10, 21);
            Point p12 = new Point(0, 21);

            Point p13 = new Point(0, 22);
            Point p14 = new Point(10, 22);
            Point p15 = new Point(10, 32);
            Point p16 = new Point(0, 32);


            return new Tetromino(new Point[] { p1, p2, p3, p4 }, new Point[] { p5, p6, p7, p8 }, new Point[] { p9, p10, p11, p12 }, new Point[] { p13, p14, p15, p16 });
        }

        public static Tetromino CreateFig7_1()
        {

            Point p1 = new Point(0, 0);
            Point p2 = new Point(10, 0);
            Point p3 = new Point(10, 10);
            Point p4 = new Point(0, 10);

            Point p5 = new Point(11, 11);
            Point p6 = new Point(21, 11);
            Point p7 = new Point(21, 21);
            Point p8 = new Point(11, 21);

            Point p9 = new Point(0, 11);
            Point p10 = new Point(10, 11);
            Point p11 = new Point(10, 21);
            Point p12 = new Point(0, 21);

            Point p13 = new Point(11, 22);
            Point p14 = new Point(21, 22);
            Point p15 = new Point(21, 32);
            Point p16 = new Point(11, 32);

            return new Tetromino(new Point[] { p1, p2, p3, p4 }, new Point[] { p5, p6, p7, p8 }, new Point[] { p9, p10, p11, p12 }, new Point[] { p13, p14, p15, p16 });
        }

    }

}
