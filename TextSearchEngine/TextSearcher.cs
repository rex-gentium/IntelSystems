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
        /* кэш стемов - по названию файла хранит частоты стемов в файле */
        private Dictionary<string, Dictionary<string, int>> stemCash;
        /* кэш весов - по названию файла хранит многомерный вектор весов стемов в файле */
        private Dictionary<string, Dictionary<string, double>> weightCash;

        public TextSearcher() : this("") { }

        public TextSearcher(String filesPath)
        {
            path = filesPath;
            String[] files = Directory.GetFiles(path, "*.txt", SearchOption.TopDirectoryOnly);
            stemCash = new Dictionary<string, Dictionary<string, int>>(files.Length);
            weightCash = new Dictionary<string, Dictionary<string, double>>(files.Length);
            foreach (String file in files)
            {
                String text = File.ReadAllText(file);
                String[] stems = Tokenize(text)
                    .Except(Stemmer.StopWords)
                    .Select(token => Stemmer.Stem(token))
                    .ToArray();
                stemCash[file] = BuildFrequencyTable(stems);
            }
            foreach (string file in stemCash.Keys)
            {
                Dictionary<string, double> weightTable = new Dictionary<string, double>();
                foreach (String stem in stemCash[file].Keys)
                    weightTable[stem] = GetStemWeightInFile(file, stem);
                weightCash[file] = weightTable;
            }
            //stemCash.Clear();
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
            Dictionary<string, double> queryVector = new Dictionary<string, double>(queryStems.Length);
            Dictionary<string, int> freqTable = BuildFrequencyTable(queryStems);
            foreach (string stem in queryStems)
            {
                double inverseFreq = GetInverseFrequency(stem);
                queryVector[stem] = freqTable[stem] * inverseFreq;
            }
            // список пар "имя файла - релевантность файла", отсортированный по убыванию релевантности
            SortedSet<Tuple<string, double>> fileToScore = new SortedSet<Tuple<string, double>>(new RelevanceComparer());
            foreach (string file in weightCash.Keys)
            {
                double fileScore = GetFileScore(file, queryVector);
                fileToScore.Add(Tuple.Create(file, fileScore));
            }
            return fileToScore;
        }

        private double GetInverseFrequency(string stem)
        {
            int filesWithStemCount = 0;
            foreach (string filename in stemCash.Keys)
                if (stemCash[filename].ContainsKey(stem))
                    ++filesWithStemCount;
            double inverseStemFreq = Math.Log(stemCash.Keys.Count / (double)filesWithStemCount);
            return inverseStemFreq;
        }

        private double GetStemWeightInFile(string file, string stem)
        {
            // посчитать частоту стема  файле
            if (!stemCash[file].ContainsKey(stem))
                return 0.0;
            int stemCountInFile = stemCash[file][stem];
            double stemFreq = stemCountInFile / (double) stemCash[file].Count;
            double inverseStemFreq = GetInverseFrequency(stem);
            return stemFreq * inverseStemFreq;
        }

        private double GetFileScore(string file, Dictionary<string, double> queryVector)
        {
            double vectorProd = 0.0;
            double fileVectorLength = 0.0;
            double queryVectorLength = 0.0;
            foreach(string stem in queryVector.Keys)
            {
                double weightInFile = (weightCash[file].ContainsKey(stem)) 
                    ? weightCash[file][stem] : 0.0;
                double weightInQuery = queryVector[stem];
                vectorProd += weightInFile * weightInQuery;
                fileVectorLength += weightInFile * weightInFile;
                queryVectorLength += weightInQuery * weightInQuery;
            }
            fileVectorLength = Math.Sqrt(fileVectorLength);
            queryVectorLength = Math.Sqrt(queryVectorLength);
            double lengthProd = fileVectorLength * queryVectorLength;
            return (lengthProd > 0) ? vectorProd / lengthProd : 0.0;
        }
    }
}
