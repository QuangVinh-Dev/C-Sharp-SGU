using UM.Admin.Models;
using UM.Admin.Services;

namespace UM.Admin.Controls
{
    public class DashboardControl : UserControl
    {
        private readonly MockDashboardService _dashboardService = MockDashboardService.Instance;
        private readonly MockAdminService _adminService = MockAdminService.Instance;

        public DashboardControl()
        {
            BackColor = Color.FromArgb(245, 245, 248);
            Padding = new Padding(24);
            AutoScroll = true;
            DoubleBuffered = true;
            BuildDashboard();
        }

        private void BuildDashboard()
        {
            var stats = _dashboardService.GetStatistics();

            var mainFlow = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                FlowDirection = FlowDirection.TopDown,
                WrapContents = false,
                AutoScroll = true,
                Padding = new Padding(0),
            };

            // === Stats Cards Row ===
            var statsLabel = CreateSectionTitle("Overall Statistics");
            mainFlow.Controls.Add(statsLabel);

            var statsFlow = new FlowLayoutPanel
            {
                FlowDirection = FlowDirection.LeftToRight,
                WrapContents = true,
                AutoSize = true,
                AutoSizeMode = AutoSizeMode.GrowAndShrink,
                Margin = new Padding(0, 0, 0, 16),
            };

            var usersCard = CreateStatCard("Total Users", stats.TotalUsers.ToString(), Color.FromArgb(66, 133, 244), Color.FromArgb(232, 240, 254));
            if (MockAdminService.Instance.CurrentAdmin.Role == AdminRole.UserAdmin)
            {
                usersCard.Cursor = Cursors.Hand;
                void NavigateToUsers(object? s, EventArgs e)
                {
                    if (ParentForm is Forms.MainForm mf) mf.NavigateTo("Users");
                }
                usersCard.Click += NavigateToUsers;
                foreach (Control c in usersCard.Controls) c.Click += NavigateToUsers;
            }
            statsFlow.Controls.Add(usersCard);
            
            var serversCard = CreateStatCard("Total Servers", stats.TotalServers.ToString(), Color.FromArgb(52, 168, 83), Color.FromArgb(232, 245, 233));
            if (MockAdminService.Instance.CurrentAdmin.Role == AdminRole.ServerAdmin)
            {
                serversCard.Cursor = Cursors.Hand;
                void NavigateToServers(object? s, EventArgs e)
                {
                    if (ParentForm is Forms.MainForm mf) mf.NavigateTo("Servers");
                }
                serversCard.Click += NavigateToServers;
                foreach (Control c in serversCard.Controls) c.Click += NavigateToServers;
            }
            statsFlow.Controls.Add(serversCard);

            statsFlow.Controls.Add(CreateStatCard("New Today", stats.NewUsersToday.ToString(), Color.FromArgb(251, 188, 4), Color.FromArgb(255, 243, 224)));
            statsFlow.Controls.Add(CreateStatCard("This Week", stats.NewUsersThisWeek.ToString(), Color.FromArgb(234, 67, 53), Color.FromArgb(252, 232, 230)));
            statsFlow.Controls.Add(CreateStatCard("This Month", stats.NewUsersThisMonth.ToString(), Color.FromArgb(103, 58, 183), Color.FromArgb(237, 231, 246)));

            mainFlow.Controls.Add(statsFlow);

            // === Reports & Banned Row ===
            var alertsFlow = new FlowLayoutPanel
            {
                FlowDirection = FlowDirection.LeftToRight,
                WrapContents = true,
                AutoSize = true,
                AutoSizeMode = AutoSizeMode.GrowAndShrink,
                Margin = new Padding(0, 0, 0, 20),
            };
            var reportsCard = CreateStatCard("Pending Reports", stats.PendingReports.ToString(), Color.FromArgb(234, 67, 53), Color.FromArgb(252, 232, 230));
            reportsCard.Cursor = Cursors.Hand;
            void NavigateToReports(object? s, EventArgs e)
            {
                if (ParentForm is Forms.MainForm mf) mf.NavigateTo("Reports");
            }
            reportsCard.Click += NavigateToReports;
            foreach (Control c in reportsCard.Controls) c.Click += NavigateToReports;
            alertsFlow.Controls.Add(reportsCard);

            alertsFlow.Controls.Add(CreateStatCard("Banned Accounts", stats.BannedAccounts.ToString(), Color.FromArgb(183, 28, 28), Color.FromArgb(255, 235, 238)));
            mainFlow.Controls.Add(alertsFlow);

            // === Recent Admin Activity ===
            mainFlow.Controls.Add(CreateSectionTitle("Recent Admin Activity"));

            var activityGrid = new DataGridView
            {
                Width = 900,
                Height = 260,
                ReadOnly = true,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                AllowUserToResizeRows = false,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                MultiSelect = false,
                RowHeadersVisible = false,
                BackgroundColor = Color.White,
                BorderStyle = BorderStyle.None,
                GridColor = Color.FromArgb(235, 235, 240),
                CellBorderStyle = DataGridViewCellBorderStyle.SingleHorizontal,
                Font = new Font("Segoe UI", 9F),
                Margin = new Padding(0, 0, 0, 20),
            };
            StyleDataGridView(activityGrid);

            activityGrid.Columns.Add("Admin", "Admin");
            activityGrid.Columns.Add("Action", "Action");
            activityGrid.Columns.Add("Target", "Target");
            activityGrid.Columns.Add("Time", "Time");
            activityGrid.Columns.Add("Result", "Result");

            activityGrid.Columns["Time"]!.DefaultCellStyle.Format = "yyyy-MM-dd HH:mm";

            foreach (var act in _adminService.Activities.Take(10))
            {
                var idx = activityGrid.Rows.Add(act.Admin, act.Action, act.Target, act.Time.ToString("yyyy-MM-dd HH:mm"), act.Result);
                var row = activityGrid.Rows[idx];

                // Color the Result cell
                if (act.Result == "Success")
                {
                    row.Cells["Result"].Style.ForeColor = Color.FromArgb(46, 125, 50);
                    row.Cells["Result"].Style.BackColor = Color.FromArgb(232, 245, 233);
                }
            }

            mainFlow.Controls.Add(activityGrid);

            Controls.Add(mainFlow);
        }

        private Panel CreateStatCard(string label, string value, Color foreColor, Color bgColor)
        {
            var card = new Panel
            {
                Width = 170,
                Height = 90,
                BackColor = bgColor,
                Margin = new Padding(0, 0, 12, 8),
                Padding = new Padding(16, 12, 16, 12),
            };
            // Rounded corners via paint
            card.Paint += (s, e) =>
            {
                using var pen = new Pen(Color.FromArgb(225, 225, 230), 1);
                var rect = new Rectangle(0, 0, card.Width - 1, card.Height - 1);
                var radius = 8;
                using var path = RoundedRect(rect, radius);
                e.Graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
                using var brush = new SolidBrush(bgColor);
                e.Graphics.FillPath(brush, path);
                e.Graphics.DrawPath(pen, path);
            };

            var valueLabel = new Label
            {
                Text = value,
                Font = new Font("Segoe UI", 22F, FontStyle.Bold),
                ForeColor = foreColor,
                AutoSize = true,
                Location = new Point(16, 10),
            };
            var titleLabel = new Label
            {
                Text = label,
                Font = new Font("Segoe UI", 8.5F),
                ForeColor = Color.FromArgb(100, 100, 110),
                AutoSize = true,
                Location = new Point(16, 55),
            };
            card.Controls.Add(valueLabel);
            card.Controls.Add(titleLabel);
            return card;
        }

        private Label CreateSectionTitle(string text)
        {
            return new Label
            {
                Text = text,
                Font = new Font("Segoe UI", 12F, FontStyle.Bold),
                ForeColor = Color.FromArgb(40, 40, 50),
                AutoSize = true,
                Margin = new Padding(0, 8, 0, 8),
            };
        }

        private static void StyleDataGridView(DataGridView dgv)
        {
            dgv.EnableHeadersVisualStyles = false;
            dgv.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(248, 249, 250);
            dgv.ColumnHeadersDefaultCellStyle.ForeColor = Color.FromArgb(80, 80, 90);
            dgv.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            dgv.ColumnHeadersDefaultCellStyle.SelectionBackColor = Color.FromArgb(248, 249, 250);
            dgv.ColumnHeadersDefaultCellStyle.SelectionForeColor = Color.FromArgb(80, 80, 90);
            dgv.ColumnHeadersHeight = 36;
            dgv.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.DisableResizing;

            dgv.DefaultCellStyle.BackColor = Color.White;
            dgv.DefaultCellStyle.ForeColor = Color.FromArgb(50, 50, 60);
            dgv.DefaultCellStyle.SelectionBackColor = Color.FromArgb(232, 240, 254);
            dgv.DefaultCellStyle.SelectionForeColor = Color.FromArgb(50, 50, 60);
            dgv.DefaultCellStyle.Padding = new Padding(4, 2, 4, 2);
            dgv.RowTemplate.Height = 32;

            dgv.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(252, 252, 254);
        }

        private static System.Drawing.Drawing2D.GraphicsPath RoundedRect(Rectangle bounds, int radius)
        {
            int d = radius * 2;
            var path = new System.Drawing.Drawing2D.GraphicsPath();
            path.AddArc(bounds.X, bounds.Y, d, d, 180, 90);
            path.AddArc(bounds.Right - d, bounds.Y, d, d, 270, 90);
            path.AddArc(bounds.Right - d, bounds.Bottom - d, d, d, 0, 90);
            path.AddArc(bounds.X, bounds.Bottom - d, d, d, 90, 90);
            path.CloseFigure();
            return path;
        }
    }
}
