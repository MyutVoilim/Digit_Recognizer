using AI_Digit_Recognition;

namespace AI_Digit_Recognition_Test

{
    [TestClass]
    public class AIDigitModelTest
    {
        public TestContext TestContext { get; set; }

        private AIDigitModel _model;
        private IFileManager _fileManager;
        private string _defaultModelFilePath;
        private string _testTrainingFilePath;
        private string _defaultSaveFilePath;
        private string[] _trainingData;
        private float[] _firstNumberTrainingData;


        [TestInitialize]
        public void Initialize()
        {
            // Common setup for all tests
            _fileManager = new FileManager();

            // Set paths to used files
            _defaultModelFilePath = @"../../../Data/defaultTestingModel.txt";
            _defaultSaveFilePath = @"../../../Data/defaultSaveFile.txt";
            _testTrainingFilePath = @"../../../Data/testTrainingFile.csv";

            // Get training data
            _fileManager.DefinePath(_testTrainingFilePath);
            _trainingData = _fileManager.ReadAllLines();

            // Converts the first number of the training data into useable format for model
            _firstNumberTrainingData = _trainingData[1].Split(",").Select(float.Parse).ToArray();


            // Initialize model with the default model
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

            CollectionAssert.AreEqual(defaultLoadModelStringData, loadedModelStringData, "The loaded model content does not match the default model content.");
        }

        [TestMethod]
        public void SaveToFile_CorrectSavingTest()
        {
            // Clear file to be saved to for fresh save
            _fileManager.DefinePath(_defaultSaveFilePath);
            _fileManager.ClearFile();

            // Save model to file
            _model.SaveToFile(_defaultSaveFilePath);

            // Compare default model to the saved model to test for accurate saving functionality
            string[] saveFileStringData = _fileManager.ReadAllLines();

            // Change path and get string data from defaut model file
            _fileManager.DefinePath(_defaultModelFilePath);
            string[] loadFileStringData = _fileManager.ReadAllLines();

            CollectionAssert.AreEqual(loadFileStringData, saveFileStringData, "The saved file content does not match the original content.");
        }

        [TestMethod]
        public void ProcessInput_CorrectOutput()
        {
            // Process and get output of first number in training data
            _model.ProcessInput(_firstNumberTrainingData.Skip(1).ToArray());
            int[] testOutput = _model.GetOutputValues();
            // Known output for first line of training data
            int[] knownOutput = new int[10] { 7, 91, 96, 76, 94, 38, 67, 80, 51, 98 };
            CollectionAssert.AreEqual(knownOutput, testOutput, "Processing and output of data was not equal to known output");
        }

        [TestMethod]
        public void CheckGuessedValueAccuracy()
        {
            // Process the first number in training data
            _model.ProcessInput(_firstNumberTrainingData.Skip(1).ToArray());

            // Get the guessed value based on output of processed data
            int guessedValue = _model.GetGuessedValue();
            int knownGuessedValue = 9;

            Assert.AreEqual(knownGuessedValue, guessedValue, "Correct value was the guessed");
        }

        [TestMethod]
        public async Task CheckTrainingAccuracy()
        {
            var _cancellationTokenSource = new CancellationTokenSource();
            var progress = new Progress<float[]>();
            await _model.Train(.1f, 1, progress, _cancellationTokenSource.Token, _testTrainingFilePath);
            int[] testOutput = _model.GetOutputValues();
            int[] knownTestOutput = new int[10] { 28, 63, 41, 30, 60, 14, 30, 42, 57, 97 };

            CollectionAssert.AreEqual(knownTestOutput, testOutput, "The Training output is not equal to the expected output");
        }

        [TestMethod]
        public void CheckTrainingData_CorrectlyLoaded()
        {
            // Get positions for the first position of each line to compare loaded data to known data
            string[] knownTrainingData = new string[10] { "label", "1", "0", "1", "4", "0", "0", "7", "3", "5" };
            string[] firstPosTrainingData = new string[10];

            // Store the first positions of each line
            for (int i = 0; i < knownTrainingData.Length; i++) {
                firstPosTrainingData[i] = _trainingData[i].Split(",")[0];
            }

            CollectionAssert.AreEqual(firstPosTrainingData, knownTrainingData, "The loaded training data and known training data are not equal");
        }

    }
}