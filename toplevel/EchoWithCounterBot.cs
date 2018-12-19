// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using AdaptiveCards;
using Microsoft.Azure.Search;
using Microsoft.Azure.Search.Models;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Schema;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

#pragma warning disable 4014
#pragma warning disable 1998

namespace Microsoft.BotBuilderSamples
{
    /// <summary>
    /// Represents a bot that processes incoming activities.
    /// For each user interaction, an instance of this class is created and the OnTurnAsync method is called.
    /// This is a Transient lifetime service.  Transient lifetime services are created
    /// each time they're requested. For each Activity received, a new instance of this
    /// class is created. Objects that are expensive to construct, or have a lifetime
    /// beyond the single turn, should be carefully managed.
    /// For example, the <see cref="MemoryStorage"/> object and associated
    /// <see cref="IStatePropertyAccessor{T}"/> object are created with a singleton lifetime.
    /// </summary>
    /// <seealso cref="https://docs.microsoft.com/en-us/aspnet/core/fundamentals/dependency-injection?view=aspnetcore-2.1"/>
    public class EchoWithCounterBot : IBot
    {
        public ILogger _logger;

        private static readonly int MaxResults = 10;
        private static readonly string Greeting = "Welcome, investigator! I'm FBI director J. Edgar Hoover. What can I help you find?";
        private static readonly string Thanks = "Thank you, it's my pleasure to be here. What can I do for you?";
        private static readonly string Hello = "Hello! Good to meet you. What are you interested in today?";
        private static readonly string YoureWelcome = "You're most welcome. How can I assist you?";

        private static SearchIndexClient searchClient = null;
        private static Dictionary<string, string> cryptonyms = null;
        private static string searchUrl = null;
        private static HashSet<string> greeted;

        private Activity activity;
        private ITurnContext context;
        private ITypingActivity typing;

        /// <summary>
        /// Initializes a new instance of the <see cref="EchoWithCounterBot"/> class.
        /// </summary>
        /// <param name="accessors">A class containing <see cref="IStatePropertyAccessor{T}"/> used to manage state.</param>
        /// <param name="loggerFactory">A <see cref="ILoggerFactory"/> that is hooked to the Azure App Service provider.</param>
        /// <seealso cref="https://docs.microsoft.com/en-us/aspnet/core/fundamentals/logging/?view=aspnetcore-2.1#windows-eventlog-provider"/>
        public EchoWithCounterBot(EchoBotAccessors accessors, ILoggerFactory loggerFactory)
        {
            if (loggerFactory == null)
            {
                throw new System.ArgumentNullException(nameof(loggerFactory));
            }

            var _logger = loggerFactory.CreateLogger<EchoWithCounterBot>();
            _logger.LogTrace("EchoBot turn start.");

            if (searchClient == null)
            {
                var searchname = Startup.Configuration.GetSection("searchName")?.Value;
                var searchkey = Startup.Configuration.GetSection("searchKey")?.Value;
                var searchindex = Startup.Configuration.GetSection("searchIndex")?.Value;

                // establish search service connection
                searchClient = new SearchIndexClient(searchname, searchindex, new SearchCredentials(searchkey));

                // read known cryptonyms (code names) from JSON file
                cryptonyms = JsonConvert.DeserializeObject<Dictionary<string, string>>(File.ReadAllText("cia-cryptonyms.json"));

                // get search URL for your main JFK Files site instance
                searchUrl = Startup.Configuration.GetSection("searchUrl")?.Value;

                // create set that remembers who the bot has greeted (add default-user to avoid double greeting on web app)
                greeted = new HashSet<string>();
                greeted.Add("default-user");

            }
        }

        /// <summary>
        /// Every conversation turn for our Echo Bot will call this method.
        /// There are no dialogs used, since it's "single turn" processing, meaning a single
        /// request and response.
        /// </summary>
        /// <param name="turnContext">A <see cref="ITurnContext"/> containing all the data needed
        /// for processing this conversation turn. </param>
        /// <param name="cancellationToken">(Optional) A <see cref="CancellationToken"/> that can be used by other objects
        /// or threads to receive notice of cancellation.</param>
        /// <returns>A <see cref="Task"/> that represents the work queued to execute.</returns>
        /// <seealso cref="BotStateSet"/>
        /// <seealso cref="ConversationState"/>
        /// <seealso cref="IMiddleware"/>

        public async Task OnTurnAsync(ITurnContext turnContext, CancellationToken cancellationToken)
        {
            var type = activity.Type;

            context = turnContext;
            activity = context.Activity;

            // add bot's ID to greeted set so we don't greet it (no harm in doing it eeach time)
            greeted.Add(activity.Recipient.Id);

            if (type == ActivityTypes.Message)
            {
                // respond to greetings and social niceties
                var question = activity.Text.ToLower();

                if (question.Contains("welcome"))
                {
                    SendSpeechReply(Thanks);
                    return;
                }
                if (question.Contains("hello"))
                {
                    SendSpeechReply(Hello);
                    return;
                }
                if (question.Contains("thank"))
                {
                    SendSpeechReply(YoureWelcome);
                    return;
                }

                question = activity.Text;

                // look for cryptomyms and send the definition of any found
                Regex words = new Regex(@"\b(\w+)\b", RegexOptions.Compiled);
                var crypt_found = false;
                foreach (Match match in words.Matches(question))
                {
                    var word = match.Groups[0].Value;
                    var upperword = word.ToUpper();
                    if (cryptonyms.ContainsKey(upperword))
                    {
                        question = new Regex("\\b" + word + "\\b").Replace(question, upperword);
                        SendSpeechReply($"{upperword}: {cryptonyms[upperword]}", cryptonyms[upperword]);
                        crypt_found = true;
                    }
                }

                // transform the question into a query by stripping noise words from the front and back
                var frontwords = new Regex(@"^((a|an|am|with|hope|try[a-z]*|interest[a-z]+|mean[a-z]+|help|assist[a-z]*|find[a-z]*|look[a-z]*|seek[a-z]*|the|who|what|why|when|did|how|where|kill[a-z]*|assassin[a-z]*|is|um|uh|ah|and|now|has|have|had|are|were|was|out|does|please|can|will|you|show|tell|find|search|for|me|i|us|for|look|would|like|want|to|see|in|know|documents?|files?|pictures?|photos?|images?|of|on|more|results|about) +)+", RegexOptions.IgnoreCase);
                var query = question.Substring(frontwords.Match(question).Length);
                query = query.TrimEnd(".?!,;:".ToCharArray());
                if (query.EndsWith(" mean") || query.EndsWith(" means"))
                {
                    query = query.Substring(0, query.Length - 5);
                }
                else if (query.EndsWith(" is"))
                {
                    query = query.Substring(0, query.Length - 3);
                }

                query = query.Trim();

                // initiate the search
                var parameters = new SearchParameters() { Top = MaxResults };   // get top n results
                var search = searchClient.Documents.SearchAsync(query, parameters);

                // send typing indicator while we wait for search to complete
                do
                {
                    SendTypingIndicator();
                } while (!search.Wait(2000));

                var results = search.Result.Results;
                SendTypingIndicator();

                // create a reply
                var reply = activity.CreateReply();
                reply.Attachments = new List<Attachment>();

                foreach (var result in results)
                {
                    // get enrichment data from search result (and parse the JSON data)
                    var enriched = JObject.Parse((string)result.Document["enriched"]);

                    // all results should have thumbnail images, but just in case, look before leaping
                    if (enriched.TryGetValue("/document/normalized_images/*/imageStoreUri", out var images))
                    {
                        // get URI of thumbnail of first content page.
                        // if the document has multiple pages, first page is an ID form
                        // the second page is the first page of interest
                        var thumbs = new List<string>(images.Values<string>());
                        var picurl = thumbs[thumbs.Count > 1 ? 1 : 0];

                        // get valid URL of original document (combine path and token)
                        var document = enriched["/document"];
                        var filename = document["metadata_storage_path"].Value<string>();
                        var token = document["metadata_storage_sas_token"].Value<string>();
                        var docurl = $"{filename}?{token}";

                        // get the text from the document. this includes OCR'd printed and
                        // handwritten text, people recognized in photos, and more
                        // As with the image, try to get the second page's text if it's multi-page
                        var text = enriched["/document/finalText"].Value<string>();
                        if (thumbs.Count > 1)
                        {
                            var sep = "[image: image1.tif]";
                            var page2 = text.IndexOf(sep);
                            text = page2 > 0 ? text.Substring(page2 + sep.Length) : text;
                        }
                        // create card for this search result and attach it to the reply
                        var card = new ResultCard(picurl, text, docurl);
                        reply.Attachments.Add(card.ToAttachment());
                    }
                }

                // Add message describing results, if any
                if (reply.Attachments.Count == 0)
                {
                    reply.Text = $"I'm sorry, I can't find any documents matching \"{query}\"";
                }
                else
                {
                    var documents = reply.Attachments.Count > 1 ? "some documents" : "a document";
                    reply.Text = $"I found {documents} about \"{query}\" you may be interested in.";
                    reply.Speak = $"I found {documents} you may be interested in.";
                    if (crypt_found)
                    {
                        reply.Speak = "Also, " + reply.Speak;
                    }

                    reply.AttachmentLayout = AttachmentLayoutTypes.Carousel;

                    // add "Dig Deeper" button
                    reply.SuggestedActions = new SuggestedActions()
                    {
                        Actions = new List<CardAction>()
                    {
                        new CardAction()
                        {
                            Title = "Dig Deeper",
                            Type = ActionTypes.OpenUrl,
                            Value = searchUrl + System.Uri.EscapeDataString(query),
                        },
                    },
                    };
                }

                // send the reply if we have search results or we didn't find a cryptonym
                if (reply.Attachments.Count > 1 || !crypt_found)
                {
                    context.SendActivityAsync(reply);
                }
            }
            // Send initial greeting
            // Each user in the chat (including the bot) is added via a ConversationUpdate message
            // Check each user to make sure it's not the bot before greeting, and only greet each user
            else if (type == ActivityTypes.ConversationUpdate) // || type == ActivityTypes.Event)
            {
                if (activity.MembersAdded != null) {
                    foreach (var member in activity.MembersAdded)
                    {
                        if (!greeted.Contains(member.Id))
                        {
                            context.SendActivityAsync(Greeting);
                            greeted.Add(member.Id);
                            break;
                        }
                    }
                }
            }
        }

        private void SendSpeechReply(string text, string speech=null)
        {
            var reply = activity.CreateReply();
            reply.Text = text;
            reply.Speak = speech == null ? text : speech;
            context.SendActivityAsync(reply);

        }

        private void SendTypingIndicator()
        {
            if (typing == null)
            {
                typing = activity.CreateReply();
                typing.Type = ActivityTypes.Typing;
            }
            context.SendActivityAsync(typing);
        }

        // Result card layout using a simple adaptive layout
        private class ResultCard : AdaptiveCard
        {
            private int maxlen = 500;

            public ResultCard(string image_url, string text, string action_url)
            {

                // some documents have megabytes of text; avoid sending it all back to client
                text = text.Length < maxlen ? text : text.Substring(0, maxlen - 1) + "…";

                // add spot for thumbnail image
                this.Body.Add(new Image()
                {
                    Url = image_url,
                    Size = ImageSize.Large,
                    HorizontalAlignment = HorizontalAlignment.Center,
                    AltText = text,
                    SelectAction = new OpenUrlAction() { Url = action_url, },
                });

                // add spot for text from document
                this.Body.Add(new TextBlock()
                {
                    Text = text,
                    MaxLines = 5,   // doesn't seem to work
                    Separation = SeparationStyle.Strong,
                });

            }

            public Attachment ToAttachment()
            {
                return new Attachment()
                {
                    ContentType = AdaptiveCard.ContentType,
                    Content = this,
                };
            }

        }

    }

}
