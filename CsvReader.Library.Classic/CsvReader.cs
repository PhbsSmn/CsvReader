using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace System.IO
{
  /// <summary>
  /// A basic CSV reader, optimized for speed (re-visioned)
  /// </summary>
  public class CsvReader
  {
    enum ReaderState
    {
      UndeterminedData,
      Data,
      TextData,
      DataCarriageReturn,
      TextDataEscape,
      MultiCharDelimiter,
      MultiCharTextDelimiter,
      MultiCharEndOfRowMarker,
      MultiCharDataEndOfRowMarker,
      MultiCharStartTextQualifier,
      MultiCharTextQualifier,
      MultiCharEscapeTextQualifier,
    }

    #region Members
    private readonly char[] _delimiter;
    private readonly char[] _textQualifier;
    private readonly char[] _endOfRowMarkers;
    private readonly int _bufferSize;
    private readonly int _startAtLine;
    #endregion

    #region Constructors
    /// <summary>
    /// Create and initialize a CSV parser
    /// </summary>
    /// <param name="delimiter">The char(s) to separate fields.</param>
    /// <param name="textQualifier">The char(s) to indicate a text field beginning and ending.</param>
    /// <param name="endOfRowMarker">The char(s) to indicate an end of row. By default \n and \r\n will be checked unless overridden.</param>
    /// <param name="startAtLine">At which line the parser should start retrieving data.</param>
    /// <param name="bufferSize">The default amount of data read from file at once just large enough to just not end up in the LOH.</param>
    public CsvReader(string delimiter = ",", string textQualifier = "\"", string endOfRowMarker = null, int startAtLine = 0, int bufferSize = 84998)
    {
      _delimiter = delimiter.ToCharArray();
      _textQualifier = textQualifier.ToCharArray();
      _bufferSize = bufferSize;
      _startAtLine = startAtLine;

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
      var currentLine = 0;
      var resultList = new List<string>();
      var readerState = ReaderState.UndeterminedData;
      var dataResult = new StringBuilder();
      var currentMultiCharDelimiterIndex = 0;
      var currentMultiCharEndOfRowMarkerIndex = 0;
      var currentMultiCharTextQualifier = 0;
      foreach (var fileCharChunk in FileToCharChunks(filePath))
      {
        // ReSharper disable once ForCanBeConvertedToForeach, suggestion was not done because of speed drop
        for (var chunkIndex = 0; chunkIndex < fileCharChunk.Length; chunkIndex++)
        {
          var currentChar = fileCharChunk[chunkIndex];

          switch (readerState)
          {
            case ReaderState.UndeterminedData:
              if (_textQualifier[0].CompareTo(currentChar) == 0)
              {
                if (_textQualifier.Length == 1)
                {
                  readerState = ReaderState.TextData;
                }
                else
                {
                  currentMultiCharTextQualifier = 1;
                  readerState = ReaderState.MultiCharStartTextQualifier;
                }
                continue;
              }
              else { readerState = ReaderState.Data; goto case ReaderState.Data; }
            case ReaderState.Data:
              if (_endOfRowMarkers == null)
              {
                if (currentChar.CompareTo('\r') == 0) { readerState = ReaderState.DataCarriageReturn; continue; }
                if (currentChar.CompareTo('\n') == 0) { goto DataDoneRowComplete; }
              }
              else
              {
                if (currentChar.CompareTo(_endOfRowMarkers[0]) == 0) { goto CustomEndofRowMarkerHandling; }
              }

              if (currentChar.CompareTo(_delimiter[0]) == 0) { goto DelimiterHandling; }

              dataResult.Append(currentChar);
              continue;
            case ReaderState.TextData:
              if (currentChar.CompareTo(_textQualifier[0]) == 0)
              {
                if (_textQualifier.Length == 1) { readerState = ReaderState.TextDataEscape; continue; }
                else
                {
                  currentMultiCharTextQualifier = 1;
                  readerState = ReaderState.MultiCharTextQualifier; continue;
                }
              }

              dataResult.Append(currentChar);
              continue;
            case ReaderState.TextDataEscape:
              if (currentChar.CompareTo(_textQualifier[0]) == 0)
              {
                if (_textQualifier.Length == 1) { dataResult.Append(currentChar); readerState = ReaderState.TextData; continue; }
                else
                {
                  currentMultiCharTextQualifier = 1;
                  readerState = ReaderState.MultiCharEscapeTextQualifier; continue;
                }
              }
              else
              {
                if (currentChar.CompareTo(_delimiter[0]) == 0) { goto DelimiterHandling; }

                if (_endOfRowMarkers == null)
                {
                  if (currentChar.CompareTo('\r') == 0) { readerState = ReaderState.DataCarriageReturn; continue; }
                  if (currentChar.CompareTo('\n') == 0) { goto DataDoneRowComplete; }
                }
                else
                {
                  if (currentChar.CompareTo(_endOfRowMarkers[0]) == 0) { goto CustomEndofRowMarkerHandling; }
                }

                throw new InvalidDataException();
              }
            case ReaderState.DataCarriageReturn:
              {
                if (currentChar.CompareTo('\n') == 0) { goto DataDoneRowComplete; }
                throw new InvalidDataException();
              }
            case ReaderState.MultiCharDelimiter:
            case ReaderState.MultiCharTextDelimiter:
              if (currentChar.CompareTo(_delimiter[currentMultiCharDelimiterIndex]) == 0)
              {
                if (_delimiter.Length == ++currentMultiCharDelimiterIndex) { goto DataDone; }
                continue;
              }

              dataResult.Append(_delimiter.Take(currentMultiCharDelimiterIndex).ToArray());
              dataResult.Append(currentChar);
              if (readerState == ReaderState.MultiCharDelimiter) { readerState = ReaderState.Data; }
              else { readerState = ReaderState.TextData; }
              continue;

            case ReaderState.MultiCharEndOfRowMarker:
            case ReaderState.MultiCharDataEndOfRowMarker:
              // ReSharper disable once PossibleNullReferenceException
              if (currentChar.CompareTo(_endOfRowMarkers[currentMultiCharEndOfRowMarkerIndex]) == 0)
              {
                if (_endOfRowMarkers.Length == ++currentMultiCharEndOfRowMarkerIndex) { goto DataDoneRowComplete; }
              }
              else if (readerState == ReaderState.MultiCharDataEndOfRowMarker)
              {
                dataResult.Append(_endOfRowMarkers.Take(currentMultiCharDelimiterIndex + 1).ToArray());
                dataResult.Append(currentChar);
                readerState = ReaderState.Data;
                continue;
              }
              throw new InvalidDataException();
            case ReaderState.MultiCharStartTextQualifier:
              if (currentChar.CompareTo(_textQualifier[currentMultiCharTextQualifier]) == 0)
              {
                if (_textQualifier.Length == ++currentMultiCharTextQualifier)
                {
                  readerState = ReaderState.TextData;
                }
              }
              else
              {
                dataResult.Append(_textQualifier.Take(currentMultiCharTextQualifier + 1).ToArray());
                readerState = ReaderState.Data;
              }
              continue;
            case ReaderState.MultiCharTextQualifier:
              if (currentChar.CompareTo(_textQualifier[currentMultiCharTextQualifier]) == 0)
              {
                if (_textQualifier.Length == ++currentMultiCharTextQualifier)
                {
                  readerState = ReaderState.TextDataEscape;
                }
              }
              else
              {
                dataResult.Append(_textQualifier.Take(currentMultiCharTextQualifier + 1).ToArray());
                readerState = ReaderState.TextData;
              }
              continue;
            case ReaderState.MultiCharEscapeTextQualifier:
              if (currentChar.CompareTo(_textQualifier[currentMultiCharTextQualifier]) == 0)
              {
                if (_textQualifier.Length == ++currentMultiCharTextQualifier)
                {
                  dataResult.Append(_textQualifier.ToArray());
                  readerState = ReaderState.TextData;
                }
                continue;
              }
              throw new InvalidDataException();
          }

          DataDone:
          {
            resultList.Add(dataResult.ToString());
            dataResult.Length = 0;
            readerState = ReaderState.UndeterminedData;
            continue;
          }

          DataDoneRowComplete:
          {
            resultList.Add(dataResult.ToString());
            dataResult.Length = 0;
            if (_startAtLine <= currentLine)
            {
              yield return resultList;
            }
            else
            {
              currentLine++;
            }

            resultList = new List<string>();
            readerState = ReaderState.UndeterminedData;
            continue;
          }

          DelimiterHandling:
          {
            if (_delimiter.Length == 1) { goto DataDone; }
            currentMultiCharDelimiterIndex = 1;
            if (readerState == ReaderState.Data)
            {
              readerState = ReaderState.MultiCharDelimiter;
            }
            else
            {
              readerState = ReaderState.MultiCharTextDelimiter;
            }
            continue;
          }

          CustomEndofRowMarkerHandling:
          {
            if (_endOfRowMarkers.Length == 1) { goto DataDoneRowComplete; }

            currentMultiCharEndOfRowMarkerIndex = 1;
            if (readerState == ReaderState.Data)
            {
              readerState = ReaderState.MultiCharDataEndOfRowMarker;
            }
            else
            {
              readerState = ReaderState.MultiCharEndOfRowMarker;
            }
            // ReSharper disable once RedundantJumpStatement
            continue;
          }
        }
      }

      if (dataResult.Length != 0) { resultList.Add(dataResult.ToString()); }
      if (resultList.Count != 0)
      {
        yield return resultList;
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
            var readingFile = true;
            do
            {
              var fileContents = new char[_bufferSize];
              var charsRead = streamReader.Read(fileContents, 0, _bufferSize);
              if (charsRead == _bufferSize)
              {
                yield return fileContents;
              }
              else
              {
                readingFile = false;
                yield return fileContents.Take(charsRead).ToArray();
              }
            } while (readingFile);
          }
        }
      }
    }
    #endregion
  }
}