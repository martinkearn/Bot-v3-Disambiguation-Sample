using FoodLogger.Services;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Luis;
using Microsoft.Bot.Builder.Luis.Models;
using Microsoft.Bot.Connector;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Web;

namespace FoodLogger.Dialogs
{

    [Serializable]
    public class LogMealDialog : IDialog<object>
    {
        [NonSerialized]
        private IList<string> _foodEntitiesFromLuis;
        private IList<string> _disambiguatedFoods;

        public async Task StartAsync(IDialogContext context)
        {
            context.Wait(MessageReceivedAsync);
        }

        public virtual async Task MessageReceivedAsync(IDialogContext context, IAwaitable<IMessageActivity> argument)
        {
            List<string> FoodEntities = new List<string>();
            context.ConversationData.TryGetValue("FoodEntities", out FoodEntities);

            //example message from user "i had 2 bananas a pastry and a coffee"
            _foodEntitiesFromLuis = new List<string>();
            _disambiguatedFoods = new List<string>();

            //enumerate food entities
            //var FoodEntities = new List<string>() { "bananas", "pastry", "coffee" };
            foreach (var foodEntity in FoodEntities)
            {
                _foodEntitiesFromLuis.Add(foodEntity);
            }

            //disambiguate foods
            await DisambiguateFoodAsync(context, null);
        }

        private async Task DisambiguateFoodAsync(IDialogContext context, IAwaitable<object> result)
        {
            string disambiguatedFood = null;

            //grab the incoming message text
            if (result != null)
            {
                object awaitedResultObject = await result;

                if (awaitedResultObject is Activity)
                {
                    disambiguatedFood = (awaitedResultObject as Activity).Text;
                }
                else if (awaitedResultObject is string)
                {
                    disambiguatedFood = awaitedResultObject as string;
                }
            }

            //add the incoming message to the disambiguated foods list and remove from the orginal entities list
            if (!string.IsNullOrEmpty(disambiguatedFood))
            {
                _disambiguatedFoods.Add(disambiguatedFood);
                //This is null after the first button is shown which is what ia causing the exception
                _foodEntitiesFromLuis.Remove(_foodEntitiesFromLuis.First());
            }

            if (_foodEntitiesFromLuis.Count > 0)
            {
                //Create card to present specific food choices 
                IMessageActivity messageButtons = (Activity)context.MakeMessage();
                messageButtons.Recipient = messageButtons.From;
                messageButtons.Type = "message";
                messageButtons.Attachments = new List<Attachment>();
                List<CardAction> cardButtons = new List<CardAction>();
                var disambiguatedFoods = FoodService.GetFoods(_foodEntitiesFromLuis.First());
                foreach (var food in disambiguatedFoods)
                {
                    cardButtons.Add(new CardAction() { Value = food, Type = "imBack", Title = food });
                }
                HeroCard plCard = new HeroCard()
                {
                    Title = null,
                    Subtitle = string.Format("You said {0}, which one did you mean?", _foodEntitiesFromLuis.First()),
                    Images = null,
                    Buttons = cardButtons
                };
                messageButtons.Attachments.Add(plCard.ToAttachment());
                await context.PostAsync(messageButtons);

                //wait for repsonse
                context.Wait(null);
            }
            else
            {
                //Store disambiguated foods in bot state so other dialogs can access it
                context.ConversationData.SetValue("DisambiguatedFoods", _disambiguatedFoods);

                //check that the user is happy to log the meal, if not start again
                PromptDialog.Confirm(
                    context,
                    AfterSummaryAsync,
                    $"You selected {string.Join(" ", _disambiguatedFoods)}... Is that correct?",
                    "Didn't get that!",
                    promptStyle: PromptStyle.None);
            }
        }

        public async Task AfterSummaryAsync(IDialogContext context, IAwaitable<bool> result)
        {
            var confirm = await result;
            if (confirm)
            {
                //user is happy, log the meal here
                FoodService.LogMeal(_disambiguatedFoods);

                //pass over to the WasItHealthyDialog flow
                await context.Forward(new WasItHealthyDialog(), ResumeAfterWasItHealthyDialog, result, CancellationToken.None);
            }
            else
            {
                //user is not happy, clear and start again
                context.ConversationData.Clear();
                _disambiguatedFoods.Clear();
                _foodEntitiesFromLuis.Clear();
                await context.PostAsync("Sorry that is not right, please try entering your meal again in a detailed way.");
                context.Wait(null);
                //context.Done(_disambiguatedFoods);
            }
        }

        private async Task ResumeAfterWasItHealthyDialog(IDialogContext context, IAwaitable<object> result)
        {
            //close the dialog
            //context.Done(_disambiguatedFoods);

            await context.PostAsync("ResumeAfterWasItHealthyDialog done");
            context.Wait(null);
        }


    }
}