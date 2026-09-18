using UM.Admin.Models;
using UM.Admin.Services;

namespace UM.Admin.Forms
{
    public partial class MainForm : Form
    {
        // Two-pane Layout Architecture
        private Panel _sidebarPanel = null!;      // Left Sidebar (Dock = Left, Width = 240)
        private Panel _rightAreaPanel = null!;    // Right Container (Dock = Fill)
        private Panel _headerPanel = null!;       // Header (Dock = Top inside Right Container, Height = 56)
        private Panel _contentPanel = null!;      // Content (Dock = Fill inside Right Container)

        // Header controls
        private Label _headerTitle = null!;
        private Label _headerSubtitle = null!;

        // Sidebar controls
        private Label _adminNameLabel = null!;
        private Label _adminRoleLabel = null!;
        private ComboBox _roleCombo = null!;
        private Panel _navContainer = null!;
        //private FlowLayoutPanel _navTable = null!;

        // 5 Standard Canonical Navigation Buttons
        private Button _btnDashboard = null!;
        private Button _btnUsers = null!;
        private Button _btnServers = null!;
        private Button _btnReports = null!;
        private Button _btnAdminRoles = null!;
        private readonly List<Button> _navButtons = new();

        private UserControl? _currentControl;

        // Color Palette
        private static readonly Color SidebarBg = Color.FromArgb(30, 30, 46);
        private static readonly Color SidebarHover = Color.FromArgb(45, 45, 65);
        private static readonly Color SidebarActive = Color.FromArgb(55, 55, 85);
        private static readonly Color HeaderBg = Color.FromArgb(250, 250, 252);
        private static readonly Color ContentBg = Color.FromArgb(245, 245, 248);
        private static readonly Color AccentBlue = Color.FromArgb(66, 133, 244);
        private static readonly Color TextWhite = Color.FromArgb(230, 230, 240);
        private static readonly Color TextMuted = Color.FromArgb(150, 150, 170);
        private static readonly Color TextDark = Color.FromArgb(40, 40, 50);
        private static readonly Color BorderColor = Color.FromArgb(225, 225, 230);

        public MainForm()
        {
            InitializeComponent();
            SetupForm();
            BuildLayout();

            // Set combobox UI, tạm gỡ event để tránh double-run
            _roleCombo.SelectedIndexChanged -= RoleCombo_SelectedIndexChanged;
            _roleCombo.SelectedItem = "SuperAdmin";
            _roleCombo.SelectedIndexChanged += RoleCombo_SelectedIndexChanged;

            // Khởi tạo admin/state + render nav tường minh, không dựa vào event side-effect
            var adminService = MockAdminService.Instance;
            adminService.CurrentAdmin = adminService.Admins.FirstOrDefault(a => a.Role == AdminRole.SuperAdmin)
                                        ?? new AdminUser { Id = 99, Name = "Admin (SuperAdmin)", Role = AdminRole.SuperAdmin, Email = "superadmin@um.com" };
            adminService.CurrentAdmin.Role = AdminRole.SuperAdmin;

            _adminNameLabel.Text = adminService.CurrentAdmin.Name;
            _adminRoleLabel.Text = AdminRole.SuperAdmin.ToString();
            _headerSubtitle.Text = $"Logged in as: {adminService.CurrentAdmin.Name} ({adminService.CurrentAdmin.Role}) | {adminService.CurrentAdmin.Email}";

            ApplyRoleBasedNavigation();
            NavigateTo("Dashboard");
        }

        private void SetupForm()
        {
            Text = "UM Admin Panel - TeamTalks";
            Size = new Size(1400, 850);
            MinimumSize = new Size(1100, 700);
            StartPosition = FormStartPosition.CenterScreen;
            BackColor = ContentBg;
            Font = new Font("Segoe UI", 9F);
            DoubleBuffered = true;
        }

        private void BuildLayout()
        {
            // ==========================================
            // 1. LEFT SIDEBAR (Width = 240, Dock = Left)
            // ==========================================
            _sidebarPanel = new Panel
            {
                Dock = DockStyle.Left,
                Width = 240,
                BackColor = SidebarBg,
            };

            // Logo panel at top of sidebar
            var logoPanel = new Panel
            {
                Dock = DockStyle.Top,
                Height = 64,
                BackColor = Color.FromArgb(24, 24, 38)
            };
            var logoLabel = new Label
            {
                Text = "UM Admin",
                Font = new Font("Segoe UI", 15F, FontStyle.Bold),
                ForeColor = TextWhite,
                AutoSize = false,
                TextAlign = ContentAlignment.MiddleCenter,
                Dock = DockStyle.Fill,
            };
            logoPanel.Controls.Add(logoLabel);
            _sidebarPanel.Controls.Add(logoPanel);

            // Mock Role Switcher Panel
            var roleSwitchPanel = new Panel
            {
                Dock = DockStyle.Top,
                Height = 72,
                Padding = new Padding(16, 10, 16, 6)
            };
            var roleLabel = new Label
            {
                Text = "Mock Role Switcher:",
                ForeColor = TextMuted,
                Font = new Font("Segoe UI", 8F, FontStyle.Bold),
                AutoSize = false,
                Height = 18,
                Dock = DockStyle.Top,
            };
            _roleCombo = new ComboBox
            {
                Dock = DockStyle.Top,
                DropDownStyle = ComboBoxStyle.DropDownList,
                Font = new Font("Segoe UI", 9.5F),
                Height = 28,
            };
            _roleCombo.Items.AddRange(new object[] { "SuperAdmin", "UserAdmin", "ServerAdmin" });
            _roleCombo.SelectedIndexChanged += RoleCombo_SelectedIndexChanged;
            roleSwitchPanel.Controls.Add(_roleCombo);
            roleSwitchPanel.Controls.Add(roleLabel);
            _sidebarPanel.Controls.Add(roleSwitchPanel);
            logoPanel.SendToBack();

            // Current Admin Info Panel at bottom of sidebar
            var adminInfoPanel = new Panel
            {
                Dock = DockStyle.Bottom,
                Height = 64,
                BackColor = Color.FromArgb(24, 24, 38),
                Padding = new Padding(16, 10, 16, 10),
            };
            _adminNameLabel = new Label
            {
                Text = MockAdminService.Instance.CurrentAdmin.Name,
                ForeColor = TextWhite,
                Font = new Font("Segoe UI", 9.5F, FontStyle.Bold),
                AutoSize = false,
                Height = 22,
                Dock = DockStyle.Top,
            };
            _adminRoleLabel = new Label
            {
                Text = MockAdminService.Instance.CurrentAdmin.Role.ToString(),
                ForeColor = TextMuted,
                Font = new Font("Segoe UI", 8F),
                AutoSize = false,
                Height = 18,
                Dock = DockStyle.Top,
            };
            adminInfoPanel.Controls.Add(_adminRoleLabel);
            adminInfoPanel.Controls.Add(_adminNameLabel);
            _sidebarPanel.Controls.Add(adminInfoPanel);

            // Navigation Container (Fill the middle of sidebar)
            _navContainer = new Panel
            {
                Dock = DockStyle.Fill,
                Padding = new Padding(0, 8, 0, 8),
                AutoScroll = true,
            };

            // FlowLayoutPanel to layout vertical navigation buttons without TableLayoutPanel auto-size collapse
            //_navTable = new FlowLayoutPanel
            //{
            //    Dock = DockStyle.Top,
            //    FlowDirection = FlowDirection.TopDown,
            //    WrapContents = false,
            //    AutoSize = true,
            //    AutoSizeMode = AutoSizeMode.GrowAndShrink,
            //    Padding = new Padding(0),
            //    Margin = new Padding(0),
            //};
            //_navContainer.Controls.Add(_navTable);

            //_navContainer.Resize += (s, e) =>
            //{
            //    int w = _navContainer.ClientSize.Width;
            //    if (w > 0)
            //    {
            //        foreach (Control c in _navTable.Controls)
            //        {
            //            c.Width = w;
            //        }
            //    }
            //};

            // Create the 5 Canonical Navigation Buttons
            _btnDashboard = CreateNavButton("Dashboard", "\U0001F4CA", "Dashboard");
            _btnUsers = CreateNavButton("Users", "\U0001F465", "Users");
            _btnServers = CreateNavButton("Servers", "\U0001F5A5", "Servers");
            _btnReports = CreateNavButton("Reports", "\U0001F4CB", "Reports");
            _btnAdminRoles = CreateNavButton("Admin Roles", "\U0001F511", "Admin Roles");

            _sidebarPanel.Controls.Add(_navContainer);
            _navContainer.BringToFront();

            // ==========================================
            // 2. RIGHT AREA CONTAINER (Dock = Fill)
            // ==========================================
            _rightAreaPanel = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = ContentBg,
                Padding = new Padding(0),
            };

            // Top Header inside Right Area
            _headerPanel = new Panel
            {
                Dock = DockStyle.Top,
                Height = 56,
                BackColor = HeaderBg,
                Padding = new Padding(24, 0, 24, 0),
            };
            var headerBorder = new Panel
            {
                Dock = DockStyle.Bottom,
                Height = 1,
                BackColor = BorderColor
            };
            _headerTitle = new Label
            {
                Text = "Dashboard",
                Font = new Font("Segoe UI", 14F, FontStyle.Bold),
                ForeColor = TextDark,
                AutoSize = false,
                TextAlign = ContentAlignment.MiddleLeft,
                Dock = DockStyle.Left,
                Width = 350,
            };
            _headerSubtitle = new Label
            {
                Text = MockAdminService.Instance.CurrentAdmin.Email,
                Font = new Font("Segoe UI", 9F),
                ForeColor = Color.FromArgb(130, 130, 140),
                AutoSize = false,
                TextAlign = ContentAlignment.MiddleRight,
                Dock = DockStyle.Right,
                Width = 450,
            };
            _headerPanel.Controls.Add(_headerTitle);
            _headerPanel.Controls.Add(_headerSubtitle);
            _headerPanel.Controls.Add(headerBorder);

            // Content Area inside Right Area
            _contentPanel = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = ContentBg,
                Padding = new Padding(0),
            };

            // Order of adding inside Right Area (Content fills below Header)
            _rightAreaPanel.Controls.Add(_contentPanel);
            _rightAreaPanel.Controls.Add(_headerPanel);

            // ==========================================
            // 3. ADD CONTAINERS TO MAIN FORM
            // ==========================================
            Controls.Add(_rightAreaPanel);
            Controls.Add(_sidebarPanel);
        }

        private Button CreateNavButton(string text, string icon, string pageName)
        {
            var btn = new Button
            {
                Text = $"   {icon}   {text}",
                Tag = pageName,
                Font = new Font("Segoe UI", 10F),
                ForeColor = TextWhite,
                FlatStyle = FlatStyle.Flat,
                BackColor = SidebarBg,
                TextAlign = ContentAlignment.MiddleLeft,
                Height = 44,
                Dock = DockStyle.Fill,
                Padding = new Padding(12, 0, 0, 0),
                Margin = new Padding(0),
                Cursor = Cursors.Hand,
                ImageAlign = ContentAlignment.MiddleLeft,
            };
            btn.FlatAppearance.BorderSize = 0;
            btn.FlatAppearance.MouseOverBackColor = SidebarHover;
            btn.FlatAppearance.MouseDownBackColor = SidebarActive;
            btn.Click += NavButton_Click;
            _navButtons.Add(btn);
            return btn;
        }

        private void NavButton_Click(object? sender, EventArgs e)
        {
            if (sender is Button btn && btn.Tag is string page)
            {
                NavigateTo(page);
            }
        }

        public void NavigateTo(string page)
        {
            var currentRole = MockAdminService.Instance.CurrentAdmin.Role;

            // Strict Role-based access verification
            if (page == "Servers" && currentRole != AdminRole.ServerAdmin)
            {
                MessageBox.Show("Truy cập bị từ chối: Chức năng Quản lý Server chỉ dành riêng cho ServerAdmin!",
                    "Permission Denied", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (page == "Users" && currentRole != AdminRole.UserAdmin)
            {
                MessageBox.Show("Truy cập bị từ chối: Chức năng Quản lý Người dùng chỉ dành riêng cho UserAdmin!",
                    "Permission Denied", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (page == "Admin Roles" && currentRole != AdminRole.SuperAdmin)
            {
                MessageBox.Show("Truy cập bị từ chối: Chức năng Quản lý Admin Roles chỉ dành riêng cho SuperAdmin!",
                    "Permission Denied", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // Update Header Title according to Page
            _headerTitle.Text = page switch
            {
                "Dashboard" => "Dashboard",
                "Users" => "User Management",
                "Servers" => "Server Management",
                "Reports" => "Report Management",
                "Admin Roles" => "System Admin Role Management",
                _ => page
            };

            // Highlight Active Navigation Button
            foreach (var btn in _navButtons)
            {
                btn.BackColor = (btn.Tag?.ToString() == page) ? SidebarActive : SidebarBg;
            }

            // Dispose current content control safely
            if (_currentControl != null)
            {
                _contentPanel.Controls.Remove(_currentControl);
                _currentControl.Dispose();
                _currentControl = null;
            }

            // Instantiate corresponding UserControl
            UserControl? newControl = page switch
            {
                "Dashboard" => new Controls.DashboardControl(),
                "Users" => new Controls.UserManagementControl(),
                "Servers" => currentRole == AdminRole.ServerAdmin ? new Controls.ServerManagementControl() : null,
                "Reports" => new Controls.ReportManagementControl(),
                "Admin Roles" => currentRole == AdminRole.SuperAdmin ? new Controls.AdminRoleControl() : null,
                _ => null
            };

            if (newControl != null)
            {
                newControl.Dock = DockStyle.Fill;
                _contentPanel.Controls.Add(newControl);
                _currentControl = newControl;
            }
        }

        private void RoleCombo_SelectedIndexChanged(object? sender, EventArgs e)
        {
            var adminService = MockAdminService.Instance;
            var roleName = _roleCombo.SelectedItem?.ToString() ?? "SuperAdmin";

            var role = roleName switch
            {
                "SuperAdmin" => AdminRole.SuperAdmin,
                "UserAdmin" => AdminRole.UserAdmin,
                "ServerAdmin" => AdminRole.ServerAdmin,
                _ => AdminRole.SuperAdmin
            };

            adminService.CurrentAdmin = adminService.Admins.FirstOrDefault(a => a.Role == role)
                                        ?? new AdminUser { Id = 99, Name = $"Admin ({role})", Role = role, Email = $"{role.ToString().ToLower()}@um.com" };
            adminService.CurrentAdmin.Role = role;

            _adminNameLabel.Text = adminService.CurrentAdmin.Name;
            _adminRoleLabel.Text = role.ToString();
            _headerSubtitle.Text = $"Logged in as: {adminService.CurrentAdmin.Name} ({adminService.CurrentAdmin.Role}) | {adminService.CurrentAdmin.Email}";

            ApplyRoleBasedNavigation();

            // Always navigate safely back to Dashboard on role change to prevent viewing forbidden pages
            NavigateTo("Dashboard");
        }

        private void ApplyRoleBasedNavigation()
        {
            if (_navContainer == null) return;

            var role = MockAdminService.Instance.CurrentAdmin.Role;

            Button[] activeButtons = role switch
            {
                AdminRole.UserAdmin => new[] { _btnDashboard, _btnUsers, _btnReports },
                AdminRole.ServerAdmin => new[] { _btnDashboard, _btnServers, _btnReports },
                AdminRole.SuperAdmin => new[] { _btnDashboard, _btnReports, _btnAdminRoles },
                _ => new[] { _btnDashboard, _btnReports }
            };

            _navContainer.SuspendLayout();

            // Bỏ hết nút cũ ra khỏi container (không Dispose vì các nút được tái sử dụng)
            foreach (var btn in _navButtons)
                _navContainer.Controls.Remove(btn);

            // Dock=Top xếp chồng theo thứ tự ngược: add sau cùng thì hiện trên cùng
            for (int i = activeButtons.Length - 1; i >= 0; i--)
            {
                var btn = activeButtons[i];
                btn.Dock = DockStyle.Top;
                btn.Visible = true;
                _navContainer.Controls.Add(btn);
            }

            _navContainer.ResumeLayout(true);
            _navContainer.PerformLayout();

            System.Diagnostics.Debug.WriteLine($"[NAV DEBUG] Role={role}, _navContainer.Controls.Count={_navContainer.Controls.Count}, " +
                $"_navContainer.Size={_navContainer.Size}, _navContainer.Visible={_navContainer.Visible}, " +
                $"_sidebarPanel.Size={_sidebarPanel.Size}");
            Console.WriteLine($"[NAV DEBUG] Role={role}, _navContainer.Controls.Count={_navContainer.Controls.Count}, " +
                $"_navContainer.Size={_navContainer.Size}, _navContainer.Visible={_navContainer.Visible}, " +
                $"_sidebarPanel.Size={_sidebarPanel.Size}");
            foreach (Control c in _navContainer.Controls)
            {
                System.Diagnostics.Debug.WriteLine($"  -> child: {c.Text}, Dock={c.Dock}, Visible={c.Visible}, Size={c.Size}, Location={c.Location}");
                Console.WriteLine($"  -> child: {c.Text}, Dock={c.Dock}, Visible={c.Visible}, Size={c.Size}, Location={c.Location}");
            }
        }
    }
}
