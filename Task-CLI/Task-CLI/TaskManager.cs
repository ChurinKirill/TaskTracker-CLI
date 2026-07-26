using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Task_CLI
{
    internal class TaskManager
    {

        List<Task> tasks;

        public TaskManager(List<Task> tasks)
        {
            this.tasks = tasks.OrderBy(x => x.id).ToList();
        }

        public void AddTask(string title, string description)
        {
            int newId = 1;
            if (this.tasks.Count > 0)
            {
                newId = tasks.Last().id + 1;
            }
            
            //this.tasks.Add(new Task(newId, title, description));
        }

        public void UpdateTask(int id, TaskUpdateModel updateModel)
        {
            Task task = this.tasks.Find(x => x.id == id) ?? throw new Exception($"Task with given id {id} does not exist");

            if (updateModel.title != null)
            {
                task.title = updateModel.title;
            }
            if (updateModel.description  != null)
            {
                task.description = updateModel.description;
            }
            if (updateModel.status != null)
            {
                task.status = updateModel.status.Value;
            }
        }


        public string ListTasks(TaskFields sortField, Status? status)
        {
            string result = "";

            List<Task> taskList;

            switch (sortField)
            {                    
                case TaskFields.Title:
                    taskList = this.tasks.OrderBy(x => x.title).ToList();
                    break;

                case TaskFields.Description:
                    taskList = this.tasks.OrderBy(x => x.description).ToList();
                    break;

                case TaskFields.CreatedAt:
                    taskList = this.tasks.OrderBy(x => x.description).ToList();
                    break;

                case TaskFields.UpdatedAt:
                    taskList = this.tasks.OrderBy(x => x.description).ToList();
                    break;

                default:
                    taskList = this.tasks.OrderBy(x => x.id).ToList();
                    break;
            }

            if (status != null)
            {
                foreach (Task task in taskList)
                {
                    if (task.status == status)
                    {
                        result += task.ToString() + '\n';
                    }
                }
            }
            else
            {
                foreach (Task task in this.tasks)
                {
                    result += task.ToString() + '\n';
                }
            }

            return result;
        }

        public List<Task> DumpTasks()
        {
            return this.tasks;
        }

    }
}
