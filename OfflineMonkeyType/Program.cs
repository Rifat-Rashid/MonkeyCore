using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;

class Program
{
    static int consoleWidth;
    static int consoleHeight;
    static string[] words;
    static List<string> typedWords;
    static StringBuilder currentWord;
    static int currentWordIndex;
    static bool[] wordCorrectness;
    static Stopwatch stopwatch;
    static int timerSeconds = 30;
    static int cursorLeft = 0;
    static int typingStartRow;

    static void Main(string[] args)
    {
        Console.CursorVisible = false;
        Console.Clear();
        InitializeConsole();
        InitializeGame();

        DrawWelcomeScreen();
        Console.ReadKey(true);

        RunGame(); // t

        DisplayResults();
        Console.ReadKey(true);
    }

    static void InitializeConsole()
    {
        consoleWidth = Console.WindowWidth;
        consoleHeight = Console.WindowHeight;
        Console.SetBufferSize(consoleWidth, consoleHeight);
    }

    static void InitializeGame()
    {
        words = "the quick brown fox jumps over the lazy dog pack my box with five dozen liquor jugs".Split();
        typedWords = new List<string>();
        currentWord = new StringBuilder();
        currentWordIndex = 0;
        wordCorrectness = new bool[words.Length];
        stopwatch = new Stopwatch();
    }

    static void DrawWelcomeScreen()
    {
        Console.Clear();
        string title = "Welcome to MonkeyType Emulator!";
        Console.SetCursorPosition((consoleWidth - title.Length) / 2, consoleHeight / 2 - 2);
        Console.WriteLine(title);
        string instruction = "Press any key to start typing...";
        Console.SetCursorPosition((consoleWidth - instruction.Length) / 2, consoleHeight / 2);
        Console.WriteLine(instruction);
    }

    static void RunGame()
    {
        Console.Clear();
        DrawStaticUI();

        UpdateDynamicUI();
        Debug.Write("Debug: Initial UI update complete");
        stopwatch.Start();

        while (stopwatch.Elapsed < TimeSpan.FromSeconds(timerSeconds) && currentWordIndex < words.Length)
        {
            UpdateDynamicUI();

            if (Console.KeyAvailable)
            {
                HandleKeyPress(Console.ReadKey(true));
            }

            Thread.Sleep(10); // Small delay to reduce CPU usage
        }
    }

    static void DrawStaticUI()
    {
        // Draw border
        for (int i = 0; i < consoleWidth; i++)
        {
            Console.SetCursorPosition(i, 0);
            Console.Write("─");
            Console.SetCursorPosition(i, consoleHeight - 1);
            Console.Write("─");
        }
        for (int i = 1; i < consoleHeight - 1; i++)
        {
            Console.SetCursorPosition(0, i);
            Console.Write("│");
            Console.SetCursorPosition(consoleWidth - 1, i);
            Console.Write("│");
        }

        // Draw corners
        Console.SetCursorPosition(0, 0);
        Console.Write("┌");
        Console.SetCursorPosition(consoleWidth - 1, 0);
        Console.Write("┐");
        Console.SetCursorPosition(0, consoleHeight - 1);
        Console.Write("└");
        Console.SetCursorPosition(consoleWidth - 1, consoleHeight - 1);
        Console.Write("┘");

        // Draw title
        string title = "MonkeyType Emulator";
        Console.SetCursorPosition((consoleWidth - title.Length) / 2, 1);
        Console.Write(title);

        typingStartRow = 5;
    }

    static void UpdateDynamicUI()
    {
        // Update timer
        int remainingSeconds = timerSeconds - (int)stopwatch.Elapsed.TotalSeconds;
        string timer = $"Time: {remainingSeconds:D2}s";
        Console.SetCursorPosition(consoleWidth - timer.Length - 2, 1);
        Console.Write(timer);

        // Update words
        DisplayWords();

        // Update stats
        UpdateStats();
    }

    static void DisplayWords()
    {
        int currentRow = typingStartRow;
        int currentCol = 2;
        Console.SetCursorPosition(currentCol, currentRow);

        for (int i = 0; i < words.Length; i++)
        {
            string wordToDisplay = i < currentWordIndex ? typedWords[i] :
                                   i == currentWordIndex ? words[i] :
                                   words[i];
            Debug.WriteLine($"Debug: Word {i}: '{wordToDisplay}', currentCol: {currentCol}");

            if (currentCol + wordToDisplay.Length + 1 >= consoleWidth - 2)
            {
                currentRow++;
                currentCol = 2;
                Console.SetCursorPosition(currentCol, currentRow);
            }

            if (i < currentWordIndex)
            {
                Console.ForegroundColor = wordCorrectness[i] ? ConsoleColor.Green : ConsoleColor.Red;
            }
            else if (i == currentWordIndex)
            {
                Console.ForegroundColor = ConsoleColor.Blue;
            }
            else
            {
                Console.ForegroundColor = ConsoleColor.Gray;
            }

            Console.Write(wordToDisplay);
            Console.Write(" ");

            currentCol += wordToDisplay.Length + 1;
        }

        Console.ForegroundColor = ConsoleColor.Gray;
    }

    static void UpdateStats()
    {
        int totalWords = Math.Min(typedWords.Count, words.Length);
        int correctWords = wordCorrectness.Take(totalWords).Count(w => w);
        double accuracy = totalWords > 0 ? (double)correctWords / totalWords * 100 : 0;
        double wpm = stopwatch.Elapsed.TotalMinutes > 0 ? typedWords.Count / stopwatch.Elapsed.TotalMinutes : 0;

        string statsLine = $"Words: {totalWords} | Accuracy: {accuracy:F2}% | WPM: {wpm:F2}";
        Console.SetCursorPosition((consoleWidth - statsLine.Length) / 2, consoleHeight - 2);
        Console.Write(statsLine);
    }

    static void HandleKeyPress(ConsoleKeyInfo key)
    {
        if (key.Key == ConsoleKey.Spacebar)
        {
            if (currentWord.Length > 0)
            {
                typedWords.Add(currentWord.ToString());
                wordCorrectness[currentWordIndex] = (currentWord.ToString() == words[currentWordIndex]);
                Debug.WriteLine("current word: " + currentWord.ToString() + " checking against: " + words[currentWordIndex]);
                currentWordIndex++;
                currentWord.Clear();
                cursorLeft = 0;
            }
        }
        else if (key.Key == ConsoleKey.Backspace)
        {
            if (currentWord.Length > 0)
            {
                currentWord.Remove(currentWord.Length - 1, 1);
                cursorLeft = Math.Max(0, cursorLeft - 1);
            }
            else if (typedWords.Count > 0 && currentWordIndex > 0)
            {
                currentWordIndex--;
                currentWord.Append(typedWords[typedWords.Count - 1]);
                typedWords.RemoveAt(typedWords.Count - 1);
                cursorLeft = currentWord.Length;
            }
        }
        else if (!char.IsControl(key.KeyChar))
        {
            currentWord.Append(key.KeyChar);
            cursorLeft++;
        }
    }

    static void DisplayResults()
    {
        Console.Clear();
        DrawStaticUI();

        int totalWords = Math.Min(typedWords.Count, words.Length);
        int correctWords = wordCorrectness.Take(totalWords).Count(w => w);
        double accuracy = totalWords > 0 ? (double)correctWords / totalWords * 100 : 0;
        double wpm = stopwatch.Elapsed.TotalMinutes > 0 ? typedWords.Count / stopwatch.Elapsed.TotalMinutes : 0;

        string[] results = {
            "Time's up!",
            $"Words typed: {totalWords}",
            $"Correct words: {correctWords}",
            $"Accuracy: {accuracy:F2}%",
            $"Words per minute: {wpm:F2}"
        };

        for (int i = 0; i < results.Length; i++)
        {
            Console.SetCursorPosition((consoleWidth - results[i].Length) / 2, typingStartRow + i);
            Console.WriteLine(results[i]);
        }

        string exitMessage = "Press any key to exit...";
        Console.SetCursorPosition((consoleWidth - exitMessage.Length) / 2, consoleHeight - 3);
        Console.Write(exitMessage);
    }
}
