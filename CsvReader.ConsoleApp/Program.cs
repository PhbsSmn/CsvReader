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
      var csvReaderTotalSeconds = 0d;
      var csvReaderDupeTotalSeconds = 0d;

      for (int i = 0; i < 10; i++)
      {
        var csvReader = new System.IO.CsvReader();
        var swCsvReader = new Stopwatch();
        swCsvReader.Start();
        foreach (var parsedRow in csvReader.Parse(filePath))
        {

        }
        swCsvReader.Stop();
        if (i != 0) { csvReaderTotalSeconds += swCsvReader.Elapsed.TotalSeconds; }
      }
      Console.WriteLine($"Reader: {csvReaderTotalSeconds}");
      Console.WriteLine($"Reader dupe: {csvReaderDupeTotalSeconds}");

      Console.ReadLine();
    }
  }
}
