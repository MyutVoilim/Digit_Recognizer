using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Automation.Provider;
using System.Windows.Documents;

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
        /// Initializes the AI model with the specified hidden layer sizes.
        /// </summary>
        /// <param name="layerSizes">Sizes of the hidden layers.</param>
        public AIDigitModel(int[] layerSizes)
        {
            InitializeHiddenLayers(layerSizes);
            InitializeWeights(layerSizes);
            InitializeInputData();
        }

        public AIDigitModel(String filename)
        {
            LoadFromFile(filename);
        }
        public void SaveToFile(string filename)
        {
            using (StreamWriter writer = new StreamWriter(filename))
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

        public void LoadFromFile(string filename)
        {
            using (StreamReader reader = new StreamReader(filename))
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
    public void InitializeInputData(float[]? inputData = null)
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
        /// Initializes weights and biases with random values.
        /// </summary>
        public void InitializeRandomData()
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
                    _bias[i][j] = (float)rand.NextDouble() * 2 - 1; // random float between -1 and 1
                }
            }
        }

        public void Train(string csvFile, float learningRate, int epochs)
        {
            string[] lines = File.ReadAllLines(csvFile);
            for (int epoch = 0; epoch < epochs; epoch++)
            {
                foreach (string line in lines.Skip(1)) // Skip the first line
                {
                    string[] values = line.Split(',');
                    int targetNumber = int.Parse(values[0]);
                    for (int i = 0; i < INPUT_SIZE; i++)
                    {
                        _inputLayer[i] = int.Parse(values[i + 1]);
                    }
                    Normalize();

                    // Forward pass 
                    float[] previousLayerOutput = _inputLayer;
                    for (int layer = 0; layer < _hiddenLayers.Length; layer++)
                    {
                        _hiddenLayers[layer] = ComputeLayerOutput(previousLayerOutput, _weights[layer], _bias[layer]);
                        previousLayerOutput = _hiddenLayers[layer];
                    }
                    _outputLayer = ComputeLayerOutput(previousLayerOutput, _weights[_weights.Length - 1], _bias[_bias.Length - 1]);

                    float[] targetOutput = new float[OUTPUT_SIZE];
                    for (int i = 0; i < OUTPUT_SIZE; i++)
                    {
                        targetOutput[i] = (i == targetNumber) ? 1.0f : 0.0f;
                    }

                    Backpropagate(targetOutput, learningRate);
                }
            }
        }
        private float[] ComputeLayerOutput(float[] inputs, float[][] weights, float[] biases)
        {
            float[] outputs = new float[weights[0].Length];
            for (int j = 0; j < weights[0].Length; j++)
            {
                outputs[j] = 0;
                for (int i = 0; i < inputs.Length; i++)
                {
                    outputs[j] += inputs[i] * weights[i][j];
                }
                outputs[j] += biases[j];
                outputs[j] = Sigmoid(outputs[j]);
            }
            return outputs;
        }

        /// <summary>
        /// Correct data via backpropagation
        /// </summary>
        /// <param name="targetOutput"></param>
        /// <param name="learningRate"></param>
        public void Backpropagate(float[] targetOutput, float learningRate)
        {
            // 1. Compute the output error
            float[] outputError = new float[_outputLayer.Length];
            for (int i = 0; i < _outputLayer.Length; i++)
            {
                float output = _outputLayer[i];
                outputError[i] = (targetOutput[i] - output) * SigmoidDerivative(output);
            }

            // 2. Compute the hidden layer errors, starting from the last hidden layer
            float[][] hiddenErrors = new float[_hiddenLayers.Length][];
            for (int layer = _hiddenLayers.Length - 1; layer >= 0; layer--)
            {
                hiddenErrors[layer] = new float[_hiddenLayers[layer].Length];
                for (int node = 0; node < _hiddenLayers[layer].Length; node++)
                {
                    float error = 0;
                    // If it's the last hidden layer, compute error relative to output layer
                    if (layer == _hiddenLayers.Length - 1)
                    {
                        for (int k = 0; k < _outputLayer.Length; k++)
                        {
                            error += _weights[layer + 1][node][k] * outputError[k];
                        }
                    }
                    else // Compute error relative to the next hidden layer
                    {
                        for (int k = 0; k < _hiddenLayers[layer + 1].Length; k++)
                        {
                            error += _weights[layer + 1][node][k] * hiddenErrors[layer + 1][k];
                        }
                    }
                    hiddenErrors[layer][node] = error * SigmoidDerivative(_hiddenLayers[layer][node]);
                }
            }

            // 3. Update weights and biases
            // Update weights for input to first hidden layer
            for (int i = 0; i < _inputLayer.Length; i++)
            {
                for (int j = 0; j < _hiddenLayers[0].Length; j++)
                {
                    _weights[0][i][j] += learningRate * hiddenErrors[0][j] * _inputLayer[i];
                }
            }

            // Update weights for hidden layers
            for (int layer = 1; layer < _weights.Length; layer++)
            {
                for (int i = 0; i < _hiddenLayers[layer - 1].Length; i++)
                {
                    for (int j = 0; j < _hiddenLayers[layer].Length; j++)
                    {
                        _weights[layer][i][j] += learningRate * hiddenErrors[layer][j] * _hiddenLayers[layer - 1][i];
                    }
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

        private float Sigmoid(float x)
        {
            return 1 / (1 + (float)Math.Exp(-x));
        }

        private float SigmoidDerivative(float sigmoidOutput)
        {
            return sigmoidOutput * (1 - sigmoidOutput);
        }

        private void Normalize()
        {
            for (int i = 0; i < _inputLayer.Length; i++)
            {
                _inputLayer[i] /= 255.0f;
            }
        }
    }

}
