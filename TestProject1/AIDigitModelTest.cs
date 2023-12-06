using AI_Digit_Recognition;
using Microsoft.Extensions.Configuration;

namespace AI_Digit_Recognition_Test

{
    [TestClass]
    public class AIDigitModelTest
    {
        private AIDigitModel _model;
        private IFileManager _fileManager;
        private string _defaultModelFilePath;
        private string _testTrainingFilePath;

        [TestInitialize]
        public void Initialize()
        {
            // Common setup for all tests
            _fileManager = new FileManager();
            _defaultModelFilePath = @"../../../Data/defaultTestingModel.txt";
            _testTrainingFilePath = @"../../../Data/testTrainingFile.csv";

            // Initialize model with the default file
            _model = new AIDigitModel(_defaultModelFilePath, _fileManager);

        }

        [TestMethod]
        public void LoadFromFile_AccurateLoadingTest()
        {
            // Get data from currently loaded model string data
            string[] loadedModelStringData = _fileManager.ReadAllLines();

            // Get data from default model string data
            _fileManager.DefinePath(_defaultModelFilePath);
            string[] defaultLoadModelStringData = _fileManager.ReadAllLines();

            CollectionAssert.AreEqual(loadedModelStringData, defaultLoadModelStringData, "The loaded model content does not match the default model content.");
        }

        [TestMethod]
        public void SaveToFile_CorrectSavingTest()
        {
            // Clear file to be saved to for fresh save
            _fileManager.DefinePath(@"../../../Data/defaultSaveFile.txt");
            _fileManager.ClearFile();

            // Save model to file
            _model.SaveToFile(@"../../../Data/defaultSaveFile.txt");

            // Compare default model to the saved model to test for accurate saving functionality
            string[] saveFileStringData = _fileManager.ReadAllLines();

            // Change path and get string data from defaut model file
            _fileManager.DefinePath(@"../../../Data/defaultTestingModel.txt");
            string[] loadFileStringData = _fileManager.ReadAllLines();

            CollectionAssert.AreEqual(saveFileStringData, loadFileStringData, "The saved file content does not match the original content.");
        }
    }
}