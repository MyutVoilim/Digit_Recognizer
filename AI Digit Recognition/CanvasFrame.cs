using System.Diagnostics;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Shapes;
using System.Linq;
using System;
using System.Windows.Media;

namespace AI_Digit_Recognition
{
    /// <summary>
    /// This class is responsible for managing the visual representation of a canvas and the binding for data manipulated while drawing on the canvas.
    /// The pixel dimensions of the canvas is converted into a 28 x 28 grid of cells that can be drawn onto the canvas using a intensity from 0 (black) to 255 (white)
    /// The underlying data is a INotifyProperty from CanvasData that is data bound to the canvas cells. Data for displaying on the canvas can be inputted as a string or retreived 
    /// as a float[] to be used else where. 
    /// </summary>
    internal class CanvasFrame
    {
        // Holds a INotifyProperty that will bind with the canvas to update children elements
        private CanvasData[,] _canvasData;

        // Canvas displaying digits
        private Canvas _digitCanvas;

        // The size of individual cells displayed on the canvas based on the UI canvas pixel count / CanvasDim
        private int _blockSize;

        // Constants for intensity of brush while drawing on canvas 0 (black) to 255 (white)
        private const int MaxIntensity = 150;
        private const int MidIntensity = 40;
        private const int LowIntensity = 20;

        // Constant for canvas dimensions will be a square grid 28 x 28
        private const int CanvasDim = 28;

        /// <summary>
        /// Constructor for attaching UI canvas component and matching the CanvasData that will be data-bound with cells on the canvas
        /// </summary>
        /// <param name="digitCanvas">Canvas componenet</param>
        public CanvasFrame(Canvas digitCanvas)
        {
            // Create INotifyPopertyChanged array that will attach to canvas cells
            _canvasData = new CanvasData[CanvasDim, CanvasDim];

            // Attached Canvas that will be data bound
            _digitCanvas = digitCanvas;

            // Calculates cells sizes based on Canvas pixel count and Intended Canvas Dimensions
            _blockSize = (int)(_digitCanvas.Width / CanvasDim);

            // Creates cell grid and binds _canvasData to the repective positions
            CreateGridAndBindData();
        }

        /// <summary>
        /// Constructor that creates its own canvas mainly for testing purposes
        /// </summary>
        /// <param name="digitCanvas">Canvas componenet</param>
        public CanvasFrame()
        {
            // Create INotifyPopertyChanged array that will attach to canvas cells
            _canvasData = new CanvasData[CanvasDim, CanvasDim];

            // Attached Canvas that will be data bound
            _digitCanvas = new Canvas();

            // Calculates cells sizes based on Canvas pixel count and Intended Canvas Dimensions
            _blockSize = (int)(_digitCanvas.Width / CanvasDim);

            // Creates cell grid and binds _canvasData to the repective positions
            CreateGridAndBindData();
        }

        #region Public Methods And Properties
        /// <summary>
        /// Gets canvas property
        /// </summary>
        public Canvas Canvas => _digitCanvas;

        /// <summary>
        /// Draws on defined cell position with faded gradiant around that cell
        /// </summary>
        /// <param name="xPix">The x pixel relative to the canvas component</param>
        /// <param name="yPix">The y pixel relative to the canvas component</param>
        public void DrawOnGrid(int xPix, int yPix)
        {
            // Converts pixel data into repective cells within canvas grid
            int xCell = xPix / _blockSize;
            int yCell = yPix / _blockSize;

            // Check inputs are within canvas dimensions
            if (xCell >= 0 && xCell < CanvasDim && yCell >= 0 && yCell < CanvasDim)
            {
                // Draws center square Cell
                ValidateAndDraw(xCell, yCell, MaxIntensity);

                // Draws slightly dimmed cells directly adjacent from center sqaure
                ValidateAndDraw(xCell + 1, yCell, MidIntensity);
                ValidateAndDraw(xCell - 1, yCell, MidIntensity);
                ValidateAndDraw(xCell, yCell + 1, MidIntensity);
                ValidateAndDraw(xCell, yCell - 1, MidIntensity);

                // Draws dimmed cells in corners from center cell
                ValidateAndDraw(xCell + 1, yCell - 1, LowIntensity);
                ValidateAndDraw(xCell - 1, yCell - 1, LowIntensity);
                ValidateAndDraw(xCell + 1, yCell + 1, LowIntensity);
                ValidateAndDraw(xCell - 1, yCell + 1, LowIntensity);
            }
        }

        /// <summary>
        /// Sets specific canvasData index to a ColorIntensity value from 0 (black) to 255 (white)
        /// </summary>
        /// <param name="xCell">Cell position on the x-axis starting at 0</param>
        /// <param name="yCell">Cell position on the y-axis starting at 0</param>
        /// <param name="colorIntensity">Intensity of color change</param>
        private void SetValueAt(int xCell, int yCell, int colorIntensity)
        {
            _canvasData[xCell, yCell].ColorIntensity = Math.Clamp(colorIntensity, 0, 255);
        }

        /// <summary>
        /// Gets the canvasData as a float array
        /// </summary>
        /// <returns>Canvas Data</returns>
        public float[] GetCanvasArray()
        {
            // Total points of data on square grid is CanvasDim^2
            float[] processedData = new float[CanvasDim * CanvasDim];

            // Convert 2D data into 1D
            for (int i = 0; i < CanvasDim * CanvasDim; i++)
            {
                processedData[i] = _canvasData[i % CanvasDim, i / CanvasDim].ColorIntensity;
            }
            return processedData;
        }

        /// <summary>
        /// Converts incoming string array data into the format used in CanvasFrame
        /// </summary>
        /// <param name="inputData">String data to convert</param>
        public void ConvertStringData(string inputData)
        {
            // Parse values into int array
            int[] convertedInputData = inputData.Split(',').Select(str => { return (int.TryParse(str, out int result))? result : 0; }).ToArray();

            // Convert 1D data into 2D , starts at 1 to the first value that is a target number
            for (int i = 1; i < convertedInputData.Length; i++)
            {
                _canvasData[(i - 1) % CanvasDim, (i - 1) / CanvasDim].ColorIntensity = convertedInputData[i];
            }
        }



        /// <summary>
        /// Clears canvas back to 0 (black)
        /// </summary>
        public void ClearCanvas()
        {
            for (int i = 0; i < CanvasDim; i++)
            {
                for (int j = 0; j < CanvasDim; j++)
                {
                    _canvasData[i, j].ColorIntensity = 0;
                }
            }
        }
        #endregion
        #region Private Methods
        /// <summary>
        /// Forms a cell grid on _digitCanvas and binds the respective positions in _canvasData
        /// </summary>
        private void CreateGridAndBindData()
        {
            //Loop through each position in _canvasData
            for (int row = 0; row < CanvasDim; row++)
            {
                for (int col = 0; col < CanvasDim; col++)
                {
                    // Fill _canvasData
                    _canvasData[row, col] = new CanvasData { ColorIntensity = 0 };

                    // Create the Blocks
                    Rectangle block = new Rectangle();
                    block.Width = _blockSize;
                    block.Height = _blockSize;

                    // Tie the ColorIntensity property from CanvasData to each block and convert the int values into grayscale brush values
                    block.DataContext = _canvasData[row, col];
                    Binding binding = new Binding("ColorIntensity");
                    binding.Converter = new IntToGrayscaleBrushConverter();
                    block.SetBinding(Rectangle.FillProperty, binding);

                    // Add Blocks to _digitCanvas
                    _digitCanvas.Children.Add(block);
                    Canvas.SetLeft(block, (row * _blockSize));
                    Canvas.SetTop(block, (col * _blockSize));
                }
            }
        }

        /// <summary>
        /// Validates that the current position is within _digitCanvas and changes color intensity on inputted cell
        /// </summary>
        /// <param name="xCell">Cell position on the x-axis starting at 0</param>
        /// <param name="yCell">Cell position on the y-axis starting at 0</param>
        /// <param name="colorIntensity">Intensity of color change</param>
        private void ValidateAndDraw(int xCell, int yCell, int colorIntensity)
        {
            if (xCell >= 0 && xCell < CanvasDim && yCell >= 0 && yCell < CanvasDim)
            {
                int intensityTotal = Math.Clamp(_canvasData[xCell, yCell].ColorIntensity + colorIntensity, 0, 255);
                _canvasData[xCell, yCell].ColorIntensity = intensityTotal;

            }
        }
        #endregion
    }
}
