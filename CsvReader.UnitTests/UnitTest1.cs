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
    public void ParseNormalDataEnd()
    {
      const string CSV_CONTENT = @"test,""data"",123";
      var filePath = GetUniqueFilePath();

      ManageTempFile(filePath, CSV_CONTENT, () =>
      {
        var reader = new CsvReader2(",", "\"");
        var result = reader.Parse(filePath).ToList();
        Assert.AreEqual(1, result.Count);
        Assert.AreEqual("test", result[0][0]);
        Assert.AreEqual("data", result[0][1]);
        Assert.AreEqual("123", result[0][2]);
      });
    }
    [TestMethod]
    public void ParseNormalTextDataEnd()
    {
      const string CSV_CONTENT = @"test,""data"",""123""";
      var filePath = GetUniqueFilePath();

      ManageTempFile(filePath, CSV_CONTENT, () =>
      {
        var reader = new CsvReader2(",", "\"");
        var result = reader.Parse(filePath).ToList();
        Assert.AreEqual(1, result.Count);
        Assert.AreEqual("test", result[0][0]);
        Assert.AreEqual("data", result[0][1]);
        Assert.AreEqual("123", result[0][2]);
      });
    }
    [TestMethod]
    public void ParseDataWithDelimiter()
    {
      const string CSV_CONTENT = @"test,""d,ata"",123";
      var filePath = GetUniqueFilePath();

      ManageTempFile(filePath, CSV_CONTENT, () =>
      {
        var reader = new CsvReader2(",", "\"");
        var result = reader.Parse(filePath).ToList();
        Assert.AreEqual(1, result.Count);
        Assert.AreEqual("test", result[0][0]);
        Assert.AreEqual("d,ata", result[0][1]);
        Assert.AreEqual("123", result[0][2]);
      });
    }
    [TestMethod]
    public void ParseDataWithTextQualifier()
    {
      const string CSV_CONTENT = "test,\"d\"\"ata\",123";
      var filePath = GetUniqueFilePath();

      ManageTempFile(filePath, CSV_CONTENT, () =>
      {
        var reader = new CsvReader2(",", "\"");
        var result = reader.Parse(filePath).ToList();
        Assert.AreEqual(1, result.Count);
        Assert.AreEqual("test", result[0][0]);
        Assert.AreEqual("d\"ata", result[0][1]);
        Assert.AreEqual("123", result[0][2]);
      });
    }

    [TestMethod]
    public void Parse10NormalDataEnd()
    {
      const string CSV_CONTENT = @"test,""data"",123";
      var filePath = GetUniqueFilePath();

      ManageTempFile(filePath, CSV_CONTENT, () =>
      {
        var reader = new CsvReader10(",", "\"");
        var result = reader.Parse(filePath).ToList();
        Assert.AreEqual(1, result.Count);
        Assert.AreEqual("test", result[0][0]);
        Assert.AreEqual("data", result[0][1]);
        Assert.AreEqual("123", result[0][2]);
      });
    }
    [TestMethod]
    public void Parse10NormalTextDataEnd()
    {
      const string CSV_CONTENT = @"test,""data"",""123""";
      var filePath = GetUniqueFilePath();

      ManageTempFile(filePath, CSV_CONTENT, () =>
      {
        var reader = new CsvReader10(",", "\"");
        var result = reader.Parse(filePath).ToList();
        Assert.AreEqual(1, result.Count);
        Assert.AreEqual("test", result[0][0]);
        Assert.AreEqual("data", result[0][1]);
        Assert.AreEqual("123", result[0][2]);
      });
    }
    [TestMethod]
    public void Parse10DataWithDelimiter()
    {
      const string CSV_CONTENT = @"test,""d,ata"",123";
      var filePath = GetUniqueFilePath();

      ManageTempFile(filePath, CSV_CONTENT, () =>
      {
        var reader = new CsvReader10(",", "\"");
        var result = reader.Parse(filePath).ToList();
        Assert.AreEqual(1, result.Count);
        Assert.AreEqual("test", result[0][0]);
        Assert.AreEqual("d,ata", result[0][1]);
        Assert.AreEqual("123", result[0][2]);
      });
    }
    [TestMethod]
    public void Parse10DataWithTextQualifier()
    {
      const string CSV_CONTENT = "test,\"d\"\"ata\",123";
      var filePath = GetUniqueFilePath();

      ManageTempFile(filePath, CSV_CONTENT, () =>
      {
        var reader = new CsvReader10(",", "\"");
        var result = reader.Parse(filePath).ToList();
        Assert.AreEqual(1, result.Count);
        Assert.AreEqual("test", result[0][0]);
        Assert.AreEqual("d\"ata", result[0][1]);
        Assert.AreEqual("123", result[0][2]);
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
