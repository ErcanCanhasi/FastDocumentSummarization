using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FastDocumetSummarization.TextModeling;
using System.IO;

namespace FastDocumetSummarization.SentenceRanking
{
    class FastNodeRanking
    {
        public static List<string> SentenceRankingForGeneralSum(List<string> docs, int sumLen, int lineLenght, int wordCount, int ngramsNum)
        {
            /*
             Sentence Ranking and extraction
             for a document represented as a fileName:
             1) first the sentence similarity graph is formed
             2) than the fast node (sentence) ranking is applied
             3) top scoring sentences are extracted into final summary          
             */
            
            //Stopwatch sw = new Stopwatch();
            //sw.Start();

            SentenceSimilarityGraph ssGraph = new SentenceSimilarityGraph();
            ssGraph.ngramsNum = ngramsNum;
            ssGraph.GenerateSSGraph(docs);

            int nn = ssGraph.senteceNames.Count;
            float beta = 0.15f;
            float[] pr = new float[nn];
            float[] newpr = new float[nn];
            for (int i = 0; i < nn; i++) pr[i] = 1 / (float)nn;
            for (int i = 0; i < nn; i++) newpr[i] = (1 - beta) / (float)nn;

            for (int k = 0; k < 10; k++)
            {
                for (int i = 0; i < ssGraph.OFF.Count - 1; i++)
                {
                    int outd = ssGraph.OFF[i + 1] - ssGraph.OFF[i];
                    for (int j = ssGraph.OFF[i]; j <= (ssGraph.OFF[i + 1] - 1); j++)
                    {
                        newpr[ssGraph.NB[j]] += beta * (pr[i] / outd);
                    }
                }
                pr = newpr;
            }

            List<string> summary = penaltyRankingExtraction(pr, ssGraph, sumLen, lineLenght, wordCount, docs);

           /* Dictionary<string, float> rankedOut = new Dictionary<string, float>();
            for (int k = 0; k < pr.Length; k++)
            {
                rankedOut[ssGraph.senteceNames[k]] = pr[k];
            }

            rankedOut = rankedOut.OrderByDescending(x => x.Value).ToDictionary(x => x.Key, x => x.Value);

            var sorted = pr.Select((x, i) => new KeyValuePair<double, int>(x, i)).OrderByDescending(x => x.Key).ToList();

            List<double> B = sorted.Select(x => x.Key).ToList();
            List<int> idx = sorted.Select(x => x.Value).ToList();

            List<string> summary = new List<string>();
            int len = 0;
            foreach (string line in rankedOut.Keys)
            {
                if (len < sumLen)  // timeLine sumLen = 50;
                {
                    if (line.Length > lineLenght && Helpers.GetWords(line).Count() > wordCount)  // 80 6
                    {
                        int index1 = line.IndexOf("- ");
                        string line1;
                        if (index1 > 0)
                        {
                            line1 = line.Substring(index1 + 2);
                        }
                        else
                        {
                            line1 = line;
                        }

                        summary.Add(line1);
                        len += Helpers.GetWords(line1).Count();
                    }
                }
            }
            //sw.Stop();
            //Console.WriteLine(sw.ElapsedMilliseconds / (double)1000);*/
            
            return summary;
        }

        public static List<string> penaltyRankingExtraction(float[] pr, SentenceSimilarityGraph ssGraph, int sumLen, int lineLenght, int wordCount, List<string> docs)
        {
            List<string> summary = new List<string>();
            int len = 0;
            while (len < sumLen)
            {
                var sorted = pr.Select((x, i) => new KeyValuePair<double, int>(x, i)).OrderByDescending(x => x.Key).ToList();
                List<double> B = sorted.Select(x => x.Key).ToList();
                List<int> idx = sorted.Select(x => x.Value).ToList();

                // extract the best sentence
                string bestLine = docs[idx[0]];
                if (pr[idx[0]] <= 0)
                    return summary; 
                pr[idx[0]] = 0.0f;
                try
                {
                    int a = ssGraph.OFF[idx[0]];
                    int b = ssGraph.OFF[idx[0] + 1];
                    int dif = b - a;
                    List<int> nearest = ssGraph.NB.GetRange(a, dif);
                    float penalty = 1 / (float)dif;
                    foreach (int near in nearest)
                    {
                        pr[near] -= penalty;
                    }
                    //.Contains(idx[5]);

                    if (bestLine.Length > lineLenght && Helpers.GetWords(bestLine).Count() > wordCount)  // 80 6
                    {
                        int index1 = bestLine.IndexOf("- ");
                        string line1;
                        if (index1 > 0)
                        {
                            line1 = bestLine.Substring(index1 + 2);
                        }
                        else
                        {
                            line1 = bestLine;
                        }

                        summary.Add(line1);
                        len += Helpers.GetWords(line1).Count();
                    }

                }
                catch(Exception e)
                {}
            }

            return summary;
        }

        public static List<string> SentenceRankingForComparativeSum(List<string> docs, List<int> offsets, int sumLen, int lineLenght, int wordCount, int ngramsNum)
        {
            /*
             Sentence Ranking and extraction
             for a document represented as a fileName:
             1) first the sentence similarity graph is formed
             2) than the fast node (sentence) ranking is applied
             3) top scoring sentences are extracted into final summary          
             */

            //Stopwatch sw = new Stopwatch();
            //sw.Start();

            SentenceSimilarityGraph ssGraph = new SentenceSimilarityGraph();
            ssGraph.ngramsNum = ngramsNum;
            ssGraph.GenerateSSGraphForComparativeSum(docs, offsets);

            int nn = ssGraph.senteceNames.Count;
            float beta = 0.15f;
            float[] pr = new float[nn];
            float[] newpr = new float[nn];
            for (int i = 0; i < nn; i++) pr[i] = 1 / (float)nn;
            for (int i = 0; i < nn; i++) newpr[i] = (1 - beta) / (float)nn;

            for (int k = 0; k < 100; k++)
            {
                for (int i = 0; i < ssGraph.OFF.Count - 1; i++)
                {
                    int outd = ssGraph.OFF[i + 1] - ssGraph.OFF[i];
                    for (int j = ssGraph.OFF[i]; j <= (ssGraph.OFF[i + 1] - 1); j++)
                    {
                        newpr[ssGraph.NB[j]] += beta * (pr[i] / outd) * ssGraph.SIGN[j];
                    }
                }
                pr = newpr;
            }

            Dictionary<string, float> rankedOut = new Dictionary<string, float>();
            for (int k = 0; k < pr.Length; k++)
            {
                rankedOut[ssGraph.senteceNames[k]] = pr[k];
            }

            rankedOut = rankedOut.OrderByDescending(x => x.Value).ToDictionary(x => x.Key, x => x.Value);
            List<string> summary = new List<string>();
            int len = 0;
            foreach (string line in rankedOut.Keys)
            {
                if (len < sumLen)
                {
                    if (line.Length > lineLenght && Helpers.GetWords(line).Count() > wordCount)
                    {
                        int index1 = line.IndexOf("- ");
                        string line1;
                        if (index1 > 0)
                        {
                            line1 = line.Substring(index1 + 2);
                        }
                        else
                        {
                            line1 = line;
                        }

                        summary.Add(line1);
                        len += Helpers.GetWords(line1).Count();
                    }
                }
            }
            //sw.Stop();
            //Console.WriteLine(sw.ElapsedMilliseconds / (double)1000);
            return summary;
        }

    }
}
