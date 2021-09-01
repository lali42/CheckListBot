using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;


namespace CheckListBot.Dialogs
{
    public class MainDialog : ComponentDialog
    {
        public MainDialog(UserState userState) : base(nameof(MainDialog))
        {
            var waterfallSteps = new WaterfallStep[]
            {
                StartBotAsync,
                InitialStepAsync,
                FinalStepAsync,
            };
            AddDialog(new WaterfallDialog(nameof(WaterfallDialog), waterfallSteps));
            AddDialog(new TextPrompt(nameof(TextPrompt)));

            //InitialDialogId = nameof(WaterfallDialog);
        }

        private async Task<DialogTurnResult> StartBotAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var promt = new PromptOptions
            {
                Prompt = MessageFactory.Text($"start bot"),
                RetryPrompt = MessageFactory.Text("Enter againt.")
            };
            return await stepContext.PromptAsync(nameof(TextPrompt), promt, cancellationToken);
            //return await stepContext.BeginDialogAsync(nameof(AddItemsDialog), null, cancellationToken);
        }
        private async Task<DialogTurnResult> InitialStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            stepContext.Values["direction"] = (string)stepContext.Result;
            var directions = (string)stepContext.Values["direction"];

            int count = directions.Count();
            var symbol = directions.Substring(0, 1);
            var direction = directions.Substring(1, count - 1);

            string message = "";
            //var promt = new PromptOptions { Prompt = MessageFactory.Text($"Please use the mark `!` and followed by `create` , `check` or `list`. ") };


            if (symbol == "!")
            {
                if (direction == "create")
                {
                    message = "create";
                    return await stepContext.BeginDialogAsync(nameof(AddItemsDialog), null, cancellationToken);
                }
                else if (direction == "check")
                {
                    return await stepContext.BeginDialogAsync(nameof(CheckListDialog), null, cancellationToken);
                }
                else if (direction == "list")
                {
                    message = "You have the following.";
                    return await stepContext.NextAsync(new PromptOptions { Prompt = MessageFactory.Text(message) }, cancellationToken);
                }
                else
                {
                    await stepContext.PromptAsync(nameof(TextPrompt), new PromptOptions { Prompt = MessageFactory.Text($"Please use the mark `!` and followed by `create` , `check` or `list`.") }, cancellationToken);
                    return await stepContext.ReplaceDialogAsync(nameof(MainDialog), null, cancellationToken);
                }
            }
            else
            {
                return await stepContext.EndDialogAsync(null, cancellationToken);
            }
        }

        private async Task<DialogTurnResult> FinalStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            return await stepContext.BeginDialogAsync(nameof(CardItemsDialog), null, cancellationToken);
        }
    }
}
