using System;
using System.Collections.Generic;
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
      var csvr = new CsvReader2();
      foreach (var item in csvr.Parse(@"C:\Users\Pieter\AppData\Local\Temp\033a3064-3081-4ce5-a600-267e1ce80d33.csv"))
      {

      }
    }
  }
}
