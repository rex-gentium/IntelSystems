using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TextSearchEngine
{
    abstract class Stemmer
    {
        public static readonly char Space = ' ';
        public static readonly char[] Digits = "0123456789".ToCharArray();
        public static readonly char[] Alphabet = "абвгдеёжзийклмнопрстуфхцчшщъыьэюя".ToCharArray();
        public static readonly string[] StopWords = File.ReadAllLines("C:/dump/stopwords.txt", Encoding.UTF8)
            .Union(Digits.Concat(Alphabet).Select(c => c.ToString()))
            .ToArray();
        public static readonly char[] Vowels = "аеёиоуыэюя".ToCharArray();
        
        /* нижеследующие массивы содержат шаблоны окончаний слов, обязаны быть отсортированы по длине элементов */
        public static readonly string[] PerfectiveGerundEndings1 = new [] { "в", "вши", "вшись" }.OrderByDescending(s => s.Length).ToArray();
        public static readonly string[] PerfectiveGerundEndings2 = new[] { "ив", "ивши", "ившись", "ыв", "ывши", "ывшись" }.OrderByDescending(s => s.Length).ToArray();
        public static readonly string[] AdjectiveEndings = new[] { "ее" , "ие", "ые", "ое",
            "ими", "ыми",
            "ей", "ий", "ый", "ой",
            "ем", "им", "ым", "ом",
            "его", "ого",
            "ему", "ому",
            "их", "ых",
            "ую", "юю",
            "ая" , "яя" ,
            "ою" , "ею" }.OrderByDescending(s => s.Length).ToArray();
        public static readonly string[] ParticipleEndings1 = new[] { "ем", "нн", "вш", "ющ", "щ" }.OrderByDescending(s => s.Length).ToArray();
        public static readonly string[] ParticipleEndings2 = new[] { "ивш", "ывш", "ующ" }.OrderByDescending(s => s.Length).ToArray();
        public static readonly string[] ReflexiveEndings = new[] { "ся", "сь" }.OrderByDescending(s => s.Length).ToArray();
        public static readonly string[] VerbEndings1 = new[] { "ла", "на", "ете", "йте", "ли", "й", "л", "ем", "н", "ло",
            "но", "ет", "ют", "ны", "ть", "ешь", "нно" }.OrderByDescending(s => s.Length).ToArray();
        public static readonly string[] VerbEndings2 = new[] { "ила", "ыла", "ена", "ейте", "уйте", "ите", "или", "ыли",
            "ей", "уй", "ил", "ыл", "им", "ым", "ен", "ило", "ыло", "ено", "ят", "ует", "уют", "ит", "ыт", "ены", "ить", "ыть", "ишь", "ую", "ю" }.OrderByDescending(s => s.Length).ToArray();
        public static readonly string[] NounEndings = new[] { "а", "ев", "ов", "ие", "ье", "е", "иями", "ями", "ами", "еи",
            "ии", "и", "ией", "ей", "ой", "ий", "й", "иям", "ям", "ием", "ем", "ам", "ом", "о", "у", "ах", "иях", "ях",
            "ы", "ь", "ию", "ью", "ю", "ия", "ья", "я" }.OrderByDescending(s => s.Length).ToArray();
        public static readonly string[] SuperlativeEndings = new[] { "ейш", "ейше" }.OrderByDescending(s => s.Length).ToArray();
        public static readonly string[] DerivationalEndings = new[] { "ост", "ость" }.OrderByDescending(s => s.Length).ToArray();

        public static bool IsAlphaSpaceDigit(char c)
            => c.Equals(Space) || Alphabet.Contains(c) || Digits.Contains(c);

        public static bool IsVowel(char c)
            => Vowels.Contains(c);

        /* Возвращает индекс, с которого в строке начинается окончание определенного класса.
         * Если окончание не было обнаружено, возвращает -1. */
        private static int IndexOfEnding(string word, string[] endings)
        {
            if (word == null) return -1;
            foreach (string ending in endings)
                if (word.EndsWith(ending))
                    return word.Length - ending.Length;
            return -1;
        }

        /* Возвращает индекс, с которого в строке начинается окончание определенного класса,
         * при условии, что этому окончанию предшествует "а" или "я".
         * Если окончание не было обнаружено, возвращает -1. */
        private static int IndexOfEndingAfterA(string word, string[] endings)
        {
            foreach (string ending in endings)
            {
                if (word.EndsWith(ending) && word.Length > ending.Length)
                {
                    /* Ending should be preceded by a or я*/
                    char[] wordChars = word.ToCharArray();
                    int endingIndex = wordChars.Length - ending.Length;
                    char preced = wordChars[endingIndex - 1];
                    if (preced.Equals('а') || preced.Equals('я'))
                        return endingIndex;
                }
            }
            return -1;
        }

        private static int IndexOfGerundEnding(string word)
        {
            int resultGroup1 = IndexOfEndingAfterA(word, PerfectiveGerundEndings1);
            return (resultGroup1 >= 0) ? resultGroup1 
                : IndexOfEnding(word, PerfectiveGerundEndings2);
        }

        private static int IndexOfReflexiveEnding(string word)
            => IndexOfEnding(word, ReflexiveEndings);

        private static int IndexOfAdjectiveEnding(string word)
            => IndexOfEnding(word, AdjectiveEndings);

        private static int IndexOfAdjectivalEnding(string word)
        {
            /* Define an ADJECTIVAL ending as an ADJECTIVE ending
             * optionally preceded by a PARTICIPLE ending. */
            int adjectiveEnding = IndexOfAdjectiveEnding(word);
            if (adjectiveEnding >= 0)
            {
                string preceding = word.Substring(0, word.Length - adjectiveEnding + 1);
                int participleEnding = IndexOfEndingAfterA(preceding, ParticipleEndings1);
                if (participleEnding < 0)
                    participleEnding = IndexOfEnding(preceding, ParticipleEndings2);
                return (participleEnding >= 0) ? participleEnding : adjectiveEnding;
            }
            return -1;
        }

        private static int IndexOfVerbEnding(string word)
        {
            int resGroup1 = IndexOfEndingAfterA(word, VerbEndings1);
            return (resGroup1 >= 0) ? resGroup1 : IndexOfEnding(word, VerbEndings2);
        }

        private static int IndexOfNounEnding(string word)
            => IndexOfEnding(word, NounEndings);

        private static int IndexOfDerivationalEnding(string word)
            => IndexOfEnding(word, DerivationalEndings);

        private static int IndexOfSuperlativeEnding(string word)
            => IndexOfEnding(word, SuperlativeEndings);

        /* возвращает стем слова. если стем не удалось выделить, возвращает null */
        public static string Stem(string word)
        {
            /* All tests take place in the the RV part of the word. */
            StemWord stemWord = new StemWord(word);
            string rv = stemWord.GetRV();
            if (rv == null) return stemWord.GetWord();
            /* All tests are in RV */
            /* Step 1: Search for a PERFECTIVE GERUND ending. If one is found remove it, and that is then the end of step 1.*/
            int perfectiveGerundIndex = IndexOfGerundEnding(rv);
            bool step1End = stemWord.TryRemove(stemWord.RVIndex, perfectiveGerundIndex);
            if (!step1End)
            {
                /* Otherwise try and remove a REFLEXIVE ending, */
                int reflexiveIndex = IndexOfReflexiveEnding(rv);
                stemWord.TryRemove(stemWord.RVIndex, reflexiveIndex);
                /* and then search in turn for (1) an ADJECTIVAL, (2) a VERB or (3) a NOUN ending. As soon as one of the endings
                 * (1) to (3) is found remove it, and terminate step 1. */
                int adjectivalIndex = IndexOfAdjectivalEnding(rv);
                step1End = stemWord.TryRemove(stemWord.RVIndex, adjectivalIndex);
                if (!step1End)
                {
                    int verbIndex = IndexOfVerbEnding(rv);
                    step1End = stemWord.TryRemove(stemWord.RVIndex, verbIndex);
                    if (!step1End)
                    {
                        int nounIndex = IndexOfNounEnding(rv);
                        stemWord.TryRemove(stemWord.RVIndex, nounIndex);
                    }
                }
            }
            /* Step 2: If the word ends with и, remove it. */
            rv = stemWord.GetRV();
            if (rv != null && rv.EndsWith("и"))
                stemWord.RemoveLast();
            /* Step 3: Search for a DERIVATIONAL ending in R2 (i.e. the entire ending must lie in R2), and if one is found, remove it. */
            string r2 = stemWord.GetR2();
            int derivationalIndex = IndexOfDerivationalEnding(r2);
            stemWord.TryRemove(stemWord.R2Index, derivationalIndex);
            /* Step 4: (1) Undouble н (n), or, (2) if the word ends with a SUPERLATIVE ending, remove it and undouble н (n), or (3) if the word ends ь (') (soft sign) remove it.  */
            rv = stemWord.GetRV();
            if (rv != null && rv.EndsWith("ь"))
                stemWord.RemoveLast();
            else
            {
                int superlativeIndex = IndexOfSuperlativeEnding(rv);
                stemWord.TryRemove(stemWord.RVIndex, superlativeIndex);
                stemWord.UndoubleNN();
            }
            return stemWord.GetWord();
        }

        
    }
}
