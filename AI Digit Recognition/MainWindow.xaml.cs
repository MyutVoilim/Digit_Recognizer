using ScottPlot;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Numerics;
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
        private bool isDrawing = false;
        private AIDigitModel digitAi = new AIDigitModel(new int[3] { 512, 256, 128});
        int x = 3;
        public MainWindow()
        {
            InitializeComponent();
            myCanvas = new CanvasFrame(fakeCanvas, 28, "C:\\Users\\tom10\\source\\repos\\AI Digit Recognition\\AI Digit Recognition\\Data\\train.csv");
            testingPlot();
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
            myCanvas.LoadLine();
            Debug.WriteLine(string.Join(" , ", (myCanvas.getCanvasArray())));
            int[] temp = digitAi.Predict(myCanvas.getCanvasArray());
            int maxValue = 0;
            for (int i = 0; i < 10; i++)
            {
                if (maxValue < temp[i]) maxValue = i;
            }
            Text1.Content = "(" + maxValue + ") " + string.Join(", ", temp);
 
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

        private void onCanvasMouseDown(object sender, MouseButtonEventArgs e)
        {
            isDrawing = true;
            myCanvas.DrawOnGrid((int) e.GetPosition(myCanvas.Canvas).X, (int)e.GetPosition(myCanvas.Canvas).Y);
        }

        private void onCanvasMouseMove(object sender, MouseEventArgs e)
        {
            if (isDrawing) { myCanvas.DrawOnGrid((int)e.GetPosition(myCanvas.Canvas).X, (int)e.GetPosition(myCanvas.Canvas).Y); }
            int[] temp = digitAi.Predict(myCanvas.getCanvasArray());
            int maxValue = 0;
            for (int i = 0; i < 10; i++)
            {
                if (maxValue < temp[i]) maxValue = i;
            }
            Text1.Content = "(" + maxValue + ") " + string.Join(", ", temp);
        }

        private void onCanvasMouseUp(object sender, MouseButtonEventArgs e)
        {
            isDrawing = false;
        }

        private async void Train_AI(object sender, RoutedEventArgs e)
        {
            try
            {
                (sender as Button).IsEnabled = false;

                // Start the training
                await digitAi.Train(.1f, 1);

                // Display a message (or do any post-training tasks)
                MessageBox.Show("Training Completed!");
            }
            catch (Exception ex)
            {
                // Handle exceptions
                MessageBox.Show($"An error occurred: {ex.Message}");
            }
            finally
            {
                // Re-enable the button
                (sender as Button).IsEnabled = true;
            }
        }

        private void Load_AI(object sender, RoutedEventArgs e)
        {
            digitAi.LoadFromFile();
        }

        private void StopTraining(object sender, RoutedEventArgs e)
        {
            x++;
                ConfidenceChart.Refresh();
        }

        private void SaveAI(object sender, RoutedEventArgs e)
        {
            digitAi.SaveToFile();
        }

        private void ClearCanvas(object sender, RoutedEventArgs e)
        {
            myCanvas.ClearCanvas();
        }

        private void testingPlot() { 

            // create sample data
            double[] values = { 26, 20, 23, 7, 16, x};

            ConfidenceChart.Plot.AddBar(values);
            ConfidenceChart.Refresh();
        }

        //Add visuals for ai probablity
        //Beautify
        //Ability for ai to dynamically react to user drawing
        //Progress bar for training
    }
}
