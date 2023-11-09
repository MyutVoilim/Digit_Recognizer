using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using Microsoft.Win32;

namespace AI_Digit_Recognition
{
    internal class LoadFile
    {
        private string _path;
        private int _currentDigit = 1; // Keeps track of the current line

        public LoadFile(string path)
        {
            _path = path;
        }

        /// <summary>
        /// Reads one line from file and then increments to read the next line when called
        /// </summary>
        /// <param name="trainingData"></param>
        public void ReadFileLine(CanvasData[,] trainingData)
        {
            using (StreamReader reader = new StreamReader(_path))
            {
                for (int i = 0; i < _currentDigit; i++)
                {
                    if (!reader.EndOfStream)
                    {
                        reader.ReadLine();
                    }
                }
                _currentDigit++;
                try
                {
                    string[] line = reader.ReadLine().Split(',');
                    for (int i = 1; i < line.Length; i++)
                    {
                        trainingData[i % 28, i / 28].Value = int.Parse(line[i]);
                    }
                } catch(Exception ex)
                {
                    Console.WriteLine("Error");
                }
            }
        }

        /// <summary>
        /// Gets all lines from a file as a string[]
        /// </summary>
        /// <returns></returns>
        public string[] ReadAllLines()
        {
            string[] allLines = File.ReadAllLines(_path);
            return allLines;
        }

        /// <summary>
        /// Opens dialog to select a file
        /// </summary>
        public void SelectFile()
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "Text Files (*.txt)|*.txt|All Files (*.*)|*.*";
            openFileDialog.Title = "Select a File";

            bool? result = openFileDialog.ShowDialog();
            if (result.HasValue && result.Value) _path = openFileDialog.FileName;
        }
    }

}
