using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace tfidf
{
    class Tfidf
    {
        public double TF(List<string> doc, string term)
        {
            double result = 0;
            foreach (string word in doc)
            {
                if (string.Equals(term, word, StringComparison.OrdinalIgnoreCase))
                    result++;
            }
            return result / doc.Count;
        }

        public double IDF(List<List<string>> docs, string term)
        {
            double n = 0;
            foreach (List<string> doc in docs)
            {
                if (doc.Any(word => string.Equals(term, word, StringComparison.OrdinalIgnoreCase)))
                    n++;
            }
            return Math.Log(docs.Count / n);
        }

        public double TFIDF(List<string> doc, List<List<string>> docs, string term)
        {
            return TF(doc, term) * IDF(docs, term);
        }

        public static void Main(string[] args)
        {
            List<string> doc1 = new List<string> { "Lorem", "ipsum", "dolor", "ipsum", "sit", "ipsum" };
            List<string> doc2 = new List<string> { "Vituperata", "incorrupte", "at", "ipsum", "pro", "quo" };
            List<string> doc3 = new List<string> { "Has", "persius", "disputationi", "id", "simul" };
            List<List<string>> documents = new List<List<string>> { doc1, doc2, doc3 };

            Tfidf calculator = new Tfidf();
            double tfidf = calculator.TFIDF(doc1, documents, "ipsum");
            Console.WriteLine("TF-IDF (ipsum) = " + tfidf);
        }
    }
}
