using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Shapes;

namespace AI_Digit_Recognition
{
    internal class CanvasFrame
    {
        private LoadFile _dataFile;
        //Holds a INotify property that will later bind with the canvas to update children
        private CanvasData[,] _canvasData;
        //Canvas that will be displayed
        private Canvas _digitCanvas;
        //The about of units across the x and y axis
        private int _canvasDim;
        //The size of individual blocks withing the grid relative to the canvas size
        private int _blockSize;

        // Constants for drawing intensities
        private const int MaxIntensity = 150;
        private const int MidIntensity = 40;
        private const int LowIntensity = 20;

        // Constant for canvas dimensions
        private const int CanvasDim = 28;

        //Default with value of 0 for grid of 28 x 28
        public CanvasFrame(Canvas digitCanvas, string file)
        {
            _canvasData = new CanvasData[CanvasDim, CanvasDim];
            _digitCanvas = digitCanvas;
            _canvasDim = CanvasDim;
            _blockSize = (int)(_digitCanvas.Width / CanvasDim);
            _dataFile = new LoadFile(file);
            CreateGrid();
        }

        /// <summary>
        /// Creates a square grid based on blockSize and binds canvas children to matrix from CanvasData to automatically update when CavasData is changed
        /// </summary>
        private void CreateGrid()
        {
            //Loop through each position in _canvasData
            for (int row = 0; row < _canvasDim; row++)
            {
                for (int col = 0; col < _canvasDim; col++)
                {
                    //Fill _canvasData
                    _canvasData[row, col] = new CanvasData { Value = 0 };
                    //Create the Blocks
                    Rectangle block = new Rectangle();
                    block.Width = _blockSize;
                    block.Height = _blockSize;
                    //Tie the Value property from the CanvasData object to each block and convert the int values into grayscale brush values
                    block.DataContext = _canvasData[row, col];
                    Binding binding = new Binding("Value");
                    binding.Converter = new IntToGrayscaleBrushConverter();
                    block.SetBinding(Rectangle.FillProperty, binding);
                    //Add Blocks to _digitCanvas
                    _digitCanvas.Children.Add(block);
                    Canvas.SetLeft(block, (row * _blockSize));
                    Canvas.SetTop(block, (col * _blockSize));
                }
            }
        }

        /// <summary>
        /// Draws on canvas
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        public void DrawOnGrid(int x, int y)
        {
            int xPos = x / _blockSize;
            int yPos = y / _blockSize;
            if (xPos >= 0 && xPos < _canvasDim && yPos >= 0 && yPos < _canvasDim)
            {

                ValidateDraw(xPos, yPos, MaxIntensity);
                //Cirlce around clicked point
                ValidateDraw(xPos + 1, yPos, MidIntensity);
                ValidateDraw(xPos - 1, yPos, MidIntensity);
                ValidateDraw(xPos, yPos + 1, MidIntensity);
                ValidateDraw(xPos, yPos - 1, MidIntensity);
                //Slight gradiant in the corners
                ValidateDraw(xPos + 1, yPos - 1, LowIntensity);
                ValidateDraw(xPos - 1, yPos - 1, LowIntensity);
                ValidateDraw(xPos + 1, yPos + 1, LowIntensity);
                ValidateDraw(xPos - 1, yPos + 1, LowIntensity);
            }
        }

        /// <summary>
        /// Grabs a line of data from training data cvs
        /// </summary>
        public void LoadLine()
        {
            _dataFile.ReadFileLine(_canvasData);

        }

        /// <summary>
        /// Gets canvas property
        /// </summary>
        public Canvas Canvas => _digitCanvas;


        /// <summary>
        /// Gets canvas size property
        /// </summary>
        public int Size { get => (int)_digitCanvas.Width; }

        /// <summary>
        /// Gets specific canvasData index value
        /// </summary>
        /// <param name="row"></param>
        /// <param name="col"></param>
        /// <returns></returns>
        public int GetValueAt(int row, int col)
        {
            return _canvasData[row, col].Value;
        }

        /// <summary>
        /// Sets specific canvasData index to a value
        /// </summary>
        /// <param name="row"></param>
        /// <param name="col"></param>
        /// <param name="value"></param>
        public void SetValueAt(int row, int col, int value)
        {
            if (value >= 0 && value <= 255)
            {
                _canvasData[row, col].Value = value;
            }
        }

        /// <summary>
        /// Gets the canvasData as a int[,] array
        /// </summary>
        /// <returns></returns>
        public float[,] GetCanvasArray()
        {
            float[,] canvasArray = new float[28, 28];
            for (int i = 0; i < _canvasDim; i++)
            {
                for (int j = 0; j < _canvasDim; j++)
                {
                    canvasArray[i, j] = (float)_canvasData[i, j].Value;
                }
            }
            return canvasArray;
        }

        /// <summary>
        /// Clears canvas
        /// </summary>
        public void ClearCanvas()
        {
            for (int i = 0; i < _canvasDim; i++)
            {
                for (int j = 0; j < _canvasDim; j++)
                {
                    _canvasData[i, j].Value = 0;
                }
            }
        }

        /// <summary>
        /// Validates that position exists and draws at specified intestity, 0 - 255 range
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="intensity"></param>
        private void ValidateDraw(int x, int y, int intensity)
        {
            if (x >= 0 && x < _canvasDim && y >= 0 && y < _canvasDim && intensity >= 0 && intensity <= 255)
            {
                _canvasData[x, y].Value += intensity;
            }
        }
    }
}
