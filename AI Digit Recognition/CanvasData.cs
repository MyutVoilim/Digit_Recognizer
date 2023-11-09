using System.ComponentModel;

namespace AI_Digit_Recognition
{

    internal class CanvasData : INotifyPropertyChanged
    {
        private int _value;


        /// <summary>
        /// Represents data for canvas child with a single integer value that notifies listeners of changes.
        /// </summary>
        public int Value
        {
            get { return _value; }
            set
            {
                if (_value != value)
                {
                    _value = value;
                    if (_value > 255) _value = 255;
                    OnPropertyChanged(nameof(Value));
                }
            }
        }

        /// <summary>
        /// Informs when property is changed
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// Raises the PropertyChagned Event for a given property
        /// </summary>
        /// <param name="propertyName"></param>
        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
