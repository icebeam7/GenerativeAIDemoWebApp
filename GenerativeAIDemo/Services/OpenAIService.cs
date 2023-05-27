using GenerativeAIDemo.Helpers;
using GenerativeAIDemo.Models.Text;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Text;
using GenerativeAIDemo.Models.Images;

namespace GenerativeAIDemo.Services
{
    public class OpenAIService
    {
        HttpClient client;

        JsonSerializerOptions options = new JsonSerializerOptions()
        {
            PropertyNameCaseInsensitive = true
        };

        public OpenAIService()
        {
            client = new HttpClient();
            client.BaseAddress = new Uri(OpenAIConstants.AzureServer);
            client.DefaultRequestHeaders.Add("api-key", OpenAIConstants.AzureKey);
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        }

        public async Task<string> AskQuestion(string query)
        {
            var completion = new CompletionRequest()
            {
                prompt = query
            };

            var body = JsonSerializer.Serialize(completion);
            var content = new StringContent(body, Encoding.UTF8, "application/json");

            var response = await client.PostAsync(OpenAIConstants.CompletionsEndpoint, content);

            if (response.IsSuccessStatusCode)
            {
                var answer = await response.Content.ReadFromJsonAsync<CompletionResponse>(options);
                return answer?.choices?.FirstOrDefault()?.text;
            }

            return string.Empty;
        }

        public async Task<string> CreateImage(string query)
        {
            var generation = new GenerationRequest()
            {
                caption = query
            };

            var body = JsonSerializer.Serialize(generation);
            var content = new StringContent(body, Encoding.UTF8, "application/json");

            var submission = await client.PostAsync(OpenAIConstants.GenerationsEndpoint, content);

            if (submission.IsSuccessStatusCode)
            {
                var headers = submission.Headers;

                var operation_location = headers.GetValues("Operation-Location").FirstOrDefault();
                operation_location = operation_location.Replace(OpenAIConstants.AzureServer, "");

                var retry_after = int.Parse(headers.GetValues("Retry-after").FirstOrDefault());
                retry_after *= 1000;

                var status = "";
                var result = "";

                while (status != "Succeeded")
                {
                    await Task.Delay(retry_after);

                    var response = await client.GetAsync(operation_location);

                    if (response.IsSuccessStatusCode)
                    {
                        var answer = await response.Content.ReadFromJsonAsync<GenerationResponse>(options);
                        status = answer?.status;
                        result = answer?.result?.contentUrl;
                    }
                }

                return result;
            }

            return default;
        }
    }
}
