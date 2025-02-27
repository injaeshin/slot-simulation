using ExcelDataReader;
using System.Data;

namespace ExcelExport;


public struct ExcelRange
{
    public int BeginRow;
    public int EndRow;
    public int BeginCol;
    public int EndCol;

    public static ExcelRange FromExcelReference(string reference)
    {
        var parts = reference.Split(':');
        if (parts.Length != 2)
            throw new ArgumentException("Invalid Excel reference format (e.g. 'B2:D5')");

        var start = ParseCellReference(parts[0]);
        var end = ParseCellReference(parts[1]);

        return new ExcelRange
        {
            BeginRow = start.row - 1,
            EndRow = end.row - 1,
            BeginCol = start.col - 1,
            EndCol = end.col - 1
        };
    }

    private static (int col, int row) ParseCellReference(string cellRef)
    {
        var match = System.Text.RegularExpressions.Regex.Match(cellRef.ToUpper(), @"([A-Z]+)(\d+)");
        if (!match.Success)
            throw new ArgumentException($"Invalid cell reference: {cellRef}");

        string colStr = match.Groups[1].Value;
        string rowStr = match.Groups[2].Value;

        // 열 이름이 너무 길면 예외 발생 (XFD = 16384가 Excel의 최대 열)
        if (colStr.Length > 3)
            throw new ArgumentException($"Column reference '{colStr}' exceeds Excel's maximum column limit");

        int col = 0;
        foreach (char c in colStr)
        {
            // 오버플로우 체크
            if (col > (int.MaxValue - (c - 'A' + 1)) / 26)
                throw new ArgumentException($"Column reference '{colStr}' is too large");
            
            col = col * 26 + (c - 'A' + 1);
        }

        if (!int.TryParse(rowStr, out int row))
            throw new ArgumentException($"Invalid row number: {rowStr}");

        // Excel의 최대 행 수는 1,048,576
        if (row > 1048576)
            throw new ArgumentException($"Row number {row} exceeds Excel's maximum row limit");

        return (col, row);
    }
}

public class ExcelReader : IDisposable
{
    private string _file;
    // private bool _useHeaderRow = false;
    // private bool _skipSecondRow = false;

    private DataSet? _dataSet;
    private Stream? _stream;
    private IExcelDataReader? _dataReader;
    private bool _disposed;

    public ExcelReader(string file)
    {
        _file = file;
        System.Text.Encoding.RegisterProvider(System.Text.CodePagesEncodingProvider.Instance);
    }

    ~ExcelReader()
    {
        Dispose(false);
    }

    public bool Open()
    {
        Clear();

        try
        {
            _stream = new FileStream(_file, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
            _dataReader = ExcelReaderFactory.CreateReader(_stream);
        }
        catch (Exception e)
        {
            Close();

            Console.WriteLine(e.ToString());

            throw;
        }

        return true;
    }

    #region Clear, Close And Dispose
    public void Clear()
    {
        _dataSet?.Clear();
    }
    private void Close()
    {
        Dispose();

        _dataSet?.Dispose();
        _dataSet = null;
        _stream?.Dispose();
        _stream = null;
        _dataReader?.Dispose();
        _dataReader = null;
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (!_disposed)
        {
            if (disposing)
            {
                // 관리되는 리소스 해제
                _dataSet?.Dispose();
                _dataSet = null;
                _stream?.Dispose();
                _stream = null;
                _dataReader?.Dispose();
                _dataReader = null;
            }

            // 관리되지 않는 리소스 해제 필요 시 여기에 추가

            _disposed = true;
        }
    }
    #endregion

    public IEnumerable<string> GetNames()
    {
        if (_dataSet == null)
        {
            _dataSet = ToDataSet();
        }

        for (int i = 0; i < _dataSet?.Tables.Count; i++)
        {
            yield return _dataSet.Tables[i].TableName;
        }
    }

    private DataSet? ToDataSet()
    {
        if (_dataReader == null)
            return null;

        var conf = new ExcelDataSetConfiguration()
        {
            ConfigureDataTable = (tableReader) => new()
            {
                UseHeaderRow = false,
                EmptyColumnNamePrefix = "EmptyColumn",
                //FilterRow = (rowReader) => rowReader.Depth > (false ? 1 : 0)
            }
        };

        return _dataReader.AsDataSet(conf);
    }

    private DataTable? ToDataTable(string sheetName)
    {
        if (_dataSet == null)
        {
            _dataSet = ToDataSet();
            if (_dataSet?.Tables.Count == 0)
                return null;
        }

        return _dataSet?.Tables[sheetName];
    }

    public DataTable? GetRangeAsDataTable(string sheetName, string range)
    {
        var excelRange = ExcelRange.FromExcelReference(range);
        var sourceTable = ToDataTable(sheetName);
        if (sourceTable == null)
            return null;

        return ExtractRange(sourceTable, excelRange);
    }

    private DataTable ExtractRange(DataTable sourceTable, ExcelRange range)
    {
    DataTable result = new DataTable(sourceTable.TableName);

    // 선택된 열의 첫 번째 행 데이터를 컬럼명으로 사용
    for (int col = range.BeginCol; col <= range.EndCol && col < sourceTable.Columns.Count; col++)
    {
        string columnName = sourceTable.Rows[range.BeginRow][col]?.ToString() ?? $"Column{col}";
        result.Columns.Add(columnName);
    }

    // 첫 번째 행을 제외한 나머지 데이터 복사
    for (int row = range.BeginRow + 1; row <= range.EndRow && row < sourceTable.Rows.Count; row++)
    {
        DataRow newRow = result.NewRow();
        int newColIndex = 0;

        for (int col = range.BeginCol; col <= range.EndCol && col < sourceTable.Columns.Count; col++)
        {
            newRow[newColIndex++] = sourceTable.Rows[row][col];
        }

        result.Rows.Add(newRow);
    }

    return result;
    }
}