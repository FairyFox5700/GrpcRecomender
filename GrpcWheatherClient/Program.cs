using Grpc.Core;
using Grpc.Net.Client;

using GrpcWheatherClient.Protos;

using IdentityModel.Client;

using System;
using System.IO;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace GrpcWheatherClient
{
    public class Program
    {
        private const string Address = "https://localhost:5005";

        static async Task Main(string[] args)
        {
            using var channel = GrpcChannel.ForAddress(Address);
            var client = new Weather.WeatherClient(channel);

            Console.WriteLine("gRPC Wheather");
            Console.WriteLine();
            Console.WriteLine("Press a key:");
            Console.WriteLine("1: Authenticate");
            Console.WriteLine("2: Get wheather for location");
            Console.WriteLine("3: Get wheather prediction for location in N days");
            Console.WriteLine("4: Download wheather prediction");
            Console.WriteLine("5: Exit");
            Console.WriteLine();

            string? token = null;

            var exiting = false;
            while(!exiting)
            {
                var consoleKeyInfo = Console.ReadKey(intercept: true);
                switch(consoleKeyInfo.KeyChar)
                {
                    case '1':
                        token = await Authenticate();
                        break;
                    case '2':
                        await GetWheatherByLocation(client, token);
                        break;
                    case '3':
                        await GetWheatherPredictionByLocation(client, token);
                        break;
                    case '4':
                        await DownloadWeatherPrediction(client, token);
                        break;
                    case '5':
                        exiting = true;
                        break;
                }
            }

            Console.WriteLine("Exiting");
        }



        private static async Task<string> Authenticate()
        {
            Console.WriteLine($"Authenticating as {Environment.UserName}...");
            var client = new HttpClient();
            var disco = await client.GetDiscoveryDocumentAsync(Address);
            if(disco.IsError)
            {
                Console.WriteLine(disco.Error);
                return string.Empty;
            }

            var tokenResponse = await client.RequestClientCredentialsTokenAsync(new ClientCredentialsTokenRequest
            {
                Address = disco.TokenEndpoint,
                ClientId = "ShoppingCartClient",
                ClientSecret = "secret",
                Scope = "ShoppingCartAPI",
            });

            if(tokenResponse.IsError)
            {
                Console.WriteLine(tokenResponse.Error);
                return string.Empty;
            }

            var token = tokenResponse.AccessToken;
            Console.WriteLine("Successfully authenticated.");
            return token;
        }

        private static async Task GetWheatherByLocation(Weather.WeatherClient client, string? token)
        {
            Console.WriteLine("Requesting wheather by location...");
            Console.WriteLine("Kindly asking you to type your location:");
            var location = Console.ReadLine();
            await ExecuteGrpcRequest(client, async () =>
            {
                return await client.GetCurentWeatherAsync(new WeatherRequest { Location = String.IsNullOrEmpty(location)?"Lviv":location }, GetHeaders(token));
            });
        }

        private static async Task GetWheatherPredictionByLocation(Weather.WeatherClient client, string? token)
        {
            Console.WriteLine("Requesting wheather prediction by location for n days...");
            Console.WriteLine("Kindly asking you to type your location:");
            var location = Console.ReadLine();
            Console.WriteLine("Kindly asking you to type days count for weather prediction:");
            var days = 0;
            var parced = Int32.TryParse( Console.ReadLine(), out days);
            await ExecuteGrpcRequest(client, async () =>
            {
                return await client.GetWeatherForecastAsync(new WeatherForecastRequest { Location = String.IsNullOrEmpty(location) ? "Lviv" : location , Days =(!parced) ?2:days }, GetHeaders(token));
            });
        }

        private static async Task DownloadWeatherPrediction(Weather.WeatherClient client, string token)
        {
            try
            {
                Console.WriteLine("Requesting wheather prediction file...");
                Console.WriteLine("Kindly asking you to type your file name:");
                var fileInfo = new Protos.FileInfo
                {
                    FileName = Console.ReadLine(),
                };
                FileStream fileStream = null;
                var download = client.FileDownload(fileInfo, GetHeaders(token));
                CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();

                int count = 0;
                decimal chunkSize = 0;
                while(await download.ResponseStream.MoveNext(cancellationTokenSource.Token))
                {
                    if(count++ == 0)
                    {
                        fileStream = new FileStream(@$"{fileInfo.FileName}", FileMode.CreateNew);
                        fileStream.SetLength(download.ResponseStream.Current.FileSize);
                    }

                    var buffer = download.ResponseStream.Current.Buffer.ToByteArray();
                    await fileStream.WriteAsync(buffer, 0, download.ResponseStream.Current.ReadedByte);
                    var processedPercentOfFile = Math.Round(((chunkSize += download.ResponseStream.Current.ReadedByte) * 100) / download.ResponseStream.Current.FileSize);
                    System.Console.WriteLine($"Process....{processedPercentOfFile}%");
                }
                System.Console.WriteLine("Completed downloading");
                await fileStream.DisposeAsync();
                fileStream.Close();
            }
            catch(Exception ex)
            {
                Console.WriteLine($"Something went wrong while file downloading {ex.Message}");
            }
        }

        private static Metadata GetHeaders(string token)
        {
            Metadata headers = null;
            if(token != null)
            {
                headers = new Metadata
                    {
                        { "Authorization", $"Bearer {token}" }
                    };
            }
            return headers;
        }
        private static async Task ExecuteGrpcRequest(Weather.WeatherClient client,Func<Task<WeatherReply>> func)
        {
            try
            {

                WeatherReply response = await func();

                if(response.Success)
                {
                    Console.WriteLine("Request successful.");
                    Console.WriteLine(response.Message);
                }
                else
                {
                    Console.WriteLine("Request failed. Validate data passed to request.");
                }
            }
            catch(Exception ex)
            {
                Console.WriteLine("Error requesting wheather." + Environment.NewLine + ex.ToString());
            }
        }
    }
}
