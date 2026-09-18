using UM.Admin.Models;
using UM.Admin.Services;

namespace UM.Admin.Controls
{
    public class ServerManagementControl : UserControl
    {
        private DataGridView _grid = null!;
        private TextBox _searchBox = null!;
        private ComboBox _statusFilter = null!;
        private Label _backendStatusLabel = null!;
        private readonly ServerAdminApiService _apiService = ServerAdminApiService.Instance;
        private readonly MockServerService _serverService = MockServerService.Instance;

        // Current servers displayed in grid
        private List<Server> _currentServers = new();

        public ServerManagementControl()
        {
            BackColor = Color.FromArgb(245, 245, 248);
            Padding = new Padding(24);
            DoubleBuffered = true;
            Dock = DockStyle.Fill;
            BuildUI();
            _ = LoadDataAsync();
        }

        private void BuildUI()
        {
            var topPanel = new Panel { Dock = DockStyle.Top, Height = 56 };

            // Search
            topPanel.Controls.Add(new Label { Text = "Search:", AutoSize = true, Location = new Point(0, 18), Font = new Font("Segoe UI", 9) });
            _searchBox = new TextBox { Location = new Point(55, 14), Width = 200, Font = new Font("Segoe UI", 9.5F), PlaceholderText = "Search servers..." };
            _searchBox.TextChanged += (s, e) => FilterData();
            topPanel.Controls.Add(_searchBox);

            // Status Filter
            topPanel.Controls.Add(new Label { Text = "Status:", AutoSize = true, Location = new Point(275, 18), Font = new Font("Segoe UI", 9) });
            _statusFilter = new ComboBox { Location = new Point(325, 14), Width = 135, DropDownStyle = ComboBoxStyle.DropDownList, Font = new Font("Segoe UI", 9.5F) };
            _statusFilter.Items.AddRange(new object[] { "All", "Active", "Blocked", "Pending Deletion" });
            _statusFilter.SelectedIndex = 0;
            _statusFilter.SelectedIndexChanged += (s, e) => FilterData();
            topPanel.Controls.Add(_statusFilter);

            // Refresh Button
            var refreshBtn = new Button
            {
                Text = "Refresh",
                Location = new Point(475, 13),
                Height = 30,
                Width = 80,
                Font = new Font("Segoe UI", 9, FontStyle.Regular),
                BackColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand
            };
            refreshBtn.FlatAppearance.BorderColor = Color.FromArgb(200, 200, 200);
            refreshBtn.Click += async (s, e) => await LoadDataAsync();
            topPanel.Controls.Add(refreshBtn);

            // + Add Server Button
            var addServerBtn = new Button
            {
                Text = "+ Add Server",
                Location = new Point(565, 13),
                Height = 30,
                Width = 115,
                Font = new Font("Segoe UI", 9, FontStyle.Bold),
                BackColor = Color.FromArgb(66, 133, 244),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand
            };
            addServerBtn.FlatAppearance.BorderSize = 0;
            addServerBtn.Click += (s, e) => ShowAddServerDialog();
            topPanel.Controls.Add(addServerBtn);

            // Backend connection indicator
            _backendStatusLabel = new Label
            {
                AutoSize = true,
                Location = new Point(700, 18),
                Font = new Font("Segoe UI", 8.5F, FontStyle.Bold),
                ForeColor = Color.FromArgb(120, 120, 130),
                Text = "● Checking Backend..."
            };
            topPanel.Controls.Add(_backendStatusLabel);

            Controls.Add(topPanel);

            // DataGridView
            _grid = new DataGridView();
            StyleGrid(_grid);
            _grid.Dock = DockStyle.Fill;
            _grid.CellFormatting += Grid_CellFormatting;
            _grid.CellContentClick += Grid_CellContentClick;
            _grid.CellDoubleClick += Grid_CellDoubleClick;

            _grid.AutoGenerateColumns = false;
            _grid.Columns.Add(new DataGridViewTextBoxColumn { Name = "Id", HeaderText = "ID", Width = 55 });
            _grid.Columns.Add(new DataGridViewTextBoxColumn { Name = "Name", HeaderText = "Server Name", Width = 180 });
            _grid.Columns.Add(new DataGridViewTextBoxColumn { Name = "Owner", HeaderText = "Owner", Width = 160 });
            _grid.Columns.Add(new DataGridViewTextBoxColumn { Name = "Members", HeaderText = "Members", Width = 80 });
            _grid.Columns.Add(new DataGridViewTextBoxColumn { Name = "Channels", HeaderText = "Channels", Width = 80 });
            _grid.Columns.Add(new DataGridViewTextBoxColumn { Name = "Storage", HeaderText = "Storage", Width = 85 });
            _grid.Columns.Add(new DataGridViewTextBoxColumn { Name = "Status", HeaderText = "Status", Width = 110 });
            _grid.Columns.Add(new DataGridViewTextBoxColumn { Name = "CreatedDate", HeaderText = "Created", Width = 95 });
            _grid.Columns.Add(new DataGridViewButtonColumn { Name = "Actions", HeaderText = "Actions", Text = "View", UseColumnTextForButtonValue = true, Width = 70 });

            Controls.Add(_grid);
            _grid.BringToFront();
        }

        public async Task LoadDataAsync()
        {
            _backendStatusLabel.Text = "● Fetching...";
            _backendStatusLabel.ForeColor = Color.FromArgb(120, 120, 130);

            try
            {
                _currentServers = await _apiService.GetAllServersAsync(includeDeleted: true);
                if (_apiService.IsBackendConnected)
                {
                    _backendStatusLabel.Text = "● Backend Connected (API Mode)";
                    _backendStatusLabel.ForeColor = Color.FromArgb(46, 125, 50);
                }
                else
                {
                    _backendStatusLabel.Text = "● Offline (Mock Fallback)";
                    _backendStatusLabel.ForeColor = Color.FromArgb(230, 124, 115);
                }
            }
            catch (Exception ex)
            {
                _backendStatusLabel.Text = "● API Error (Mock Fallback)";
                _backendStatusLabel.ForeColor = Color.FromArgb(198, 40, 40);
                _currentServers = _serverService.Servers;
                MessageBox.Show($"Unable to fetch from API: {ex.Message}\nUsing local data.", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }

            FilterData();
        }

        private void FilterData()
        {
            _grid.Rows.Clear();
            var search = _searchBox.Text.Trim().ToLower();
            var statusFilter = _statusFilter.SelectedItem?.ToString() ?? "All";

            foreach (var s in _currentServers)
            {
                bool matchSearch = string.IsNullOrEmpty(search) ||
                    s.Name.ToLower().Contains(search) ||
                    s.Owner.ToLower().Contains(search) ||
                    s.Id.ToString().Contains(search);

                string statusText = s.Status switch
                {
                    ServerStatus.Active => "Active",
                    ServerStatus.Blocked => "Blocked",
                    ServerStatus.PendingDeletion => "Pending Deletion",
                    _ => s.Status.ToString()
                };

                bool matchStatus = statusFilter == "All" || statusText == statusFilter;

                if (matchSearch && matchStatus)
                {
                    _grid.Rows.Add(s.Id, s.Name, s.Owner, s.Members, s.Channels, s.StorageUsed, statusText, s.CreatedDate.ToString("yyyy-MM-dd"));
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
                    if (status == "Active")
                    {
                        e.CellStyle.ForeColor = Color.FromArgb(46, 125, 50);
                    }
                    else if (status == "Blocked" || status == "Locked")
                    {
                        e.CellStyle.ForeColor = Color.FromArgb(198, 40, 40);
                    }
                    else if (status == "Pending Deletion")
                    {
                        e.CellStyle.ForeColor = Color.FromArgb(230, 81, 0); // Orange
                    }
                }
            }
        }

        private async void Grid_CellContentClick(object? sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0 && _grid.Columns[e.ColumnIndex].Name == "Actions")
            {
                if (_grid.Rows[e.RowIndex].Tag is Server server)
                {
                    await OpenServerDetailsAsync(server.Id);
                }
            }
        }

        private async void Grid_CellDoubleClick(object? sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0 && _grid.Rows[e.RowIndex].Tag is Server server)
            {
                await OpenServerDetailsAsync(server.Id);
            }
        }

        #region Add Server Dialog

        private void ShowAddServerDialog()
        {
            using var form = new Form
            {
                Text = "Create New Server",
                Size = new Size(460, 260),
                StartPosition = FormStartPosition.CenterParent,
                FormBorderStyle = FormBorderStyle.FixedDialog,
                MaximizeBox = false,
                MinimizeBox = false,
                BackColor = Color.White,
                Font = new Font("Segoe UI", 9.5F)
            };

            var panel = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                FlowDirection = FlowDirection.TopDown,
                Padding = new Padding(24, 20, 24, 10),
                WrapContents = false
            };

            panel.Controls.Add(MakeLabel("Server Name *", true));
            var nameBox = new TextBox { Width = 390, Font = new Font("Segoe UI", 10F), PlaceholderText = "Enter server name (1-100 characters)" };
            panel.Controls.Add(nameBox);

            panel.Controls.Add(new Panel { Height = 10 });
            panel.Controls.Add(MakeLabel("Icon File ID (Optional)"));
            var iconBox = new TextBox { Width = 390, Font = new Font("Segoe UI", 10F), PlaceholderText = "e.g. 1 (Leave empty for default)" };
            panel.Controls.Add(iconBox);

            var btnPanel = new Panel { Dock = DockStyle.Bottom, Height = 56, BackColor = Color.FromArgb(248, 249, 250) };
            var createBtn = MakeButton("Create", Color.FromArgb(66, 133, 244), 90);
            createBtn.Location = new Point(245, 12);
            var cancelBtn = MakeButton("Cancel", Color.FromArgb(160, 160, 160), 80);
            cancelBtn.Location = new Point(345, 12);

            cancelBtn.Click += (s, e) => form.Close();
            createBtn.Click += async (s, e) =>
            {
                var name = nameBox.Text.Trim();
                if (string.IsNullOrWhiteSpace(name))
                {
                    MessageBox.Show("Server name cannot be empty.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    nameBox.Focus();
                    return;
                }

                if (name.Length > 100)
                {
                    MessageBox.Show("Server name must not exceed 100 characters.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    nameBox.Focus();
                    return;
                }

                long? iconId = null;
                if (!string.IsNullOrWhiteSpace(iconBox.Text))
                {
                    if (long.TryParse(iconBox.Text.Trim(), out var parsedId))
                    {
                        iconId = parsedId;
                    }
                    else
                    {
                        MessageBox.Show("Icon File ID must be a valid integer number.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        iconBox.Focus();
                        return;
                    }
                }

                try
                {
                    createBtn.Enabled = false;
                    createBtn.Text = "Creating...";
                    await _apiService.CreateServerAsync(name, iconId);
                    MockAdminService.Instance.AddActivity("Created server", name);

                    MessageBox.Show($"Server '{name}' created successfully!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    form.Close();
                    await LoadDataAsync();
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Failed to create server: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                finally
                {
                    createBtn.Enabled = true;
                    createBtn.Text = "Create";
                }
            };

            btnPanel.Controls.Add(createBtn);
            btnPanel.Controls.Add(cancelBtn);

            form.Controls.Add(panel);
            form.Controls.Add(btnPanel);
            form.ShowDialog();
        }

        #endregion

        #region Server Details Dialog with Tabs (Overview, Categories, Channels, Roles, Settings)

        private async Task OpenServerDetailsAsync(long serverId)
        {
            var server = await _apiService.GetServerDetailAsync(serverId);
            if (server == null)
            {
                MessageBox.Show("Server not found or has been deleted.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            using var form = new Form
            {
                Text = $"Server Details - {server.Name} (ID: {server.Id})",
                Size = new Size(880, 680),
                StartPosition = FormStartPosition.CenterParent,
                FormBorderStyle = FormBorderStyle.FixedDialog,
                MaximizeBox = true,
                MinimizeBox = false,
                BackColor = Color.White,
                Font = new Font("Segoe UI", 9.5F)
            };

            // Top Summary Card
            var topSummary = new Panel { Dock = DockStyle.Top, Height = 90, Padding = new Padding(20, 14, 20, 10), BackColor = Color.FromArgb(248, 249, 252) };
            var titleLabel = new Label
            {
                Text = server.Name,
                Font = new Font("Segoe UI", 14F, FontStyle.Bold),
                ForeColor = Color.FromArgb(30, 30, 40),
                AutoSize = true,
                Location = new Point(20, 12)
            };
            var statusBadge = new Label
            {
                Text = server.Status switch { ServerStatus.Active => "● ACTIVE", ServerStatus.Blocked => "● BLOCKED", ServerStatus.PendingDeletion => "● PENDING DELETION", _ => server.Status.ToString().ToUpper() },
                Font = new Font("Segoe UI", 9F, FontStyle.Bold),
                ForeColor = server.Status == ServerStatus.Active ? Color.FromArgb(46, 125, 50) : (server.Status == ServerStatus.PendingDeletion ? Color.FromArgb(230, 81, 0) : Color.FromArgb(198, 40, 40)),
                AutoSize = true,
                Location = new Point(20 + titleLabel.PreferredWidth + 15, 16)
            };
            var metaLabel = new Label
            {
                Text = $"Server ID: {server.Id}  |  Owner: {server.Owner} (Code: {(string.IsNullOrEmpty(server.OwnerPublicCode) ? $"USR{server.OwnerId}" : server.OwnerPublicCode)})  |  Members: {server.Members}  |  Channels: {server.Channels}",
                Font = new Font("Segoe UI", 9F),
                ForeColor = Color.FromArgb(100, 100, 110),
                AutoSize = true,
                Location = new Point(20, 44)
            };
            if (server.Status == ServerStatus.PendingDeletion && server.ScheduledDeleteAt.HasValue)
            {
                metaLabel.Text += $"  |  Scheduled Permanent Deletion: {server.ScheduledDeleteAt.Value:yyyy-MM-dd HH:mm}";
            }

            topSummary.Controls.Add(titleLabel);
            topSummary.Controls.Add(statusBadge);
            topSummary.Controls.Add(metaLabel);

            // TabControl
            var tabControl = new TabControl
            {
                Dock = DockStyle.Fill,
                Font = new Font("Segoe UI", 9.5F),
                Padding = new Point(14, 8)
            };

            // 1. Tab Overview
            var tabOverview = new TabPage("Overview");
            BuildOverviewTab(tabOverview, server, form);
            tabControl.TabPages.Add(tabOverview);

            // 2. Tab Categories
            var tabCategories = new TabPage("Categories");
            BuildCategoriesTab(tabCategories, server);
            tabControl.TabPages.Add(tabCategories);

            // 3. Tab Channels
            var tabChannels = new TabPage("Channels");
            BuildChannelsTab(tabChannels, server);
            tabControl.TabPages.Add(tabChannels);

            // 4. Tab Server Roles
            var tabRoles = new TabPage("Server Roles");
            BuildRolesTab(tabRoles, server);
            tabControl.TabPages.Add(tabRoles);

            // 5. Tab Settings
            var tabSettings = new TabPage("Server Settings");
            BuildSettingsTab(tabSettings, server, form);
            tabControl.TabPages.Add(tabSettings);

            form.Controls.Add(tabControl);
            form.Controls.Add(topSummary);

            form.ShowDialog();
        }

        #endregion

        #region TAB 1: OVERVIEW & ACTIONS

        private void BuildOverviewTab(TabPage tab, Server server, Form parentForm)
        {
            tab.BackColor = Color.White;
            var layout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 2,
                RowCount = 1,
                Padding = new Padding(20)
            };
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 55F));
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 45F));

            // Left: Information Panel
            var infoPanel = new Panel { Dock = DockStyle.Fill, AutoScroll = true };
            var infoFlow = new FlowLayoutPanel { Dock = DockStyle.Fill, FlowDirection = FlowDirection.TopDown, AutoScroll = true, WrapContents = false };

            infoFlow.Controls.Add(MakeSectionHeader("Server Information"));
            infoFlow.Controls.Add(MakeInfoRow("Server ID:", server.Id.ToString()));
            infoFlow.Controls.Add(MakeInfoRow("Server Name:", server.Name));
            infoFlow.Controls.Add(MakeInfoRow("Owner Name:", server.Owner));
            infoFlow.Controls.Add(MakeInfoRow("Owner ID / Code:", $"{server.OwnerId} / {(string.IsNullOrEmpty(server.OwnerPublicCode) ? "N/A" : server.OwnerPublicCode)}"));
            infoFlow.Controls.Add(MakeInfoRow("Created Date:", server.CreatedDate.ToString("yyyy-MM-dd HH:mm:ss")));
            infoFlow.Controls.Add(MakeInfoRow("Updated Date:", server.UpdatedDate?.ToString("yyyy-MM-dd HH:mm:ss") ?? "N/A"));
            infoFlow.Controls.Add(MakeInfoRow("Active Members:", server.Members.ToString()));
            infoFlow.Controls.Add(MakeInfoRow("Total Channels:", server.Channels.ToString()));
            infoFlow.Controls.Add(MakeInfoRow("Storage Used (Read-only):", server.StorageUsed));

            string statusStr = server.Status == ServerStatus.PendingDeletion 
                ? "Pending Deletion (Scheduled 14 days)" 
                : (server.Status == ServerStatus.Blocked ? "Blocked / Locked" : "Active");
            infoFlow.Controls.Add(MakeInfoRow("Current Status:", statusStr, server.Status == ServerStatus.Active ? Color.FromArgb(46, 125, 50) : Color.FromArgb(198, 40, 40)));

            if (server.Status == ServerStatus.PendingDeletion && server.ScheduledDeleteAt.HasValue)
            {
                infoFlow.Controls.Add(MakeInfoRow("Scheduled Deletion:", $"{server.ScheduledDeleteAt.Value:yyyy-MM-dd HH:mm:ss} (Auto permanent delete after 14 days)", Color.FromArgb(230, 81, 0)));
            }

            infoPanel.Controls.Add(infoFlow);
            layout.Controls.Add(infoPanel, 0, 0);

            // Right: Actions Panel
            var actionsPanel = new Panel { Dock = DockStyle.Fill, Padding = new Padding(15, 0, 0, 0) };
            var actionsFlow = new FlowLayoutPanel { Dock = DockStyle.Fill, FlowDirection = FlowDirection.TopDown, WrapContents = false };

            actionsFlow.Controls.Add(MakeSectionHeader("Quick Actions"));

            // Edit Server Button
            var editBtn = MakeActionButton("✎  Edit Server Information", Color.FromArgb(66, 133, 244));
            editBtn.Click += (s, e) => ShowEditServerDialog(server, parentForm);
            actionsFlow.Controls.Add(editBtn);

            // Transfer Ownership Button
            var transferBtn = MakeActionButton("⇄  Transfer Ownership", Color.FromArgb(103, 58, 183));
            transferBtn.Click += (s, e) => ShowTransferOwnershipDialog(server, parentForm);
            actionsFlow.Controls.Add(transferBtn);

            // Block / Unblock Button
            if (server.Status == ServerStatus.Blocked || server.Status == ServerStatus.Locked)
            {
                var unblockBtn = MakeActionButton("🔓  Unblock / Unlock Server", Color.FromArgb(46, 125, 50));
                unblockBtn.Click += async (s, e) =>
                {
                    if (MessageBox.Show($"Are you sure you want to UNBLOCK server '{server.Name}'?", "Confirm Unblock", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                    {
                        await _apiService.SetServerLockAsync(server.Id, false);
                        MockAdminService.Instance.AddActivity("Unlocked server", server.Name);
                        MessageBox.Show("Server unblocked successfully!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        parentForm.Close();
                        await LoadDataAsync();
                    }
                };
                actionsFlow.Controls.Add(unblockBtn);
            }
            else
            {
                var blockBtn = MakeActionButton("🔒  Block / Lock Server", Color.FromArgb(198, 40, 40));
                blockBtn.Click += async (s, e) =>
                {
                    if (MessageBox.Show($"Are you sure you want to BLOCK server '{server.Name}'?\n\nUsers will not be able to access or send messages until unblocked.", "Confirm Block", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == DialogResult.Yes)
                    {
                        await _apiService.SetServerLockAsync(server.Id, true);
                        MockAdminService.Instance.AddActivity("Locked server", server.Name);
                        MessageBox.Show("Server blocked successfully!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        parentForm.Close();
                        await LoadDataAsync();
                    }
                };
                actionsFlow.Controls.Add(blockBtn);
            }

            // View Members Button
            var membersBtn = MakeActionButton("👥  View Members List", Color.FromArgb(52, 168, 83));
            membersBtn.Click += (s, e) => ShowServerMembers(server);
            actionsFlow.Controls.Add(membersBtn);

            // View Reports Button
            var reportsBtn = MakeActionButton("⚠️  View Server Reports", Color.FromArgb(242, 142, 43));
            reportsBtn.Click += (s, e) => ShowServerReports(server);
            actionsFlow.Controls.Add(reportsBtn);

            // Delete Server Button (Soft-delete / Delay delete 14 days)
            var deleteBtn = MakeActionButton("🗑  Delete Server (14-Day Delay)", Color.FromArgb(183, 28, 28));
            deleteBtn.Click += async (s, e) =>
            {
                var result = MessageBox.Show(
                    "Are you sure you want to delete this server?\n\nThis server will be hidden immediately and permanently deleted after 14 days if it is not restored.",
                    "Delete Server Confirmation",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Warning);

                if (result == DialogResult.Yes)
                {
                    try
                    {
                        deleteBtn.Enabled = false;
                        await _apiService.DeleteServerAsync(server.Id);
                        MockAdminService.Instance.AddActivity("Scheduled server deletion", server.Name);

                        MessageBox.Show(
                            $"Server '{server.Name}' has been scheduled for deletion.\nIt is now hidden and will be permanently deleted after 14 days.",
                            "Server Scheduled for Deletion",
                            MessageBoxButtons.OK,
                            MessageBoxIcon.Information);

                        parentForm.Close();
                        await LoadDataAsync();
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Failed to delete server: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                    finally
                    {
                        deleteBtn.Enabled = true;
                    }
                }
            };
            actionsFlow.Controls.Add(deleteBtn);

            actionsPanel.Controls.Add(actionsFlow);
            layout.Controls.Add(actionsPanel, 1, 0);

            tab.Controls.Add(layout);
        }

        private void ShowEditServerDialog(Server server, Form parentDetailsForm)
        {
            using var form = new Form
            {
                Text = $"Edit Server - {server.Name}",
                Size = new Size(460, 260),
                StartPosition = FormStartPosition.CenterParent,
                FormBorderStyle = FormBorderStyle.FixedDialog,
                MaximizeBox = false, MinimizeBox = false,
                BackColor = Color.White, Font = new Font("Segoe UI", 9.5F)
            };

            var panel = new FlowLayoutPanel { Dock = DockStyle.Fill, FlowDirection = FlowDirection.TopDown, Padding = new Padding(24, 20, 24, 10), WrapContents = false };
            panel.Controls.Add(MakeLabel("Server Name *", true));
            var nameBox = new TextBox { Width = 390, Font = new Font("Segoe UI", 10F), Text = server.Name };
            panel.Controls.Add(nameBox);

            panel.Controls.Add(new Panel { Height = 10 });
            panel.Controls.Add(MakeLabel("Icon File ID (Optional)"));
            var iconBox = new TextBox { Width = 390, Font = new Font("Segoe UI", 10F), Text = server.IconFileId?.ToString() ?? "" };
            panel.Controls.Add(iconBox);

            var btnPanel = new Panel { Dock = DockStyle.Bottom, Height = 56, BackColor = Color.FromArgb(248, 249, 250) };
            var saveBtn = MakeButton("Save", Color.FromArgb(66, 133, 244), 90);
            saveBtn.Location = new Point(255, 12);
            var cancelBtn = MakeButton("Cancel", Color.FromArgb(160, 160, 160), 80);
            cancelBtn.Location = new Point(355, 12);
            cancelBtn.Click += (s, e) => form.Close();

            saveBtn.Click += async (s, e) =>
            {
                var newName = nameBox.Text.Trim();
                if (string.IsNullOrWhiteSpace(newName))
                {
                    MessageBox.Show("Server name cannot be empty.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }
                long? newIconId = null;
                if (!string.IsNullOrWhiteSpace(iconBox.Text) && long.TryParse(iconBox.Text.Trim(), out var parsedIcon))
                {
                    newIconId = parsedIcon;
                }

                try
                {
                    saveBtn.Enabled = false;
                    await _apiService.UpdateServerAsync(server.Id, newName, newIconId);
                    MockAdminService.Instance.AddActivity("Updated server info", newName);
                    MessageBox.Show("Server updated successfully!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    form.Close();
                    parentDetailsForm.Close();
                    await LoadDataAsync();
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Failed to update server: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                finally
                {
                    saveBtn.Enabled = true;
                }
            };

            btnPanel.Controls.Add(saveBtn);
            btnPanel.Controls.Add(cancelBtn);
            form.Controls.Add(panel);
            form.Controls.Add(btnPanel);
            form.ShowDialog();
        }

        private void ShowTransferOwnershipDialog(Server server, Form parentDetailsForm)
        {
            using var form = new Form
            {
                Text = $"Transfer Ownership - {server.Name}",
                Size = new Size(500, 270),
                StartPosition = FormStartPosition.CenterParent,
                FormBorderStyle = FormBorderStyle.FixedDialog,
                MaximizeBox = false, MinimizeBox = false,
                BackColor = Color.White, Font = new Font("Segoe UI", 9.5F)
            };

            var panel = new FlowLayoutPanel { Dock = DockStyle.Fill, FlowDirection = FlowDirection.TopDown, Padding = new Padding(24, 20, 24, 10), WrapContents = false };
            panel.Controls.Add(MakeLabel($"Current Owner: {server.Owner} (ID: {server.OwnerId})", true, Color.FromArgb(70, 70, 80)));
            panel.Controls.Add(new Panel { Height = 10 });
            panel.Controls.Add(MakeLabel("New Owner User ID *", true));
            var newOwnerBox = new TextBox { Width = 430, Font = new Font("Segoe UI", 10F), PlaceholderText = "Enter target user ID (must be active member)" };
            panel.Controls.Add(newOwnerBox);

            var note = new Label
            {
                Text = "Note: Transferring ownership gives full control of this server to the new owner.\nThis action takes effect immediately.",
                Font = new Font("Segoe UI", 8.5F, FontStyle.Italic),
                ForeColor = Color.FromArgb(120, 120, 130),
                AutoSize = true,
                Margin = new Padding(0, 8, 0, 0)
            };
            panel.Controls.Add(note);

            var btnPanel = new Panel { Dock = DockStyle.Bottom, Height = 56, BackColor = Color.FromArgb(248, 249, 250) };
            var transferBtn = MakeButton("Transfer", Color.FromArgb(103, 58, 183), 100);
            transferBtn.Location = new Point(285, 12);
            var cancelBtn = MakeButton("Cancel", Color.FromArgb(160, 160, 160), 80);
            cancelBtn.Location = new Point(395, 12);
            cancelBtn.Click += (s, e) => form.Close();

            transferBtn.Click += async (s, e) =>
            {
                if (!long.TryParse(newOwnerBox.Text.Trim(), out var newOwnerId) || newOwnerId <= 0)
                {
                    MessageBox.Show("Please enter a valid User ID.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                if (newOwnerId == server.OwnerId)
                {
                    MessageBox.Show("Target user is already the owner of this server.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                if (MessageBox.Show($"Are you sure you want to transfer ownership of '{server.Name}' to User ID #{newOwnerId}?", "Confirm Transfer", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == DialogResult.Yes)
                {
                    try
                    {
                        transferBtn.Enabled = false;
                        await _apiService.TransferOwnershipAsync(server.Id, newOwnerId, $"User #{newOwnerId}");
                        MockAdminService.Instance.AddActivity("Transferred server ownership", $"{server.Name} to User #{newOwnerId}");
                        MessageBox.Show("Ownership transferred successfully!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        form.Close();
                        parentDetailsForm.Close();
                        await LoadDataAsync();
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Failed to transfer ownership: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                    finally
                    {
                        transferBtn.Enabled = true;
                    }
                }
            };

            btnPanel.Controls.Add(transferBtn);
            btnPanel.Controls.Add(cancelBtn);
            form.Controls.Add(panel);
            form.Controls.Add(btnPanel);
            form.ShowDialog();
        }

        #endregion

        #region TAB 2: CATEGORY MANAGEMENT

        private void BuildCategoriesTab(TabPage tab, Server server)
        {
            tab.BackColor = Color.White;
            tab.Padding = new Padding(15);

            var topPanel = new Panel { Dock = DockStyle.Top, Height = 42 };
            var addCatBtn = MakeButton("+ Add Category", Color.FromArgb(66, 133, 244), 125);
            addCatBtn.Location = new Point(0, 4);

            topPanel.Controls.Add(addCatBtn);
            tab.Controls.Add(topPanel);

            var catGrid = new DataGridView();
            StyleGrid(catGrid);
            catGrid.Dock = DockStyle.Fill;
            catGrid.AutoGenerateColumns = false;
            catGrid.Columns.Add(new DataGridViewTextBoxColumn { Name = "Id", HeaderText = "ID", Width = 60 });
            catGrid.Columns.Add(new DataGridViewTextBoxColumn { Name = "Name", HeaderText = "Category Name", Width = 250 });
            catGrid.Columns.Add(new DataGridViewTextBoxColumn { Name = "Position", HeaderText = "Position", Width = 80 });
            catGrid.Columns.Add(new DataGridViewTextBoxColumn { Name = "ChannelsCount", HeaderText = "Channels", Width = 80 });
            catGrid.Columns.Add(new DataGridViewTextBoxColumn { Name = "Created", HeaderText = "Created Date", Width = 110 });
            catGrid.Columns.Add(new DataGridViewButtonColumn { Name = "Edit", HeaderText = "", Text = "Edit", UseColumnTextForButtonValue = true, Width = 65 });
            catGrid.Columns.Add(new DataGridViewButtonColumn { Name = "Delete", HeaderText = "", Text = "Delete", UseColumnTextForButtonValue = true, Width = 65 });

            async Task RefreshCategories()
            {
                catGrid.Rows.Clear();
                var categories = await _apiService.GetCategoriesAsync(server.Id);
                server.Categories = categories;
                foreach (var c in categories)
                {
                    catGrid.Rows.Add(c.Id, c.Name, c.Position, c.Channels?.Count ?? 0, c.CreatedAt.ToString("yyyy-MM-dd"));
                    catGrid.Rows[catGrid.Rows.Count - 1].Tag = c;
                }
            }

            addCatBtn.Click += async (s, e) =>
            {
                using var dialog = new Form
                {
                    Text = "Add Category",
                    Size = new Size(400, 220),
                    StartPosition = FormStartPosition.CenterParent,
                    FormBorderStyle = FormBorderStyle.FixedDialog,
                    MaximizeBox = false, MinimizeBox = false,
                    BackColor = Color.White, Font = new Font("Segoe UI", 9.5F)
                };

                var p = new FlowLayoutPanel { Dock = DockStyle.Fill, FlowDirection = FlowDirection.TopDown, Padding = new Padding(20), WrapContents = false };
                p.Controls.Add(MakeLabel("Category Name *", true));
                var catNameBox = new TextBox { Width = 340, Font = new Font("Segoe UI", 10F) };
                p.Controls.Add(catNameBox);

                p.Controls.Add(new Panel { Height = 8 });
                p.Controls.Add(MakeLabel("Position"));
                var posBox = new TextBox { Width = 100, Font = new Font("Segoe UI", 10F), Text = (server.Categories.Count).ToString() };
                p.Controls.Add(posBox);

                var bPanel = new Panel { Dock = DockStyle.Bottom, Height = 48, BackColor = Color.FromArgb(248, 249, 250) };
                var okBtn = MakeButton("Add", Color.FromArgb(66, 133, 244), 75);
                okBtn.Location = new Point(215, 8);
                var cBtn = MakeButton("Cancel", Color.FromArgb(160, 160, 160), 75);
                cBtn.Location = new Point(300, 8);
                cBtn.Click += (s2, e2) => dialog.Close();

                okBtn.Click += async (s2, e2) =>
                {
                    var catName = catNameBox.Text.Trim();
                    if (string.IsNullOrWhiteSpace(catName))
                    {
                        MessageBox.Show("Category name cannot be empty.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return;
                    }
                    int.TryParse(posBox.Text.Trim(), out var pos);
                    try
                    {
                        await _apiService.CreateCategoryAsync(server.Id, catName, pos);
                        dialog.Close();
                        await RefreshCategories();
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Failed to create category: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                };

                bPanel.Controls.Add(okBtn);
                bPanel.Controls.Add(cBtn);
                dialog.Controls.Add(p);
                dialog.Controls.Add(bPanel);
                dialog.ShowDialog();
            };

            catGrid.CellContentClick += async (s, e) =>
            {
                if (e.RowIndex < 0) return;
                var cat = catGrid.Rows[e.RowIndex].Tag as ServerCategory;
                if (cat == null) return;

                if (catGrid.Columns[e.ColumnIndex].Name == "Edit")
                {
                    using var editDialog = new Form
                    {
                        Text = $"Edit Category - {cat.Name}",
                        Size = new Size(400, 220),
                        StartPosition = FormStartPosition.CenterParent,
                        FormBorderStyle = FormBorderStyle.FixedDialog,
                        MaximizeBox = false, MinimizeBox = false,
                        BackColor = Color.White, Font = new Font("Segoe UI", 9.5F)
                    };
                    var p = new FlowLayoutPanel { Dock = DockStyle.Fill, FlowDirection = FlowDirection.TopDown, Padding = new Padding(20), WrapContents = false };
                    p.Controls.Add(MakeLabel("Category Name *", true));
                    var nameBox = new TextBox { Width = 340, Font = new Font("Segoe UI", 10F), Text = cat.Name };
                    p.Controls.Add(nameBox);
                    p.Controls.Add(new Panel { Height = 8 });
                    p.Controls.Add(MakeLabel("Position"));
                    var posBox = new TextBox { Width = 100, Font = new Font("Segoe UI", 10F), Text = cat.Position.ToString() };
                    p.Controls.Add(posBox);

                    var bPanel = new Panel { Dock = DockStyle.Bottom, Height = 48, BackColor = Color.FromArgb(248, 249, 250) };
                    var saveBtn = MakeButton("Save", Color.FromArgb(66, 133, 244), 75);
                    saveBtn.Location = new Point(215, 8);
                    var cBtn = MakeButton("Cancel", Color.FromArgb(160, 160, 160), 75);
                    cBtn.Location = new Point(300, 8);
                    cBtn.Click += (s2, e2) => editDialog.Close();

                    saveBtn.Click += async (s2, e2) =>
                    {
                        var n = nameBox.Text.Trim();
                        if (string.IsNullOrWhiteSpace(n)) return;
                        int.TryParse(posBox.Text.Trim(), out var pos);
                        await _apiService.UpdateCategoryAsync(cat.Id, n, pos);
                        editDialog.Close();
                        await RefreshCategories();
                    };
                    bPanel.Controls.Add(saveBtn);
                    bPanel.Controls.Add(cBtn);
                    editDialog.Controls.Add(p);
                    editDialog.Controls.Add(bPanel);
                    editDialog.ShowDialog();
                }
                else if (catGrid.Columns[e.ColumnIndex].Name == "Delete")
                {
                    if (MessageBox.Show($"Are you sure you want to delete category '{cat.Name}' and its channels?", "Confirm Delete", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == DialogResult.Yes)
                    {
                        await _apiService.DeleteCategoryAsync(cat.Id);
                        await RefreshCategories();
                    }
                }
            };

            tab.Controls.Add(catGrid);
            catGrid.BringToFront();
            _ = RefreshCategories();
        }

        #endregion

        #region TAB 3: CHANNEL MANAGEMENT

        private void BuildChannelsTab(TabPage tab, Server server)
        {
            tab.BackColor = Color.White;
            tab.Padding = new Padding(15);

            var topPanel = new Panel { Dock = DockStyle.Top, Height = 42 };
            var addChannelBtn = MakeButton("+ Add Channel", Color.FromArgb(66, 133, 244), 120);
            addChannelBtn.Location = new Point(0, 4);
            topPanel.Controls.Add(addChannelBtn);
            tab.Controls.Add(topPanel);

            var chGrid = new DataGridView();
            StyleGrid(chGrid);
            chGrid.Dock = DockStyle.Fill;
            chGrid.AutoGenerateColumns = false;
            chGrid.Columns.Add(new DataGridViewTextBoxColumn { Name = "Id", HeaderText = "ID", Width = 60 });
            chGrid.Columns.Add(new DataGridViewTextBoxColumn { Name = "Name", HeaderText = "Channel Name", Width = 200 });
            chGrid.Columns.Add(new DataGridViewTextBoxColumn { Name = "Type", HeaderText = "Type", Width = 80 });
            chGrid.Columns.Add(new DataGridViewTextBoxColumn { Name = "Category", HeaderText = "Category", Width = 160 });
            chGrid.Columns.Add(new DataGridViewTextBoxColumn { Name = "Position", HeaderText = "Position", Width = 70 });
            chGrid.Columns.Add(new DataGridViewTextBoxColumn { Name = "Created", HeaderText = "Created Date", Width = 110 });
            chGrid.Columns.Add(new DataGridViewButtonColumn { Name = "Edit", HeaderText = "", Text = "Edit", UseColumnTextForButtonValue = true, Width = 65 });
            chGrid.Columns.Add(new DataGridViewButtonColumn { Name = "Delete", HeaderText = "", Text = "Delete", UseColumnTextForButtonValue = true, Width = 65 });

            async Task RefreshChannels()
            {
                chGrid.Rows.Clear();
                var channels = await _apiService.GetChannelsAsync(server.Id);
                var categories = await _apiService.GetCategoriesAsync(server.Id);

                foreach (var ch in channels)
                {
                    string catName = "No Category";
                    if (ch.CategoryId.HasValue)
                    {
                        var cat = categories.FirstOrDefault(c => c.Id == ch.CategoryId.Value);
                        if (cat != null) catName = cat.Name;
                    }
                    chGrid.Rows.Add(ch.Id, (ch.Type == 2 ? "🔊 " : "# ") + ch.Name, ch.TypeName, catName, ch.Position, ch.CreatedAt.ToString("yyyy-MM-dd"));
                    chGrid.Rows[chGrid.Rows.Count - 1].Tag = ch;
                }
            }

            addChannelBtn.Click += async (s, e) =>
            {
                var categories = await _apiService.GetCategoriesAsync(server.Id);
                using var dialog = new Form
                {
                    Text = "Add Channel",
                    Size = new Size(420, 310),
                    StartPosition = FormStartPosition.CenterParent,
                    FormBorderStyle = FormBorderStyle.FixedDialog,
                    MaximizeBox = false, MinimizeBox = false,
                    BackColor = Color.White, Font = new Font("Segoe UI", 9.5F)
                };

                var p = new FlowLayoutPanel { Dock = DockStyle.Fill, FlowDirection = FlowDirection.TopDown, Padding = new Padding(20), WrapContents = false };
                p.Controls.Add(MakeLabel("Channel Name *", true));
                var chNameBox = new TextBox { Width = 360, Font = new Font("Segoe UI", 10F), PlaceholderText = "e.g. announcements" };
                p.Controls.Add(chNameBox);

                p.Controls.Add(new Panel { Height = 8 });
                p.Controls.Add(MakeLabel("Channel Type"));
                var typeCombo = new ComboBox { Width = 360, DropDownStyle = ComboBoxStyle.DropDownList, Font = new Font("Segoe UI", 9.5F) };
                typeCombo.Items.AddRange(new object[] { "Text Channel", "Voice Channel" });
                typeCombo.SelectedIndex = 0;
                p.Controls.Add(typeCombo);

                p.Controls.Add(new Panel { Height = 8 });
                p.Controls.Add(MakeLabel("Category"));
                var catCombo = new ComboBox { Width = 360, DropDownStyle = ComboBoxStyle.DropDownList, Font = new Font("Segoe UI", 9.5F) };
                catCombo.Items.Add(new ComboBoxItem { Text = "-- None (Root Level) --", Value = null });
                foreach (var c in categories)
                {
                    catCombo.Items.Add(new ComboBoxItem { Text = c.Name, Value = c.Id });
                }
                catCombo.SelectedIndex = 0;
                p.Controls.Add(catCombo);

                var bPanel = new Panel { Dock = DockStyle.Bottom, Height = 48, BackColor = Color.FromArgb(248, 249, 250) };
                var okBtn = MakeButton("Add", Color.FromArgb(66, 133, 244), 75);
                okBtn.Location = new Point(235, 8);
                var cBtn = MakeButton("Cancel", Color.FromArgb(160, 160, 160), 75);
                cBtn.Location = new Point(320, 8);
                cBtn.Click += (s2, e2) => dialog.Close();

                okBtn.Click += async (s2, e2) =>
                {
                    var chName = chNameBox.Text.Trim().ToLower().Replace(" ", "-");
                    if (string.IsNullOrWhiteSpace(chName))
                    {
                        MessageBox.Show("Channel name cannot be empty.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return;
                    }
                    byte chType = (byte)(typeCombo.SelectedIndex == 1 ? 2 : 1);
                    long? selectedCatId = (catCombo.SelectedItem as ComboBoxItem)?.Value as long?;

                    try
                    {
                        await _apiService.CreateChannelAsync(server.Id, chName, selectedCatId, chType);
                        dialog.Close();
                        await RefreshChannels();
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Failed to create channel: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                };

                bPanel.Controls.Add(okBtn);
                bPanel.Controls.Add(cBtn);
                dialog.Controls.Add(p);
                dialog.Controls.Add(bPanel);
                dialog.ShowDialog();
            };

            chGrid.CellContentClick += async (s, e) =>
            {
                if (e.RowIndex < 0) return;
                var ch = chGrid.Rows[e.RowIndex].Tag as ServerChannel;
                if (ch == null) return;

                if (chGrid.Columns[e.ColumnIndex].Name == "Edit")
                {
                    var categories = await _apiService.GetCategoriesAsync(server.Id);
                    using var editDialog = new Form
                    {
                        Text = $"Edit Channel - {ch.Name}",
                        Size = new Size(400, 240),
                        StartPosition = FormStartPosition.CenterParent,
                        FormBorderStyle = FormBorderStyle.FixedDialog,
                        MaximizeBox = false, MinimizeBox = false,
                        BackColor = Color.White, Font = new Font("Segoe UI", 9.5F)
                    };
                    var p = new FlowLayoutPanel { Dock = DockStyle.Fill, FlowDirection = FlowDirection.TopDown, Padding = new Padding(20), WrapContents = false };
                    p.Controls.Add(MakeLabel("Channel Name *", true));
                    var nameBox = new TextBox { Width = 340, Font = new Font("Segoe UI", 10F), Text = ch.Name };
                    p.Controls.Add(nameBox);

                    p.Controls.Add(new Panel { Height = 8 });
                    p.Controls.Add(MakeLabel("Category"));
                    var catCombo = new ComboBox { Width = 340, DropDownStyle = ComboBoxStyle.DropDownList, Font = new Font("Segoe UI", 9.5F) };
                    catCombo.Items.Add(new ComboBoxItem { Text = "-- None --", Value = null });
                    int selectIdx = 0;
                    for (int i = 0; i < categories.Count; i++)
                    {
                        catCombo.Items.Add(new ComboBoxItem { Text = categories[i].Name, Value = categories[i].Id });
                        if (ch.CategoryId == categories[i].Id) selectIdx = i + 1;
                    }
                    catCombo.SelectedIndex = selectIdx;
                    p.Controls.Add(catCombo);

                    var bPanel = new Panel { Dock = DockStyle.Bottom, Height = 48, BackColor = Color.FromArgb(248, 249, 250) };
                    var saveBtn = MakeButton("Save", Color.FromArgb(66, 133, 244), 75);
                    saveBtn.Location = new Point(215, 8);
                    var cBtn = MakeButton("Cancel", Color.FromArgb(160, 160, 160), 75);
                    cBtn.Location = new Point(300, 8);
                    cBtn.Click += (s2, e2) => editDialog.Close();

                    saveBtn.Click += async (s2, e2) =>
                    {
                        var n = nameBox.Text.Trim();
                        if (string.IsNullOrWhiteSpace(n)) return;
                        long? catId = (catCombo.SelectedItem as ComboBoxItem)?.Value as long?;
                        await _apiService.UpdateChannelAsync(ch.Id, n, catId);
                        editDialog.Close();
                        await RefreshChannels();
                    };
                    bPanel.Controls.Add(saveBtn);
                    bPanel.Controls.Add(cBtn);
                    editDialog.Controls.Add(p);
                    editDialog.Controls.Add(bPanel);
                    editDialog.ShowDialog();
                }
                else if (chGrid.Columns[e.ColumnIndex].Name == "Delete")
                {
                    if (MessageBox.Show($"Are you sure you want to delete channel '{ch.Name}'?", "Confirm Delete", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == DialogResult.Yes)
                    {
                        await _apiService.DeleteChannelAsync(ch.Id);
                        await RefreshChannels();
                    }
                }
            };

            tab.Controls.Add(chGrid);
            chGrid.BringToFront();
            _ = RefreshChannels();
        }

        #endregion

        #region TAB 4: SERVER ROLES & PERMISSIONS

        private void BuildRolesTab(TabPage tab, Server server)
        {
            tab.BackColor = Color.White;
            tab.Padding = new Padding(15);

            var notePanel = new Panel { Dock = DockStyle.Top, Height = 56, BackColor = Color.FromArgb(240, 244, 255), Padding = new Padding(12) };
            var noteLabel = new Label
            {
                Text = "ℹ️ Server Roles & Permissions (Nội bộ Server) — Phân biệt với Admin Roles hệ thống (SuperAdmin, UserAdmin, ServerAdmin).\nCác vai trò này xác định quyền của thành viên bên trong Server.",
                Font = new Font("Segoe UI", 9F, FontStyle.Regular),
                ForeColor = Color.FromArgb(40, 60, 120),
                Dock = DockStyle.Fill
            };
            notePanel.Controls.Add(noteLabel);
            tab.Controls.Add(notePanel);

            var topActions = new Panel { Dock = DockStyle.Top, Height = 44 };
            var addRoleBtn = MakeButton("+ Add Server Role", Color.FromArgb(66, 133, 244), 140);
            addRoleBtn.Location = new Point(0, 6);
            topActions.Controls.Add(addRoleBtn);
            tab.Controls.Add(topActions);

            var roleGrid = new DataGridView();
            StyleGrid(roleGrid);
            roleGrid.Dock = DockStyle.Fill;
            roleGrid.AutoGenerateColumns = false;
            roleGrid.Columns.Add(new DataGridViewTextBoxColumn { Name = "Id", HeaderText = "ID", Width = 60 });
            roleGrid.Columns.Add(new DataGridViewTextBoxColumn { Name = "Name", HeaderText = "Role Name", Width = 180 });
            roleGrid.Columns.Add(new DataGridViewTextBoxColumn { Name = "Color", HeaderText = "Color", Width = 90 });
            roleGrid.Columns.Add(new DataGridViewTextBoxColumn { Name = "Position", HeaderText = "Position", Width = 80 });
            roleGrid.Columns.Add(new DataGridViewTextBoxColumn { Name = "IsSystem", HeaderText = "System Role", Width = 90 });
            roleGrid.Columns.Add(new DataGridViewTextBoxColumn { Name = "Permissions", HeaderText = "Permissions", Width = 240 });

            void LoadRoles()
            {
                roleGrid.Rows.Clear();
                var roles = server.Roles.Any() ? server.Roles : new List<ServerRoleItem>
                {
                    new() { Id = 1, ServerId = server.Id, Name = "Owner", Color = "#FFD700", Position = 0, IsSystem = true, Permissions = new() { "ADMINISTRATOR" } },
                    new() { Id = 2, ServerId = server.Id, Name = "Admin", Color = "#E74C3C", Position = 1, IsSystem = false, Permissions = new() { "MANAGE_SERVER", "MANAGE_CHANNELS", "KICK_MEMBERS" } },
                    new() { Id = 3, ServerId = server.Id, Name = "Member", Color = "#99AAB5", Position = 2, IsSystem = true, Permissions = new() { "VIEW_CHANNEL", "SEND_MESSAGES" } }
                };

                foreach (var r in roles)
                {
                    roleGrid.Rows.Add(r.Id, r.Name, r.Color, r.Position, r.IsSystem ? "Yes" : "No", string.Join(", ", r.Permissions));
                }
            }

            addRoleBtn.Click += (s, e) =>
            {
                MessageBox.Show("Server Roles management integration point: Backend RoleController is pending.\nRole schema and services are fully defined in UM.Core.", "Role Management", MessageBoxButtons.OK, MessageBoxIcon.Information);
            };

            tab.Controls.Add(roleGrid);
            roleGrid.BringToFront();
            LoadRoles();
        }

        #endregion

        #region TAB 5: SERVER SETTINGS

        private void BuildSettingsTab(TabPage tab, Server server, Form parentDetailsForm)
        {
            tab.BackColor = Color.White;
            tab.Padding = new Padding(24);

            var panel = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                FlowDirection = FlowDirection.TopDown,
                WrapContents = false,
                AutoScroll = true
            };

            panel.Controls.Add(MakeSectionHeader("Server Configuration"));

            panel.Controls.Add(MakeLabel("Server Name"));
            var nameBox = new TextBox { Width = 400, Font = new Font("Segoe UI", 10F), Text = server.Name };
            panel.Controls.Add(nameBox);

            panel.Controls.Add(new Panel { Height = 10 });
            panel.Controls.Add(MakeLabel("Icon File ID"));
            var iconBox = new TextBox { Width = 400, Font = new Font("Segoe UI", 10F), Text = server.IconFileId?.ToString() ?? "" };
            panel.Controls.Add(iconBox);

            panel.Controls.Add(new Panel { Height = 10 });
            panel.Controls.Add(MakeLabel("Suspension / Lock Status"));
            var suspendCheck = new CheckBox
            {
                Text = "Suspend / Block server operations (Restricts user access)",
                Checked = server.Status == ServerStatus.Blocked || server.Status == ServerStatus.Locked,
                Font = new Font("Segoe UI", 9.5F),
                AutoSize = true
            };
            panel.Controls.Add(suspendCheck);

            panel.Controls.Add(new Panel { Height = 20 });
            var saveBtn = MakeButton("Save Settings", Color.FromArgb(66, 133, 244), 130);
            saveBtn.Click += async (s, e) =>
            {
                var newName = nameBox.Text.Trim();
                if (string.IsNullOrWhiteSpace(newName))
                {
                    MessageBox.Show("Server name cannot be empty.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }
                long? iconId = null;
                if (!string.IsNullOrWhiteSpace(iconBox.Text) && long.TryParse(iconBox.Text.Trim(), out var parsedIcon))
                {
                    iconId = parsedIcon;
                }

                try
                {
                    saveBtn.Enabled = false;
                    await _apiService.UpdateServerAsync(server.Id, newName, iconId, suspendCheck.Checked);
                    MessageBox.Show("Settings saved successfully!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    parentDetailsForm.Close();
                    await LoadDataAsync();
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Failed to save settings: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                finally
                {
                    saveBtn.Enabled = true;
                }
            };
            panel.Controls.Add(saveBtn);

            tab.Controls.Add(panel);
        }

        #endregion

        #region Helpers & Members / Reports Popups

        private void ShowServerMembers(Server server)
        {
            using var form = new Form
            {
                Text = $"Members - {server.Name}",
                Size = new Size(780, 520),
                StartPosition = FormStartPosition.CenterParent,
                BackColor = Color.White,
                Font = new Font("Segoe UI", 9.5F)
            };

            var searchPanel = new Panel { Dock = DockStyle.Top, Height = 44, Padding = new Padding(10, 8, 10, 0) };
            var memberSearch = new TextBox { Dock = DockStyle.Left, Width = 280, PlaceholderText = "Search members by name..." };
            searchPanel.Controls.Add(memberSearch);

            var memberGrid = new DataGridView();
            StyleGrid(memberGrid);
            memberGrid.Dock = DockStyle.Fill;
            memberGrid.AutoGenerateColumns = false;
            memberGrid.Columns.Add(new DataGridViewTextBoxColumn { Name = "UserId", HeaderText = "User ID", Width = 70 });
            memberGrid.Columns.Add(new DataGridViewTextBoxColumn { Name = "Username", HeaderText = "Username", Width = 150 });
            memberGrid.Columns.Add(new DataGridViewTextBoxColumn { Name = "DisplayName", HeaderText = "Display Name", Width = 170 });
            memberGrid.Columns.Add(new DataGridViewTextBoxColumn { Name = "Role", HeaderText = "Server Role", Width = 100 });
            memberGrid.Columns.Add(new DataGridViewTextBoxColumn { Name = "JoinedDate", HeaderText = "Joined Date", Width = 110 });
            memberGrid.Columns.Add(new DataGridViewTextBoxColumn { Name = "Status", HeaderText = "Status", Width = 85 });

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
                Size = new Size(780, 420),
                StartPosition = FormStartPosition.CenterParent,
                BackColor = Color.White,
                Font = new Font("Segoe UI", 9.5F)
            };

            var reportGrid = new DataGridView();
            StyleGrid(reportGrid);
            reportGrid.Dock = DockStyle.Fill;
            reportGrid.AutoGenerateColumns = false;
            reportGrid.Columns.Add(new DataGridViewTextBoxColumn { Name = "Id", HeaderText = "Report ID", Width = 65 });
            reportGrid.Columns.Add(new DataGridViewTextBoxColumn { Name = "Reporter", HeaderText = "Reporter", Width = 130 });
            reportGrid.Columns.Add(new DataGridViewTextBoxColumn { Name = "Target", HeaderText = "Target", Width = 150 });
            reportGrid.Columns.Add(new DataGridViewTextBoxColumn { Name = "Reason", HeaderText = "Reason", Width = 180 });
            reportGrid.Columns.Add(new DataGridViewTextBoxColumn { Name = "Status", HeaderText = "Status", Width = 90 });
            reportGrid.Columns.Add(new DataGridViewTextBoxColumn { Name = "CreatedTime", HeaderText = "Created", Width = 130 });

            var reports = MockReportService.Instance.GetReportsForServer((int)server.Id);
            foreach (var r in reports)
            {
                reportGrid.Rows.Add(r.Id, r.Reporter, r.Target, r.Reason, r.Status.ToString(), r.CreatedTime.ToString("yyyy-MM-dd HH:mm"));
            }

            if (reports.Count == 0)
            {
                var emptyLabel = new Label { Text = "No violation reports found for this server.", Dock = DockStyle.Fill, TextAlign = ContentAlignment.MiddleCenter, ForeColor = Color.Gray, Font = new Font("Segoe UI", 11F) };
                form.Controls.Add(emptyLabel);
            }
            else
            {
                form.Controls.Add(reportGrid);
            }
            form.ShowDialog();
        }

        private static Label MakeSectionHeader(string title)
        {
            return new Label
            {
                Text = title,
                Font = new Font("Segoe UI", 11F, FontStyle.Bold),
                ForeColor = Color.FromArgb(30, 30, 45),
                AutoSize = true,
                Margin = new Padding(0, 0, 0, 12)
            };
        }

        private static Panel MakeInfoRow(string label, string value, Color? valColor = null)
        {
            var p = new Panel { Width = 420, Height = 28, Margin = new Padding(0, 0, 0, 4) };
            var lbl = new Label { Text = label, Width = 150, Font = new Font("Segoe UI", 9F, FontStyle.Bold), ForeColor = Color.FromArgb(90, 90, 100), Location = new Point(0, 4) };
            var val = new Label { Text = value, Width = 265, Font = new Font("Segoe UI", 9F), ForeColor = valColor ?? Color.FromArgb(30, 30, 40), Location = new Point(155, 4), AutoEllipsis = true };
            p.Controls.Add(lbl);
            p.Controls.Add(val);
            return p;
        }

        private static Button MakeActionButton(string text, Color bgColor)
        {
            var btn = new Button
            {
                Text = text,
                Width = 260,
                Height = 36,
                FlatStyle = FlatStyle.Flat,
                BackColor = bgColor,
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 9F, FontStyle.Bold),
                Margin = new Padding(0, 0, 0, 10),
                Cursor = Cursors.Hand,
                TextAlign = ContentAlignment.MiddleLeft,
                Padding = new Padding(12, 0, 0, 0)
            };
            btn.FlatAppearance.BorderSize = 0;
            return btn;
        }

        private static Label MakeLabel(string text, bool bold = false, Color? color = null)
        {
            return new Label
            {
                Text = text,
                AutoSize = true,
                Margin = new Padding(0, 0, 0, 4),
                Font = new Font("Segoe UI", 9.5F, bold ? FontStyle.Bold : FontStyle.Regular),
                ForeColor = color ?? Color.FromArgb(40, 40, 50),
            };
        }

        private static Button MakeButton(string text, Color bgColor, int width = 80)
        {
            var btn = new Button
            {
                Text = text,
                Width = width,
                Height = 30,
                FlatStyle = FlatStyle.Flat,
                BackColor = bgColor,
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 8.5F, FontStyle.Bold),
                Margin = new Padding(0, 0, 8, 0),
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

        private class ComboBoxItem
        {
            public string Text { get; set; } = string.Empty;
            public object? Value { get; set; }
            public override string ToString() => Text;
        }

        #endregion
    }
}
