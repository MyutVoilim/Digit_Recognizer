using Microsoft.Win32;
using ScottPlot;
using System;
using System.Collections.Generic;

using System.Diagnostics;
using System.Diagnostics.Metrics;
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
        private IFileManager _fileManager = new FileManager();

        // Allows user to notify that AI training should stop
        private CancellationTokenSource _cancellationTokenSource;

        // Intialize canvas data and AI model
        private CanvasFrame _mainCanvas; 
        private AIDigitModel _digitAiModel;

        // Create learningRate which controls how quickly the AI learns and trainingEpochs how many time training data is processed during training
        private float _learningRate = .01f;
        private int _trainingEpochs = 2;

        // Hold values for creating new AI structure
        private int[] _createAiInput;

        // Diplays confidence rating for each number from 0 - 9
        private int[] _confidenceValues = new int[10];

        // Tracks which line is being displayed in the UI
        private int _currentTrainingLine = 1;

        // Holds the training data in string format
        private string[] _stringTrainingData;

        // Tracks the path of the project folder
        private string _projectFolder = Directory.GetParent(Directory.GetCurrentDirectory()).Parent.Parent.FullName; //Get directory for project

        // File to traing AI model with
        private string _trainingFilePath; 

        // Bool to toggle training and drawing
        private bool _isTraining = false;
        private bool _isDrawing = false; 


        public MainWindow()
        {
            InitializeComponent();
            // Get location of default training file and store data
            _trainingFilePath = System.IO.Path.Combine(_projectFolder, @"Data\train.csv");
            _fileManager.DefinePath(_trainingFilePath);
            StoreTrainingData();

            // Intialize new DigitCanvas and create an AI model
            _mainCanvas = new CanvasFrame(DigitCanvas);
            _digitAiModel = new AIDigitModel(new int[2] {32, 16}, _fileManager);

            // Update UI to show default values
            CreateConfidenceChart();
            ProcessDigit();
            UpdateCurrentUsedValues();
        }

        /// <summary>
        /// Runs digit on canvas through AI and updates confidence values
        /// </summary>
        private void ProcessDigit() {
            // Processes the data on the canvas
            _digitAiModel.ProcessInput(_mainCanvas.GetCanvasArray());

            // Updates confidence values
            _confidenceValues = _digitAiModel.GetOutputValues();

            // Display AI's current best guess
            AiGuessLabel.Content = _digitAiModel.GetGuessedValue();

            // Display new confidence values
            ConfidenceChart.Reset();
            ConfidenceChart.Plot.AddBar(_confidenceValues.Select(i => (double) i).ToArray());
            ConfidenceChart.Refresh();
        }

        /// <summary>
        /// Stores in string array training data from a training file
        /// </summary>
        private void StoreTrainingData()
        {
            _stringTrainingData = _fileManager.ReadAllLines();
        }

        #region Async Method
        /// <summary>
        /// Begins training AI based on a learning rate and epoch count
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void TrainAiAsync()
        {
            try
            {
                if (_isTraining)
                {
                    _cancellationTokenSource?.Cancel();
                    _isTraining = false;
                }
                else
                {
                    _cancellationTokenSource = new CancellationTokenSource();
                    _isTraining = true;

                    // Set progress components to visible
                    ProgressBar.Visibility = Visibility.Visible;
                    ProgressLabel.Visibility = Visibility.Visible;
                    AccuracyLabel.Visibility = Visibility.Visible;
                    AccuracyInfoLabel.Visibility = Visibility.Visible;

                    // Will update UI when as data is processed, runs on UI thread
                    Progress<float[]> progress = new Progress<float[]>(percent =>
                    {
                        ProgressLabel.Content = $"{(int)percent[0]}% Completed";
                        ProgressBar.Value = (int)percent[0];

                        // Only show percentages if epochs is 0, meaning the ai is not training just checking accuracy
                        AccuracyLabel.Content = $"{(int)percent[1]}%";
                    });

                    Train_AI_Button.Content = "Stop Training";

                    // Start the training
                    await _digitAiModel.Train(_learningRate, _trainingEpochs, progress, _cancellationTokenSource.Token, _trainingFilePath);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"An error occurred: {ex.Message}");
            }
            finally
            {
                Train_AI_Button.Content = "Start Training";

                // Remove visiblity of progress components
                ProgressBar.Visibility = Visibility.Collapsed;
                ProgressLabel.Visibility = Visibility.Collapsed;
                ProgressBar.Value = 0;
                ProgressLabel.Content = "0% Completed";

                //accuracyLabel.Visibility = Visibility.Visible;
                _isTraining = false;
            }
        }

        /// <summary>
        /// Creates a new AI model based on user inputs
        /// </summary>
        private async Task CreateAiAsync(int input)
        {
            // Track current layer position to enter input information
            int position = 0;

            // Create createAiInput size if not created yet
            if (_createAiInput == null)
            {
                _createAiInput = new int[input];
                AiCreationInfoTextBox.Text = "Enter node count for each layer. Suggested range 32 - 256. Larger node counts will take longer to train.";
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
            AiInputLabel.Content = $"Node # for layer {position}";

            // If the last value in createAiInput is not 0 then all inputs have been entered and new AI model can be created
            if (_createAiInput[_createAiInput.Length - 1] != 0)
            {
                _digitAiModel = new AIDigitModel(_createAiInput, _fileManager);
                AiInputLabel.Content = "AI Created!";
                _createAiInput = null;
                await Task.Delay(2000);
                AiInputLabel.Content = "Layer Amount:";
                AiCreationInfoTextBox.Text = "Enter a layer amount.Suggested range between 2 - 4";
            }
        }
        #endregion
        #region Display Methods
        /// <summary>
        /// Updates the UI to reflect current used learning rate and epoch count
        /// </summary>
        private void UpdateCurrentUsedValues()
        {
            CurrentValuesInfoLabel.Content = $"Current Epoch: {_trainingEpochs}  Learning Rate: {_learningRate}";
        }

        /// <summary>
        /// Populates the confidence bar chart with labels and data
        /// </summary>
        private void CreateConfidenceChart()
        {
            string[] labels = { "0", "1", "2", "3", "4", "5", "6", "7", "8", "9" };
            double[] positions = { 0, 1, 2, 3, 4, 6, 7, 8, 9, 10 };
            ConfidenceChart.Plot.AddBar(_confidenceValues.Select(i => (double)i).ToArray(), positions);
            ConfidenceChart.Plot.XTicks(positions, labels);
            ConfidenceChart.Plot.SetAxisLimits(yMin: 0);
        }

        /// <summary>
        /// Cycles through training data to display digits from training file onto canvas screen.
        /// </summary>
        private void DisplayTrainingData()
        {
            _mainCanvas.ConvertStringData(_stringTrainingData[_currentTrainingLine]);
            _currentTrainingLine++;
        }
        #endregion

        #region Utility
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
        /// Checks to see if a testbox is not empty or 0
        /// </summary>
        /// <returns></returns>
        private bool CheckValidInput(TextBox checkTextBox)
        {
            if (string.IsNullOrWhiteSpace(checkTextBox.Text))
            {
                return false;
            }
            if (int.Parse(checkTextBox.Text) == 0)
            {
                return false;
            }
            return true;
        }
        #endregion

        /// <summary>
        /// Sets flag that user has started drawing from clicking the canvas
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void onCanvasMouseDown(object sender, MouseButtonEventArgs e)
        {
            _isDrawing = true;
            _mainCanvas.DrawOnGrid((int)e.GetPosition(_mainCanvas.Canvas).X, (int)e.GetPosition(_mainCanvas.Canvas).Y);
            ProcessDigit();
        }

        /// <summary>
        /// Draws on canvas if user has their mouse down and is moving it over the canvas
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void onCanvasMouseMove(object sender, MouseEventArgs e)
        {
            if (_isDrawing)
            {
                _mainCanvas.DrawOnGrid((int)e.GetPosition(_mainCanvas.Canvas).X, (int)e.GetPosition(_mainCanvas.Canvas).Y);
                ProcessDigit();
            }
        }

        /// <summary>
        /// Sets flag that user no longer has their mouse clicked down on digit canvas
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void onCanvasMouseUp(object sender, MouseButtonEventArgs e)
        {
            _isDrawing = false;
        }

        /// <summary>
        /// Loads AI model from a file
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Load_AI(object sender, RoutedEventArgs e)
        {
            _digitAiModel.LoadFromFile();
        }

        /// <summary>
        /// Saves current AI model to a file
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
        /// Gets a number from the training file to display on screen and have AI guess what number it is
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void GetNumber(object sender, RoutedEventArgs e)
        {
            DisplayTrainingData();
            ProcessDigit();

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
        /// Selects a training file to use
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SelectTrainingData(object sender, RoutedEventArgs e)
        {
            _fileManager.SelectFile();
            if (_fileManager.DoesPathExist())
            {
                // Get new training path and reset to new data
                _trainingFilePath = _fileManager.GetPath();
                StoreTrainingData();
                _currentTrainingLine = 1;
            }
        }

        /// <summary>
        /// Checks the accuracy of the current AI model against training data, does not activly train the AI
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CheckAccuracyClick(object sender, RoutedEventArgs e)
        {
            // Epochs of 0 indicates to not train but only process training data
            _trainingEpochs = 0;
            TrainAiAsync();
        }

        /// <summary>
        /// Begin training AI model using current learning rate and epoch count
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Train_AI_Button_Click(object sender, RoutedEventArgs e)
        {
            TrainAiAsync();
        }


        /// <summary>
        /// Save AI model to a file
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SaveMenuClicked(object sender, RoutedEventArgs e)
        {
            _digitAiModel.SaveToFile();
        }

        /// <summary>
        /// Load AI model from a file
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void LoadMenuClicked(object sender, RoutedEventArgs e)
        {
            _digitAiModel.LoadFromFile();
        }

        /// <summary>
        /// Updates the Labels associated with the slider to adjust learning rate
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void LearningRateSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            LearningRateNumberLabel.Content = Math.Round(LearningRateSlider.Value, 3);
        }

        /// <summary>
        /// Updates epoch count and learning rate when enter is hit while changing training values
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ChangeTrainingValuesInputEnter(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                _learningRate = float.Parse(LearningRateNumberLabel.Content.ToString());

                if (CheckValidInput(EpochTextbox)) _trainingEpochs = int.Parse(EpochTextbox.Text);

                UpdateCurrentUsedValues();
            }
        }

        /// <summary>
        /// Updates epoch count and learning rate when user clicks update while changing training values
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ChangeTrainingValuesButtonClick(object sender, RoutedEventArgs e)
        {
            _learningRate = float.Parse(LearningRateNumberLabel.Content.ToString());

            if (CheckValidInput(EpochTextbox)) _trainingEpochs = int.Parse(EpochTextbox.Text);

            UpdateCurrentUsedValues();
        }

        /// <summary>
        /// Calls CreateAI when enter key is hit using intputTextbox
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CreateAiInputEnter(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                if (CheckValidInput(InputTextbox)) CreateAiAsync(int.Parse(InputTextbox.Text));
                InputTextbox.Clear();
            }
        }

        /// <summary>
        /// Calls CreateAi with valid inputs when createButton is clicked
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CreateAiButtonClick(object sender, RoutedEventArgs e)
        {
            if (CheckValidInput(InputTextbox)) CreateAiAsync(int.Parse(InputTextbox.Text));
            InputTextbox.Clear();
        }


        //Make sure the the use of file is  consistent wither using the class or calling the files directly
        //See about configuration files
        //Add documentation
        //Add unit testing
        //Rearage and put regions on all classes
        //Add regions and make sure thing are xml commented and spelled correctly
        //Remove unneccissary using system stuff
        //Ensure things are titled properly where they are meant to be titled
        //Make sure UI is named correctly

        //CODE THAT HAS BEEN CHECKED AS PASSED
        //AIDigitModel, FileManager, IFileManager, CanvasData, IntToGrayscale, CanvasFrame
    }
}
