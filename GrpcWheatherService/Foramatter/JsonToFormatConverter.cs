using Aspose.Cells;
using Aspose.Cells.Utility;

using System;
using System.Threading.Tasks;

namespace GrpcWheatherService.Services.Formatter
{
    public class JsonToFormatConverter : IJsonToFormatConverter
    {
        public async Task<string> ConvertToExcel(string jsonInput)
        {
            Workbook workbook = new Workbook();
            Worksheet worksheet = workbook.Worksheets[0];

            JsonLayoutOptions options = new JsonLayoutOptions();
            options.ArrayAsTable = true;

            JsonUtility.ImportData(jsonInput, worksheet.Cells, 0, 0, options);

            var fileName = $"{Guid.NewGuid()}_Data_Weather.xlsx";
            workbook.Save(fileName);
            return fileName;
        }
    }
}
