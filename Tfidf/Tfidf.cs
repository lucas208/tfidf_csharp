using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

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

    public static void Main(string[] args)
    {
        var tempoInicial = DateTime.Now;

        var path = @"C:\Users\030856141600\Desktop\Concorrente\Dataset\reviews.csv";
        var documents = new List<List<string>>();

        try
        {
            var lines = File.ReadAllLines(path);
            foreach (var line in lines)
            {
                var tokens = line.Split(",(?=([^\"]*\"[^\"]*\")*[^\"]*$)");
                var doc = tokens.Select(token => token.Trim('"').Trim()).ToList();
                documents.Add(doc);
            }
        }
        catch (IOException e)
        {
            Console.WriteLine(e.Message);
        }

        var tempoFinal = DateTime.Now;

        Console.WriteLine($"Tempo de leitura do dataset: {(tempoFinal - tempoInicial).TotalSeconds}s");

        var calculator = new Tfidf();

        Console.WriteLine($"DOC 1: {string.Join(", ", documents[1])}");

        tempoInicial = DateTime.Now;
        var tfidf = calculator.TfIdf(string.Join(" ", documents[1]), documents, "content");
        tempoFinal = DateTime.Now;

        Console.WriteLine($"Tempo de cálculo do TF-IDF: {(tempoFinal - tempoInicial).TotalSeconds}s");
        Console.WriteLine($"TF-IDF = {tfidf}");
    }
}
