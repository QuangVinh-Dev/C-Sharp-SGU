namespace UM.UI.Models
{
    /// <summary>
    /// Represents a chat message in the messaging panel.
    /// </summary>
    public class ChatMessage
    {
        public string Sender { get; set; } = string.Empty;
        public string AvatarInitial { get; set; } = string.Empty;
        public string AvatarColor { get; set; } = "#888888";
        public string Content { get; set; } = string.Empty;
        public string Timestamp { get; set; } = string.Empty;
        /// <summary>
        /// Path to an image file for image messages. Empty if text-only.
        /// </summary>
        public string ImagePath { get; set; } = string.Empty;
        /// <summary>
        /// Whether this message was sent by the current user (for bubble alignment).
        /// </summary>
        public bool IsCurrentUser { get; set; }
    }
}
