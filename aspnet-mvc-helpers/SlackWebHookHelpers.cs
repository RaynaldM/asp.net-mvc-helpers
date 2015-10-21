using System;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using RestSharp;

namespace aspnet_mvc_helpers
{
    /// <summary>
    /// Helpers to send message to slack
    ///  look at https://api.slack.com/incoming-webhooks for more information
    /// todo : implement attachement (https://api.slack.com/docs/attachments)
    /// </summary>
    public static class SlackWebHookHelpers
    {
        /// <summary>
        /// Send a simple text message to slack
        ///  All other parameters is defaults
        /// </summary>
        /// <param name="webHookSlackUrl">Url provided by Slack on Incoming WebHooks page</param>
        /// <param name="message">Message to send</param>
        ///  <returns>200 : Ok, other : Http Status code of error</returns>
        public static HttpStatusCode Send(string webHookSlackUrl, string message)
        {
            var info = new PayLoad { Text = message };
            return Send(webHookSlackUrl, info);
        }

        /// <summary>
        /// Send a full qualified object to Slack
        /// </summary>
        /// <param name="webHookSlackUrl">Url provided by Slack on Incoming WebHooks page</param>
        /// <param name="infoLoad"></param>
        ///  <returns>200 : Ok, other : Http Status code of error</returns>
        public static HttpStatusCode Send(string webHookSlackUrl, PayLoad infoLoad)
        {
            if (string.IsNullOrEmpty(webHookSlackUrl))
                throw new ArgumentNullException(nameof(webHookSlackUrl));

            var jsonToSend = JsonConvert.SerializeObject(infoLoad,
                new JsonSerializerSettings
                {
                    NullValueHandling = NullValueHandling.Ignore,
                    StringEscapeHandling = StringEscapeHandling.EscapeNonAscii
                });

            // init rest client with Slack Url
            var client = new RestClient(webHookSlackUrl);

            // set the method : POST for slack
            var request = new RestRequest(Method.POST);
            // set the payload param
            request.AddParameter("payload", jsonToSend);

            // and execute it
            var response = client.Execute(request);
            // return Http Status of call
            return response.StatusCode;

            // http://www.codeproject.com/Articles/611176/Calling-ASP-NET-WebAPI-using-HttpClient
            // http://johnnycode.com/2012/02/23/consuming-your-own-asp-net-web-api-rest-service/
        }

        /// <summary>
        /// Send a simple text message to slack
        ///  All other parameters is defaults
        /// </summary>
        /// <param name="webHookSlackUrl">Url provided by Slack on Incoming WebHooks page</param>
        /// <param name="message">Message to send</param>
        ///  <returns>200 : Ok, other : Http Status code of error</returns>
        public static Task<HttpStatusCode> SendAsync(string webHookSlackUrl, string message)
        {
            var info = new PayLoad { Text = message };
            return SendAsync(webHookSlackUrl, info);
        }

        /// <summary>
        /// Send a full qualified object to Slack
        /// </summary>
        /// <param name="webHookSlackUrl">Url provided by Slack on Incoming WebHooks page</param>
        /// <param name="infoLoad"></param>
        ///  <returns>200 : Ok, other : Http Status code of error</returns>
        public static async Task<HttpStatusCode> SendAsync(string webHookSlackUrl, PayLoad infoLoad)
        {
            if (string.IsNullOrEmpty(webHookSlackUrl))
                throw new ArgumentNullException(nameof(webHookSlackUrl));

            var jsonToSend = JsonConvert.SerializeObject(infoLoad,
                new JsonSerializerSettings
                {
                    NullValueHandling = NullValueHandling.Ignore,
                    StringEscapeHandling = StringEscapeHandling.EscapeNonAscii
                });

            // init rest client with Slack Url
            var client = new RestClient(webHookSlackUrl);

            // set the method : POST for slack
            var request = new RestRequest(Method.POST);
            // set the payload param
            request.AddParameter("payload", jsonToSend);
            var cancellationTokenSource = new CancellationTokenSource();

            // and execute it
            var response = await client.ExecuteTaskAsync(request, cancellationTokenSource.Token);
            // return Http Status of call
            return response.StatusCode;
        }
    }

    /// <summary>
    /// Class to serialize in JSON for send to Slack
    /// </summary>
    public class PayLoad
    {
        //  'payload={"channel": "#newuser", "username": "webhookbot", "text": "This is posted to #newuser and comes from a bot named webhookbot.", "icon_emoji": ":ghost:"}' https://hooks.slack.com/services/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
        /// <summary>
        /// Channel where you want to publish
        /// </summary>
        [JsonProperty(PropertyName = "channel")]
        public string Channel { get; set; }
        /// <summary>
        /// In the name of
        /// </summary>
        [JsonProperty(PropertyName = "username")]
        public string Username { get; set; }
        /// <summary>
        /// Message you want to public
        /// </summary>
        [JsonProperty(PropertyName = "text")]
        public string Text { get; set; }
        /// <summary>
        ///  Emoji code
        /// </summary>
        [JsonProperty(PropertyName = "icon_emoji")]
        public string IconEmoji { get; set; }
        /// <summary>
        /// Url of message icon 
        /// </summary>
        [JsonProperty(PropertyName = "icon_url")]
        public string IconUrl { get; set; }
    }
}
