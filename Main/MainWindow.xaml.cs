using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;
using System.Media;

namespace Main
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        /// <summary>
        /// кол-во секунд при прохождении уровня
        /// </summary>
        long seconds;
        /// <summary>
        /// размер шрифта цифр, обозначающие кол-во бомб по-близости
        /// </summary>
        double numFontSize;
        /// <summary>
        /// Random
        /// </summary>
        Random rand;
        /// <summary>
        /// схема уровня (отображаются клетки: пустые '0', цифры '1-8', бомбы 'B')
        /// </summary>
        char[,] map;
        /// <summary>
        /// матрица прямоугольников
        /// </summary>
        Rectangle[,] rects;
        /// <summary>
        /// матрица флагов
        /// </summary>
        Rectangle[,] flagsMap;
        /// <summary>
        /// макс. кол-во флагов
        /// </summary>
        int flagsCount;
        /// <summary>
        /// макс. возможное кол-во открытых полей (не считая поля-бомбы)
        /// </summary>
        int maxOpenField;
        /// <summary>
        /// текущее кол-во открытых полей
        /// </summary>
        int currentOpenField;
        /// <summary>
        /// таймер уровня
        /// </summary>
        DispatcherTimer playTimer;
        /// <summary>
        /// звук нажатия на кнопку
        /// </summary>
        MediaPlayer clickSound;
        /// <summary>
        /// звук открытия поля
        /// </summary>
        MediaPlayer openField;
        /// <summary>
        /// звук взрыва
        /// </summary>
        MediaPlayer explosion;
        /// <summary>
        /// фоновая музыка
        /// </summary>
        MediaPlayer backgroundMusik;

        /// <summary>
        /// конструктор по умолчанию
        /// </summary>
        public MainWindow()
        {
            InitializeComponent();

            map = new char[0, 0];
            rects = new Rectangle[0, 0];
            flagsMap = new Rectangle[0, 0];
            rand = new Random();
            flagsCount = 0;
            numFontSize = GetNumFontSize();
            maxOpenField = currentOpenField = 0;
            playTimer = new DispatcherTimer();
            playTimer.Tick += PlayTimer_Tick;
            playTimer.Interval = TimeSpan.FromSeconds(1);
            seconds = 0;
            clickSound = new MediaPlayer();
            openField = new MediaPlayer();
            explosion = new MediaPlayer();
            backgroundMusik = new MediaPlayer();
            backgroundMusik.Volume = 1;
            backgroundMusik.MediaEnded += BackgroundMusik_MediaEnded;

            openField.Open(new Uri(Environment.CurrentDirectory + @"\Resources\fieldOpen.mp3", UriKind.Absolute));
            explosion.Open(new Uri(Environment.CurrentDirectory + @"\Resources\explosion.mp3", UriKind.Absolute));
            backgroundMusik.Open(new Uri(Environment.CurrentDirectory + @"\Resources\BackGroundMusik.wav", UriKind.Absolute));
            clickSound.Open(new Uri(Environment.CurrentDirectory + @"\Resources\click.mp3", UriKind.Absolute));
            backgroundMusik.Play();

            // сгенерировать уровень
            GenerateNewLVL();
        }

        /// <summary>
        /// при окончании фоновой музыки
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BackgroundMusik_MediaEnded(object sender, EventArgs e)
        {
            // сбросить время
            backgroundMusik.Position = TimeSpan.FromMilliseconds(0);
            // запустить музыку
            backgroundMusik.Play();
        }

        /// <summary>
        /// получить размер шрифта для цифр, отображающих кол-во бомб по-близости
        /// </summary>
        /// <returns></returns>
        double GetNumFontSize()
        {
            // получить наименьшую сторону
            double min = Math.Min(MainGrid.ActualHeight - 60, MainGrid.ActualWidth);

            // вернуть размер шрифта относительно размера поля
            switch (Size_ComboBox.Text)
            {
                case "10x10": return min / 11;
                case "15x15": return min / 17;
                case "20x20": return min / 23;
                case "25x25": return min / 29;
                case "30x30": return min / 35;
                default: return 10;
            }
        }
        /// <summary>
        /// при срабатывании таймера
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void PlayTimer_Tick(object sender, EventArgs e)
        {
            // отобразить прошедшее время
            timer_TextBlock.Text = GetTime(++seconds);
        }
        /// <summary>
        /// получить время в формате 00:00:00
        /// </summary>
        /// <param name="totalSec">кол-во секунд</param>
        /// <returns></returns>
        string GetTime(long totalSec)
        {
            // кол-во секунд, минут, часов
            long sec = 0, min = 0, hours = 0;

            // получить секунды
            sec = totalSec % 60;
            // получить минуты
            min = (totalSec / 60) % 60;
            // получить часы
            hours = (totalSec / 60) / 60;

            return $"{(hours.ToString().Length == 1 ? "0" : "")}{hours}:{(min.ToString().Length == 1 ? "0" : "")}{min}:{(sec.ToString().Length == 1 ? "0" : "")}{sec}";
        }
        /// <summary>
        /// передвижение окна
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Oben_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            DragMove();
        }
        /// <summary>
        /// закрытие окна
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CloseWindow(object sender, MouseButtonEventArgs e)
        {
            Close();
        }
        /// <summary>
        /// максимизировать и "нормализировать" окно
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MaxWindow(object sender, MouseButtonEventArgs e)
        {
            if (this.WindowState == WindowState.Maximized) this.WindowState = WindowState.Normal;
            else this.WindowState = WindowState.Maximized;
        }
        /// <summary>
        /// скрыть окно
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void HideWindow(object sender, MouseButtonEventArgs e)
        {
            this.WindowState = WindowState.Minimized;
        }
        /// <summary>
        /// ввод текста в окно для кол-во бомб
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BombCount_TextBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            // запретить ввод в случаях:
            // 1. длина текста в поле равна 3
            // 2. вводимое значение не является числом
            if (BombCount_TextBox.Text.Length == 3 || 
                !Char.IsDigit(e.Text, 0)) e.Handled = true;
        }
        /// <summary>
        /// ввод текста в окно для кол-во бомб
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BombCount_TextBox_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            // запретить ввод пробела
            if (e.Key == Key.Space) e.Handled = true;
            // если нажат Enter - сгенерировать новый уровень
            if (e.Key == Key.Enter) GenerateNewLVL();
        }
        /// <summary>
        /// начать новый уровень
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void StartNewPlay_MouseDown(object sender, MouseButtonEventArgs e)
        {
            // проиграть звук нажатия
            PlaySound(clickSound);
            // остановить таймер текущей игры
            playTimer.Stop();

            // сгенерировать новый уровень
            GenerateNewLVL();
        }

        /// <summary>
        /// Генерация нового уровня
        /// </summary>
        void GenerateNewLVL()
        {
            // сбросить "подсчётные" данные
            seconds = 0;
            timer_TextBlock.Text = "00:00:00";
            currentOpenField = 0;
            Size_ComboBox.Focus();
            numFontSize = GetNumFontSize();
            playBoard.Children.Clear();
            playBoard.RowDefinitions.Clear();
            playBoard.ColumnDefinitions.Clear();

            // если кол-во бомб указано неверно - указать 10
            if (BombCount_TextBox.Text.Replace("0", "").Length == 0) BombCount_TextBox.Text = "10";

            // определение размера поля
            int[] size = Size_ComboBox.Text.Split(new char[] { 'x', }, StringSplitOptions.RemoveEmptyEntries).Select(s => int.Parse(s)).ToArray();
            if (int.Parse(BombCount_TextBox.Text) >= size[0] * size[1] / 2)
                BombCount_TextBox.Text = (size[0] * size[1] / 2).ToString();

            // кол-во бомб
            FlagsCount_TextBlock.Text = BombCount_TextBox.Text;
            int bombs = flagsCount = int.Parse(FlagsCount_TextBlock.Text);

            // макс. кол-во открываемых полей
            maxOpenField = size[0] * size[1] - bombs;

            // сгенерировать схемы

            // заполнить все клетки пустым полем '0'
            map = new char[size[0], size[1]];
            for (int i = 0; i < size[0]; i++) for (int j = 0; j < size[1]; j++) map[i, j] = '0';

            // схемы расположения элементов игрового поля
            rects = new Rectangle[size[0], size[1]];
            for (int i = 0; i < size[0]; i++) for (int j = 0; j < size[1]; j++) rects[i, j] = new Rectangle();
            flagsMap = new Rectangle[size[0], size[1]];
            for (int i = 0; i < size[0]; i++) for (int j = 0; j < size[1]; j++) flagsMap[i, j] = null;

            // кол-во полей
            int n = size[0] * size[1];
            // список индексов бомб
            List<int> bombsLst = new List<int>();
            // сгенерировать бомбы
            for (int i = 0; i < bombs; i++)
            {
                // получить индекс для бомбы
                int bombIndex = rand.Next(n);

                // если индекс уже в списке уменьшить i на 1
                if (bombsLst.Contains(bombIndex)) i--;
                // иначе добавить индекс в список
                else bombsLst.Add(bombIndex);
            }

            // добавление бомб в схему
            foreach (int bombIndex in bombsLst)
            {
                AddBomb((int)(bombIndex / size[0]), (int)(bombIndex % size[1]), size[0], size[1]);
            }

            // разметить игровое поле
            for (int r = 0; r < size[0]; r++)
                playBoard.RowDefinitions.Add(new RowDefinition());
            for (int c = 0; c < size[1]; c++)
                playBoard.ColumnDefinitions.Add(new ColumnDefinition());

            // цвет тёмного и светлого поля
            SolidColorBrush darkField = (SolidColorBrush)TryFindResource("DarkCloseField");
            SolidColorBrush lightField = (SolidColorBrush)TryFindResource("LightCloseField");

            // если цвета не найдены
            if (darkField == null) darkField = Brushes.Green;
            if (lightField == null) lightField = Brushes.LightGreen;

            // отобразить поля на уровня
            for (int i = 0; i < size[0]; i++)
            {
                for (int j = 0; j < size[1]; j++)
                {
                    Rectangle field = rects[i, j];
                    field.Margin = new Thickness(0);
                    field.StrokeThickness = 0;
                    field.Fill = (i * size[0] + j + (size[0] % 10 == 0 ? i : 0)) % 2 == 0 ? darkField : lightField;
                    // поле не открыто
                    field.Tag = false;

                    Grid.SetRow(field, i);
                    Grid.SetColumn(field, j);
                    playBoard.Children.Add(field);

                    field.MouseLeftButtonDown += Field_MouseLeftButtonDown;
                    field.MouseRightButtonDown += Field_MouseRightButtonDown;
                }
            }
        }
        /// <summary>
        /// ПКМ по полю (поставть флаг)
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Field_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            // поставить флаг если доступное ко-во флагов больше 0
            // и есть открытые поля
            if (flagsCount > 0 && currentOpenField > 0)
            {
                // проиграть звук нажатия по полю
                PlaySound(openField);
                // поле, по которому щёлкнули ПКМ
                Rectangle field = sender as Rectangle;

                // флаг
                Rectangle flag = new Rectangle();
                flag.Fill = new ImageBrush(new BitmapImage(new Uri(Environment.CurrentDirectory + @"\Resources\flag.png", UriKind.Absolute)));
                flag.MouseLeftButtonDown += Flag_MouseLeftButtonDown;
                flag.MouseRightButtonDown += Flag_MouseRightButtonDown;

                // получить сторку и столбец поля
                int row = Grid.GetRow(field), col = Grid.GetColumn(field);
                // установить строку и столбец для флага
                Grid.SetRow(flag, row);
                Grid.SetColumn(flag, col);
                // добавить флаг
                playBoard.Children.Add(flag);

                // записать флаг в схеме
                flagsMap[row, col] = flag;
                // уменьшить доступное кол-во флагов
                FlagsCount_TextBlock.Text = (--flagsCount).ToString();
            }
        }
        /// <summary>
        /// ПКМ по флагу (убрать флаг)
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Flag_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            // проиграть звук нажатия по полю
            PlaySound(openField);
            // получить флаг, по которому нажали
            Rectangle flag = sender as Rectangle;
            // убрать флаг из схемы
            flagsMap[Grid.GetRow(flag), Grid.GetColumn(flag)] = null;
            // убрать флаг с игрового поля
            playBoard.Children.Remove(flag);
            // увуличить кол-во доступных флагов
            FlagsCount_TextBlock.Text = (++flagsCount).ToString();
            e.Handled = true;
        }
        /// <summary>
        /// ЛКМ по флагу
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Flag_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            e.Handled = true;
        }
        /// <summary>
        /// ЛКМ по полю (открыть поле)
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Field_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            // поле, по которому щёлкнули
            Rectangle field = sender as Rectangle;
            // строка и столбец поля
            int row = Grid.GetRow(field), col = Grid.GetColumn(field);

            // генерировать новый уровкнь, пока первая открытая клетка - бомба
            while (currentOpenField == 0 && map[row, col] == 'B') GenerateNewLVL();

            // если открыта бомба
            if (map[row, col] == 'B')
            {
                // проиграть звук взрыва
                PlaySound(explosion);
                // игра окончена проигрышем
                GameOver(false);
            }
            // иначе
            else
            {
                // проиграть звук открытия поля
                PlaySound(openField);
                // запустить таймер
                playTimer.Start();
                // если результат открытия поля равен true - завершить игру выигрышем
                if (OpenField(row, col)) GameOver(true);
            }
        }
        /// <summary>
        /// Добавить бомбу в схему
        /// </summary>
        /// <param name="row">индекс строки</param>
        /// <param name="col">индекс столбца</param>
        /// <param name="maxRow">макс. кол-во строк</param>
        /// <param name="maxCol">макс. кол-во столбцов</param>
        void AddBomb(int row, int col, int maxRow, int maxCol)
        {
            // обозначить бомбу
            map[row, col] = 'B';

            // изменить поля вокруг текущей бомбы:
            // если поле рядом не бомба и не выходит за границы
            // увеличить значение на 1

            if (row - 1 >= 0 && col - 1 >= 0 && map[row - 1, col - 1] != 'B')
                map[row - 1, col - 1] = Convert.ToChar(Convert.ToInt32(map[row - 1, col - 1]) + 1);

            if (row - 1 >= 0 && map[row - 1, col] != 'B')
                map[row - 1, col] = Convert.ToChar(Convert.ToInt32(map[row - 1, col]) + 1);

            if (row - 1 >= 0 && col + 1 < maxCol && map[row - 1, col + 1] != 'B')
                map[row - 1, col + 1] = Convert.ToChar(Convert.ToInt32(map[row - 1, col + 1]) + 1);

            if (col + 1 < maxCol && map[row, col + 1] != 'B')
                map[row, col + 1] = Convert.ToChar(Convert.ToInt32(map[row, col + 1]) + 1);

            if (row + 1 < maxRow && col + 1 < maxCol && map[row + 1, col + 1] != 'B')
                map[row + 1, col + 1] = Convert.ToChar(Convert.ToInt32(map[row + 1, col + 1]) + 1);

            if (row + 1 < maxRow && map[row + 1, col] != 'B')
                map[row + 1, col] = Convert.ToChar(Convert.ToInt32(map[row + 1, col]) + 1);

            if (row + 1 < maxRow && col - 1 >= 0 && map[row + 1, col - 1] != 'B')
                map[row + 1, col - 1] = Convert.ToChar(Convert.ToInt32(map[row + 1, col - 1]) + 1);

            if (col - 1 >= 0 && map[row, col - 1] != 'B')
                map[row, col - 1] = Convert.ToChar(Convert.ToInt32(map[row, col - 1]) + 1);
        }
        /// <summary>
        /// Изменение размера окна
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MainGrid_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            // минимальный размер сторон
            double tmp = Math.Min(MainGrid.ActualHeight - 60, MainGrid.ActualWidth);
            // игровое поле всегда квадрат
            boardBorder.Height = tmp;
            boardBorder.Width = tmp;
            board_StackPanel.Orientation = tmp == MainGrid.ActualWidth ? Orientation.Horizontal : Orientation.Vertical;

            // получить размер шрифта для цифр, отображающих кол-во бомб по-близости
            numFontSize = GetNumFontSize();
            // изменить размер шрифта
            foreach (FrameworkElement item in playBoard.Children)
            {
                if (item is TextBlock)
                {
                    (item as TextBlock).FontSize = numFontSize;
                }
            }
        }
        /// <summary>
        /// Рекурсивное открытие полей
        /// </summary>
        /// <param name="row">индекс строки</param>
        /// <param name="col">индекс столбца</param>
        /// <returns></returns>
        bool OpenField(int row, int col)
        {
            // текущий прямоугольник 
            Rectangle field = rects[row, col];

            // если поле не открыто
            if (!(bool)field.Tag)
            {
                // если на поле нет флага
                if (flagsMap[row, col] == null)
                {
                    // поле открыто
                    field.Tag = true;
                    field.Fill = (SolidColorBrush)TryFindResource((row * map.GetLength(0) + col + (map.GetLength(0) % 10 == 0 ? row : 0)) % 2 == 0 ? "DarkOpenField" : "LightOpenField");
                    field.MouseLeftButtonDown -= Field_MouseLeftButtonDown;
                    field.MouseRightButtonDown -= Field_MouseRightButtonDown;
                    // увеличить кол-во текущих открытых полей
                    ++currentOpenField;
                }
                // получить число со схемы
                int dig = int.Parse(map[row, col].ToString());

                // если число равно 0 - значит поле пустое
                if (dig == 0)
                {
                    // открывать следующие поля, если значения не выходят за границы
                    // если OpenField вернул true - вернуть true

                    if (row - 1 >= 0) if (OpenField(row - 1, col)) return true;
                    if (row - 1 >= 0 && col + 1 < map.GetLength(1)) if (OpenField(row - 1, col + 1)) return true;
                    if (col + 1 < map.GetLength(1)) if (OpenField(row, col + 1)) return true;
                    if (row + 1 < map.GetLength(0) && col + 1 < map.GetLength(1)) if (OpenField(row + 1, col + 1)) return true;
                    if (row + 1 < map.GetLength(0)) if (OpenField(row + 1, col)) return true;
                    if (row + 1 < map.GetLength(0) && col - 1 >= 0) if (OpenField(row + 1, col - 1)) return true;
                    if (col - 1 >= 0) if (OpenField(row, col - 1)) return true;
                    if (row - 1 >= 0 && col - 1 >= 0) if (OpenField(row - 1, col - 1)) return true;
                }
                // если число не 0 и флага на поле нет - отобразить число
                else if (flagsMap[row, col] == null)
                {
                    // число
                    TextBlock num = new TextBlock();
                    num.Margin = new Thickness(0);
                    num.VerticalAlignment = VerticalAlignment.Center;
                    num.HorizontalAlignment = HorizontalAlignment.Center;
                    // текст числа
                    num.Text = dig.ToString();
                    num.Foreground = GetNumColor(dig);
                    num.TextAlignment = TextAlignment.Center;
                    num.FontSize = numFontSize;
                    // определение положения числа и добавление на поле
                    Grid.SetRow(num, row);
                    Grid.SetColumn(num, col);
                    playBoard.Children.Add(num);
                }

                // если текущие отурытые поля равны максимальномк значению - вернуть true
                if (currentOpenField >= maxOpenField) return true;
            }

            return false;
        }
        /// <summary>
        /// Получить цвет по цифре
        /// </summary>
        /// <param name="num">цифра</param>
        /// <returns></returns>
        Brush GetNumColor(int num)
        {
            // вернуть цвет по цифре num
            switch (num)
            {
                case 1: return Brushes.Blue;
                case 2: return Brushes.Green;
                case 3: return Brushes.Red;
                case 4: return Brushes.DarkViolet;
                case 5: return Brushes.Orange;
                case 6: return Brushes.Aqua;
                case 7: return Brushes.Black;
                case 8: return Brushes.Gray;

                default: return Brushes.White;
            }
        }
        /// <summary>
        /// Уровень закончен
        /// </summary>
        /// <param name="result">true - выигрышь, false - проигрышь</param>
        void GameOver(bool result)
        {
            // остановить таймер уровня
            playTimer.Stop();

            // создать диалоговое икно
            GameResultDialog resultDialog = new GameResultDialog();
            resultDialog.Owner = this;
            // результат уровня (true - выигрышь, false - проигрышь)
            if (result)
            {
                // указать выигрышь
                resultDialog.IsWin = true;
                // указать время
                resultDialog.Time = timer_TextBlock.Text;
            }
            else
            {
                // показать места бомб
                ShowBombs();
                // указать проигрышь
                resultDialog.IsWin = false;
            }
            // показать результирующее окно
            resultDialog.ShowDialog();

            // сгенерировать новый уровень
            GenerateNewLVL();
        }
        /// <summary>
        /// Показать все бомбы
        /// </summary>
        void ShowBombs()
        {
            //// цвета полей
            //SolidColorBrush darkField = (SolidColorBrush)TryFindResource("DarkOpenField");
            //SolidColorBrush lightField = (SolidColorBrush)TryFindResource("LightOpenField");

            // убрать все флаги
            foreach (Rectangle flag in flagsMap) playBoard.Children.Remove(flag);

            // картинка бомбы
            ImageBrush bombImg = new ImageBrush(new BitmapImage(new Uri(Environment.CurrentDirectory + @"\Resources\bomb.png", UriKind.Absolute)));
            bombImg.Stretch = Stretch.Uniform;
            // отобразить бомбы
            for (int i = 0; i < map.GetLength(0); i++)
            {
                for (int j = 0; j < map.GetLength(1); j++)
                {
                    // если текущие индексы указывают на бомбу
                    if (map[i, j] == 'B')
                    {
                        // бомба
                        Rectangle bomb = new Rectangle();
                        bomb.Fill = bombImg;
                        bomb.Margin = new Thickness(0);

                        // установить положение бомбы и добавить
                        Grid.SetRow(bomb, i);
                        Grid.SetColumn(bomb, j);
                        playBoard.Children.Add(bomb);
                    }
                }
            }
        }
        /// <summary>
        /// проиграть мелодию
        /// </summary>
        /// <param name="player">мелодия</param>
        void PlaySound(MediaPlayer player)
        {
            // проиграть мелодию
            player.Play();
            // сбросить время
            player.Position = TimeSpan.FromMilliseconds(0);
        }
        /// <summary>
        /// Выделение текста в поле для кол-во бомб
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BombCount_TextBox_SelectionChanged(object sender, RoutedEventArgs e)
        {
            // если длина выделенного текста не равна 0,
            // то установить зачение 0
            if (BombCount_TextBox.SelectionLength != 0) BombCount_TextBox.SelectionLength = 0;
        }
    }
}
