using UM.Admin.Models;
using UM.Admin.Services;

namespace UM.Admin.Controls
{
    public class ServerManagementControl : UserControl
    {
        private DataGridView _grid = null!;
        private TextBox _searchBox = null!;
        private ComboBox _statusFilter = null!;
        private readonly MockServerService _serverService = MockServerService.Instance;

        public ServerManagementControl()
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
            _searchBox = new TextBox { Location = new Point(55, 12), Width = 220, Font = new Font("Segoe UI", 9), PlaceholderText = "Search servers..." };
            _searchBox.TextChanged += (s, e) => LoadData();
            topPanel.Controls.Add(_searchBox);

            topPanel.Controls.Add(new Label { Text = "Status:", AutoSize = true, Location = new Point(300, 15), Font = new Font("Segoe UI", 9) });
            _statusFilter = new ComboBox { Location = new Point(350, 12), Width = 120, DropDownStyle = ComboBoxStyle.DropDownList, Font = new Font("Segoe UI", 9) };
            _statusFilter.Items.AddRange(new object[] { "All", "Active", "Locked" });
            _statusFilter.SelectedIndex = 0;
            _statusFilter.SelectedIndexChanged += (s, e) => LoadData();
            topPanel.Controls.Add(_statusFilter);

            var refreshBtn = new Button { Text = "Refresh", Location = new Point(490, 10), Height = 28, Width = 80, Font = new Font("Segoe UI", 9), BackColor = Color.White, FlatStyle = FlatStyle.Flat };
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
            _grid.Columns.Add(new DataGridViewTextBoxColumn { Name = "Id", HeaderText = "ID", Width = 50 });
            _grid.Columns.Add(new DataGridViewTextBoxColumn { Name = "Name", HeaderText = "Server Name" });
            _grid.Columns.Add(new DataGridViewTextBoxColumn { Name = "Owner", HeaderText = "Owner" });
            _grid.Columns.Add(new DataGridViewTextBoxColumn { Name = "Members", HeaderText = "Members", Width = 70 });
            _grid.Columns.Add(new DataGridViewTextBoxColumn { Name = "Channels", HeaderText = "Channels", Width = 70 });
            _grid.Columns.Add(new DataGridViewTextBoxColumn { Name = "Storage", HeaderText = "Storage", Width = 80 });
            _grid.Columns.Add(new DataGridViewTextBoxColumn { Name = "Status", HeaderText = "Status", Width = 70 });
            _grid.Columns.Add(new DataGridViewTextBoxColumn { Name = "CreatedDate", HeaderText = "Created", Width = 100 });
            _grid.Columns.Add(new DataGridViewButtonColumn { Name = "Actions", HeaderText = "Actions", Text = "View", UseColumnTextForButtonValue = true, Width = 60 });

            Controls.Add(_grid);
            _grid.BringToFront();
        }

        private void LoadData()
        {
            _grid.Rows.Clear();
            var search = _searchBox.Text.Trim().ToLower();
            var statusFilter = _statusFilter.SelectedItem?.ToString() ?? "All";

            foreach (var s in _serverService.Servers)
            {
                bool matchSearch = string.IsNullOrEmpty(search) ||
                    s.Name.ToLower().Contains(search) ||
                    s.Owner.ToLower().Contains(search);
                bool matchStatus = statusFilter == "All" || s.Status.ToString() == statusFilter;

                if (matchSearch && matchStatus)
                {
                    _grid.Rows.Add(s.Id, s.Name, s.Owner, s.Members, s.Channels, s.StorageUsed, s.Status.ToString(), s.CreatedDate.ToString("yyyy-MM-dd"));
                    _grid.Rows[_grid.Rows.Count - 1].Tag = s;
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
                    e.CellStyle.ForeColor = status == "Active" ? Color.FromArgb(46, 125, 50) : Color.FromArgb(198, 40, 40);
                }
            }
        }

        private void Grid_CellContentClick(object? sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0 && _grid.Columns[e.ColumnIndex].Name == "Actions")
                ShowServerDetail(_grid.Rows[e.RowIndex].Tag as Server);
        }

        private void Grid_CellDoubleClick(object? sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0) ShowServerDetail(_grid.Rows[e.RowIndex].Tag as Server);
        }

        private void ShowServerDetail(Server? server)
        {
            if (server == null) return;
            using var form = new Form
            {
                Text = $"Server Details - {server.Name}",
                Size = new Size(520, 480),
                StartPosition = FormStartPosition.CenterParent,
                FormBorderStyle = FormBorderStyle.FixedDialog,
                MaximizeBox = false, MinimizeBox = false,
                BackColor = Color.White, Font = new Font("Segoe UI", 9.5F)
            };

            var info = new FlowLayoutPanel { Dock = DockStyle.Fill, FlowDirection = FlowDirection.TopDown, Padding = new Padding(20), AutoScroll = true };
            info.Controls.Add(MakeLabel($"Server ID: {server.Id}", true));
            info.Controls.Add(MakeLabel($"Name: {server.Name}"));
            info.Controls.Add(MakeLabel($"Owner: {server.Owner}"));
            info.Controls.Add(MakeLabel($"Created: {server.CreatedDate:yyyy-MM-dd}"));
            info.Controls.Add(MakeLabel($"Members: {server.Members}"));
            info.Controls.Add(MakeLabel($"Channels: {server.Channels}"));
            info.Controls.Add(MakeLabel($"Storage: {server.StorageUsed}"));
            info.Controls.Add(MakeLabel($"Status: {server.Status}", false, server.Status == ServerStatus.Locked ? Color.Red : Color.FromArgb(46, 125, 50)));
            form.Controls.Add(info);

            var btnPanel = new FlowLayoutPanel { Dock = DockStyle.Bottom, Height = 50, Padding = new Padding(15, 8, 15, 8), FlowDirection = FlowDirection.LeftToRight };

            var membersBtn = MakeButton("View Members", Color.FromArgb(66, 133, 244), 110);
            membersBtn.Click += (s, e) => ShowServerMembers(server);
            btnPanel.Controls.Add(membersBtn);

            var reportsBtn = MakeButton("View Reports", Color.FromArgb(103, 58, 183), 110);
            reportsBtn.Click += (s, e) => ShowServerReports(server);
            btnPanel.Controls.Add(reportsBtn);

            if (server.Status == ServerStatus.Active)
            {
                var lockBtn = MakeButton("Lock Server", Color.FromArgb(198, 40, 40), 110);
                lockBtn.Click += (s, e) =>
                {
                    if (MessageBox.Show($"Are you sure you want to LOCK server '{server.Name}'?\n\nData will be preserved. Only the operational status changes.", "Lock Server", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == DialogResult.Yes)
                    {
                        _serverService.LockServer(server.Id);
                        MockAdminService.Instance.AddActivity("Locked server", server.Name);
                        LoadData();
                        form.Close();
                    }
                };
                btnPanel.Controls.Add(lockBtn);
            }
            else
            {
                var unlockBtn = MakeButton("Unlock Server", Color.FromArgb(46, 125, 50), 110);
                unlockBtn.Click += (s, e) =>
                {
                    if (MessageBox.Show($"Are you sure you want to UNLOCK server '{server.Name}'?", "Unlock Server", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                    {
                        _serverService.UnlockServer(server.Id);
                        MockAdminService.Instance.AddActivity("Unlocked server", server.Name);
                        LoadData();
                        form.Close();
                    }
                };
                btnPanel.Controls.Add(unlockBtn);
            }

            form.Controls.Add(btnPanel);
            form.ShowDialog();
        }

        private void ShowServerMembers(Server server)
        {
            using var form = new Form
            {
                Text = $"Members - {server.Name}",
                Size = new Size(750, 500), StartPosition = FormStartPosition.CenterParent,
                BackColor = Color.White, Font = new Font("Segoe UI", 9.5F)
            };

            var searchPanel = new Panel { Dock = DockStyle.Top, Height = 40, Padding = new Padding(10, 8, 10, 0) };
            var memberSearch = new TextBox { Dock = DockStyle.Left, Width = 250, PlaceholderText = "Search members..." };
            searchPanel.Controls.Add(memberSearch);

            var memberGrid = new DataGridView();
            StyleGrid(memberGrid);
            memberGrid.Dock = DockStyle.Fill;
            memberGrid.AutoGenerateColumns = false;
            memberGrid.Columns.Add(new DataGridViewTextBoxColumn { Name = "UserId", HeaderText = "User ID", Width = 60 });
            memberGrid.Columns.Add(new DataGridViewTextBoxColumn { Name = "Username", HeaderText = "Username" });
            memberGrid.Columns.Add(new DataGridViewTextBoxColumn { Name = "DisplayName", HeaderText = "Display Name" });
            memberGrid.Columns.Add(new DataGridViewTextBoxColumn { Name = "Role", HeaderText = "Role", Width = 80 });
            memberGrid.Columns.Add(new DataGridViewTextBoxColumn { Name = "JoinedDate", HeaderText = "Joined", Width = 100 });
            memberGrid.Columns.Add(new DataGridViewTextBoxColumn { Name = "Status", HeaderText = "Status", Width = 80 });

            void LoadMembers()
            {
                memberGrid.Rows.Clear();
                var q = memberSearch.Text.Trim().ToLower();
                foreach (var m in _serverService.GetMembers(server.Id))
                {
                    bool match = string.IsNullOrEmpty(q) || m.Username.ToLower().Contains(q) || m.DisplayName.ToLower().Contains(q);
                    if (match)
                    {
                        memberGrid.Rows.Add(m.UserId, m.Username, m.DisplayName, m.Role, m.JoinedDate.ToString("yyyy-MM-dd"), m.Status.ToString());
                    }
                }
            }
            memberSearch.TextChanged += (s, e) => LoadMembers();
            LoadMembers();

            form.Controls.Add(memberGrid);
            form.Controls.Add(searchPanel);
            form.ShowDialog();
        }

        private void ShowServerReports(Server server)
        {
            using var form = new Form
            {
                Text = $"Reports - {server.Name}",
                Size = new Size(750, 400), StartPosition = FormStartPosition.CenterParent,
                BackColor = Color.White, Font = new Font("Segoe UI", 9.5F)
            };

            var reportGrid = new DataGridView();
            StyleGrid(reportGrid);
            reportGrid.Dock = DockStyle.Fill;
            reportGrid.AutoGenerateColumns = false;
            reportGrid.Columns.Add(new DataGridViewTextBoxColumn { Name = "Id", HeaderText = "Report ID", Width = 60 });
            reportGrid.Columns.Add(new DataGridViewTextBoxColumn { Name = "Reporter", HeaderText = "Reporter" });
            reportGrid.Columns.Add(new DataGridViewTextBoxColumn { Name = "Target", HeaderText = "Target" });
            reportGrid.Columns.Add(new DataGridViewTextBoxColumn { Name = "Reason", HeaderText = "Reason" });
            reportGrid.Columns.Add(new DataGridViewTextBoxColumn { Name = "Status", HeaderText = "Status", Width = 80 });
            reportGrid.Columns.Add(new DataGridViewTextBoxColumn { Name = "CreatedTime", HeaderText = "Created", Width = 120 });

            var reports = MockReportService.Instance.GetReportsForServer(server.Id);
            foreach (var r in reports)
            {
                reportGrid.Rows.Add(r.Id, r.Reporter, r.Target, r.Reason, r.Status.ToString(), r.CreatedTime.ToString("yyyy-MM-dd HH:mm"));
            }

            if (reports.Count == 0)
            {
                var emptyLabel = new Label { Text = "No reports found for this server.", Dock = DockStyle.Fill, TextAlign = ContentAlignment.MiddleCenter, ForeColor = Color.Gray, Font = new Font("Segoe UI", 11F) };
                form.Controls.Add(emptyLabel);
            }
            else
            {
                form.Controls.Add(reportGrid);
            }
            form.ShowDialog();
        }

        private static Label MakeLabel(string text, bool bold = false, Color? color = null)
        {
            return new Label
            {
                Text = text, AutoSize = true, Margin = new Padding(0, 0, 0, 6),
                Font = new Font("Segoe UI", 9.5F, bold ? FontStyle.Bold : FontStyle.Regular),
                ForeColor = color ?? Color.FromArgb(40, 40, 50),
            };
        }

        private static Button MakeButton(string text, Color bgColor, int width = 80)
        {
            var btn = new Button
            {
                Text = text, Width = width, Height = 30, FlatStyle = FlatStyle.Flat,
                BackColor = bgColor, ForeColor = Color.White,
                Font = new Font("Segoe UI", 8.5F, FontStyle.Bold),
                Margin = new Padding(0, 0, 8, 0), Cursor = Cursors.Hand,
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
