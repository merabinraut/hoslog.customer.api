using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace hoslog.customer.common.Service
{
    public class LineAPIService
    {
        public async Task<string> SendMessageToLineGroup(string secretKey, string groupId, string message)
        {
            using (HttpClient client = new HttpClient())
            {
                // Set the authorization header
                client.DefaultRequestHeaders.Add("Authorization", "Bearer " + secretKey);

                // The LINE API URL for pushing messages
                string apiUrl = "https://api.line.me/v2/bot/message/push";

                // Create the message payload
                var messagePayload = new
                {
                    to = groupId, // recipient's group ID
                    messages = new[]
                    {
                        new
                        {
                            type = "text",
                            text = message
                        }
                    }
                };

                // Serialize the payload to JSON
                var jsonPayload = JsonConvert.SerializeObject(messagePayload);

                // Create the HTTP content
                var content = new StringContent(jsonPayload, Encoding.UTF8, "application/json");

                try
                {
                    // Send the POST request
                    HttpResponseMessage response = await client.PostAsync(apiUrl, content);

                    // Check if the request was successful
                    if (response.IsSuccessStatusCode)
                    {
                        return "Message sent successfully!";
                    }
                    else
                    {
                        //Console.WriteLine($"Failed to send message. Status code: {response.StatusCode}");
                        string responseContent = await response.Content.ReadAsStringAsync();
                        //Console.WriteLine($"Response: {responseContent}");
                        return $"Failed to send message. Status code: {response.StatusCode}. Response: {responseContent}";
                    }
                }
                catch (Exception ex)
                {
                    // Handle any exceptions that may occur during the request
                    //Console.WriteLine($"An error occurred: {ex.Message}");
                    return $"An error occurred: {ex.Message}";
                }
            }
        }

    }
}
