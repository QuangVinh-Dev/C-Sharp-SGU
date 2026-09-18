using UM.Admin.Models;
using UM.Admin.Services;

namespace UM.Admin.Controls
{
    public class AdminRoleControl : UserControl
    {
        private DataGridView _grid = null!;
        private TextBox _searchBox = null!;
        private readonly MockAdminService _adminService = MockAdminService.Instance;
        private Panel _contentPanel = null!;
        private Label _accessDeniedLabel = null!;

        public AdminRoleControl()
        {
            BackColor = Color.FromArgb(245, 245, 248);
            Padding = new Padding(24);
            DoubleBuffered = true;
            Dock = DockStyle.Fill;
            BuildUI();
        }

        private void BuildUI()
        {
            // Access denied label (hidden by default)
            _accessDeniedLabel = new Label
            {
                Text = " Access Denied\n\nOnly SuperAdmin can manage admin roles.",
                Font = new Font("Segoe UI", 14F),
                ForeColor = Color.FromArgb(198, 40, 40),
                TextAlign = ContentAlignment.MiddleCenter,
                Dock = DockStyle.Fill,
                Visible = false,
            };
            Controls.Add(_accessDeniedLabel);

            _contentPanel = new Panel { Dock = DockStyle.Fill, Visible = true };

            // Check access
            if (_adminService.CurrentAdmin.Role != AdminRole.SuperAdmin)
            {
                _accessDeniedLabel.Visible = true;
                _contentPanel.Visible = false;
            }

            var topPanel = new Panel { Dock = DockStyle.Top, Height = 50 };

            topPanel.Controls.Add(new Label { Text = "Search:", AutoSize = true, Location = new Point(0, 15), Font = new Font("Segoe UI", 9) });
            _searchBox = new TextBox { Location = new Point(55, 12), Width = 220, Font = new Font("Segoe UI", 9), PlaceholderText = "Search admins..." };
            _searchBox.TextChanged += (s, e) => LoadData();
            topPanel.Controls.Add(_searchBox);

            var refreshBtn = new Button { Text = "Refresh", Location = new Point(300, 10), Height = 28, Width = 80, Font = new Font("Segoe UI", 9), BackColor = Color.White, FlatStyle = FlatStyle.Flat };
            refreshBtn.FlatAppearance.BorderColor = Color.FromArgb(200, 200, 200);
            refreshBtn.Click += (s, e) => LoadData();
            topPanel.Controls.Add(refreshBtn);

            var addBtn = new Button { Text = "+ Add Admin", Location = new Point(395, 10), Height = 28, Width = 110, Font = new Font("Segoe UI", 9, FontStyle.Bold), BackColor = Color.FromArgb(66, 133, 244), ForeColor = Color.White, FlatStyle = FlatStyle.Flat };
            addBtn.FlatAppearance.BorderSize = 0;
            addBtn.Click += (s, e) => ShowAddAdminDialog();
            topPanel.Controls.Add(addBtn);

            _contentPanel.Controls.Add(topPanel);

            _grid = new DataGridView();
            StyleGrid(_grid);
            _grid.Dock = DockStyle.Fill;
            _grid.CellFormatting += Grid_CellFormatting;
            _grid.CellContentClick += Grid_CellContentClick;
            _grid.CellDoubleClick += Grid_CellDoubleClick;

            _grid.AutoGenerateColumns = false;
            _grid.Columns.Add(new DataGridViewTextBoxColumn { Name = "Id", HeaderText = "ID", Width = 40 });
            _grid.Columns.Add(new DataGridViewTextBoxColumn { Name = "Name", HeaderText = "Name" });
            _grid.Columns.Add(new DataGridViewTextBoxColumn { Name = "Email", HeaderText = "Email" });
            _grid.Columns.Add(new DataGridViewTextBoxColumn { Name = "Role", HeaderText = "Role", Width = 90 });
            _grid.Columns.Add(new DataGridViewTextBoxColumn { Name = "Status", HeaderText = "Status", Width = 70 });
            _grid.Columns.Add(new DataGridViewTextBoxColumn { Name = "CreatedDate", HeaderText = "Created", Width = 100 });
            _grid.Columns.Add(new DataGridViewTextBoxColumn { Name = "LastActive", HeaderText = "Last Active", Width = 120 });
            _grid.Columns.Add(new DataGridViewButtonColumn { Name = "Edit", HeaderText = "", Text = "Edit", UseColumnTextForButtonValue = true, Width = 50 });
            _grid.Columns.Add(new DataGridViewButtonColumn { Name = "Remove", HeaderText = "", Text = "Remove", UseColumnTextForButtonValue = true, Width = 60 });

            _contentPanel.Controls.Add(_grid);
            _grid.BringToFront();

            Controls.Add(_contentPanel);
            _contentPanel.BringToFront();

            if (_adminService.CurrentAdmin.Role == AdminRole.SuperAdmin)
                LoadData();
        }

        private void LoadData()
        {
            _grid.Rows.Clear();
            var search = _searchBox.Text.Trim().ToLower();

            foreach (var a in _adminService.Admins)
            {
                bool match = string.IsNullOrEmpty(search) ||
                    a.Name.ToLower().Contains(search) ||
                    a.Email.ToLower().Contains(search) ||
                    a.Role.ToString().ToLower().Contains(search);

                if (match)
                {
                    _grid.Rows.Add(a.Id, a.Name, a.Email, a.Role.ToString(), a.Status.ToString(),
                        a.CreatedDate.ToString("yyyy-MM-dd"), a.LastActive.ToString("yyyy-MM-dd HH:mm"));
                    _grid.Rows[_grid.Rows.Count - 1].Tag = a;
                }
            }
        }

        private void Grid_CellFormatting(object? sender, DataGridViewCellFormattingEventArgs e)
        {
            if (_grid.Columns[e.ColumnIndex].Name == "Role" && e.Value != null)
            {
                if (e.CellStyle != null)
                {
                    e.CellStyle.Font = new Font("Segoe UI", 9, FontStyle.Bold);
                    e.CellStyle.ForeColor = e.Value.ToString() switch
                    {
                        "SuperAdmin" => Color.FromArgb(103, 58, 183),
                        "UserAdmin" => Color.FromArgb(33, 150, 243),
                        "ServerAdmin" => Color.FromArgb(46, 125, 50),
                        _ => Color.Gray,
                    };
                }
            }
            if (_grid.Columns[e.ColumnIndex].Name == "Status" && e.Value != null)
            {
                if (e.CellStyle != null)
                {
                    e.CellStyle.Font = new Font("Segoe UI", 9, FontStyle.Bold);
                    e.CellStyle.ForeColor = e.Value.ToString() == "Active" ? Color.FromArgb(46, 125, 50) : Color.FromArgb(198, 40, 40);
                }
            }
        }

        private void Grid_CellContentClick(object? sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0) return;
            var admin = _grid.Rows[e.RowIndex].Tag as AdminUser;
            if (admin == null) return;

            if (_grid.Columns[e.ColumnIndex].Name == "Edit")
                ShowEditAdminDialog(admin);
            else if (_grid.Columns[e.ColumnIndex].Name == "Remove")
                RemoveAdmin(admin);
        }

        private void Grid_CellDoubleClick(object? sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0)
            {
                var admin = _grid.Rows[e.RowIndex].Tag as AdminUser;
                if (admin != null) ShowEditAdminDialog(admin);
            }
        }

        private void ShowAddAdminDialog()
        {
            using var form = new Form
            {
                Text = "Add Admin", Size = new Size(420, 280), StartPosition = FormStartPosition.CenterParent,
                FormBorderStyle = FormBorderStyle.FixedDialog, MaximizeBox = false, MinimizeBox = false,
                BackColor = Color.White, Font = new Font("Segoe UI", 9.5F)
            };

            form.Controls.Add(new Label { Text = "Name:", Location = new Point(20, 23), AutoSize = true });
            var nameBox = new TextBox { Location = new Point(130, 20), Width = 240 };
            form.Controls.Add(nameBox);

            form.Controls.Add(new Label { Text = "Email:", Location = new Point(20, 63), AutoSize = true });
            var emailBox = new TextBox { Location = new Point(130, 60), Width = 240 };
            form.Controls.Add(emailBox);

            form.Controls.Add(new Label { Text = "Role:", Location = new Point(20, 103), AutoSize = true });
            var roleCombo = new ComboBox { Location = new Point(130, 100), Width = 240, DropDownStyle = ComboBoxStyle.DropDownList };
            roleCombo.Items.AddRange(new object[] { "SuperAdmin", "UserAdmin", "ServerAdmin" });
            roleCombo.SelectedIndex = 1;
            form.Controls.Add(roleCombo);

            var saveBtn = new Button { Text = "Add", Location = new Point(130, 155), Width = 90, Height = 32, DialogResult = DialogResult.OK, BackColor = Color.FromArgb(66, 133, 244), ForeColor = Color.White, FlatStyle = FlatStyle.Flat };
            saveBtn.FlatAppearance.BorderSize = 0;
            form.Controls.Add(saveBtn);
            form.Controls.Add(new Button { Text = "Cancel", Location = new Point(230, 155), Width = 90, Height = 32, DialogResult = DialogResult.Cancel, FlatStyle = FlatStyle.Flat });

            if (form.ShowDialog() == DialogResult.OK && !string.IsNullOrWhiteSpace(nameBox.Text))
            {
                var role = Enum.Parse<AdminRole>(roleCombo.SelectedItem?.ToString() ?? "UserAdmin");
                _adminService.AddAdmin(new AdminUser
                {
                    Name = nameBox.Text.Trim(),
                    Email = emailBox.Text.Trim(),
                    Role = role,
                    Status = AdminStatus.Active,
                    CreatedDate = DateTime.Now,
                    LastActive = DateTime.Now,
                });
                _adminService.AddActivity("Added new admin", nameBox.Text.Trim());
                LoadData();
            }
        }

        private void ShowEditAdminDialog(AdminUser admin)
        {
            using var form = new Form
            {
                Text = "Edit Admin", Size = new Size(420, 330), StartPosition = FormStartPosition.CenterParent,
                FormBorderStyle = FormBorderStyle.FixedDialog, MaximizeBox = false, MinimizeBox = false,
                BackColor = Color.White, Font = new Font("Segoe UI", 9.5F)
            };

            form.Controls.Add(new Label { Text = "Name:", Location = new Point(20, 23), AutoSize = true });
            var nameBox = new TextBox { Location = new Point(130, 20), Width = 240, Text = admin.Name };
            form.Controls.Add(nameBox);

            form.Controls.Add(new Label { Text = "Email:", Location = new Point(20, 63), AutoSize = true });
            var emailBox = new TextBox { Location = new Point(130, 60), Width = 240, Text = admin.Email };
            form.Controls.Add(emailBox);

            form.Controls.Add(new Label { Text = "Role:", Location = new Point(20, 103), AutoSize = true });
            var roleCombo = new ComboBox { Location = new Point(130, 100), Width = 240, DropDownStyle = ComboBoxStyle.DropDownList };
            roleCombo.Items.AddRange(new object[] { "SuperAdmin", "UserAdmin", "ServerAdmin" });
            roleCombo.SelectedItem = admin.Role.ToString();
            form.Controls.Add(roleCombo);

            form.Controls.Add(new Label { Text = "Status:", Location = new Point(20, 143), AutoSize = true });
            var statusCombo = new ComboBox { Location = new Point(130, 140), Width = 240, DropDownStyle = ComboBoxStyle.DropDownList };
            statusCombo.Items.AddRange(new object[] { "Active", "Inactive" });
            statusCombo.SelectedItem = admin.Status.ToString();
            form.Controls.Add(statusCombo);

            var saveBtn = new Button { Text = "Save", Location = new Point(130, 195), Width = 90, Height = 32, DialogResult = DialogResult.OK, BackColor = Color.FromArgb(66, 133, 244), ForeColor = Color.White, FlatStyle = FlatStyle.Flat };
            saveBtn.FlatAppearance.BorderSize = 0;
            form.Controls.Add(saveBtn);
            form.Controls.Add(new Button { Text = "Cancel", Location = new Point(230, 195), Width = 90, Height = 32, DialogResult = DialogResult.Cancel, FlatStyle = FlatStyle.Flat });

            if (form.ShowDialog() == DialogResult.OK)
            {
                admin.Name = nameBox.Text.Trim();
                admin.Email = emailBox.Text.Trim();
                admin.Role = Enum.Parse<AdminRole>(roleCombo.SelectedItem?.ToString() ?? "UserAdmin");
                admin.Status = Enum.Parse<AdminStatus>(statusCombo.SelectedItem?.ToString() ?? "Active");
                _adminService.UpdateAdmin(admin);
                _adminService.AddActivity("Updated admin", admin.Name);
                LoadData();
            }
        }

        private void RemoveAdmin(AdminUser admin)
        {
            if (admin.Id == _adminService.CurrentAdmin.Id)
            {
                MessageBox.Show("You cannot remove yourself.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (MessageBox.Show($"Are you sure you want to remove admin '{admin.Name}'?\n\nThis action cannot be undone.", "Remove Admin", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == DialogResult.Yes)
            {
                _adminService.RemoveAdmin(admin.Id);
                _adminService.AddActivity("Removed admin", admin.Name);
                LoadData();
            }
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
