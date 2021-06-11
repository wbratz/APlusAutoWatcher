using APlusAutoWatcher.Data;
using APlusAutoWatcher.Enums;
using NUnit.Framework;

namespace APlusAutoWatcher.Tests
{
    [TestFixture]
    public class ChapterDataGeneratorTests
    {
        [Test]
        [TestCase("8.1.1", ChapterValues.Chapter, "9.1.1")]
        [TestCase("8.6.12", ChapterValues.Chapter, "9.1.1")]
        [TestCase("8.1.1", ChapterValues.Section, "8.2.1")]
        [TestCase("8.6.12", ChapterValues.Section, "8.7.1")]
        [TestCase("8.1.1", ChapterValues.Subsection, "8.1.2")]
        [TestCase("8.1.18", ChapterValues.Subsection, "8.1.19")]
        public void ChapterDataGenerator_IncrementsChapter(string value, ChapterValues chapterValue, string expectedResult)
        {
            var chapterDataGen = new ChapterDataGenerator();

            var result = chapterDataGen.GetNextChapter(value, chapterValue);

            Assert.AreEqual(expectedResult, result);
        }
    }
}