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
    [LuisModel("b403229d-c55a-4bdc-a840-a2b7e25e6634", "d004b0b064694dd1bec537e3629863fb")]
    [Serializable]
    public class RootDialog : LuisDialog<IList<string>>
    {
        [NonSerialized]
        private IMessageActivity _originActivity;
        private IList<string> _foodEntitiesFromLuis;
        private IList<string> _disambiguatedFoods;

        protected override async Task MessageReceived(IDialogContext context, IAwaitable<IMessageActivity> item)
        {
            _originActivity = await item;
            await base.MessageReceived(context, item);
        }

        [LuisIntent("None")]
        public async Task None(IDialogContext context, LuisResult result)
        {
            await RequestMealDetails(context, null);
        }

        [LuisIntent("LogMeal")]
        public async Task LogMeal(IDialogContext context, LuisResult result)
        {
            var foodEntities = result.Entities.Where(x => x.Type == "Food");

            if (foodEntities.Count() == 0)
            {
                //no foods found, ask the user to enter what they ate
                await RequestMealDetails(context, null);
            }
            else
            {
                //example message from user "i had 2 bananas a pastry and a coffee"
                _foodEntitiesFromLuis = new List<string>();
                _disambiguatedFoods = new List<string>();

                //enumerate food entities
                foreach (var foodEntity in foodEntities)
                {
                    _foodEntitiesFromLuis.Add(foodEntity.Entity);
                }

                //disambiguate foods
                await DisambiguateFoodAsync(context, null);
            }
        }

        private async Task RequestMealDetails(IDialogContext context, IAwaitable<object> result)
        {
            string text = string.Format("Please tell me what you ate");

            await context.PostAsync(text);

            context.Done(_disambiguatedFoods);
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
                context.Wait(DisambiguateFoodAsync);
            }
            else
            {
                await Summary(context, null);
            }
        }

        private async Task Summary(IDialogContext context, IAwaitable<object> result)
        {
            string text = string.Format("You selected {0}... I'll log that for you", string.Join(" ", _disambiguatedFoods));
            await context.PostAsync(text);

            //pass over to the WasItHealthyDialog flow
            await context.Forward(new WasItHealthyDialog(), ResumeAfterWasItHealthyDialog, result, CancellationToken.None);
        }

        private async Task ResumeAfterWasItHealthyDialog(IDialogContext context, IAwaitable<object> result)
        {
            context.Wait(MessageReceived);
            context.Done(_disambiguatedFoods);
        }


    }
}