using Google.Protobuf;
using Grpc.Core;
using GrpcWheatherService.Email;
using GrpcWheatherService.Models;
using GrpcWheatherService.Protos;
using GrpcWheatherService.Services.Formatter;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using System;
using System.IO;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;

namespace GrpcWheatherService.Services
{
    [Authorize]
    public class WeatherService : Weather.WeatherBase
    {

        private readonly ILogger<WeatherService> _logger;
        private const string ApiKey = "4dbf6a49592f4be0a4e90328212109";
        private IHttpClientFactory _httpClientFactory;
        private readonly IJsonToFormatConverter _jsonToFormatConverter;

        public WeatherService(ILogger<WeatherService> logger,
            IHttpClientFactory httpClienFactory,
            IJsonToFormatConverter jsonToFormatConverter)
        {
            _logger = logger;
            _httpClientFactory = httpClienFactory;
            _jsonToFormatConverter = jsonToFormatConverter;
        }

        private async Task<WeatherReply> ExecuteWheatherServiceRequestReply(Func<Task<string>> func)
        {
            try
            {
                var result = await func();

                return new WeatherReply
                {
                    Message = result,
                    Success = true,
                };
            }
            catch(Exception ex)
            {
                _logger.LogError(ex.Message);
                return new WeatherReply
                {
                    Message = ex.Message,
                    Success = false,
                };
            }

        }

        public override async Task<WeatherReply> GetCurentWeather(WeatherRequest request, ServerCallContext context)
        {

            _logger.LogInformation($"Requesting current weather for location {request.Location}.");
            return await ExecuteWheatherServiceRequestReply(async () =>
            {
                var url = $"https://api.weatherapi.com/v1/current.json?key={ApiKey}&q={request.Location}&aqi=no";
                var client = _httpClientFactory.CreateClient();
                var response = await client.GetAsync(url);
                var data2 = await response.Content.ReadAsStringAsync();
                var model = JsonSerializer.Deserialize<CurrentWheatherResponse>(data2);
                var data = @$"____________________WHEATHER________________{Environment.NewLine}
                 YOUR LOCATION :{Environment.NewLine} 
                 NAME: {model.location.name}, REGION:{model.location.region}, COUNTRY {model.location.country}.{Environment.NewLine} 
                 LOCATION {Environment.NewLine} 
                 LATITUDE: {model.location.lat}, LONGITUDE {model.location.lon}, LOCAL TIME {model.location.localtime} {Environment.NewLine}
                 YOUR CURRENT TEMPERATURE : {model.current.temp_c} C | {model.current.temp_f} F. {Environment.NewLine}
                 LAST UPDATED {model.current.last_updated} {Environment.NewLine}";
                return data;
            });
        }

        public override async Task<WeatherReply> GetWeatherForecast(WeatherForecastRequest request, ServerCallContext context)
        {
            _logger.LogInformation($"Requesting current weather  forecast for location {request.Location}.");
            return await ExecuteWheatherServiceRequestReply(async () =>
            {
                var url = $"https://api.weatherapi.com/v1/forecast.json?key={ApiKey}&q={request.Location}&days={request.Days}&aqi=no&alerts=yes";
                var client = _httpClientFactory.CreateClient();

                var response = await client.GetAsync(url).ConfigureAwait(false);
                string json = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
                var fileName = await _jsonToFormatConverter.ConvertToExcel(JObject.Parse(json)["forecast"].ToString()).ConfigureAwait(false);
                return $"Generated wheather file {fileName}";
            });
        }

        public override async Task FileDownload(Protos.FileInfo fileInfo, IServerStreamWriter<FileChunk> responseStream, ServerCallContext context)
        {
            _logger.LogInformation($"Received File Download Request ... ");
            using FileStream fileStream = new FileStream(fileInfo.FileName, FileMode.Open, FileAccess.Read);
            byte[] buffer = new byte[2048];

            FileChunk content = new FileChunk
            {
                FileSize = fileStream.Length,
                Info = new Protos.FileInfo
                {
                    FileName = Path.GetFileNameWithoutExtension(fileStream.Name),
                },
                ReadedByte = 0,
            };

            while((content.ReadedByte = await fileStream.ReadAsync(buffer, 0, buffer.Length)) > 0)
            {
                content.Buffer = ByteString.CopyFrom(buffer);
                await responseStream.WriteAsync(content);
            }
            fileStream.Close();
        }
    }
}








/*  var emailResponseSuccess = await _emailService.SendMessage(new MailModel
       {
           Body = "Weather data for you",
           Subject = "Weather data",
           ToEmail = request.Email,
           Attachments = new System.Collections.Generic.List<Microsoft.AspNetCore.Http.IFormFile>
           {
               GetFile(fileName)

           }
       }).ConfigureAwait(false);*/
/* if(emailResponseSuccess)
 {
     return $"Email with generated excel {fileName} is sent to your email {request.Email}";
 }
 {
     return $"Excel generation failed";
 }*/
