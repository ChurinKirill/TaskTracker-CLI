
using System.Text.Json;
using Task_CLI;

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

switch (args[0])
{
    case "add":
        string title = args[1];
        string description = "No description";


        if (args.Length > 2 && (args[2] == "-d" || args[2] == "-description"))
        {
            description = args[3];
        }
        else if (args.Length > 2)
        {
            Console.WriteLine("Unknown command, please try again!");
            break;
        }

        taskManager.AddTask(title, description);
        Console.WriteLine($"Task {title} added successfully");
        
        break;

    case "update":
        var model = new TaskUpdateModel();

        if (args.Length < 2)
        {
            Console.WriteLine("Unknown command, please try again!");
            break;
        }
        bool parseSuccess = int.TryParse(args[1], out int id);

        if (!parseSuccess)
        {
            Console.WriteLine("Unknown command, please try again!");
            break;
        }

        for (int i = 2; i < args.Length; i += 2)
        {
            try
            {

                switch (args[i])
                {
                    case "-t":
                    case "-title":
                        model.title = args[i + 1];
                        break;

                    case "-d":
                    case "-description":
                        model.description = args[i + 1];
                        break;

                    case "-s":
                    case "-status":

                        switch (args[i + 1])
                        {
                            case "done":
                                model.status = Status.Done;
                                break;

                            case "in-progress":
                                model.status = Status.InProgress;
                                break;

                            case "not-done":
                                model.status = Status.NotDone;
                                break;
                        }

                        break;

                    default:
                        Console.WriteLine("Unknown command, please try again!");
                        break;
                }
            }
            catch (Exception e)
            {

                Console.WriteLine(e);
            }

        }

        taskManager.UpdateTask(id, model);
        Console.WriteLine($"Task {id} updated successfully");

        break;

    case "list":

        string result = "id\ttitle\tdescription\tcreated-at\tupdated-at\tstatus\n";

        Status? sortStatus = null;

        if (args.Length == 1)
        {
            result += taskManager.ListTasks(TaskFields.Id, null);
        }
        else if (args[1] == "-s" || args[1] == "-sort")
        {
            TaskFields sortField;
            if (args.Length < 3)
            {
                Console.WriteLine("Unknown command, please try again!");
                break;
            }
            switch (args[2])
            {
                case "title":
                    sortField = TaskFields.Title;
                    break;

                case "description":
                    sortField = TaskFields.Description;
                    break;

                case "created-at":
                    sortField = TaskFields.CreatedAt;
                    break;

                case "updated-at":
                    sortField = TaskFields.UpdatedAt;
                    break;

                default:
                    sortField = TaskFields.Id;
                    break;
            }

            if (args.Length == 4)
            {
                switch (args[3])
                {
                    case "done":
                        sortStatus = Status.Done;
                        break;

                    case "not-done":
                        sortStatus = Status.NotDone;
                        break;

                    case "in-progress":
                        sortStatus = Status.InProgress;
                        break;
                }
            }
            result += taskManager.ListTasks(sortField, sortStatus);
        }
        else
        {
            switch (args[1])
            {
                case "done":
                    sortStatus = Status.Done;
                    break;

                case "not-done":
                    sortStatus = Status.NotDone;
                    break;

                case "in-progress":
                    sortStatus = Status.InProgress;
                    break;

                default:
                    Console.WriteLine("Unknown command, please try again!");
                    break;
            }

            result += taskManager.ListTasks(TaskFields.Id, sortStatus);
        }

        Console.Write(result);

        break;

    default:
        Console.WriteLine("Unknown command, please try again!");
        break;
}


List<Task_CLI.Task> outputTasks = taskManager.DumpTasks();

jsonText = JsonSerializer.Serialize(outputTasks);
File.WriteAllText("data.json", jsonText);

