using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Automation.Provider;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Navigation;
using System.Windows.Threading;

namespace AI_Digit_Recognition
{
    internal class AIDigitModel
    {
        private static Random rand = new Random();

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
            InitializeHiddenLayers(layerSizes);
            InitializeWeights(layerSizes);
            InitializeRandomData();
            InitializeInputData();
        }

        /// <summary>
        /// Load Ai model from text file
        /// </summary>
        public AIDigitModel()
        {
            LoadFromFile();
        }

        #region Public Methods
        /// <summary>
        /// Saves input and hidden layers to a text file
        /// </summary>
        /// <param name="filename"></param>
        public void SaveToFile()
        {
            SaveFileDialog saveFileDialog = new SaveFileDialog();
            saveFileDialog.Filter = "Text Files (*.txt)|*.txt|All Files (*.*)|*.*"; // Filters for text files
            saveFileDialog.Title = "Save an AI Model File";

            bool? result = saveFileDialog.ShowDialog();
            if (result.HasValue && result.Value)
            {
                using (StreamWriter writer = new StreamWriter(saveFileDialog.FileName))
                {
                    // Save input layer
                    writer.WriteLine(string.Join(",", _inputLayer));

                    // Save output layer
                    writer.WriteLine(string.Join(",", _outputLayer));

                    // Save hidden layers
                    foreach (var layer in _hiddenLayers)
                    {
                        writer.WriteLine(string.Join(",", layer));
                    }

                    // Save biases
                    foreach (var layerBias in _bias)
                    {
                        writer.WriteLine(string.Join(",", layerBias));
                    }

                    // Save weights
                    foreach (var layerWeights in _weights)
                    {
                        foreach (var nodeWeights in layerWeights)
                        {
                            writer.WriteLine(string.Join(",", nodeWeights));
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Loads input and hidden layers from a text file
        /// </summary>
        /// <param name="filename"></param>
        public void LoadFromFile()
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "Text Files (*.txt)|*.txt|All Files (*.*)|*.*";
            openFileDialog.Title = "Load an AI Model File";

            bool? result = openFileDialog.ShowDialog();
            if (result.HasValue && result.Value)
            {
                using (StreamReader reader = new StreamReader(openFileDialog.FileName))
                {
                    // Load input layer
                    _inputLayer = reader.ReadLine().Split(',').Select(float.Parse).ToArray();

                    // Load output layer
                    _outputLayer = reader.ReadLine().Split(',').Select(float.Parse).ToArray();

                    // Load hidden layers
                    for (int i = 0; i < _hiddenLayers.Length; i++)
                    {
                        _hiddenLayers[i] = reader.ReadLine().Split(',').Select(float.Parse).ToArray();
                    }

                    // Load biases
                    for (int i = 0; i < _bias.Length; i++)
                    {
                        _bias[i] = reader.ReadLine().Split(',').Select(float.Parse).ToArray();
                    }

                    // Load weights
                    for (int i = 0; i < _weights.Length; i++)
                    {
                        for (int j = 0; j < _weights[i].Length; j++)
                        {
                            _weights[i][j] = reader.ReadLine().Split(',').Select(float.Parse).ToArray();
                        }
                    }
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="inputData"></param>
        /// <returns></returns>
        public int[] Predict(float[,] inputData)
        {

            // Convert 2D array to 1D
            float[] inputLayerData = new float[INPUT_SIZE];
            for (int i = 0; i < INPUT_SIZE; i++)
            {
                inputLayerData[i] = inputData[i % 28, i / 28];
            }

            InitializeInputData(inputLayerData);
            Normalize();
            ForwardPass();

            // Convert _outputLayer values to percentage with two decimal places
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
            }
        }

        /// <summary>
        /// Trains Ai model using given cvs training data with given epoch and learning rate
        /// </summary>
        /// <param name="csvFile"></param>
        /// <param name="learningRate"></param>
        /// <param name="epochs"></param>
        public async Task Train(float learningRate, int epochs)
        {
            await Task.Run(() =>
            {
                OpenFileDialog openFileDialog = new OpenFileDialog();
                openFileDialog.Filter = "CSV Files (*.csv)|*.csv|Text Files (*.txt)|*.txt|All Files (*.*)|*.*";
                openFileDialog.Title = "Select a Training File";

                bool? result = openFileDialog.ShowDialog();
                if (result.HasValue && result.Value)
                {
                    string[] lines = File.ReadAllLines(openFileDialog.FileName);
                    for (int epoch = 0; epoch < epochs; epoch++)
                    {
                        foreach (string line in lines.Skip(1)) // Skip the first line
                        {
                            string[] values = line.Split(',');
                            int targetNumber = int.Parse(values[0]); // First number is the target number
                            for (int i = 0; i < INPUT_SIZE; i++)
                            {
                                _inputLayer[i] = int.Parse(values[i + 1]);
                            }

                            Normalize();
                            ForwardPass();

                            //Transforms the target number in the format used in the backpropagation
                            float[] targetOutput = new float[OUTPUT_SIZE];
                            for (int i = 0; i < OUTPUT_SIZE; i++)
                            {
                                targetOutput[i] = (i == targetNumber) ? 1.0f : 0.0f;
                            }

                            Backpropagate(targetOutput, learningRate);
                        }
                    }
                }
            });
        }
        #endregion
        #region Intialization Methods
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
        private float[] ComputeLayerOutput(float[] inputs, float[][] weights, float[] biases)
        {
            float[] outputs = new float[weights[0].Length];
            for (int i = 0; i < weights[0].Length; i++)
            {
                outputs[i] = 0;
                
                // Multiplies the weights
                for (int j = 0; j < inputs.Length; j++)
                {
                    outputs[i] += inputs[j] * weights[j][i];
                }

                //Add the bias
                outputs[i] += biases[i];

                //Pass through activation function
                outputs[i] = Sigmoid(outputs[i]);
            }
            return outputs;
        }

        /// <summary>
        /// Correct data via backpropagation
        /// </summary>
        /// <param name="targetOutput"></param>
        /// <param name="learningRate"></param>
        private void Backpropagate(float[] targetOutput, float learningRate)
        {
            int hiddenLayerLength = _hiddenLayers.Length;

            // Compute the output error
            float[] outputError = new float[_outputLayer.Length];
            for (int i = 0; i < _outputLayer.Length; i++)
            {
                outputError[i] = (targetOutput[i] - _outputLayer[i]) * SigmoidDerivative(_outputLayer[i]);
            }

            //Compute the hidden layer errors, starting from the last hidden layer
            float[][] hiddenErrors = new float[hiddenLayerLength][];
            for (int layer = hiddenLayerLength - 1; layer >= 0; layer--)
            {
                hiddenErrors[layer] = new float[_hiddenLayers[layer].Length];
                for (int node = 0; node < _hiddenLayers[layer].Length; node++)
                {
                    float error = 0;

                    // If it's the last hidden layer, compute error relative to output layer
                    if (layer == hiddenLayerLength - 1)
                    {
                        for (int i = 0; i < _outputLayer.Length; i++)
                        {
                            error += _weights[layer + 1][node][i] * outputError[i];
                        }
                    }
                    else // Compute error relative to the next hidden layer
                    {
                        for (int i = 0; i < _hiddenLayers[layer + 1].Length; i++)
                        {
                            error += _weights[layer + 1][node][i] * hiddenErrors[layer + 1][i];
                        }
                    }
                    hiddenErrors[layer][node] = error * SigmoidDerivative(_hiddenLayers[layer][node]);
                }
            }

            // Update weights and biases starting input layers connection to first hidden layer
            for (int i = 0; i < _inputLayer.Length; i++)
            {
                for (int j = 0; j < _hiddenLayers[0].Length; j++)
                {
                    _weights[0][i][j] += learningRate * hiddenErrors[0][j] * _inputLayer[i];
                }
            }

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

            // Update weights from last hidden layer to output
            for (int i = 0; i < _hiddenLayers[hiddenLayerLength - 1].Length; i++)
            {
                for (int j = 0; j < _outputLayer.Length; j++)
                {
                    _weights[_hiddenLayers.Length][i][j] += learningRate * outputError[j] * _hiddenLayers[_hiddenLayers.Length - 1][i];
                }
            }

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
        /// Does a forward pass of the neural network from input layer to output layer
        /// </summary>
        private void ForwardPass()
        {
            float[] previousLayerOutput = _inputLayer;
            for (int layer = 0; layer < _hiddenLayers.Length; layer++)
            {
                _hiddenLayers[layer] = ComputeLayerOutput(previousLayerOutput, _weights[layer], _bias[layer]);
                previousLayerOutput = _hiddenLayers[layer];
            }

            _outputLayer = ComputeLayerOutput(previousLayerOutput, _weights[_weights.Length - 1], _bias[_bias.Length - 1]);
        }

    }
    #endregion

}
