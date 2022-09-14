using System;

namespace ReadingOrderWebScraper.Model;

public class Comic
{
    public int ReadingOrderNumber { get; set; }
    public int NextCmroId { get; set; }
    public string Title { get; set; }
    public string Url { get; set; }
    public string NextIssueLink { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime LastModifiedOn { get; set; }
    public string CmroStatus { get; set; }
}