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

            await context.PostAsync($"inside was it health dialog.");

            context.Done(string.Empty);
        }


        //private async Task WasItHealthy(IDialogContext context, IAwaitable<object> result)
        //{
        //    var isHealthy = FoodService.IsMealHealthy(_disambiguatedFoods);

        //    if (isHealthy)
        //    {
        //        string text = string.Format("Nice one, you made a healthy choice, you're on the right track");
        //        await context.PostAsync(text);
        //    }
        //    else
        //    {
        //        string text = string.Format("Tsk tsk, that was not a great choice. I can recommend some healthier options next time if you wish?");
        //        await context.PostAsync(text);
        //    }

        //    context.Done(_disambiguatedFoods);
        //}
    }
}