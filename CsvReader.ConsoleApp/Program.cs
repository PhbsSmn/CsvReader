using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
      var csvReader2TotalSeconds = 0d;
      var csvReader10TotalSeconds = 0d;

      for (int i = 0; i < 10; i++)
      {
        var csvReader2 = new CsvReader2();
        var swCsvReader2 = new Stopwatch();
        swCsvReader2.Start();
        foreach (var parsedRow in csvReader2.Parse(filePath))
        {

        }
        swCsvReader2.Stop();
        if (i != 0) { csvReader2TotalSeconds += swCsvReader2.Elapsed.TotalSeconds; }


        var csvReader10 = new CsvReader10();
        var swCsvReader10 = new Stopwatch();
        swCsvReader10.Start();
        foreach (var parsedRow in csvReader10.Parse(filePath))
        {

        }
        swCsvReader10.Stop();
        if (i != 0) { csvReader10TotalSeconds += swCsvReader10.Elapsed.TotalSeconds; }
      }

      Console.WriteLine($"Reader  2: {csvReader2TotalSeconds}");
      Console.WriteLine($"Reader 10: {csvReader10TotalSeconds}");

      Console.ReadLine();
    }
  }
}
