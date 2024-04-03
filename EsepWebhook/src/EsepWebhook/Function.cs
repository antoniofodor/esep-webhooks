using System;
using System.IO;
using System.Net.Http;
using System.Text;
using Amazon.Lambda.Core;
using Newtonsoft.Json;

[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.SystemTextJson.DefaultLambdaJsonSerializer))]

namespace EsepWebhook
{
    public class Function
    {
        public string FunctionHandler(object input, ILambdaContext context)
        {
            try
            {
                // Deserialize the input received from the GitHub webhook
                dynamic payload = JsonConvert.DeserializeObject<dynamic>(input.ToString());

                // Check if the 'issue' object and the 'html_url' field within it exist
                if (payload != null && payload.issue != null && payload.issue.html_url != null)
                {
                    // Extract the URL of the created issue
                    string issueUrl = payload.issue.html_url;

                    // Compose the payload to be sent to Slack
                    string slackPayload = $"{{\"text\":\"Issue Created: {issueUrl}\"}}";

                    // Create an HTTP client
                    using (var client = new HttpClient())
                    {
                        // Send a POST request to the Slack webhook URL with the payload
                        var response = client.PostAsync(Environment.GetEnvironmentVariable("SLACK_URL"), new StringContent(slackPayload, Encoding.UTF8, "application/json")).Result;

                        // Read the response
                        using (var reader = new StreamReader(response.Content.ReadAsStream()))
                        {
                            return reader.ReadToEnd();
                        }
                    }
                }
                else
                {
                    // If the 'issue' object or 'html_url' field is missing, return an error message
                    return "Issue URL not found in the payload.";
                }
            }
            catch (Exception ex)
            {
                // If an exception occurs, return the error message
                return $"An error occurred: {ex.Message}";
            }
        }
    }
}
