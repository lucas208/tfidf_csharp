using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

public class Tfidf
{
    public double Tf(string comment, string term)
    {
        var words = comment.Replace(".", " ").Replace(",", "").Split(' ');
        var termFrequency = words.Count(word => term.Equals(word, StringComparison.OrdinalIgnoreCase));
        Console.WriteLine($"FREQ: {termFrequency}");
        Console.WriteLine($"LEN: {words.Length}");
        Console.WriteLine($"TF: {(double)termFrequency / words.Length}");

        return (double)termFrequency / words.Length;
    }

    public double Idf(List<List<string>> docs, string term)
    {
        double n = 1;
        foreach (var doc in docs)
        {
            var words = string.Join(" ", doc).Replace(".", " ").Replace(",", "").Split(' ');
            if (words.Any(word => term.Equals(word, StringComparison.OrdinalIgnoreCase)))
            {
                n++;
            }
        }
        Console.WriteLine($"n: {n}");
        Console.WriteLine($"Doc size(): {docs.Count}");
        Console.WriteLine($"IDF: {Math.Log(docs.Count / n)}");

        return Math.Log(docs.Count / n);
    }

    public double TfIdf(string doc, List<List<string>> docs, string term)
    {
        var tempoInicial = DateTime.Now;
        var tf = Tf(doc, term);
        var tempoFinal = DateTime.Now;
        Console.WriteLine($"Tempo de cálculo do TF: {(tempoFinal - tempoInicial).TotalMilliseconds}ms");

        tempoInicial = DateTime.Now;
        var idf = Idf(docs, term);
        tempoFinal = DateTime.Now;
        Console.WriteLine($"Tempo de cálculo do IDF: {(tempoFinal - tempoInicial).TotalSeconds}s");

        return tf * idf;
    }

    public static async Task<List<List<string>>> LeituraParcialAsync(int linhaInicial, int linhaFinal)
    {
        var path = @"C:\Users\usuario\Documents\Lucas\Estudos\IMD\Engsoft 2024.1\Concorrente\Dataset\reviews.csv";
        var documents = new List<List<string>>();
        var linhaAtual = 0;
        try
        {
            using (var reader = new StreamReader(path))
            {
                string line;
                while ((line = await reader.ReadLineAsync()) != null)
                {
                    linhaAtual++;
                    if (linhaAtual < linhaInicial)
                    {
                        continue;
                    }
                    if (linhaAtual > linhaFinal)
                    {
                        break;
                    }
                    var tokens = line.Split(",(?=([^\"]*\"[^\"]*\")*[^\"]*$)");
                    var doc = tokens.Select(token => token.Trim('"').Trim()).ToList();
                    documents.Add(doc);
                }
            }
        }
        catch (IOException e)
        {
            Console.WriteLine(e.Message);
        }

        return documents;
    }

    public class Calculo
    {
        private double tfidf;
        private readonly string document;
        private readonly List<List<string>> documents;

        public Calculo(string document, List<List<string>> documents)
        {
            this.document = document;
            this.documents = documents;
        }

        public void Run()
        {
            Console.WriteLine("Entrou na thread");
            Tfidf calculator = new Tfidf();
            tfidf = calculator.TfIdf(document, documents, "content");
            Console.WriteLine("Saiu da thread");
        }

        public double GetTfidf()
        {
            return tfidf;
        }
    }

    public static async Task Main(string[] args)
    {
        var leitura1 = LeituraParcialAsync(1, 1000000);
        var leitura2 = LeituraParcialAsync(1000001, 2000000);
        var leitura3 = LeituraParcialAsync(2000001, 3000000);

        var tempoInicial = DateTime.Now;

        var documents1 = await leitura1;
        var documents2 = await leitura2;
        var documents3 = await leitura3;

        var tempoFinal = DateTime.Now;

        Console.WriteLine($"Tempo de leitura do dataset: {(tempoFinal - tempoInicial).TotalSeconds}s");

        var calculo1 = new Calculo(string.Join(" ", documents1[1]), documents1);
        var calculo2 = new Calculo(string.Join(" ", documents1[1]), documents2);
        var calculo3 = new Calculo(string.Join(" ", documents1[1]), documents3);

        tempoInicial = DateTime.Now;

        calculo1.Run();
        calculo2.Run();
        calculo3.Run();

        var tfidf1 = calculo1.GetTfidf();
        var tfidf2 = calculo2.GetTfidf();
        var tfidf3 = calculo3.GetTfidf();

        tempoFinal = DateTime.Now;

        Console.WriteLine($"Tempo de cálculo do TF-IDF (aproximado): {(tempoFinal - tempoInicial).TotalSeconds}s");
        Console.WriteLine($"TF-IDF (aproximado) = {(tfidf1 + tfidf2 + tfidf3) / 3.0}");
        Console.WriteLine("Pressione qualquer tecla para sair...");
        Console.ReadKey();
    }
}
