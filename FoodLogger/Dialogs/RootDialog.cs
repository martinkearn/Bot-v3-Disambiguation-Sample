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
    public class RootDialog : LuisDialog<object>
    {
        [NonSerialized]
        private IMessageActivity _originActivity;

        protected override async Task MessageReceived(IDialogContext context, IAwaitable<IMessageActivity> item)
        {
            _originActivity = await item;
            await base.MessageReceived(context, item);
        }

        [LuisIntent("LogMeal")]
        public async Task LogMealAsync(IDialogContext context, LuisResult result)
        {
            var foodEntities = result.Entities.Where(x => x.Type == "Food").Select(x => x.Entity).ToList();

            if (foodEntities.Count() == 0)
            {
                //no foods found, ask the user to enter what they ate
                await context.PostAsync("Please tell me what you ate");
            }
            else
            {
                //Store food entities in bot state so other dialogs can access it
                context.ConversationData.SetValue("FoodEntities", foodEntities);

                //call Log Meal dialog
                await context.Forward(new LogMealDialog(), ResumeAfterLogMealDialog, result, CancellationToken.None);
            }
        }

        private async Task ResumeAfterLogMealDialog(IDialogContext context, IAwaitable<object> result)
        {
            //close the dialog
            await context.PostAsync("Thanks for entering your meal. What would you like to do next? You can say 'Help' to see what I can do.");
            context.Wait(MessageReceived);
        }
    }
}