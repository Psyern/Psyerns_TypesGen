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

    // ── Right side: file row ──────────────────────────────────────
    private Panel pnlRight = null!;
    private Panel pnlFile = null!;
    private Button btnSelectDestination = null!;
    private Label lblDestination = null!;
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

    private StatusStrip statusStrip = null!;
    private ToolStripStatusLabel lblStatus = null!;

    // ── Services / state ──────────────────────────────────────────
    private readonly System.Windows.Forms.Timer _autosaveTimer = new() { Interval = 500 };
    private readonly TypesXmlService _typesService = new();
    private readonly UndoRedoService _undoRedo = new();
    private readonly Dictionary<string, TypeEntry> _cache = new(StringComparer.OrdinalIgnoreCase);

    private List<string> _allClasses = new();
    private string? _currentClassname;
    private bool _loadingUi;
    private bool _suppressSelectionHandler;
    private int _lastValidQuantMin = -1;
    private int _lastValidQuantMax = -1;

    public MainForm()
    {
        Text = "DayZ Types Helper";
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
            Height = 74
        };

        btnImportClassList = new Button
        {
            Name = "btnImportClassList",
            Text = "Import Classnamelist",
            Left = 8,
            Top = 8,
            Width = 150,
            Height = 26
        };

        btnImportCfgLimits = new Button
        {
            Name = "btnImportCfgLimits",
            Text = "Import cfglimits.xml",
            Left = 166,
            Top = 8,
            Width = 150,
            Height = 26
        };

        btnImportTypesXml = new Button
        {
            Name = "btnImportTypesXml",
            Text = "Import types.xml",
            Left = 8,
            Top = 40,
            Width = 150,
            Height = 26
        };

        pnlLeftButtons.Controls.Add(btnImportClassList);
        pnlLeftButtons.Controls.Add(btnImportCfgLimits);
        pnlLeftButtons.Controls.Add(btnImportTypesXml);

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
            Height = 104
        };

        btnSelectDestination = new Button
        {
            Name = "btnSelectDestination",
            Text = "Select destination types.xml",
            Left = 8,
            Top = 8,
            Width = 220,
            Height = 28
        };

        btnExportNow = new Button
        {
            Name = "btnExportNow",
            Text = "Export now",
            Left = 238,
            Top = 8,
            Width = 110,
            Height = 28
        };

        chkExportAll = new CheckBox
        {
            Name = "chkExportAll",
            Text = "Export all",
            Left = 360,
            Top = 12,
            Width = 100,
            Checked = false
        };

        btnUndo = new Button
        {
            Name = "btnUndo",
            Text = "↩ Undo",
            Left = 470,
            Top = 8,
            Width = 80,
            Height = 28,
            Enabled = false
        };

        btnRedo = new Button
        {
            Name = "btnRedo",
            Text = "↪ Redo",
            Left = 558,
            Top = 8,
            Width = 80,
            Height = 28,
            Enabled = false
        };

        btnBulkApply = new Button
        {
            Name = "btnBulkApply",
            Text = "Bulk Apply to Selected",
            Left = 648,
            Top = 8,
            Width = 170,
            Height = 28,
            Enabled = false
        };

        chkDarkMode = new CheckBox
        {
            Name = "chkDarkMode",
            Text = "Dark Mode",
            Left = 830,
            Top = 12,
            Width = 100,
            Checked = false
        };

        lblDestination = new Label
        {
            Name = "lblDestination",
            Text = "Destination: (none)",
            Left = 8,
            Top = 44,
            Width = 900,
            AutoEllipsis = true
        };

        var lblMultiHint = new Label
        {
            Text = "Tip: Hold Ctrl/Shift in classlist for multi-select → then 'Bulk Apply'",
            Left = 8,
            Top = 68,
            Width = 700,
            AutoSize = false,
            ForeColor = Color.Gray,
            Font = new Font(Font.FontFamily, 8f, FontStyle.Italic)
        };

        pnlFile.Controls.Add(btnSelectDestination);
        pnlFile.Controls.Add(btnExportNow);
        pnlFile.Controls.Add(chkExportAll);
        pnlFile.Controls.Add(btnUndo);
        pnlFile.Controls.Add(btnRedo);
        pnlFile.Controls.Add(btnBulkApply);
        pnlFile.Controls.Add(chkDarkMode);
        pnlFile.Controls.Add(lblDestination);
        pnlFile.Controls.Add(lblMultiHint);

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
        numQuantMin = new NumericUpDown { Name = "numQuantMin", Value = -1, Tag = -1 };
        numQuantMax = new NumericUpDown { Name = "numQuantMax", Value = -1, Tag = -1 };
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

    private void WireEvents()
    {
        txtSearch.TextChanged += (_, _) => ApplyFilter();
        lstClasses.SelectedIndexChanged += (_, _) => OnClassSelectionChanged();

        btnImportClassList.Click += (_, _) => ImportClassList();
        btnImportCfgLimits.Click += (_, _) => ImportCfgLimits();
        btnImportTypesXml.Click += (_, _) => ImportTypesXml();
        btnSelectDestination.Click += (_, _) => SelectDestination();
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
            count++;
        }

        PersistToXml(force: true);
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

            if (!_typesService.HasDestination)
            {
                MessageBox.Show(this, "Please select destination types.xml first.", "Export", MessageBoxButtons.OK, MessageBoxIcon.Information);
                SetStatus("Export skipped: no destination selected.");
                return;
            }

            PersistToXml(force: true);
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
