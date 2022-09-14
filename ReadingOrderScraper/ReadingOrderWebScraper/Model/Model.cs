namespace ReadingOrderWebScraper.Model;

public class Model
{
    public int ReadingOrderNumber { get; set; }
    public string Title { get; set; }
    public string URL { get; set; }
    public string NextIssue { get; set; }
    public string PreviousIssue { get; set; }
    public string NextIssueLink { get; set; }
    public string PreviousIssueLink { get; set; }
}