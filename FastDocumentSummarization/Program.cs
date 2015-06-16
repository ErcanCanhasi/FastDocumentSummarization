using FastDocumetSummarization.TextModeling;
using FastDocumetSummarization.SentenceRanking;
using FastDocumetSummarization.Summarization;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FastDocumetSummarization
{
    class Program
    {

        static void Main(string[] args)
        {
            //Summarizer.prepareForRouge();
            //Summarizer.generateModels();
            //Summarizer.ComparativeSummarization();
            //Summarizer.TimeLineSummarization();
            Summarizer.GeneralAndQuerySummarization();
        }
    }
}
