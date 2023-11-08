using Microsoft.Win32;
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
using System.Windows.Media.Media3D;
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
        private string projectFolder = Directory.GetParent(Directory.GetCurrentDirectory()).Parent.Parent.FullName;
        private string trainingFile;
        private int[] createAiInput;
        public MainWindow()
        {
            trainingFile = System.IO.Path.Combine(projectFolder, @"Data\train.csv");
            InitializeComponent();
            myCanvas = new CanvasFrame(fakeCanvas, 28, trainingFile);
            digitAi = new AIDigitModel(System.IO.Path.Combine(projectFolder, @"Data\AiDigitModel.txt"));
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
                trainingLabel.Content = "Training...";
                await digitAi.Train(learningRate, epochAmount, trainingFile);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"An error occurred: {ex.Message}");
            }
            finally
            {
                // Re-enable the button
                (sender as Button).IsEnabled = true;
                trainingLabel.Content = "Taining is Done!";
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

        // Calls CreateAi when createAiButton is clicked
        private void createButtonClick(object sender, RoutedEventArgs e)
        {
            if (checkValidInput()) CreateAi(int.Parse(InputTextbox.Text));
            InputTextbox.Clear();
        }

        /// <summary>
        /// Creates a new AI model based on user inputs
        /// </summary>
        private void CreateAi(int input)
        {
            // Track current layer position to enter input information
            int position = 0;

            // Create createAiInput size if not created yet
            if (createAiInput == null)
            {
                createAiInput = new int[input];
                aiInputLabel.Content = "Size for layer";
            } 
            // Find if layer node size has not been defined and set it as input
            else
            {
                for (int i = 0; i < createAiInput.Length; i++)
                {
                    position++;
                    if (createAiInput[i] == 0)
                    {
                        createAiInput[i] = input;
                        break;
                    }
                }
            }
            aiInputLabel.Content = $"Node # for layer {position}";

            // If the last value in createAiInput is not 0 then all inputs have been entered and new AI model can be created
            if (createAiInput[createAiInput.Length - 1] != 0)
            {
                digitAi = new AIDigitModel(createAiInput);
                aiInputLabel.Content = "Ai Created!";
                createAiInput = null;
            }
        }

        /// <summary>
        /// Calls CreateAI when enter key is hit using intputTextbox
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void inputEnter(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter) {
                if (checkValidInput()) CreateAi(int.Parse(InputTextbox.Text));
                InputTextbox.Clear();
            }
        }

        /// <summary>
        /// Selects a training file to use
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SelectTrainingData(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "CSV Files (*.csv)|*.csv|Text Files (*.txt)|*.txt|All Files (*.*)|*.*";
            openFileDialog.Title = "Select a Training File";

            bool? result = openFileDialog.ShowDialog();
            if (result.HasValue && result.Value) trainingFile = openFileDialog.FileName;
        }

        /// <summary>
        /// Checks to see if InputTextBox is not empty or 0
        /// </summary>
        /// <returns></returns>
        private bool checkValidInput()
        {
            bool validInput = true;
            if (string.IsNullOrWhiteSpace(InputTextbox.Text)) {
                validInput = false;
            }
            if (int.Parse(InputTextbox.Text) == 0)
            {
                validInput = false;
            }
            return validInput;
        }

        //Ensure proper z-index
        //Clean up code
        //Visually make it look good
        //Make sure the the use of file is  consistent wither using the class or calling the files directly

    }
}
