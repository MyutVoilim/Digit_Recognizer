using System.IO;
using Microsoft.Win32;

namespace AI_Digit_Recognition
{
    /// <summary>
    /// Manages file operations such as reading, writing, and selecting files through dialog windows.
    /// </summary>
    internal class FileManager : IFileManager
    {
        private string _path;

        /// <summary>
        /// Initializes a new instance of the FileManager class.
        /// </summary>
        public FileManager()
        {

        }

        /// <summary>
        /// Initializes a new instance of the FileManager class with defined path
        /// </summary>
        /// <param name="path"></param>
        public FileManager(string path)
        {
            _path = path;
        }


        /// <summary>
        /// Appends float[] data to the file at the current path.
        /// </summary>
        /// <param name="data">The text to append to the file.</param>
        public void WriteToFile(float[] data)
        {
            if (_path == null) SelectSaveFile();
            if (_path != null)
            {
                using (StreamWriter sw = File.AppendText(_path))
                {

                    sw.WriteLine(string.Join(",", data));
                }
            }
        }

        /// <summary>
        /// Appends int[] data to the file at the current path.
        /// </summary>
        /// <param name="data">The text to append to the file.</param>
        public void WriteToFile(int[] data)
        {
            if (_path == null) SelectSaveFile();
            if (_path != null)
            {
                using (StreamWriter sw = File.AppendText(_path))
                {

                    sw.WriteLine(string.Join(",", data));
                }
            }
        }

        /// <summary>
        /// Appends string[] data to the file at the current path.
        /// </summary>
        /// <param name="data">The text to append to the file.</param>
        public void WriteToFile(string[] data)
        {
            if (_path == null) SelectSaveFile();
            if (_path != null)
            {
                using (StreamWriter sw = File.AppendText(_path))
                {

                    sw.WriteLine(string.Join(",", data));
                }
            }
        }

        /// <summary>
        /// Reads all lines from the file at the current path.
        /// </summary>
        /// <returns>An array of strings containing all lines from the file.</returns>
        public string[] ReadAllLines()
        {
            if (_path == null) SelectFile();
            return _path != null ? File.ReadAllLines(_path) : null;
        }

        /// <summary>
        /// Defines a path to use
        /// </summary>
        /// <param name="path"></param>
        public void DefinePath(string path)
        {
            _path= path;
        }

        /// <summary>
        /// Opens a dialog to select a file and sets it to _path
        /// </summary>
        public void SelectFile()
        {
            OpenFileDialog openFileDialog = new OpenFileDialog
            {
                Filter = "Text Files (*.txt)|*.txt|All Files (*.*)|*.*",
                Title = "Select a File"
            };

            _path = openFileDialog.ShowDialog() == true ? openFileDialog.FileName : null;
        }



        /// <summary>
        /// Opens a dialog to save a file and returns the selected file's path.
        /// </summary>
        /// <returns>The path of the file to be saved or null if no location is selected.</returns>
        public void SelectSaveFile()
        {
            SaveFileDialog saveFileDialog = new SaveFileDialog
            {
                Filter = "Text Files (*.txt)|*.txt|All Files (*.*)|*.*",
                Title = "Save a File"
            };

            _path =  saveFileDialog.ShowDialog() == true ? saveFileDialog.FileName : null;
        }

        /// <summary>
        /// Clears a file of all text
        /// </summary>
        public void ClearFile()
        {
            File.WriteAllText(_path, string.Empty);
        }

        /// <summary>
        /// Checks to see of a _path exists
        /// </summary>
        /// <returns></returns>
        public bool DoesPathExist()
        {
            return (_path != null)? true: false;
        }

    }
}
