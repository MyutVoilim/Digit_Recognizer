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
        private CanvasFrame _mainCanvas; // Data for the canvas that also has internal data for data binding
        private bool _isDrawing = false; // Determine when user is drawing on canvas
        private AIDigitModel _digitAiModel; // Structure for AI
        private int[] _confidenceValues = new int[10]; // Values from 0 - 9 holding the confidence values for their respective numbers
        private float _learningRate = .1f; // Rate that AI adjusts internal values during training, .1f is a fairly large learning rate
        private int _trainingEpochs = 5; // Amount of times AI iterates through entire training data during training
        private string _projectFolder = Directory.GetParent(Directory.GetCurrentDirectory()).Parent.Parent.FullName; //Get directory for project
        private string _trainingFilePath; //File to traing AI model with
        private int[] _createAiInput;
        private IFileManager _fileManager = new FileManager();

        public MainWindow()
        {
            InitializeComponent();
            _trainingFilePath = System.IO.Path.Combine(_projectFolder, @"Data\train.csv");
            _mainCanvas = new CanvasFrame(digitCanvas, _trainingFilePath);
            //_digitAiModel = new AIDigitModel(System.IO.Path.Combine(_projectFolder, @"Data\AiDigitModel.txt"), _fileManager);
            _digitAiModel = new AIDigitModel(new int[3] { 516, 256, 128 }, _fileManager);
            CreateConfidenceChart();
            UpdateChart();
        }

        /// <summary>
        /// Loads number onto canvas from CSV file
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void GetNumber(object sender, RoutedEventArgs e)
        {
            _mainCanvas.LoadLine();
            UpdateChart();
 
        }

        /// <summary>
        /// Sets flag that user has started drawing from clicking the canvas
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void onCanvasMouseDown(object sender, MouseButtonEventArgs e)
        {
            _isDrawing = true;
            _mainCanvas.DrawOnGrid((int) e.GetPosition(_mainCanvas.Canvas).X, (int)e.GetPosition(_mainCanvas.Canvas).Y);
            UpdateChart();
        }

        /// <summary>
        /// Draws on canvas is user has their mouse down and is moving it over the canvas
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void onCanvasMouseMove(object sender, MouseEventArgs e)
        {
            if (_isDrawing) { 
                _mainCanvas.DrawOnGrid((int)e.GetPosition(_mainCanvas.Canvas).X, (int)e.GetPosition(_mainCanvas.Canvas).Y);
                UpdateChart();
            }
        }

        /// <summary>
        /// Sets flag that user no longer has their mouse clicked down
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void onCanvasMouseUp(object sender, MouseButtonEventArgs e)
        {
            _isDrawing = false;
        }

        /// <summary>
        /// Loads AI connections and weights from selected file
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Load_AI(object sender, RoutedEventArgs e)
        {
            _digitAiModel.LoadFromFile();
        }

        /// <summary>
        /// Saves current AI connections and weights to a selected file
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SaveAI(object sender, RoutedEventArgs e)
        {
            _digitAiModel.SaveToFile();
        }

        /// <summary>
        /// Clears the canvas back to black
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ClearCanvas(object sender, RoutedEventArgs e)
        {
            _mainCanvas.ClearCanvas();
        }

        /// <summary>
        /// Updates the chart to reflect new cofidence values and finds value with highest confidence
        /// </summary>
        private void UpdateChart() {
            _digitAiModel.ProcessInput(_mainCanvas.GetCanvasArray());
            _confidenceValues = _digitAiModel.GetOutputValues();

            aiGuessLabel.Content = _digitAiModel.GetGuessedValue();

            ConfidenceChart.Reset();
            ConfidenceChart.Plot.AddBar(_confidenceValues.Select(i => (double) i).ToArray());
            ConfidenceChart.Refresh();
        }

        /// <summary>
        /// Populates the bar chart with labels and data
        /// </summary>
        private void CreateConfidenceChart()
        {
            string[] labels = { "0", "1","2","3","4","5","6","7","8","9"};
            double[] positions = {0, 1, 2, 3, 4, 6, 7, 8, 9, 10};
            ConfidenceChart.Plot.AddBar(_confidenceValues.Select(i => (double)i).ToArray(), positions);
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
            // Set progress components to visible
            progressBar.Visibility = Visibility.Visible;
            progressLabel.Visibility= Visibility.Visible;

            // Will update UI when as data is processed, runs on UI thread
            var progress = new Progress<float[]>(percent =>
            {
                progressLabel.Content = $"{(int)percent[0]}% completed";
                progressBar.Value = (int)percent[0];

                // Only show percentages if epochs is 0, meaning the ai is not training just checking accuracy
                if (_trainingEpochs == 0) accuracyLabel.Content = (int)percent[1];
            });

            try
            {
                (sender as Button).IsEnabled = false;

                // Start the training
                trainingLabel.Content = "Training...";
                await _digitAiModel.Train(_learningRate, _trainingEpochs,progress, _trainingFilePath);
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

                // Remove visiblity of progress components
                progressBar.Visibility = Visibility.Collapsed;
                progressLabel.Visibility = Visibility.Collapsed;
                progressBar.Value = 0;

                if (_trainingEpochs == 0) accuracyLabel.Visibility = Visibility.Visible;
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

        /// <summary>
        /// Calls CreateAi with valid inputs when createButton is clicked
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
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
            if (_createAiInput == null)
            {
                _createAiInput = new int[input];
                aiInputLabel.Content = "Size for layer";
            } 
            // Find if layer node size has not been defined and set it as input
            else
            {
                for (int i = 0; i < _createAiInput.Length; i++)
                {
                    position++;
                    if (_createAiInput[i] == 0)
                    {
                        _createAiInput[i] = input;
                        break;
                    }
                }
            }
            aiInputLabel.Content = $"Node # for layer {position}";

            // If the last value in createAiInput is not 0 then all inputs have been entered and new AI model can be created
            if (_createAiInput[_createAiInput.Length - 1] != 0)
            {
                _digitAiModel = new AIDigitModel(_createAiInput, _fileManager);
                aiInputLabel.Content = "Ai Created!";
                _createAiInput = null;
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
            if (result.HasValue && result.Value) _trainingFilePath = openFileDialog.FileName;
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

        /// <summary>
        /// Tests current AI model against new test data
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void TestAi(object sender, RoutedEventArgs e)
        {
            _digitAiModel.TestData(System.IO.Path.Combine(_projectFolder, @"Data\testingOutput.csv"), System.IO.Path.Combine(_projectFolder, @"Data\test.csv"));
        }

        /// <summary>
        /// Updates the epoch count based on user input
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ChangeTrainingValues(object sender, RoutedEventArgs e)
        {
            _trainingEpochs = int.Parse(epochTextbox.Text);
        }


        //Clean up code
        //Make sure the the use of file is  consistent wither using the class or calling the files directly
        //See about configuration files
        //Add documentation
        //Add unit testing
        //Rearage and put regions on all classes
        //Add regions and make sure thing are xml commented and spelled correctly
        //Add trainging data that actually has the answers
        //Add ability to stop training
        //make IFileManager
        //Remove unneccissary using system stuff
        //Ensure things are titled properly where they are meant to be titled
        //Make sure UI is named correctly
        //potentially run ai until certain accuracy is reached

        //!!!!!!!!!!IMPORTANT!!!!!!!!!!!!!!!!
        //make user filemanager is consistent through all classes

        //CODE THAT HAS BEEN CHECKED AS PASSED
        //AIDigitModel, FileManager, IFileManager, CanvasData, IntToGrayscale
    }
}
