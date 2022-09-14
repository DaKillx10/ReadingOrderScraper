using CsvHelper;
using HtmlAgilityPack;
using System.Collections.Generic;
using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

public class Helper
{
    private string _titlePath;
    private string _buttonPath;
    private string _nextIssuePath;

    public Helper()
    {

    }

    public HtmlDocument GetDocument(string url)
    {
        HtmlWeb web = new HtmlWeb();
        HtmlDocument doc = web.Load(url);
        return doc;
    }

    public List<Comic> GetComics(string url)
    {
        var comics = new List<Comic>();
        //Start with #1 of Reading Order
        var doc = GetDocument(url);
        for (int i = 1; i <= 10; i++)
        {
            //Create new Comic
            var comic = new Comic();

            //Get ComicBookDetails
            comic = GetComicDetail(doc, comic, i);

            comic.CreatedAt = DateTime.UtcNow;
            comic.LastModifiedOn = DateTime.UtcNow;
            comic.CMROStatus = "Retrieved";

            //Delay Task for Scraping 
            Task.Delay(TimeSpan.FromSeconds(10));
            Console.WriteLine($"OrderNo {comic.ReadingOrderNumber},Title {comic.Title},  MU-Link {comic.URL}");
            //Get new Url and repeat

            comic.NextIssueLink = GetNextIssue(doc);
            doc = GetDocument(comic.NextIssueLink);
            // Add Comics to ComicList 
            comics.Add(comic);

        }

        return comics;
    }

    public Comic GetComicDetail(HtmlDocument document, Comic comic, int position)
    {
        try
        {
            var titleText = document.DocumentNode.SelectSingleNode(TitleHeadXpath).InnerText;
            var titleSplit = titleText.Split("|")[0];
            comic.Title = titleSplit;
            comic.ReadingOrderNumber = position;
            var link = "";
            var hrefNodes = document.DocumentNode.SelectNodes(ReadMeButtonXPath);
            if (hrefNodes != null)
            {
                foreach (var href in hrefNodes)
                {
                    link = href.Attributes["href"].Value;
                }
            }

            comic.URL = link;
            return comic;
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex);
            throw;
        }
    }

    public string GetNextIssue(HtmlDocument document)
    {
        var link = "";
        var hrefNodes = document.DocumentNode.SelectNodes(NextIssueXPath);
        var href = hrefNodes.FirstOrDefault();
        var query = href?.Attributes["href"].Value;
        link = $"https://cmro.travis-starnes.com/{query}";
        return link;
    }

    public void ExportToCsv(List<Comic> comics)
    {
        using (var writer = new StreamWriter("./comics.csv"))
        using (var csv = new CsvWriter(writer, CultureInfo.InvariantCulture))
        {
            csv.WriteRecords(comics);
        }
    }

}