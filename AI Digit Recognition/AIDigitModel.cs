using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Shapes;

namespace AI_Digit_Recognition
{
    internal class AIDigitModel
    {
        private static Random rand = new Random();
        private FileManager _fileManager;

        private const int INPUT_SIZE = 728;  // Size of the input layer.
        private const int OUTPUT_SIZE = 10;  // Size of the output layer.

        private float[] _inputLayer = new float[INPUT_SIZE];
        private float[] _outputLayer = new float[OUTPUT_SIZE];
        private float[][] _hiddenLayers;
        private float[][][] _weights;  // Weights between nodes of consecutive layers.
        private float[][] _bias;       // Bias values for each node in hidden layers.

        /// <summary>
        /// Initializes fresh Ai model with the specified hidden layer sizes.
        /// </summary>
        /// <param name="layerSizes">Sizes of the hidden layers.</param>
        public AIDigitModel(int[] layerSizes)
        {
            _fileManager = new FileManager();
            InitializeHiddenLayers(layerSizes);
            InitializeWeights(layerSizes);
            InitializeRandomData();
            InitializeInputData();
        }

        /// <summary>
        /// Load Ai model from text file
        /// </summary>
        public AIDigitModel(string aiModelFile)
        {
            _fileManager = new FileManager();
            LoadFromFile(aiModelFile);
        }

        #region Public Methods
        /// <summary>
        /// Saves AI model to txt file
        /// </summary>
        public void SaveToFile()
        {
            // Get hidden layer dimensions
            int[] hiddenLayerData = new int[_hiddenLayers.Length];

            for (int i = 0; i < _hiddenLayers.Length; i++)
            {
                hiddenLayerData[i] = _hiddenLayers[i].Length;
            }

            // Clears the current path to allow user to select a new save path
            _fileManager.DeletePath();

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

        /// <summary>
        /// Loads AI model data from file
        /// </summary>
        public void LoadFromFile(string aiModeFileData = null)
        {
            if (aiModeFileData == null) _fileManager.SelectFile();
            else _fileManager.DefinePath(aiModeFileData);

            // Hold AI model data from file
            string[] modelData = _fileManager.ReadAllLines();

            // Keeps track of current line being read from file, starts at 3 since first three lines do not change
            int currentLine = 3;

            // Load hidden layer amount
            int[] hiddenLayerArray = modelData[0].Split(",").Select(int.Parse).ToArray();

            // Load input layer
            _inputLayer = modelData[1].Split(",").Select(float.Parse).ToArray();

            // Load output layer
            _outputLayer = modelData[2].Split(",").Select(float.Parse).ToArray();

            // Initalize hidden layers and biases with new dimensions
            InitializeHiddenLayers(hiddenLayerArray);

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

        /// <summary>
        /// Takes input to be proccessed by AI and returns confidence values for AI's guess
        /// </summary>
        /// <param name="inputData"></param>
        /// <returns></returns>
        public void ProcessNumber(float[] inputLayerData)
        {
            // Move data into input layer and normalize it
            InitializeInputData(inputLayerData);

            // Proccess data through neural network
            ForwardPass();

        }

        /// <summary>
        /// Gets the output layer as precentages up to 100
        /// </summary>
        /// <returns>int[] with index corrilating to the repective number it represents e.g. index 0 = number 0</returns>
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
        /// Intializes Input layer with either blank data or inputted array data
        /// </summary>
        /// <param name="inputData"></param>
        private void InitializeInputData(float[]? inputData = null)
        {
            if (inputData == null)
            {
                for (int i = 0; i < _inputLayer.Length; i++)
                {
                    _inputLayer[i] = 0;
                }
            }
            else
            {
                for (int i = 0; i < _inputLayer.Length; i++)
                {
                    _inputLayer[i] = inputData[i];
                }
                Normalize();
            }
        }

        /// <summary>
        /// Trains Ai model using given cvs training data with given epoch and learning rate
        /// </summary>
        /// <param name="csvFile"></param>
        /// <param name="learningRate"></param>
        /// <param name="epochs"></param>
        public async Task Train(float learningRate, int epochs, IProgress<float[]> progress, string trainingFile = null)
        {
            await Task.Run(async () =>
            {
            // Keep track of progress through training and accuracy of guesses
            float lineCount = 0;
            float totalLines = 0;
            int targetNumber = 0;
            float[] inputData;

            // formatted as {lineCount, correctGuessCount}
            float[] progressAndAccuracy = new float[2];

            // Load and get training data
            if (trainingFile != null) _fileManager.DefinePath(trainingFile);
            else _fileManager.SelectFile();

            if (trainingFile != null)
            {
                // Load input data
                string[] lines = _fileManager.ReadAllLines();

                // Calculate how many lines need to be processed, if 0 then only processing once
                totalLines = lines.Length * ((epochs == 0) ? 1 : epochs);

                // Begin training through entire data set unless epochs is 0, then only process data not train
                for (int epoch = epochs == 0 ? -1 : 0; epoch < epochs; epoch++)
                {
                    // Skip the first line as it contain label information
                    foreach (string line in lines.Skip(1))
                    {
                        // Format data in float[]
                        inputData = line.Split(",").Select(float.Parse).ToArray();

                        // First number is the target number
                        targetNumber = (int)inputData[0];

                        // Process the input data 
                        ProcessNumber(inputData.Skip(1).ToArray());

                        // Backpropagate to correct for errors, if epoch is 0 then only process data
                        if (epochs > 0)
                        {
                            await BackpropagateAsync(targetNumber, learningRate);
                        }
                        else
                        {
                            if (GetGuessedValue() == targetNumber) progressAndAccuracy[1]++;
                        }


                        // Iterate progress count and periodically update UI
                        lineCount++;
                        if (lineCount % 100 == 0)
                        {
                            progressAndAccuracy[0] = (lineCount / totalLines) * 100;
                            progress?.Report(progressAndAccuracy);
                        }
                    }

                    progressAndAccuracy[1] = (progressAndAccuracy[1] / totalLines) * 100;
                    progress?.Report(progressAndAccuracy);
                    }

                }
                else
                {
                    Console.WriteLine("No File");
                }
            });
        }

        /*public async Task TestOnTrainingData(string trainingFile = null, IProgress<int> progress)
        {
            await Task.Run(() =>
            {
                // Keep track of progress and accuracy
                float progressCount = 0;
                float totalLines = 0;
                float numberCorrect = 0;
                int targetNumber = 0;
                float[] inputData;

                // Load and get training data
                if (trainingFile != null) _fileManager.DefinePath(trainingFile);
                else _fileManager.SelectFile();

                // Load input Data from file
                string[] lines = _fileManager.ReadAllLines();
                totalLines= lines.Length;

                foreach (string line in lines)
                {
                    // Format data into float[]
                    inputData = line.Split(",").Select(float.Parse).ToArray();

                    // First number is the target number
                    targetNumber = (int)inputData[0];

                    // Process the input data 
                    ProcessNumber(inputData.Skip(1).ToArray());
                }

            });
        }*/

        /// <summary>
        /// Uses a test file to test how accurate AI model is to fresh data
        /// </summary>
        /// <param name="writeToFile">file to answers to</param>
        /// <param name="readFromFile">test data</param>
        public void TestData(string writeToFile = null, string readFromFile = null)
        {
            int count = 0;
            // Set file path to test data or select a file with test data
            if (readFromFile != null) _fileManager.DefinePath(readFromFile);
            else _fileManager.SelectFile();


            // Extract input data from file
            string[] inputData = _fileManager.ReadAllLines();

            // Set save path to writeToFile or select a file to write to
            if (writeToFile != null) _fileManager.DefinePath(writeToFile);
            else _fileManager.SelectSaveFile();

            // Clear file to fresh data
            _fileManager.ClearFile();

            // Loop through inputs and process it through the network
            for (int lineCount = 1; lineCount < inputData.Length; lineCount++)
            {
                ProcessNumber(inputData[lineCount].Split(",").Select(float.Parse).ToArray());

                // Write current line and guessed answer onto file
                _fileManager.WriteToFile(new int[2] { lineCount, GetGuessedValue() });
                count++;
                if (count % 100== 0) Debug.WriteLine(count);
            }
        }
        #endregion
        #region Intialization Method and Utility
        /// <summary>
        /// Normaize input data to be within useful range of activation function
        /// </summary>
        private void Normalize()
        {
            for (int i = 0; i < _inputLayer.Length; i++)
            {
                _inputLayer[i] /= 255.0f;
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
                    currentHighestPosition= i;
                }
            }

            return currentHighestPosition;
        }

        /// <summary>
        /// Initializes the hidden layers and bias based on the provided sizes.
        /// </summary>
        private void InitializeHiddenLayers(int[] layerSizes)
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
        private void InitializeRandomData()
        {
            // Initialize weights
            for (int i = 0; i < _weights.Length; i++)
            {
                for (int j = 0; j < _weights[i].Length; j++)
                {
                    for (int k = 0; k < _weights[i][j].Length; k++)
                    {
                        _weights[i][j][k] = (float)rand.NextDouble() * 2 - 1; // random float between -1 and 1
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

        /// <summary>
        /// Takes a layer and multiplies each nodes connection to the next layer for each node and add the bias of that next node, outputs after appling activation function
        /// </summary>
        /// <param name="inputs"></param>
        /// <param name="weights"></param>
        /// <param name="biases"></param>
        /// <returns></returns>
        /// 
        #endregion
        #region Computation Methods

        /// <summary>
        /// Does a forward pass of the neural network from input layer to output layer
        /// </summary>
        private void ForwardPass()
        {
            // Computes layer values after apply weights and biases except for output layer
            float[] previousLayerOutput = _inputLayer;
            for (int layer = 0; layer < _hiddenLayers.Length; layer++)
            {
                _hiddenLayers[layer] = ComputeLayerOutput(previousLayerOutput, _weights[layer], _bias[layer]);
                previousLayerOutput = _hiddenLayers[layer];
            }

            // Computs output layer which will not use any bias
            _outputLayer = ComputeLayerOutput(previousLayerOutput, _weights[_weights.Length - 1], new float[10]);
        }

        private float[] ComputeLayerOutput(float[] inputs, float[][] weights, float[] biases)
        {
            float[] outputs = new float[biases.Length];

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
        /// <param name="targetOutput"></param>
        /// <param name="learningRate"></param>
        private async Task BackpropagateAsync(int targetNumber, float learningRate)
        {
            //Transforms the target number in the format used in the backpropagation
            float[] targetOutput = new float[OUTPUT_SIZE];
            for (int i = 0; i < OUTPUT_SIZE; i++)
            {
                targetOutput[i] = (i == targetNumber) ? 1.0f : 0.0f;
            }

            int hiddenLayerLength = _hiddenLayers.Length;

            // Compute the output error in parallel
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

                Parallel.For(0, _hiddenLayers[layer].Length, node =>
                {
                    float error = 0;

                    // Compute error relative to the next hidden layer
                    if (layer != hiddenLayerLength - 1)
                    {
                        for (int k = 0; k < _hiddenLayers[layer + 1].Length; k++)
                        {
                            error += _weights[layer + 1][node][k] * hiddenErrors[layer + 1][k];
                        }
                    }
                    else // If it's the last hidden layer, compute error relative to output layer
                    {
                        for (int k = 0; k < _outputLayer.Length; k++)
                        {
                            error += _weights[layer + 1][node][k] * outputError[k];
                        }
                    }
                    hiddenErrors[layer][node] = error * SigmoidDerivative(_hiddenLayers[layer][node]);
                });
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
            for (int i = 0; i < _inputLayer.Length; i++)
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
        /// Activation function sigmoid, Idealy values shou.d be within -5 to 5
        /// </summary>
        /// <param name="x"></param>
        /// <returns></returns>
        private float Sigmoid(float x)
        {
            return 1 / (1 + (float)Math.Exp(-x));
        }

        /// <summary>
        /// Derivative if sigmoid for reversing the sigmoid activation function
        /// </summary>
        /// <param name="sigmoidOutput"></param>
        /// <returns></returns>
        private float SigmoidDerivative(float sigmoidOutput)
        {
            return sigmoidOutput * (1 - sigmoidOutput);
        }
    }
    #endregion

}
