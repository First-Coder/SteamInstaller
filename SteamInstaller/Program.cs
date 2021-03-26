using System;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Net;
using System.Reflection;
using System.Text.RegularExpressions;

namespace SteamInstaller
{
    class Program
    {
        /// <summary>
        /// Constants
        /// </summary>
        private const string STEAM_CMD_URL = "https://steamcdn-a.akamaihd.net/client/installer/steamcmd.zip";
        private const string FILENAME = "steamcmd.zip";
        private const string FOLDERNAME = "SteamCMD";
        private const string EXE = "steamcmd.exe";

        /// <summary>
        /// Constructor of SteamInstaller
        /// </summary>
        /// <param name="args"></param>
        static void Main(string[] args)
        {
            bool quit = false;
            if (args.Length > 0 && args[0].Equals("/quit"))
            {
                quit = true;
            }

            Console.WriteLine(Environment.NewLine);
            WriteColor(@"[$$$$$$$$\ $$\                       $$\            $$$$$$\                  $$\]", ConsoleColor.DarkGreen);
            WriteColor(@"[$$  _____|\__|                      $$ |          $$  __$$\                 $$ |]", ConsoleColor.DarkGreen);
            WriteColor(@"[$$ |      $$\  $$$$$$\   $$$$$$$\ $$$$$$\         $$ /  \__| $$$$$$\   $$$$$$$ | $$$$$$\   $$$$$$\]", ConsoleColor.DarkGreen);
            WriteColor(@"[$$$$$\    $$ |$$  __$$\ $$  _____|\_$$  _|$$$$$$\ $$ |      $$  __$$\ $$  __$$ |$$  __$$\ $$  __$$\]", ConsoleColor.DarkGreen);
            WriteColor(@"[$$  __|   $$ |$$ |  \__|\$$$$$$\    $$ |  \______|$$ |      $$ /  $$ |$$ /  $$ |$$$$$$$$ |$$ |  \__|]", ConsoleColor.DarkGreen);
            WriteColor(@"[$$ |      $$ |$$ |       \____$$\   $$ |$$\       $$ |  $$\ $$ |  $$ |$$ |  $$ |$$   ____|$$ |]", ConsoleColor.DarkGreen);
            WriteColor(@"[$$ |      $$ |$$ |      $$$$$$$  |  \$$$$  |      \$$$$$$  |\$$$$$$  |\$$$$$$$ |\$$$$$$$\ $$ |]", ConsoleColor.DarkGreen);
            WriteColor(@"[\__|      \__|\__|      \_______/    \____/        \______/  \______/  \_______| \_______|\__|]", ConsoleColor.DarkGreen);
            Console.WriteLine(Environment.NewLine);

            WriteColor(@"[//--Informationen------------------------------------------------]", ConsoleColor.DarkGreen);
            WriteColor($"[// Title:] {Assembly.GetEntryAssembly().GetName().Name}", ConsoleColor.DarkGreen);
            WriteColor($"[// Version:] {Assembly.GetEntryAssembly().GetCustomAttribute<AssemblyFileVersionAttribute>().Version}", ConsoleColor.DarkGreen);
            WriteColor($"[// Autor:] {Assembly.GetEntryAssembly().GetCustomAttribute<AssemblyCopyrightAttribute>().Copyright}", ConsoleColor.DarkGreen);
            WriteColor(@"[//--Settings-----------------------------------------------------]", ConsoleColor.DarkGreen);
            WriteColor($"[// Install SteamCMD:] {Directory.GetCurrentDirectory()}", ConsoleColor.DarkGreen);
            WriteColor(@"[//--Download-----------------------------------------------------]", ConsoleColor.DarkGreen);
            var download = DownloadFile();
            if(!download)
            {
                WriteColor(@"[//---------------------------------------------------------------]", ConsoleColor.DarkGreen);
                Console.ReadKey();
                return;
            }
            WriteColor(@"[//--Install------------------------------------------------------]", ConsoleColor.DarkGreen);
            if (Directory.Exists(FOLDERNAME))
            {
                WriteColor($"[//] Download folder {FOLDERNAME} is exists...", ConsoleColor.DarkGreen);
                Directory.Delete(FOLDERNAME, true);
            }

            WriteColor($"[//] Create folder {FOLDERNAME}...", ConsoleColor.DarkGreen);
            Directory.CreateDirectory(FOLDERNAME);

            WriteColor($"[//] Unzip {FILENAME} to{FOLDERNAME}...", ConsoleColor.DarkGreen);
            ZipFile.ExtractToDirectory(FILENAME, FOLDERNAME);

            WriteColor($"[//] Execute {EXE}...", ConsoleColor.DarkGreen);
            WriteColor(@"[//--SteamCMD Output----------------------------------------------]", ConsoleColor.DarkGreen);
            using (Process compiler = new Process())
            {
                compiler.StartInfo.FileName = $"{FOLDERNAME}/{EXE}";
                compiler.StartInfo.Arguments = "+quit";
                compiler.StartInfo.UseShellExecute = false;
                compiler.StartInfo.RedirectStandardOutput = true;
                compiler.OutputDataReceived += Compiler_OutputDataReceived;

                compiler.Start();
                compiler.BeginOutputReadLine();

                compiler.WaitForExit();

                WriteColor($"[//] Application {EXE} finished with code {compiler.ExitCode}", ConsoleColor.DarkGreen);
                WriteColor(@"[//---------------------------------------------------------------]", ConsoleColor.DarkGreen);
                compiler.Close();
            }

            if (!quit)
            {
                Console.WriteLine(Environment.NewLine);
                Console.WriteLine("Push any key to close the console");
                Console.ReadKey();
            }
        }

        /// <summary>
        /// Write some coloring console messages for the user
        /// https://stackoverflow.com/questions/2743260/is-it-possible-to-write-to-the-console-in-colour-in-net
        /// </summary>
        /// <param name="message">Message to write</param>
        /// <param name="color">ConsoleColor value of the color</param>
        static void WriteColor(string message, ConsoleColor color)
        {
            var pieces = Regex.Split(message, @"(\[[^\]]*\])");

            for (int i = 0; i < pieces.Length; i++)
            {
                string piece = pieces[i];

                if (piece.StartsWith("[") && piece.EndsWith("]"))
                {
                    Console.ForegroundColor = color;
                    piece = piece.Substring(1, piece.Length - 2);
                }

                Console.Write(piece);
                Console.ResetColor();
            }

            Console.WriteLine();
        }

        /// <summary>
        /// Write console output to the screen
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e">Eventinformation from the console</param>
        private static void Compiler_OutputDataReceived(object sender, DataReceivedEventArgs e)
        {
            if (!string.IsNullOrEmpty(e.Data))
            {
                WriteColor($"[//] {e.Data}", ConsoleColor.Cyan);
            }
        }

        /// <summary>
        /// Download the SteamCMD file
        /// </summary>
        /// <returns>Return true if the file is successful downloaded</returns>
        private static bool DownloadFile()
        {
            WriteColor($"[//] Start download steamcmd from {STEAM_CMD_URL}...", ConsoleColor.DarkGreen);
            try
            {
                var client = new WebClient();
                client.DownloadFile(STEAM_CMD_URL, FILENAME);
                WriteColor($"[//] Download complete!", ConsoleColor.DarkGreen);
                return true;
            }
            catch (WebException e)
            {
                var response = (HttpWebResponse)e.Response;
                WriteColor($"[// Error:] Download failed! Webexception ended with status: {response.StatusCode}", ConsoleColor.DarkRed);
                WriteColor($"[// Error:] {e.Message}", ConsoleColor.DarkRed);
                return false;
            }
        }
    }
}
