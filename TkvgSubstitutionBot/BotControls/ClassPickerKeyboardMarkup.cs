using Telegram.Bot.Types.ReplyMarkups;

namespace TkvgSubstitutionBot.BotControls;

public static class ClassPickerKeyboardMarkup
{
    /// <summary>
    /// Keyboard for query flows (today/next day): [3A] [4B] / [Ввести] [Все]
    /// </summary>
    public static InlineKeyboardMarkup GetClassPickerKeyboard(string command)
    {
        return new InlineKeyboardMarkup()
            .AddNewRow()
            .AddButton("3A", $"{command}:3.a")
            .AddButton("4B", $"{command}:4.b")
            .AddNewRow()
            .AddButton("Ввести", $"enter_class:{command}")
            .AddButton("Все", $"{command}:all");
    }

    /// <summary>
    /// Keyboard for subscribe flow: [3A] [4B] / [Ввести] (no "Все")
    /// </summary>
    public static InlineKeyboardMarkup GetSubscribeClassPickerKeyboard(string command)
    {
        return new InlineKeyboardMarkup()
            .AddNewRow()
            .AddButton("3A", $"{command}:3.a")
            .AddButton("4B", $"{command}:4.b")
            .AddNewRow()
            .AddButton("Ввести", $"enter_class:{command}");
    }
}
