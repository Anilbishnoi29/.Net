
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Security.Principal;

namespace ProgressBar
{
    class Program
    {
        private const int STD_INPUT_HANDLE = -10;
        private const int ENABLE_QUICK_EDIT_MODE = 0x0040;

        static string WhereToRead = string.Empty;
        static string WhatToSearch = string.Empty;
        static string WhereToPutResult = string.Empty;
        static int UserInputCount = 0;
        static bool SearchAgain = false;
        static bool ContinueToSearch = true;
        static bool IsWhereToReadFile = false;
        static string CloseTheApp = string.Empty;
        static bool ReScan = false;
        static Dictionary<string, string> PreviousSearch = new Dictionary<string, string>();
        static DateTime SearchingTime;
        static HashSet<string> matchedFile = new HashSet<string>();

        [DllImport("kernel32.dll")]
        private static extern IntPtr GetStdHandle(int nStdHandle);

        [DllImport("kernel32.dll")]
        private static extern bool GetConsoleMode(IntPtr hConsoleHandle, out int lpMode);

        [DllImport("kernel32.dll")]
        private static extern bool SetConsoleMode(IntPtr hConsoleHandle, int dwMode);

        static void Main(string[] args)
        {
            try
            {
                Console.SetWindowSize(150, 30);
                IntPtr consoleHandle = GetStdHandle(STD_INPUT_HANDLE);
                int consoleMode;

                if (!IsAdministrator())
                {
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.WriteLine("Please start the application in 'Admin' mode...");
                    Console.ReadKey();
                    return;
                }
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine(@" +-+-+-+-+-+-+-+-+ +-+-+-+-+-+-+ +-+-+-+-+-+-+-+-+-+
 |D|o|c|u|m|e|n|t| |S|e|a|r|c|h| |A|s|s|i|s|t|a|n|t|
 +-+-+-+-+-+-+-+-+ +-+-+-+-+-+-+ +-+-+-+-+-+-+-+-+-+");
                Console.WriteLine();
                bool isUserProvideAllTheInput = GetUserInput();
                if (isUserProvideAllTheInput)
                {
                    if (GetConsoleMode(consoleHandle, out consoleMode))
                    {
                        // Disable Quick Edit mode
                        SetConsoleMode(consoleHandle, consoleMode & ~ENABLE_QUICK_EDIT_MODE);
                    }
                    while (ContinueToSearch)
                    {
                        if (SearchAgain) GetUserInput();

                        if (PreviousSearch.ContainsKey(WhatToSearch))
                        {
                            string previousFile = Path.Combine(PreviousSearch[WhatToSearch], "");
                            System.Threading.Thread.Sleep(2000);
                            Console.WriteLine("# Check the output file for '" + WhatToSearch + "' this keyword already searched: (" + previousFile + ")");
                            System.Diagnostics.Process.Start(previousFile);
                            //return;
                            Console.WriteLine();
                            Console.Write("# If you want to rescan files press 'r' otherwise press Any Key:");
                            Console.ForegroundColor = ConsoleColor.Yellow;
                            Console.Write("~");
                            Console.ForegroundColor = ConsoleColor.Green;
                            Console.Write("$ ");
                            Console.ResetColor();
                            string rescan = Console.ReadLine().ToLower().Trim();
                            if (rescan == "r")
                            {
                                ReScan = true;
                                PreviousSearch.Remove(WhatToSearch);
                            }

                            continue;
                        }
                        MainSearchFile();
                        SearchAgain = false;
                        Console.WriteLine();
                        Console.Write("# Enter the 'Y' for search again and 'N' for Close the application:");
                        Console.ForegroundColor = ConsoleColor.Yellow;
                        Console.Write("~");
                        Console.ForegroundColor = ConsoleColor.Green;
                        Console.Write("$ ");
                        Console.ResetColor();
                        CloseTheApp = Console.ReadLine().ToLower().Trim();
                        if (CloseTheApp == "n" || CloseTheApp == "no")
                        {
                            ContinueToSearch = false;
                            break;
                        }
                        if (CloseTheApp != "y" && CloseTheApp != "n")
                        {
                            bool isUserInputCorrect = true;
                            while (isUserInputCorrect)
                            {
                                Console.WriteLine();
                                Console.Write("# Enter the 'Y' for search again and 'N' for Close the application:");
                                Console.ForegroundColor = ConsoleColor.Yellow;
                                Console.Write("~");
                                Console.ForegroundColor = ConsoleColor.Green;
                                Console.Write("$ ");
                                Console.ResetColor();
                                CloseTheApp = Console.ReadLine().ToLower().Trim();
                                if (CloseTheApp == "n" || CloseTheApp == "y")
                                {
                                    isUserInputCorrect = false;
                                    break;
                                }
                            }
                        }

                        SearchAgain = true;
                        Console.WriteLine();
                        //Console.WriteLine("Compeleted");
                        Console.CursorVisible = true;
                    }
                }
                System.Threading.Thread.Sleep(500);
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Error: " + ex.Message);
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine("Please start the application in 'Admin' mode and close all the open file from given directory...");
                Console.ReadKey();
            }
            //Console.Read();
        }

        private static void Some()
        {
            throw new DirectoryNotFoundException();
        }

        private static void MainSearchFile()
        {
            SearchFiles();
            if (matchedFile.Count() <= 0)
            {
                Console.ForegroundColor = ConsoleColor.Red;

                Console.Write("-> 0 Result found");
            }

            if (matchedFile.Count() > 0)
            {
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine();
                Console.WriteLine();
                DateTime dateTime = DateTime.Now;
                var totalSearchingTimeTaken = dateTime.Subtract(SearchingTime);//.ToString("HH:mm:ss.SSS");
                int hours = totalSearchingTimeTaken.Hours;
                int minutes = totalSearchingTimeTaken.Minutes;
                int seconds = totalSearchingTimeTaken.Seconds;
                int milliseconds = totalSearchingTimeTaken.Milliseconds;
                string totalTimeTakenOutput = string.Empty;
                if (hours == 0 && minutes == 0 && seconds == 0)
                {
                    totalTimeTakenOutput = milliseconds + " milliseconds";
                }
                if (hours == 0 && minutes == 0)
                {
                    totalTimeTakenOutput = seconds + " seconds";
                }
                if (hours == 0)
                {
                    if(minutes > 0)
                    {
                        totalTimeTakenOutput = minutes + " minute and " + seconds + " seconds";
                    }
                    else
                    {
                        totalTimeTakenOutput =  seconds + " seconds";
                    }
                }
                else
                {
                    totalTimeTakenOutput = hours + "H:" + minutes + "M:" + seconds + "S";
                }
                Console.WriteLine("# Searching compeleted in [" + totalTimeTakenOutput + "], preparing result...");
                //Console.WriteLine("# Searching compeleted in [" + totalSearchingTimeTaken + "] time, preparing result...");
                Console.WriteLine();
                if (!Directory.Exists(WhereToPutResult)) Directory.CreateDirectory(WhereToPutResult);
                string outputFile = Path.Combine(WhereToPutResult, WhatToSearch + "-" + DateTime.Now.ToString("MM-dd-yyyy HH:mm:ss").Replace(":", "_") + ".txt");
                int totalResultFile = 0;
                foreach (var file in matchedFile)
                {
                    totalResultFile++;
                    File.AppendAllText(outputFile, $"{totalResultFile}) {file} {Environment.NewLine}");
                }
                Console.ResetColor();
                System.Threading.Thread.Sleep(2000);
                Console.WriteLine("# Check the output file for '" + WhatToSearch + "': (" + outputFile + ")");
                PreviousSearch.Add(WhatToSearch, outputFile);
                System.Diagnostics.Process.Start(outputFile);
            }
        }

        private static bool GetUserInput()
        {
            if (!ReScan)
            {
                if (SearchAgain) WhatToSearch = string.Empty;
            }
            ReScan = false;
            if (string.IsNullOrEmpty(WhereToRead)) CheckFolderForWhereToRead();
            if (string.IsNullOrEmpty(WhereToPutResult)) CheckFolderForWhereToPutResult();
            if (string.IsNullOrEmpty(WhatToSearch)) CheckForWhatToSearch();

            if (string.IsNullOrEmpty(WhereToRead) && string.IsNullOrEmpty(WhatToSearch) && string.IsNullOrEmpty(WhereToPutResult))
            {
                Console.WriteLine("Some Input are missing, Please Provide All The path and Search keywprd: ");
                UserInputCount++;
                GetUserInput();
            }
            if (!string.IsNullOrEmpty(WhereToRead) && !string.IsNullOrEmpty(WhatToSearch) && !string.IsNullOrEmpty(WhereToPutResult))
            {
                return true;
            }
            return false;
        }

        private static void CheckForWhatToSearch()
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.Write("# Enter the Keyword for search:");
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.Write("~");
            Console.ForegroundColor = ConsoleColor.Green;
            Console.Write("$ ");
            Console.ResetColor();
            //Console.Write(">");
            WhatToSearch = @"" + Console.ReadLine().ToLower().Trim();
            if (string.IsNullOrEmpty(WhatToSearch))
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine("Provide the keyword for search");
                CheckForWhatToSearch();
            }

        }

        private static void CheckFolderForWhereToPutResult()
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.Write("# Enter the directory path where to store the output file:");
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.Write("~");
            Console.ForegroundColor = ConsoleColor.Green;
            Console.Write("$ ");
            Console.ResetColor();
            WhereToPutResult = @"" + Console.ReadLine().ToLower().Trim();
            if (!Directory.Exists(WhereToPutResult))
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine("directory is not exist");
                CheckFolderForWhereToPutResult();
            }
        }

        private static void CheckFolderForWhereToRead()
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.Write("# Enter the path of file or directory:");
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.Write("~");
            Console.ForegroundColor = ConsoleColor.Green;
            Console.Write("$ ");
            Console.ResetColor();
            WhereToRead = @"" + Console.ReadLine().ToLower().Trim();

            if (!Directory.Exists(WhereToRead))
            {
                if (File.Exists(WhereToRead))
                {
                    IsWhereToReadFile = true;
                    return;
                }
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine("Directory is not exist");
                CheckFolderForWhereToRead();
            }
            else
            {
                if (Directory.GetFiles(WhereToRead).Count() <= 0)
                {
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.WriteLine("0 File found");
                    CheckFolderForWhereToRead();
                }
                else
                {
                    Console.WriteLine(Directory.GetFiles(WhereToRead).Count() + " Files found");
                }
            }
        }

        private static void SearchFiles()
        {
            try
            {
                if (IsWhereToReadFile)
                {
                    //string[] files = Directory.GetFiles(WhereToRead);
                    //int totalFiles = files.Count();
                    //if (SearchAgain)
                    //{
                    //    Console.WriteLine(Directory.GetFiles(WhereToRead).Count() + " Files found");
                    //    if (Directory.GetFiles(WhereToRead).Count() == 0) return;
                    //}
                    int count = 0;
                    int fileCounter = 0;
                    Console.WriteLine();
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.WriteLine("Searching started...");
                    SearchingTime = DateTime.Now;
                    Console.WriteLine();
                    Console.CursorVisible = false;

                    int consoleLine = Console.LargestWindowHeight + 2;
                    Console.SetCursorPosition(1, consoleLine);
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.Write(" ...................................[0%]");
                    int lastFileNameLength = 0;
                    for (int j = 0; j < 1; j++)
                    {
                        fileCounter++;
                        string fileName = "Current Processing File: (" + fileCounter + "/" + 1 + ") [" + Path.GetFileName(WhereToRead).ToString() + "]";

                        Console.SetCursorPosition(2, consoleLine - 2);

                        if (lastFileNameLength > fileName.Length)
                        {
                            Console.ForegroundColor = ConsoleColor.Black;
                            for (int i = 0; i < lastFileNameLength + 2; i++)
                            {
                                Console.Write("#");
                            }
                        }
                        Console.ForegroundColor = ConsoleColor.White;
                        Console.Write("\r" + fileName);
                        Console.SetCursorPosition(2, consoleLine + 1);
                        count = ReadFile(count, WhereToRead);

                        int percent = (int)Math.Round((double)(100 * fileCounter) / 1);

                        Console.SetCursorPosition(2, consoleLine);
                        for (int i = 1; i < percent; i++)
                        {
                            i += 2;
                            Console.ForegroundColor = ConsoleColor.Green;
                            Console.BackgroundColor = ConsoleColor.Green;
                            Console.Write('#');
                        }
                        Console.ForegroundColor = ConsoleColor.Green;
                        Console.BackgroundColor = ConsoleColor.Black;
                        Console.CursorSize = 10;
                        Console.SetCursorPosition(35, consoleLine);
                        Console.Write($" [{percent}%]");

                        System.Threading.Thread.Sleep(10);
                        lastFileNameLength = fileName.Length;
                    }
                    Console.WriteLine();
                    //Console.WriteLine(" Compeleted Check the output directory");
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.BackgroundColor = ConsoleColor.Black;
                }
                else
                {
                    string[] files = Directory.GetFiles(WhereToRead);
                    int totalFiles = files.Count();
                    if (SearchAgain)
                    {
                        Console.WriteLine(Directory.GetFiles(WhereToRead).Count() + " Files found");
                        if (Directory.GetFiles(WhereToRead).Count() == 0) return;
                    }
                    int count = 0;
                    int fileCounter = 0;
                    Console.WriteLine();
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.WriteLine("Searching started...");
                    SearchingTime = DateTime.Now;
                    Console.WriteLine();
                    Console.CursorVisible = false;

                    int consoleLine = Console.CursorTop + 2;
                    Console.SetCursorPosition(1, consoleLine);
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.Write(" ...................................[0%]");
                    int lastFileNameLength = 0;
                    foreach (string file in files)
                    {
                        fileCounter++;
                        string fileName = "Current Processing File: (" + fileCounter + "/" + totalFiles + ") [" + Path.GetFileName(file).ToString() + "]";

                        Console.SetCursorPosition(2, consoleLine - 2);

                        if (lastFileNameLength > fileName.Length)
                        {
                            Console.ForegroundColor = ConsoleColor.Black;
                            for (int i = 0; i < lastFileNameLength + 2; i++)
                            {
                                Console.Write("#");
                            }
                        }
                        Console.ForegroundColor = ConsoleColor.White;
                        Console.Write("\r" + fileName);
                        Console.SetCursorPosition(2, consoleLine + 1);
                        count = ReadFile(count, file);

                        int percent = (int)Math.Round((double)(100 * fileCounter) / totalFiles);

                        Console.SetCursorPosition(2, consoleLine);
                        for (int i = 1; i < percent; i++)
                        {
                            i += 2;
                            Console.ForegroundColor = ConsoleColor.Green;
                            Console.BackgroundColor = ConsoleColor.Green;
                            Console.Write('#');
                        }
                        Console.ForegroundColor = ConsoleColor.Green;
                        Console.BackgroundColor = ConsoleColor.Black;
                        Console.CursorSize = 10;
                        Console.SetCursorPosition(35, consoleLine);
                        Console.Write($" [{percent}%]");

                        System.Threading.Thread.Sleep(10);
                        lastFileNameLength = fileName.Length;
                    }
                    Console.WriteLine();
                    //Console.WriteLine(" Compeleted Check the output directory");
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.BackgroundColor = ConsoleColor.Black;
                }
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Error: " + ex.Message);
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine("Please start the application in 'Admin' mode and close all the open file from given directory...");
                Console.ReadKey();
            }
        }

        private static int ReadFile(int count, string file)
        {
            using (StreamReader streamReader = new StreamReader(file))
            {
                string line = string.Empty;
                while ((line = streamReader.ReadLine()) != null)
                {
                    if (line.ToLower().Contains(WhatToSearch.ToLower()))
                    {
                        count++;
                        matchedFile.Add($"{file}");
                        break;
                    }
                }
            }

            return count;
        }

        public static bool IsAdministrator()
        {
            return (new WindowsPrincipal(WindowsIdentity.GetCurrent()))
                      .IsInRole(WindowsBuiltInRole.Administrator);
        }
    }
}
