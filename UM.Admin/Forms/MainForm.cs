using UM.Admin.Models;
using UM.Admin.Services;

namespace UM.Admin.Forms
{
    public partial class MainForm : Form
    {
        private Panel _sidebarPanel = null!;
        private Panel _headerPanel = null!;
        private Panel _contentPanel = null!;
        private Label _headerTitle = null!;
        private Label _headerSubtitle = null!;
        private Label _adminNameLabel = null!;
        private Label _adminRoleLabel = null!;
        private ComboBox _roleCombo = null!;
        private readonly List<Button> _navButtons = new();
        private UserControl? _currentControl;

        // Colors
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
            ApplyRoleBasedNavigation();
            NavigateTo("Dashboard");
        }

        private void SetupForm()
        {
            Text = "UM Admin Panel";
            Size = new Size(1400, 850);
            MinimumSize = new Size(1100, 700);
            StartPosition = FormStartPosition.CenterScreen;
            BackColor = ContentBg;
            Font = new Font("Segoe UI", 9F);
            DoubleBuffered = true;
        }

        private void BuildLayout()
        {
            // ===== SIDEBAR =====
            _sidebarPanel = new Panel
            {
                Dock = DockStyle.Left,
                Width = 240,
                BackColor = SidebarBg,
            };

            // Logo area
            var logoPanel = new Panel { Dock = DockStyle.Top, Height = 64, BackColor = Color.FromArgb(24, 24, 38) };
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

            // Role switcher (for mock testing)
            var roleSwitchPanel = new Panel { Dock = DockStyle.Top, Height = 70, Padding = new Padding(12, 8, 12, 4) };
            var roleLabel = new Label
            {
                Text = "Mock Role:",
                ForeColor = TextMuted,
                Font = new Font("Segoe UI", 8F),
                AutoSize = false,
                Height = 18,
                Dock = DockStyle.Top,
            };
            _roleCombo = new ComboBox
            {
                Dock = DockStyle.Top,
                DropDownStyle = ComboBoxStyle.DropDownList,
                Font = new Font("Segoe UI", 9F),
                Height = 28,
            };
            _roleCombo.Items.AddRange(new object[] { "SuperAdmin", "UserAdmin", "ServerAdmin" });
            _roleCombo.SelectedIndex = 0;
            _roleCombo.SelectedIndexChanged += RoleCombo_SelectedIndexChanged;
            roleSwitchPanel.Controls.Add(_roleCombo);
            roleSwitchPanel.Controls.Add(roleLabel);
            _sidebarPanel.Controls.Add(roleSwitchPanel);

            // Navigation buttons
            var navContainer = new Panel { Dock = DockStyle.Fill, Padding = new Padding(0, 8, 0, 0) };
            var navFlow = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                FlowDirection = FlowDirection.TopDown,
                WrapContents = false,
                AutoScroll = true,
                Padding = new Padding(0),
            };

            // Section: Main
            navFlow.Controls.Add(CreateSectionLabel("MAIN"));
            navFlow.Controls.Add(CreateNavButton("Dashboard", "\U0001F4CA"));
            navFlow.Controls.Add(CreateNavButton("Users", "\U0001F465"));
            navFlow.Controls.Add(CreateNavButton("Servers", "\U0001F5A5"));
            navFlow.Controls.Add(CreateNavButton("Reports", "\U0001F4CB"));

            // Section: Administration
            navFlow.Controls.Add(CreateSectionLabel("ADMINISTRATION"));
            navFlow.Controls.Add(CreateNavButton("User Management", "\U0001F465"));
            navFlow.Controls.Add(CreateNavButton("Admin Roles", "\U0001F511"));

            navContainer.Controls.Add(navFlow);
            _sidebarPanel.Controls.Add(navContainer);

            // Admin info at bottom of sidebar
            var adminInfoPanel = new Panel
            {
                Dock = DockStyle.Bottom,
                Height = 60,
                BackColor = Color.FromArgb(24, 24, 38),
                Padding = new Padding(12, 8, 12, 8),
            };
            _adminNameLabel = new Label
            {
                Text = MockAdminService.Instance.CurrentAdmin.Name,
                ForeColor = TextWhite,
                Font = new Font("Segoe UI", 9.5F, FontStyle.Bold),
                AutoSize = false,
                Height = 20,
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

            // ===== HEADER =====
            _headerPanel = new Panel
            {
                Dock = DockStyle.Top,
                Height = 56,
                BackColor = HeaderBg,
                Padding = new Padding(24, 0, 24, 0),
            };
            // Header bottom border
            var headerBorder = new Panel { Dock = DockStyle.Bottom, Height = 1, BackColor = BorderColor };
            _headerTitle = new Label
            {
                Text = "Dashboard",
                Font = new Font("Segoe UI", 14F, FontStyle.Bold),
                ForeColor = TextDark,
                AutoSize = false,
                TextAlign = ContentAlignment.MiddleLeft,
                Dock = DockStyle.Left,
                Width = 300,
            };
            _headerSubtitle = new Label
            {
                Text = MockAdminService.Instance.CurrentAdmin.Email,
                Font = new Font("Segoe UI", 9F),
                ForeColor = Color.FromArgb(130, 130, 140),
                AutoSize = false,
                TextAlign = ContentAlignment.MiddleRight,
                Dock = DockStyle.Right,
                Width = 300,
            };
            _headerPanel.Controls.Add(_headerTitle);
            _headerPanel.Controls.Add(_headerSubtitle);
            _headerPanel.Controls.Add(headerBorder);

            // ===== CONTENT AREA =====
            _contentPanel = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = ContentBg,
                Padding = new Padding(0),
            };

            // Add to form (order matters for Dock)
            Controls.Add(_contentPanel);
            Controls.Add(_headerPanel);
            Controls.Add(_sidebarPanel);
        }

        private Label CreateSectionLabel(string text)
        {
            return new Label
            {
                Text = text,
                Font = new Font("Segoe UI", 7.5F, FontStyle.Bold),
                ForeColor = TextMuted,
                AutoSize = false,
                Height = 32,
                Width = 240,
                Padding = new Padding(20, 12, 0, 0),
                Margin = new Padding(0),
            };
        }

        private Button CreateNavButton(string text, string icon)
        {
            var btn = new Button
            {
                Text = $"  {icon}  {text}",
                Tag = text,
                Font = new Font("Segoe UI", 10F),
                ForeColor = TextWhite,
                FlatStyle = FlatStyle.Flat,
                BackColor = SidebarBg,
                TextAlign = ContentAlignment.MiddleLeft,
                Height = 40,
                Width = 240,
                Padding = new Padding(12, 0, 0, 0),
                Margin = new Padding(0, 1, 0, 1),
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

        private void NavigateTo(string page)
        {
            _headerTitle.Text = page;

            // Update active nav button
            foreach (var btn in _navButtons)
            {
                btn.BackColor = (btn.Tag?.ToString() == page) ? SidebarActive : SidebarBg;
            }

            // Dispose current control
            if (_currentControl != null)
            {
                _contentPanel.Controls.Remove(_currentControl);
                _currentControl.Dispose();
                _currentControl = null;
            }

            // Create new content
            UserControl? newControl = page switch
            {
                "Dashboard" => new Controls.DashboardControl(),
                "Users" => new Controls.UserManagementControl(),
                "User Management" => new Controls.UserManagementControl(),
                "Servers" => new Controls.ServerManagementControl(),
                "Reports" => new Controls.ReportManagementControl(),
                "Admin Roles" => new Controls.AdminRoleControl(),
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
                                        ?? adminService.Admins[0];
            adminService.CurrentAdmin.Role = role;

            _adminNameLabel.Text = adminService.CurrentAdmin.Name;
            _adminRoleLabel.Text = role.ToString();
            _headerSubtitle.Text = adminService.CurrentAdmin.Email;

            ApplyRoleBasedNavigation();
            NavigateTo("Dashboard");
        }

        private void ApplyRoleBasedNavigation()
        {
            var role = MockAdminService.Instance.CurrentAdmin.Role;

            foreach (var btn in _navButtons)
            {
                var page = btn.Tag?.ToString();
                btn.Visible = page switch
                {
                    "Dashboard" => true,
                    "Users" => role == AdminRole.SuperAdmin,
                    "User Management" => role == AdminRole.UserAdmin,
                    "Servers" => role == AdminRole.SuperAdmin || role == AdminRole.ServerAdmin,
                    "Reports" => true,
                    "Admin Roles" => role == AdminRole.SuperAdmin,
                    _ => true,
                };
            }
        }
    }
}
