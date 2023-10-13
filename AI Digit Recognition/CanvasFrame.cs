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
        //Grayscale values between 0 and 255 for a x * y grid
        //private int[,] _canvasData;
        private CanvasData[,] _canvasData = new CanvasData[_canvasDim, _canvasDim];
        //private int _rows, _cols = 0;
        private Canvas _digitCanvas;
        //Size of canvas in pixels
        private const int _canvasSize = 280;
        //The about of units across the x and y axis
        private const int _canvasDim = 28;
        //The size of individual blocks withing the grid relative to the canvas size
        private const int _blockSize = _canvasSize / _canvasDim;

        private Random rnd = new Random();

        //Default with value of 0 for grid of 28 x 28
        public CanvasFrame(Canvas digitCanvas)
        {
            _digitCanvas = digitCanvas;
            _digitCanvas.Background = Brushes.Blue;
            _digitCanvas.Width = _canvasSize;
            _digitCanvas.Height = _canvasSize;
            //_canvasData = canvasData.Data;
            CreateGrid();
        }

        //Creates grid lines for the canvas based on the target rows and columns
        private void CreateGrid()
        {
            for (int row = 0; row < _canvasDim; row++)
            {
                for (int col = 0; col < _canvasDim; col++)
                {
                    Rectangle block = new Rectangle();
                    block.Width = _blockSize;
                    block.Height = _blockSize;
                    _canvasData[row, col] = new CanvasData { Value = 0 };
                    block.DataContext = _canvasData[row, col];
                    Binding binding = new Binding("Value");
                    binding.Converter = new IntToGrayscaleBrushConverter();
                    block.SetBinding(Rectangle.FillProperty, binding);
                   // block.Fill = new SolidColorBrush(Color.FromRgb((byte)rnd.Next(0, 255), (byte)rnd.Next(0, 255), (byte)rnd.Next(0, 255)));
                    _digitCanvas.Children.Add(block);
                    Canvas.SetLeft(block, (row * _blockSize));
                    Canvas.SetTop(block, (col * _blockSize));
                }
            }
        }

        public Canvas GetCanvas()
        {
            return _digitCanvas;
        }
        public int GetCanvasSize()
        {
            return _canvasSize;
        }

        public void changeValue(int value)
        {
            _canvasData[0, 10].Value = value;
        }
    }
    public class IntToGrayscaleBrushConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is int intValue && intValue >= 0 && intValue <= 255)
            {
                byte byteValue = (byte)intValue;
                return new SolidColorBrush(Color.FromRgb(byteValue, byteValue, byteValue));
            }
            return DependencyProperty.UnsetValue;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
