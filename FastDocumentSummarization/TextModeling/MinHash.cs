using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FastDocumetSummarization
{
    public class MinHash
    {

        public static double Calc(HashSet<uint> hs1, HashSet<uint> hs2)
        {
            return ((double)hs1.Intersect(hs2).Count() / (double)hs1.Union(hs2).Count());
        }

        public static double Calc(List<uint> ls1, List<uint> ls2)
        {
            HashSet<uint> hs1 = new HashSet<uint>(ls1);
            HashSet<uint> hs2 = new HashSet<uint>(ls2);
            return Calc(hs1, hs2);
        }

        // Constructor passed universe size and number of hash functions
        public MinHash(int universeSize, int numHashFunctions)
        {
            this.numHashFunctions = numHashFunctions;
            // number of bits to store the universe
            int u = BitsForUniverse(universeSize);
            GenerateHashFunctions(u);
        }

        private int numHashFunctions;

        // Returns number of hash functions defined for this instance
        public int NumHashFunctions
        {
            get { return numHashFunctions; }
        }

        public delegate uint Hash(int toHash);
        private Hash[] hashFunctions;  //dictionary with fast lookups

        // Public access to hash functions
        public Hash[] HashFunctions
        {
            get { return hashFunctions; }
        }

        // Generates the Universal Random Hash functions
        // http://en.wikipedia.org/wiki/Universal_hashing
        private void GenerateHashFunctions(int u)
        {
            hashFunctions = new Hash[numHashFunctions];

            // will get the same hash functions each time since the same random number seed is used
            Random r = new Random(10);
            for (int i = 0; i < numHashFunctions; i++)
            {
                uint a = 0;
                // parameter a is an odd positive
                while (a % 1 == 1 || a <= 0)
                    a = (uint)r.Next();
                uint b = 0;
                int maxb = 1 << u;
                // parameter b must be greater than zero and less than universe size
                while (b <= 0)
                    b = (uint)r.Next(maxb);
                hashFunctions[i] = x => QHash(x, a, b, u);
            }
        }

        // Returns the number of bits needed to store the universe
        public int BitsForUniverse(int universeSize)
        {
            return (int)Math.Truncate(Math.Log((double)universeSize, 2.0)) + 1;
        }

        // Universal hash function with two parameters a and b, and universe size in bits
        private static uint QHash(int x, uint a, uint b, int u)
        {
            return (a * (uint)x + b) >> (32 - u);
        }

        // Returns the list of min hashes for the given set of word Ids
        public List<uint> GetMinHash(List<int> wordIds)
        {
            uint[] minHashes = new uint[numHashFunctions];
            for (int h = 0; h < numHashFunctions; h++)
            {
                minHashes[h] = int.MaxValue;
            }
            foreach (int id in wordIds)
            {
                for (int h = 0; h < numHashFunctions; h++)
                {
                    uint hash = hashFunctions[h](id);
                    minHashes[h] = Math.Min(minHashes[h], hash);
                }
            }
            return minHashes.ToList();
        }

        // Calculates the similarity of two lists of min hash values. Approximately Numerically equivilant to Jaccard Similarity
        public static double Similarity(List<uint> l1, List<uint> l2)
        {
           return Calc(l1, l2);
        }
    }
}
