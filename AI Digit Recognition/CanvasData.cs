using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;

namespace AI_Digit_Recognition
{
    internal class CanvasData : INotifyPropertyChanged
    {
        /*
        private int[,] _data = new int[28, 28];

        public CanvasData(int[,] data)
        {
            Data = data;
        }

        public CanvasData()
        {
            for (int i = 0; i < Data.GetLength(0); i++)
            {
                for (int z = 0; z < _data.GetLength(1); z++)
                {
                    Data[i, z] = 0;
                }
            }
        }

        public int[,] Data
        {
            get { return _data; }
            set { _data = value; }
        }
        */
        private int _value;

        public int Value
        {
            get { return _value; }
            set
            {
                if (_value != value)
                {
                    _value = value;
                    OnPropertyChanged(nameof(Value));
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
