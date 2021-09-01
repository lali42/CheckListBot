using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace CheckListBot.Dialogs
{
    public class AddItemsDialog : ComponentDialog
    {
        private readonly IStatePropertyAccessor<CheckList> _checkListAccessor;
        private readonly IStatePropertyAccessor<Items> _itemsAccessor;

        public AddItemsDialog(UserState userState) : base(nameof(AddItemsDialog))
        {
            _checkListAccessor = userState.CreateProperty<CheckList>("CheckList");
            _itemsAccessor = userState.CreateProperty<Items>("Items");

            var waterfallSteps = new WaterfallStep[]
            {
                GetValueAsync,
                AddItemstoListAsync,
                ShowListItemsAsync,
                CheckListAysnc,
                EndLoopAsync,
            };
            AddDialog(new WaterfallDialog(nameof(WaterfallDialog), waterfallSteps));
            AddDialog(new TextPrompt(nameof(TextPrompt)));
            AddDialog(new NumberPrompt<int>(nameof(NumberPrompt<int>)));
            AddDialog(new ChoicePrompt(nameof(ChoicePrompt)));

            InitialDialogId = nameof(WaterfallDialog);
        }
        private async Task<DialogTurnResult> GetValueAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            return await stepContext.PromptAsync(nameof(TextPrompt), new PromptOptions { Prompt = MessageFactory.Text($"Enter your list, or `done` for checklist.") }, cancellationToken);
        }
        private async Task<DialogTurnResult> AddItemstoListAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var checkList = await _checkListAccessor.GetAsync(stepContext.Context, () => new CheckList(), cancellationToken);
            var items = await _itemsAccessor.GetAsync(stepContext.Context, () => new Items(), cancellationToken);
            stepContext.Values["item"] = stepContext.Result;
            var item = (string)stepContext.Values["item"];

            items.Title = item;
            var listItems = checkList.Lists;

            var list = stepContext.Options as List<string> ?? new List<string>();

            if (item != "done")
            {
                listItems.Add(items);
                foreach (var i in listItems)
                {
                    list.Add(i.Title);
                }
                await stepContext.PromptAsync(nameof(TextPrompt), new PromptOptions { Prompt = MessageFactory.Text($"{string.Join(",", list)}") }, cancellationToken);
            }
            if (item == "done" && listItems.Count != 0)
            {
                return await stepContext.NextAsync(listItems, cancellationToken);
            }
            else
            {
                return await stepContext.ReplaceDialogAsync(nameof(AddItemsDialog), listItems, cancellationToken);
            }
        }
        private async Task<DialogTurnResult> ShowListItemsAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            return await stepContext.PromptAsync(nameof(TextPrompt), new PromptOptions { Prompt = MessageFactory.Text($"Enter the number to check your item. \n You can use the mark , squeeze and use `done` when you finish.") }, cancellationToken);
        }
        private async Task<DialogTurnResult> CheckListAysnc(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var checkList = await _checkListAccessor.GetAsync(stepContext.Context, () => new CheckList(), cancellationToken);
            var listItems = checkList.Lists;
            stepContext.Values["check"] = stepContext.Result;
            var check = (string)stepContext.Values["check"];
            var name = stepContext.Context.Activity.From.Name;

            if (check != "done")
            {
                if (listItems.Count == 0)
                {
                    return await stepContext.PromptAsync(nameof(TextPrompt), new PromptOptions { Prompt = MessageFactory.Text($"{name} Say : Don't have item.") }, cancellationToken);
                }
                else
                {
                    var listcheck = check.Split(",").ToList();
                    var listCheck = listcheck.Select(int.Parse).ToList();
                    int index = 1;

                    if (listCheck.Max() > listItems.Count)
                    {
                        await stepContext.PromptAsync(nameof(TextPrompt), new PromptOptions { Prompt = MessageFactory.Text("Don't have your slect number!") }, cancellationToken);
                        return await stepContext.ContinueDialogAsync(cancellationToken);
                    }
                    else
                    {
                        var list = stepContext.Options as List<string> ?? new List<string>();
                        for (int i = 0; i < listCheck.Count; i++)
                        {
                            var item = listCheck[i];
                            listItems.RemoveAt(item - index);
                            index++;
                        }
                        foreach (var item in listItems)
                        {
                            list.Add(item.Title);
                        }
                        var promtOptions = new PromptOptions { Prompt = MessageFactory.Text($"{string.Join(",", list)}") };
                        await stepContext.PromptAsync(nameof(TextPrompt), promtOptions, cancellationToken);
                        return await stepContext.ContinueDialogAsync(cancellationToken);
                    }
                }
            }
            else
            {
                //await stepContext.Context.SendActivityAsync(MessageFactory.Text($"Finish"), cancellationToken);
                return await stepContext.PromptAsync(nameof(TextPrompt), new PromptOptions { Prompt = MessageFactory.Text($"Finish {name}") }, cancellationToken);
                //return await stepContext.NextAsync(null,cancellationToken);
            }
        }
        private async Task<DialogTurnResult> EndLoopAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            return await stepContext.EndDialogAsync(null,cancellationToken);
        }

    }
}
