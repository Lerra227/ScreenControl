using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using OpenQA.Selenium;
using OpenQA.Selenium.Edge;

class Program
{
    static void Main()
    {
        // Путь к драйверу Edge
        // var edgeDriverPath = "\"C:\\Program Files (x86)\\Microsoft\\Edge\\Application\\msedgedriver.exe\"";
        Console.WriteLine("Hello!\nEnter the path to the file with URLs:");
        // Путь к файлу с URL
        string urlsFilePath = Console.ReadLine();
       

        Console.WriteLine("Enter the path to save the file with screenshots:");
        string screenshotsDirectory = Console.ReadLine();
       

        Console.WriteLine("Enter the path to the future report directory:");
        string reportDirectory = Console.ReadLine();

        if (urlsFilePath == null || screenshotsDirectory == null|| reportDirectory == null) throw new Exception("Data was not entered");



        // Создание экземпляра драйвера Edge
        // using (var driver = new EdgeDriver(edgeDriverPath))
        using (var driver = new EdgeDriver())
        {
            // Чтение URL из файла
            var urls = File.ReadAllLines(urlsFilePath);

            // Создание каталога для сохранения скриншотов
           // var screenshotsDirectory = "C:\\Users\\amana\\Documents\\screenshots";
            Directory.CreateDirectory(screenshotsDirectory);

            // Создание списка для хранения результатов запросов
            var results = new List<RequestResult>();

            // Обход каждого URL
            foreach (var url in urls)
            {
                // Открытие URL в браузере
                driver.Navigate().GoToUrl(url);

                // Получение скриншота
                var screenshotPath = Path.Combine(screenshotsDirectory, $"{GetHashCode(url)}.png");
                ((ITakesScreenshot)driver).GetScreenshot().SaveAsFile(screenshotPath, ScreenshotImageFormat.Png);

                // Добавление результатов запроса
                results.Add(new RequestResult
                {
                    Url = url,
                    ScreenshotPath = screenshotPath,
                    PageSource = driver.PageSource
                });
            }

            // Создание каталога для сохранения отчета
          //  var reportDirectory = "C:\\Users\\amana\\Documents\\report.txt";
            Directory.CreateDirectory(reportDirectory);

            // Группировка результатов по похожести скриншотов
            var groupedResults = results.GroupBy(r => GetSimilarityHash(r.ScreenshotPath));

            // Создание отчета HTML
            var reportPath = Path.Combine(reportDirectory, "report.html");
            using (var writer = new StreamWriter(reportPath))
            {
                writer.WriteLine("<html><head><title>Report</title></head><body>");

                foreach (var group in groupedResults)
                {
                    writer.WriteLine("<h2>Similarity Group</h2>");

                    foreach (var result in group)
                    {
                        writer.WriteLine($"<p>URL: {result.Url}</p>");
                        writer.WriteLine($"<p>Screenshot: <img src='{result.ScreenshotPath}' width='300'/></p>");
                        writer.WriteLine($"<p>Page Source: {result.PageSource}</p>");
                    }
                }

                writer.WriteLine("</body></html>");
            }

            Console.WriteLine($"Отчет сохранен в {reportPath}");
        }
    }

    // Получение хеша для определения похожести скриншотов
    static int GetSimilarityHash(string imagePath)
    {
        var image = new Bitmap(imagePath);
        return image.GetHashCode();
    }
   /* {
        using (var image = new Bitmap(imagePath))
        {
            using (var resized = new Bitmap(image, new Size(8, 8)))
            {
                var hash = new StringBuilder();

                for (var y = 0; y < resized.Height; y++)
                {
                    for (var x = 0; x < resized.Width; x++)
                    {
                        var pixel = resized.GetPixel(x, y);
                        hash.Append(pixel.GetBrightness() < 0.5 ? "0" : "1");
                    }
                }

                return Convert.ToDouble(hash.ToString(), 2);
            }
        }
    } */

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