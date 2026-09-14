using Microsoft.UI;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
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
        private string _currentChannel = "todo";
        private int _selectedServerIndex = 0;
        private readonly List<Border> _serverBorders = new();

        public TaskPage()
        {
            this.InitializeComponent();
            LoadMockData();
            LoadServers();
            ShowChannel("todo");
        }

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
            TodoBadge.Text = _allTasks.Count(t => t.Status == "Todo").ToString();
            DoingBadge.Text = _allTasks.Count(t => t.Status == "Doing").ToString();
            DoneBadge.Text = _allTasks.Count(t => t.Status == "Done").ToString();
        }

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

        /// <summary>
        /// Shows a specific channel's tasks in the main content area.
        /// "todo" shows all 3 sections, "doing" shows only doing, "done" shows only done.
        /// </summary>
        private void ShowChannel(string channel)
        {
            _currentChannel = channel;

            var todoTasks = _allTasks.Where(t => t.Status == "Todo").ToList();
            var doingTasks = _allTasks.Where(t => t.Status == "Doing").ToList();
            var doneTasks = _allTasks.Where(t => t.Status == "Done").ToList();

            // Update counts
            TodoCountText.Text = $"{todoTasks.Count} công việc";
            DoingCountText.Text = $"{doingTasks.Count} công việc";
            DoneCountText.Text = $"{doneTasks.Count} công việc";

            // Bind data
            TodoRepeater.ItemsSource = todoTasks;
            DoingRepeater.ItemsSource = doingTasks;
            DoneRepeater.ItemsSource = doneTasks;

            // Show/hide sections based on selected channel
            switch (channel)
            {
                case "todo":
                    CurrentChannelName.Text = "todo";
                    CurrentChannelDescription.Text = "Danh sách công việc cần hoàn thành";
                    TodoSection.Visibility = Visibility.Visible;
                    DoingSection.Visibility = Visibility.Visible;
                    DoneSection.Visibility = Visibility.Visible;
                    break;
                case "doing":
                    CurrentChannelName.Text = "doing";
                    CurrentChannelDescription.Text = "Công việc đang thực hiện";
                    TodoSection.Visibility = Visibility.Collapsed;
                    DoingSection.Visibility = Visibility.Visible;
                    DoneSection.Visibility = Visibility.Collapsed;
                    break;
                case "done":
                    CurrentChannelName.Text = "done";
                    CurrentChannelDescription.Text = "Công việc đã hoàn thành";
                    TodoSection.Visibility = Visibility.Collapsed;
                    DoingSection.Visibility = Visibility.Collapsed;
                    DoneSection.Visibility = Visibility.Visible;
                    break;
            }

            // Update channel highlight
            UpdateChannelHighlight(channel);
        }

        /// <summary>
        /// Updates the visual highlight of the active channel in the sidebar.
        /// </summary>
        private void UpdateChannelHighlight(string activeChannel)
        {
            // Reset all
            ChannelTodo.Background = new SolidColorBrush(Microsoft.UI.Colors.Transparent);
            ChannelDoing.Background = new SolidColorBrush(Microsoft.UI.Colors.Transparent);
            ChannelDone.Background = new SolidColorBrush(Microsoft.UI.Colors.Transparent);

            // Highlight active
            var highlightBrush = new SolidColorBrush(
                Microsoft.UI.ColorHelper.FromArgb(255, 232, 240, 254)); // #E8F0FE

            switch (activeChannel)
            {
                case "todo":
                    ChannelTodo.Background = highlightBrush;
                    break;
                case "doing":
                    ChannelDoing.Background = highlightBrush;
                    break;
                case "done":
                    ChannelDone.Background = highlightBrush;
                    break;
            }
        }

        /// <summary>
        /// Handles channel click in the sidebar.
        /// </summary>
        private void Channel_PointerPressed(object sender, PointerRoutedEventArgs e)
        {
            if (sender is Border border)
            {
                if (border == ChannelTodo)
                    ShowChannel("todo");
                else if (border == ChannelDoing)
                    ShowChannel("doing");
                else if (border == ChannelDone)
                    ShowChannel("done");
            }
        }

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
    }
}