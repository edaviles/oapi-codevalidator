using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using System.Net.Http.Json;
using Azure;
using static System.Environment;
using Azure.AI.OpenAI;

namespace skapi.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class ValidateCodeController : ControllerBase
    {

        static string endpoint = GetEnvironmentVariable("AZURE_OPENAI_ENDPOINT");
        static string key = GetEnvironmentVariable("AZURE_OPENAI_KEY");
        static string system = GetEnvironmentVariable("SYSTEM_WHO");
        static string tokens = GetEnvironmentVariable("TOKENS_LIMIT");
        static string deployment = GetEnvironmentVariable("DEPLOYMENT_MODEL");

        OpenAIClient client = new(new Uri(endpoint), new AzureKeyCredential(key));

        ChatCompletionsOptions chatCompletionsOptions = new ChatCompletionsOptions()
        {
            DeploymentName = deployment, //This must match the custom deployment name you chose for your model
            Messages =
            {
                new ChatRequestSystemMessage(system),
                new ChatRequestUserMessage("## Source ##\r\n          protected override async Task OnMessageActivityAsync(ITurnContext<IMessageActivity> turnContext, CancellationToken cancellationToken)\n{\n    // var replyText = $\"Echo: {turnContext.Activity.Text}\";\n\n    string endpoint = GetEnvironmentVariable(\"AZURE_OPENAI_ENDPOINT\");\n    string key = GetEnvironmentVariable(\"AZURE_OPENAI_KEY\");\n    string system = GetEnvironmentVariable(\"SYSTEM_WHO\");\n    string tokens = GetEnvironmentVariable(\"TOKENS_LIMIT\");\n    string deployment = GetEnvironmentVariable(\"DEPLOYMENT_MODEL\");\n\n    OpenAIClient client = new(new Uri(endpoint), new AzureKeyCredential(key));\n\n    var chatCompletionsOptions = new ChatCompletionsOptions()\n    {\n        Messages =\n        {\n            new ChatMessage(ChatRole.System, system),\n            new ChatMessage(ChatRole.User, turnContext.Activity.Text),\n        },\n        MaxTokens = int.Parse(tokens)\n    };\n\n\n    Response<ChatCompletions> response = client.GetChatCompletions(deploymentOrModelName: deployment,chatCompletionsOptions);\n\n    var replyText = response.Value.Choices[0].Message.Content;\n    await turnContext.SendActivityAsync(MessageFactory.Text(replyText, replyText), cancellationToken);\n\n\n}                        ## End ##"),
             //   new ChatRequestAssistantMessage("Yes, customer managed keys are supported by Azure OpenAI."),
                new ChatRequestUserMessage("Help me to validate if the code support best practices based on Secure Code practice?, answering Yes, is secure or Not Secure, is your answer is not add a summary with details in 2 lines") 
            },
            MaxTokens = int.Parse(tokens)
        };

        [HttpGet(Name = "GetValidateCode")]
        public async Task<IActionResult> GenerateText(string code)
        {
            try
            {

                Response<ChatCompletions> response = client.GetChatCompletions(chatCompletionsOptions);

                return Ok(response.Value.Choices[0].Message.Content);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}