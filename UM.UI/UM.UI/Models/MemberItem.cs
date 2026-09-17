using System.Collections.Generic;

namespace UM.UI.Models
{
    /// <summary>
    /// Represents a team member with role, permissions, and online status.
    /// </summary>
    public class MemberItem
    {
        public string Name { get; set; } = string.Empty;
        public string AvatarInitial { get; set; } = string.Empty;
        public string AvatarColor { get; set; } = "#888888";
        public string Role { get; set; } = "Member"; // Owner, Admin, Member
        public bool IsOnline { get; set; }

        /// <summary>
        /// Permission flags for this member.
        /// Keys: ManageMembers, CreateTask, EditTask, DeleteTask, AssignTask, ViewTask
        /// </summary>
        public Dictionary<string, bool> Permissions { get; set; } = new()
        {
            { "ManageMembers", false },
            { "CreateTask", false },
            { "EditTask", false },
            { "DeleteTask", false },
            { "AssignTask", false },
            { "ViewTask", true },
        };
    }
}
