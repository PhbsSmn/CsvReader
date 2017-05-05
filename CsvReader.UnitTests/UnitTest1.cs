using System;
using System.IO;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CsvReader.UnitTests
{
  [TestClass]
  public class UnitTest1
  {
    [TestMethod]
    public void NormalDataEnd()
    {
      const string CSV_CONTENT = @"test,""data"",123";
      var filePath = GetUniqueFilePath();

      ManageTempFile(filePath, CSV_CONTENT, () =>
      {
        var reader = new System.IO.CsvReader();
        var result = reader.Parse(filePath).ToList();
        Assert.AreEqual(1, result.Count);
        Assert.AreEqual("test", result[0][0]);
        Assert.AreEqual("data", result[0][1]);
        Assert.AreEqual("123", result[0][2]);
      });
    }
    [TestMethod]
    public void NormalTextDataEnd()
    {
      const string CSV_CONTENT = @"test,""data"",""123""";
      var filePath = GetUniqueFilePath();

      ManageTempFile(filePath, CSV_CONTENT, () =>
      {
        var reader = new System.IO.CsvReader();
        var result = reader.Parse(filePath).ToList();
        Assert.AreEqual(1, result.Count);
        Assert.AreEqual("test", result[0][0]);
        Assert.AreEqual("data", result[0][1]);
        Assert.AreEqual("123", result[0][2]);
      });
    }
    [TestMethod]
    public void DataWithDelimiter()
    {
      const string CSV_CONTENT = @"test,""d,ata"",123";
      var filePath = GetUniqueFilePath();

      ManageTempFile(filePath, CSV_CONTENT, () =>
      {
        var reader = new System.IO.CsvReader();
        var result = reader.Parse(filePath).ToList();
        Assert.AreEqual(1, result.Count);
        Assert.AreEqual("test", result[0][0]);
        Assert.AreEqual("d,ata", result[0][1]);
        Assert.AreEqual("123", result[0][2]);
      });
    }
    [TestMethod]
    public void DataWithMultiDelimiter()
    {
      const string CSV_CONTENT = @"test,@,""d,@,ata"",@,123";
      var filePath = GetUniqueFilePath();

      ManageTempFile(filePath, CSV_CONTENT, () =>
      {
        var reader = new System.IO.CsvReader(delimiter: ",@,");
        var result = reader.Parse(filePath).ToList();
        Assert.AreEqual(1, result.Count);
        Assert.AreEqual("test", result[0][0]);
        Assert.AreEqual("d,@,ata", result[0][1]);
        Assert.AreEqual("123", result[0][2]);
      });
    }
    [TestMethod]
    public void DataWithMultiDelimiter2()
    {
      const string CSV_CONTENT = @"test,@d,@,""d,@,ata"",@,123";
      var filePath = GetUniqueFilePath();

      ManageTempFile(filePath, CSV_CONTENT, () =>
      {
        var reader = new System.IO.CsvReader(delimiter: ",@,");
        var result = reader.Parse(filePath).ToList();
        Assert.AreEqual(1, result.Count);
        Assert.AreEqual("test,@d", result[0][0]);
        Assert.AreEqual("d,@,ata", result[0][1]);
        Assert.AreEqual("123", result[0][2]);
      });
    }
    [TestMethod]
    public void DataEndCustomEndOfRowMarker()
    {
      const string CSV_CONTENT = @"test,""data"",123|row2";
      var filePath = GetUniqueFilePath();

      ManageTempFile(filePath, CSV_CONTENT, () =>
      {
        var reader = new System.IO.CsvReader(endOfRowMarker: "|");
        var result = reader.Parse(filePath).ToList();
        Assert.AreEqual(2, result.Count);
        Assert.AreEqual("test", result[0][0]);
        Assert.AreEqual("data", result[0][1]);
        Assert.AreEqual("123", result[0][2]);
        Assert.AreEqual("row2", result[1][0]);
      });
    }
    [TestMethod]
    public void TextDataEndCustomEndOfRowMarker()
    {
      const string CSV_CONTENT = @"test,""data"",""123""|row2";
      var filePath = GetUniqueFilePath();

      ManageTempFile(filePath, CSV_CONTENT, () =>
      {
        var reader = new System.IO.CsvReader(endOfRowMarker: "|");
        var result = reader.Parse(filePath).ToList();
        Assert.AreEqual(2, result.Count);
        Assert.AreEqual("test", result[0][0]);
        Assert.AreEqual("data", result[0][1]);
        Assert.AreEqual("123", result[0][2]);
        Assert.AreEqual("row2", result[1][0]);
      });
    }
    [TestMethod]
    public void DataEndCustomMultiEndOfRowMarker()
    {
      const string CSV_CONTENT = @"test,""data"",123|#row2";
      var filePath = GetUniqueFilePath();

      ManageTempFile(filePath, CSV_CONTENT, () =>
      {
        var reader = new System.IO.CsvReader(endOfRowMarker: "|#");
        var result = reader.Parse(filePath).ToList();
        Assert.AreEqual(2, result.Count);
        Assert.AreEqual("test", result[0][0]);
        Assert.AreEqual("data", result[0][1]);
        Assert.AreEqual("123", result[0][2]);
        Assert.AreEqual("row2", result[1][0]);
      });
    }
    [TestMethod]
    public void DataEndCustomMultiEndOfRowMarker2()
    {
      const string CSV_CONTENT = @"test,""data"",12|3|#row2";
      var filePath = GetUniqueFilePath();

      ManageTempFile(filePath, CSV_CONTENT, () =>
      {
        var reader = new System.IO.CsvReader(endOfRowMarker: "|#");
        var result = reader.Parse(filePath).ToList();
        Assert.AreEqual(2, result.Count);
        Assert.AreEqual("test", result[0][0]);
        Assert.AreEqual("data", result[0][1]);
        Assert.AreEqual("12|3", result[0][2]);
        Assert.AreEqual("row2", result[1][0]);
      });
    }
    [TestMethod]
    public void TextDataEndCustomMultiEndOfRowMarker()
    {
      const string CSV_CONTENT = @"test,""data"",""123""|#row2";
      var filePath = GetUniqueFilePath();

      ManageTempFile(filePath, CSV_CONTENT, () =>
      {
        var reader = new System.IO.CsvReader(endOfRowMarker: "|#");
        var result = reader.Parse(filePath).ToList();
        Assert.AreEqual(2, result.Count);
        Assert.AreEqual("test", result[0][0]);
        Assert.AreEqual("data", result[0][1]);
        Assert.AreEqual("123", result[0][2]);
        Assert.AreEqual("row2", result[1][0]);
      });
    }
    [TestMethod]
    public void TextDataWithTextQualifier()
    {
      const string CSV_CONTENT = "test,\"d\"\"ata\",123";
      var filePath = GetUniqueFilePath();

      ManageTempFile(filePath, CSV_CONTENT, () =>
      {
        var reader = new System.IO.CsvReader();
        var result = reader.Parse(filePath).ToList();
        Assert.AreEqual(1, result.Count);
        Assert.AreEqual("test", result[0][0]);
        Assert.AreEqual("d\"ata", result[0][1]);
        Assert.AreEqual("123", result[0][2]);
      });
    }
    [TestMethod]
    public void TextDataWithMultiTextQualifier()
    {
      const string CSV_CONTENT = "test,\"@d\"@\"@ata\"@,123";
      var filePath = GetUniqueFilePath();

      ManageTempFile(filePath, CSV_CONTENT, () =>
      {
        var reader = new System.IO.CsvReader(textQualifier:"\"@");
        var result = reader.Parse(filePath).ToList();
        Assert.AreEqual(1, result.Count);
        Assert.AreEqual("test", result[0][0]);
        Assert.AreEqual("d\"@ata", result[0][1]);
        Assert.AreEqual("123", result[0][2]);
      });
    }
    [TestMethod]
    public void StartAt3Line()
    {
      const string CSV_CONTENT = @"

col1,col2
r1col1,r1col2";
      var filePath = GetUniqueFilePath();

      ManageTempFile(filePath, CSV_CONTENT, () =>
      {
        var reader = new System.IO.CsvReader(startAtLine: 2);
        var result = reader.Parse(filePath).ToList();
        Assert.AreEqual(2, result.Count);
        Assert.AreEqual("col1", result[0][0]);
        Assert.AreEqual("col2", result[0][1]);
        Assert.AreEqual("r1col1", result[1][0]);
        Assert.AreEqual("r1col2", result[1][1]);
      });

    }

    private static void ManageTempFile(string filePath, string content, Action action)
    {
      try
      {
        File.WriteAllText(filePath, content);

        action();
      }
      finally
      {
        File.Delete(filePath);
      }
    }

    private static string GetUniqueFilePath()
    {
      var fileName = Path.Combine(Path.GetTempPath(), Guid.NewGuid() + ".csv");
      if (File.Exists(fileName))
      {
        fileName = GetUniqueFilePath();
      }
      return fileName;
    }
  }
}
