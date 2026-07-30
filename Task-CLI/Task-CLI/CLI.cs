using System.Text.Json;
using CommandLine;

namespace Task_CLI
{
    [Verb("add", HelpText = "Add new task")]
    public class AddOptions
    {
        [Option('d', "desctiption", HelpText = "Task description")]
        public string? Description { get; set; }

        [Value(0, Required = true, HelpText = "Task title")]
        public string Title { get; set; }
    }

    [Verb("update", HelpText = "Update task with given ID (usage: update <command> [<command> ...])")]
    public class UpdateOptions
    {
        [Option('d', "description", HelpText = "New description")]
        public string? Description { get; set; }

        [Option('s', "status", HelpText = "New status")]
        public string? Status { get; set; }

        [Option('t', "title", HelpText = "New title")]
        public string? Title { get; set; }

        [Value(0, Required = true, HelpText = "Task ID")]
        public int Id { get; set; }
    }

    [Verb("list", HelpText = "List of tasks")]
    public class ListOptions
    {
        [Option('s', "sort", HelpText = "Sorting task value")]
        public string? Sort { get; set; }

        [Value(0, Required = false, HelpText = "Sorting status")]
        public string? Status { get; set; }

    }

    [Verb("delete", HelpText = "delete task with given ID")]
    public class DeleteOptions
    {
        [Value(0, Required = true, HelpText = "Task ID")]
        public int Id { get; set; }
    }

    public static class CLICommander
    {
        private static void SaveChanges(TaskManager taskManager)
        {
            List<Task_CLI.Task> outputTasks = taskManager.DumpTasks();

            string jsonText = JsonSerializer.Serialize(outputTasks);
            File.WriteAllText("data.json", jsonText);
        }

        public static int RunAdd(TaskManager taskManager, AddOptions opts)
        {
            string title = opts.Title ?? throw new Exception("Unknown command (Title required)");
            taskManager.AddTask(title, opts.Description ?? "No description");
            SaveChanges(taskManager);
            Console.WriteLine($"Successfully added task ({opts.Title})");
            return 0;
        }
        public static int RunUpdate(TaskManager taskManager, UpdateOptions opts)
        {
            var model = new TaskUpdateModel();

            model.title = opts.Title;
            model.description = opts.Description;
            if (opts.Status != null)
            {
                model.status = opts.Status switch
                {
                    "done" => Status.Done,
                    "not-done" => Status.NotDone,
                    "in-progress" => Status.InProgress,
                    _ => Status.NotDone
                };
            }

            taskManager.UpdateTask(opts.Id, model);
            SaveChanges(taskManager);

            return 0;
        }
        public static int RunList(TaskManager taskManager, ListOptions opts)
        {
            string result = "id\ttitle\tdescription\tcreated-at\tupdated-at\tstatus\n";

            Status? sortStatus = null;
            TaskFields sortField = TaskFields.Id;

            if (opts.Sort != null)
            {
                sortField = opts.Sort switch
                {
                    "title" => TaskFields.Title,
                    "description" => TaskFields.Description,
                    "created-at" => TaskFields.CreatedAt,
                    "updated-at" => TaskFields.UpdatedAt,
                    _ => TaskFields.Id
                };
            }

            if (opts.Status != null)
            {
                sortStatus = opts.Status switch
                {
                    "done" => Status.Done,
                    "not-done" => Status.NotDone,
                    "in-progress" => Status.InProgress,
                    _ => Status.NotDone
                };
            }

            result += taskManager.ListTasks(sortField, sortStatus);

            Console.Write(result);

            return 0;
        }
        public static int RunDelete(TaskManager taskManager, DeleteOptions opts)
        {
            taskManager.DeleteTask(opts.Id);
            SaveChanges(taskManager);
            Console.WriteLine($"Successfully deleted task ({opts.Id})");
            return 0;
        }
    }

}
