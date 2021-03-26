using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using SteamInstaller.Extensions;
using System;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Net;
using System.Net.Http;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

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
        private static string test = "";

        private static ILogger logger;

        /// <summary>
        /// Constructor of SteamInstaller
        /// </summary>
        /// <param name="args"></param>
        static void Main(string[] args)
        {
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


            var serviceCollection = new ServiceCollection();
            ConfigureServices(serviceCollection);

            var serviceProvider = serviceCollection.BuildServiceProvider();
            logger = serviceProvider.GetService<ILogger<Program>>();

            if (DownloadFile())
            {
                Directory.Delete(FOLDERNAME, true);
                
                logger.LogInformation($"Unzip {FILENAME} to {FOLDERNAME}");
                ZipFile.ExtractToDirectory(FILENAME, FOLDERNAME);

                logger.LogInformation($"Execute {EXE}");
                using (Process compiler = new Process())
                {
                    compiler.StartInfo.FileName = $"{FOLDERNAME}/{EXE}";
                    compiler.StartInfo.Arguments = "+quit";
                    compiler.StartInfo.UseShellExecute = false;
                    compiler.StartInfo.CreateNoWindow = true;
                    compiler.StartInfo.RedirectStandardOutput = true;
                    compiler.OutputDataReceived += Compiler_OutputDataReceived;

                    compiler.Start();
                    compiler.BeginOutputReadLine();

                    compiler.WaitForExit();
                    var bytes = Encoding.ASCII.GetBytes(test);
                    Console.WriteLine(Encoding.UTF8.GetString(bytes));

                    logger.LogInformation($"{EXE} finished with code {compiler.ExitCode}");
                    logger.LogInformation($"{FOLDERNAME} is successful installed!");

                    compiler.Close();
                }
            }

            logger.LogInformation("HELLO GUYS");
            Console.ReadKey();
            //_ = DownloadExtension.DownloadAsync(STEAM_CMD_URL, "steamcmd.zip");
            //Console.WriteLine("Hello World!");
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

        private async static void Test()
        {
            




            //var proc = new Process
            //{
            //    StartInfo = new ProcessStartInfo
            //    {
            //        FileName = $"{FOLDERNAME}/{EXE}",
            //        UseShellExecute = false,
            //        RedirectStandardOutput = true,
            //        RedirectStandardInput = true,

            //        CreateNoWindow = true
            //    }
            //};

            //proc.Start();

            //while (!proc.StandardOutput.EndOfStream)
            //{
            //    await Task.Delay(5000);
            //    //proc.Dispose();
            //}
            //proc.Dispose();
            ////proc.WaitForExit();
            //logger.LogInformation($"{EXE} finished with code {proc.ExitCode}");
            //logger.LogInformation($"{FOLDERNAME} is successful installed!");
            //proc.Close();
        }

        private static void Compiler_OutputDataReceived(object sender, DataReceivedEventArgs e)
        {
            //Console.WriteLine(e.Data);
            test += e.Data + Environment.NewLine;
        }

        private static void ConfigureServices(IServiceCollection services)
        {
            services.AddLogging(configure => configure.AddConsole());
        }

        private static bool DownloadFile()
        {
            logger.LogInformation($"Start download steamcmd from {STEAM_CMD_URL}");
            try
            {
                var client = new WebClient();
                client.DownloadFile(STEAM_CMD_URL, FILENAME);
                logger.LogInformation("Download complete!");
                return true;
            }
            catch (WebException e)
            {
                var response = (HttpWebResponse)e.Response;
                logger.LogError($"Download failed! Webexception ended with status: {response.StatusCode}");
                logger.LogError(e.Message);
                return false;
            }
        }
    }
}
