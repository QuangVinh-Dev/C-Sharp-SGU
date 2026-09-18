using Microsoft.UI;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Animation;
using Microsoft.UI.Xaml.Media.Imaging;
using Microsoft.UI.Xaml.Shapes;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UM.UI.Models;

namespace UM.UI.Views
{
    public sealed partial class TaskPage : Page
    {
        private List<TaskItem> _allTasks = new();
        private List<ServerItem> _servers = new();
        private List<MemberItem> _members = new();
        private List<ChatMessage> _chatMessages = new();
        private string _currentChannel = "todo";
        private int _selectedServerIndex = 0;
        private readonly List<Border> _serverBorders = new();
        private MemberItem? _selectedMember;
        private bool _isCurrentUserOwner = true; // Mock: current user is Owner

        public TaskPage()
        {
            this.InitializeComponent();
            LoadMockData();
            LoadMembers();
            LoadMockChat();
            LoadServers();
            BuildMemberList();
            PopulateAssignTaskCombo();
            ShowChannel("todo");
        }

        // ================================================================
        // MOCK DATA
        // ================================================================

        /// <summary>
        /// Loads mock task data for demo purposes.
        /// </summary>
        private void LoadMockData()
        {
            _allTasks = new List<TaskItem>
            {
                // Todo
                new TaskItem
                {
                    Title = "Thiết kế giao diện đăng nhập",
                    Assignee = "Vinh",
                    DueDate = "20/09/2026",
                    Status = "Todo",
                    Priority = "Cao"
                },
                new TaskItem
                {
                    Title = "Tạo database",
                    Assignee = "Hùng",
                    DueDate = "22/09/2026",
                    Status = "Todo",
                    Priority = "Cao"
                },
                new TaskItem
                {
                    Title = "Viết API đăng nhập",
                    Assignee = "Linh",
                    DueDate = "25/09/2026",
                    Status = "Todo",
                    Priority = "Trung bình"
                },

                // Doing
                new TaskItem
                {
                    Title = "Xây dựng TaskPage",
                    Assignee = "Vinh",
                    DueDate = "18/09/2026",
                    Status = "Doing",
                    Priority = "Cao"
                },
                new TaskItem
                {
                    Title = "Thiết kế database",
                    Assignee = "Tuấn",
                    DueDate = "19/09/2026",
                    Status = "Doing",
                    Priority = "Trung bình"
                },

                // Done
                new TaskItem
                {
                    Title = "Tạo project",
                    Assignee = "Vinh",
                    DueDate = "10/09/2026",
                    Status = "Done",
                    Priority = "Cao"
                },
                new TaskItem
                {
                    Title = "Thiết lập Git",
                    Assignee = "Hùng",
                    DueDate = "10/09/2026",
                    Status = "Done",
                    Priority = "Thấp"
                },
            };

            // Update channel badges
            UpdateBadges();
        }

        /// <summary>
        /// Loads mock member data.
        /// </summary>
        private void LoadMembers()
        {
            _members = new List<MemberItem>
            {
                new MemberItem
                {
                    Name = "Vinh", AvatarInitial = "V", AvatarColor = "#4CAF50",
                    Role = "Owner", IsOnline = true,
                    Permissions = new Dictionary<string, bool>
                    {
                        {"ManageMembers", true}, {"CreateTask", true}, {"EditTask", true},
                        {"DeleteTask", true}, {"AssignTask", true}, {"ViewTask", true}
                    }
                },
                new MemberItem
                {
                    Name = "Hùng", AvatarInitial = "H", AvatarColor = "#2196F3",
                    Role = "Admin", IsOnline = true,
                    Permissions = new Dictionary<string, bool>
                    {
                        {"ManageMembers", false}, {"CreateTask", true}, {"EditTask", true},
                        {"DeleteTask", false}, {"AssignTask", true}, {"ViewTask", true}
                    }
                },
                new MemberItem
                {
                    Name = "Linh", AvatarInitial = "L", AvatarColor = "#FF9800",
                    Role = "Member", IsOnline = false,
                    Permissions = new Dictionary<string, bool>
                    {
                        {"ManageMembers", false}, {"CreateTask", false}, {"EditTask", false},
                        {"DeleteTask", false}, {"AssignTask", false}, {"ViewTask", true}
                    }
                },
                new MemberItem
                {
                    Name = "Tuấn", AvatarInitial = "T", AvatarColor = "#9C27B0",
                    Role = "Member", IsOnline = false,
                    Permissions = new Dictionary<string, bool>
                    {
                        {"ManageMembers", false}, {"CreateTask", false}, {"EditTask", false},
                        {"DeleteTask", false}, {"AssignTask", false}, {"ViewTask", true}
                    }
                },
            };
        }

        /// <summary>
        /// Loads mock chat messages.
        /// </summary>
        private void LoadMockChat()
        {
            _chatMessages = new List<ChatMessage>
            {
                new ChatMessage
                {
                    Sender = "Vinh", AvatarInitial = "V", AvatarColor = "#4CAF50",
                    Content = "Đã hoàn thành phần giao diện.",
                    Timestamp = "09:15"
                },
                new ChatMessage
                {
                    Sender = "Hùng", AvatarInitial = "H", AvatarColor = "#2196F3",
                    Content = "Tôi đang làm database.",
                    Timestamp = "09:20"
                },
                new ChatMessage
                {
                    Sender = "Linh", AvatarInitial = "L", AvatarColor = "#FF9800",
                    Content = "API đăng nhập gần xong.",
                    Timestamp = "09:25"
                },
                new ChatMessage
                {
                    Sender = "Tuấn", AvatarInitial = "T", AvatarColor = "#9C27B0",
                    Content = "Mình sẽ review code chiều nay.",
                    Timestamp = "09:30"
                },
                new ChatMessage
                {
                    Sender = "Vinh", AvatarInitial = "V", AvatarColor = "#4CAF50",
                    Content = "OK, team cố gắng hoàn thành trước deadline nhé!",
                    Timestamp = "09:35"
                },
            };
        }

        /// <summary>
        /// Updates channel badge counts.
        /// </summary>
        private void UpdateBadges()
        {
            TodoBadge.Text = _allTasks.Count(t => t.Status == "Todo").ToString();
            DoingBadge.Text = _allTasks.Count(t => t.Status == "Doing").ToString();
            DoneBadge.Text = _allTasks.Count(t => t.Status == "Done").ToString();
        }

        // ================================================================
        // SERVER LIST
        // ================================================================

        /// <summary>
        /// Loads mock server data and builds the server image list.
        /// Images are loaded from the Assets/Servers folder relative to the app.
        /// </summary>
        private void LoadServers()
        {
            _servers = new List<ServerItem>
            {
                new ServerItem { Name = "Dự án SGU", ImagePath = "Assets/Servers/server1.png" },
                new ServerItem { Name = "Nhóm Học Tập", ImagePath = "Assets/Servers/server2.png" },
                new ServerItem { Name = "Câu Lạc Bộ IT", ImagePath = "Assets/Servers/server3.png" },
            };

            _serverBorders.Clear();
            var appDir = AppContext.BaseDirectory;

            for (int i = 0; i < _servers.Count; i++)
            {
                var server = _servers[i];
                var index = i;

                // Resolve image path relative to app directory
                var fullPath = System.IO.Path.Combine(appDir, server.ImagePath);

                // Create circular image avatar
                var ellipse = new Ellipse
                {
                    Width = 48,
                    Height = 48,
                };

                if (System.IO.File.Exists(fullPath))
                {
                    var bitmap = new BitmapImage(new Uri(fullPath));
                    ellipse.Fill = new ImageBrush
                    {
                        ImageSource = bitmap,
                        Stretch = Stretch.UniformToFill
                    };
                }
                else
                {
                    // Fallback: gray circle with initial
                    ellipse.Fill = new SolidColorBrush(ColorHelper.FromArgb(255, 200, 200, 200));
                }

                // Wrap in a Border for active indicator
                var border = new Border
                {
                    Width = 54,
                    Height = 54,
                    CornerRadius = new CornerRadius(27),
                    BorderThickness = new Thickness(2.5),
                    BorderBrush = new SolidColorBrush(Colors.Transparent),
                    Background = new SolidColorBrush(Colors.Transparent),
                    Child = ellipse,
                    Padding = new Thickness(0),
                };

                ToolTipService.SetToolTip(border, server.Name);

                border.PointerPressed += (s, e) =>
                {
                    _selectedServerIndex = index;
                    UpdateServerHighlight();
                };

                _serverBorders.Add(border);
                ServerListPanel.Children.Add(border);
            }

            // Add "+" button at the bottom
            var addBorder = new Border
            {
                Width = 48,
                Height = 48,
                CornerRadius = new CornerRadius(24),
                Background = new SolidColorBrush(ColorHelper.FromArgb(255, 250, 250, 250)),
                BorderBrush = new SolidColorBrush(ColorHelper.FromArgb(255, 208, 208, 208)),
                BorderThickness = new Thickness(1.5),
                Child = new TextBlock
                {
                    Text = "+",
                    Foreground = new SolidColorBrush(ColorHelper.FromArgb(255, 136, 136, 136)),
                    FontSize = 22,
                    HorizontalAlignment = HorizontalAlignment.Center,
                    VerticalAlignment = VerticalAlignment.Center,
                }
            };
            ToolTipService.SetToolTip(addBorder, "Thêm server");
            ServerListPanel.Children.Add(addBorder);

            // Highlight first server
            UpdateServerHighlight();
        }

        /// <summary>
        /// Updates the visual indicator for the active server.
        /// </summary>
        private void UpdateServerHighlight()
        {
            var activeBrush = new SolidColorBrush(ColorHelper.FromArgb(255, 60, 60, 60)); // Dark gray
            var inactiveBrush = new SolidColorBrush(Colors.Transparent);

            for (int i = 0; i < _serverBorders.Count; i++)
            {
                _serverBorders[i].BorderBrush = (i == _selectedServerIndex) ? activeBrush : inactiveBrush;
            }
        }

        // ================================================================
        // MEMBER LIST IN SIDEBAR
        // ================================================================

        /// <summary>
        /// Builds the member list in the sidebar from member data.
        /// Each member is clickable to open the user management panel.
        /// </summary>
        private void BuildMemberList()
        {
            MemberListPanel.Children.Clear();

            foreach (var member in _members)
            {
                var memberBorder = new Border
                {
                    Background = new SolidColorBrush(Colors.Transparent),
                    CornerRadius = new CornerRadius(6),
                    Padding = new Thickness(10, 6, 10, 6),
                    Margin = new Thickness(0, 1, 0, 1),
                };

                var stack = new StackPanel
                {
                    Orientation = Orientation.Horizontal,
                    Spacing = 10
                };

                // Avatar
                var avatarBorder = new Border
                {
                    Width = 28,
                    Height = 28,
                    CornerRadius = new CornerRadius(14),
                    Background = new SolidColorBrush(ParseColor(member.AvatarColor)),
                };
                var avatarText = new TextBlock
                {
                    Text = member.AvatarInitial,
                    Foreground = new SolidColorBrush(Colors.White),
                    FontSize = 12,
                    FontWeight = Microsoft.UI.Text.FontWeights.SemiBold,
                    HorizontalAlignment = HorizontalAlignment.Center,
                    VerticalAlignment = VerticalAlignment.Center,
                };
                avatarBorder.Child = avatarText;

                // Online indicator
                var nameStack = new StackPanel { VerticalAlignment = VerticalAlignment.Center };
                var nameBlock = new TextBlock
                {
                    Text = member.Name,
                    Foreground = new SolidColorBrush(ColorHelper.FromArgb(255, 51, 51, 51)),
                    FontSize = 13,
                };
                nameStack.Children.Add(nameBlock);

                // Add online dot
                if (member.IsOnline)
                {
                    var onlineDot = new Border
                    {
                        Width = 8,
                        Height = 8,
                        CornerRadius = new CornerRadius(4),
                        Background = new SolidColorBrush(ColorHelper.FromArgb(255, 76, 175, 80)),
                        HorizontalAlignment = HorizontalAlignment.Left,
                        Margin = new Thickness(0, 2, 0, 0),
                    };
                    nameStack.Children.Add(onlineDot);
                }

                stack.Children.Add(avatarBorder);
                stack.Children.Add(nameStack);
                memberBorder.Child = stack;

                // Click handler to open user management panel
                var m = member;
                memberBorder.PointerPressed += (s, e) => OpenUserPanel(m);

                // Hover effect
                memberBorder.PointerEntered += (s, e) =>
                    memberBorder.Background = new SolidColorBrush(ColorHelper.FromArgb(255, 240, 240, 240));
                memberBorder.PointerExited += (s, e) =>
                    memberBorder.Background = new SolidColorBrush(Colors.Transparent);

                MemberListPanel.Children.Add(memberBorder);
            }
        }

        // ================================================================
        // CHANNEL NAVIGATION
        // ================================================================

        /// <summary>
        /// Shows a specific channel's content in the main area.
        /// </summary>
        private void ShowChannel(string channel)
        {
            _currentChannel = channel;

            // Hide all content panels
            TaskContentScrollViewer.Visibility = Visibility.Collapsed;
            ChatContentPanel.Visibility = Visibility.Collapsed;
            MainDashboardPanel.Visibility = Visibility.Collapsed;
            AssignTaskPanel.Visibility = Visibility.Collapsed;

            switch (channel)
            {
                case "todo":
                    CurrentChannelName.Text = "todo";
                    CurrentChannelDescription.Text = "Danh sách công việc cần hoàn thành";
                    TaskContentScrollViewer.Visibility = Visibility.Visible;
                    TodoSection.Visibility = Visibility.Visible;
                    DoingSection.Visibility = Visibility.Visible;
                    DoneSection.Visibility = Visibility.Visible;
                    RefreshTaskLists();
                    break;
                case "doing":
                    CurrentChannelName.Text = "doing";
                    CurrentChannelDescription.Text = "Công việc đang thực hiện";
                    TaskContentScrollViewer.Visibility = Visibility.Visible;
                    TodoSection.Visibility = Visibility.Collapsed;
                    DoingSection.Visibility = Visibility.Visible;
                    DoneSection.Visibility = Visibility.Collapsed;
                    RefreshTaskLists();
                    break;
                case "done":
                    CurrentChannelName.Text = "done";
                    CurrentChannelDescription.Text = "Công việc đã hoàn thành";
                    TaskContentScrollViewer.Visibility = Visibility.Visible;
                    TodoSection.Visibility = Visibility.Collapsed;
                    DoingSection.Visibility = Visibility.Collapsed;
                    DoneSection.Visibility = Visibility.Visible;
                    RefreshTaskLists();
                    break;
                case "general":
                    CurrentChannelName.Text = "general";
                    CurrentChannelDescription.Text = "Trò chuyện chung của team";
                    ChatContentPanel.Visibility = Visibility.Visible;
                    ChatRepeater.ItemsSource = _chatMessages.ToList();
                    break;
                case "thông-báo":
                    CurrentChannelName.Text = "thông-báo";
                    CurrentChannelDescription.Text = "Thông báo quan trọng";
                    ChatContentPanel.Visibility = Visibility.Visible;
                    ChatRepeater.ItemsSource = _chatMessages.Take(2).ToList();
                    break;
                case "main":
                    CurrentChannelName.Text = "Tổng quan";
                    CurrentChannelDescription.Text = "Bảng điều khiển dự án";
                    MainDashboardPanel.Visibility = Visibility.Visible;
                    BuildDashboard();
                    break;
                case "assign":
                    CurrentChannelName.Text = "Giao công việc";
                    CurrentChannelDescription.Text = "Tạo và giao task cho thành viên";
                    AssignTaskPanel.Visibility = Visibility.Visible;
                    AssignTaskListRepeater.ItemsSource = _allTasks.ToList();
                    break;
            }

            // Update channel highlight
            UpdateChannelHighlight(channel);
        }

        /// <summary>
        /// Refreshes task repeater data.
        /// </summary>
        private void RefreshTaskLists()
        {
            var todoTasks = _allTasks.Where(t => t.Status == "Todo").ToList();
            var doingTasks = _allTasks.Where(t => t.Status == "Doing").ToList();
            var doneTasks = _allTasks.Where(t => t.Status == "Done").ToList();

            TodoCountText.Text = $"{todoTasks.Count} công việc";
            DoingCountText.Text = $"{doingTasks.Count} công việc";
            DoneCountText.Text = $"{doneTasks.Count} công việc";

            TodoRepeater.ItemsSource = todoTasks;
            DoingRepeater.ItemsSource = doingTasks;
            DoneRepeater.ItemsSource = doneTasks;
        }

        /// <summary>
        /// Updates the visual highlight of the active channel in the sidebar.
        /// </summary>
        private void UpdateChannelHighlight(string activeChannel)
        {
            var transparent = new SolidColorBrush(Colors.Transparent);
            var highlightBrush = new SolidColorBrush(
                ColorHelper.FromArgb(255, 232, 240, 254)); // #E8F0FE

            // Reset all
            ChannelTodo.Background = transparent;
            ChannelDoing.Background = transparent;
            ChannelDone.Background = transparent;
            ChannelGeneral.Background = transparent;
            ChannelThongBao.Background = transparent;
            ChannelMain.Background = transparent;
            ChannelAssignTask.Background = transparent;

            // Highlight active
            switch (activeChannel)
            {
                case "todo": ChannelTodo.Background = highlightBrush; break;
                case "doing": ChannelDoing.Background = highlightBrush; break;
                case "done": ChannelDone.Background = highlightBrush; break;
                case "general": ChannelGeneral.Background = highlightBrush; break;
                case "thông-báo": ChannelThongBao.Background = highlightBrush; break;
                case "main": ChannelMain.Background = highlightBrush; break;
                case "assign": ChannelAssignTask.Background = highlightBrush; break;
            }
        }

        /// <summary>
        /// Handles channel click in the sidebar.
        /// </summary>
        private void Channel_PointerPressed(object sender, PointerRoutedEventArgs e)
        {
            if (sender is Border border)
            {
                if (border == ChannelTodo) ShowChannel("todo");
                else if (border == ChannelDoing) ShowChannel("doing");
                else if (border == ChannelDone) ShowChannel("done");
                else if (border == ChannelGeneral) ShowChannel("general");
                else if (border == ChannelThongBao) ShowChannel("thông-báo");
                else if (border == ChannelMain) ShowChannel("main");
                else if (border == ChannelAssignTask) ShowChannel("assign");
            }
        }

        // ================================================================
        // SEARCH
        // ================================================================

        /// <summary>
        /// Filters displayed tasks as the user types in the search box.
        /// </summary>
        private void TaskSearchBox_TextChanged(AutoSuggestBox sender, AutoSuggestBoxTextChangedEventArgs args)
        {
            if (args.Reason == AutoSuggestionBoxTextChangeReason.UserInput)
            {
                FilterTasks(sender.Text);
            }
        }

        /// <summary>
        /// Handles search query submission (Enter key).
        /// </summary>
        private void TaskSearchBox_QuerySubmitted(AutoSuggestBox sender, AutoSuggestBoxQuerySubmittedEventArgs args)
        {
            FilterTasks(args.QueryText);
        }

        /// <summary>
        /// Filters the task lists based on search query, respecting current channel.
        /// </summary>
        private void FilterTasks(string query)
        {
            var filtered = _allTasks.AsEnumerable();

            if (!string.IsNullOrWhiteSpace(query))
            {
                var q = query.Trim().ToLowerInvariant();
                filtered = filtered.Where(t =>
                    t.Title.ToLowerInvariant().Contains(q) ||
                    t.Assignee.ToLowerInvariant().Contains(q));
            }

            var todoTasks = filtered.Where(t => t.Status == "Todo").ToList();
            var doingTasks = filtered.Where(t => t.Status == "Doing").ToList();
            var doneTasks = filtered.Where(t => t.Status == "Done").ToList();

            TodoCountText.Text = $"{todoTasks.Count} công việc";
            DoingCountText.Text = $"{doingTasks.Count} công việc";
            DoneCountText.Text = $"{doneTasks.Count} công việc";

            TodoRepeater.ItemsSource = todoTasks;
            DoingRepeater.ItemsSource = doingTasks;
            DoneRepeater.ItemsSource = doneTasks;
        }

        // ================================================================
        // USER MANAGEMENT PANEL
        // ================================================================

        /// <summary>
        /// Opens the user management panel for a specific member with slide-in animation.
        /// </summary>
        private void OpenUserPanel(MemberItem member)
        {
            _selectedMember = member;

            // Set avatar
            UserPanelAvatar.Background = new SolidColorBrush(ParseColor(member.AvatarColor));
            UserPanelAvatarText.Text = member.AvatarInitial;
            UserPanelName.Text = member.Name;

            // Role badge
            UserPanelRoleText.Text = member.Role;

            // Online status
            if (member.IsOnline)
            {
                UserPanelOnlineBadge.Background = new SolidColorBrush(ColorHelper.FromArgb(255, 232, 245, 233));
                UserPanelOnlineText.Text = "Online";
                UserPanelOnlineText.Foreground = new SolidColorBrush(ColorHelper.FromArgb(255, 46, 125, 50));
            }
            else
            {
                UserPanelOnlineBadge.Background = new SolidColorBrush(ColorHelper.FromArgb(255, 245, 245, 245));
                UserPanelOnlineText.Text = "Offline";
                UserPanelOnlineText.Foreground = new SolidColorBrush(ColorHelper.FromArgb(255, 170, 170, 170));
            }

            // Permissions
            _isUpdatingPermissions = true;
            PermManageMembers.IsOn = member.Permissions.GetValueOrDefault("ManageMembers", false);
            PermCreateTask.IsOn = member.Permissions.GetValueOrDefault("CreateTask", false);
            PermEditTask.IsOn = member.Permissions.GetValueOrDefault("EditTask", false);
            PermDeleteTask.IsOn = member.Permissions.GetValueOrDefault("DeleteTask", false);
            PermAssignTask.IsOn = member.Permissions.GetValueOrDefault("AssignTask", false);
            PermViewTask.IsOn = member.Permissions.GetValueOrDefault("ViewTask", true);
            _isUpdatingPermissions = false;

            // Owner-only controls
            if (_isCurrentUserOwner)
            {
                OwnerRoleSection.Visibility = Visibility.Visible;
                OwnerAssignSection.Visibility = Visibility.Visible;

                // Enable toggle switches
                PermManageMembers.IsEnabled = true;
                PermCreateTask.IsEnabled = true;
                PermEditTask.IsEnabled = true;
                PermDeleteTask.IsEnabled = true;
                PermAssignTask.IsEnabled = true;
                PermViewTask.IsEnabled = true;

                // Set role combo
                _isUpdatingPermissions = true;
                UserPanelRoleCombo.SelectedIndex = member.Role switch
                {
                    "Owner" => 0,
                    "Admin" => 1,
                    _ => 2
                };
                _isUpdatingPermissions = false;

                // Populate task combo for quick assign
                UserPanelTaskCombo.Items.Clear();
                foreach (var task in _allTasks)
                {
                    UserPanelTaskCombo.Items.Add($"{task.Title} ({task.Status})");
                }
            }
            else
            {
                OwnerRoleSection.Visibility = Visibility.Collapsed;
                OwnerAssignSection.Visibility = Visibility.Collapsed;

                PermManageMembers.IsEnabled = false;
                PermCreateTask.IsEnabled = false;
                PermEditTask.IsEnabled = false;
                PermDeleteTask.IsEnabled = false;
                PermAssignTask.IsEnabled = false;
                PermViewTask.IsEnabled = false;
            }

            // Show panel with animation
            UserPanelOverlay.Visibility = Visibility.Visible;
            UserManagementPanel.Visibility = Visibility.Visible;
            AnimatePanel(true);
        }

        /// <summary>
        /// Closes the user management panel with slide-out animation.
        /// </summary>
        private void CloseUserPanel()
        {
            AnimatePanel(false);
        }

        private void CloseUserPanel_Click(object sender, RoutedEventArgs e)
        {
            CloseUserPanel();
        }

        private void UserPanelOverlay_PointerPressed(object sender, PointerRoutedEventArgs e)
        {
            CloseUserPanel();
        }

        // ================================================================
        // PANEL ANIMATION
        // ================================================================

        /// <summary>
        /// Animates the user management panel sliding in/out with fade.
        /// </summary>
        private void AnimatePanel(bool open)
        {
            var storyboard = new Storyboard();

            // Slide animation
            var slideAnim = new DoubleAnimation
            {
                From = open ? 400 : 0,
                To = open ? 0 : 400,
                Duration = new Duration(TimeSpan.FromMilliseconds(200)),
                EasingFunction = new CubicEase { EasingMode = open ? EasingMode.EaseOut : EasingMode.EaseIn }
            };
            Storyboard.SetTarget(slideAnim, UserPanelTranslate);
            Storyboard.SetTargetProperty(slideAnim, "X");

            // Overlay fade animation
            var fadeAnim = new DoubleAnimation
            {
                From = open ? 0.0 : 1.0,
                To = open ? 1.0 : 0.0,
                Duration = new Duration(TimeSpan.FromMilliseconds(200)),
            };
            Storyboard.SetTarget(fadeAnim, UserPanelOverlay);
            Storyboard.SetTargetProperty(fadeAnim, "Opacity");

            storyboard.Children.Add(slideAnim);
            storyboard.Children.Add(fadeAnim);

            if (!open)
            {
                storyboard.Completed += (s, e) =>
                {
                    UserPanelOverlay.Visibility = Visibility.Collapsed;
                    UserManagementPanel.Visibility = Visibility.Collapsed;
                };
            }

            storyboard.Begin();
        }

        // ================================================================
        // PERMISSIONS
        // ================================================================

        private bool _isUpdatingPermissions = false;

        /// <summary>
        /// Handles permission toggle changes - updates mock data.
        /// </summary>
        private void Permission_Toggled(object sender, RoutedEventArgs e)
        {
            if (_isUpdatingPermissions || _selectedMember == null) return;

            if (sender is ToggleSwitch toggle)
            {
                if (toggle == PermManageMembers)
                    _selectedMember.Permissions["ManageMembers"] = toggle.IsOn;
                else if (toggle == PermCreateTask)
                    _selectedMember.Permissions["CreateTask"] = toggle.IsOn;
                else if (toggle == PermEditTask)
                    _selectedMember.Permissions["EditTask"] = toggle.IsOn;
                else if (toggle == PermDeleteTask)
                    _selectedMember.Permissions["DeleteTask"] = toggle.IsOn;
                else if (toggle == PermAssignTask)
                    _selectedMember.Permissions["AssignTask"] = toggle.IsOn;
                else if (toggle == PermViewTask)
                    _selectedMember.Permissions["ViewTask"] = toggle.IsOn;
            }
        }

        /// <summary>
        /// Handles role combo selection change.
        /// </summary>
        private void UserPanelRoleCombo_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (_isUpdatingPermissions || _selectedMember == null) return;

            if (UserPanelRoleCombo.SelectedItem is ComboBoxItem item)
            {
                var newRole = item.Content?.ToString() ?? "Member";
                _selectedMember.Role = newRole;
                UserPanelRoleText.Text = newRole;

                // Auto-set permissions based on role
                _isUpdatingPermissions = true;
                switch (newRole)
                {
                    case "Owner":
                        SetAllPermissions(true);
                        break;
                    case "Admin":
                        PermManageMembers.IsOn = false;
                        PermCreateTask.IsOn = true;
                        PermEditTask.IsOn = true;
                        PermDeleteTask.IsOn = false;
                        PermAssignTask.IsOn = true;
                        PermViewTask.IsOn = true;
                        break;
                    case "Member":
                        SetAllPermissions(false);
                        PermViewTask.IsOn = true;
                        break;
                }
                _isUpdatingPermissions = false;

                // Update member data
                _selectedMember.Permissions["ManageMembers"] = PermManageMembers.IsOn;
                _selectedMember.Permissions["CreateTask"] = PermCreateTask.IsOn;
                _selectedMember.Permissions["EditTask"] = PermEditTask.IsOn;
                _selectedMember.Permissions["DeleteTask"] = PermDeleteTask.IsOn;
                _selectedMember.Permissions["AssignTask"] = PermAssignTask.IsOn;
                _selectedMember.Permissions["ViewTask"] = PermViewTask.IsOn;
            }
        }

        private void SetAllPermissions(bool value)
        {
            PermManageMembers.IsOn = value;
            PermCreateTask.IsOn = value;
            PermEditTask.IsOn = value;
            PermDeleteTask.IsOn = value;
            PermAssignTask.IsOn = value;
            PermViewTask.IsOn = value;
        }

        /// <summary>
        /// Quick-assigns a task from the user panel.
        /// </summary>
        private void UserPanelAssignTask_Click(object sender, RoutedEventArgs e)
        {
            if (_selectedMember == null || UserPanelTaskCombo.SelectedIndex < 0) return;

            var taskIndex = UserPanelTaskCombo.SelectedIndex;
            if (taskIndex < _allTasks.Count)
            {
                _allTasks[taskIndex].Assignee = _selectedMember.Name;
                UpdateBadges();

                // Refresh current view
                if (_currentChannel == "todo" || _currentChannel == "doing" || _currentChannel == "done")
                    RefreshTaskLists();
                if (_currentChannel == "assign")
                    AssignTaskListRepeater.ItemsSource = _allTasks.ToList();
            }
        }

        // ================================================================
        // CHAT
        // ================================================================

        /// <summary>
        /// Sends a chat message from the input box.
        /// </summary>
        private void SendMessage_Click(object sender, RoutedEventArgs e)
        {
            SendChatMessage();
        }

        private void ChatInputBox_KeyDown(object sender, KeyRoutedEventArgs e)
        {
            if (e.Key == Windows.System.VirtualKey.Enter)
            {
                SendChatMessage();
                e.Handled = true;
            }
        }

        private void SendChatMessage()
        {
            var text = ChatInputBox.Text?.Trim();
            if (string.IsNullOrEmpty(text)) return;

            var newMsg = new ChatMessage
            {
                Sender = "Vinh", // Mock current user
                AvatarInitial = "V",
                AvatarColor = "#4CAF50",
                Content = text,
                Timestamp = DateTime.Now.ToString("HH:mm"),
            };

            _chatMessages.Add(newMsg);
            ChatRepeater.ItemsSource = _chatMessages.ToList();
            ChatInputBox.Text = string.Empty;
        }

        // ================================================================
        // MAIN DASHBOARD
        // ================================================================

        /// <summary>
        /// Builds the main dashboard content.
        /// </summary>
        private void BuildDashboard()
        {
            // Stats cards
            DashboardStatsPanel.Children.Clear();
            var totalTasks = _allTasks.Count;
            var todoCount = _allTasks.Count(t => t.Status == "Todo");
            var doingCount = _allTasks.Count(t => t.Status == "Doing");
            var doneCount = _allTasks.Count(t => t.Status == "Done");

            DashboardStatsPanel.Children.Add(CreateStatCard("Tổng task", totalTasks.ToString(), "#1A73E8", "#E8F0FE"));
            DashboardStatsPanel.Children.Add(CreateStatCard("TODO", todoCount.ToString(), "#E65100", "#FFF3E0"));
            DashboardStatsPanel.Children.Add(CreateStatCard("DOING", doingCount.ToString(), "#1565C0", "#E3F2FD"));
            DashboardStatsPanel.Children.Add(CreateStatCard("DONE", doneCount.ToString(), "#2E7D32", "#E8F5E9"));

            // Members panel
            DashboardMembersPanel.Children.Clear();
            foreach (var member in _members)
            {
                var row = new StackPanel { Orientation = Orientation.Horizontal, Spacing = 10 };

                var avatar = new Border
                {
                    Width = 28, Height = 28, CornerRadius = new CornerRadius(14),
                    Background = new SolidColorBrush(ParseColor(member.AvatarColor)),
                };
                avatar.Child = new TextBlock
                {
                    Text = member.AvatarInitial, Foreground = new SolidColorBrush(Colors.White),
                    FontSize = 11, FontWeight = Microsoft.UI.Text.FontWeights.SemiBold,
                    HorizontalAlignment = HorizontalAlignment.Center,
                    VerticalAlignment = VerticalAlignment.Center,
                };

                var info = new StackPanel { VerticalAlignment = VerticalAlignment.Center, Spacing = 1 };
                info.Children.Add(new TextBlock
                {
                    Text = member.Name, FontSize = 13, Foreground = new SolidColorBrush(ColorHelper.FromArgb(255, 51, 51, 51))
                });
                info.Children.Add(new TextBlock
                {
                    Text = $"{member.Role} • {(member.IsOnline ? "Online" : "Offline")}",
                    FontSize = 11,
                    Foreground = new SolidColorBrush(ColorHelper.FromArgb(255, 170, 170, 170))
                });

                row.Children.Add(avatar);
                row.Children.Add(info);
                DashboardMembersPanel.Children.Add(row);
            }

            // Recent activity
            DashboardActivityPanel.Children.Clear();
            var activities = new[]
            {
                "Vinh đã cập nhật giao diện TaskPage",
                "Hùng đã bắt đầu thiết kế database",
                "Linh đã hoàn thành 80% API đăng nhập",
                "Tuấn đã review code module auth",
                "Vinh đã tạo project và thiết lập Git",
            };
            foreach (var activity in activities)
            {
                var actRow = new StackPanel { Orientation = Orientation.Horizontal, Spacing = 8 };
                actRow.Children.Add(new FontIcon
                {
                    Glyph = "\uE7C3", FontSize = 12,
                    Foreground = new SolidColorBrush(ColorHelper.FromArgb(255, 170, 170, 170)),
                    VerticalAlignment = VerticalAlignment.Center,
                });
                actRow.Children.Add(new TextBlock
                {
                    Text = activity, FontSize = 12,
                    Foreground = new SolidColorBrush(ColorHelper.FromArgb(255, 100, 100, 100)),
                    TextWrapping = TextWrapping.Wrap,
                });
                DashboardActivityPanel.Children.Add(actRow);
            }

            // Recent messages
            DashboardMessagesPanel.Children.Clear();
            foreach (var msg in _chatMessages.TakeLast(3))
            {
                var msgRow = new StackPanel { Orientation = Orientation.Horizontal, Spacing = 8 };
                var msgAvatar = new Border
                {
                    Width = 24, Height = 24, CornerRadius = new CornerRadius(12),
                    Background = new SolidColorBrush(ParseColor(msg.AvatarColor)),
                    VerticalAlignment = VerticalAlignment.Top,
                };
                msgAvatar.Child = new TextBlock
                {
                    Text = msg.AvatarInitial, Foreground = new SolidColorBrush(Colors.White),
                    FontSize = 10, FontWeight = Microsoft.UI.Text.FontWeights.SemiBold,
                    HorizontalAlignment = HorizontalAlignment.Center,
                    VerticalAlignment = VerticalAlignment.Center,
                };

                var msgContent = new StackPanel { Spacing = 1 };
                msgContent.Children.Add(new TextBlock
                {
                    Text = $"{msg.Sender} • {msg.Timestamp}", FontSize = 11,
                    Foreground = new SolidColorBrush(ColorHelper.FromArgb(255, 136, 136, 136)),
                });
                msgContent.Children.Add(new TextBlock
                {
                    Text = msg.Content, FontSize = 12,
                    Foreground = new SolidColorBrush(ColorHelper.FromArgb(255, 68, 68, 68)),
                    TextWrapping = TextWrapping.Wrap,
                });

                msgRow.Children.Add(msgAvatar);
                msgRow.Children.Add(msgContent);
                DashboardMessagesPanel.Children.Add(msgRow);
            }
        }

        /// <summary>
        /// Creates a stat card for the dashboard.
        /// </summary>
        private Border CreateStatCard(string label, string value, string foreColor, string bgColor)
        {
            var border = new Border
            {
                Background = new SolidColorBrush(ParseColor(bgColor)),
                BorderBrush = new SolidColorBrush(ColorHelper.FromArgb(255, 232, 232, 232)),
                BorderThickness = new Thickness(1),
                CornerRadius = new CornerRadius(8),
                Padding = new Thickness(20, 14, 20, 14),
                MinWidth = 120,
            };

            var stack = new StackPanel { Spacing = 4 };
            stack.Children.Add(new TextBlock
            {
                Text = label, FontSize = 12,
                Foreground = new SolidColorBrush(ColorHelper.FromArgb(255, 136, 136, 136)),
            });
            stack.Children.Add(new TextBlock
            {
                Text = value, FontSize = 28, FontWeight = Microsoft.UI.Text.FontWeights.Bold,
                Foreground = new SolidColorBrush(ParseColor(foreColor)),
            });

            border.Child = stack;
            return border;
        }

        // ================================================================
        // ASSIGN TASK PAGE
        // ================================================================

        /// <summary>
        /// Populates the assign task member combo box.
        /// </summary>
        private void PopulateAssignTaskCombo()
        {
            AssignTaskMemberCombo.Items.Clear();
            foreach (var member in _members)
            {
                AssignTaskMemberCombo.Items.Add(member.Name);
            }
        }

        /// <summary>
        /// Handles the assign task form submission.
        /// </summary>
        private void AssignTaskSubmit_Click(object sender, RoutedEventArgs e)
        {
            var taskName = AssignTaskNameBox.Text?.Trim();
            if (string.IsNullOrEmpty(taskName)) return;

            var assignee = AssignTaskMemberCombo.SelectedItem?.ToString() ?? "";
            var status = "Todo";
            if (AssignTaskStatusCombo.SelectedItem is ComboBoxItem statusItem)
            {
                status = statusItem.Content?.ToString() ?? "Todo";
            }

            // Format deadline
            var deadline = AssignTaskDeadline.Date.ToString("dd/MM/yyyy");

            var newTask = new TaskItem
            {
                Title = taskName,
                Assignee = string.IsNullOrEmpty(assignee) ? "Chưa giao" : assignee,
                DueDate = deadline,
                Status = status,
                Priority = "Trung bình",
            };

            _allTasks.Add(newTask);
            UpdateBadges();

            // Refresh task list on assign page
            AssignTaskListRepeater.ItemsSource = _allTasks.ToList();

            // Clear form
            AssignTaskNameBox.Text = string.Empty;
            AssignTaskDescBox.Text = string.Empty;
            AssignTaskMemberCombo.SelectedIndex = -1;
            AssignTaskStatusCombo.SelectedIndex = 0;
        }

        // ================================================================
        // UTILITY
        // ================================================================

        /// <summary>
        /// Parses a hex color string to a Windows.UI.Color.
        /// </summary>
        private static Windows.UI.Color ParseColor(string hex)
        {
            hex = hex.TrimStart('#');
            if (hex.Length == 6)
            {
                return ColorHelper.FromArgb(255,
                    byte.Parse(hex.Substring(0, 2), System.Globalization.NumberStyles.HexNumber),
                    byte.Parse(hex.Substring(2, 2), System.Globalization.NumberStyles.HexNumber),
                    byte.Parse(hex.Substring(4, 2), System.Globalization.NumberStyles.HexNumber));
            }
            return ColorHelper.FromArgb(255, 136, 136, 136);
        }

        private async void PickImage_Click(object sender, RoutedEventArgs e)
        {
            // TODO: chọn ảnh
        }

        private void RemoveImage_Click(object sender, RoutedEventArgs e)
        {
            PreviewImage.Source = null;
            PreviewFileName.Text = string.Empty;
            ImagePreviewArea.Visibility = Visibility.Collapsed;
        }
    }
}