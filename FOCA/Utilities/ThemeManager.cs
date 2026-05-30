using System;
using System.Drawing;
using System.Windows.Forms;

namespace FOCA.Utilities
{
    public static class ThemeManager
    {
        // Modern Premium Dark Palette (Harmonious zinc and blue shades)
        public static readonly Color ColorBgDark = Color.FromArgb(24, 24, 27);      // zinc-900
        public static readonly Color ColorBgPanel = Color.FromArgb(39, 39, 42);     // zinc-800
        public static readonly Color ColorBgControl = Color.FromArgb(63, 63, 70);   // zinc-700
        public static readonly Color ColorTextLight = Color.FromArgb(244, 244, 245); // zinc-100
        public static readonly Color ColorTextMuted = Color.FromArgb(161, 161, 170); // zinc-400
        public static readonly Color ColorAccentBlue = Color.FromArgb(37, 99, 235);  // blue-600
        public static readonly Color ColorAccentHover = Color.FromArgb(29, 78, 216); // blue-700
        public static readonly Color ColorBorder = Color.FromArgb(82, 82, 91);       // zinc-600

        public static readonly Font FontDefault = new Font("Segoe UI", 9F, FontStyle.Regular);
        public static readonly Font FontBold = new Font("Segoe UI", 9F, FontStyle.Bold);
        public static readonly Font FontHeader = new Font("Segoe UI", 11F, FontStyle.Bold);

        public static void ApplyTheme(Form form)
        {
            if (form == null) return;

            form.BackColor = ColorBgDark;
            form.ForeColor = ColorTextLight;
            form.Font = FontDefault;

            // Enable Double Buffering to reduce layout refresh flickering
            System.Reflection.PropertyInfo dbProp = typeof(Control).GetProperty("DoubleBuffered", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            dbProp?.SetValue(form, true, null);

            ApplyThemeToControls(form.Controls);
        }

        private static void ApplyThemeToControls(Control.ControlCollection controls)
        {
            foreach (Control control in controls)
            {
                // Recursive call for container controls
                if (control.HasChildren && !(control is TreeView || control is ListView || control is DataGridView))
                {
                    ApplyThemeToControls(control.Controls);
                }

                // General Font Update
                if (control.Font.Name == "Microsoft Sans Serif" || control.Font.Name == "Tahoma" || control.Font.Name == "System")
                {
                    control.Font = control.Font.Bold ? FontBold : FontDefault;
                }

                // Apply styles based on control type
                if (control is Panel panel)
                {
                    if (panel.BorderStyle == BorderStyle.Fixed3D)
                        panel.BorderStyle = BorderStyle.FixedSingle;
                    
                    panel.BackColor = ColorBgPanel;
                    panel.ForeColor = ColorTextLight;
                }
                else if (control is Button button)
                {
                    button.FlatStyle = FlatStyle.Flat;
                    button.FlatAppearance.BorderSize = 0;
                    
                    // Highlight primary buttons or make them all cohesive
                    if (button.Name.ToLower().Contains("start") || button.Name.ToLower().Contains("create") || button.Name.ToLower().Contains("save"))
                    {
                        button.BackColor = ColorAccentBlue;
                        button.ForeColor = Color.White;
                        button.FlatAppearance.MouseOverBackColor = ColorAccentHover;
                    }
                    else
                    {
                        button.BackColor = ColorBgControl;
                        button.ForeColor = ColorTextLight;
                        button.FlatAppearance.MouseOverBackColor = Color.FromArgb(82, 82, 91);
                    }
                }
                else if (control is TextBox textBox)
                {
                    textBox.BorderStyle = BorderStyle.FixedSingle;
                    textBox.BackColor = ColorBgPanel;
                    textBox.ForeColor = ColorTextLight;
                }
                else if (control is TreeView treeView)
                {
                    treeView.BorderStyle = BorderStyle.FixedSingle;
                    treeView.BackColor = ColorBgDark;
                    treeView.ForeColor = ColorTextLight;
                    treeView.LineColor = ColorTextMuted;
                }
                else if (control is ListView listView)
                {
                    listView.BorderStyle = BorderStyle.FixedSingle;
                    listView.BackColor = ColorBgDark;
                    listView.ForeColor = ColorTextLight;
                }
                else if (control is StatusStrip statusStrip)
                {
                    statusStrip.BackColor = ColorBgPanel;
                    statusStrip.ForeColor = ColorTextLight;
                    statusStrip.RenderMode = ToolStripRenderMode.Professional;
                }
                else if (control is MenuStrip menuStrip)
                {
                    menuStrip.BackColor = ColorBgPanel;
                    menuStrip.ForeColor = ColorTextLight;
                    menuStrip.RenderMode = ToolStripRenderMode.Professional;
                    
                    // Dark theme for sub-items
                    foreach (ToolStripItem item in menuStrip.Items)
                    {
                        item.ForeColor = ColorTextLight;
                        if (item is ToolStripMenuItem menuItem)
                        {
                            ApplyThemeToMenuItems(menuItem);
                        }
                    }
                }
                else if (control is ToolStrip toolStrip)
                {
                    toolStrip.BackColor = ColorBgPanel;
                    toolStrip.ForeColor = ColorTextLight;
                    toolStrip.RenderMode = ToolStripRenderMode.Professional;
                    foreach (ToolStripItem item in toolStrip.Items)
                    {
                        item.ForeColor = ColorTextLight;
                    }
                }
                else if (control is GroupBox groupBox)
                {
                    groupBox.ForeColor = ColorAccentBlue; // Accent header
                    groupBox.BackColor = ColorBgPanel;
                }
                else if (control is TabControl tabControl)
                {
                    tabControl.BackColor = ColorBgPanel;
                    foreach (TabPage page in tabControl.TabPages)
                    {
                        page.BackColor = ColorBgPanel;
                        page.ForeColor = ColorTextLight;
                        ApplyThemeToControls(page.Controls);
                    }
                }
                else if (control is Label label)
                {
                    // Label text colors
                    if (label.ForeColor == SystemColors.ControlText || label.ForeColor == Color.Black)
                    {
                        label.ForeColor = ColorTextLight;
                    }
                }
                else if (control is CheckBox checkBox)
                {
                    if (checkBox.ForeColor == SystemColors.ControlText || checkBox.ForeColor == Color.Black)
                        checkBox.ForeColor = ColorTextLight;
                }
                else if (control is RadioButton radioButton)
                {
                    if (radioButton.ForeColor == SystemColors.ControlText || radioButton.ForeColor == Color.Black)
                        radioButton.ForeColor = ColorTextLight;
                }
            }
        }

        private static void ApplyThemeToMenuItems(ToolStripMenuItem parent)
        {
            foreach (ToolStripItem item in parent.DropDownItems)
            {
                item.ForeColor = ColorTextLight;
                item.BackColor = ColorBgPanel;
                if (item is ToolStripMenuItem subItem)
                {
                    ApplyThemeToMenuItems(subItem);
                }
            }
        }
    }
}
