using System;

namespace ReadingOrderWebScraper.Model;

public class Comic
{
    public int ReadingOrderNumber { get; set; }
    public int CmroId { get; set; }
    public string Title { get; set; }
    public string Url { get; set; }
    public string NextIssue { get; set; }
    public string PreviousIssue { get; set; }
    public string NextIssueLink { get; set; }
    public string PreviousIssueLink { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime LastModifiedOn { get; set; }
    public string CmroStatus { get; set; }
}