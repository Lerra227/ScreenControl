using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Net;
using System.Linq;
using System.Text;
using OpenQA.Selenium;
using OpenQA.Selenium.Edge;
using System.Threading.Tasks;
using System.Runtime.InteropServices;
using NDesk.Options;


class Program
{
    static void Main(string[] args)
    {
        
        bool show_help = false;
        string screenshotsDirectory = "";
        string urlsFile = "";


    OptionSet p = new OptionSet()
      .Add("h|?|help", delegate (string v) { show_help = v != null; })
      .Add("o|out=", "path to directory(output directory)", delegate (string v) { screenshotsDirectory = v; })
      .Add("u|urls=", "path to file(urls)", delegate (string v) { urlsFile = v; });

        List<string> extra;
        try
        {
            extra = p.Parse(args);
        }
        catch (OptionException e)
        {
            Console.WriteLine(e.Message);
            Console.WriteLine("Try `-h' for more information.");
            return;
        }

        if (show_help)
        {
            ShowHelp(p);
            return;
        }

        static void ShowHelp(OptionSet p)
        {
            Console.WriteLine("Example: [PATH TO FILE WITH URLS] -o [PATH TO DIRECTORY FOR SCREENSHOTS] ");
            Console.WriteLine("Options:");
            p.WriteOptionDescriptions(Console.Out);
        }

       
        // Путь к файлу с URL

        if (!File.Exists(urlsFile))
        {
            Console.WriteLine("File not found: " + urlsFile);
            return;
        }

        

        if (urlsFile == null || screenshotsDirectory == null) throw new Exception("Data was not entered");

        string just_style = @"   ____                             __  ___            __    _          
  / __/____ ____ ___  ___  ___     /  |/  /___ _ ____ / /   (_)___  ___ 
 _\ \ / __// __// -_)/ -_)/ _ \   / /|_/ // _ `// __// _ \ / // _ \/ -_)
/___/ \__//_/   \__/ \__//_//_/  /_/  /_/ \_,_/ \__//_//_//_//_//_/\__/ 
                                                                        ";
        Console.WriteLine(just_style);
        Console.WriteLine("========================================================================");
        Console.WriteLine("\t\t\t\tby Lerra227");
        Console.WriteLine("========================================================================\n");



        // Создание экземпляра драйвера Edge
        using (var driver = new EdgeDriver())
        {
            // Чтение URL из файла
            var urls = File.ReadAllLines(urlsFile);
            

            // Создание каталога для сохранения скриншотов
            Directory.CreateDirectory(screenshotsDirectory);

            // Создание списка для хранения результатов запросов
            var results = new List<RequestResult>();

            // Обход каждого URL
            foreach (var url in urls)
            {
                string htmlContent = LoadWebPage(url);

              
                if(htmlContent == null)
                {
                    Console.WriteLine("Failed to load");
                }
                // Открытие URL в браузере
                driver.Navigate().GoToUrl(url);

                string HashName = Convert.ToString(GetHashCode(url)) + ".png";
                // Получение скриншота
                var screenshotPath = Path.Combine(screenshotsDirectory, HashName);
                ((ITakesScreenshot)driver).GetScreenshot().SaveAsFile(screenshotPath, ScreenshotImageFormat.Png);

                // Добавление результатов запроса
                results.Add(new RequestResult
                {
                    Url = url,
                    ScreenshotPath = screenshotPath,
                    PageSource = driver.PageSource
                });
            }


            // Группировка результатов по похожести скриншотов
            var groupedResults = results.GroupBy(r => GetSimilarityHash(r.ScreenshotPath));

            // Создание отчета HTML
            var reportPath = Path.Combine(screenshotsDirectory, "report.html");
            using (var writer = new StreamWriter(reportPath))
            {
                writer.WriteLine("<html><head><title>Report</title></head><body>");

                foreach (var group in groupedResults)
                {
                    writer.WriteLine("<h2>Similarity Group</h2>");

                    foreach (var result in group)
                    {
                        writer.WriteLine("<p>URL:" + result.Url + "</p>");
                        writer.WriteLine("<p>Screenshot: <img src='" + result.ScreenshotPath + "' width='300'/></p>");
                        writer.WriteLine("<p>Page Source: "+ result.PageSource + "</p>");
                    }
                }

                writer.WriteLine("</body></html>");
            }

            Console.WriteLine("Report saved in " + reportPath);
        }
    }

    static string LoadWebPage(string url)
    {
        try
        {
            using (var client = new WebClient())
            {
                // Загрузка содержимого веб-страницы.
                return client.DownloadString(url);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine("failed to load: " + ex.Message);
            return null;
        }
    }

    // Получение хеша для определения похожести скриншотов
    static int GetSimilarityHash(string imagePath)
    {
        var image = new Bitmap(imagePath);
        return image.GetHashCode();
    }

    // Получение хеша для определения похожести URL
    static int GetHashCode(string url)
    {
        return url.GetHashCode();
    }
}

// Класс для хранения результатов запроса
class RequestResult
{
    public string Url { get; set; }
    public string ScreenshotPath { get; set; }
    public string PageSource { get; set; }
}
