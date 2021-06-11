using APlusAutoWatcher.Enums;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace APlusAutoWatcher.Data
{
    public class ChapterDataGenerator : IGenerateChapterData
    {
        private readonly ILogger<IGenerateChapterData> _logger;

        public ChapterDataGenerator(ILogger<IGenerateChapterData> logger)
        {
            _logger = logger;
        }

        public string GetNextChapter(string currentChapter, ChapterValues incrementValue)
        {
            _logger.LogInformation($"Current Chapter {currentChapter}");

            var chapter = currentChapter.Split(".");

            IncrementValue(incrementValue, chapter);

            _logger.LogInformation($"Next Chapter {chapter[0]}.{chapter[1]}.{chapter[2]}");

            return $"{chapter[0]}.{chapter[1]}.{chapter[2]}";
        }

        private static void IncrementValue(ChapterValues incrementSection, string[] chapter)
        {
            var intValue = int.Parse(chapter[(int)incrementSection]);
            intValue++;

            chapter[(int)incrementSection] = intValue.ToString();

            if (incrementSection == ChapterValues.Section)
            {
                chapter[2] = "1";
            }

            if (incrementSection == ChapterValues.Chapter)
            {
                chapter[1] = "1";
                chapter[2] = "1";
            }
        }
    }
}