namespace Task_CLI
{
    public enum TaskFields
    {
        Id,
        Title,
        Description,
        CreatedAt,
        UpdatedAt
    }
    public enum Status
    {
        Done,
        NotDone,
        InProgress
    }


    public struct TaskUpdateModel
    {
        public string? title;
        public string? description;
        public Status? status;
    }

    public class Task
    {
        public int id { get; set; }
        public string title { get; set; }
        public string description { get; set; }
        public DateTime createdAt { get; set; }
        public DateTime updatedAt { get; set; }
        public Status status { get; set; }

        public Task(int id, string title, string description, DateTime createdAt, DateTime updatedAt, Status status)
        {
            this.id = id;
            this.title = title;
            this.description = description;

            this.createdAt = createdAt;
            this.updatedAt = updatedAt;
            this.status = status;
        }

        public override string ToString()
        {
            string statusString;
            switch (this.status)
            {
                case Status.NotDone:
                    statusString = "not-done";
                    break;
                case Status.InProgress:
                    statusString = "in-progress";
                    break;
                case Status.Done:
                    statusString = "done";
                    break;
                default:
                    statusString = "not-done";
                    break;
            }
            string result = $"{this.id}\t{this.title}\t{this.description}\t{this.createdAt}\t{this.updatedAt}\t{statusString}";
            return result;
        }
    }
}
