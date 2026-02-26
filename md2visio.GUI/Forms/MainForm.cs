using md2visio.GUI.Services;
using System.ComponentModel;
using System.Diagnostics;
using System.Threading.Tasks;

namespace md2visio.GUI.Forms
{
    /// <summary>
    /// md2visio Main Window
    /// </summary>
    public partial class MainForm : Form
    {
        private readonly ConversionService _conversionService;

        // Control declarations
        private Panel _dragDropPanel = null!;
        private Label _dragDropLabel = null!;
        private Label _selectedFileLabel = null!;
        private TextBox _outputDirTextBox = null!;
        private TextBox _fileNameTextBox = null!;
        private CheckBox _showVisioCheckBox = null!;
        private CheckBox _silentOverwriteCheckBox = null!;
        private RichTextBox _logTextBox = null!;
        private ProgressBar _progressBar = null!;
        private Label _statusLabel = null!;
        private Button _browseFileButton = null!;
        private Button _selectDirButton = null!;
        private Button _startConversionButton = null!;
        private Button _openOutputButton = null!;
        private Button _clearLogButton = null!;


        private string? _selectedFilePath;

        public MainForm()
        {
            _conversionService = new ConversionService();
            _conversionService.ProgressChanged += OnProgressChanged;
            _conversionService.LogMessage += OnLogMessage;

            InitializeComponent();
            SetupEventHandlers();
            UpdateUI();
        }

        private void InitializeComponent()
        {
            // Window settings
            Text = "md2visio - Mermaid to Visio Tool";
            Size = new Size(1250, 850);
            StartPosition = FormStartPosition.CenterScreen;
            MinimumSize = new Size(600, 500);

            // Create main panel
            var mainPanel = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 1,
                RowCount = 7,
                Padding = new Padding(10)
            };

            // Set row styles
            mainPanel.RowStyles.Add(new RowStyle(SizeType.Absolute, 40)); // Title
            mainPanel.RowStyles.Add(new RowStyle(SizeType.Absolute, 120)); // File selection area
            mainPanel.RowStyles.Add(new RowStyle(SizeType.Absolute, 120)); // Output settings
            mainPanel.RowStyles.Add(new RowStyle(SizeType.Absolute, 80)); // Options
            mainPanel.RowStyles.Add(new RowStyle(SizeType.AutoSize)); // Supported types
            mainPanel.RowStyles.Add(new RowStyle(SizeType.Percent, 100)); // Log area
            mainPanel.RowStyles.Add(new RowStyle(SizeType.Absolute, 80)); // Buttons and status bar

            Controls.Add(mainPanel);

            // Create areas
            CreateTitleArea(mainPanel, 0);
            CreateFileSelectionArea(mainPanel, 1);
            CreateOutputSettingsArea(mainPanel, 2);
            CreateOptionsArea(mainPanel, 3);
            CreateSupportedTypesArea(mainPanel, 4);
            CreateLogArea(mainPanel, 5);
            CreateStatusArea(mainPanel, 6);
        }

        private void CreateTitleArea(TableLayoutPanel parent, int row)
        {
            var titleLabel = new Label
            {
                Text = "📄 md2visio - Mermaid to Visio Tool",
                Font = new Font("Microsoft YaHei UI", 12, FontStyle.Bold),
                ForeColor = Color.DarkBlue,
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleLeft
            };

            parent.Controls.Add(titleLabel, 0, row);
        }

        private void CreateFileSelectionArea(TableLayoutPanel parent, int row)
        {
            var groupBox = new GroupBox
            {
                Text = "📁 Input File",
                Dock = DockStyle.Fill,
                Font = new Font("Microsoft YaHei UI", 9, FontStyle.Bold)
            };

            var container = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 2,
                RowCount = 2,
                Padding = new Padding(10)
            };
            container.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 80));
            container.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 20));
            container.RowStyles.Add(new RowStyle(SizeType.Percent, 70));
            container.RowStyles.Add(new RowStyle(SizeType.Percent, 30));

            // Drag and drop area
            _dragDropPanel = new Panel
            {
                BorderStyle = BorderStyle.FixedSingle,
                BackColor = Color.LightGray,
                Dock = DockStyle.Fill,
                AllowDrop = true
            };

            _dragDropLabel = new Label
            {
                Text = "Drag .md file here or click 'Browse' to select",
                TextAlign = ContentAlignment.MiddleCenter,
                Dock = DockStyle.Fill,
                Font = new Font("Microsoft YaHei UI", 10)
            };
            _dragDropPanel.Controls.Add(_dragDropLabel);

            // Browse button
            _browseFileButton = new Button
            {
                Text = "Browse...",
                Dock = DockStyle.Fill,
                Font = new Font("Microsoft YaHei UI", 9),
                Margin = new Padding(10, 0, 0, 0)
            };

            // Selected file display
            _selectedFileLabel = new Label
            {
                Text = "No file selected",
                Dock = DockStyle.Fill,
                ForeColor = Color.Gray,
                Font = new Font("Microsoft YaHei UI", 8)
            };

            container.Controls.Add(_dragDropPanel, 0, 0);
            container.Controls.Add(_browseFileButton, 1, 0);
            container.Controls.Add(_selectedFileLabel, 0, 1);
            container.SetColumnSpan(_selectedFileLabel, 2);

            groupBox.Controls.Add(container);
            parent.Controls.Add(groupBox, 0, row);
        }

        private void CreateOutputSettingsArea(TableLayoutPanel parent, int row)
        {
            var groupBox = new GroupBox
            {
                Text = "📂 Output Settings",
                Dock = DockStyle.Fill,
                Font = new Font("Microsoft YaHei UI", 9, FontStyle.Bold)
            };

            var container = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 3,
                RowCount = 2,
                Padding = new Padding(10, 10, 10, 10)
            };
            container.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 100));
            container.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            container.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 120));
            container.RowStyles.Add(new RowStyle(SizeType.Absolute, 45));
            container.RowStyles.Add(new RowStyle(SizeType.Absolute, 45));

            // Output directory
            var outputDirLabel = new Label { Text = "Output Dir:", TextAlign = ContentAlignment.MiddleRight, Dock = DockStyle.Fill, Font = new Font("Microsoft YaHei UI", 9) };
            _outputDirTextBox = new TextBox { Text = Environment.GetFolderPath(Environment.SpecialFolder.Desktop), Dock = DockStyle.Fill, Font = new Font("Microsoft YaHei UI", 9) };
            _selectDirButton = new Button { Text = "Select...", Dock = DockStyle.Fill, Margin = new Padding(5, 0, 0, 0), Font = new Font("Microsoft YaHei UI", 9) };

            // File name
            var fileNameLabel = new Label { Text = "Filename:", TextAlign = ContentAlignment.MiddleRight, Dock = DockStyle.Fill, Font = new Font("Microsoft YaHei UI", 9) };
            _fileNameTextBox = new TextBox { Text = "output", Dock = DockStyle.Fill, Font = new Font("Microsoft YaHei UI", 9) };
            var extensionLabel = new Label { Text = ".vsdx", TextAlign = ContentAlignment.MiddleLeft, Dock = DockStyle.Fill, Font = new Font("Microsoft YaHei UI", 9) };

            container.Controls.Add(outputDirLabel, 0, 0);
            container.Controls.Add(_outputDirTextBox, 1, 0);
            container.Controls.Add(_selectDirButton, 2, 0);
            container.Controls.Add(fileNameLabel, 0, 1);
            container.Controls.Add(_fileNameTextBox, 1, 1);
            container.Controls.Add(extensionLabel, 2, 1);

            groupBox.Controls.Add(container);
            parent.Controls.Add(groupBox, 0, row);
        }

        private void CreateOptionsArea(TableLayoutPanel parent, int row)
        {
            var groupBox = new GroupBox
            {
                Text = "⚙️ Conversion Options",
                Dock = DockStyle.Fill,
                Font = new Font("Microsoft YaHei UI", 9, FontStyle.Bold)
            };

            var container = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                FlowDirection = FlowDirection.LeftToRight,
                Padding = new Padding(10, 20, 10, 20),
                WrapContents = false
            };

            _showVisioCheckBox = new CheckBox
            {
                Text = "Show Visio Window",
                AutoSize = true,
                Font = new Font("Microsoft YaHei UI", 9),
                Margin = new Padding(0, 0, 30, 0)
            };

            _silentOverwriteCheckBox = new CheckBox
            {
                Text = "Silent Overwrite",
                AutoSize = true,
                Font = new Font("Microsoft YaHei UI", 9),
                Checked = true
            };

            container.Controls.Add(_showVisioCheckBox);
            container.Controls.Add(_silentOverwriteCheckBox);

            groupBox.Controls.Add(container);
            parent.Controls.Add(groupBox, 0, row);
        }

        private void CreateSupportedTypesArea(TableLayoutPanel parent, int row)
        {
            var groupBox = new GroupBox
            {
                Text = "📊 Supported Diagram Types",
                Dock = DockStyle.Top,
                Font = new Font("Microsoft YaHei UI", 9, FontStyle.Bold),
                AutoSize = true,
                AutoSizeMode = AutoSizeMode.GrowAndShrink
            };

            var container = new FlowLayoutPanel
            {
                Dock = DockStyle.Top,
                FlowDirection = FlowDirection.LeftToRight,
                WrapContents = true,
                Padding = new Padding(10, 15, 10, 15),
                AutoSize = true,
                AutoSizeMode = AutoSizeMode.GrowAndShrink
            };

            // Create individual type labels
            var supportedTypes = new[]
            {
                ("✅ Flowchart", "graph/flowchart"),
                ("✅ Pie Chart", "pie"),
                ("✅ Journey Map", "journey"),
                ("✅ Packet", "packet"),
                ("✅ XY Chart", "xychart"),
                ("✅ Sequence", "sequence"),
                ("✅ ER Diagram", "er")
            };

            foreach (var (icon, name) in supportedTypes)
            {
                var label = new Label
                {
                    Text = $"{icon} {name}",
                    AutoSize = true,
                    Font = new Font("Microsoft YaHei UI", 9),
                    ForeColor = icon.StartsWith("✅") ? Color.DarkGreen : Color.Red,
                    Margin = new Padding(0, 5, 15, 5)
                };
                container.Controls.Add(label);
            }

            groupBox.Controls.Add(container);
            parent.Controls.Add(groupBox, 0, row);

            void SyncSupportedTypesWidth()
            {
                // FlowLayoutPanel needs constrained width to correctly calculate height after wrapping
                int width = groupBox.ClientSize.Width - container.Margin.Horizontal - container.Padding.Horizontal;
                if (width > 0)
                    container.MaximumSize = new Size(width, 0);
            }

            groupBox.SizeChanged += (_, __) => SyncSupportedTypesWidth();
            groupBox.HandleCreated += (_, __) => SyncSupportedTypesWidth();
        }

        private void CreateLogArea(TableLayoutPanel parent, int row)
        {
            var groupBox = new GroupBox
            {
                Text = "📝 Conversion Log",
                Dock = DockStyle.Fill,
                Font = new Font("Microsoft YaHei UI", 9, FontStyle.Bold)
            };

            var container = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 2,
                RowCount = 1,
                Padding = new Padding(5)
            };
            container.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            container.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 120));

            _logTextBox = new RichTextBox
            {
                Dock = DockStyle.Fill,
                ReadOnly = true,
                Font = new Font("Consolas", 9),
                BackColor = Color.Black,
                ForeColor = Color.Lime
            };

            _clearLogButton = new Button
            {
                Text = "Clear Log",
                Dock = DockStyle.Fill,
                Font = new Font("Microsoft YaHei UI", 9),
                Margin = new Padding(5, 5, 0, 5),
                MinimumSize = new Size(85, 30)
            };

            container.Controls.Add(_logTextBox, 0, 0);
            container.Controls.Add(_clearLogButton, 1, 0);

            groupBox.Controls.Add(container);
            parent.Controls.Add(groupBox, 0, row);
        }

        private void CreateStatusArea(TableLayoutPanel parent, int row)
        {
            var container = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 6,
                RowCount = 2
            };
            container.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 180));
            container.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 180));
            container.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 180));
            container.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 180));
            container.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
            container.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            container.RowStyles.Add(new RowStyle(SizeType.Absolute, 40));
            container.RowStyles.Add(new RowStyle(SizeType.Percent, 100));

            // Buttons
            _startConversionButton = new Button
            {
                Text = "🚀 Start",
                Dock = DockStyle.Fill,
                BackColor = Color.LightGreen,
                Font = new Font("Microsoft YaHei UI", 9, FontStyle.Bold),
                Margin = new Padding(0, 0, 5, 0)
            };

            var checkVisioButton = new Button
            {
                Text = "🔍 Check Visio",
                Dock = DockStyle.Fill,
                BackColor = Color.LightBlue,
                Font = new Font("Microsoft YaHei UI", 9, FontStyle.Bold),
                Margin = new Padding(0, 0, 5, 0)
            };
            checkVisioButton.Click += OnCheckVisioClick;

            _openOutputButton = new Button
            {
                Text = "📁 Open Output",
                Dock = DockStyle.Fill,
                Enabled = false,
                Margin = new Padding(0, 0, 5, 0)
            };

            var exitButton = new Button
            {
                Text = "❌ Exit",
                Dock = DockStyle.Fill,
                BackColor = Color.LightCoral,
                Margin = new Padding(0, 0, 5, 0)
            };
            exitButton.Click += (s, e) => Close();

            // Status label
            _statusLabel = new Label
            {
                Text = "Ready",
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleLeft,
                Font = new Font("Microsoft YaHei UI", 9)
            };

            // Progress bar
            _progressBar = new ProgressBar
            {
                Dock = DockStyle.Fill,
                Visible = false
            };

            container.Controls.Add(_startConversionButton, 0, 0);
            container.Controls.Add(checkVisioButton, 1, 0);
            container.Controls.Add(_openOutputButton, 2, 0);
            container.Controls.Add(exitButton, 3, 0);
            container.Controls.Add(_statusLabel, 4, 0);

            var authorLabel = new LinkLabel
            {
                Text = "© konbakuyomu",
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleRight,
                Font = new Font("Microsoft YaHei UI", 9)
            };
            authorLabel.Links.Add(0, authorLabel.Text.Length, "https://github.com/konbakuyomu/md2visio-gui/");
            authorLabel.LinkClicked += (s, e) => {
                Process.Start(new ProcessStartInfo(e.Link.LinkData.ToString()) { UseShellExecute = true });
            };

            container.Controls.Add(authorLabel, 5, 0);
            container.Controls.Add(_progressBar, 0, 1);
            container.SetColumnSpan(_progressBar, 6);

            parent.Controls.Add(container, 0, row);
        }

        private void SetupEventHandlers()
        {
            // Drag events
            _dragDropPanel.DragEnter += OnDragEnter;
            _dragDropPanel.DragDrop += OnDragDrop;
            _dragDropPanel.Click += OnDragPanelClick;

            // Button events
            _browseFileButton.Click += OnBrowseFileClick;
            _selectDirButton.Click += OnSelectDirClick;
            _startConversionButton.Click += OnStartConversionClick;
            _openOutputButton.Click += OnOpenOutputClick;
            _clearLogButton.Click += OnClearLogClick;

            // Filename auto-update
            _selectedFileLabel.TextChanged += OnSelectedFileChanged;
        }

        private void OnDragEnter(object? sender, DragEventArgs e)
        {
            if (e.Data?.GetDataPresent(DataFormats.FileDrop) == true)
            {
                e.Effect = DragDropEffects.Copy;
                _dragDropPanel.BackColor = Color.LightBlue;
            }
        }

        private void OnDragDrop(object? sender, DragEventArgs e)
        {
            _dragDropPanel.BackColor = Color.LightGray;
            
            if (e.Data?.GetData(DataFormats.FileDrop) is string[] files && files.Length > 0)
            {
                var file = files[0];
                if (Path.GetExtension(file).Equals(".md", StringComparison.OrdinalIgnoreCase))
                {
                    SetSelectedFile(file);
                }
                else
                {
                    MessageBox.Show("Please select a .md file!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
            }
        }

        private void OnDragPanelClick(object? sender, EventArgs e)
        {
            OnBrowseFileClick(sender, e);
        }

        private void OnBrowseFileClick(object? sender, EventArgs e)
        {
            using var dialog = new OpenFileDialog
            {
                Filter = "Markdown File|*.md|All Files|*.*",
                Title = "Select Markdown File"
            };

            if (dialog.ShowDialog() == DialogResult.OK)
            {
                SetSelectedFile(dialog.FileName);
            }
        }

        private void OnSelectDirClick(object? sender, EventArgs e)
        {
            using var dialog = new FolderBrowserDialog
            {
                Description = "Select Output Directory",
                SelectedPath = _outputDirTextBox.Text
            };

            if (dialog.ShowDialog() == DialogResult.OK)
            {
                _outputDirTextBox.Text = dialog.SelectedPath;
            }
        }

        private async void OnStartConversionClick(object? sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(_selectedFilePath))
            {
                MessageBox.Show("Please select a file to convert first!", "Tip", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            if (string.IsNullOrEmpty(_outputDirTextBox.Text))
            {
                MessageBox.Show("Please select an output directory!", "Tip", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            SetUIBusy(true);

            try
            {
                var result = await _conversionService.ConvertAsync(
                    _selectedFilePath,
                    _outputDirTextBox.Text,
                    _fileNameTextBox.Text, // Pass user-set filename
                    _showVisioCheckBox.Checked,
                    _silentOverwriteCheckBox.Checked
                );

                if (result.IsSuccess)
                {
                    _openOutputButton.Enabled = true;
                    ShowUserMessage(
                        $"Conversion Successful!\nGenerated {result.OutputFiles?.Length} files.",
                        "Success",
                        MessageBoxIcon.Information);
                }
                else
                {
                    ShowUserMessage(
                        $"Conversion Failed!\nError: {result.ErrorMessage}",
                        "Error",
                        MessageBoxIcon.Error);
                }
            }
            catch (Exception ex)
            {
                ShowUserMessage(
                    $"Error during conversion:\n{ex.Message}",
                    "Error",
                    MessageBoxIcon.Error);
            }
            finally
            {
                SetUIBusy(false);
            }
        }

        private void OnOpenOutputClick(object? sender, EventArgs e)
        {
            if (Directory.Exists(_outputDirTextBox.Text))
            {
                Process.Start("explorer.exe", _outputDirTextBox.Text);
            }
        }

        private void OnClearLogClick(object? sender, EventArgs e)
        {
            _logTextBox.Clear();
        }

        private void OnSelectedFileChanged(object? sender, EventArgs e)
        {
            if (!string.IsNullOrEmpty(_selectedFilePath))
            {
                var fileName = Path.GetFileNameWithoutExtension(_selectedFilePath);
                _fileNameTextBox.Text = fileName;
            }
        }

        private void SetSelectedFile(string filePath)
        {
            _selectedFilePath = filePath;
            _selectedFileLabel.Text = $"Selected: {filePath}";
            _selectedFileLabel.ForeColor = Color.Green;

            // Detect diagram type
            var types = _conversionService.DetectMermaidTypes(filePath);
            if (types.Count > 0)
            {
                LogMessage($"Detected diagram types: {string.Join(", ", types)}");
            }

            UpdateUI();
        }

        private void SetUIBusy(bool busy)
        {
            _startConversionButton.Enabled = !busy;
            _browseFileButton.Enabled = !busy;
            _selectDirButton.Enabled = !busy;
            _progressBar.Visible = busy;
            
            if (busy)
            {
                _statusLabel.Text = "Converting...";
                _progressBar.Value = 0;
            }
            else
            {
                _statusLabel.Text = "Ready";
            }
        }

        private void UpdateUI()
        {
            _startConversionButton.Enabled = !string.IsNullOrEmpty(_selectedFilePath);
        }

        private void OnProgressChanged(object? sender, ConversionProgressEventArgs e)
        {
            if (InvokeRequired)
            {
                Invoke(new Action(() => OnProgressChanged(sender, e)));
                return;
            }

            _progressBar.Value = e.Percentage;
            _statusLabel.Text = e.Message;
        }

        private void OnLogMessage(object? sender, ConversionLogEventArgs e)
        {
            if (InvokeRequired)
            {
                Invoke(new Action(() => OnLogMessage(sender, e)));
                return;
            }

            LogMessage($"[{e.Timestamp:HH:mm:ss}] {e.Message}");
        }

        private void LogMessage(string message)
        {
            _logTextBox.AppendText($"{message}\n");
            _logTextBox.ScrollToCaret();
        }

        private void ShowUserMessage(string message, string caption, MessageBoxIcon icon)
        {
            if (WindowState == FormWindowState.Minimized)
            {
                WindowState = FormWindowState.Normal;
            }
            Activate();
            BringToFront();
            MessageBox.Show(this, message, caption, MessageBoxButtons.OK, icon);
        }

        private async void OnCheckVisioClick(object? sender, EventArgs e)
        {
            SetUIBusy(true);
            _statusLabel.Text = "Checking Visio environment...";

            try
            {
                var result = await Task.Run(() => _conversionService.CheckVisioAvailability());
                
                if (result.IsSuccess)
                {
                    MessageBox.Show($"✅ Visio check passed!\n\n{string.Join("\n", result.OutputFiles ?? new string[0])}",
                        "Environment Check Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    _statusLabel.Text = "Visio Normal";
                }
                else
                {
                    MessageBox.Show($"❌ Visio check failed!\n\n{result.ErrorMessage}",
                        "Environment Check Failed", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    _statusLabel.Text = "Visio Abnormal";
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Exception during check:\n{ex.Message}", "Exception", MessageBoxButtons.OK, MessageBoxIcon.Error);
                _statusLabel.Text = "Check Exception";
            }
            finally
            {
                SetUIBusy(false);
            }
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            // Release resources held by service, e.g., Visio COM object
            _conversionService.Dispose();
            base.OnFormClosing(e);
        }
    }
} 
