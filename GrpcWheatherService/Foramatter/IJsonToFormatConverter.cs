using System.Threading.Tasks;

namespace GrpcWheatherService.Services.Formatter
{
    public interface IJsonToFormatConverter
    {
        Task<string> ConvertToExcel(string jsonInput);
    }
}