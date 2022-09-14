﻿using CsvHelper;
using HtmlAgilityPack;
using System.Collections.Generic;
using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using ReadingOrderWebScraper.Model;

namespace ReadingOrderWebScraper.Helper
{
    public class Helper
    {
    
        private readonly string _titlePath;
        private readonly string _buttonPath;
        private readonly string _nextIssuePath;
        private readonly string _baseUrl;
        private readonly string _proxyIp;
        private readonly int _proxyPort;
        private readonly ILogger _log;
        private List<Comic> _backupComics;

        public Helper(string titlePath, 
            string buttonPath, 
            string nextIssuePath,
            string baseUrl, 
            ILogger log,
            int proxyPort,
            string proxyIp)
        {
            _titlePath = titlePath;
            _buttonPath = buttonPath;
            _nextIssuePath = nextIssuePath;
            _baseUrl = baseUrl;
            _log = log;
            _proxyPort = proxyPort;
            _proxyIp = proxyIp;
        }

        public HtmlDocument GetDocument(string url)
        {
            string userId = string.Empty; //leave it blank
            string password = string.Empty;
            try
            {
                HtmlWeb web = new HtmlWeb();
                var doc = web.Load(url, _proxyIp, _proxyPort, userId, password);
                return doc;
            }
            catch (Exception ex)
            {
                _log.LogError($"Error with {ex.Message} thrown");
                throw;
            }

            //HtmlWeb web = new HtmlWeb();
            //HtmlDocument doc = await web.LoadFromWebAsync(url);
            //return doc;
        }

        public List<Comic> StartAtStart(int comicCount)
        {
            return GetComics(comicCount, _baseUrl);
        }


        public List<Comic> StartAtLatestIssue(int comicCount, string path)
        {
            var existingComics = ReadCsvFile(path);
            var latestComic = GetLatestComic(existingComics);
            var comics =  GetComics(comicCount, latestComic.NextIssueLink);
            return comics;
        }

        public bool CheckIfFileExists(string path)
        {
            return File.Exists(path);
        }

        public List<Comic> ReadCsvFile(string path)
        {
            try
            {
                using (var reader = new StreamReader(path))
                using (var csv = new CsvReader(reader, CultureInfo.InvariantCulture))
                {
                    var records = csv.GetRecords<Comic>().ToList();
                    return records;
                }
            }
            catch(Exception e)
            {
                // Log error
                _log.LogError($"Exception {e.Message} thrown.");
                throw;
            }
        }

        public Comic GetLatestComic(List<Comic> comics)
        {
            var sortedComics = comics.OrderByDescending(x => x.LastModifiedOn);
            var lastComic = sortedComics.LastOrDefault();
            return lastComic;
        }

        public List<Comic> GetComics(int comicCount, string url)
        {
            try
            {
                var comics = new List<Comic>();
                //Start with #1 of Reading Order
                var doc = GetDocument(url);
                for (int i = 1; i <= comicCount; i++)
                {

                    //Create new Comic
                    var comic = new Comic();

                    //Get ComicBookDetails
                    comic = GetComicDetail(doc, comic, i);

                    comic.CreatedAt = DateTime.UtcNow;
                    comic.LastModifiedOn = DateTime.UtcNow;
                    comic.CmroStatus = "Retrieved";

                    //Delay Task for Scraping with 10 Second Delay
                    var waitScrapingTask = Task.Delay(1000);
                    waitScrapingTask.Wait();
                    _log.LogInformation($"OrderNo {comic.ReadingOrderNumber},Title {comic.Title},  MU-Link {comic.Url}");
                    
                    //Get new Url for next Issue and repeat
                    comic = GetNextIssue(doc, comic);
                    doc =  GetDocument(comic.NextIssueLink);

                    // Add Comics to ComicList 
                    comics.Add(comic);
                    
                    _backupComics = comics;
                    // Export Comics after every 100 comics
                    if (i % 1 == 0)
                    {
                        var csvName = $"C:\\Users\\dal\\Desktop\\Part_Comics_UpToNo_{i}.csv";
                        ExportToCsv(comics, csvName);
                        _log.LogInformation($"Exported as File {csvName} at {DateTime.UtcNow:s}");
                    }

                }

                return comics;
            }
            catch (Exception e)
            {
                // Log error
                _log.LogError($"Exception {e.Message} thrown.");

                // Write already retrieved csv into a csv
                var csvName = $"C:\\Users\\dal\\Desktop\\Backup_Comics.csv";
                ExportToCsv(_backupComics, csvName);
                _log.LogInformation($"Exported as File {csvName} at {DateTime.UtcNow}");
                return _backupComics;
            }
        }

        public Comic GetComicDetail(HtmlDocument document, Comic comic, int position)
        {
            try
            {
                var titleText = document.DocumentNode.SelectSingleNode(_titlePath).InnerText;
                var titleSplit = titleText.Split("|")[0];
                comic.Title = titleSplit;
                comic.ReadingOrderNumber = position;
                var link = "";
                var hrefNodes = document.DocumentNode.SelectNodes(_buttonPath);
                if (hrefNodes != null)
                {
                    foreach (var href in hrefNodes)
                    {
                        link = href.Attributes["href"].Value;
                    }
                }

                comic.Url = link;
                return comic;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                throw;
            }
        }

        public Comic GetNextIssue(HtmlDocument document, Comic comic)
        {
            string link;
            var hrefNodes = document.DocumentNode.SelectNodes(_nextIssuePath);
            var href = hrefNodes.FirstOrDefault();
            var query = href?.Attributes["href"].Value;
            link = $"https://cmro.travis-starnes.com/{query}";
            comic.NextIssueLink = link;
            var cmroId = query?.Split("=")[1];
            comic.NextCmroId = int.Parse(cmroId);
            return comic;
        }

        public void ExportToCsv(List<Comic> comics, string path)
        {
            using (var writer = new StreamWriter(path))
            using (var csv = new CsvWriter(writer, CultureInfo.InvariantCulture))
            {
                csv.WriteRecords(comics);
            }
        }

    }

}
