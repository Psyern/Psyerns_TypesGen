namespace DayZTypesHelper;

/// <summary>
/// Applies a dark or light colour scheme recursively to all controls.
/// </summary>
public static class DarkModeHelper
{
    // ── Dark palette ──────────────────────────────────────────────
    private static readonly Color DarkBg = Color.FromArgb(30, 30, 30);
    private static readonly Color DarkBgAlt = Color.FromArgb(45, 45, 48);
    private static readonly Color DarkBgInput = Color.FromArgb(51, 51, 55);
    private static readonly Color DarkFg = Color.FromArgb(220, 220, 220);
    private static readonly Color DarkAccent = Color.FromArgb(0, 122, 204);
    private static readonly Color DarkBorder = Color.FromArgb(63, 63, 70);

    // ── Light palette (system defaults) ───────────────────────────
    private static readonly Color LightBg = SystemColors.Control;
    private static readonly Color LightBgInput = SystemColors.Window;
    private static readonly Color LightFg = SystemColors.ControlText;

    public static bool IsDark { get; private set; }

    public static void Apply(Control root, bool dark)
    {
        IsDark = dark;
        ApplyRecursive(root, dark);
    }

    private static void ApplyRecursive(Control ctrl, bool dark)
    {
        // Order matters: more specific types MUST come before their base types.
        switch (ctrl)
        {
            case Form form:
                form.BackColor = dark ? DarkBg : LightBg;
                form.ForeColor = dark ? DarkFg : LightFg;
                break;

            case TextBox tb:
                tb.BackColor = dark ? DarkBgInput : LightBgInput;
                tb.ForeColor = dark ? DarkFg : LightFg;
                tb.BorderStyle = BorderStyle.FixedSingle;
                break;

            case NumericUpDown nud:
                nud.BackColor = dark ? DarkBgInput : LightBgInput;
                nud.ForeColor = dark ? DarkFg : LightFg;
                break;

            // CheckedListBox inherits ListBox – must come first
            case CheckedListBox clb:
                clb.BackColor = dark ? DarkBgInput : LightBgInput;
                clb.ForeColor = dark ? DarkFg : LightFg;
                break;

            case ListBox lb:
                lb.BackColor = dark ? DarkBgInput : LightBgInput;
                lb.ForeColor = dark ? DarkFg : LightFg;
                break;

            case CheckBox cb:
                cb.ForeColor = dark ? DarkFg : LightFg;
                cb.BackColor = dark ? DarkBgAlt : LightBg;
                break;

            case Button btn:
                btn.FlatStyle = FlatStyle.Flat;
                btn.BackColor = dark ? DarkAccent : SystemColors.Control;
                btn.ForeColor = dark ? Color.White : SystemColors.ControlText;
                btn.FlatAppearance.BorderColor = dark ? DarkBorder : SystemColors.ControlDark;
                btn.FlatAppearance.BorderSize = 1;
                break;

            case GroupBox gb:
                gb.ForeColor = dark ? DarkFg : LightFg;
                gb.BackColor = dark ? DarkBgAlt : LightBg;
                break;

            case StatusStrip ss:
                ss.BackColor = dark ? DarkBgAlt : SystemColors.Control;
                ss.ForeColor = dark ? DarkFg : LightFg;
                foreach (ToolStripItem item in ss.Items)
                {
                    item.ForeColor = dark ? DarkFg : LightFg;
                    item.BackColor = dark ? DarkBgAlt : SystemColors.Control;
                }
                break;

            // SplitContainer inherits ContainerControl – before Panel
            case SplitContainer sc:
                sc.BackColor = dark ? DarkBg : LightBg;
                ApplyRecursive(sc.Panel1, dark);
                ApplyRecursive(sc.Panel2, dark);
                break;

            // TableLayoutPanel and FlowLayoutPanel inherit Panel – before Panel
            case TableLayoutPanel tlp:
                tlp.BackColor = dark ? DarkBg : LightBg;
                break;

            case FlowLayoutPanel flp:
                flp.BackColor = dark ? DarkBgAlt : LightBg;
                break;

            case Panel p:
                p.BackColor = dark ? DarkBg : LightBg;
                break;

            case Label lbl:
                lbl.ForeColor = dark ? DarkFg : LightFg;
                break;

            default:
                ctrl.BackColor = dark ? DarkBg : LightBg;
                ctrl.ForeColor = dark ? DarkFg : LightFg;
                break;
        }

        foreach (Control child in ctrl.Controls)
        {
            ApplyRecursive(child, dark);
        }
    }
}
