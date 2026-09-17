using UM.Admin.Models;
using UM.Admin.Services;

namespace UM.Admin.Controls
{
    public class UserManagementControl : UserControl
    {
        private DataGridView _grid = null!;
        private TextBox _searchBox = null!;
        private ComboBox _statusFilter = null!;
        private ComboBox _roleFilter = null!;
        private readonly MockUserService _userService = MockUserService.Instance;

        public UserManagementControl()
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
            _searchBox = new TextBox { Location = new Point(55, 12), Width = 200, Font = new Font("Segoe UI", 9), PlaceholderText = "Search users..." };
            _searchBox.TextChanged += (s, e) => LoadData();
            topPanel.Controls.Add(_searchBox);

            topPanel.Controls.Add(new Label { Text = "Status:", AutoSize = true, Location = new Point(270, 15), Font = new Font("Segoe UI", 9) });
            _statusFilter = new ComboBox { Location = new Point(320, 12), Width = 120, DropDownStyle = ComboBoxStyle.DropDownList, Font = new Font("Segoe UI", 9) };
            _statusFilter.Items.AddRange(new object[] { "All", "Active", "Banned", "Suspended" });
            _statusFilter.SelectedIndex = 0;
            _statusFilter.SelectedIndexChanged += (s, e) => LoadData();
            topPanel.Controls.Add(_statusFilter);

            topPanel.Controls.Add(new Label { Text = "Role:", AutoSize = true, Location = new Point(460, 15), Font = new Font("Segoe UI", 9) });
            _roleFilter = new ComboBox { Location = new Point(500, 12), Width = 120, DropDownStyle = ComboBoxStyle.DropDownList, Font = new Font("Segoe UI", 9) };
            _roleFilter.Items.AddRange(new object[] { "All", "Member", "Moderator", "Admin" });
            _roleFilter.SelectedIndex = 0;
            _roleFilter.SelectedIndexChanged += (s, e) => LoadData();
            topPanel.Controls.Add(_roleFilter);

            var refreshBtn = new Button { Text = "Refresh", Location = new Point(640, 10), Height = 28, Width = 80, Font = new Font("Segoe UI", 9), BackColor = Color.White, FlatStyle = FlatStyle.Flat };
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

            // Setup columns
            _grid.AutoGenerateColumns = false;
            _grid.Columns.Add(new DataGridViewTextBoxColumn { Name = "Id", HeaderText = "ID", Width = 50 });
            _grid.Columns.Add(new DataGridViewTextBoxColumn { Name = "Username", HeaderText = "Username" });
            _grid.Columns.Add(new DataGridViewTextBoxColumn { Name = "DisplayName", HeaderText = "Display Name" });
            _grid.Columns.Add(new DataGridViewTextBoxColumn { Name = "Email", HeaderText = "Email" });
            _grid.Columns.Add(new DataGridViewTextBoxColumn { Name = "Status", HeaderText = "Status", Width = 80 });
            _grid.Columns.Add(new DataGridViewTextBoxColumn { Name = "Role", HeaderText = "Role", Width = 80 });
            _grid.Columns.Add(new DataGridViewTextBoxColumn { Name = "CreatedDate", HeaderText = "Created Date", Width = 100 });
            _grid.Columns.Add(new DataGridViewTextBoxColumn { Name = "LastActive", HeaderText = "Last Active", Width = 100 });
            
            var role = MockAdminService.Instance.CurrentAdmin.Role;
            bool canEdit = role == AdminRole.SuperAdmin || role == AdminRole.UserAdmin;

            var editCol = new DataGridViewButtonColumn { Name = "Edit", HeaderText = "Edit", Text = "Sửa", UseColumnTextForButtonValue = true, Width = 60 };
            editCol.Visible = canEdit;
            _grid.Columns.Add(editCol);

            var lockCol = new DataGridViewButtonColumn { Name = "LockUnlock", HeaderText = "Action", UseColumnTextForButtonValue = false, Width = 80 };
            lockCol.Visible = canEdit;
            _grid.Columns.Add(lockCol);

            _grid.Columns.Add(new DataGridViewButtonColumn { Name = "Actions", HeaderText = "View", Text = "Chi tiết", UseColumnTextForButtonValue = true, Width = 60 });

            Controls.Add(_grid);
            _grid.BringToFront();
        }

        private void LoadData()
        {
            _grid.Rows.Clear();
            var search = _searchBox.Text.Trim().ToLower();
            var statusFilter = _statusFilter.SelectedItem?.ToString() ?? "All";
            var roleFilter = _roleFilter.SelectedItem?.ToString() ?? "All";

            foreach (var u in _userService.Users)
            {
                bool matchSearch = string.IsNullOrEmpty(search) ||
                    u.Username.ToLower().Contains(search) ||
                    u.DisplayName.ToLower().Contains(search) ||
                    u.Email.ToLower().Contains(search);

                bool matchStatus = statusFilter == "All" || u.Status.ToString() == statusFilter;
                bool matchRole = roleFilter == "All" || u.Role == roleFilter;

                if (matchSearch && matchStatus && matchRole)
                {
                    int index = _grid.Rows.Add(u.Id, u.Username, u.DisplayName, u.Email,
                        u.Status.ToString(), u.Role,
                        u.CreatedDate.ToString("yyyy-MM-dd"),
                        u.LastActive.ToString("yyyy-MM-dd HH:mm"));
                    _grid.Rows[index].Tag = u;
                    _grid.Rows[index].Cells["LockUnlock"].Value = (u.Status == UserStatus.Active) ? "Khóa" : "Mở khóa";
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
                        "Active" => Color.FromArgb(46, 125, 50),
                        "Banned" => Color.FromArgb(198, 40, 40),
                        "Suspended" => Color.FromArgb(230, 126, 34),
                        _ => Color.Gray,
                    };
                }
            }
        }

        private void Grid_CellContentClick(object? sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0)
            {
                var user = _grid.Rows[e.RowIndex].Tag as User;
                if (user == null) return;
                
                var role = MockAdminService.Instance.CurrentAdmin.Role;
                bool canEdit = role == AdminRole.SuperAdmin || role == AdminRole.UserAdmin;

                string colName = _grid.Columns[e.ColumnIndex].Name;
                if (colName == "Edit")
                {
                    if (canEdit) ShowEditDialog(user);
                    else MessageBox.Show("Không có quyền thực hiện chức năng này.", "Access Denied", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
                else if (colName == "LockUnlock")
                {
                    if (canEdit) ToggleLockStatus(user);
                    else MessageBox.Show("Không có quyền thực hiện chức năng này.", "Access Denied", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
                else if (colName == "Actions")
                {
                    ShowUserDetail(user);
                }
            }
        }

        private void Grid_CellDoubleClick(object? sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0) ShowUserDetail(_grid.Rows[e.RowIndex].Tag as User);
        }

        private void ToggleLockStatus(User user)
        {
            var role = MockAdminService.Instance.CurrentAdmin.Role;
            if (role != AdminRole.SuperAdmin && role != AdminRole.UserAdmin)
            {
                MessageBox.Show("Không có quyền thực hiện chức năng này.", "Access Denied", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (user.Status == UserStatus.Active)
            {
                if (MessageBox.Show($"Bạn có chắc chắn muốn khóa người dùng '{user.DisplayName}'?", "Khóa người dùng", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == DialogResult.Yes)
                {
                    var adminName = MockAdminService.Instance.CurrentAdmin.Name;
                    _userService.BanUser(user.Id, "Khóa bởi Admin", adminName);
                    MockAdminService.Instance.AddActivity("Khóa người dùng", user.DisplayName);
                    LoadData();
                }
            }
            else
            {
                if (MessageBox.Show($"Bạn có chắc chắn muốn mở khóa người dùng '{user.DisplayName}'?", "Mở khóa người dùng", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                {
                    _userService.UnbanUser(user.Id);
                    MockAdminService.Instance.AddActivity("Mở khóa người dùng", user.DisplayName);
                    LoadData();
                }
            }
        }

        private void ShowUserDetail(User? user)
        {
            if (user == null) return;
            using var form = new Form
            {
                Text = $"User Details - {user.DisplayName}",
                Size = new Size(520, 560),
                StartPosition = FormStartPosition.CenterParent,
                FormBorderStyle = FormBorderStyle.FixedDialog,
                MaximizeBox = false, MinimizeBox = false,
                BackColor = Color.White, Font = new Font("Segoe UI", 9.5F)
            };

            var info = new FlowLayoutPanel { Dock = DockStyle.Fill, FlowDirection = FlowDirection.TopDown, Padding = new Padding(20), AutoScroll = true };
            info.Controls.Add(MakeLabel($"User ID: {user.Id}", true));
            info.Controls.Add(MakeLabel($"Username: {user.Username}"));
            info.Controls.Add(MakeLabel($"Display Name: {user.DisplayName}"));
            info.Controls.Add(MakeLabel($"Email: {user.Email}"));
            info.Controls.Add(MakeLabel($"Status: {user.Status}", false, user.Status == UserStatus.Banned ? Color.Red : Color.FromArgb(46, 125, 50)));
            info.Controls.Add(MakeLabel($"Role: {user.Role}"));
            info.Controls.Add(MakeLabel($"Created: {user.CreatedDate:yyyy-MM-dd}"));
            info.Controls.Add(MakeLabel($"Last Active: {user.LastActive:yyyy-MM-dd HH:mm}"));
            info.Controls.Add(MakeLabel($"Servers Joined: {user.ServersJoined}"));
            info.Controls.Add(MakeLabel($"Report Count: {user.ReportCount}"));

            if (user.Status == UserStatus.Banned)
            {
                info.Controls.Add(MakeLabel($"Ban Reason: {user.BanReason}", false, Color.Red));
                info.Controls.Add(MakeLabel($"Banned By: {user.BannedBy}", false, Color.Red));
                info.Controls.Add(MakeLabel($"Ban Time: {user.BanTime:yyyy-MM-dd HH:mm}", false, Color.Red));
                info.Controls.Add(MakeLabel($"Duration: {user.BanDuration}"));
            }
            form.Controls.Add(info);

            var btnPanel = new FlowLayoutPanel { Dock = DockStyle.Bottom, Height = 50, Padding = new Padding(15, 8, 15, 8), FlowDirection = FlowDirection.LeftToRight };

            var role = MockAdminService.Instance.CurrentAdmin.Role;
            bool canEdit = role == AdminRole.SuperAdmin || role == AdminRole.UserAdmin;
            bool canDelete = role == AdminRole.SuperAdmin;

            if (canEdit)
            {
                var editBtn = MakeButton("Sửa", Color.FromArgb(66, 133, 244));
                editBtn.Click += (s, e) => { form.Close(); ShowEditDialog(user); };
                btnPanel.Controls.Add(editBtn);

                if (user.Status == UserStatus.Active)
                {
                    var banBtn = MakeButton("Khóa", Color.FromArgb(198, 40, 40));
                    banBtn.Click += (s, e) => { form.Close(); ToggleLockStatus(user); };
                    btnPanel.Controls.Add(banBtn);
                }
                else
                {
                    var unbanBtn = MakeButton("Mở khóa", Color.FromArgb(46, 125, 50));
                    unbanBtn.Click += (s, e) => { form.Close(); ToggleLockStatus(user); };
                    btnPanel.Controls.Add(unbanBtn);
                }
            }

            if (canDelete)
            {
                var delBtn = MakeButton("Delete", Color.FromArgb(183, 28, 28));
                delBtn.Click += (s, e) =>
                {
                    if (MessageBox.Show("Are you sure you want to delete this user? This cannot be undone.", "Delete User", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == DialogResult.Yes)
                    {
                        _userService.DeleteUser(user.Id);
                        MockAdminService.Instance.AddActivity("Deleted user", user.DisplayName);
                        LoadData();
                    }
                    form.Close();
                };
                btnPanel.Controls.Add(delBtn);
            }

            var logBtn = MakeButton("View Log", Color.FromArgb(100, 100, 110));
            logBtn.Click += (s, e) => ShowAccountLogDialog(user);
            btnPanel.Controls.Add(logBtn);

            form.Controls.Add(btnPanel);
            form.ShowDialog();
        }

        private void ShowEditDialog(User user)
        {
            var role = MockAdminService.Instance.CurrentAdmin.Role;
            if (role != AdminRole.SuperAdmin && role != AdminRole.UserAdmin)
            {
                MessageBox.Show("Không có quyền thực hiện chức năng này.", "Access Denied", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            using var form = new Form
            {
                Text = "Sửa Người Dùng", Size = new Size(420, 300), StartPosition = FormStartPosition.CenterParent,
                FormBorderStyle = FormBorderStyle.FixedDialog, MaximizeBox = false, MinimizeBox = false, BackColor = Color.White,
                Font = new Font("Segoe UI", 9.5F)
            };

            form.Controls.Add(new Label { Text = "Username:", Location = new Point(20, 23), AutoSize = true });
            var nameBox = new TextBox { Location = new Point(130, 20), Width = 240, Text = user.Username };
            form.Controls.Add(nameBox);

            form.Controls.Add(new Label { Text = "Display Name:", Location = new Point(20, 63), AutoSize = true });
            var displayBox = new TextBox { Location = new Point(130, 60), Width = 240, Text = user.DisplayName };
            form.Controls.Add(displayBox);

            form.Controls.Add(new Label { Text = "Email:", Location = new Point(20, 103), AutoSize = true });
            var emailBox = new TextBox { Location = new Point(130, 100), Width = 240, Text = user.Email };
            form.Controls.Add(emailBox);

            form.Controls.Add(new Label { Text = "Role:", Location = new Point(20, 143), AutoSize = true });
            var roleCombo = new ComboBox { Location = new Point(130, 140), Width = 240, DropDownStyle = ComboBoxStyle.DropDownList };
            roleCombo.Items.AddRange(new object[] { "Member", "Moderator", "Admin" });
            roleCombo.SelectedItem = user.Role;
            form.Controls.Add(roleCombo);

            var saveBtn = new Button { Text = "Save", Location = new Point(130, 195), Width = 90, Height = 32, DialogResult = DialogResult.OK, BackColor = Color.FromArgb(66, 133, 244), ForeColor = Color.White, FlatStyle = FlatStyle.Flat };
            saveBtn.FlatAppearance.BorderSize = 0;
            form.Controls.Add(saveBtn);
            var cancelBtn = new Button { Text = "Cancel", Location = new Point(230, 195), Width = 90, Height = 32, DialogResult = DialogResult.Cancel, FlatStyle = FlatStyle.Flat };
            form.Controls.Add(cancelBtn);

            if (form.ShowDialog() == DialogResult.OK)
            {
                if (string.IsNullOrWhiteSpace(nameBox.Text) || string.IsNullOrWhiteSpace(displayBox.Text))
                {
                    MessageBox.Show("Username and Display Name cannot be empty.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                user.Username = nameBox.Text.Trim();
                user.DisplayName = displayBox.Text.Trim();
                user.Email = emailBox.Text.Trim();
                user.Role = roleCombo.SelectedItem?.ToString() ?? "Member";
                _userService.UpdateUser(user);
                MockAdminService.Instance.AddActivity("Updated user info", user.DisplayName);
                LoadData();
            }
        }

        private void ShowAccountLogDialog(User user)
        {
            using var form = new Form
            {
                Text = $"Account Log - {user.DisplayName}",
                Size = new Size(700, 450), StartPosition = FormStartPosition.CenterParent,
                BackColor = Color.White, Font = new Font("Segoe UI", 9.5F)
            };

            var logGrid = new DataGridView();
            StyleGrid(logGrid);
            logGrid.Dock = DockStyle.Fill;
            logGrid.AutoGenerateColumns = false;
            logGrid.Columns.Add(new DataGridViewTextBoxColumn { Name = "Time", HeaderText = "Time" });
            logGrid.Columns.Add(new DataGridViewTextBoxColumn { Name = "Action", HeaderText = "Action" });
            logGrid.Columns.Add(new DataGridViewTextBoxColumn { Name = "Source", HeaderText = "Source" });
            logGrid.Columns.Add(new DataGridViewTextBoxColumn { Name = "Description", HeaderText = "Description", FillWeight = 200 });

            var logs = _userService.GetLogsForUser(user.Id);
            foreach (var log in logs)
            {
                logGrid.Rows.Add(log.Time.ToString("yyyy-MM-dd HH:mm"), log.Action, log.Source, log.Description);
            }

            form.Controls.Add(logGrid);
            form.ShowDialog();
        }

        private static Label MakeLabel(string text, bool bold = false, Color? color = null)
        {
            return new Label
            {
                Text = text,
                AutoSize = true,
                Margin = new Padding(0, 0, 0, 6),
                Font = new Font("Segoe UI", 9.5F, bold ? FontStyle.Bold : FontStyle.Regular),
                ForeColor = color ?? Color.FromArgb(40, 40, 50),
            };
        }

        private static Button MakeButton(string text, Color bgColor)
        {
            var btn = new Button
            {
                Text = text, Width = 80, Height = 30,
                FlatStyle = FlatStyle.Flat,
                BackColor = bgColor, ForeColor = Color.White,
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
    }
}
