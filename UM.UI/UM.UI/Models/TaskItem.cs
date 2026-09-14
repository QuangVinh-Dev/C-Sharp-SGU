namespace UM.UI.Models
{
    /// <summary>
    /// Represents a task item in the task management system.
    /// </summary>
    public class TaskItem
    {
        public string Title { get; set; } = string.Empty;
        public string Assignee { get; set; } = string.Empty;
        public string DueDate { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public string Priority { get; set; } = string.Empty;
    }
}
