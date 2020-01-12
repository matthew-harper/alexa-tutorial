using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Amazon.Lambda.Core;
using Alexa.NET.Response;
using Alexa.NET.Request;
using Alexa.NET.Request.Type;
using Newtonsoft.Json;
using Alexa.NET;

// Assembly attribute to enable the Lambda function's JSON input to be converted into a .NET class.
[assembly: LambdaSerializerAttribute(typeof(Amazon.Lambda.Serialization.Json.JsonSerializer))]

namespace tutorial
{
    public class Function
    {
        public SkillResponse FunctionHandler(SkillRequest input, ILambdaContext context)
        {
            ILambdaLogger log = context.Logger;
            log.LogLine($"Skill Request Object:" + JsonConvert.SerializeObject(input));

            Session session = input.Session;
            if (session.Attributes == null)
                session.Attributes = new Dictionary<string, object>();

            Type requestType = input.GetRequestType();
            if (input.GetRequestType() == typeof(LaunchRequest))
            {
                string speech = "Welcome! Say new game to start";
                Reprompt rp = new Reprompt("Say new game to start");
                return ResponseBuilder.Ask(speech, rp, session);
            }
            else if (input.GetRequestType() == typeof(SessionEndedRequest))
            {
                return ResponseBuilder.Tell("Goodbye!");
            }
            else if (input.GetRequestType() == typeof(IntentRequest))
            {
                var intentRequest = (IntentRequest)input.Request;
                switch (intentRequest.Intent.Name)
                {
                    case "AMAZON.CancelIntent":
                    case "AMAZON.StopIntent":
                        return ResponseBuilder.Tell("Goodbye!");
                    case "AMAZON.HelpIntent":
                        {
                            Reprompt rp = new Reprompt("What's next?");
                            return ResponseBuilder.Ask("Here's some help. What's next?", rp, session);
                        }
                    case "NewGameIntent":
                        {
                            session.Attributes["num_guesses"] = 0;
                            Random rnd = new Random();
                            Int32 magicNumber = rnd.Next(1, 10);
                            session.Attributes["magic_number"] = magicNumber;

                            string next = "Guess a number betwen 1 and 10";
                            Reprompt rp = new Reprompt(next);
                            return ResponseBuilder.Ask(next, rp, session);
                        }
                    case "AnswerIntent":
                        {
                            // check answer
                            string userString = intentRequest.Intent.Slots["Number"].Value;
                            Int32 userInt = 0;
                            Int32.TryParse(userString, out userInt);
                            bool correct = (userInt == (Int32)(long)session.Attributes["magic_number"]);
                            Int32 numTries = (Int32)(long)session.Attributes["num_guesses"] + 1;
                            string speech = "";
                            if (correct)
                            {
                                speech = "Correct! You guessed it in " + numTries.ToString() + " tries. Say new game to play again, or stop to exit. ";
                                session.Attributes["num_guesses"] = 0;
                            }
                            else
                            {
                                speech = "Nope, guess again.";
                                session.Attributes["num_guesses"] = numTries;
                            }
                            Reprompt rp = new Reprompt("speech");
                            return ResponseBuilder.Ask(speech, rp, session);
                        }
                    default:
                        {
                            log.LogLine($"Unknown intent: " + intentRequest.Intent.Name);
                            string speech = "I didn't understand - try again?";
                            Reprompt rp = new Reprompt(speech);
                            return ResponseBuilder.Ask(speech, rp, session);
                        }
                }
            }
            return ResponseBuilder.Tell("Goodbye!");
        }

    }
}