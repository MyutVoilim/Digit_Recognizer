using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Shapes;

namespace AI_Digit_Recognition
{
    /// <summary>
    /// This creates a feedforward neural network that is training to recognize digits drawn onto a 28 x 28 canvas. The inputs are the
    /// values of each cell on the canvas for a total of 728 inputs ranging from values of 0 to not active and 255 for active. These values then go thorugh various
    /// hidden layers and eventually to a output layer whose value corrispond to the number 0 - 9. This particular model allows for the user to define how many layers
    /// hidden layers should exist and the node count for each of those layers. The input is normalized to put values in a effective range for the sigmoid activation
    /// function and passed through multiplying the weights of each node the the entire node layer of the right layer along with applying a bias to those values.
    /// Values are corrected through a defined learning rate which will effect the degree of change in the system and a backpropigation function will correct errors
    /// in the system to slowly improve the ability of the network.
    /// </summary>
    internal class AIDigitModel
    {
        private Random rand = new Random(); // Used in initialization of weights
        private IFileManager _fileManager;// Manage read from and wrtie to files

        private const int INPUT_SIZE = 784;  
        private const int OUTPUT_SIZE = 10;
        private const float MAX_INPUT_VALUE = 255.0f;

        private float[] _inputLayer = new float[INPUT_SIZE]; // Size of the input layer all cells on canvas grid
        private float[] _outputLayer = new float[OUTPUT_SIZE]; // Size of the output layer, digits 0 - 9
        private float[][] _hiddenLayers; // Each layer between the input and output layers
        private float[][][] _weights;  // Weights between nodes from a node on the left connecting to all nodes to the right
        private float[][] _bias; // Bias values for each node in hidden layers, excludes the input and output nodes

        /// <summary>
        /// Initializes fresh Ai model with the specified hidden layer sizes and populate with random weight data.
        /// </summary>
        /// <param name="layerSizes">Dimensions of the hidden layers.</param>
        public AIDigitModel(int[] layerSizes, IFileManager fileManager)
        {
            _fileManager = fileManager;

            //Initalize dimensions of hidden, bias, and weight layers
            InitializeHiddenAndBiasLayers(layerSizes);
            InitializeWeights(layerSizes);

            // Populate weights with random data
            InitializeRandomWeightData();
            InitializeInputData();
        }

        /// <summary>
        /// Load existing AI model from file
        /// </summary>
        /// <param name="aiModelFile">File containing AI model</param>
        public AIDigitModel(string aiModelFile, IFileManager fileManager)
        {
            _fileManager = fileManager;
            LoadFromFile(aiModelFile);
        }

        #region Public Methods
        /// <summary>
        /// Saves AI model data to a file
        /// </summary>
        public void SaveToFile()
        {
            // Get hidden layer dimensions used in loading
            int[] hiddenLayerData = new int[_hiddenLayers.Length];
            for (int i = 0; i < _hiddenLayers.Length; i++)
            {
                hiddenLayerData[i] = _hiddenLayers[i].Length;
            }

            // Allow user to select a new save path
            _fileManager.SelectSaveFile();

            if (_fileManager.DoesPathExist())
            {
                try
                {
                    // Clear the file to fresh data
                    _fileManager.ClearFile();

                    // Save hidden layer dimensions
                    _fileManager.WriteToFile(hiddenLayerData);

                    // Save input layer
                    _fileManager.WriteToFile(_inputLayer);

                    // Save output layer
                    _fileManager.WriteToFile(_outputLayer);

                    // Save hidden layers
                    foreach (float[] layer in _hiddenLayers)
                    {
                        _fileManager.WriteToFile(layer);
                    }

                    // Save biases
                    foreach (float[] layerBias in _bias)
                    {
                        _fileManager.WriteToFile(layerBias);
                    }

                    // Save weights
                    foreach (float[][] layer in _weights)
                    {
                        foreach (float[] nodeWeights in layer)
                        {
                            _fileManager.WriteToFile(nodeWeights);
                        }
                    }
                }
                catch (Exception err)
                {
                    MessageBox.Show($"An error occurred while saving the file: {err.Message}", "Save Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        /// <summary>
        /// Loads AI model data to a file
        /// </summary>
        /// <param name="aiModeFileData">File to load data to</param>
        public void LoadFromFile(string? aiModeFileData = null)
        {
            // Select a file if no file given
            if (aiModeFileData == null) _fileManager.SelectFile();
            else _fileManager.DefinePath(aiModeFileData);

            if (_fileManager.DoesPathExist())
            {
                try
                {
                    // hold string data from AI model file
                    string[] modelData = _fileManager.ReadAllLines();

                    // Keeps track of current line being read from file, starts at 3 since first three lines will always be static
                    int currentLine = 3;

                    // Load hidden layer amount
                    int[] hiddenLayerArray = modelData[0].Split(",").Select(int.Parse).ToArray();

                    // Load input layer
                    _inputLayer = modelData[1].Split(",").Select(float.Parse).ToArray();

                    // Load output layer
                    _outputLayer = modelData[2].Split(",").Select(float.Parse).ToArray();

                    // Initalize hidden layers and biases with new dimensions
                    InitializeHiddenAndBiasLayers(hiddenLayerArray);

                    // Load hidden layers
                    for (int i = 0; i < _hiddenLayers.Length; i++)
                    {
                        _hiddenLayers[i] = modelData[currentLine].Split(",").Select(float.Parse).ToArray();
                        currentLine++;
                    }

                    // Load biases
                    for (int i = 0; i < _hiddenLayers.Length; i++)
                    {
                        _bias[i] = modelData[currentLine].Split(",").Select(float.Parse).ToArray();
                        currentLine++;
                    }

                    // Initalize weights with new dimensions
                    InitializeWeights(hiddenLayerArray);

                    // Load weights
                    for (int i = 0; i < _hiddenLayers.Length + 1; i++)
                    {
                        for (int j = 0; j < _weights[i].Length; j++)
                        {
                            _weights[i][j] = modelData[currentLine].Split(",").Select(float.Parse).ToArray();
                            currentLine++;
                        }
                    }
                }
                catch (FormatException err)
                {
                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        MessageBox.Show($"Data format error: {err.Message}", "Format Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    });
                }
                catch (Exception err)
                {
                    Application.Current.Dispatcher.Invoke(new Action(() =>
                    {
                        MessageBox.Show($"An error occurred while loading the file: {err.Message}", "Load Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    }));
                }
            }
        }

        /// <summary>
        /// Proccesses input through AI model and changes the output layer
        /// </summary>
        /// <param name="inputLayerData">Input data to be processed<param>
        public void ProcessInput(float[] inputLayerData)
        {
            if (inputLayerData.Length == INPUT_SIZE)
            {
                // Move data into input layer and normalize it
                InitializeInputData(inputLayerData);
                // Proccess data through neural network
                ForwardPass();
            } else
            {
                Debug.WriteLine($"Input data was not in the correct length: {inputLayerData.Length}");
            }
        }

        /// <summary>
        /// Gets the output layer as precentages up to 100 for each number
        /// </summary>
        /// <returns>Index correlates to repective number it represents e.g. index 0 = number 0</returns>
        public int[] GetOutputValues()
        {
            int[] percentages = new int[OUTPUT_SIZE];
            for (int i = 0; i < OUTPUT_SIZE; i++)
            {
                percentages[i] = (int)(_outputLayer[i] * 100);
            }
            return percentages;
        }

        /// <summary>
        /// Train AI model using training data with learning based on learning rate and epoch count
        /// </summary>
        /// <param name="learningRate">Magnitude model corrects for errors</param>
        /// <param name="epochs">Number of iterations through entire data set</param>
        /// <param name="progress">IProgress to ping back through UI thread and update UI</param>
        /// <param name="trainingFile">File containing training data</param>
        /// <returns></returns>
        public async Task Train(float learningRate, int epochs, IProgress<float[]> progress, string? trainingFile = null)
        {
            await Task.Run(async () =>
            {
                // Keep track of progress through training
                float lineCount = 0;
                float totalLines = 0;

                // formatted as {lineCount, correctGuessCount}
                float[] progressAndAccuracy = new float[2];

                // The correct number the model is trying to guess
                float[][] inputData;
                int[] targetNumbers;


                // Load training data from file if given one or let user select a file
                if (trainingFile != null) _fileManager.DefinePath(trainingFile);
                else _fileManager.SelectFile();

                // Begin training
                if (_fileManager.DoesPathExist())
                {
                    try
                    {
                        // Load input data
                        string[] lines = _fileManager.ReadAllLines();

                        // Initiate one less then lines to skip label data
                        inputData = new float[lines.Length - 1][];
                        targetNumbers = new int[lines.Length - 1];

                        // Parse string data and get target number
                        for (int i = 0; i < lines.Length - 1; i++)
                        {
                            inputData[i] = lines[i + 1].Split(",").Select(float.Parse).ToArray();
                            targetNumbers[i] = (int)inputData[i][0];
                        }

                        // Calculate how many lines need to be processed, if epoch is 0 then only pass through once
                        totalLines = lines.Length * ((epochs == 0) ? 1 : epochs);

                        // Begin training through entire data set unless epochs is 0, then only process data not train
                        for (int epoch = epochs == 0 ? -1 : 0; epoch < epochs; epoch++)
                        {
                            // Skip the first line as it contain label information
                            for (int line = 1; line < inputData.Length; line++)
                            {
                                // Process data through model
                                ProcessInput(inputData[line].Skip(1).ToArray());

                                // Backpropagate to correct for errors, if epoch is 0 then only process data
                                if (epochs > 0)
                                {
                                    // Correct for errors
                                    await BackpropagateAsync(targetNumbers[line], learningRate);
                                }
                                else
                                {
                                    // Check if model calculated the correct number
                                    if (GetGuessedValue() == targetNumbers[line]) progressAndAccuracy[1]++;
                                }


                                // Iterate progress count and periodically update UI
                                lineCount++;
                                if (lineCount % 2000 == 0)
                                {
                                    progressAndAccuracy[0] = (lineCount / totalLines) * 100;
                                    progress?.Report(progressAndAccuracy);
                                }
                            }

                            // Finish updating UI
                            progressAndAccuracy[1] = (progressAndAccuracy[1] / totalLines) * 100;
                            progress?.Report(progressAndAccuracy);
                        }
                    }
                    catch (FormatException err)
                    {
                        Application.Current.Dispatcher.Invoke(() =>
                        {
                            MessageBox.Show($"Data format error: {err.Message}", "Format Error", MessageBoxButton.OK, MessageBoxImage.Error);
                        });
                    }
                    catch (Exception err)
                    {
                        Application.Current.Dispatcher.Invoke(() =>
                        {
                            MessageBox.Show($"An error occurred: {err.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                        });
                    }

                }
                else
                {
                    // No file selected
                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        MessageBox.Show("No file selected for training.", "File Required", MessageBoxButton.OK, MessageBoxImage.Warning);
                    });
                }
            });
        }


        /// <summary>
        /// Uses a test file to check how accurate AI model is to fresh data and writes answers to a file
        /// </summary>
        /// <param name="writeToFile">File to write answers to</param>
        /// <param name="readFromFile">File to read test data from</param>
        public void TestData(string writeToFile = null, string readFromFile = null)
        {
            // Set file path to test data or select a file with test data
            if (readFromFile != null) _fileManager.DefinePath(readFromFile);
            else _fileManager.SelectFile();

            if (_fileManager.DoesPathExist())
            {
                try
                {
                    // Extract input data from file
                    string[] inputData = _fileManager.ReadAllLines();

                    // Set save path to writeToFile or select a file to write to
                    if (writeToFile != null) _fileManager.DefinePath(writeToFile);
                    else _fileManager.SelectSaveFile();

                    // Clear file to fresh data
                    _fileManager.ClearFile();

                    // Append labels used neccessary for sumbmission format
                    _fileManager.WriteToFile(new string[2] { "ImageId", "Label" });

                    // Loop through inputs and process it through the network
                    for (int lineCount = 1; lineCount < inputData.Length; lineCount++)
                    {
                        ProcessInput(inputData[lineCount].Split(",").Select(float.Parse).ToArray());

                        // Write current line and guessed answer onto file
                        _fileManager.WriteToFile(new int[2] { lineCount, GetGuessedValue() });
                    }
                }
                catch (Exception err)
                {
                    MessageBox.Show($"An error occurred while saving to the file: {err.Message}", "Save Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            } else
            {
                // No file selected
                Application.Current.Dispatcher.Invoke(() =>
                {
                    MessageBox.Show("No file selected for training.", "File Required", MessageBoxButton.OK, MessageBoxImage.Warning);
                });
            }
        }

        /// <summary>
        /// Gets what number has the highest confidence value from the output layer
        /// </summary>
        /// <returns>Number with highest confidence</returns>
        public int GetGuessedValue()
        {
            float maxValue = 0;
            int currentHighestPosition = 0;
            for (int i = 0; i < _outputLayer.Length; i++)
            {
                if (maxValue < _outputLayer[i])
                {
                    maxValue = _outputLayer[i];
                    currentHighestPosition = i;
                }
            }

            return currentHighestPosition;
        }

        #endregion
        #region Intialization Method and Utility
        /// <summary>
        /// Intializes Input layer with either blank data or input data
        /// </summary>
        /// <param name="inputData">Input data to be assigned and normalized</param>
        private void InitializeInputData(float[]? inputData = null)
        {
            // Asign input layer to 0 if no data is given
            if (inputData == null)
            {
                for (int i = 0; i < INPUT_SIZE; i++)
                {
                    _inputLayer[i] = 0;
                }
            }
            else // Assign input layer to data given
            {
                 _inputLayer = inputData;

                // Normalize data to be proccessed
                Normalize();
            }
        }

        /// <summary>
        /// Normaize input data to be within useful range of activation function
        /// </summary>
        private void Normalize()
        {
            for (int i = 0; i < INPUT_SIZE; i++)
            {
                // Divides input by its maximum value for values between 0 - 1
                _inputLayer[i] /= MAX_INPUT_VALUE;
            }
        }

        /// <summary>
        /// Initializes the hidden layers and bias based on the provided sizes.
        /// </summary>
        private void InitializeHiddenAndBiasLayers(int[] layerSizes)
        {
            int layerAmount = layerSizes.Length;
            _hiddenLayers = new float[layerAmount][];
            _bias = new float[layerAmount][];

            for (int i = 0; i < layerAmount; i++)
            {
                _hiddenLayers[i] = new float[layerSizes[i]];
                _bias[i] = new float[layerSizes[i]];
            }
        }

        /// <summary>
        /// Sets up the weights for connections between the layers.
        /// </summary>
        private void InitializeWeights(int[] layerSizes)
        {
            int layerAmount = layerSizes.Length;
            _weights = new float[layerAmount + 1][][];

            // Weights between input and first hidden layer.
            _weights[0] = new float[INPUT_SIZE][];
            for (int i = 0; i < INPUT_SIZE; i++)
            {
                _weights[0][i] = new float[layerSizes[0]];
            }

            // Weights for hidden layers.
            for (int i = 1; i < layerAmount; i++)
            {
                _weights[i] = new float[layerSizes[i - 1]][];
                for (int j = 0; j < layerSizes[i - 1]; j++)
                {
                    _weights[i][j] = new float[layerSizes[i]];
                }
            }

            // Weights between the last hidden layer and output layer.
            _weights[layerAmount] = new float[layerSizes[layerAmount - 1]][];
            for (int i = 0; i < layerSizes[layerAmount - 1]; i++)
            {
                _weights[layerAmount][i] = new float[OUTPUT_SIZE];
            }
        }

        /// <summary>
        /// Initializes weights with random values and the biases to 0.
        /// </summary>
        private void InitializeRandomWeightData()
        {
            // Initialize weights
            for (int i = 0; i < _weights.Length; i++)
            {
                for (int j = 0; j < _weights[i].Length; j++)
                {
                    for (int k = 0; k < _weights[i][j].Length; k++)
                    {
                        // Random float between -1 and 1
                        _weights[i][j][k] = (float)rand.NextDouble() * 2 - 1;
                    }
                }
            }

            // Initialize biases
            for (int i = 0; i < _bias.Length; i++)
            {
                for (int j = 0; j < _bias[i].Length; j++)
                {
                    _bias[i][j] = 0;
                }
            }
        }

        #endregion
        #region Computation Methods

        /// <summary>
        /// Does a forward pass of the neural network from input layer to output layer
        /// </summary>
        private void ForwardPass()
        {
            // Computes layer values after applying weights and biases except for output layer
            float[] previousLayerOutput = _inputLayer;
            for (int layer = 0; layer < _hiddenLayers.Length; layer++)
            {
                _hiddenLayers[layer] = CalculateLayerOutput(previousLayerOutput, _weights[layer], _bias[layer]);
                previousLayerOutput = _hiddenLayers[layer];
            }

            // Computes output layer which will not have a bias applied, uses biases of 0
            _outputLayer = CalculateLayerOutput(previousLayerOutput, _weights[_weights.Length - 1], new float[10]);
        }

        /// <summary>
        /// Calculates values for a given layer based on the repective weights and bias values
        /// </summary>
        /// <param name="inputs">Input layer</param>
        /// <param name="weights">Weight layer</param>
        /// <param name="biases">Bias layer</param>
        /// <returns>A calculated output layer</returns>
        private float[] CalculateLayerOutput(float[] inputs, float[][] weights, float[] biases)
        {
            // Contains the results of calculation through the model
            float[] outputs = new float[biases.Length];

            // Process outputs in parallel
            Parallel.For(0, outputs.Length, i =>
            {
                // Multiplies the weights
                for (int j = 0; j < inputs.Length; j++)
                {
                    outputs[i] += inputs[j] * weights[j][i];
                }

                // Add the bias
                outputs[i] += biases[i];

                // Pass through activation function
                outputs[i] = Sigmoid(outputs[i]);
            });

            return outputs;
        }

        /// <summary>
        /// Correct data via backpropagation based on learning rate and calculated error
        /// </summary>
        /// <param name="targetNumber">Correct number to be guessed</param>
        /// <param name="learningRate">Rate at which network adjusts for errors</param>
        private async Task BackpropagateAsync(int targetNumber, float learningRate)
        {
            //Transforms the target number in the format used in the backpropagation
            float[] targetOutput = new float[OUTPUT_SIZE];
            for (int i = 0; i < OUTPUT_SIZE; i++)
            {
                targetOutput[i] = (i == targetNumber) ? 1.0f : 0.0f;
            }

            int hiddenLayerLength = _hiddenLayers.Length;

            // Compute the output error
            float[] outputError = new float[_outputLayer.Length];
            for (int i = 0; i < _outputLayer.Length; i++)
            {
                outputError[i] = (targetOutput[i] - _outputLayer[i]) * SigmoidDerivative(_outputLayer[i]);
            }


            // Compute the hidden layer errors, starting from the last hidden layer
            float[][] hiddenErrors = new float[hiddenLayerLength][];
            for (int layer = hiddenLayerLength - 1; layer >= 0; layer--)
            {
                hiddenErrors[layer] = new float[_hiddenLayers[layer].Length];

                // Loop through every node in the current layer
                for (int node = 0; node < hiddenLayerLength; node++)
                {
                    // Contain the magnitude of the error from the target values, will accumulate the accuracy values from all connection to the right layer
                    float error = 0;

                    // Compute magnatude of error of current hidden layers node to all weights conncted to right hidden layers error for each node
                    if (layer != hiddenLayerLength - 1)
                    {
                        for (int k = 0; k < _hiddenLayers[layer + 1].Length; k++)
                        {
                            error += _weights[layer + 1][node][k] * hiddenErrors[layer + 1][k];
                        }
                    }
                    else // If it's the last hidden layer, compute magnatude of error relative to output layer error for each node
                    {
                        for (int k = 0; k < _outputLayer.Length; k++)
                        {
                            error += _weights[layer + 1][node][k] * outputError[k];
                        }
                    }
                    // Calculates the magnatude of how innaccurate thid node is from the target number
                    hiddenErrors[layer][node] = error * SigmoidDerivative(_hiddenLayers[layer][node]);
                }
            }

            // Define tasks to run
            var tasks = new List<Task> {
            Task.Run(() => UpdateInputLayerWeights(learningRate, hiddenErrors)),
            Task.Run(() => UpdateHiddenLayerWeights(learningRate, hiddenErrors, hiddenLayerLength)),
            Task.Run(() => UpdateOutputLayerWeights(learningRate, outputError, hiddenLayerLength)),
            Task.Run(() => UpdateBiases(learningRate, hiddenErrors))
            };

            // Wait for all the tasks to complete
            await Task.WhenAll(tasks);
        }

        /// <summary>
        /// Updates the input layers weights based on learning rate and error
        /// </summary>
        /// <param name="learningRate"></param>
        /// <param name="hiddenErrors"></param>
        private void UpdateInputLayerWeights(float learningRate, float[][] hiddenErrors)
        {
            // Update weights and biases starting input layers connection to first hidden layer
            for (int i = 0; i < INPUT_SIZE; i++)
            {
                for (int j = 0; j < _hiddenLayers[0].Length; j++)
                {
                    _weights[0][i][j] += learningRate * hiddenErrors[0][j] * _inputLayer[i];
                }
            }
        }

        /// <summary>
        /// Updates hidden layer weights based on learning rate and error
        /// </summary>
        /// <param name="learningRate"></param>
        /// <param name="hiddenErrors"></param>
        /// <param name="hiddenLayerLength"></param>
        private void UpdateHiddenLayerWeights(float learningRate, float[][] hiddenErrors, int hiddenLayerLength)
        {
            // Update weights for rest of hidden layers
            for (int layer = 0; layer < hiddenLayerLength - 1; layer++)
            {
                for (int i = 0; i < _hiddenLayers[layer].Length; i++)
                {
                    for (int j = 0; j < _hiddenLayers[layer + 1].Length; j++)
                    {
                        _weights[layer + 1][i][j] += learningRate * hiddenErrors[layer + 1][j] * _hiddenLayers[layer][i];
                    }
                }
            }
        }

        /// <summary>
        /// Updates output layers weights based on learning rate and error
        /// </summary>
        /// <param name="learningRate"></param>
        /// <param name="outputError"></param>
        /// <param name="hiddenLayerLength"></param>
        private void UpdateOutputLayerWeights(float learningRate, float[] outputError, int hiddenLayerLength)
        {
            // Update weights from last hidden layer to output
            for (int i = 0; i < _hiddenLayers[hiddenLayerLength - 1].Length; i++)
            {
                for (int j = 0; j < _outputLayer.Length; j++)
                {
                    _weights[hiddenLayerLength][i][j] += learningRate * outputError[j] * _hiddenLayers[hiddenLayerLength - 1][i];
                }
            }
        }

        /// <summary>
        /// Updates biases based on learning rate and error
        /// </summary>
        /// <param name="learningRate"></param>
        /// <param name="hiddenErrors"></param>
        private void UpdateBiases(float learningRate, float[][] hiddenErrors)
        {
            // Update biases
            for (int layer = 0; layer < _hiddenLayers.Length; layer++)
            {
                for (int node = 0; node < _hiddenLayers[layer].Length; node++)
                {
                    _bias[layer][node] += learningRate * hiddenErrors[layer][node];
                }
            }
        }

        /// <summary>
        /// Activation function sigmoid, Idealy values should be within -5 to 5
        /// </summary>
        /// <param name="x">Value to apply activation to</param>
        /// <returns>Number after activation</returns>
        private float Sigmoid(float x)
        {
            return 1 / (1 + (float)Math.Exp(-x));
        }

        /// <summary>
        /// Derivative if sigmoid for reversing the sigmoid activation function
        /// </summary>
        /// <param name="sigmoidOutput"></param>
        /// <returns>Number after applying derivative of sigmoid</returns>
        private float SigmoidDerivative(float sigmoidOutput)
        {
            return sigmoidOutput * (1 - sigmoidOutput);
        }
    }
    #endregion

}
