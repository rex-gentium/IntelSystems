using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace TextSearchEngine
{
    class TextSearcher
    {
        private String path;
        private Dictionary<string, Dictionary<string, int>> stemCash;

        public TextSearcher() : this("") { }

        public TextSearcher(String filesPath)
        {
            path = filesPath;
            String[] files = Directory.GetFiles(path, "*.txt", SearchOption.TopDirectoryOnly);
            stemCash = new Dictionary<string, Dictionary<string, int>>(files.Length);
            foreach (String file in files)
            {
                String text = File.ReadAllText(file);
                String[] stems = Tokenize(text)
                    .Except(Stemmer.StopWords)
                    .Select(token => Stemmer.Stem(token))
                    .ToArray();
                Dictionary<string, int> freqTable = BuildFrequencyTable(stems);
                stemCash[file] = freqTable;
            }
        }

        private Dictionary<string, int> BuildFrequencyTable(string[] stems)
        {
            Dictionary<string, int> freqTable = new Dictionary<string, int>();
            foreach (string stem in stems)
                if (freqTable.ContainsKey(stem))
                    freqTable[stem]++;
                else freqTable[stem] = 1;
            return freqTable;
        }

        public static String[] Tokenize(String text)
        {
            char[] filteredText = text.ToLower()
                    .Replace('ё', 'е')
                    .Select(c => (Stemmer.IsAlphaSpaceDigit(c) ? c : ' '))
                    .ToArray();
            return Regex.Split(new string(filteredText), " +");
        }

        public bool IsValidQuery(string q)
        {
            return Tokenize(q).Except(Stemmer.StopWords).Count() > 0;
        }
        
        public SortedSet<Tuple<string, double>> SearchForQuery(string query)
        {
            String[] queryStems = Tokenize(query)
                    .Except(Stemmer.StopWords)
                    .Select(token => Stemmer.Stem(token))
                    .ToArray();
            // список пар "имя файла - релевантность файла", отсортированный по убыванию релевантности
            SortedSet<Tuple<string, double>> fileToRelevance = new SortedSet<Tuple<string, double>>(new RelevanceComparer());
            foreach (string file in stemCash.Keys)
            {
                double fileRelevance = 0.0;
                foreach (string stem in queryStems)
                    fileRelevance += GetFileRelevance(file, stem);
                fileToRelevance.Add(Tuple.Create(file, fileRelevance));
            }
            return fileToRelevance;
        }

        private double GetFileRelevance(string file, string stem)
        {
            // посчитать частоту стема  файле
            if (!stemCash[file].ContainsKey(stem))
                return 0.0;
            int stemCountInFile = stemCash[file][stem];
            // посчитать частоту стема во всех остальных файлах
            int stemCountInOtherFiles = 0;
            foreach (string otherFile in stemCash.Keys)
            {
                if (otherFile.Equals(file)) continue;
                if (stemCash[otherFile].ContainsKey(stem))
                    stemCountInOtherFiles += stemCash[otherFile][stem];
            }
            return (double)stemCountInFile / (double)stemCountInOtherFiles;
        }
    }
}
