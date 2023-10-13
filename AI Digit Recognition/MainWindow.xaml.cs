using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
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

namespace AI_Digit_Recognition
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private CanvasFrame myCanvas;
        private CanvasData myCanvasData = new CanvasData();
        private Canvas testingCanvas = new Canvas();
        private Random rnd = new Random();
        public MainWindow()
        {
            InitializeComponent();
            myCanvas = new CanvasFrame(fakeCanvas);
            //testing();
            //createTimer();
        }

        public void createTimer()
        {
            DispatcherTimer timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromSeconds(1);
            timer.Tick += timer_tick;
            timer.Start();

        }

        void timer_tick(object sender, EventArgs e)
        {
           //This is where the canvas would be visually updated
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            ColorGrid();
            myCanvas.changeValue(100);
            Text1.Content = "hit";
        }
        private void ColorGrid()
        {
            //myCanvas.CreateChild(rnd.Next(0, myCanvas.GetCanvasSize()), rnd.Next(0, myCanvas.GetCanvasSize()));
            
        }
        private void AddCanvas()
        {
            //DigitGrid.Children.Add(myCanvas.GetCanvas());
        }
        private void testing()
        {
            //testingCanvas.Background = Brushes.Green;
            //testingCanvas.Width = 280;
            //testingCanvas.Height = 280;
            //DigitGrid.Children.Add(testingCanvas);
            //fakeCanvas = myCanvas.GetCanvas();
        }

        private void fakeCanvas_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            testing();
        }
    }
}
