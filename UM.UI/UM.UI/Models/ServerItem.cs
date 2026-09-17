namespace UM.UI.Models
{
    /// <summary>
    /// Represents a server/workspace in the server sidebar.
    /// </summary>
    public class ServerItem
    {
        public string Name { get; set; } = string.Empty;
        public string ImagePath { get; set; } = string.Empty;
        /// <summary>
        /// Optional description displayed below the server name.
        /// </summary>
        public string Description { get; set; } = string.Empty;
    }
}
