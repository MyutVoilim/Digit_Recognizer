using System;
using System.ComponentModel;

namespace AI_Digit_Recognition
{

    /// <summary>
    /// Represents the color intensity data for an element in a canvas
    /// This class provides property change notifications to enable UI updates in response to data changes.
    /// </summary>
    internal class CanvasData : INotifyPropertyChanged
    {
        private int _colorIntensity;


        /// <summary>
        /// Gets or sets the color intensity value of the canvas element. 
        /// The value ranges from 0 (black) to 255 (white), representing the intensity of the color.
        /// Changes to this property trigger a notification to any listeners.
        /// </summary>
        public int ColorIntensity
        {
            get { return _colorIntensity; }
            set
            {
                if (_colorIntensity != value)
                {
                    // Restrict min and max values
                    _colorIntensity = Math.Clamp(value, 0, 255);
                    OnPropertyChanged(nameof(ColorIntensity));
                }
            }
        }

        /// <summary>
        /// Occurs when a property value changes.
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// Invokes the PropertyChanged event for the specified property.
        /// This method is used to notify the UI or other components when a property value has changed.
        /// </summary>
        /// <param name="propertyName">The name of the property that changed.</param>
        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
