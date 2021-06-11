using APlusAutoWatcher.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace APlusAutoWatcher.Utilities.Contracts
{
    public interface IScrapeWebpages
    {
        ChapterValues ParseWebPage(string version, string chapter, string uniqueId);
    }
}