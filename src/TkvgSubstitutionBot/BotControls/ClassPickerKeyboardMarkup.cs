using Telegram.Bot.Types.ReplyMarkups;

namespace TkvgSubstitutionBot.BotControls;

public static class ClassPickerKeyboardMarkup
{
 
    public static InlineKeyboardMarkup GetClassPickerKeyboard(string command)
    {
        var inlineMarkup = new InlineKeyboardMarkup()
            .AddNewRow()
            .AddButton("3A", $"{command}:3.a")
            .AddButton("4B", $"{command}:4.b")
            .AddNewRow()
            .AddButton("Все", $"{command}:all");
        return inlineMarkup;
    }
}