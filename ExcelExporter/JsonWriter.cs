using System.Data;
using System.Text;
using System.Text.Json;

namespace ExcelExport;


public class JsonWriter
{
    private Dictionary<string, object> _jsonData;

    public JsonWriter()
    {
        _jsonData = new Dictionary<string, object>();
    }

    public bool AddData(DataTable dt)
    {
        try
        {
            var tableData = new List<Dictionary<string, string>>();

            for (int row = 0; row < dt.Rows.Count; row++)
            {
                var rowData = new Dictionary<string, string>();

                for (int col = 0; col < dt.Columns.Count; col++)
                {
                    var columnName = dt.Columns[col].ColumnName ?? $"Column{col}";
                    var value = dt.Rows[row][col]?.ToString() ?? "";
                    rowData[columnName] = value;
                }

                tableData.Add(rowData);
            }

            _jsonData.Add(dt.TableName, tableData);
            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"JSON 변환 중 오류 발생: {ex.Message}");
            return false;
        }
    }

    public bool AddDataAsColumnBase(DataTable dt)
    {
        try
        {
            var columnData = new Dictionary<string, List<string>>();

            // 각 컬럼별로 데이터 수집
            for (int col = 0; col < dt.Columns.Count; col++)
            {
                var columnName = dt.Columns[col].ColumnName ?? $"Column{col}";
                var values = new List<string>();

                for (int row = 0; row < dt.Rows.Count; row++)
                {
                    var value = dt.Rows[row][col]?.ToString() ?? "";
                    values.Add(value);
                }

                columnData[columnName] = values;
            }

            // 시트 데이터를 Dictionary에 추가
            _jsonData.Add($"{dt.TableName}", columnData);
            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"JSON 변환 중 오류 발생: {ex.Message}");
            return false;
        }
    }

    public bool SaveToFile(string outputPath)
    {
        if (string.IsNullOrEmpty(outputPath))
        {
            Console.WriteLine("출력 경로가 유효하지 않습니다.");
            return false;
        }

        try
        {
            if (File.Exists(outputPath))
            {
                File.Delete(outputPath);
            }

            var options = new JsonSerializerOptions
            {
                WriteIndented = true,
                Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping
            };

            string json = JsonSerializer.Serialize(_jsonData, options);
            File.WriteAllText(outputPath, json, Encoding.UTF8);

            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"파일 저장 중 오류 발생: {ex.Message}");
            return false;
        }
    }
}
