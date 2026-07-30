using System.Text.Json;
using Task_CLI;
using Terminal.Gui;
using CommandLine;



List<Task_CLI.Task> tasks;
string jsonText = String.Empty;
bool fileExists = File.Exists("data.json");

if (fileExists)
{
    jsonText = File.ReadAllText("data.json");
    var deserializedData = JsonSerializer.Deserialize<List<Task_CLI.Task>>(jsonText);
    if (deserializedData == null)
    {
        tasks = new List<Task_CLI.Task>();
    }
    else
    {
        tasks = deserializedData;
    }
}
else
{
    tasks = new List<Task_CLI.Task>();
}

TaskManager taskManager = new TaskManager(tasks);

// TUI
if (args.Length == 0)
{
    Application.Init();

    var window = new TaskTrackerWindow("Task tracker (Ctrl+Q for exit)", taskManager);
    Application.Run(window);

    window.Dispose();
    Application.Shutdown();
    return 0;
}
// CLI
else
{
    return CommandLine.Parser.Default.ParseArguments<AddOptions, UpdateOptions, ListOptions, DeleteOptions>(args)
        .MapResult(
            (AddOptions opts) => CLICommander.RunAdd(taskManager, opts),
            (UpdateOptions opts) => CLICommander.RunUpdate(taskManager, opts),
            (ListOptions opts) => CLICommander.RunList(taskManager, opts),
            (DeleteOptions opts) => CLICommander.RunDelete(taskManager, opts),
            errs => 1);
}