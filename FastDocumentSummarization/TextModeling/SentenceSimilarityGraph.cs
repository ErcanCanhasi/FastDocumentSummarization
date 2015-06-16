using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FastDocumetSummarization.TextModeling
{
    
    class SentenceSimilarityGraph
    {
        public Dictionary<int, string> senteceNames = new Dictionary<int, string>();
        public  HashSet<int> nodes = new HashSet<int>();
        public  List<int> NB = new List<int>();
        public List<float> SIGN = new List<float>();
        public  List<int> OFF = new List<int>();
        public int ngramsNum = 0;  // comparativeSum = 5, timeLineSum = 4, 
        static int ngramsCounter = 0;
        static Dictionary<String, int> ngrams = new Dictionary<string, int>();

        public List<int> GetShingleVec(String doc)
        {
            List<int> shingleVec = new List<int>();
            //for (int n = 0; n < shinglesNum; n++)
            //{
            //doc.Insert(0, "<start>");

            for (int j = 0; j < (doc.Length - ngramsNum); j++)
            {
                StringBuilder s = new StringBuilder();
                for (int i = j; i < (j + ngramsNum); i++)
                    s.Append(doc[i]);
                string s1 = s.ToString();
                if (!ngrams.ContainsKey(s1))
                {
                    ngrams[s1] = ngramsCounter;
                    ngramsCounter++;
                }
                if (!shingleVec.Contains(ngrams[s1]))
                { shingleVec.Add(ngrams[s1]); }
            }
            // }
            return shingleVec;
        }

        public List<int> GetShingleVecWords(String doc)
        {
            List<int> shingleVec = new List<int>();
            //for (int n = 0; n < shinglesNum; n++)
            //{
            //doc.Insert(0, "<start>");
            string[] words = Helpers.GetWords(doc);
            for (int j = 0; j < (words.Length - ngramsNum); j++)
            {
                StringBuilder s = new StringBuilder();
                for (int i = j; i < (j + ngramsNum); i++)
                    s.Append(words[i] + " ");
                string s1 = s.ToString();
                if (!ngrams.ContainsKey(s1))
                {
                    ngrams[s1] = ngramsCounter;
                    ngramsCounter++;
                }
                if (!shingleVec.Contains(ngrams[s1]))
                { shingleVec.Add(ngrams[s1]); }
            }
            // }
            return shingleVec;
        }
        public void GenerateSSGraph(List<string> docs)
        {
            /* 1) decomposite the document represetned by fileName into sentences
             * 2) generate the sentence similarity graph via minhashing and LSH
             * 3) describe the graph by neiborhood list NB and offset list OFF
            */
            Stopwatch sw = new Stopwatch();
            sw.Start();
            List<string> docsOrg = new List<string>(docs);
            for (int i = 0; i < docs.Count; i++)
            {
                senteceNames[i] = docs[i];
            }

            int r = docs.Count;
            int n = 40;
            int rows = 2; // b= n / r;
            int[][] minHashes = new int[r][];
            for (int i = 0; i < r; i++)
            {
                //minHashes[i] = getShingleVec(parseWords(docs[i]).ToList()).ToArray();
                minHashes[i] = GetShingleVec(docs[i]).ToArray();
            }

            MinHash mh = new MinHash(r, n);
            int[,] minhashes = new int[r, n];
            for (int i = 0; i < r; i++)
            {
                List<int> doc = minHashes[i].ToList();
                List<uint> hvs = mh.GetMinHash(doc).ToList();
                for (int j = 0; j < hvs.Count; j++)
                {
                    minhashes[i, j] = (int)hvs[j];
                }
            }

            
            OFF.Add(0);
            int conCount = 0;


            LSH lsh = new LSH(minhashes, rows);
            lsh.Calc();
            int idx = 0;
            for (int k = 0; k < minhashes.GetUpperBound(0); k++)
            {
                List<int> nearest = lsh.GetNearest(k);
                if (!nodes.Contains(k))
                    nodes.Add(k);
                //Console.Write("\n" + k+" ");
                foreach (int i in nearest)
                {
                    //Console.Write(near + ", ");
                    if (!nodes.Contains(i))
                        nodes.Add(i);
                    if (i == idx)
                        continue;
                    NB.Add(i);
                    conCount++;
                    ++idx;
                }
                OFF.Add(conCount);
            }
            sw.Stop();
            Console.WriteLine(sw.ElapsedMilliseconds / (double)1000);
        }

        public void GenerateSSGraphForComparativeSum(List<string> docs, List<int> offsets)
        {
            /*
             * same as the first just applied to comparative sum 
             * here we have the set of different documents
             */
            Stopwatch sw = new Stopwatch();
            sw.Start();
            List<string> docsOrg = new List<string>(docs);
            for (int i = 0; i < docs.Count; i++)
            {
                senteceNames[i] = docs[i];
            }

            int r = docs.Count;
            int n = 100;
            int rows = 5; // n / r;
            int[][] minHashes = new int[r][];
            for (int i = 0; i < r; i++)
            {
                //minHashes[i] = getShingleVec(parseWords(docs[i]).ToList()).ToArray();
                minHashes[i] = GetShingleVec(docs[i]).ToArray();
            }

            MinHash mh = new MinHash(r, n);
            int[,] minhashes = new int[r, n];
            for (int i = 0; i < r; i++)
            {
                List<int> doc = minHashes[i].ToList();
                List<uint> hvs = mh.GetMinHash(doc).ToList();
                for (int j = 0; j < hvs.Count; j++)
                {
                    minhashes[i, j] = (int)hvs[j];
                }
            }


            OFF.Add(0);
            int conCount = 0;


            LSH lsh = new LSH(minhashes, rows);
            lsh.Calc();
            int idx = 0;
            for (int k = 0; k < minhashes.GetUpperBound(0); k++)
            {
                List<int> nearest = lsh.GetNearest(k);
                if (!nodes.Contains(k))
                    nodes.Add(k);
                //Console.Write("\n" + k+" ");
                foreach (int i in nearest)
                {
                        //Console.Write(near + ", ");
                        if (!nodes.Contains(i))
                            nodes.Add(i);
                        if (i == idx)
                            continue;
                        NB.Add(i);
                        if (Helpers.AreFromSameGroup(k, i, offsets))  SIGN.Add(1);
                        else SIGN.Add(-0.5f);
                        conCount++;
                        ++idx;
                }
                OFF.Add(conCount);
            }
            sw.Stop();
            Console.WriteLine(sw.ElapsedMilliseconds / (double)1000);
        }


    }
}
