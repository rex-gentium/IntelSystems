using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TextSearchEngine
{
    class StemWord
    {
        private string word;
        public int RVIndex { get; }
        public int R1Index { get; }
        public int R2Index { get; }

        public StemWord(string word)
        {
            this.word = word;
            RVIndex = IndexOfRV(word);
            R1Index = IndexOfR1(word);
            R2Index = IndexOfR2(word);
        }

        public string GetWord() => (string) word.Clone();
        public string GetRV() 
            => (RVIndex >= 0 && RVIndex < word.Length) ? word.Substring(RVIndex) : null;
        public string GetR1()
            => (R1Index >= 0 && R1Index < word.Length) ? word.Substring(R1Index) : null;
        public string GetR2()
            => (R2Index >= 0 && R2Index < word.Length) ? word.Substring(R2Index) : null;

        /* Возвращает индекс начала RV-подстроки слова.
         * Если RV-подстроку не удается выделить, возвращает -1.*/
        private static int IndexOfRV(string word)
        {
            /* RV is a region after the first vowel,
             * or the end of the word if it contains no vowel */
            for (int i = 0; i < word.Length - 1; ++i)
                if (Stemmer.IsVowel(word[i]))
                    return i + 1;
            return -1;
        }

        /* Возвращает индекс начала R1-подстроки слова.
         * Если R1-подстроку не удается выделить, возвращает -1.*/
        private static int IndexOfR1(string word)
        {
            /* R1 is the region after the first non-vowel following a vowel,
             * or the end of the word if there is no such non-vowel. */
            for (int i = 1; i < word.Length - 1; ++i)
                if (!Stemmer.IsVowel(word[i]) && Stemmer.IsVowel(word[i - 1]))
                    return i + 1;
            return -1;
        }

        /* Возвращает индекс начала R2-подстроки слова.
         * Если R2-подстроку не удается выделить, возвращает -1.*/
        private static int IndexOfR2(string word)
        {
            /* R2 is the region after the first non-vowel following a vowel in R1,
            * or the end of the word if there is no such non-vowel. */
            int r1 = IndexOfR1(word);
            if (r1 < 0) return -1;
            int r2 = IndexOfR1(word.Substring(r1));
            return (r2 >= 0) ? r1 + r2 : -1;
        }

        /* удаляет символы в слове начиная с указанной позиции, возвращает true.
         * если индекс был за границами массива строки, возвращает false.
         * offset - индекс, относительно которого задан endingIndex*/
        public bool TryRemove(int offset, int endingIndex)
        {
            if (endingIndex < 0 || offset + endingIndex > word.Length) return false;
            word = word.Substring(0, offset + endingIndex);
            return true;
        }

        /* если слово оканчивается на двойную н, заменяет её на одинарную и возвращает true,
         * иначе возвращает false. */
        public bool UndoubleNN()
        {
            if (!word.EndsWith("нн")) return false;
            RemoveLast();
            return true;
        }

        /* удаляет один символ с конца слова */
        public void RemoveLast()
        {
            word = word.Substring(0, word.Length - 1);
        }
    }
}
