using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AI_Digit_Recognition
{
    /// <summary>
    /// Defines the contract for file operations such as reading, writing, and selecting files.
    /// </summary>
    public interface IFileManager
    {
        /// <summary>
        /// Appends float array data to a file.
        /// </summary>
        /// <param name="data">Data to append to the file.</param>
        void WriteToFile(float[] data);

        /// <summary>
        /// Appends integer array data to a file.
        /// </summary>
        /// <param name="data">Data to append to the file.</param>
        void WriteToFile(int[] data);

        /// <summary>
        /// Appends string array data to a file.
        /// </summary>
        /// <param name="data">Data to append to the file.</param>
        void WriteToFile(string[] data);

        /// <summary>
        /// Reads all lines from the file at the current path.
        /// </summary>
        /// <returns>An array of strings containing all lines from the file.</returns>
        string[] ReadAllLines();

        /// <summary>
        /// Sets the path for file operations.
        /// </summary>
        /// <param name="path">The file path to use.</param>
        void DefinePath(string path);

        /// <summary>
        /// Opens a dialog to select a file for operations.
        /// </summary>
        void SelectFile();

        /// <summary>
        /// Opens a dialog to save a file and set its path for operations.
        /// </summary>
        void SelectSaveFile();

        /// <summary>
        /// Clears the contents of the file at the current path.
        /// </summary>
        void ClearFile();

        /// <summary>
        /// Checks to see of a path exists
        /// </summary>
        /// <returns></returns>
        bool DoesPathExist();

        /// <summary>
        /// Returns the current path
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        string GetPath();
    }
}
