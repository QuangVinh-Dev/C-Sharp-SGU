using UM.Admin.Models;
using UM.Admin.Services;

namespace UM.Admin.Controls
{
    public class ReportManagementControl : UserControl
    {
        private DataGridView _grid = null!;
        private TextBox _searchBox = null!;
        private ComboBox _statusFilter = null!;
        private ComboBox _targetTypeFilter = null!;
        private readonly MockReportService _reportService = MockReportService.Instance;

        public ReportManagementControl()
        {
            BackColor = Color.FromArgb(245, 245, 248);
            Padding = new Padding(24);
            DoubleBuffered = true;
            Dock = DockStyle.Fill;
            BuildUI();
            LoadData();
        }

        private void BuildUI()
        {
            var topPanel = new Panel { Dock = DockStyle.Top, Height = 50 };

            topPanel.Controls.Add(new Label { Text = "Search:", AutoSize = true, Location = new Point(0, 15), Font = new Font("Segoe UI", 9) });
            _searchBox = new TextBox { Location = new Point(55, 12), Width = 200, Font = new Font("Segoe UI", 9), PlaceholderText = "Search reports..." };
            _searchBox.TextChanged += (s, e) => LoadData();
            topPanel.Controls.Add(_searchBox);

            topPanel.Controls.Add(new Label { Text = "Status:", AutoSize = true, Location = new Point(270, 15), Font = new Font("Segoe UI", 9) });
            _statusFilter = new ComboBox { Location = new Point(320, 12), Width = 110, DropDownStyle = ComboBoxStyle.DropDownList, Font = new Font("Segoe UI", 9) };
            _statusFilter.Items.AddRange(new object[] { "All", "Pending", "Reviewing", "Resolved", "Rejected" });
            _statusFilter.SelectedIndex = 0;
            _statusFilter.SelectedIndexChanged += (s, e) => LoadData();
            topPanel.Controls.Add(_statusFilter);

            topPanel.Controls.Add(new Label { Text = "Type:", AutoSize = true, Location = new Point(445, 15), Font = new Font("Segoe UI", 9) });
            _targetTypeFilter = new ComboBox { Location = new Point(485, 12), Width = 110, DropDownStyle = ComboBoxStyle.DropDownList, Font = new Font("Segoe UI", 9) };
            _targetTypeFilter.Items.AddRange(new object[] { "All", "User", "Server", "Message" });
            _targetTypeFilter.SelectedIndex = 0;
            _targetTypeFilter.SelectedIndexChanged += (s, e) => LoadData();
            topPanel.Controls.Add(_targetTypeFilter);

            var refreshBtn = new Button { Text = "Refresh", Location = new Point(610, 10), Height = 28, Width = 80, Font = new Font("Segoe UI", 9), BackColor = Color.White, FlatStyle = FlatStyle.Flat };
            refreshBtn.FlatAppearance.BorderColor = Color.FromArgb(200, 200, 200);
            refreshBtn.Click += (s, e) => LoadData();
            topPanel.Controls.Add(refreshBtn);

            Controls.Add(topPanel);

            _grid = new DataGridView();
            StyleGrid(_grid);
            _grid.Dock = DockStyle.Fill;
            _grid.CellFormatting += Grid_CellFormatting;
            _grid.CellContentClick += Grid_CellContentClick;
            _grid.CellDoubleClick += Grid_CellDoubleClick;

            _grid.AutoGenerateColumns = false;
            _grid.Columns.Add(new DataGridViewTextBoxColumn { Name = "Id", HeaderText = "ID", Width = 40 });
            _grid.Columns.Add(new DataGridViewTextBoxColumn { Name = "Reporter", HeaderText = "Reporter" });
            _grid.Columns.Add(new DataGridViewTextBoxColumn { Name = "TargetType", HeaderText = "Type", Width = 60 });
            _grid.Columns.Add(new DataGridViewTextBoxColumn { Name = "Target", HeaderText = "Target" });
            _grid.Columns.Add(new DataGridViewTextBoxColumn { Name = "Reason", HeaderText = "Reason" });
            _grid.Columns.Add(new DataGridViewTextBoxColumn { Name = "CreatedTime", HeaderText = "Created", Width = 120 });
            _grid.Columns.Add(new DataGridViewTextBoxColumn { Name = "Status", HeaderText = "Status", Width = 80 });
            _grid.Columns.Add(new DataGridViewTextBoxColumn { Name = "AssignedAdmin", HeaderText = "Assigned To" });
            _grid.Columns.Add(new DataGridViewButtonColumn { Name = "Actions", HeaderText = "Actions", Text = "View", UseColumnTextForButtonValue = true, Width = 60 });

            Controls.Add(_grid);
            _grid.BringToFront();
        }

        private void LoadData()
        {
            _grid.Rows.Clear();
            var search = _searchBox.Text.Trim().ToLower();
            var statusFilter = _statusFilter.SelectedItem?.ToString() ?? "All";
            var typeFilter = _targetTypeFilter.SelectedItem?.ToString() ?? "All";
            var currentRole = MockAdminService.Instance.CurrentAdmin.Role;

            foreach (var r in _reportService.Reports)
            {
                // Role-based filtering
                if (currentRole == AdminRole.UserAdmin && r.TargetType != ReportTargetType.User) continue;
                if (currentRole == AdminRole.ServerAdmin && r.TargetType != ReportTargetType.Server) continue;

                bool matchSearch = string.IsNullOrEmpty(search) ||
                    r.Reporter.ToLower().Contains(search) ||
                    r.Target.ToLower().Contains(search) ||
                    r.Reason.ToLower().Contains(search) ||
                    r.Id.ToString().Contains(search);

                bool matchStatus = statusFilter == "All" || r.Status.ToString() == statusFilter;
                bool matchType = typeFilter == "All" || r.TargetType.ToString() == typeFilter;

                if (matchSearch && matchStatus && matchType)
                {
                    _grid.Rows.Add(r.Id, r.Reporter, r.TargetType.ToString(), r.Target, r.Reason,
                        r.CreatedTime.ToString("yyyy-MM-dd HH:mm"), r.Status.ToString(), r.AssignedAdmin ?? "-");
                    _grid.Rows[_grid.Rows.Count - 1].Tag = r;
                }
            }
        }

        private void Grid_CellFormatting(object? sender, DataGridViewCellFormattingEventArgs e)
        {
            if (_grid.Columns[e.ColumnIndex].Name == "Status" && e.Value != null)
            {
                var status = e.Value.ToString();
                if (e.CellStyle != null)
                {
                    e.CellStyle.Font = new Font("Segoe UI", 9, FontStyle.Bold);
                    e.CellStyle.ForeColor = status switch
                    {
                        "Pending" => Color.FromArgb(230, 126, 34),
                        "Reviewing" => Color.FromArgb(33, 150, 243),
                        "Resolved" => Color.FromArgb(46, 125, 50),
                        "Rejected" => Color.FromArgb(130, 130, 140),
                        _ => Color.Gray,
                    };
                }
            }
        }

        private void Grid_CellContentClick(object? sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0 && _grid.Columns[e.ColumnIndex].Name == "Actions")
                ShowReportDetail(_grid.Rows[e.RowIndex].Tag as Report);
        }

        private void Grid_CellDoubleClick(object? sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0) ShowReportDetail(_grid.Rows[e.RowIndex].Tag as Report);
        }

        private void ShowReportDetail(Report? report)
        {
            if (report == null) return;
            using var form = new Form
            {
                Text = $"Report #{report.Id}",
                Size = new Size(600, 650),
                StartPosition = FormStartPosition.CenterParent,
                FormBorderStyle = FormBorderStyle.FixedDialog,
                MaximizeBox = false, MinimizeBox = false,
                BackColor = Color.White, Font = new Font("Segoe UI", 9.5F)
            };

            var mainPanel = new Panel { Dock = DockStyle.Fill, AutoScroll = true, Padding = new Padding(20) };

            int y = 10;
            mainPanel.Controls.Add(new Label { Text = $"Report #{report.Id}", Font = new Font("Segoe UI", 13, FontStyle.Bold), Location = new Point(10, y), AutoSize = true });
            y += 35;
            mainPanel.Controls.Add(new Label { Text = $"Reporter: {report.Reporter}", Location = new Point(10, y), AutoSize = true }); y += 22;
            mainPanel.Controls.Add(new Label { Text = $"Target ({report.TargetType}): {report.Target}", Location = new Point(10, y), AutoSize = true }); y += 22;
            mainPanel.Controls.Add(new Label { Text = $"Reason: {report.Reason}", Location = new Point(10, y), AutoSize = true, Font = new Font("Segoe UI", 9.5F, FontStyle.Bold) }); y += 22;
            mainPanel.Controls.Add(new Label { Text = $"Description: {report.Description}", Location = new Point(10, y), AutoSize = true, MaximumSize = new Size(540, 0) }); y += 40;
            mainPanel.Controls.Add(new Label { Text = $"Created: {report.CreatedTime:yyyy-MM-dd HH:mm}", Location = new Point(10, y), AutoSize = true }); y += 22;

            var statusLabel = new Label
            {
                Text = $"Status: {report.Status}",
                Location = new Point(10, y), AutoSize = true,
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                ForeColor = report.Status switch
                {
                    ReportStatus.Pending => Color.FromArgb(230, 126, 34),
                    ReportStatus.Reviewing => Color.FromArgb(33, 150, 243),
                    ReportStatus.Resolved => Color.FromArgb(46, 125, 50),
                    _ => Color.Gray,
                }
            };
            mainPanel.Controls.Add(statusLabel); y += 22;
            mainPanel.Controls.Add(new Label { Text = $"Assigned Admin: {report.AssignedAdmin ?? "None"}", Location = new Point(10, y), AutoSize = true }); y += 30;

            // Evidence section
            mainPanel.Controls.Add(new Label { Text = "Evidence:", Location = new Point(10, y), AutoSize = true, Font = new Font("Segoe UI", 10, FontStyle.Bold) }); y += 25;
            foreach (var ev in report.Evidences)
            {
                var evPanel = new Panel
                {
                    Location = new Point(10, y), Width = 540, Height = 50,
                    BackColor = Color.FromArgb(248, 249, 250), Padding = new Padding(8),
                };
                evPanel.Controls.Add(new Label { Text = $"[{ev.Type}] {ev.Name}", AutoSize = true, Font = new Font("Segoe UI", 8.5F, FontStyle.Bold), Location = new Point(5, 3) });
                evPanel.Controls.Add(new Label { Text = ev.Content, AutoSize = true, MaximumSize = new Size(520, 0), Font = new Font("Segoe UI", 8.5F), ForeColor = Color.FromArgb(80, 80, 90), Location = new Point(5, 22) });
                mainPanel.Controls.Add(evPanel);
                y += 55;
            }

            form.Controls.Add(mainPanel);

            // Action buttons
            var btnPanel = new Panel { Dock = DockStyle.Bottom, Height = 90, BackColor = Color.FromArgb(248, 249, 250), Padding = new Padding(15) };

            var reviewBtn = MakeButton("Start Review", Color.FromArgb(33, 150, 243), 110);
            reviewBtn.Location = new Point(15, 10);
            reviewBtn.Click += (s, e) => ChangeReportStatus(report, ReportStatus.Reviewing, statusLabel, form);
            btnPanel.Controls.Add(reviewBtn);

            var resolveBtn = MakeButton("Resolve", Color.FromArgb(46, 125, 50), 90);
            resolveBtn.Location = new Point(135, 10);
            resolveBtn.Click += (s, e) => ChangeReportStatus(report, ReportStatus.Resolved, statusLabel, form);
            btnPanel.Controls.Add(resolveBtn);

            var rejectBtn = MakeButton("Reject", Color.FromArgb(130, 130, 140), 90);
            rejectBtn.Location = new Point(235, 10);
            rejectBtn.Click += (s, e) => ChangeReportStatus(report, ReportStatus.Rejected, statusLabel, form);
            btnPanel.Controls.Add(rejectBtn);

            // Assign admin
            btnPanel.Controls.Add(new Label { Text = "Assign:", Location = new Point(15, 52), AutoSize = true, Font = new Font("Segoe UI", 9) });
            var adminCombo = new ComboBox { Location = new Point(70, 48), Width = 150, DropDownStyle = ComboBoxStyle.DropDownList };
            foreach (var admin in MockAdminService.Instance.Admins.Where(a => a.Status == AdminStatus.Active))
                adminCombo.Items.Add(admin.Name);
            if (report.AssignedAdmin != null) adminCombo.SelectedItem = report.AssignedAdmin;
            btnPanel.Controls.Add(adminCombo);

            var assignBtn = MakeButton("Assign", Color.FromArgb(103, 58, 183), 70);
            assignBtn.Location = new Point(230, 46);
            assignBtn.Click += (s, e) =>
            {
                if (adminCombo.SelectedItem != null)
                {
                    var adminName = adminCombo.SelectedItem.ToString()!;
                    if (MessageBox.Show($"Assign this report to {adminName}?", "Assign", MessageBoxButtons.YesNo) == DialogResult.Yes)
                    {
                        report.AssignedAdmin = adminName;
                        MockAdminService.Instance.AddActivity("Assigned report", $"Report #{report.Id} to {adminName}");
                        LoadData();
                        MessageBox.Show("Admin assigned.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                }
            };
            btnPanel.Controls.Add(assignBtn);

            form.Controls.Add(btnPanel);
            form.ShowDialog();
        }

        private void ChangeReportStatus(Report report, ReportStatus newStatus, Label statusLabel, Form parentForm)
        {
            if (MessageBox.Show($"Change report status to {newStatus}?", "Confirm", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
            {
                var adminName = MockAdminService.Instance.CurrentAdmin.Name;
                _reportService.UpdateReportStatus(report.Id, newStatus, adminName);
                MockAdminService.Instance.AddActivity($"Report {newStatus.ToString().ToLower()}", $"Report #{report.Id}");
                statusLabel.Text = $"Status: {newStatus}";
                LoadData();
                MessageBox.Show($"Report status changed to {newStatus}.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        private static Button MakeButton(string text, Color bgColor, int width = 80)
        {
            var btn = new Button
            {
                Text = text, Width = width, Height = 30, FlatStyle = FlatStyle.Flat,
                BackColor = bgColor, ForeColor = Color.White,
                Font = new Font("Segoe UI", 8.5F, FontStyle.Bold),
                Cursor = Cursors.Hand,
            };
            btn.FlatAppearance.BorderSize = 0;
            return btn;
        }

        private static void StyleGrid(DataGridView grid)
        {
            grid.EnableHeadersVisualStyles = false;
            grid.BorderStyle = BorderStyle.None;
            grid.BackgroundColor = Color.White;
            grid.RowHeadersVisible = false;
            grid.AllowUserToAddRows = false;
            grid.AllowUserToDeleteRows = false;
            grid.AllowUserToResizeRows = false;
            grid.ReadOnly = true;
            grid.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            grid.MultiSelect = false;
            grid.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            grid.RowTemplate.Height = 36;
            grid.ColumnHeadersHeight = 38;
            grid.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.DisableResizing;
            grid.GridColor = Color.FromArgb(235, 235, 240);
            grid.CellBorderStyle = DataGridViewCellBorderStyle.SingleHorizontal;
            grid.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(248, 249, 250);
            grid.ColumnHeadersDefaultCellStyle.ForeColor = Color.FromArgb(80, 80, 90);
            grid.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            grid.ColumnHeadersDefaultCellStyle.SelectionBackColor = Color.FromArgb(248, 249, 250);
            grid.ColumnHeadersDefaultCellStyle.SelectionForeColor = Color.FromArgb(80, 80, 90);
            grid.DefaultCellStyle.BackColor = Color.White;
            grid.DefaultCellStyle.ForeColor = Color.FromArgb(50, 50, 60);
            grid.DefaultCellStyle.SelectionBackColor = Color.FromArgb(232, 240, 254);
            grid.DefaultCellStyle.SelectionForeColor = Color.FromArgb(50, 50, 60);
            grid.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(252, 252, 254);
        }
    }
}
