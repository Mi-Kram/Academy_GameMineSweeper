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
using System.Windows.Shapes;

namespace Main
{
    /// <summary>
    /// Логика взаимодействия для GameResultDialog.xaml
    /// </summary>
    public partial class GameResultDialog : Window
    {
        /// <summary>
        /// результат уровня (выигрышь / проигрышь)
        /// </summary>
        public bool IsWin { get; set; }
        /// <summary>
        /// время
        /// </summary>
        public string Time { get; set; }
        /// <summary>
        /// звук нажатия на кнопку
        /// </summary>
        MediaPlayer clickSound;

        /// <summary>
        /// конструктор по умолчанию
        /// </summary>
        public GameResultDialog()
        {
            InitializeComponent();

            IsWin = false;
            Time = null;
            clickSound = new MediaPlayer();
            clickSound.Open(new Uri(Environment.CurrentDirectory + @"\Resources\click.mp3", UriKind.Absolute));
        }
        /// <summary>
        /// конструктор с параметрами
        /// </summary>
        /// <param name="isWin">true - победа, false - поражение</param>
        /// <param name="time">время</param>
        public GameResultDialog(bool isWin, string time = null)
        {
            InitializeComponent();

            IsWin = isWin;
            Time = time;
        }

        /// <summary>
        /// перемещение окна
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Oben_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            DragMove();
        }

        /// <summary>
        /// при загрузке окна
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Grid_Loaded(object sender, RoutedEventArgs e)
        {
            // если победа - установить заголовок WIN
            if (IsWin) Oben_TextBlock.Text = "WIN";
            // если указано время
            if (Time != null)
            {
                // показать время
                time_TextBlock.Text = "Time: " + Time;
                // скрыть изображение бомбы
                bombImg.Visibility = Visibility.Hidden;
                // отобразить время
                time_Border.Visibility = Visibility.Visible;
            }
        }

        /// <summary>
        /// Нажатие на кнопку OK
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OKButton_Click(object sender, RoutedEventArgs e)
        {
            // проиграть звук нажития
            clickSound.Play();
            // закрыть окно
            Close();
        }
    }
}
