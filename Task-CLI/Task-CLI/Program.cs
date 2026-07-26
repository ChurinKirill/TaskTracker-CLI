
using System.Data;
using System.Text.Json;
using System.Threading.Tasks;
using Task_CLI;
using Terminal.Gui;



Application.Run<TaskTrackerWindow>();
Application.Shutdown();

public class TaskTrackerWindow : Window
{
    //public static string? Username { get; internal set; }
    
    DataTable dataTable;

    TaskManager taskManager;

    static readonly string[] statusOptions =
        { "Done", "Not done", "In progress" };

    TableView tableView;

    string currentSortColumn = "ID";
    bool sortAscending = true;

    public TaskTrackerWindow()
    {
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
        this.taskManager = new TaskManager(tasks);

        Title = "Task tracker (Ctrl+Q for exit)";

        this.dataTable = new DataTable("Tasks");

        this.dataTable.Columns.Add("ID", typeof(int));
        this.dataTable.Columns.Add("Title", typeof(string));
        this.dataTable.Columns.Add("Description", typeof(string));
        this.dataTable.Columns.Add("Created at", typeof(DateTime));
        this.dataTable.Columns.Add("Updated at", typeof(DateTime));
        this.dataTable.Columns.Add("Status", typeof(string));

        foreach (var task in this.taskManager.DumpTasks())
        {
            this.dataTable.Rows.Add(
                task.id,
                task.title,
                task.description,
                task.createdAt,
                task.updatedAt,
                task.status switch 
                { 
                    Status.Done => "Done",
                    Status.NotDone => "Not done",
                    Status.InProgress => "In progress",
                    _ => "Not done"
                });
        }


        this.tableView = new TableView(this.dataTable)
        {
            X = 1,
            Y = 1,
            Width = Dim.Fill() - 2,
            Height = Dim.Fill() - 2,
            FullRowSelect = false,
            MultiSelect = false,
        };

        tableView.Style.AlwaysShowHeaders = true;
        tableView.Style.ExpandLastColumn = true;
        tableView.Style.ShowHorizontalHeaderOverline = true;
        tableView.Style.ShowVerticalCellLines = true;

        tableView.CellActivated += (args) =>
        {

            // Проверяем, что активирована колонка Title или DEscription (индексы 1 или 2)
            if ((args.Col == 1 || args.Col == 2) && args.Row >= 0 && args.Row < this.dataTable.Rows.Count)
            {
                ShowDataEditDialog(args.Row, args.Col);
            }
            // Проверяем, что активирована колонка "Статус" (индекс 5)
            else if (args.Col == 5 && args.Row >= 0 && args.Row < this.dataTable.Rows.Count)
            {
                ShowStatusSelectionDialog(args.Row);
            }
            else
            {
                // Для других колонок - показываем значение
                var value = this.dataTable.Rows[args.Row][args.Col];
                MessageBox.Query("Значение", $"Текущее значение: {value}", "OK");
            }
        };


        string helpText = "Help";

        var btnHelp = new Button()
        {
            Text = helpText,
            X = Pos.AnchorEnd(1) - (helpText.Length + 4),
            Y = Pos.Bottom(tableView),
            IsDefault = false
        };

        btnHelp.Clicked += () =>
        {
            ShowHelpDialog();
        };

        var btnAdd = new Button()
        {
            Text = "Add",
            X = 0,
            Y = Pos.Bottom(tableView),
            IsDefault = false
        };

        btnAdd.Clicked += () =>
        {
            ShowTaskAddDialog();
        };

        var btnSort = new Button()
        {
            Text = "Sorting",
            X = Pos.Right(btnAdd) + 1,
            Y = Pos.Bottom(tableView),
            IsDefault = false
        };

        btnSort.Clicked += () =>
        {
            ShowSortingSettingsDialog();
        };

        //var btnFilter = new Button()
        //{
        //    Text = "Test filter",
        //    X = Pos.Right(btnSort) + 4,
        //    Y = Pos.Bottom(tableView),
        //    IsDefault = false
        //};

        //btnFilter.Clicked += () =>
        //{
        //    this.dataTable.DefaultView.RowFilter = "Status = 'Done'";
        //    this.tableView.Update();
        //};

        // Добавляем элементы в окно
        Add(tableView, btnHelp, btnAdd, btnSort);
    }

    private void SaveChanges()
    {
        List<Task_CLI.Task> tasks = new List<Task_CLI.Task>();

        //if (this.dataTable.Rows.Count == 0 ) 
        //{
        //    throw new Exception("Nothing to save");
        //}

        this.dataTable.DefaultView.Sort = "ID ASC";

        var table = this.dataTable.DefaultView.ToTable();

        for (int i = 0; i <  table.Rows.Count; i++)
        {
            var row = table.Rows[i];
            tasks.Add(new Task_CLI.Task(
                (int)row["ID"],
                row["Title"] == DBNull.Value ? "No title" : (string)row["Title"],
                row["Description"] == DBNull.Value ? "No description" : (string)row["Description"],
                (DateTime)row["Created at"],
                (DateTime)row["Updated at"],
                row["Status"] switch
                {
                    "Done" => Status.Done,
                    "Not done" => Status.NotDone,
                    "In progress" => Status.InProgress,
                    _ => Status.NotDone
                }
                ));
        }
        
        string jsonText = JsonSerializer.Serialize(tasks);
        File.WriteAllText("data.json", jsonText);
    }

    private void SortTable()
    {
        this.dataTable.DefaultView.Sort = $"{this.currentSortColumn} {(this.sortAscending ? "ASC" : "DESC")}";

        // Создаём новую таблицу и заменяем источник
        var newTable = this.dataTable.DefaultView.ToTable();

        // Заменяем таблицу в TableView
        this.tableView.Table = newTable;

        // Обновляем ссылку (если она используется в других местах)
        this.dataTable = newTable;

        this.tableView.Update();
    }

    private void ShowTaskAddDialog()
    {
        var dialog = new Dialog("Add task", 50, 20);

        int id = -1;
        for (int i = 0; i < this.dataTable.Rows.Count; i++)
        {
            if ((int)this.dataTable.Rows[i]["ID"] > id)
                id = (int)this.dataTable.Rows[i]["ID"];
        }
        ++id;

        var idLabel = new Label()
        {
            Text = $"ID: {id}",
            X = 0,
            Y = 0,
        };
        var titleLabel = new Label()
        {
            Text = "Title:",
            X = 0,
            Y = Pos.Bottom(idLabel) + 1,
        };
        var titleText = new TextField("")
        {
            X = Pos.Right(titleLabel) + 1,
            Y = titleLabel.Y,
            Width = Dim.Fill() - 1,
        };
        var descriptionLabel = new Label()
        {
            Text = "Description:",
            X = 0,
            Y = Pos.Bottom(titleLabel) + 1,
        };
        var descriptionText = new TextField("")
        {
            X = Pos.Right(descriptionLabel) + 1,
            Y = descriptionLabel.Y,
            Width = Dim.Fill() - 1,
        };

        var btnAdd = new Button("Add") { IsDefault = true };
        var btnClose = new Button("Cancel");

        bool isConfirmed = false;

        btnAdd.Clicked += () =>
        {
            isConfirmed = true;
            Application.RequestStop();
        };
        btnClose.Clicked += () =>
        {
            isConfirmed = false;
            Application.RequestStop();
        };

        dialog.Add(idLabel, titleLabel, titleText, descriptionLabel, descriptionText);
        dialog.AddButton(btnAdd);
        dialog.AddButton(btnClose);

        Application.Run(dialog);

        if (isConfirmed)
        {
            this.dataTable.Rows.Add(
                id,
                titleText.Text,
                descriptionText.Text,
                DateTime.Now,
                DateTime.Now,
                "Not done"
                );

            SaveChanges();
            SortTable();
        }
    }

    private void ShowDataEditDialog(int rowIndex, int colimnIndex)
    {
        var currentValue = this.dataTable.Rows[rowIndex][colimnIndex].ToString();
        var dialog = new Dialog("", 60, 20);
        var label = new Label()
        {
            Text = "Edit data:",
            X = 0,
            Y = 0,
            Width = Dim.Fill(),
            Height = 1,
        };

        var editTextView = new TextView()
        {
            Text = currentValue,
            X = 0,
            Y = 3,
            Width = Dim.Fill(),
            Height = Dim.Fill() - 2,
            WordWrap = true,
            ReadOnly = false,
        };

        dialog.Add(label, editTextView);

        var btnAccept = new Button("Accept") { IsDefault = true };
        var btnCancel = new Button("Cancel");

        bool isConfirmed = false;

        btnAccept.Clicked += () =>
        {
            isConfirmed = true;
            Application.RequestStop();
        };
        btnCancel.Clicked += () =>
        {
            isConfirmed = false;
            Application.RequestStop();
        };

        dialog.AddButton(btnAccept);
        dialog.AddButton(btnCancel);

        Application.Run(dialog);

        if (isConfirmed)
        {
            string newValue = editTextView.Text.ToString();
            this.dataTable.Rows[rowIndex].SetField(colimnIndex, newValue);
            this.dataTable.Rows[rowIndex]["Updated at"] = DateTime.Now;

            SaveChanges();
            tableView.Update();

            MessageBox.Query("Updated", "Value updated successfully", "OK");
        }
    }

    private void ShowStatusSelectionDialog(int rowIndex)
    {
        var row = this.dataTable.Rows[rowIndex];
        var currentStatus = row["Status"].ToString();
        var currentIndex = Array.IndexOf(statusOptions, currentStatus);
        if (currentIndex < 0) currentIndex = 0;

        // Создаем диалог с ComboBox
        var dialog = new Dialog("", 50, 10);

        // ComboBox с вариантами
        var comboBox = new ComboBox()
        {
            X = 1,
            Y = 1,
            Width = 45,
            Height = 5,
            Source = new ListWrapper(statusOptions.ToList()),
        };
        comboBox.SelectedItem = currentIndex;

        // Подсказка
        var label = new Label("Select new status: (F4 to open list)")
        {
            X = 1,
            Y = 0
        };

        dialog.Add(label, comboBox);

        bool isConfirmed = false;

        // Кнопки
        var okButton = new Button("Accept") { IsDefault = true };
        var cancelButton = new Button("Cancel");

        okButton.Clicked += () =>
        {
            isConfirmed = true;
            Application.RequestStop();
        };
        cancelButton.Clicked += () =>
        {
            isConfirmed = false;
            Application.RequestStop();
        };

        dialog.AddButton(okButton);
        dialog.AddButton(cancelButton);

        // Запускаем диалог
        Application.Run(dialog);

        if (isConfirmed)
        {
            var selectedValue = statusOptions[comboBox.SelectedItem];
            row["Status"] = selectedValue;
            row["Updated at"] = DateTime.Now;

            SaveChanges();
            tableView.Update();

            // Информируем об изменении
            MessageBox.Query("Updated", $"Status changed to: {selectedValue}", "OK");
        }
    }

    private void ShowSortingSettingsDialog()
    {
        var dialog = new Dialog("Select sorting column", 60, 25);

        var label = new Label()
        {
            Text = $"Current sorting by \"{this.currentSortColumn}\" {(this.sortAscending ? "ASC" : "DESC")}",
            X = 0,
            Y = 0,
            Width = Dim.Fill(),
            Height = 1,
        };

        string newSortColumn = "ID";
        bool newAsc = true;

        var radioGroupColumns = new RadioGroup(new NStack.ustring[]
        {
            "ID",
            "Title",
            "Description",
            "Created at",
            "Updated at",
            "Status",
        })
        {
            X = 0,
            Y = 2,
            Width = 25,
            Height = 6
        };

        var radioGroupMethod = new RadioGroup(new NStack.ustring[]
        {
            "ASC",
            "DESC",
        })
        {
            X = Pos.Right(radioGroupColumns) + 4,
            Y = 2,
            Width = 25,
            Height = 6
        };

        var selectedLabel = new Label()
        {
            Text = "Nothing selected",
            X = 0,
            Y = Pos.Bottom(radioGroupColumns) + 10,
            Width = Dim.Fill(),
            Height = 1,
        };

        radioGroupColumns.SelectedItemChanged += (args) =>
        {
            newSortColumn = radioGroupColumns.RadioLabels[args.SelectedItem].ToString() ?? "ID";

            selectedLabel.Text = $"Selected: \"{newSortColumn}\" {(newAsc ? "ASC" : "DESC")}";
        };
        radioGroupMethod.SelectedItemChanged += (args) =>
        {
            newAsc = args.SelectedItem switch
            {
                0 => true,
                1 => false,
                _ => false
            };

            selectedLabel.Text = $"Selected: \"{newSortColumn}\" {(newAsc ? "ASC" : "DESC")}";
        };

        var btnAccept = new Button("Accept") { IsDefault = true };
        var btnCancel = new Button("Cancel") { };

        bool isConfirmed = false;

        btnAccept.Clicked += () =>
        {
            isConfirmed = true;
            Application.RequestStop();
        };

        btnCancel.Clicked += () =>
        {
            isConfirmed = false;
            Application.RequestStop();
        };

        dialog.Add(label, radioGroupColumns, radioGroupMethod, selectedLabel);

        dialog.AddButton(btnAccept);
        dialog.AddButton(btnCancel);

        Application.Run(dialog);

        if (isConfirmed)
        {
            this.currentSortColumn = newSortColumn;
            this.sortAscending = newAsc;
            SortTable();
        }
    }

    private void ShowHelpDialog()
    {
        var dialog = new Dialog("Help menu", 70, 20);

        List<Label> texts = new List<Label>()
        {
            new Label()
            {
                Text = "Press shift+A or \"Add\" button to add new task",
                Width = Dim.Fill(),
                Height = 2,
            },
            new Label()
            {
                Text = "Press shift+D or \"Delete\" button to delete selected task",
                Width = Dim.Fill(),
                Height = 2,
            },
        };
        
        for (int i = 0; i <  texts.Count; i++)
        {
            texts[i].Y = i * 3;
            dialog.Add(texts[i]);
        }

        var btnClose = new Button()
        {
            Text = "Close",
            IsDefault = true
        };

        btnClose.Clicked += () =>
        {
            Application.RequestStop();
        };

        dialog.AddButton(btnClose);

        Application.Run(dialog);
    }

}

/*

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

*/