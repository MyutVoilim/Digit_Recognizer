using ScottPlot;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Text.RegularExpressions;
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
        private CanvasFrame myCanvas; // Data for the canvas that also has internal data for data binding !!!!!!!!!!!
        private bool isDrawing = false; // Determine when user is drawing on canvas
        private AIDigitModel digitAi; // Structure for AI
        private int[] confidenceValues = new int[10]; // Values from 0 - 9 holding the confidence values for their respective numbers
        private float learningRate = .1f; // Rate that AI adjusts internal values during training, .1f is a fairly large learning rate
        private int epochAmount = 1; // Amount of times AI iterates through entire training data during training
        public MainWindow()
        {
            var projectFolder = Directory.GetParent(Directory.GetCurrentDirectory()).Parent.Parent.FullName;
            var file = System.IO.Path.Combine(projectFolder, @"Data\train.csv");

            InitializeComponent();
            myCanvas = new CanvasFrame(fakeCanvas, 28, System.IO.Path.Combine(projectFolder, @"Data\train.csv"));
            //digitAi = new AIDigitModel(System.IO.Path.Combine(projectFolder, @"Data\BasicAIData.txt"));
            digitAi = new AIDigitModel(new int[2] { 16, 20});
            CreateConfidenceChart();
            UpdateChart();
        }

        /// <summary>
        /// Gets a number from training cvs file
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void GetNumber(object sender, RoutedEventArgs e)
        {
            myCanvas.LoadLine();
            UpdateChart();
 
        }

        /// <summary>
        /// Sets flag that user has started drawing from clicking the canvas
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void onCanvasMouseDown(object sender, MouseButtonEventArgs e)
        {
            isDrawing = true;
            myCanvas.DrawOnGrid((int) e.GetPosition(myCanvas.Canvas).X, (int)e.GetPosition(myCanvas.Canvas).Y);
        }

        /// <summary>
        /// Draws on canvas is user has their mouse down and is moving it over the canvas
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void onCanvasMouseMove(object sender, MouseEventArgs e)
        {
            if (isDrawing) { myCanvas.DrawOnGrid((int)e.GetPosition(myCanvas.Canvas).X, (int)e.GetPosition(myCanvas.Canvas).Y); }
            UpdateChart();
        }

        /// <summary>
        /// Sets flag that user no longer has their mouse clicked down
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void onCanvasMouseUp(object sender, MouseButtonEventArgs e)
        {
            isDrawing = false;
        }

        /// <summary>
        /// Loads AI connections and weights from selected file
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Load_AI(object sender, RoutedEventArgs e)
        {
            digitAi.LoadFromFile();
        }

        /// <summary>
        /// Saves current AI connections and weights to a selected file
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SaveAI(object sender, RoutedEventArgs e)
        {
            digitAi.SaveToFile();
        }

        /// <summary>
        /// Clears the canvas back to black
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ClearCanvas(object sender, RoutedEventArgs e)
        {
            myCanvas.ClearCanvas();
        }

        /// <summary>
        /// Updates the chart to reflect new cofidence values and finds value with highest confidence
        /// </summary>
        private void UpdateChart() {
            confidenceValues = digitAi.Predict(myCanvas.GetCanvasArray());
            int maxValue = 0;

            for (int i = 0; i < 10; i++)
            {
                if (maxValue < confidenceValues[i]) maxValue = i;
            }
            Text1.Content = maxValue;

            ConfidenceChart.Reset();
            ConfidenceChart.Plot.AddBar(confidenceValues.Select(i => (double) i).ToArray());
            ConfidenceChart.Refresh();
        }

        /// <summary>
        /// Populates the bar chart with labels and data
        /// </summary>
        private void CreateConfidenceChart()
        {
            string[] labels = { "0", "1","2","3","4","5","6","7","8","9"};
            double[] positions = {0, 1, 2, 3, 4, 6, 7, 8, 9, 10};
            ConfidenceChart.Plot.AddBar(confidenceValues.Select(i => (double)i).ToArray(), positions);
            ConfidenceChart.Plot.XTicks(positions, labels);
            ConfidenceChart.Plot.SetAxisLimits(yMin: 0);

        }

        /// <summary>
        /// Begins training AI based on a learning rate and epoch
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void TrainAI(object sender, RoutedEventArgs e)
        {
            try
            {
                (sender as Button).IsEnabled = false;

                // Start the training
                await digitAi.Train(learningRate, epochAmount);

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
        
        /// <summary>
        /// Checks to see if InputTextbox is using only numbers
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void TextboxRestrictions(object sender, TextCompositionEventArgs e)
        {
            if (!IsNumeric(e.Text))
            {
                e.Handled = true;
            }
        }

        /// <summary>
        /// Checks to see if a string can convert to int
        /// </summary>
        /// <param name="text"></param>
        /// <returns></returns>
        private bool IsNumeric(string text)
        {
            return int.TryParse(text, out _);
        }
        private void createButtonClick(object sender, RoutedEventArgs e)
        {
            CreateAi();
        }

        private void CreateAi()
        {
            aiInputLabel.Content = "This worked";
        }

        private void inputEnter(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter) {
                CreateAi();
            }
        }

        //Ensure proper z-index
        //Clean up code
        //Visually make it look good
        //Ensure that the logic for different classes interaction makes sense and is consistent eg. whether canvasDAata should be in the main rather than intialized in canvas frame
        //Make sure the the use of file is  consistent wither using the class or calling the files directly
        //Make so that a default AI file is loaded from the project
    }
}
