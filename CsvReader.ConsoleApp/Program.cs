using System;
using System.Diagnostics;

namespace CsvReader.ConsoleApp
{
  class Program
  {
    static void Main(string[] args)
    {
      CompareReaderSpeed();
    }

    static void CompareReaderSpeed()
    {
      const string filePath = @"C:\Temp\RawPWExportLarge2.csvlike";
      var csvReader10TotalSeconds = 0d;

      for (int i = 0; i < 50; i++)
      {
        var csvReader = new System.IO.CsvReader();
        var swCsvReader = new Stopwatch();
        swCsvReader.Start();
        foreach (var parsedRow in csvReader.Parse(filePath))
        {

        }
        swCsvReader.Stop();
        if (i != 0) { csvReader10TotalSeconds += swCsvReader.Elapsed.TotalSeconds; }
      }
      Console.WriteLine($"Reader 10: {csvReader10TotalSeconds}");

      Console.ReadLine();
    }
  }
}
