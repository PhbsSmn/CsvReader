# CsvReader
A simple fast csv reader library

Instead of having to wait for a fully processed file. After every successfull row a yield will be done so while reading the file you can already process the result of the file.
The csv reader only returns ienumerable<string> because the data processing is not the concern of the parser.
