using System;
using System.Collections.Generic;
using System.Speech.Recognition;
using System.Diagnostics;
using System.Speech.Synthesis;
using System.Threading;
using System.Text;

namespace TestingPanel
{
    internal class Program
    {
        static bool start = true;
        static Dictionary<string, System.Diagnostics.Process> openedProcesses = new Dictionary<string, System.Diagnostics.Process>();
        private static bool amorKeywordRecognized = false;
        private static System.Speech.Synthesis.SpeechSynthesizer synthesizer = new System.Speech.Synthesis.SpeechSynthesizer();
        private static ManualResetEvent stopRecognition = new ManualResetEvent(false); // Declare the ManualResetEvent
        static string languageConfig = "en-US";

        private static SpeechRecognitionEngine speechRecognizer = new SpeechRecognitionEngine();
        private static SpeechRecognitionEngine searchRecognizer = new SpeechRecognitionEngine();
        static void Main(string[] args)
        {
            Console.OutputEncoding = Encoding.UTF8;

            // Initialize the speechRecognizer and attach the recognition event handler
            speechRecognizer.SetInputToDefaultAudioDevice();
            speechRecognizer.SpeechRecognized += new EventHandler<SpeechRecognizedEventArgs>(Recognizer_SpeechRecognized);

            searchRecognizer.SetInputToDefaultAudioDevice();
            searchRecognizer.LoadGrammar(new DictationGrammar());

            // Load the custom grammar for specific commands
            LoadCustomGrammar();

            Console.WriteLine("Listening for commands...");
            Console.WriteLine("Say 'exit' to stop the program.");

            // Start asynchronous speech recognition
            speechRecognizer.RecognizeAsync(RecognizeMode.Multiple);

            // Wait until "exit" command is spoken
            stopRecognition.WaitOne();
        }

        

        private static void LoadCustomGrammar()
        {
            // Create a Choices object to hold the list of commands
            Choices commands = new Choices();
            commands.Add(new string[]
            {
                "hello",
                "amor",
                "open dota 2",
                "open genshin",
                "open chrome",
                "search",
                "status",
                "speak language",
                "close dota 2",
                "close genshin",
                "close chrome",
                "exit"
            });

            GrammarBuilder gb = new GrammarBuilder();
            gb.Append(commands);

            // Create a Grammar object and load it into the recognizer
            Grammar grammar = new Grammar(gb);
            speechRecognizer.LoadGrammar(grammar);
        }

        private static void Recognizer_SpeechRecognized(object sender, SpeechRecognizedEventArgs e)
        {
            string recognizedText = e.Result.Text;
            Console.WriteLine($"Recognized: {recognizedText}");
            // Your code to process the recognized text and execute actions based on commands
            // Use switch to handle multiple commands
            string filePath;
            if (!amorKeywordRecognized)
            {
                if (recognizedText.ToLower().Equals("amor"))
                {
                    amorKeywordRecognized = true;
                    SpeakText("Yes my lord?");
                    Console.WriteLine("Amor keyword recognized. You can now start giving commands.");
                }
                else
                {
                    Console.WriteLine("Waiting for the keyword 'amor' to be recognized...");
                }
            }
            else
            {
                switch (recognizedText.ToLower())
                {
                    case "hello":
                        SpeakText("Hello there!");
                        Console.WriteLine("Hello there!");
                        break;
                    case "open dota 2":
                        // Replace "C:\\Path\\To\\Your\\File.txt" with the full path of the file you want to open  
                        // Replace "C:\\Path\\To\\Your\\File.txt" with the full path of the Dota 2 executable
                        string dota2ExecutablePath = @"D:\Program Files (x86)\Steam\steamapps\common\dota 2 beta\game\bin\win64\dota2.exe";
                        string launchOptions = "-novid -console -high -gamestateintegration -condebug"; // Replace with your desired launch options

                        try
                        {
                            System.Diagnostics.ProcessStartInfo startInfo = new System.Diagnostics.ProcessStartInfo
                            {
                                FileName = dota2ExecutablePath,
                                Arguments = launchOptions,
                                UseShellExecute = false
                            };

                            System.Diagnostics.Process fileProcess = System.Diagnostics.Process.Start(startInfo);
                            if (fileProcess != null)
                            {
                                // Store the process in the dictionary with an identifier (e.g., "file")
                                openedProcesses["dota2"] = fileProcess;
                                SpeakText("Dota 2 is opening...");
                                Console.WriteLine("Dota 2 is opening...");
                            }
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"Error opening file: {ex.Message}");
                        }
                        break;
                    case "open genshin":
                        // Replace "C:\\Path\\To\\Your\\File.txt" with the full path of the file you want to open  
                        // Replace "C:\\Path\\To\\Your\\File.txt" with the full path of the Dota 2 executable
                        filePath = @"D:\Program Files\Genshin Impact\launcher.exe";

                        try
                        {
                            System.Diagnostics.ProcessStartInfo startInfo = new System.Diagnostics.ProcessStartInfo
                            {
                                FileName = filePath,
                            };

                            System.Diagnostics.Process fileProcess = System.Diagnostics.Process.Start(startInfo);
                            if (fileProcess != null)
                            {
                                // Store the process in the dictionary with an identifier (e.g., "file")
                                openedProcesses["genshin"] = fileProcess;
                                SpeakText("Genshin is opening...");
                                Console.WriteLine("Genshin is opening...");
                            }
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"Error opening file: {ex.Message}");
                        }
                        break;
                    case "open chrome":
                        // Replace "C:\\Path\\To\\Your\\File.txt" with the full path of the file you want to open
                        filePath = @"C:\Program Files\Google\Chrome\Application\chrome.exe";
                        try
                        {
                            System.Diagnostics.Process fileProcess = System.Diagnostics.Process.Start(filePath);
                            if (fileProcess != null)
                            {
                                // Store the process in the dictionary with an identifier (e.g., "file")
                                openedProcesses["chrome"] = fileProcess;
                                SpeakText("Chrome is opening...");
                                Console.WriteLine("Chrome is opening...");
                            }
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"Error opening file: {ex.Message}");
                        }
                        break;
                    case "search":
                        SpeakText("What do you want to search for?");
                        Console.WriteLine("What do you want to search for?");
                        // Use speech recognition to capture the search query from your voice
                        string searchQuery = CaptureVoiceInput();
                        if (!string.IsNullOrWhiteSpace(searchQuery))
                        {
                            // Perform a Google search by opening the default web browser
                            // and navigating to the Google search URL with the search query
                            string googleSearchUrl = "https://www.google.com/search?q=" + Uri.EscapeDataString(searchQuery);
                            System.Diagnostics.Process.Start(googleSearchUrl);
                        }
                        break;
                    case "status":
                        ShowProgramStatus();
                        break;
                    case "speak language":
                        SpeakText("Change program language");
                        string request = CaptureVoiceInput();
                        request = request.ToLower().Trim().TrimEnd('.');
                        if (request == null)
                        {
                            return;
                        }
                        if (request == "russian")
                        {
                            languageConfig = "ru-RU";
                            Console.WriteLine("Commands now are in Russian");
                            SpeakText("Commands now are in Russian");
                        }
                        if (request == "english")
                        {
                            languageConfig = "en-US";
                            Console.WriteLine("Commands now are in English");
                            SpeakText("Commands now are in English");
                        }
                        break;
                    case "close dota 2":
                        SpeakText("Closing Dota 2...");
                        CloseSpecificApp("dota2");
                        break;
                    case "close genshin":
                        SpeakText("Closing Genshin...");
                        CloseSpecificApp("genshin");
                        break;
                    case "close chrome":
                        SpeakText("Closing Chrome...");
                        CloseSpecificApp("chrome");
                        break;
                    case "exit":
                        SpeakText("Closing the program...");
                        Console.WriteLine("Closing the program...");
                        stopRecognition.Set();
                        start = false; // Stop recognition and exit the program
                        break;
                    default:
                        Console.WriteLine("Unrecognized command.");
                        amorKeywordRecognized = !amorKeywordRecognized;
                        break;
                }
                amorKeywordRecognized = !amorKeywordRecognized;
            }
        }

        private static string CaptureVoiceInput()
        {
            // Process recognition result
            RecognitionResult result = searchRecognizer.Recognize();
            if (result != null && result.Text != null)
            {
                string recognizedText = result.Text;
                Console.WriteLine($"Recognized: {recognizedText}");
                return recognizedText;
            }

            return null;
        }

        private static void ShowProgramStatus()
        {
            Console.WriteLine("Opened Programs:");
            Console.WriteLine(languageConfig);
            SpeakText("Opened Programs:");
            foreach (var kvp in openedProcesses)
            {
                string programName = kvp.Key;
                bool isRunning = !kvp.Value.HasExited;
                Console.WriteLine($"- {programName}: {(isRunning ? "Running" : "Exited")}");
            }
        }

        private static void CloseSpecificApp(string identifier)
        {
            if (openedProcesses.TryGetValue(identifier, out System.Diagnostics.Process appProcess))
            {
                if (!appProcess.HasExited)
                {
                    // Close the specific app using CloseMainWindow
                    appProcess.CloseMainWindow();
                    openedProcesses.Remove(identifier);
                    Console.WriteLine($"Closed {identifier}.");
                }
                else
                {
                    Console.WriteLine($"App with identifier '{identifier}' has already exited.");
                }
            }
            else
            {
                Console.WriteLine($"App with identifier '{identifier}' not found or already closed.");
            }
        }

        private static void SpeakText(string text)
        {
            synthesizer.SelectVoiceByHints(VoiceGender.Female);
            synthesizer.Speak(text);
        }
    }
}
