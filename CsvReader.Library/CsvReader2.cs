using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace System.IO
{
  /// <summary>
  /// A basic CSV reader, optimized for speed (re-visioned)
  /// </summary>
  public class CsvReader2
  {
    enum ReaderState
    {
      UndeterminedData,
      Data,
      DataCarriageReturn,
      TextData,
      DataDone,
      DataDoneRowComplete,
      TextDataEscape,
      EndRowCheck,
    }

    #region Members
    private readonly char[] _delimiter;
    private readonly char[] _textQualifier;
    private readonly char[] _endOfRowMarkers;
    private readonly int _bufferSize;
    #endregion

    #region Constructors
    /// <summary>
    /// Create and initialize a CSV parser
    /// </summary>
    /// <param name="delimiter">The char(s) to separate fields.</param>
    /// <param name="textQualifier">The char(s) to indicate a text field beginning and ending.</param>
    /// <param name="endOfRowMarker">The char(s) to indicate an end of row. By default \n & \r\n will be checked unless overriden then these will be ignored.</param>
    /// <param name="startLine">At which line the parser should start retrieving data.</param>
    public CsvReader2(string delimiter = ",", string textQualifier = "\"", string endOfRowMarker = null, int startLine = 0, int bufferSize = 84998)
    {
      _delimiter = delimiter.ToCharArray();
      _textQualifier = textQualifier.ToCharArray();
      _bufferSize = bufferSize;

      if (!string.IsNullOrEmpty(endOfRowMarker))
      {
        _endOfRowMarkers = endOfRowMarker.ToCharArray();
      }
    }
    #endregion

    #region Methods
    /// <summary>
    /// Parses a CSV file.
    /// </summary>
    /// <param name="filePath">The path where the file is located.</param>
    /// <returns>A formated string list</returns>
    public IEnumerable<List<string>> Parse(string filePath)
    {
      var resultList = new List<string>();
      var readerState = ReaderState.UndeterminedData;
      var dataResult = new StringBuilder();
      var isRowComplete = false;

      foreach (var fileCharChunk in FileToCharChunks(filePath))
      {
        for (var chunkIndex = 0; chunkIndex < fileCharChunk.Length;)
        {
          switch (readerState)
          {
            case ReaderState.UndeterminedData:
              {
                var currentChar = fileCharChunk[chunkIndex];
                if (_textQualifier[0].CompareTo(currentChar) == 0)
                {
                  chunkIndex++;
                  readerState = ReaderState.TextData;
                  break;
                }
                if (currentChar == '\0')
                {
                  yield break;
                }
                else
                {
                  readerState = ReaderState.Data;
                }
              }
              break;
            case ReaderState.Data:
              {
                var currentChar = fileCharChunk[chunkIndex];
                if (_endOfRowMarkers == null)
                {
                  if (currentChar.CompareTo('\n') == 0)
                  {
                    chunkIndex++;
                    readerState = ReaderState.DataDoneRowComplete;
                    break;
                  }

                  if (currentChar.CompareTo('\r') == 0)
                  {
                    chunkIndex++;
                    readerState = ReaderState.DataCarriageReturn;
                    break;
                  }

                }
                else
                {
                  throw new NotImplementedException();
                }

                if (currentChar.CompareTo(_delimiter[0]) == 0)
                {
                  if (_delimiter.Length == 1)
                  {
                    chunkIndex++;
                    readerState = ReaderState.DataDone;
                    break;
                  }
                  else
                  {
                    throw new NotImplementedException();
                  }
                }

                if (currentChar == '\0')
                {
                  readerState = ReaderState.DataDoneRowComplete;
                  break;
                }

                chunkIndex++;
                dataResult.Append(currentChar);
              }
              break;
            case ReaderState.TextData:
              {
                var currentChar = fileCharChunk[chunkIndex];
                if (currentChar.CompareTo(_textQualifier[0]) == 0)
                {
                  if (_textQualifier.Length == 1)
                  {
                    chunkIndex++;
                    readerState = ReaderState.TextDataEscape;
                    break;
                  }
                  else
                  {
                    throw new NotImplementedException();
                  }
                }

                chunkIndex++;
                dataResult.Append(currentChar);
              }
              break;
            case ReaderState.TextDataEscape:
              {
                var currentChar = fileCharChunk[chunkIndex];
                if (currentChar.CompareTo(_textQualifier[0]) == 0)
                {
                  if (_textQualifier.Length == 1)
                  {
                    chunkIndex++;
                    dataResult.Append(currentChar);
                    readerState = ReaderState.TextData;
                  }
                  else
                  {
                    throw new NotImplementedException();
                  }
                }
                else
                {
                  if (currentChar.CompareTo(_delimiter[0]) == 0)
                  {
                    if (_delimiter.Length == 1)
                    {
                      chunkIndex++;
                      readerState = ReaderState.DataDone;
                      break;
                    }
                    else
                    {
                      throw new NotImplementedException();
                    }
                  }

                  if (_endOfRowMarkers == null)
                  {
                    if (currentChar.CompareTo('\n') == 0)
                    {
                      chunkIndex++;
                      readerState = ReaderState.DataDoneRowComplete;
                      break;
                    }

                    if (currentChar.CompareTo('\r') == 0)
                    {
                      chunkIndex++;
                      readerState = ReaderState.DataCarriageReturn;
                      break;
                    }

                  }
                  else
                  {
                    throw new NotImplementedException();
                  }

                  if (currentChar == '\0')
                  {
                    readerState = ReaderState.DataDoneRowComplete;
                    break;
                  }

                  throw new InvalidDataException();
                }
              }
              break;
            case ReaderState.DataCarriageReturn:
              {
                var currentChar = fileCharChunk[chunkIndex];
                if (currentChar.CompareTo('\n') == 0)
                {
                  chunkIndex++;
                  readerState = ReaderState.DataDoneRowComplete;
                }
                else
                {
                  throw new InvalidDataException();
                }
              }
              break;
            case ReaderState.DataDone:
              {
                resultList.Add(dataResult.ToString());
                dataResult.Length = 0;
                readerState = ReaderState.UndeterminedData;
              }
              break;
            case ReaderState.DataDoneRowComplete:
              {
                resultList.Add(dataResult.ToString());
                dataResult.Length = 0;
                readerState = ReaderState.UndeterminedData;
                yield return resultList;
                resultList = new List<string>();
              }
              break;
          }
        }
      }
    }
    #endregion

    #region File
    private IEnumerable<char[]> FileToCharChunks(string filePath)
    {
      // Check if the file and store path exists.
      if (File.Exists(filePath))
      {
        // Open the file.
        using (var fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read))
        {
          using (var streamReader = new StreamReader(fileStream))
          {
            char[] fileContents;
            int charsRead;
            do
            {
              fileContents = new char[_bufferSize];
              charsRead = streamReader.Read(fileContents, 0, _bufferSize);
              yield return fileContents;
            } while (charsRead > 0);
          }
        }
      }
    }
    #endregion

    #region Data
    private bool IsDataMarker(IEnumerable<char> fileChars, char[] marker)
    {
      var index = 0;
      foreach (var fileChar in fileChars)
      {
        if (!fileChar.Equals(marker[index]))
        {
          return false;
        }
        index++;
      }

      return true;
    }
    #endregion
  }
}
