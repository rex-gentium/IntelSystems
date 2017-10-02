using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TextSearchEngine
{
    /* Цель компаратора - сортировать пары "имя файла - релевантность файла"
     * по убыванию релевантности, допуская дублирующиеся значения релевантности.
     * Тогда возвращаемое значение Compare означает:
     * Меньше нуля - x > y (в стандартном компараторе x < y)
     * Ноль - x = y и они оба null (в стандартном компараторе x = y)
     * Больше нуля - x < y (в стандартном компараторе x > y) */

    class RelevanceComparer : IComparer<Tuple<string, double>>
    {
        public int Compare(Tuple<string, double> x, Tuple<string, double> y)
        {
            if (x == null)
            {
                // If x is null and y is null, they're equal. 
                // If x is null and y is not null, y is greater.
                return (y == null) ? 0 : 1;
            }
            else
            {
                // If x is not null and y is null, x is greater.
                if (y == null)
                    return -1;
                else
                {
                    // If x is not null and y is not null, compare the relevance of the two files.
                    int res = y.Item2.CompareTo(x.Item2);
                    // в случае если релевантности одинаковы, сортировать по имени файла
                    // файлов с совпадающими именами не предполагается
                    return (res != 0) ? res : x.Item1.CompareTo(y.Item1);
                }
            }
        }
    }
}
