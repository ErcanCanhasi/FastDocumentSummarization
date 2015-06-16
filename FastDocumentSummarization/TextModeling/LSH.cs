using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FastDocumetSummarization
{
    class LSH
    {
        Dictionary<int, HashSet<int>> m_lshBuckets = new Dictionary<int, HashSet<int>>();

        int[,] minHashes;
        int numBands;
        int numHashFunctions;
        int rowsPerBand;
        int numSets;
        List<SortedList<int, List<int>>> lshBuckets = new List<SortedList<int, List<int>>>();

        public LSH(int[,] minHashes, int rowsPerBand)
        {
            this.minHashes = minHashes;
            numHashFunctions = minHashes.GetUpperBound(1) + 1;
            numSets = minHashes.GetUpperBound(0) + 1;
            this.rowsPerBand = rowsPerBand;
            this.numBands = numHashFunctions / rowsPerBand;
        }

        public void Calc()
        {
            int thisHash = 0;

            for (int b = 0; b < numBands; b++)
            {
                var thisSL = new SortedList<int, List<int>>();
                for (int s = 0; s < numSets; s++)
                {
                    int hashValue = 0;
                    for (int th = thisHash; th < thisHash + rowsPerBand; th++)
                    {
                        hashValue = unchecked(hashValue * 1174247 + minHashes[s, th]);
                    }
                    if (!thisSL.ContainsKey(hashValue))
                    {
                        thisSL.Add(hashValue, new List<int>());
                    }
                    thisSL[hashValue].Add(s);
                }
                thisHash += rowsPerBand;
                var copy = new SortedList<int, List<int>>();
                foreach (int ic in thisSL.Keys)
                {
                    if (thisSL[ic].Count() > 1)
                    {
                        copy.Add(ic, thisSL[ic]);
                    }
                }
                lshBuckets.Add(copy);
            }
        }

        public List<int> GetNearest(int n)
        {
            List<int> nearest = new List<int>();
            foreach (SortedList<int, List<int>> b in lshBuckets)
            {
                foreach (List<int> li in b.Values)
                {
                    if (li.Contains(n))
                    {
                        nearest.AddRange(li);
                        break;
                    }
                }
            }
            nearest = nearest.Distinct().ToList();
            nearest.Remove(n);  // remove the document itself
            return nearest;
        }
    }
}   