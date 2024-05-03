using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;

public class Tfidf
{
    private static readonly object locker = new object();
    private static readonly List<List<string>> documentosCompartilhados = new List<List<string>>();

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

        public Leitor(int linhaInicial, int linhaFinal)
        {
            this.linhaInicial = linhaInicial;
            this.linhaFinal = linhaFinal;
        }

        public void Run()
        {
            var documents = LeituraParcial(linhaInicial, linhaFinal);
            Console.WriteLine("Entrou na thread de leitura");

            lock (locker)
            {
                Console.WriteLine("Entrou na seção crítica de leitura");
                documentosCompartilhados.AddRange(documents);
                Console.WriteLine("Saiu da seção crítica de leitura");
            }
            Console.WriteLine("Saiu da thread de leitura");
        }
    }

    public class Calculador
    {
        private double tfidf;
        private readonly string document;
        private readonly int linhaInicial;
        private readonly int linhaFinal;

        public Calculador(string document, int linhaInicial, int linhaFinal)
        {
            this.document = document;
            this.linhaInicial = linhaInicial;
            this.linhaFinal = linhaFinal;
        }

        public void Run()
        {
            Console.WriteLine("Entrou na thread de cálculo");

            List<List<string>> documents;
            lock (locker)
            {
                Console.WriteLine("Entrou na seção crítica de cálculo");
                documents = documentosCompartilhados.GetRange(linhaInicial, linhaFinal - linhaInicial);
                Console.WriteLine("Saiu da seção crítica de cálculo");
            }

            var calculator = new Tfidf();
            tfidf = calculator.TfIdf(document, documents, "content");

            Console.WriteLine("Saiu da thread de cálculo");
        }

        public double GetTfidf()
        {
            return tfidf;
        }
    }

    public static void Main(string[] args)
    {
        var leitor1 = new Leitor(1, 750000);
        var leitor2 = new Leitor(750001, 1500000);
        var leitor3 = new Leitor(1500001, 2250000);
        var leitor4 = new Leitor(2250001, 3000000);

        var tempoInicial = DateTime.Now;

        var thread1 = new Thread(leitor1.Run);
        var thread2 = new Thread(leitor2.Run);
        var thread3 = new Thread(leitor3.Run);
        var thread4 = new Thread(leitor4.Run);

        thread1.Start();
        thread2.Start();
        thread3.Start();
        thread4.Start();

        thread1.Join();
        thread2.Join();
        thread3.Join();
        thread4.Join();

        var tempoFinal = DateTime.Now;

        Console.WriteLine($"Tempo de leitura do dataset: {(tempoFinal - tempoInicial).TotalSeconds}s");

        var calculador1 = new Calculador(string.Join(" ", documentosCompartilhados[1]), 0, 750000);
        var calculador2 = new Calculador(string.Join(" ", documentosCompartilhados[1]), 750000, 1500000);
        var calculador3 = new Calculador(string.Join(" ", documentosCompartilhados[1]), 1500000, 2250000);
        var calculador4 = new Calculador(string.Join(" ", documentosCompartilhados[1]), 2250000, 3000000);

        tempoInicial = DateTime.Now;

        var thread5 = new Thread(calculador1.Run);
        var thread6 = new Thread(calculador2.Run);
        var thread7 = new Thread(calculador3.Run);
        var thread8 = new Thread(calculador4.Run);

        thread5.Start();
        thread6.Start();
        thread7.Start();
        thread8.Start();

        thread5.Join();
        var tfidf1 = calculador1.GetTfidf();
        thread6.Join();
        var tfidf2 = calculador2.GetTfidf();
        thread7.Join();
        var tfidf3 = calculador3.GetTfidf();
        thread8.Join();
        var tfidf4 = calculador4.GetTfidf();

        tempoFinal = DateTime.Now;

        Console.WriteLine($"Tempo de cálculo do TF-IDF (aproximado): {(tempoFinal - tempoInicial).TotalSeconds}s");
        Console.WriteLine($"TF-IDF (aproximado) = {(tfidf1 + tfidf2 + tfidf3 + tfidf4) / 4.0}");
        Console.WriteLine("Pressione qualquer tecla para sair...");
        Console.ReadKey();
    }
}