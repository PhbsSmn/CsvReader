﻿using System;
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
    /// <param name="endOfRowMarker">The char(s) to indicate an end of row. By default \n and \r\n will be checked unless overriden then these will be ignored.</param>
    /// <param name="startLine">At which line the parser should start retrieving data.</param>
    /// <param name="bufferSize">The default amount of data read from file at once just large enough to just not end up in the LOH.</param>
    public CsvReader(string delimiter = ",", string textQualifier = "\"", string endOfRowMarker = null, int startLine = 0, int bufferSize = 84998)
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
      var currentMultiCharDelimiterIndex = 0;
      var currentMultiCharEndOfRowMarkerIndex = 0;
      foreach (var fileCharChunk in FileToCharChunks(filePath))
      {
        for (var chunkIndex = 0; chunkIndex < fileCharChunk.Length; chunkIndex++)
        {
          var currentChar = fileCharChunk[chunkIndex];

          switch (readerState)
          {
            case ReaderState.UndeterminedData: // case done
              if (_textQualifier[0].CompareTo(currentChar) == 0) { readerState = ReaderState.TextData; continue; }
              else { readerState = ReaderState.Data; goto case ReaderState.Data; }
            case ReaderState.Data: // multichar endline missing
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
            case ReaderState.TextData: // multichar textqualifier missing
              if (currentChar.CompareTo(_textQualifier[0]) == 0)
              {
                if (_textQualifier.Length == 1) { readerState = ReaderState.TextDataEscape; continue; }
                else
                {
                  throw new NotImplementedException();
                }
              }

              dataResult.Append(currentChar);
              continue;
            case ReaderState.TextDataEscape: // multichar endline & multichar textqualifier missing
              if (currentChar.CompareTo(_textQualifier[0]) == 0)
              {
                if (_textQualifier.Length == 1) { dataResult.Append(currentChar); readerState = ReaderState.TextData; continue; }
                else
                {
                  throw new NotImplementedException();
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
            case ReaderState.DataCarriageReturn: // Case done
              {
                if (currentChar.CompareTo('\n') == 0) { goto DataDoneRowComplete; }
                else { throw new InvalidDataException(); }
              }
            case ReaderState.MultiCharDelimiter:
            case ReaderState.MultiCharTextDelimiter:
              if (currentChar.CompareTo(_delimiter[currentMultiCharDelimiterIndex]) == 0)
              {
                if (_delimiter.Length == ++currentMultiCharDelimiterIndex) { goto DataDone; }
                continue;
              }
              else
              {
                dataResult.Append(_delimiter.Take(currentMultiCharDelimiterIndex).ToArray());
                dataResult.Append(currentChar);
                if (readerState == ReaderState.MultiCharDelimiter) { readerState = ReaderState.Data; }
                else { readerState = ReaderState.TextData; }
                continue;
              }
            case ReaderState.MultiCharEndOfRowMarker:
            case ReaderState.MultiCharDataEndOfRowMarker:
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
          }

          DataDone:
          resultList.Add(dataResult.ToString());
          dataResult.Length = 0;
          readerState = ReaderState.UndeterminedData;
          continue;

          DataDoneRowComplete:
          resultList.Add(dataResult.ToString());
          dataResult.Length = 0;
          yield return resultList;
          resultList = new List<string>();
          readerState = ReaderState.UndeterminedData;
          continue;

          DelimiterHandling:
          if (_delimiter.Length == 1) { goto DataDone; }
          else
          {
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
          if (_endOfRowMarkers.Length == 1) { goto DataDoneRowComplete; }
          else
          {
            currentMultiCharEndOfRowMarkerIndex = 1;
            if (readerState == ReaderState.Data)
            {
              readerState = ReaderState.MultiCharDataEndOfRowMarker;
            }
            else
            {
              readerState = ReaderState.MultiCharEndOfRowMarker;
            }
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
            char[] fileContents;
            int charsRead;
            var readingFile = true;
            do
            {
              fileContents = new char[_bufferSize];
              charsRead = streamReader.Read(fileContents, 0, _bufferSize);
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