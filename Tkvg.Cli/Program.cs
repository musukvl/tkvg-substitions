using Spectre.Console.Cli;
using Tkvg.Cli.Commands;

var app = new CommandApp<SubstitutionsCommand>();

app.Configure(config =>
{
    config.SetApplicationName("tkvg-substitutions");
    config.AddExample("23.03");
    config.AddExample("23.03", "--class", "3b");
});

return await app.RunAsync(args);
