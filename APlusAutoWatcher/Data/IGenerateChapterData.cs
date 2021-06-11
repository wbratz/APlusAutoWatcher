using APlusAutoWatcher.Enums;

namespace APlusAutoWatcher.Data
{
    public interface IGenerateChapterData
    {
        string GetNextChapter(string currentChapter, ChapterValues incrementValue);
    }
}