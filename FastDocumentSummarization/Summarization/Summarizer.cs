using FastDocumetSummarization.SentenceRanking;
using FastDocumetSummarization.TextModeling;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FastDocumetSummarization.Summarization
{
    class Summarizer
    {
        public static void generateModels()
        {
            string[] subdirectoryEntries = Directory.GetDirectories(@"C:\Users\Ercan\Desktop\PHD\2015\5thPaper\models\");
            foreach (string pathIn in subdirectoryEntries)
            {
                string fullPath = Path.GetFullPath(pathIn).TrimEnd(Path.DirectorySeparatorChar);
                string NfullPath = fullPath + @"\timelines\";
                string[] files = Directory.EnumerateFiles(pathIn, "*.*", SearchOption.AllDirectories).ToArray();
                foreach (string fileName in files)
                {

                    List<string> summaries = new List<string>();
                    summaries.AddRange(Helpers.readDocsFromOneFile(fileName));
                    List<string> tmp = new List<string>();
                    foreach (string sum in summaries)
                    {
                        if (sum.StartsWith("----"))
                        {
                            string newFileName = tmp[0];
                            StringBuilder sb = new StringBuilder();
                            for (int i = 1; i < tmp.Count; i++)
                            {
                                sb.AppendLine(tmp[i]);
                            }

                            string fnToWrite = fullPath + @"\" + newFileName + ".txt";
                            File.WriteAllText(fnToWrite, sb.ToString());
                            tmp.Clear();
                        }
                        else
                        {
                            tmp.Add(sum);
                        }
                    }
                }
            }
        }

        public static bool chechFileExt(string flName, string[] files)
        {
            bool r = false;
            foreach (var file in files)
            {
                string flnName2 = file.Split(Path.DirectorySeparatorChar).Last();
                if (flName.Equals(flnName2)) { r = true; break; }
            }
            return r;
        }

        public static void prepareForRouge()
        {
            string pathToModels = @"C:\Users\Ercan\Desktop\PHD\2015\5thPaper\models\";
            string pathToPeers = @"C:\Users\Ercan\Desktop\PHD\2015\5thPaper\peers\";

            string[] subdirectoryEntries = Directory.GetDirectories(pathToModels);
            StringBuilder sb = new StringBuilder();
            foreach (string pathIn in subdirectoryEntries)
            {
                string fullPath = Path.GetFullPath(pathIn).TrimEnd(Path.DirectorySeparatorChar);
                string[] files = Directory.EnumerateFiles(pathIn, "*.*", SearchOption.TopDirectoryOnly).ToArray();
                string subFolder = fullPath.Split(Path.DirectorySeparatorChar).Last();
                string[] filesInPeers = Directory.EnumerateFiles(pathToPeers + subFolder, "*.*", SearchOption.TopDirectoryOnly).ToArray();

                foreach (string fileName in files)
                {
                    string fileNameInner = fileName.Split(Path.DirectorySeparatorChar).Last();
                    if (chechFileExt(fileNameInner, filesInPeers))
                    {
                        string evalID = subFolder + fileNameInner;
                        string peersmodelFileName = fileNameInner;


                        string template = string.Format(
            @"<EVAL ID=""{0}"">
<PEER-ROOT>
/home/ercan/rouge/tlsum/peers/{1}/
</PEER-ROOT>
<MODEL-ROOT>
/home/ercan/rouge/tlsum/models/{1}/
</MODEL-ROOT>
<INPUT-FORMAT TYPE=""SPL"">
</INPUT-FORMAT>
<PEERS>
<P ID=""1"">{2}</P>
</PEERS>
<MODELS>
<M ID=""A"">{2}</M>
</MODELS>
</EVAL>", evalID, subFolder, peersmodelFileName);
                        sb.AppendLine(template);
                    }
                }
            }

            File.WriteAllText(@"C:\Users\Ercan\Desktop\PHD\2015\5thPaper\settings.xml", sb.ToString());
        }

        public static void GeneralAndQuerySummarization()
        {
            //
            string[] files = Directory.EnumerateFiles(@"C:\Users\Ercan\Desktop\PHD\archivePhD\in\duc2006\", "*.*", SearchOption.AllDirectories).ToArray();

            foreach (string file in files)
            {
                List<String> docs = new List<String>();
                docs.AddRange(Helpers.readDocsFromOneFile(file));
                List<string> summary = FastNodeRanking.SentenceRankingForGeneralSum(docs, 250, 100, 5, 4);
                string onlyFileName = Path.GetFileName(file);
                File.WriteAllLines(@"C:\Users\Ercan\Desktop\PHD\archivePhD\out\" + onlyFileName, summary);
            }
        }

        

        public static void TimeLineSummarization()
        {
            string[] subdirectoryEntries = Directory.GetDirectories(@"C:\Users\Ercan\Desktop\PHD\2015\5thPaper\Timeline17\Data");
            foreach (string pathIn in subdirectoryEntries)
            {
                string fullPathOrg = Path.GetFullPath(pathIn).TrimEnd(Path.DirectorySeparatorChar);
                string fileNameOrg = fullPathOrg.Split(Path.DirectorySeparatorChar).Last();

                string[] subsubdirectoryEntries = Directory.GetDirectories(pathIn + "\\InputDocs\\");
                foreach (string pathInIn in subsubdirectoryEntries)
                {
                    string fullPath = Path.GetFullPath(pathInIn).TrimEnd(Path.DirectorySeparatorChar);
                    string fileName = fullPath.Split(Path.DirectorySeparatorChar).Last();
                    string[] files = Directory.EnumerateFiles(pathInIn, "*.*", SearchOption.AllDirectories).ToArray();
                    List<String> docs = new List<String>();
                    foreach (string file in files)
                    {
                        docs.AddRange(Helpers.readDocsFromOneFile(file));
                    }
                    List<string> summary = FastNodeRanking.SentenceRankingForGeneralSum(docs, 50, 80, 6, 4);
                    File.WriteAllLines(@"C:\Users\Ercan\Desktop\PHD\2015\5thPaper\peers\" + fileNameOrg + "\\" + fileName + ".txt", summary);
                }
            }


        }
       
        public static void ComparativeSummarization()
        {
            string[] files = Directory.EnumerateFiles(@"C:\Users\Ercan\Desktop\PHD\archivePhD\duc2007_compare\in", "*.*", SearchOption.AllDirectories).ToArray();
            List<String> docs = new List<String>();
            List<int> offsets = new List<int>();
            offsets.Add(0);
            foreach (string file in files)
            {
                List<string> fCon = Helpers.readDocsFromOneFile(file);
                int lst = (offsets[offsets.Count-1])+(fCon.Count);
                offsets.Add(lst);
                docs.AddRange(fCon);
            }

            List<string> summary = FastNodeRanking.SentenceRankingForComparativeSum(docs, offsets, 1000,100,5,5);
            File.WriteAllLines(@"C:\Users\Ercan\Desktop\PHD\archivePhD\duc2007_compare\out.txt", summary);

        }


    }
}