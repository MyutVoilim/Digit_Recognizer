using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows.Shapes;

namespace AI_Digit_Recognition
{
    internal class CanvasFrame
    {
        LoadFile _dataFile;
        //Holds a INotify property that will later bind with the canvas to update children
        private CanvasData[,] _canvasData;
        //Canvas that will be displayed
        private Canvas _digitCanvas;
        //Size of canvas in pixels, assums canvas is sqaure
        private int _canvasSize;
        //The about of units across the x and y axis
        private int _canvasDim;
        //The size of individual blocks withing the grid relative to the canvas size
        private int _blockSize;

        //Default with value of 0 for grid of 28 x 28
        public CanvasFrame(Canvas digitCanvas, int CanvasDim, string file)
        {
            _canvasData = new CanvasData[CanvasDim, CanvasDim];
            _digitCanvas = digitCanvas;
            _canvasDim = CanvasDim;
            _canvasSize = (int) digitCanvas.Width;
            _blockSize = (int)(_canvasSize / CanvasDim);
            _dataFile = new LoadFile(file);
            CreateGrid();
        }

        //Creates a grid based on _blockSize that is fills on the canvas  as rectangles and then Binds the respective values to
        //the positions in the _canvasData, The Grid will automatically update when values are changed in _canvasData
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

        public void DrawOnGrid(int x, int y)
        {
            int xPos = x / _blockSize;
            int yPos = y / _blockSize;
            if (xPos >= 0 && xPos < _canvasDim && yPos >= 0 && yPos < _canvasDim)
            {
                _canvasData[xPos, yPos].Value = 255;
            }
        }

        public void LoadLine() 
        {
            _dataFile.ReadFile(_canvasData);

        }

        public Canvas Canvas { get { return _digitCanvas; } }
        public int Size { get { return _canvasSize; } }

        public int GetValueAt(int row, int col)
        {
            return _canvasData[row, col].Value;
        }
        public void SetValueAt(int row, int col, int value)
        {
            _canvasData[row,col].Value = value;
        }
        public float[,] getCanvasArray()
        {
            float[,] canvasArray = new float[28,28];
            for (int i = 0; i < 28; i++)
            {
                for (int j = 0; j < 28; j++)
                {
                    canvasArray[i,j] = (float)_canvasData[i,j].Value; 
                }
            }
            return canvasArray;
        }

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
    }
}
