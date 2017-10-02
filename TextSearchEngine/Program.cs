using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TextSearchEngine
{
    class Program
    {
        static void Main(string[] args)
        {
            String directory = (args.Length > 0) ? args[0] : "D:/dump/";
            Console.InputEncoding = System.Text.Encoding.Unicode;
            Console.OutputEncoding = System.Text.Encoding.Unicode;

            TextSearcher searcher = new TextSearcher(directory);

            while (true)
            {
                Console.Write("Искать по: ");
                String query = Console.ReadLine();
                if (query.Length == 0 || !searcher.IsValidQuery(query))
                    Console.WriteLine("Запрос не может быть пуст или состоять только из стоп-слов");
                else
                {
                    SortedSet<Tuple<string, double>> searchRes = searcher.SearchForQuery(query);
                    Console.WriteLine("Результаты поиска в " + directory + ":");
                    foreach (Tuple<string, double> t in searchRes)
                        Console.WriteLine("Файл: " + Path.GetFileName(t.Item1)
                            + "\nРелевантность: " + t.Item2.ToString() + "\n");
                }
            }
        }
    }
}
