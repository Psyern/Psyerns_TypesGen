using DayZTypesHelper.Models;
using DayZTypesHelper.Services;

namespace DayZTypesHelper;

public sealed class MainForm : Form
{
    // ── Left side ─────────────────────────────────────────────────
    private SplitContainer splitMain = null!;
    private TextBox txtSearch = null!;
    private ListBox lstClasses = null!;
    private Panel pnlLeftButtons = null!;
    private Button btnImportClassList = null!;
    private Button btnImportCfgLimits = null!;
    private Button btnImportTypesXml = null!;
    private Button btnImportMarketJson = null!;
    private Button btnCreateMarketJson = null!;
    private Button btnClearList = null!;

    // ── Right side: file row ──────────────────────────────────────
    private Panel pnlRight = null!;
    private Panel pnlFile = null!;
    private Button btnSelectDestination = null!;
    private Button btnSelectMarketDest = null!;
    private Label lblDestination = null!;
    private Label lblMarketDest = null!;
    private Button btnExportNow = null!;
    private CheckBox chkExportAll = null!;
    private Button btnUndo = null!;
    private Button btnRedo = null!;
    private Button btnBulkApply = null!;
    private CheckBox chkDarkMode = null!;

    // ── Editor ────────────────────────────────────────────────────
    private TableLayoutPanel tlpEditor = null!;
    private NumericUpDown numNominal = null!;
    private NumericUpDown numLifetime = null!;
    private NumericUpDown numRestock = null!;
    private NumericUpDown numMin = null!;
    private NumericUpDown numQuantMin = null!;
    private NumericUpDown numQuantMax = null!;
    private NumericUpDown numCost = null!;

    private GroupBox grpFlags = null!;
    private CheckBox chkCountInCargo = null!;
    private CheckBox chkCountInHoarder = null!;
    private CheckBox chkCountInMap = null!;
    private CheckBox chkCountInPlayer = null!;
    private CheckBox chkCrafted = null!;
    private CheckBox chkDeloot = null!;

    private GroupBox grpCategories = null!;
    private GroupBox grpTags = null!;
    private GroupBox grpUsageFlags = null!;
    private GroupBox grpValueFlags = null!;
    private CheckedListBox clbCategories = null!;
    private CheckedListBox clbTags = null!;
    private CheckedListBox clbUsage = null!;
    private CheckedListBox clbValue = null!;

    // ── Market editor ─────────────────────────────────────────────
    private GroupBox grpMarket = null!;
    private TableLayoutPanel tlpMarket = null!;
    private NumericUpDown numMaxPrice = null!;
    private NumericUpDown numMinPrice = null!;
    private NumericUpDown numSellPricePercent = null!;
    private NumericUpDown numMaxStock = null!;
    private NumericUpDown numMinStock = null!;
    private NumericUpDown numQuantityPercent = null!;
    private TextBox txtSpawnAttachments = null!;
    private TextBox txtVariants = null!;

    private StatusStrip statusStrip = null!;
    private ToolStripStatusLabel lblStatus = null!;

    // ── Services / state ──────────────────────────────────────────
    private readonly System.Windows.Forms.Timer _autosaveTimer = new() { Interval = 500 };
    private readonly TypesXmlService _typesService = new();
    private readonly MarketJsonService _marketService = new();
    private readonly UndoRedoService _undoRedo = new();
    private readonly Dictionary<string, TypeEntry> _cache = new(StringComparer.OrdinalIgnoreCase);
    private readonly Dictionary<string, MarketItem> _marketCache = new(StringComparer.OrdinalIgnoreCase);

    private List<string> _allClasses = new();
    private string? _currentClassname;
    private bool _loadingUi;
    private bool _suppressSelectionHandler;
    private int _lastValidQuantMin = -1;
    private int _lastValidQuantMax = -1;

    public MainForm()
    {
        Text = "Psyerns Types Helper";
        Width = 1200;
        Height = 800;
        StartPosition = FormStartPosition.CenterScreen;
        KeyPreview = true;

        InitializeComponent();
        WireEvents();

        // Default to dark mode
        chkDarkMode.Checked = true;
        DarkModeHelper.Apply(this, true);

        SetStatus("Ready.");
    }

    private void InitializeComponent()
    {
        splitMain = new SplitContainer
        {
            Dock = DockStyle.Fill,
            Orientation = Orientation.Vertical,
            SplitterDistance = 300
        };

        txtSearch = new TextBox
        {
            Dock = DockStyle.Top,
            PlaceholderText = "Search..."
        };

        lstClasses = new ListBox
        {
            Dock = DockStyle.Fill,
            SelectionMode = SelectionMode.MultiExtended
        };

        pnlLeftButtons = new Panel
        {
            Dock = DockStyle.Bottom,
            AutoSize = true,
            AutoSizeMode = AutoSizeMode.GrowAndShrink
        };

        var flowLeftButtons = new FlowLayoutPanel
        {
            Dock = DockStyle.Fill,
            AutoSize = true,
            AutoSizeMode = AutoSizeMode.GrowAndShrink,
            WrapContents = true,
            Padding = new Padding(4)
        };

        btnImportClassList = new Button
        {
            Name = "btnImportClassList",
            Text = "Import Classnamelist",
            Width = 150,
            Height = 26,
            Margin = new Padding(2)
        };

        btnImportCfgLimits = new Button
        {
            Name = "btnImportCfgLimits",
            Text = "Import cfglimits.xml",
            Width = 150,
            Height = 26,
            Margin = new Padding(2)
        };

        btnImportTypesXml = new Button
        {
            Name = "btnImportTypesXml",
            Text = "Import types.xml",
            Width = 150,
            Height = 26,
            Margin = new Padding(2)
        };

        btnImportMarketJson = new Button
        {
            Name = "btnImportMarketJson",
            Text = "Import Market JSON",
            Width = 150,
            Height = 26,
            Margin = new Padding(2)
        };

        btnCreateMarketJson = new Button
        {
            Name = "btnCreateMarketJson",
            Text = "Create Market JSON",
            Width = 150,
            Height = 26,
            Margin = new Padding(2)
        };

        btnClearList = new Button
        {
            Name = "btnClearList",
            Text = "Clear List",
            Width = 150,
            Height = 26,
            Margin = new Padding(2)
        };

        flowLeftButtons.Controls.Add(btnImportClassList);
        flowLeftButtons.Controls.Add(btnImportCfgLimits);
        flowLeftButtons.Controls.Add(btnImportTypesXml);
        flowLeftButtons.Controls.Add(btnImportMarketJson);
        flowLeftButtons.Controls.Add(btnCreateMarketJson);
        flowLeftButtons.Controls.Add(btnClearList);
        pnlLeftButtons.Controls.Add(flowLeftButtons);

        splitMain.Panel1.Controls.Add(lstClasses);
        splitMain.Panel1.Controls.Add(txtSearch);
        splitMain.Panel1.Controls.Add(pnlLeftButtons);

        pnlRight = new Panel
        {
            Dock = DockStyle.Fill,
            AutoScroll = true
        };

        pnlFile = new Panel
        {
            Dock = DockStyle.Top,
            AutoSize = true,
            AutoSizeMode = AutoSizeMode.GrowAndShrink,
            Padding = new Padding(4)
        };

        // ── Row 1: Buttons (FlowLayout, wraps automatically) ──
        var flowButtons = new FlowLayoutPanel
        {
            Dock = DockStyle.Top,
            AutoSize = true,
            AutoSizeMode = AutoSizeMode.GrowAndShrink,
            WrapContents = true,
            Padding = new Padding(4)
        };

        btnSelectDestination = new Button
        {
            Name = "btnSelectDestination",
            Text = "Dest. types.xml",
            Width = 140,
            Height = 28,
            Margin = new Padding(2)
        };

        btnSelectMarketDest = new Button
        {
            Name = "btnSelectMarketDest",
            Text = "Dest. Market JSON",
            Width = 150,
            Height = 28,
            Margin = new Padding(2)
        };

        btnExportNow = new Button
        {
            Name = "btnExportNow",
            Text = "Export now",
            Width = 110,
            Height = 28,
            Margin = new Padding(2)
        };

        chkExportAll = new CheckBox
        {
            Name = "chkExportAll",
            Text = "Export all",
            Width = 90,
            Height = 28,
            Checked = false,
            Margin = new Padding(2, 6, 2, 2)
        };

        btnUndo = new Button
        {
            Name = "btnUndo",
            Text = "↩ Undo",
            Width = 80,
            Height = 28,
            Enabled = false,
            Margin = new Padding(2)
        };

        btnRedo = new Button
        {
            Name = "btnRedo",
            Text = "↪ Redo",
            Width = 80,
            Height = 28,
            Enabled = false,
            Margin = new Padding(2)
        };

        btnBulkApply = new Button
        {
            Name = "btnBulkApply",
            Text = "Bulk Apply to Selected",
            Width = 170,
            Height = 28,
            Enabled = false,
            Margin = new Padding(2)
        };

        chkDarkMode = new CheckBox
        {
            Name = "chkDarkMode",
            Text = "Dark Mode",
            Width = 100,
            Height = 28,
            Checked = false,
            Margin = new Padding(2, 6, 2, 2)
        };

        flowButtons.Controls.Add(btnSelectDestination);
        flowButtons.Controls.Add(btnSelectMarketDest);
        flowButtons.Controls.Add(btnExportNow);
        flowButtons.Controls.Add(chkExportAll);
        flowButtons.Controls.Add(btnUndo);
        flowButtons.Controls.Add(btnRedo);
        flowButtons.Controls.Add(btnBulkApply);
        flowButtons.Controls.Add(chkDarkMode);

        // ── Row 2: Destination label ──
        lblDestination = new Label
        {
            Name = "lblDestination",
            Text = "Types.xml: (none)",
            Dock = DockStyle.Top,
            AutoEllipsis = true,
            Padding = new Padding(6, 2, 6, 0),
            Height = 18
        };

        lblMarketDest = new Label
        {
            Name = "lblMarketDest",
            Text = "Market JSON: (none)",
            Dock = DockStyle.Top,
            AutoEllipsis = true,
            Padding = new Padding(6, 0, 6, 2),
            Height = 18
        };

        // ── Row 3: Hint label ──
        var lblMultiHint = new Label
        {
            Text = "Tip: Hold Ctrl/Shift in classlist for multi-select → then 'Bulk Apply'",
            Dock = DockStyle.Top,
            AutoSize = false,
            Height = 20,
            Padding = new Padding(6, 0, 6, 4),
            ForeColor = Color.Gray,
            Font = new Font(Font.FontFamily, 8f, FontStyle.Italic)
        };

        // Add in reverse order (Dock = Top stacks bottom-up)
        pnlFile.Controls.Add(lblMultiHint);
        pnlFile.Controls.Add(lblMarketDest);
        pnlFile.Controls.Add(lblDestination);
        pnlFile.Controls.Add(flowButtons);

        tlpEditor = new TableLayoutPanel
        {
            Name = "tlpEditor",
            Dock = DockStyle.Top,
            AutoSize = true,
            ColumnCount = 2,
            Padding = new Padding(8)
        };
        tlpEditor.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 180));
        tlpEditor.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));

        numNominal = new NumericUpDown { Name = "numNominal" };
        numLifetime = new NumericUpDown { Name = "numLifetime" };
        numRestock = new NumericUpDown { Name = "numRestock" };
        numMin = new NumericUpDown { Name = "numMin" };
        numQuantMin = new NumericUpDown { Name = "numQuantMin", Minimum = -1, Value = -1, Tag = -1 };
        numQuantMax = new NumericUpDown { Name = "numQuantMax", Minimum = -1, Value = -1, Tag = -1 };
        numCost = new NumericUpDown { Name = "numCost" };

        AddNumericRow("nominal", numNominal, 0, 1_000_000);
        AddNumericRow("lifetime", numLifetime, 0, 1_000_000);
        AddNumericRow("restock", numRestock, 0, 1_000_000);
        AddNumericRow("min", numMin, 0, 1_000_000);
        AddNumericRow("quantmin", numQuantMin, -1, 100);
        AddNumericRow("quantmax", numQuantMax, -1, 100);
        AddNumericRow("cost", numCost, 0, 1_000_000);

        grpFlags = new GroupBox { Name = "grpFlags", Text = "Flags", Dock = DockStyle.Top, AutoSize = true, Padding = new Padding(8) };
        var flagsFlow = new FlowLayoutPanel { Dock = DockStyle.Fill, AutoSize = true, WrapContents = true };
        chkCountInCargo = new CheckBox { Name = "chkCountInCargo", Text = "count_in_cargo", AutoSize = true };
        chkCountInHoarder = new CheckBox { Name = "chkCountInHoarder", Text = "count_in_hoarder", AutoSize = true };
        chkCountInMap = new CheckBox { Name = "chkCountInMap", Text = "count_in_map", AutoSize = true };
        chkCountInPlayer = new CheckBox { Name = "chkCountInPlayer", Text = "count_in_player", AutoSize = true };
        chkCrafted = new CheckBox { Name = "chkCrafted", Text = "crafted", AutoSize = true };
        chkDeloot = new CheckBox { Name = "chkDeloot", Text = "deloot", AutoSize = true };
        flagsFlow.Controls.AddRange(new Control[] { chkCountInCargo, chkCountInHoarder, chkCountInMap, chkCountInPlayer, chkCrafted, chkDeloot });
        grpFlags.Controls.Add(flagsFlow);

        grpCategories = new GroupBox { Name = "grpCategories", Text = "Categories", Dock = DockStyle.Top, Height = 180, Padding = new Padding(8) };
        grpTags = new GroupBox { Name = "grpTags", Text = "Tags", Dock = DockStyle.Top, Height = 180, Padding = new Padding(8) };
        grpUsageFlags = new GroupBox { Name = "grpUsageFlags", Text = "UsageFlags", Dock = DockStyle.Top, Height = 180, Padding = new Padding(8) };
        grpValueFlags = new GroupBox { Name = "grpValueFlags", Text = "ValueFlags", Dock = DockStyle.Top, Height = 180, Padding = new Padding(8) };

        clbCategories = new CheckedListBox { Name = "clbCategories", Dock = DockStyle.Fill, CheckOnClick = true };
        clbTags = new CheckedListBox { Name = "clbTags", Dock = DockStyle.Fill, CheckOnClick = true };
        clbUsage = new CheckedListBox { Name = "clbUsage", Dock = DockStyle.Fill, CheckOnClick = true };
        clbValue = new CheckedListBox { Name = "clbValue", Dock = DockStyle.Fill, CheckOnClick = true };

        grpCategories.Controls.Add(clbCategories);
        grpTags.Controls.Add(clbTags);
        grpUsageFlags.Controls.Add(clbUsage);
        grpValueFlags.Controls.Add(clbValue);

        AddFullWidthRow(grpFlags);
        AddFullWidthRow(grpCategories);
        AddFullWidthRow(grpTags);
        AddFullWidthRow(grpUsageFlags);
        AddFullWidthRow(grpValueFlags);

        // ── Market Editor ─────────────────────────────────────────
        grpMarket = new GroupBox
        {
            Name = "grpMarket",
            Text = "Market (Expansion JSON)",
            Dock = DockStyle.Top,
            AutoSize = true,
            Padding = new Padding(8)
        };

        tlpMarket = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            AutoSize = true,
            ColumnCount = 2,
            Padding = new Padding(4)
        };
        tlpMarket.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 160));
        tlpMarket.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));

        numMaxPrice = new NumericUpDown { Name = "numMaxPrice" };
        numMinPrice = new NumericUpDown { Name = "numMinPrice" };
        numSellPricePercent = new NumericUpDown { Name = "numSellPricePercent", Minimum = -1 };
        numMaxStock = new NumericUpDown { Name = "numMaxStock" };
        numMinStock = new NumericUpDown { Name = "numMinStock" };
        numQuantityPercent = new NumericUpDown { Name = "numQuantityPercent", Minimum = -1 };

        AddMarketRow("MaxPriceThreshold", numMaxPrice, 0, 10_000_000);
        AddMarketRow("MinPriceThreshold", numMinPrice, 0, 10_000_000);
        AddMarketRow("SellPricePercent", numSellPricePercent, -1, 100);
        AddMarketRow("MaxStockThreshold", numMaxStock, 0, 10_000_000);
        AddMarketRow("MinStockThreshold", numMinStock, 0, 10_000_000);
        AddMarketRow("QuantityPercent", numQuantityPercent, -1, 100);

        // SpawnAttachments (comma-separated)
        var lblSpawn = new Label { Text = "SpawnAttachments", AutoSize = true, Dock = DockStyle.Fill, Padding = new Padding(0, 6, 0, 0) };
        txtSpawnAttachments = new TextBox { Name = "txtSpawnAttachments", Dock = DockStyle.Fill, PlaceholderText = "comma-separated classnames" };
        var spawnRow = tlpMarket.RowCount++;
        tlpMarket.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        tlpMarket.Controls.Add(lblSpawn, 0, spawnRow);
        tlpMarket.Controls.Add(txtSpawnAttachments, 1, spawnRow);

        // Variants (comma-separated)
        var lblVariants = new Label { Text = "Variants", AutoSize = true, Dock = DockStyle.Fill, Padding = new Padding(0, 6, 0, 0) };
        txtVariants = new TextBox { Name = "txtVariants", Dock = DockStyle.Fill, PlaceholderText = "comma-separated classnames" };
        var varRow = tlpMarket.RowCount++;
        tlpMarket.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        tlpMarket.Controls.Add(lblVariants, 0, varRow);
        tlpMarket.Controls.Add(txtVariants, 1, varRow);

        grpMarket.Controls.Add(tlpMarket);
        AddFullWidthRow(grpMarket);

        statusStrip = new StatusStrip { Name = "statusStrip" };
        lblStatus = new ToolStripStatusLabel { Name = "lblStatus", Text = "Ready" };
        statusStrip.Items.Add(lblStatus);

        pnlRight.Controls.Add(tlpEditor);
        pnlRight.Controls.Add(pnlFile);

        splitMain.Panel2.Controls.Add(pnlRight);

        Controls.Add(splitMain);
        Controls.Add(statusStrip);
    }

    private void AddNumericRow(string label, NumericUpDown nud, int min, int max)
    {
        var row = tlpEditor.RowCount++;
        tlpEditor.RowStyles.Add(new RowStyle(SizeType.AutoSize));

        var lbl = new Label
        {
            Text = label,
            AutoSize = true,
            Dock = DockStyle.Fill,
            Padding = new Padding(0, 6, 0, 0)
        };

        nud.Minimum = min;
        nud.Maximum = max;
        nud.DecimalPlaces = 0;
        nud.Dock = DockStyle.Left;
        nud.Width = 160;

        tlpEditor.Controls.Add(lbl, 0, row);
        tlpEditor.Controls.Add(nud, 1, row);
    }

    private void AddFullWidthRow(Control control)
    {
        var row = tlpEditor.RowCount++;
        tlpEditor.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        tlpEditor.Controls.Add(control, 0, row);
        tlpEditor.SetColumnSpan(control, 2);
    }

    private void AddMarketRow(string label, NumericUpDown nud, int min, int max)
    {
        var row = tlpMarket.RowCount++;
        tlpMarket.RowStyles.Add(new RowStyle(SizeType.AutoSize));

        var lbl = new Label { Text = label, AutoSize = true, Dock = DockStyle.Fill, Padding = new Padding(0, 6, 0, 0) };
        nud.Minimum = min;
        nud.Maximum = max;
        nud.DecimalPlaces = 0;
        nud.Dock = DockStyle.Left;
        nud.Width = 160;

        tlpMarket.Controls.Add(lbl, 0, row);
        tlpMarket.Controls.Add(nud, 1, row);
    }

    private void WireEvents()
    {
        txtSearch.TextChanged += (_, _) => ApplyFilter();
        lstClasses.SelectedIndexChanged += (_, _) => OnClassSelectionChanged();

        btnImportClassList.Click += (_, _) => ImportClassList();
        btnImportCfgLimits.Click += (_, _) => ImportCfgLimits();
        btnImportTypesXml.Click += (_, _) => ImportTypesXml();
        btnImportMarketJson.Click += (_, _) => ImportMarketJson();
        btnCreateMarketJson.Click += (_, _) => CreateMarketJsonFromSelected();
        btnClearList.Click += (_, _) => ClearList();
        btnSelectDestination.Click += (_, _) => SelectDestination();
        btnSelectMarketDest.Click += (_, _) => SelectMarketDestination();
        btnExportNow.Click += (_, _) => ExportNow();
        chkExportAll.CheckedChanged += (_, _) => OnEditorChanged();

        btnUndo.Click += (_, _) => PerformUndo();
        btnRedo.Click += (_, _) => PerformRedo();
        btnBulkApply.Click += (_, _) => BulkApplyToSelected();
        chkDarkMode.CheckedChanged += (_, _) =>
        {
            DarkModeHelper.Apply(this, chkDarkMode.Checked);
        };

        numNominal.ValueChanged += (_, _) => OnEditorChanged();
        numLifetime.ValueChanged += (_, _) => OnEditorChanged();
        numRestock.ValueChanged += (_, _) => OnEditorChanged();
        numMin.ValueChanged += (_, _) => OnEditorChanged();
        numQuantMin.ValueChanged += (_, _) => OnQuantChanged(true);
        numQuantMax.ValueChanged += (_, _) => OnQuantChanged(false);
        numCost.ValueChanged += (_, _) => OnEditorChanged();

        chkCountInCargo.CheckedChanged += (_, _) => OnEditorChanged();
        chkCountInHoarder.CheckedChanged += (_, _) => OnEditorChanged();
        chkCountInMap.CheckedChanged += (_, _) => OnEditorChanged();
        chkCountInPlayer.CheckedChanged += (_, _) => OnEditorChanged();
        chkCrafted.CheckedChanged += (_, _) => OnEditorChanged();
        chkDeloot.CheckedChanged += (_, _) => OnEditorChanged();

        clbCategories.ItemCheck += (_, _) => BeginInvoke(OnEditorChanged);
        clbTags.ItemCheck += (_, _) => BeginInvoke(OnEditorChanged);
        clbUsage.ItemCheck += (_, _) => BeginInvoke(OnEditorChanged);
        clbValue.ItemCheck += (_, _) => BeginInvoke(OnEditorChanged);

        numMaxPrice.ValueChanged += (_, _) => OnEditorChanged();
        numMinPrice.ValueChanged += (_, _) => OnEditorChanged();
        numSellPricePercent.ValueChanged += (_, _) => OnEditorChanged();
        numMaxStock.ValueChanged += (_, _) => OnEditorChanged();
        numMinStock.ValueChanged += (_, _) => OnEditorChanged();
        numQuantityPercent.ValueChanged += (_, _) => OnEditorChanged();
        txtSpawnAttachments.TextChanged += (_, _) => OnEditorChanged();
        txtVariants.TextChanged += (_, _) => OnEditorChanged();

        _autosaveTimer.Tick += (_, _) => AutosaveTick();

        // Ctrl+Z / Ctrl+Y keyboard shortcuts
        KeyDown += (_, e) =>
        {
            if (e.Control && e.KeyCode == Keys.Z)
            {
                e.SuppressKeyPress = true;
                PerformUndo();
            }
            else if (e.Control && e.KeyCode == Keys.Y)
            {
                e.SuppressKeyPress = true;
                PerformRedo();
            }
        };

        FormClosing += (sender, e) =>
        {
            try
            {
                if (!SaveCurrentFromUiIntoCache())
                {
                    e.Cancel = true;
                    SetStatus("Cannot close: current values are invalid.");
                    return;
                }

                PersistToXml(force: true);
                PersistMarket(force: true);
            }
            catch (Exception ex)
            {
                e.Cancel = true;
                MessageBox.Show(this, ex.Message, "Save on close failed", MessageBoxButtons.OK, MessageBoxIcon.Error);
                SetStatus("Close canceled: save failed.");
            }
        };
    }

    private void SetStatus(string message) => lblStatus.Text = message;

    private void ApplyFilter()
    {
        var selected = lstClasses.SelectedItem as string;
        var query = txtSearch.Text.Trim();

        IEnumerable<string> filtered = _allClasses;
        if (!string.IsNullOrWhiteSpace(query))
        {
            filtered = filtered.Where(c => c.Contains(query, StringComparison.OrdinalIgnoreCase));
        }

        _suppressSelectionHandler = true;
        lstClasses.BeginUpdate();
        lstClasses.Items.Clear();
        foreach (var className in filtered)
        {
            lstClasses.Items.Add(className);
        }

        if (!string.IsNullOrWhiteSpace(selected) && lstClasses.Items.Contains(selected))
        {
            lstClasses.SelectedItem = selected;
        }
        lstClasses.EndUpdate();
        _suppressSelectionHandler = false;
    }

    private void ImportClassList()
    {
        using var ofd = new OpenFileDialog
        {
            Title = "Select Classnamelist",
            Filter = "Text files (*.txt)|*.txt|All files (*.*)|*.*"
        };

        if (ofd.ShowDialog(this) != DialogResult.OK)
        {
            return;
        }

        try
        {
            _allClasses = ClassnameListService.Load(ofd.FileName);

            foreach (var className in _allClasses)
            {
                if (!_cache.ContainsKey(className))
                {
                    _cache[className] = _typesService.HasDestination ? (_typesService.TryRead(className) ?? TypeEntry.CreateDefault(className)) : TypeEntry.CreateDefault(className);
                }
            }

            ApplyFilter();
            SetStatus(_allClasses.Count == 0 ? "No classnames found in file." : $"Loaded {_allClasses.Count} classnames.");
        }
        catch (Exception ex)
        {
            MessageBox.Show(this, ex.Message, "Import error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            SetStatus("Import failed.");
        }
    }

    private void ImportCfgLimits()
    {
        using var ofd = new OpenFileDialog
        {
            Title = "Select cfglimitsdefinition.xml",
            Filter = "XML files (*.xml)|*.xml|All files (*.*)|*.*"
        };

        if (ofd.ShowDialog(this) != DialogResult.OK)
        {
            return;
        }

        try
        {
            var data = CfgLimitsService.Load(ofd.FileName);
            PopulateCheckedList(clbCategories, data.Categories);
            PopulateCheckedList(clbTags, data.Tags);
            PopulateCheckedList(clbUsage, data.UsageFlags);
            PopulateCheckedList(clbValue, data.ValueFlags);

            if (!string.IsNullOrWhiteSpace(_currentClassname))
            {
                LoadClassIntoUi(_currentClassname);
            }

            SetStatus($"Loaded cfg lists: C={data.Categories.Count}, T={data.Tags.Count}, U={data.UsageFlags.Count}, V={data.ValueFlags.Count}.");
        }
        catch (Exception ex)
        {
            MessageBox.Show(this, ex.Message, "Import error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            SetStatus("Import failed.");
        }
    }

    private static void PopulateCheckedList(CheckedListBox clb, List<string> items)
    {
        clb.BeginUpdate();
        clb.Items.Clear();
        foreach (var item in items)
        {
            clb.Items.Add(item, false);
        }
        clb.EndUpdate();
    }

    private void ImportTypesXml()
    {
        using var ofd = new OpenFileDialog
        {
            Title = "Import types.xml (read classnames + values)",
            Filter = "XML files (*.xml)|*.xml|All files (*.*)|*.*"
        };

        if (ofd.ShowDialog(this) != DialogResult.OK)
            return;

        try
        {
            var entries = TypesXmlService.ImportFromFile(ofd.FileName);
            if (entries.Count == 0)
            {
                SetStatus("No entries found in types.xml.");
                return;
            }

            var newNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            foreach (var entry in entries)
            {
                _cache[entry.Name] = entry;
                newNames.Add(entry.Name);
            }

            // Merge into class list
            var merged = new HashSet<string>(_allClasses, StringComparer.OrdinalIgnoreCase);
            foreach (var n in newNames) merged.Add(n);
            _allClasses = merged.OrderBy(s => s, StringComparer.OrdinalIgnoreCase).ToList();

            // Auto-populate cfglimits lists from imported data
            MergeCfgListsFromEntries(entries);

            ApplyFilter();
            SetStatus($"Imported {entries.Count} entries from types.xml.");
        }
        catch (Exception ex)
        {
            MessageBox.Show(this, ex.Message, "Import error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            SetStatus("Import types.xml failed.");
        }
    }

    /// <summary>Auto-merge categories/tags/usage/value from imported entries into the CheckedListBoxes.</summary>
    private void MergeCfgListsFromEntries(List<TypeEntry> entries)
    {
        MergeIntoCheckedList(clbCategories, entries.SelectMany(e => e.Categories));
        MergeIntoCheckedList(clbTags, entries.SelectMany(e => e.Tags));
        MergeIntoCheckedList(clbUsage, entries.SelectMany(e => e.UsageFlags));
        MergeIntoCheckedList(clbValue, entries.SelectMany(e => e.ValueFlags));
    }

    private void ImportMarketJson()
    {
        using var ofd = new OpenFileDialog
        {
            Title = "Import Market JSON",
            Filter = "JSON files (*.json)|*.json|All files (*.*)|*.*"
        };

        if (ofd.ShowDialog(this) != DialogResult.OK)
            return;

        try
        {
            var items = MarketJsonService.ImportFromFile(ofd.FileName);
            if (items.Count == 0)
            {
                SetStatus("No items found in Market JSON.");
                return;
            }

            var merged = new HashSet<string>(_allClasses, StringComparer.OrdinalIgnoreCase);
            foreach (var item in items)
            {
                _marketCache[item.ClassName] = item;
                merged.Add(item.ClassName);

                // Also create a types.xml entry if not present
                if (!_cache.ContainsKey(item.ClassName))
                {
                    _cache[item.ClassName] = _typesService.HasDestination
                        ? (_typesService.TryRead(item.ClassName) ?? TypeEntry.CreateDefault(item.ClassName))
                        : TypeEntry.CreateDefault(item.ClassName);
                }
            }

            _allClasses = merged.OrderBy(s => s, StringComparer.OrdinalIgnoreCase).ToList();
            ApplyFilter();
            SetStatus($"Imported {items.Count} items from Market JSON.");
        }
        catch (Exception ex)
        {
            MessageBox.Show(this, ex.Message, "Import error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            SetStatus("Import Market JSON failed.");
        }
    }

    private void SelectMarketDestination()
    {
        using var sfd = new SaveFileDialog
        {
            Title = "Select destination Market JSON",
            Filter = "JSON files (*.json)|*.json|All files (*.*)|*.*",
            FileName = "Market.json"
        };

        if (sfd.ShowDialog(this) != DialogResult.OK)
            return;

        try
        {
            _marketService.SetDestination(sfd.FileName);
            lblMarketDest.Text = $"Market JSON: {sfd.FileName}";

            // Read existing items from the destination
            if (File.Exists(sfd.FileName))
            {
                var existing = _marketService.Load(sfd.FileName);
                foreach (var item in existing)
                {
                    if (!_marketCache.ContainsKey(item.ClassName))
                        _marketCache[item.ClassName] = item;
                }
            }

            if (!string.IsNullOrWhiteSpace(_currentClassname))
                LoadClassIntoUi(_currentClassname);

            SetStatus("Market destination set.");

            try
            {
                PersistMarket(force: false);
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, ex.Message, "Initial market save warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                SetStatus("Market destination set, but initial sync failed.");
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show(this, ex.Message, "Market destination error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            SetStatus("Market destination selection failed.");
        }
    }

    private void CreateMarketJsonFromSelected()
    {
        // Gather selected classnames (or all if none selected)
        var selected = lstClasses.SelectedItems.Cast<string>().ToList();
        if (selected.Count == 0)
        {
            // Nothing selected → offer to use the entire list
            if (_allClasses.Count == 0)
            {
                MessageBox.Show(this, "No classnames loaded. Import a classlist or types.xml first.",
                    "Create Market JSON", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            var useAll = MessageBox.Show(this,
                $"No items selected.\nCreate Market JSON for all {_allClasses.Count} classnames in the list?",
                "Create Market JSON", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

            if (useAll != DialogResult.Yes) return;
            selected = new List<string>(_allClasses);
        }

        // Ask for display name
        var displayName = PromptInput("Market Display Name",
            "Enter a display name for this market category\n(e.g. #STR_EXPANSION_MARKET_CATEGORY_AMMO):",
            "NewMarket");

        if (displayName == null) return; // cancelled

        // Choose save location
        using var sfd = new SaveFileDialog
        {
            Title = "Save new Market JSON file",
            Filter = "JSON files (*.json)|*.json|All files (*.*)|*.*",
            FileName = $"{displayName.Replace("#", "").Replace(" ", "_")}.json"
        };

        if (sfd.ShowDialog(this) != DialogResult.OK)
            return;

        try
        {
            // Build a fresh market service for the new file
            _marketService.SetDestination(sfd.FileName);
            lblMarketDest.Text = $"Market JSON: {sfd.FileName}";

            // Set display name on the root node
            _marketService.SetDisplayName(displayName);

            // Create default market items for each selected classname
            var count = 0;
            foreach (var className in selected)
            {
                if (!_marketCache.TryGetValue(className, out var mkt))
                {
                    mkt = MarketItem.CreateDefault(className);
                }

                mkt.IsDirty = true;
                _marketCache[className] = mkt;
                _marketService.Upsert(mkt);
                count++;
            }

            _marketService.Save();

            if (!string.IsNullOrWhiteSpace(_currentClassname))
                LoadClassIntoUi(_currentClassname);

            SetStatus($"Created Market JSON with {count} items → {sfd.FileName}");
        }
        catch (Exception ex)
        {
            MessageBox.Show(this, ex.Message, "Create Market JSON error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            SetStatus("Create Market JSON failed.");
        }
    }

    /// <summary>Simple input prompt dialog.</summary>
    private static string? PromptInput(string title, string label, string defaultValue)
    {
        var form = new Form
        {
            Text = title,
            Width = 440,
            Height = 180,
            FormBorderStyle = FormBorderStyle.FixedDialog,
            StartPosition = FormStartPosition.CenterParent,
            MaximizeBox = false,
            MinimizeBox = false
        };

        var lbl = new Label { Left = 12, Top = 12, Width = 400, Text = label, AutoSize = true };
        var txt = new TextBox { Left = 12, Top = lbl.Bottom + 8, Width = 400, Text = defaultValue };
        var btnOk = new Button { Text = "OK", Left = 230, Top = txt.Bottom + 12, Width = 85, DialogResult = DialogResult.OK };
        var btnCancel = new Button { Text = "Cancel", Left = 325, Top = txt.Bottom + 12, Width = 85, DialogResult = DialogResult.Cancel };

        form.Controls.AddRange(new Control[] { lbl, txt, btnOk, btnCancel });
        form.AcceptButton = btnOk;
        form.CancelButton = btnCancel;

        return form.ShowDialog() == DialogResult.OK ? txt.Text.Trim() : null;
    }

    private void ClearList()
    {
        var result = MessageBox.Show(this,
            "Clear the entire class list and cached values?\n\nThis cannot be undone.",
            "Clear List",
            MessageBoxButtons.YesNo,
            MessageBoxIcon.Question);

        if (result != DialogResult.Yes) return;

        _currentClassname = null;
        _allClasses.Clear();
        _cache.Clear();
        _marketCache.Clear();
        _undoRedo.Clear();
        _autosaveTimer.Stop();

        lstClasses.Items.Clear();
        txtSearch.Clear();

        // Reset editor to defaults
        _loadingUi = true;
        try
        {
            numNominal.Value = 0;
            numLifetime.Value = 0;
            numRestock.Value = 0;
            numMin.Value = 0;
            numQuantMin.Value = -1;
            numQuantMax.Value = -1;
            numCost.Value = 0;

            chkCountInCargo.Checked = false;
            chkCountInHoarder.Checked = false;
            chkCountInMap.Checked = false;
            chkCountInPlayer.Checked = false;
            chkCrafted.Checked = false;
            chkDeloot.Checked = false;

            for (int i = 0; i < clbCategories.Items.Count; i++) clbCategories.SetItemChecked(i, false);
            for (int i = 0; i < clbTags.Items.Count; i++) clbTags.SetItemChecked(i, false);
            for (int i = 0; i < clbUsage.Items.Count; i++) clbUsage.SetItemChecked(i, false);
            for (int i = 0; i < clbValue.Items.Count; i++) clbValue.SetItemChecked(i, false);

            numMaxPrice.Value = 0;
            numMinPrice.Value = 0;
            numSellPricePercent.Value = -1;
            numMaxStock.Value = 0;
            numMinStock.Value = 0;
            numQuantityPercent.Value = -1;
            txtSpawnAttachments.Clear();
            txtVariants.Clear();
        }
        finally
        {
            _loadingUi = false;
        }

        UpdateUndoRedoButtons();
        btnBulkApply.Enabled = false;
        SetStatus("List cleared.");
    }

    private static void MergeIntoCheckedList(CheckedListBox clb, IEnumerable<string> newItems)
    {
        var existing = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        for (var i = 0; i < clb.Items.Count; i++)
        {
            var text = clb.Items[i]?.ToString() ?? string.Empty;
            existing.Add(text);
        }

        foreach (var item in newItems)
        {
            if (!string.IsNullOrWhiteSpace(item) && existing.Add(item))
            {
                clb.Items.Add(item, false);
            }
        }
    }

    private void PerformUndo()
    {
        if (string.IsNullOrWhiteSpace(_currentClassname)) return;
        if (!_cache.TryGetValue(_currentClassname, out var current)) return;

        // Save current UI state first
        SaveCurrentFromUiIntoCache();

        var previous = _undoRedo.Undo(current);
        if (previous == null)
        {
            SetStatus("Nothing to undo.");
            return;
        }

        previous.Name = _currentClassname;
        previous.IsDirty = true;
        _cache[_currentClassname] = previous;
        LoadClassIntoUi(_currentClassname);
        UpdateUndoRedoButtons();
        SetStatus("Undo performed.");
    }

    private void PerformRedo()
    {
        if (string.IsNullOrWhiteSpace(_currentClassname)) return;
        if (!_cache.TryGetValue(_currentClassname, out var current)) return;

        var next = _undoRedo.Redo(current);
        if (next == null)
        {
            SetStatus("Nothing to redo.");
            return;
        }

        next.Name = _currentClassname;
        next.IsDirty = true;
        _cache[_currentClassname] = next;
        LoadClassIntoUi(_currentClassname);
        UpdateUndoRedoButtons();
        SetStatus("Redo performed.");
    }

    private void UpdateUndoRedoButtons()
    {
        var cn = _currentClassname ?? string.Empty;
        btnUndo.Enabled = _undoRedo.CanUndo(cn);
        btnRedo.Enabled = _undoRedo.CanRedo(cn);
    }

    private void BulkApplyToSelected()
    {
        var selectedItems = lstClasses.SelectedItems.Cast<string>().ToList();
        if (selectedItems.Count < 2)
        {
            MessageBox.Show(this, "Select at least 2 classnames to bulk-apply.", "Bulk Apply", MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }

        if (!SaveCurrentFromUiIntoCache())
            return;

        if (string.IsNullOrWhiteSpace(_currentClassname) || !_cache.TryGetValue(_currentClassname, out var template))
        {
            SetStatus("No current class to use as template.");
            return;
        }

        var result = MessageBox.Show(this,
            $"Apply current editor values to {selectedItems.Count} selected classnames?\n\n" +
            "This will overwrite all values for the selected classes.\n" +
            "The operation can be undone per class.",
            "Bulk Apply Confirmation",
            MessageBoxButtons.YesNo,
            MessageBoxIcon.Question);

        if (result != DialogResult.Yes) return;

        // Get market template if available
        MarketItem? marketTemplate = null;
        if (_marketCache.TryGetValue(_currentClassname, out var mktTpl))
            marketTemplate = mktTpl;

        var count = 0;
        foreach (var className in selectedItems)
        {
            if (!_cache.TryGetValue(className, out var entry))
            {
                entry = TypeEntry.CreateDefault(className);
                _cache[className] = entry;
            }

            // Push undo snapshot before overwriting
            _undoRedo.PushUndo(entry);

            entry.CopyFrom(template);
            entry.Name = className; // keep original name
            entry.IsDirty = true;

            // Also bulk-apply market data
            if (marketTemplate != null)
            {
                if (!_marketCache.TryGetValue(className, out var mktEntry))
                {
                    mktEntry = MarketItem.CreateDefault(className);
                    _marketCache[className] = mktEntry;
                }
                mktEntry.CopyFrom(marketTemplate);
                mktEntry.ClassName = className;
                mktEntry.IsDirty = true;
            }

            count++;
        }

        PersistToXml(force: true);
        PersistMarket(force: true);
        UpdateUndoRedoButtons();
        SetStatus($"Bulk-applied to {count} classnames.");
    }

    private void SelectDestination()
    {
        using var sfd = new SaveFileDialog
        {
            Title = "Select destination types.xml",
            Filter = "types.xml (*.xml)|*.xml|All files (*.*)|*.*",
            FileName = "types.xml"
        };

        if (sfd.ShowDialog(this) != DialogResult.OK)
        {
            return;
        }

        try
        {
            _typesService.SetDestination(sfd.FileName);
            lblDestination.Text = $"Destination: {sfd.FileName}";

            foreach (var className in _allClasses)
            {
                if (!_cache.ContainsKey(className))
                {
                    var fromXml = _typesService.TryRead(className);
                    if (fromXml != null)
                    {
                        _cache[className] = fromXml;
                    }
                }
            }

            if (!string.IsNullOrWhiteSpace(_currentClassname))
            {
                LoadClassIntoUi(_currentClassname);
            }

            SetStatus("Destination set.");
            try
            {
                PersistToXml(force: false);
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, ex.Message, "Initial save warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                SetStatus("Destination set, but initial sync failed.");
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show(this, ex.Message, "Destination error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            SetStatus("Destination selection failed.");
        }
    }

    private void ExportNow()
    {
        try
        {
            if (!SaveCurrentFromUiIntoCache())
            {
                return;
            }

            var exported = false;

            if (_typesService.HasDestination)
            {
                PersistToXml(force: true);
                exported = true;
            }

            if (_marketService.HasFile)
            {
                PersistMarket(force: true);
                exported = true;
            }

            if (!exported)
            {
                MessageBox.Show(this, "Please select a destination types.xml or Market JSON first.", "Export", MessageBoxButtons.OK, MessageBoxIcon.Information);
                SetStatus("Export skipped: no destination selected.");
                return;
            }

            SetStatus("Exported.");
        }
        catch (Exception ex)
        {
            MessageBox.Show(this, ex.Message, "Export error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            SetStatus("Export failed.");
        }
    }

    private void OnClassSelectionChanged()
    {
        if (_loadingUi || _suppressSelectionHandler)
        {
            return;
        }

        // Update bulk-apply button state
        btnBulkApply.Enabled = lstClasses.SelectedItems.Count >= 2;

        // For the editor, use the last selected item (focused item)
        if (lstClasses.SelectedItem is not string selected)
        {
            return;
        }

        if (string.Equals(selected, _currentClassname, StringComparison.OrdinalIgnoreCase))
        {
            return;
        }

        try
        {
            if (!SaveCurrentFromUiIntoCache())
            {
                _suppressSelectionHandler = true;
                lstClasses.SelectedItem = _currentClassname;
                _suppressSelectionHandler = false;
                return;
            }

            PersistToXml(force: true);
            PersistMarket(force: true);

            _currentClassname = selected;
            LoadClassIntoUi(selected);
            UpdateUndoRedoButtons();
            SetStatus($"Selected: {selected}" + (lstClasses.SelectedItems.Count > 1 ? $" (+{lstClasses.SelectedItems.Count - 1} more)" : ""));
        }
        catch (Exception ex)
        {
            MessageBox.Show(this, ex.Message, "Selection error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            SetStatus("Selection failed.");
        }
    }

    private void LoadClassIntoUi(string classname)
    {
        _loadingUi = true;
        try
        {
            if (!_cache.TryGetValue(classname, out var entry))
            {
                entry = _typesService.HasDestination
                    ? (_typesService.TryRead(classname) ?? TypeEntry.CreateDefault(classname))
                    : TypeEntry.CreateDefault(classname);
                _cache[classname] = entry;
            }

            numNominal.Value = Clamp(entry.Nominal, numNominal.Minimum, numNominal.Maximum);
            numLifetime.Value = Clamp(entry.Lifetime, numLifetime.Minimum, numLifetime.Maximum);
            numRestock.Value = Clamp(entry.Restock, numRestock.Minimum, numRestock.Maximum);
            numMin.Value = Clamp(entry.Min, numMin.Minimum, numMin.Maximum);
            numQuantMin.Value = Clamp(entry.QuantMin, numQuantMin.Minimum, numQuantMin.Maximum);
            numQuantMax.Value = Clamp(entry.QuantMax, numQuantMax.Minimum, numQuantMax.Maximum);
            numCost.Value = Clamp(entry.Cost, numCost.Minimum, numCost.Maximum);

            _lastValidQuantMin = (int)numQuantMin.Value;
            _lastValidQuantMax = (int)numQuantMax.Value;

            chkCountInCargo.Checked = entry.CountInCargo;
            chkCountInHoarder.Checked = entry.CountInHoarder;
            chkCountInMap.Checked = entry.CountInMap;
            chkCountInPlayer.Checked = entry.CountInPlayer;
            chkCrafted.Checked = entry.Crafted;
            chkDeloot.Checked = entry.Deloot;

            ApplySetToCheckedList(clbCategories, entry.Categories);
            ApplySetToCheckedList(clbTags, entry.Tags);
            ApplySetToCheckedList(clbUsage, entry.UsageFlags);
            ApplySetToCheckedList(clbValue, entry.ValueFlags);

            // ── Market fields ──
            if (_marketCache.TryGetValue(classname, out var mkt))
            {
                numMaxPrice.Value = Clamp(mkt.MaxPriceThreshold, numMaxPrice.Minimum, numMaxPrice.Maximum);
                numMinPrice.Value = Clamp(mkt.MinPriceThreshold, numMinPrice.Minimum, numMinPrice.Maximum);
                numSellPricePercent.Value = Clamp(mkt.SellPricePercent, numSellPricePercent.Minimum, numSellPricePercent.Maximum);
                numMaxStock.Value = Clamp(mkt.MaxStockThreshold, numMaxStock.Minimum, numMaxStock.Maximum);
                numMinStock.Value = Clamp(mkt.MinStockThreshold, numMinStock.Minimum, numMinStock.Maximum);
                numQuantityPercent.Value = Clamp(mkt.QuantityPercent, numQuantityPercent.Minimum, numQuantityPercent.Maximum);
                txtSpawnAttachments.Text = string.Join(", ", mkt.SpawnAttachments);
                txtVariants.Text = string.Join(", ", mkt.Variants);
            }
            else
            {
                numMaxPrice.Value = 0;
                numMinPrice.Value = 0;
                numSellPricePercent.Value = -1;
                numMaxStock.Value = 0;
                numMinStock.Value = 0;
                numQuantityPercent.Value = -1;
                txtSpawnAttachments.Clear();
                txtVariants.Clear();
            }
        }
        finally
        {
            _loadingUi = false;
        }
    }

    private static decimal Clamp(int value, decimal min, decimal max)
    {
        if (value < min) return min;
        if (value > max) return max;
        return value;
    }

    private static void ApplySetToCheckedList(CheckedListBox clb, HashSet<string> set)
    {
        clb.BeginUpdate();
        for (var i = 0; i < clb.Items.Count; i++)
        {
            var item = clb.Items[i]?.ToString() ?? string.Empty;
            clb.SetItemChecked(i, set.Contains(item));
        }
        clb.EndUpdate();
    }

    private void OnQuantChanged(bool isMin)
    {
        if (_loadingUi)
        {
            return;
        }

        var nud = isMin ? numQuantMin : numQuantMax;
        var value = (int)nud.Value;

        if (value != -1 && (value < 1 || value > 100))
        {
            MessageBox.Show(this, "quantmin/quantmax must be -1 or 1..100", "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning);

            _loadingUi = true;
            nud.Value = isMin ? _lastValidQuantMin : _lastValidQuantMax;
            _loadingUi = false;
            return;
        }

        if (isMin)
        {
            _lastValidQuantMin = value;
        }
        else
        {
            _lastValidQuantMax = value;
        }

        OnEditorChanged();
    }

    private void OnEditorChanged()
    {
        if (_loadingUi || string.IsNullOrWhiteSpace(_currentClassname))
        {
            return;
        }

        // Push undo snapshot before the change (only on first edit since last save)
        if (_cache.TryGetValue(_currentClassname, out var current) && !current.IsDirty)
        {
            _undoRedo.PushUndo(current);
        }

        if (!SaveCurrentFromUiIntoCache(markDirtyOnly: true))
        {
            return;
        }

        UpdateUndoRedoButtons();
        _autosaveTimer.Stop();
        _autosaveTimer.Start();
        SetStatus("Editing...");
    }

    private void AutosaveTick()
    {
        _autosaveTimer.Stop();

        try
        {
            if (!SaveCurrentFromUiIntoCache())
            {
                return;
            }

            PersistToXml(force: false);
            PersistMarket(force: false);
            SetStatus("Saved.");
        }
        catch (Exception ex)
        {
            SetStatus("Autosave failed. Please use Export now.");
            MessageBox.Show(this, ex.Message, "Autosave error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }
    }

    private bool SaveCurrentFromUiIntoCache(bool markDirtyOnly = false)
    {
        if (string.IsNullOrWhiteSpace(_currentClassname))
        {
            return true;
        }

        if (!_cache.TryGetValue(_currentClassname, out var entry))
        {
            entry = TypeEntry.CreateDefault(_currentClassname);
            _cache[_currentClassname] = entry;
        }

        if (markDirtyOnly)
        {
            entry.IsDirty = true;
            return true;
        }

        var quantMin = (int)numQuantMin.Value;
        var quantMax = (int)numQuantMax.Value;
        if (!IsValidQuant(quantMin) || !IsValidQuant(quantMax))
        {
            MessageBox.Show(this, "quantmin/quantmax must be -1 or 1..100", "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            SetStatus("Validation failed: quant must be -1 or 1..100.");
            return false;
        }

        entry.Nominal = (int)numNominal.Value;
        entry.Lifetime = (int)numLifetime.Value;
        entry.Restock = (int)numRestock.Value;
        entry.Min = (int)numMin.Value;
        entry.QuantMin = quantMin;
        entry.QuantMax = quantMax;
        entry.Cost = (int)numCost.Value;

        entry.CountInCargo = chkCountInCargo.Checked;
        entry.CountInHoarder = chkCountInHoarder.Checked;
        entry.CountInMap = chkCountInMap.Checked;
        entry.CountInPlayer = chkCountInPlayer.Checked;
        entry.Crafted = chkCrafted.Checked;
        entry.Deloot = chkDeloot.Checked;

        entry.Categories.Clear();
        entry.Tags.Clear();
        entry.UsageFlags.Clear();
        entry.ValueFlags.Clear();

        ReadChecked(clbCategories, entry.Categories);
        ReadChecked(clbTags, entry.Tags);
        ReadChecked(clbUsage, entry.UsageFlags);
        ReadChecked(clbValue, entry.ValueFlags);

        entry.IsDirty = true;

        // ── Save market fields ──
        SaveMarketFromUi(_currentClassname);

        return true;
    }

    private static bool IsValidQuant(int value) => value == -1 || (value >= 1 && value <= 100);

    private static void ReadChecked(CheckedListBox clb, HashSet<string> set)
    {
        foreach (var obj in clb.CheckedItems)
        {
            var text = obj?.ToString();
            if (!string.IsNullOrWhiteSpace(text))
            {
                set.Add(text.Trim());
            }
        }
    }

    private void SaveMarketFromUi(string className)
    {
        if (!_marketCache.TryGetValue(className, out var mkt))
        {
            mkt = MarketItem.CreateDefault(className);
            _marketCache[className] = mkt;
        }

        mkt.MaxPriceThreshold = (int)numMaxPrice.Value;
        mkt.MinPriceThreshold = (int)numMinPrice.Value;
        mkt.SellPricePercent = (int)numSellPricePercent.Value;
        mkt.MaxStockThreshold = (int)numMaxStock.Value;
        mkt.MinStockThreshold = (int)numMinStock.Value;
        mkt.QuantityPercent = (int)numQuantityPercent.Value;

        mkt.SpawnAttachments = ParseCommaSeparated(txtSpawnAttachments.Text);
        mkt.Variants = ParseCommaSeparated(txtVariants.Text);

        mkt.IsDirty = true;
    }

    private static List<string> ParseCommaSeparated(string text)
    {
        if (string.IsNullOrWhiteSpace(text)) return new List<string>();
        return text.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                   .Where(s => s.Length > 0)
                   .ToList();
    }

    private void PersistMarket(bool force)
    {
        if (!_marketService.HasFile) return;

        foreach (var kvp in _marketCache)
        {
            var item = kvp.Value;
            if (!force && !item.IsDirty) continue;

            _marketService.Upsert(item);
            item.IsDirty = false;
        }

        _marketService.Save();
    }

    private void PersistToXml(bool force)
    {
        if (!_typesService.HasDestination)
        {
            return;
        }

        if (chkExportAll.Checked)
        {
            foreach (var className in _allClasses)
            {
                if (!_cache.TryGetValue(className, out var entry))
                {
                    entry = _typesService.TryRead(className) ?? TypeEntry.CreateDefault(className);
                    _cache[className] = entry;
                }

                if (!force && !entry.IsDirty)
                {
                    continue;
                }

                _typesService.Upsert(entry);
                entry.IsDirty = false;
            }
        }
        else
        {
            // Persist all dirty entries (supports bulk-edit)
            var persisted = false;
            foreach (var kvp in _cache)
            {
                var entry = kvp.Value;
                if (!force && !entry.IsDirty) continue;

                _typesService.Upsert(entry);
                entry.IsDirty = false;
                persisted = true;
            }

            if (!persisted && !string.IsNullOrWhiteSpace(_currentClassname))
            {
                if (_cache.TryGetValue(_currentClassname, out var current))
                {
                    _typesService.Upsert(current);
                    current.IsDirty = false;
                }
            }
        }

        _typesService.Save();
    }
}
