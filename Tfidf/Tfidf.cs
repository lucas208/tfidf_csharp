using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;

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

    public static List<List<string>> LeituraParcial(int linhaInicial, int linhaFinal)
    {
        var path = @"C:\Users\usuario\Documents\Lucas\Estudos\IMD\Engsoft 2024.1\Concorrente\Dataset\reviews.csv";
        var documents = new List<List<string>>();
        var linhaAtual = 0;

        try
        {
            using (var reader = new StreamReader(path))
            {
                string line;
                while ((line = reader.ReadLine()) != null)
                {
                    linhaAtual++;

                    if (linhaAtual < linhaInicial)
                    {
                        continue;
                    }
                    if (linhaAtual > linhaFinal) {
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

    public class Leitor
    {
        private readonly int linhaInicial;
        private readonly int linhaFinal;

        private List<List<string>> documents;

        public Leitor(int linhaInicial, int linhaFinal)
        {
            this.linhaInicial = linhaInicial;
            this.linhaFinal = linhaFinal;
            this.documents = new List<List<string>>();
        }

        public void Run()
        {
            Console.WriteLine("Entrou na thread");
            documents = LeituraParcial(linhaInicial, linhaFinal);
            Console.WriteLine("Saiu da thread");
        }

        public List<List<string>> GetDocuments()
        {
            return documents;
        }
    }

    public class Calculador
    {
        private double tfidf;
        private readonly string document;
        private readonly List<List<string>> documents;

        public Calculador(string document, List<List<string>> documents)
        {
            this.document = document;
            this.documents = documents;
        }

        public void Run()
        {
            Console.WriteLine("Entrou na thread");
            var calculator = new Tfidf();
            tfidf = calculator.TfIdf(document, documents, "content");
            Console.WriteLine("Saiu da thread");
        }

        public double GetTfidf()
        {
            return tfidf;
        }
    }

    public static void Main(string[] args)
    {
        var leitor1 = new Leitor(1, 1000000);
        var leitor2 = new Leitor(1000001, 2000000);
        var leitor3 = new Leitor(2000001, 3000000);

        var tempoInicial = DateTime.Now;

        var thread1 = new Thread(leitor1.Run);
        var thread2 = new Thread(leitor2.Run);
        var thread3 = new Thread(leitor3.Run);

        thread1.Start();
        thread2.Start();
        thread3.Start();

        thread1.Join();
        var documents1 = leitor1.GetDocuments();
        thread2.Join();
        var documents2 = leitor2.GetDocuments();
        thread3.Join();
        var documents3 = leitor3.GetDocuments();

        var tempoFinal = DateTime.Now;

        Console.WriteLine($"Tempo de leitura do dataset: {(tempoFinal - tempoInicial).TotalSeconds}s");

        var calculador1 = new Calculador(string.Join(" ", documents1[1]), documents1);
        var calculador2 = new Calculador(string.Join(" ", documents1[1]), documents2);
        var calculador3 = new Calculador(string.Join(" ", documents1[1]), documents3);

        tempoInicial = DateTime.Now;

        var thread4 = new Thread(calculador1.Run);
        var thread5 = new Thread(calculador2.Run);
        var thread6 = new Thread(calculador3.Run);

        thread4.Start();
        thread5.Start();
        thread6.Start();

        thread4.Join();
        var tfidf1 = calculador1.GetTfidf();
        thread5.Join();
        var tfidf2 = calculador2.GetTfidf();
        thread6.Join();
        var tfidf3 = calculador3.GetTfidf();

        tempoFinal = DateTime.Now;

        Console.WriteLine($"Tempo de cálculo do TF-IDF (aproximado): {(tempoFinal - tempoInicial).TotalSeconds}s");
        Console.WriteLine($"TF-IDF (aproximado) = {(tfidf1 + tfidf2 + tfidf3) / 3.0}");
        Console.WriteLine("Pressione qualquer tecla para sair...");
        Console.ReadKey();
    }
}
