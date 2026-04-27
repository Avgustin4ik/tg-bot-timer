using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Extensions;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.InlineQueryResults;
using Telegram.Bot.Types.ReplyMarkups;

namespace Console.Advanced.Services;

public class UpdateHandler(ITelegramBotClient bot, ILogger<UpdateHandler> logger) : IUpdateHandler
{
    private static readonly InputPollOption[] PollOptions = ["Hello", "World!"];

    public async Task HandleErrorAsync(ITelegramBotClient botClient, Exception exception, HandleErrorSource source, CancellationToken cancellationToken)
    {
        logger.LogInformation("HandleError: {Exception}", exception);
        // Cooldown in case of network connection error
        if (exception is RequestException)
            await Task.Delay(TimeSpan.FromSeconds(2), cancellationToken);
    }

    public async Task HandleUpdateAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        await (update switch
        {
            { Message: { } message }                        => OnMessage(message),
            { EditedMessage: { } message }                  => OnMessage(message),
            { CallbackQuery: { } callbackQuery }            => OnCallbackQuery(callbackQuery),
            _                                               => UnknownUpdateHandlerAsync(update)
        });
    }

    private async Task OnMessage(Message msg)
    {
        logger.LogInformation("Receive message type: {MessageType}", msg.Type);
        if (msg.Text is not { } messageText)
            return;

        Message sentMessage = await (messageText.Split(' ')[0] switch
        {
            "/set" => SetTimer(msg),
            "/change" => ChangeTimer(msg),
            "/remove" => RemoveTimer(msg),
            "/inline_mode" => StartInlineQuery(msg),
            "/html" => SendViaHtml(msg),
            "/throw" => FailingHandler(msg),
            _ => Usage(msg)
        });
        logger.LogInformation("The message was sent with id: {SentMessageId}", sentMessage.Id);
    }

    private Task<Message> RemoveTimer(Message msg)
    {
        throw new NotImplementedException();
    }

    private Task<Message> ChangeTimer(Message msg)
    {
        throw new NotImplementedException();
    }

    private async Task<Message> ConfirmationMessage(Message msg, TimeSpan duration)
    {
        string message = $"Установить таймер на\n" +
                         $"{duration}";
        var inlineMarkup = new InlineKeyboardMarkup()
            .AddNewRow()
            .AddButtons(new InlineKeyboardButton("Yes", "Y"),
                new InlineKeyboardButton("No", "N"));
        return await bot.SendMessage(msg.Chat, message, replyMarkup: inlineMarkup);
    }

    private async Task<Message> SetTimer(Message msg)
    {
        //todo check if we have timer
        //if true = go to change timer
        const string message ="""
                  <b><u>Новый таймер</u></b>:
                  На какой период вы хотите установить таймер? Введите время в удобном для вас формате либо выберите один из готовых вариантов.
              """;
        var inlineMarkup = new InlineKeyboardMarkup()
            .AddNewRow()
            .AddButtons(new InlineKeyboardButton("5 min", "5"),
                        new InlineKeyboardButton("30 min", "30"),
                        new InlineKeyboardButton("60 min", "60"));
        //todo setup timer at recived time
        // return await SendInlineKeyboard(msg);
        return await bot.SendMessage(msg.Chat, message, ParseMode.Html, replyMarkup: inlineMarkup);
    }

    async Task<Message> Usage(Message msg)
    {
        const string usage = """
                <b><u>Bot menu</u></b>:
                /set            - setup timer
                /change         - change existing timer
                /remove         - remove existing timer
            """;
        return await bot.SendMessage(msg.Chat, usage, parseMode: ParseMode.Html, replyMarkup: new ReplyKeyboardRemove());
    }

    async Task<Message> SendInlineKeyboard(Message msg)
    {
        var inlineMarkup = new InlineKeyboardMarkup()
            .AddNewRow("1.1", "1.2", "1.3")
            .AddNewRow()
                .AddButton("WithCallbackData", "CallbackData")
                .AddButton(new InlineKeyboardButton("WithUrl", "https://github.com/TelegramBots/Telegram.Bot"));
        return await bot.SendMessage(msg.Chat, "Inline buttons:", replyMarkup: inlineMarkup);
    }

    async Task<Message> SendReplyKeyboard(Message msg)
    {
        var replyMarkup = new ReplyKeyboardMarkup(true)
            .AddNewRow("1.1", "1.2", "1.3")
            .AddNewRow().AddButton("2.1").AddButton("2.2");
        return await bot.SendMessage(msg.Chat, "Keyboard buttons:", replyMarkup: replyMarkup);
    }

    async Task<Message> RemoveKeyboard(Message msg)
    {
        return await bot.SendMessage(msg.Chat, "Removing keyboard", replyMarkup: new ReplyKeyboardRemove());
    }

    async Task<Message> StartInlineQuery(Message msg)
    {
        var button = new InlineKeyboardButton("Inline Mode", InlineButtonType.SwitchInlineQueryCurrentChat);
        return await bot.SendMessage(msg.Chat, "Press the button to start Inline Query\n\n" +
            "(Make sure you enabled Inline Mode in @BotFather)", replyMarkup: new InlineKeyboardMarkup(button));
    }

    async Task<Message> SendViaHtml(Message msg)
    {
        var space = msg.Text?.IndexOf(' ') ?? -1;
        if (space == -1) return await bot.SendMessage(msg.Chat, "Usage: /html [html]");
        return (await bot.SendHtml(msg.Chat, msg.Text![(space + 1)..]))[0];
    }

    static Task<Message> FailingHandler(Message msg)
    {
        throw new NotImplementedException("FailingHandler");
    }

    // Process Inline Keyboard callback data
    private async Task OnCallbackQuery(CallbackQuery callbackQuery)
    {
        logger.LogInformation("Received inline keyboard callback from: {CallbackQueryId}", callbackQuery.Id);
        await bot.AnswerCallbackQuery(callbackQuery.Id, $"Received {callbackQuery.Data}");
        await bot.SendMessage(callbackQuery.Message!.Chat, $"Received {callbackQuery.Data}");
    }

    #region Inline Mode

    private async Task OnInlineQuery(InlineQuery inlineQuery)
    {
        logger.LogInformation("Received inline query from: {InlineQueryFromId}", inlineQuery.From.Id);

        InlineQueryResult[] results = [ // displayed result
            new InlineQueryResultArticle("1", "Telegram.Bot", new InputTextMessageContent("hello")),
            new InlineQueryResultArticle("2", "is the best", new InputTextMessageContent("world"))
        ];
        await bot.AnswerInlineQuery(inlineQuery.Id, results, cacheTime: 0, isPersonal: true);
    }

    private async Task OnChosenInlineResult(ChosenInlineResult chosenInlineResult)
    {
        logger.LogInformation("Received inline result: {ChosenInlineResultId}", chosenInlineResult.ResultId);
        await bot.SendMessage(chosenInlineResult.From.Id, $"You chose result with Id: {chosenInlineResult.ResultId}");
    }

    #endregion
    
    private Task UnknownUpdateHandlerAsync(Update update)
    {
        logger.LogInformation("Unknown update type: {UpdateType}", update.Type);
        return Task.CompletedTask;
    }
}
