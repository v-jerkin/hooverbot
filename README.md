# The Hoover Bot

The Hoover Bot is a conversational interface to The JFK Files. The JFK Files is a Web app that lets you search a corpus of documents related to the assassination of President John F. Kennedy on November 22, 1963, released by the United States government. Microsoft has presented this technology demonstration, which showcases the power of Azure Cognitive Search, on several occasions.

* [Experience The JFK Files](https://jfk-demo.azurewebsites.net/)
* [The JFK Files GitHub](https://github.com/Microsoft/AzureSearch_JFK_Files)
* [Video presentation](https://channel9.msdn.com/Shows/AI-Show/Using-Cognitive-Search-to-Understand-the-JFK-Documents)

Using the same database as the JFK Files, the Hoover Bot lets you ask former FBI director J. Edgar Hoover about the JFK assassination, either by typing or by speaking. The bot answers using a simulation of Hoover's voice.

The Hovoer Bot requires a subscription to the following Microsoft Azure Cognitive Services. (A trial or regular free-tier subscription is fine.)

* Azure Bot Service: provides the chat-room-like conversational framework
* Speech Service: provides customized speech recognition and synthesis

The Hoover Bot is a single-page Web app that works in any modern browser. The document contains instructions for setting up the Azure services used by the demo and building and deploying your own copy of the bot.

**NOTE:** The Hoover Bot is a technology demonstration designed to illustrate specific uses of Microsoft's technology and is not production-ready.

## Prerequisites

You will need a Microsoft Azure account, along with subscriptions to the Azure Bot Service and the Speech Service. Visual Studio 2017 is also required (the free Community Edition will work).

The JFK Files is a separate application with a database backend powered by Azure Search. You can find its repository here.

    https://github.com/Microsoft/AzureSearch_JFK_Files

Follow the instructions in this repo to create your own instance of the JFK Files. There's a template that will create the necessary Azure services for you. You'll need the URLs for the Azure Search service and the Web Service that were created during setup.

Note that adding all the documents to the index may take substantial time. We suggest letting the process run overnight.

## Creating the Bot

The Hoover bot is based on the `EchoBot` template. In its original form, this bot simply echoes back whatever you type or say to it, along with a turn counter. We'll update it to search The JFK Files. We'll also add a customized Web app that includes our own CSS styles and images to match The JFK Files. Finally, we'll use custom speech and voice services to make sure the bot understands the user's spoken queries and responds using a facsimile of J. Edgar Hoover's voice.

To create the bot on Azure:

1. Create a Web App bot in the Azure portal. This style of bot includes a Web hosting component, so we won't need to host it elsewhere. The free pricing tier is suitable for developing the bot. Choose "EchoBot (C#)" as the template.

   Your bot will be hosted on a subdomain of `azurewebsites.net`. Therefore, its name must be unique among all other sites hosted on that domain. Try `hooverbot-abc` where `abc` are your initials, or variations on this theme. Valid characters are letters, numbers, and hyphens.

1. Download the source code for the bot from the Build blade of the new resource. This is a Visual Studio project that we will be customizing with both code and resources.

1. Open the `EchoBotWithCounter.sln` file to launch Visual Studio and open the project.

1. Using NuGet (**Tools > NuGet Package Manager**), add the following libraries:

    * `Microsoft.Azure.Search.Data`, the Azure search client
    * `Microsoft.AdaptiveCards`, adaptive cards for bot responses
    * `Newtonsoft.Json`, a parser for JSON files

1. Copy the files from the `toplevel` folder of this repository to the top level of the Visual Studio project. Some of them have the same name as files already in the project; allow the new files to replace the old one.

1. Open `appsettings.json` and enter the required values. You can find the values you need in the Azure dashboard.

    * `botFilePath` and `botFileSecret` can be found in the Application Settings blade of your Web App Bot (scroll down to the Application Settings heading).
    * `searchName` and `searchKey` can be found in the Keys blade of your JFK Files search service. The `searchName` is the string displayed in bold at the top of the blade. 

        We suggest not using an admin key. Instead, click **Manage query keys** to obtain a query key, which can only be used for searches. This prevents othres from obtaining administrative access to your Azure Search instance if the key leaks out.

        The `searchIndex` name should already be `jfkindex` and should not be changed.

        Change the hostname of the `SearchUrl` field (the part after `https://` where it currently says `jfk-site-hostname`) to have the hostname of your JFK Files Web app instance.

1. Copy the files from this project's `wwwroot` folder into the bot project's `wwwroot` folder. These are the bot's user interface and client-side logic.

1. Make sure the project builds and runs. With the project running, try your bot in the [emulator](https://docs.microsoft.com/en-us/azure/bot-service/bot-service-debug-emulator?view=azure-bot-service-4.0). Just double-click the `.bot` file in Visual Studio.
 
Running the project also opens the Web Chat app in a browser. This app connects to a version of the bot running in the Azure cloud. It won't work until you publish the bot to Azure. Use the emulator to test unpublished versions of your bot.

## Hooking up Web Chat

Azure Bot Service's Web Chat is a JavaScript component that lets you easily embed your bot in any Web page. We'll use it to power the J. Edgar Hoover Bot Web page. To get the Web Chat app to talk to your bot, you must enable the bot's Direct Line channel and provide an authentication token in the `bot.htm` page. 

In the Azure portal, enable Direct Line in your Web App Bot's Channels blade. Copy one of the two secret keys from the Direct Line configuration page and paste it into `bot.htm` in place of the placeholder text. For more help, see [Connect a bot to Direct Line](https://docs.microsoft.com/en-us/azure/bot-service/bot-service-channel-connect-directline?view=azure-bot-service-3.0).

You'll also see a Web Chat channel in the Channels blade. This refers to the v3 Web Chat feature used by the Bot Framework emulator, also available via the Test in Web Chat blade in the Azure portal. The v4 Web Chat you embed in your own Web sites uses the Direct Line protocol. Using the v4 Web Chat gives you the ultimate in flexibility and control.

After you've added the Direct Line secret, you can now publish the bot so you can test it in a  browser.

**Important security note**  The Hoover Bot is a technology demonstration and is not intended to be a production application. Your bot and speech subscription keys are embedded in the source code of the Web page and can be easily seen by anyone with access to the site. This will allow them to use the service in their own apps.

By using trial or free tier keys, at most your bot might stop working because too many requests are being made using your keys. We do not recommend using paid keys for the Hoover Bot, as this could cost you real money. You are responsible for the security of your keys and for all requests made using your keys. 

## Publishing your bot

Publish your bot to the Azure cloud by following these steps.

1. Open the file `[botname].PublishSettings` in your Visual Studio project. For example, if your bot's Azure name is `myhooverbot`, the file is named `myhooverbot.PublishSettings`.
1. Find the password (value of `userPWD` field) in this file and copy it to the clipboard.
1. Right-click the project and choose Publish to open the Publish page in Visual Studio.
1. On the Publish page, click **Publish**.
1. When you're asked for the password, paste it in and click **OK**.
 
After your code has been published, the JFK bot's Web site opens in your browser. It may take a moment for the site to "warm up" after being restarted as part of deployment.

To make publishing easier the next time, you can save your deployment password. Click **Configure** and paste the password in the appropriate field, then click **Save**.

## Adding voice input and output

It is straightforward to add speech recognition and voice repsonse to the Web Chat app using the Azure Speech Service. However, at this time, the Web Chat app does not support custom speech models or voices, both of which we would like to use in our bot.

* A custom language model will help assure that the bot recognizes the cryptonyms (code names) used for certain persons, programs, and events.

* A custom voice will allow the bot to respond using a facsimile of J. Edgar Hoover's voice.

Fortunately, the Web Chat component is extensible, so we can add support for the custom speech features we want to use ourselves.

### Customizing Speech Synthesis

To make our bot's voice sound like J. Edgar Hoover, we need recordings of his voice, along with a text transcript of each recording. We have used as our source a 21-minute minute recording of a November 1963 phone call between Hoover and President Lyndon B. Johnson. From this audio, we have extracted nearly 200 utterances, edited them slightly to remove "disfluencies" like "ah" and "um," and transcribed them into text.

The quality of the recording isn't ideal. It's a telephone call to begin with, and the recording is old and contains a lot of noise. It would be better, as well, if we had more utterances, especially since we can't use any utterances where Johnson and Hoover are speaking at the same time. Still, even with just a couple hundred utterances, the synthesized voice is recognizably Hoover's. There are other recorded phone conversations between Johnson and Hoover that could be used to provide further utterances, if you want to improve the quality.

After extracting the utterances, we created a ZIP archive containing a numbered audio file for each utterance, along with a text file that holds the number of the file, a tab (code point 9), and the text of the utterance. The text has been normalized, spelling out numbers and abbreviations. And now we pass this data on to you.

To create your custom voice using this data set:

1. Associate the custom voice data with your Speech service subscription by entering the subscription key in the Custom Voice portal, [cris.ai](http://cris.ai).
1. Upload the `hoover.zip` file (in the `voice` folder) as well as the `transcript.txt` file to the Custom Voice portal.
1. Use this data set to train a new custom voice. This will take a significant amount of time, so perhaps let it run overnight. (However, note you can train a custom speech model at the same time.)
1. Create a new endpoint to use with this voice. You'll find the URL under the endpoint's Details. Take a not eof it; we'll need it in the Hoover bot Web app. You want the 10-minute WebSocket (`wss:`) endpoint.

For full details on the uploading and training process, see [Creating custom voice fonts](https://docs.microsoft.com/azure/cognitive-services/speech-service/how-to-customize-voice-font).

### Customizing Speech Recognition

The JFK assassination documents include a number of terms not found in everyday English. Chief among these are the cryptonyms (code names) representing various persons, operations, locations, events, and even categories of secrecy. The cryptonym for Lee harvey Oswald, for example, is GPLOOR. It's important that when the user speaks "g p floor" that it's recognized as the cryptonym GPFLOOR so that it can be successfully used in a search. This can be done by customizing the *pronunciation model* of the speech-to-text function of the Speech service.

The pronunciation data to be submitted to the Custom Speech portal is a simple UTF-8 or ASCII text file containing the "dislpay form" of the term ("GPFLOOR" in this case), a tab character (code point 9), and the pronunciation of the term (here, "g p floor").

There are hundreds of known CIA cryptonyms. Fortunately, the JFK Files search demo includes a list of them, along with a description of each, in the `CryptonymLinker` skill for Cognitive Search. We have converted this list to the format required by the Custom Speech portal, removed the descriptions, and added the pronunciation of each term. The resulting file is included here as `cryptonyms.txt`. (Not to be confused with `cryptonyms.json`, which contains definitions of each term and is used by the bot's back-end to send back definitions of cryptonyms.)

Note that some cryptonyms are regular English words, like ZIPPER. There are still included in the pronunciation data because they should appear in their all-uppercase form when recognized. We've also included "JFK," which is not a cryptonym, but should be recognized as a single word.

**Tip**: To prevent shorter cryptonyms from being recognized instead of longer cryptonyms that begin with the shorter one (e.g. recognizing GP instead of GPFLOOR), we sorted the pronunciation file in reverse alphabetical order.

**Aside** Searching The JFK Files for "JFK" is not actually very useful, because nearly every document in the collection, even those related to other individuals, includes a cover page indicating that the document is part of the "JFK Assassination System." In some documents, a notice containing "JFK" appears on *every* page. So searching Kennedy's cryptonym, GPIDEAL, is more effective in finding documents of which he is the subject. (You might try adding some logic to the bot to help with this...)

Creating a custom language model using the cryptonym pronunciation data also requires language data; you can't train a language model without both files. The language file contains phrases or sentences that are likely to be uttered by a user. The language data is treated an addition to a base model provided by Microsoft, so it needn't be extensive. We have provided a file, `questions.txt`, consisting of a handful of sample questions that might be asked by users of the Hoover Bot.

With these two files, you're ready to adapt Speech Recognition.

1. Associate the custom recognition with your Speech service subscription by entering the subscription key in the Custom Speech portal.
1. Under Adaptation Data, upload `cryptonyms.txt` (in the `speech` folder) as a pronunciation data set and `questions.txt` as a language data set.
1. Use these data sets to train a new custom speech-to-text model. (This will take a significant amount of time. However, it can be done simultaneously with training the custom voice.)
1. Create an endpoint to be used with the custom speech model and make a note of it.

For full details on the uploading and training process, see [Enable custom pronunciation](https://docs.microsoft.com/azure/cognitive-services/speech-service/how-to-customize-pronunciation), [Create a custom language model](https://docs.microsoft.com/azure/cognitive-services/speech-service/how-to-customize-language-model). and [Create a custom speech-to-text endpoint](https://docs.microsoft.com/azure/cognitive-services/speech-service/how-to-create-custom-endpoint).

### Enabling custom speech in the Web app

1. Open the `bot.htm` file again and add your your Speech key and the required endpoints where indicated. You can find your key in the Azure dashboard and the endpoints in the [cris.ai portal](http://cris.ai/). The token endpoint just needs the hostname edited to reflect the region your subscription is in.

1. Build and publish the bot. The bot's Web Chat opens in a browser. It takes a moment for the bot to "warm up" after the service is restarted, so wait patiently until the bot displays its greeting. 

With the bot open in the browser, you can activate the "Use speech" checkbox. After Hoover's voice greets you, you can ask him questions such as "Mr. Hoover, what does GPFLOOR mean?" Note that the bot's speech recognition is temporarily disabled while the bot is speaking, and that recognition turns off automatically after twenty seconds of no speech.

## Technical details

The entirety of the bot's server-side logic is in `EchoWithCounterBot.cs`. Here are some high points.

* There are some static constants early on that can be changed to customize the bot's stock responses to greetings and other social niceties, or the maximum number of search results retained.

* Sending the initial greeting ("Hello, fellow investigator!") is more tricky than it might seem at first. When a chat starts, the bot receives a `ConversationUpdate` event for itself joining the chat and another for the user. So one part of the successful strategy is to ignore the bot's event and respond only to the actual user joining the chat. Also, only one instance of the bot is created for all users of the Web Chat, so we must make sure each Web Chat user has their own user ID. On top of all that, Web Chat doesn't send `ConversationUpdate` until the user sends his or her first message, so we need some way to force it. (We'll see how we deal with the latter two issues in a bit).

* Requests are processed by the method `OnTurnAsync`. Thhis method handles responses to three kinds of user requests. First, it detects greetings and such, and responds with a canned phrase. Second, it detects cryptonyms in user requests and responds with a definition. Finally, it executes search queries against the JFK Files' Azure Search back-end. In the case of cryptonyms, both the definition and searh results are performed.

* In the search results section there's a `do`/`while` loop that sends a typing indicator while the bot is performing the search. Once the typing message is sent, the Web Chat client displays a "..." animation for three seconds. Sometimes searches can take longer than that, leading to the user thinking the bot has stopped responding, and surprise when the bot seemingly randomly sends results later. So we continue sending a typing message every two seconds while the search completes, and another one as we begin preparing the response.

* Finally, search results queries are put together into a "carousel" of cards, each bearing an image thumbnail and text extracted from the document. This is done using a custom `AdaptiveCard` card layout class. Since most documents begin with a cover page that looks similar across the entire archive, we use the second thumbnai in multi-page documents to give each result a visually-distinguishable thumbnail.

Client-side, you'll find all our logic in `bot.htm`. This document is included into the main Hoover Bot page `default.htm` using an HTML `<iframe>` tag. In `bot.htm`, you'll find some CSS rules for styling the chat (including one to remove the Upload button, whic we don't use) and some HTML and JavaScript code. Here's an overview.

* We generate a unique user ID using `Date.now()`, which is milliseconds since the epoch. Our bot keeps track of the users it has greeted to avoid greeting them more than once. (Recall that there is only one instance of the bot for all Web Chat users.) So this makes sure users are greeted once and only once.

* Similarly, remember how we mentioned that Web Chat doesn't tell the bot a user has joined until that user sends a message? We work around that by manually sending a message. The messege is a "back channel" event message that is actually ignored by the server-side code, but it ensures that Web Chat sends a `ConversationUpdate`.

* You'll notice that when defined the `user` and `bot` objects, which represent those two user accounts, we made sure to assigne the correct `role` to each. This ensures that when we send a speech-derived question to the bot, that message is right-justified in the chat window just as though the user had typed it.

* We create a Direct Line connection and render the Web Chat interface pretty much exactly as you've seen it in the tutorials. However, we also subscribe to events containing a `speak` attribute so we can speak the bot's responses aloud.

Speaking of speech, as previously mentioned, the Web Chat app supports the Speech Service, but its Speech Service support does not extend to our custom speech and voice models. To integrate custom speech with the Web app, then, we have used the following approaches.

* For speech-to-text, we use the Speech Service JavaScript SDK's asynchronous speech recognition API. When the SDK recognizes a complete utterance, it is passed to a function that checks to see if the bot is being addressed (the utterance begins with "Mr. Hoover") and, if so, uses the Direct Line API to transmit the utterance to the bot. It appears the chat window soon afterward.

    Why didn't we just stuff the user's query into the input field and simulate a click on the Send button? Because doesn't actually work: the Web Chat requires actual user typing to fill the input field. We do stuff the user's query into the input field, but only to make it obvious what's being sent.

* For text-to-speech, we use the Speech Service's REST API to request an MP3 file of the utterance, then play that file using HMTL 5 audio. To allow this to proceed efficiently, two queues are used: one for the requests being made to the Speech Service, and the other for the audio files being played. This allows speech to be synthesized while still being spoken while ensuring that the bot's spoken responses don't overlap.

* Note that we temporarily disable speech recognition while the bot is talking. This prevents wasting bandwidth on speech recognition if the bot can overhear itself, and possibly sending spurious requests.

## Troubleshooting

The first troubleshooting step is always to double-check the keys and endpoints in `appsettings.json` and `bot.htm`. The following FAQ offers additional suggestions to various issues you may encounter.

### Q: While opening Web Chat in a browser from the local `default.hmt` and attempting to use speech, the browser frequently asks for permission to use the microphone.

A: The Web Chat app turns speech recognition off and on while the bot is speaking, and also turns it off after no speech has been detected for twenty seconds. For locally-hosted files, this causes you to be prompted repeatedly for permission to use the microphone. This is a security precaution taken by browser makers, and most don't have a way to turn that warning off for locally-hosted files. Instead, while the bot is running locally (press F5 in Visual Studio), access the page through `http://localhost:3839` using Chrome. Chrome will retain the microphone permission for the session.

### Q: I don't hear any responses in Hoover's voice while speech is on.

A: Another precaution taken by browser makers to protect users... in this case, to protect users from being annoyed. See previous answer for the solution.

### Q: My microphone doesn't work; the bot never recognizes what I say.

A: Make sure you are prefacing each request with "Mr. Hoover." Make sure your audio quality is good (record yourself saying something using Windows' Voice Recorder app, for example). Finally, check the browser's console (press F12 in Chrome or Edge, or Control-Shift-K in Firefox) while toggling speech on. If your browser can't access the microphone, an `NotAllawod` error message or similar appears in the browser console. Adjust your browser's settings to grant it access to the microphone.

### Q: The bot doesn't respond, or tells me something seems to have gone wrong.

A: The "gone wrong" message indicates an unhandled exception on the server side of the bot. You can view the server-side log in the Log Stream blade in the bot's App Service resource to help you figure it out. Some problems of this nature are capacity issues or are transient.

### Q: The speech checkbox keeps turning off.

A: Speech recognition automatically turns off after twenty seconds without speech. If you don't want this, change this line:

    `speechRecognizer.speechEndDetected = _ => element.checked && element.click();`

 to:

    `speechRecognizer.speechEndDetected = _ => speechRecognizer.startContinuousRecognitionAsync();`

This causes speech recognition to re-enable itself automatically whenever it times out. This workaround may not be necessary with future versions of the Speech SDK.

### Q: The bot doesn't load in the browser, is slow to respond, or doesn't send its initial greeting for a long time.

A: This can happen after you restart the bot or publish a new version from Visual Studio (which necessitates a restart). Just wait a moment and reload the page.

### Q: I'm running the bot in Visual Studio, but the bot's behavior doesn't reflect the changes I have made recently to the source code.

A: The Web Chat launched by the build process uses the version of the bot running in the Azure cloud. Use the Bot Framework Emulator to test the local bot, or else publish the project if you want to test it in a browser.

### Q: What are `CounterState.cs` and `EchoBotAccessors.cs` used for?

Q: In the Hoover Bot, nothing. These files are left over from the `EchoBot` sample that you initally downloaded. You can ignore them (or [check out that sample](https://github.com/Microsoft/BotBuilder-Samples/tree/master/samples), obviously).

### Q: How do I upgrade the Bot and Speech libraries?

A: For the server-side C# application, NuGet has you covered. 

The Bot Framework JavaScript library is delivered by a CDN. Simply change the version number in the `<script>` tag's URL to the one you want, or `latest` to use the latest version.

The Speech Service JavaScript library is provided as part of this project and served from the Asame zure Web site that hosts the bot. [Download the latest version](https://aka.ms/csspeech/jsbrowserpackage) and copy `microsoft.cognitiveservices.speech.sdk.bundle-min.js` from the zip file into the Visual Studio project's `wwwroot` folder.