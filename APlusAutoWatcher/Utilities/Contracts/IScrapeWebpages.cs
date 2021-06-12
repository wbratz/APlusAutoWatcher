namespace APlusAutoWatcher.Utilities.Contracts
{
    public interface IScrapeWebpages
    {
        string ParseWebPage(string baseUrl, string path, string path2);
    }
}