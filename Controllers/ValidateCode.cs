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
                new ChatRequestUserMessage("## Source ##\r\n          IDENTIFICATION DIVISION. PROGRAM-ID. LENGTH-CALCULATOR. DATA DIVISION. WORKING-STORAGE SECTION. 01 WS-INPUT PIC X(100). 01 WS-LENGTH PIC 9(3). PROCEDURE DIVISION. BEGIN. DISPLAY \"Enter a string: \". ACCEPT WS-INPUT. COMPUTE WS-LENGTH = FUNCTION LENGTH(WS-INPUT). DISPLAY \"The length of the string is: \" WS-LENGTH. STOP RUN. END PROGRAM LENGTH-CALCULATOR.                ## End ##"),
                new ChatRequestUserMessage("Help me to validate if the code support best practices based on Secure Code practice?, answering Yes, is secure or Not Secure, is your answer is not add a summary with details in 2 lines") 
            },
            MaxTokens = int.Parse(tokens)
        };

        /// [HttpGet(Name = "GetValidateCode")]
        public async Task<IActionResult> GenerateText(string code)
        {
            try
            {
                if (code == null)
                {
                    return BadRequest("Error, please provide a code to validate");
                } 
                else 
                {
                    if (code == "demo")
                    {
                        return Ok("Yourt code is secure, good job!");
                    }
                    else 
                    {
                        Response<ChatCompletions> response = client.GetChatCompletions(chatCompletionsOptions);
                        
                        return BadRequest(response.Value.Choices[0].Message.Content);
                    }
                }
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}