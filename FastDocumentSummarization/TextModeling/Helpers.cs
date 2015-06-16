using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace FastDocumetSummarization.TextModeling
{
    class Helpers
    {
       
        
        private static Regex s_wordRegex = new Regex(@"[\w']+");

        public static List<string> stopWords;
        public static void createStopWordList()
        {
            stopWords = File.ReadAllText(@"C:\Users\Ercan\Desktop\projects\findingDups\stopwords\stop-words_english_4_google_en.txt").Split('\n').ToList();
            stopWords.Sort();
            //Helpers.ReadFromBlob(path).Split('\n').ToList();
        }

        public static bool isItStopWord(string s)
        {
            int i = stopWords.BinarySearch(s);
            return i >= 0 ? true : false;
        }

        public static bool AreFromSameGroup(int i, int j, List<int> offsets)
        {
            bool r = false;
            int g1 = -1, g2 = -1;
            int ii = 0, jj = 0;
            foreach (int limit in offsets)
            {
                if (i < limit) { g1 = ii; break; }
                ii++; 
            }
            foreach (int limit in offsets)
            {
                if (j < limit) { g2 = jj; break; }
                jj++;
            }

            if (g1 == g2) r = true;
            return r;
        }

        public static void GenerateMapping(List<List<string>> docSets, ref List<string> docs, ref Dictionary<int, int> mapping)
        {
            int i = 0;
            int j = 0;
            foreach(var set in docSets)
            {
                foreach (var doc in set)
                {
                    docs.Add(doc);
                    mapping[i] = j;
                    i++;
                }
                j++;
            }
        }

        private static string removeStopWords(string p)
        {
            StringBuilder newS = new StringBuilder();
            createStopWordList();
            foreach (string word in GetWords(p))
            {
                if (!isItStopWord(word + "\r"))
                { newS.Append(word + " "); }
            }
            return newS.ToString();
        }


        private static IEnumerable<string> parseWords(string text)
        {
            return Regex.Matches(text.ToLower(), @"[\w-[\d_]]+")
                        .Cast<Match>()
                        .Select(m => m.Value);
        }

        public static List<String> readDocsFromOneFile(string fileName)
        {
            //@"C:\Users\Ercan\Desktop\PHD\archivePhD\in\duc2006\"+
            return File.ReadLines(fileName).Where(line => line != "").ToList();
        }

        public static string[] GetWords(params string[] list)
        {
            List<string> words = new List<string>();
            if (list != null)
            {
                foreach (string item in list)
                {
                    if (!string.IsNullOrEmpty(item))
                    {
                        string input = item.ToLower();
                        int idx = 0;
                        while (true)
                        {
                            Match match = s_wordRegex.Match(input, idx);
                            if (!match.Success)
                            {
                                break;
                            }
                            string word = input.Substring(match.Index, match.Length);
                            if (!words.Contains(word))
                            {
                                words.Add(word);
                            }
                            idx = match.Index + match.Length;
                        }
                    }
                }
            }
            return words.ToArray();
        }


      
    }
}
