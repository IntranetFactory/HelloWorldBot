using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Description;
using Microsoft.Bot.Connector;
using Newtonsoft.Json;
using System.Text;
using System.IO;
using System.Web;
using System.Collections.Generic;

namespace HelloWorldBot
{
    [BotAuthentication]
    public class MessagesController : ApiController
    {

        string MicrosoftAppId = "698e26cf-30a2-4ec1-b808-deb0c5748f38";
        string MicrosoftAppPassword = "quPPIQS948=taujqSI78*{=";

        /// <summary>
        /// POST: api/Messages
        /// Receive a message from a user and reply to it
        /// </summary>
        public async Task<HttpResponseMessage> Post([FromBody]Activity activity)
        {
            // Global values
            bool boolAskedForUserName = false;
            string strUserName = "";

            try
            {
                if (activity.Type == ActivityTypes.Message)
                {
                    // Get any saved values
                    //StateClient sc = activity.GetStateClient();
                    //string authToken = GetAuthToken();

                    //BotData userData = sc.BotState.GetPrivateConversationData(
                    //    activity.ChannelId, activity.Conversation.Id, activity.From.Id);

                    //sc.BotState.GetPrivateConversationDataWithHttpMessagesAsync(customHeaders)
                    //sc.BotState.GetPrivateConversationDataWithHttpMessagesAsync(activity.ChannelId)
                    //client_id = MICROSOFT - APP - ID & client_secret = MICROSOFT - APP - PASSWORD
                    //Bearer ACCESS_TOKEN
                    //boolAskedForUserName = userData.GetProperty<bool>("AskedForUserName");
                    strUserName = "";// userData.GetProperty<string>("UserName") ?? "";

                    // Create text for a reply message   
                    StringBuilder strReplyMessage = new StringBuilder();

                    if (boolAskedForUserName == false) // Never asked for name
                    {
                        strReplyMessage.Append($"Hello,");
                        strReplyMessage.Append($"\n");
                        strReplyMessage.Append($"You can say anything");
                        strReplyMessage.Append($"\n");
                        strReplyMessage.Append($"to me and I will repeat it back");
                        strReplyMessage.Append($"\n\n");
                        strReplyMessage.Append($"What is your name?");

                        // Set BotUserData
                        //userData.SetProperty<bool>("AskedForUserName", true);
                    }
                    else // Have asked for name
                    {
                        if (strUserName == "") // Name was never provided
                        {
                            // If we have asked for a username but it has not been set
                            // the current response is the user name
                            strReplyMessage.Append($"Hello {activity.Text}!");

                            // Set BotUserData
                            //userData.SetProperty<string>("UserName", activity.Text);
                        }
                        else // Name was provided
                        {
                            strReplyMessage.Append($"{strUserName}, You said: {activity.Text}");
                        }
                    }

                    // Save BotUserData
                    //sc.BotState.SetPrivateConversationData(
                    //    activity.ChannelId, activity.Conversation.Id, activity.From.Id, userData);

                    // Create a reply message
                    //ConnectorClient connector = new ConnectorClient(new Uri(activity.ServiceUrl));
                    //BotAuthentication ba = new BotAuthentication();
                    //ba.MicrosoftAppId = MicrosoftAppId;
                    //ba.MicrosoftAppPassword = MicrosoftAppPassword;
                    //BotAuthenticator bar = new BotAuthenticator(MicrosoftAppId, MicrosoftAppPassword);

                    string authToken = GetAuthToken();
                    //authToken = MicrosoftAppPassword;
                    ConnectorClient connector = CreateConnectorClient(activity.ServiceUrl);
                    Activity replyMessage = activity.CreateReply(strReplyMessage.ToString());
                    //await connector.Conversations.ReplyToActivityAsync(replyMessage);
                    //(activity.Conversation.Id, activity.ReplyToId, activity, cancellationToken);
                    Dictionary<string, List<string>> customHeaders = new Dictionary<string, List<string>>();
                    string bearerToken = string.Format("Bearer {0}", authToken);
                    List<string> headerList = new List<string>();
                    headerList.Add(bearerToken);

                    customHeaders.Add("Authorization", headerList);

                    //if (customHeaders != null)
                    //{
                    //    foreach (var _header in customHeaders)
                    //    {
                    //        if (_httpRequest.Headers.Contains(_header.Key))
                    //        {
                    //            _httpRequest.Headers.Remove(_header.Key);
                    //        }
                    //        _httpRequest.Headers.TryAddWithoutValidation(_header.Key, _header.Value);
                    //    }
                    //}

                    await connector.Conversations.ReplyToActivityWithHttpMessagesAsync(activity.Conversation.Id, activity.Id, activity, customHeaders);

                    //------------old code - ----------------------
                    //ConnectorClient connector = CreateConnectorClient(activity.ServiceUrl);
                    //Activity replyMessage = activity.CreateReply(strReplyMessage.ToString());
                    //await connector.Conversations.ReplyToActivityAsync(replyMessage);
                    //------------old code - ----------------------
                }
                else
                {
                    Activity replyMessage = HandleSystemMessage(activity);
                    if (replyMessage != null)
                    {
                        //ConnectorClient connector = new ConnectorClient(new Uri(activity.ServiceUrl));
                        ConnectorClient connector = CreateConnectorClient(activity.ServiceUrl);
                        await connector.Conversations.ReplyToActivityAsync(replyMessage);
                    }
                }
            }
            catch (Exception ex)
            {
                string test = ex.Message;
            }

            // Return response
            var response = Request.CreateResponse(HttpStatusCode.OK);
            return response;

        }



        private Activity HandleSystemMessage(Activity message)
        {
            if (message.Type == ActivityTypes.DeleteUserData)
            {
                // Get BotUserData
                StateClient sc = message.GetStateClient();
                BotData userData = sc.BotState.GetPrivateConversationData(
                    message.ChannelId, message.Conversation.Id, message.From.Id);

                // Set BotUserData
                userData.SetProperty<string>("UserName", "");
                userData.SetProperty<bool>("AskedForUserName", false);

                // Save BotUserData
                sc.BotState.SetPrivateConversationData(
                    message.ChannelId, message.Conversation.Id, message.From.Id, userData);

                // Create a reply message
                ConnectorClient connector = new ConnectorClient(new Uri(message.ServiceUrl));
                Activity replyMessage = message.CreateReply("Personal data has been deleted.");
                return replyMessage;
            }
            else if (message.Type == ActivityTypes.ConversationUpdate)
            {
                // Handle conversation state changes, like members being added and removed
                // Use Activity.MembersAdded and Activity.MembersRemoved and Activity.Action for info
                // Not available in all channels
            }
            else if (message.Type == ActivityTypes.ContactRelationUpdate)
            {
                // Handle add/remove from contact lists
                // Activity.From + Activity.Action represent what happened
            }
            else if (message.Type == ActivityTypes.Typing)
            {
                // Handle knowing tha the user is typing
            }
            else if (message.Type == ActivityTypes.Ping)
            {
            }

            return null;
        }

        //private ConnectorClient CreateConnectorClient(StateClient sc, Activity activity, BotData userData)
        private ConnectorClient CreateConnectorClient(string url)
        {

            //sc.BotState.SetPrivateConversationData(activity.ChannelId, activity.Conversation.Id, activity.From.Id, userData);
            //sc.BotState.SetPrivateConversationData(activity.ChannelId, activity.Conversation.Id, MicrosoftAppId, userData);
            //sc.BotState.SetPrivateConversationData(activity.ChannelId, activity.Conversation.Id, MicrosoftAppId, userData);
            ConnectorClient connector = new ConnectorClient(new Uri(url), MicrosoftAppId, MicrosoftAppPassword);
            return connector;

        }


        //private string GetAuthToken()
        //{

        //    string result = "";
        //    string url = "https://login.microsoftonline.com/botframework.com/oauth2/v2.0/token";

        //    dynamic tokenResponse = await PostFormUrlEncoded<dynamic>(OAuth2Url, OAuth2PostData);

        //    return result;

        //}

        //public static async Task<TResult> PostFormUrlEncoded<TResult>(string url, IEnumerable<KeyValuePair<string, string>> postData)
        //{
        //    using (var httpClient = new HttpClient())
        //    {
        //        using (var content = new FormUrlEncodedContent(postData))
        //        {
        //            content.Headers.Clear();
        //            content.Headers.Add("Content-Type", "application/x-www-form-urlencoded");

        //            HttpResponseMessage response = await httpClient.PostAsync(url, content);

        //            return await response.Content.ReadAsAsync<TResult>();
        //        }
        //    }
        //}


        private string GetAuthToken()
        {

            string result = "";
            HttpWebRequest httpWebRequest = null;
            HttpWebResponse httpResponse = null;

            try
            {

                //curl - k - X POST https://login.microsoftonline.com/botframework.com/oauth2/v2.0/token -d "grant_type=client_credentials&client_id=APP_ID&client_secret=APP_PASSWORD&scope=https%3A%2F%2Fapi.botframework.com%2F.default"
                //string url = string.Format("https://login.microsoftonline.com/botframework.com/oauth2/v2.0/token?grant_type=client_credentials&client_id={0}&client_secret={1}&scope=https%3A%2F%2Fapi.botframework.com%2F.default", MicrosoftAppId, MicrosoftAppPassword);
                //string url = string.Format("https://login.microsoftonline.com/botframework.com/oauth2/v2.0/token?client_id={0}&client_secret={1}&scope=https%3A%2F%2Fapi.botframework.com%2F.default", MicrosoftAppId, MicrosoftAppPassword);
                string url = "https://login.microsoftonline.com/botframework.com/oauth2/v2.0/token";

                httpWebRequest = (HttpWebRequest)WebRequest.Create(url);
                httpWebRequest.Method = "POST";
                httpWebRequest.ContentType = "application/x-www-form-urlencoded";
                //httpWebRequest.Headers.Add("Content-Length", "0");

                //dynamic requestBodyJson = new dynamic();
                //string requestBodyJsonString = "{ 'grant_type':'client_credentials', 'client_id':'698e26cf-30a2-4ec1-b808-deb0c5748f38','client_secret':'quPPIQS948=taujqSI78*{=', 'scope':'https%3A%2F%2Fapi.botframework.com%2F.default'}";
                string requestBodyJsonString = "{ 'grant_type':'client_credentials', 'client_id':'698e26cf-30a2-4ec1-b808-deb0c5748f38', 'client_secret':'quPPIQS948=taujqSI78*{=', 'scope':'https%3A%2F%2Fapi.botframework.com%2F.default'}";

                //requestBody.grant_type = "client_credentials";
                dynamic requestBodyJson = Newtonsoft.Json.JsonConvert.DeserializeObject(requestBodyJsonString);

                ASCIIEncoding encoding = new ASCIIEncoding();

                string requestBody = string.Format("&grant_type=client_credentials&client_id={0}&client_secret={1}&scope={2}", HttpUtility.UrlEncode(MicrosoftAppId), HttpUtility.UrlEncode(MicrosoftAppPassword), "https%3A%2F%2Fapi.botframework.com%2F.default");
                string requestBodyEncoded = HttpUtility.UrlEncode(requestBody);
                byte[] data = encoding.GetBytes(requestBody.ToString());

                httpWebRequest.ContentLength = data.Length;

                string deserialized = Newtonsoft.Json.JsonConvert.SerializeObject(requestBodyJson);

                if (requestBodyJson != null)
                {
                    using (var streamWriter = new StreamWriter(httpWebRequest.GetRequestStream()))
                    {
                        streamWriter.Write(requestBody);
                    }
                }

                httpResponse = (HttpWebResponse)httpWebRequest.GetResponse();

                if (httpResponse.StatusCode == HttpStatusCode.OK)
                {
                    using (var streamReader = new StreamReader(httpResponse.GetResponseStream()))
                    {
                        result = streamReader.ReadToEnd();
                        //r = (JsonObject)SimpleJson.SimpleJson.DeserializeObject(result);
                    }
                }
                else
                {
                    //Logger.Error("GetWebRequest error: " + httpResponse.StatusCode);
                }
            }
            catch (WebException ex)
            {
                using (var stream = ex.Response.GetResponseStream())
                using (var reader = new StreamReader(stream))
                {
                    string error = reader.ReadToEnd();
                    int x = 0;
                }

            }
            catch (Exception ex)
            {

                if (httpResponse != null)
                {
                    using (var streamReader = new StreamReader(httpResponse.GetResponseStream()))
                    {
                        result = streamReader.ReadToEnd();
                        //r = (JsonObject)SimpleJson.SimpleJson.DeserializeObject(result);
                    }
                }
            }

            dynamic resultJson = JsonConvert.DeserializeObject(result);
            string tokenType = resultJson.token_type.Value;
            string token = resultJson.access_token.Value;
            long expires_in = resultJson.expires_in.Value;
            result = token;

            return result;

        }

    }
}