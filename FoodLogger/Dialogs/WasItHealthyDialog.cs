using FoodLogger.Services;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Connector;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;

namespace FoodLogger.Dialogs
{
    [Serializable]
    public class WasItHealthyDialog : IDialog<object>
    {

        public async Task StartAsync(IDialogContext context)
        {
            context.Wait(this.MessageReceivedAsync);
        }


        public virtual async Task MessageReceivedAsync(IDialogContext context, IAwaitable<IMessageActivity> result)
        {
            //get disambiguated food from bot state
            List<string> DisambiguatedFoods = new List<string>();
            context.ConversationData.TryGetValue("DisambiguatedFoods", out DisambiguatedFoods);
            var disambiguatedFoodsString = (DisambiguatedFoods.Count > 0) ?
                string.Join(" ", DisambiguatedFoods) :
                "your meal";


            //check if the meal was healthy and report back to user
            var isHealthy = FoodService.IsMealHealthy(DisambiguatedFoods);
            if (isHealthy)
            {
                string text = ($"Nice one, you made a healthy choice with {disambiguatedFoodsString}, you're on the right track");
                await context.PostAsync(text);
            }
            else
            {
                string text = ($"Tsk tsk, {disambiguatedFoodsString} was not a great choice. I can recommend some healthier options next time if you wish?");
                await context.PostAsync(text);
            }

            context.Done(isHealthy);
        }

    }
}