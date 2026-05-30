using System;
using System.Drawing;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace FOCA.Utilities
{
    public static class ThemeManager
    {
        [DllImport("uxtheme.dll", CharSet = CharSet.Unicode)]
        private static extern int SetWindowTheme(IntPtr hWnd, string pszSubAppName, string pszSubIdList);

        public enum ColorKey
        {
            BgDark,
            BgPanel,
            BgControl,
            TextMain,
            TextMuted,
            AccentBlue,
            AccentHover,
            Border
        }

        // Modern Cyber Dark Palette (Deep graphite-black space theme with glowing cyan highlights)
        private static readonly Color[] DarkPalette = new Color[]
        {
            Color.FromArgb(7, 10, 19),       // BgDark (deep cosmic slate black - #070a13)
            Color.FromArgb(15, 23, 42),      // BgPanel (slate navy cards - #0f172a)
            Color.FromArgb(30, 41, 59),      // BgControl (slate control bg - #1e293b)
            Color.FromArgb(248, 250, 252),   // TextMain (bright slate white - #f8fafc)
            Color.FromArgb(148, 163, 184),   // TextMuted (slate gray - #94a3b8)
            Color.FromArgb(0, 240, 255),     // AccentBlue (cyber neon cyan highlight)
            Color.FromArgb(2, 132, 199),     // AccentHover (deep cyber blue)
            Color.FromArgb(30, 41, 59)       // Border (crisp slate steel lines - #1e293b)
        };

        // Modern Cyber Light Palette (Clean zinc and slate light theme with ocean blue accents)
        private static readonly Color[] LightPalette = new Color[]
        {
            Color.FromArgb(241, 245, 249),   // BgDark (slate light gray - #f1f5f9)
            Color.FromArgb(255, 255, 255),   // BgPanel (white card panels)
            Color.FromArgb(226, 232, 240),   // BgControl (soft gray controls - #e2e8f0)
            Color.FromArgb(15, 23, 42),      // TextMain (slate navy text - #0f172a)
            Color.FromArgb(100, 116, 139),   // TextMuted (muted slate gray - #64748b)
            Color.FromArgb(37, 99, 235),     // AccentBlue (royal blue accent)
            Color.FromArgb(29, 78, 216),     // AccentHover (darker royal blue)
            Color.FromArgb(203, 213, 225)    // Border (clean slate gray borders - #cbd5e1)
        };

        public static readonly Font FontDefault = new Font("Segoe UI", 9F, FontStyle.Regular);
        public static readonly Font FontBold = new Font("Segoe UI", 9F, FontStyle.Bold);

        private static readonly string ThemeFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "theme.txt");
        public static string CurrentTheme { get; private set; } = "Dark"; // Default to Dark

        static ThemeManager()
        {
            LoadThemePreference();
        }

        public static void LoadThemePreference()
        {
            try
            {
                if (File.Exists(ThemeFilePath))
                {
                    string content = File.ReadAllText(ThemeFilePath).Trim();
                    if (content.Equals("Light", StringComparison.OrdinalIgnoreCase))
                    {
                        CurrentTheme = "Light";
                        return;
                    }
                }
            }
            catch { }
            CurrentTheme = "Dark";
        }

        public static void SaveThemePreference(string themeName)
        {
            try
            {
                File.WriteAllText(ThemeFilePath, themeName);
            }
            catch { }
            CurrentTheme = themeName;
        }

        public static Color GetColor(ColorKey key)
        {
            int index = (int)key;
            return CurrentTheme == "Light" ? LightPalette[index] : DarkPalette[index];
        }

        public static void ApplyTheme(Form form)
        {
            if (form == null) return;

            form.BackColor = GetColor(ColorKey.BgDark);
            form.ForeColor = GetColor(ColorKey.TextMain);
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
                if (control.HasChildren && !(control is TreeView || control is ListView || control is DataGridView || control is CheckedListBox || control is ListBox))
                {
                    ApplyThemeToControls(control.Controls);
                }

                // General Font Update
                if (control.Font.Name == "Microsoft Sans Serif" || control.Font.Name == "Tahoma" || control.Font.Name == "System")
                {
                    control.Font = control.Font.Bold ? FontBold : FontDefault;
                }

                // Apply styles based on control type
                if (control is SplitContainer splitContainer)
                {
                    splitContainer.BorderStyle = BorderStyle.None;
                    splitContainer.BackColor = GetColor(ColorKey.BgDark);
                    splitContainer.Panel1.BackColor = GetColor(ColorKey.BgDark);
                    splitContainer.Panel2.BackColor = GetColor(ColorKey.BgDark);

                    splitContainer.Paint -= SplitContainer_Paint;
                    splitContainer.Paint += SplitContainer_Paint;

                    ApplyThemeToControls(splitContainer.Panel1.Controls);
                    ApplyThemeToControls(splitContainer.Panel2.Controls);
                }
                else if (control is UserControl userControl)
                {
                    userControl.BackColor = GetColor(ColorKey.BgDark);
                    userControl.ForeColor = GetColor(ColorKey.TextMain);
                    ApplyThemeToControls(userControl.Controls);
                }
                else if (control is TableLayoutPanel tablePanel)
                {
                    tablePanel.BackColor = GetColor(ColorKey.BgDark);
                    tablePanel.ForeColor = GetColor(ColorKey.TextMain);
                    ApplyThemeToControls(tablePanel.Controls);
                }
                else if (control is FlowLayoutPanel flowPanel)
                {
                    flowPanel.BackColor = GetColor(ColorKey.BgDark);
                    flowPanel.ForeColor = GetColor(ColorKey.TextMain);
                    ApplyThemeToControls(flowPanel.Controls);
                }
                else if (control is Panel panel)
                {
                    if (panel.BorderStyle == BorderStyle.Fixed3D)
                        panel.BorderStyle = BorderStyle.FixedSingle;

                    panel.BackColor = GetColor(ColorKey.BgDark);
                    panel.ForeColor = GetColor(ColorKey.TextMain);
                }
                else if (control is Button button)
                {
                    button.FlatStyle = FlatStyle.Flat;
                    button.FlatAppearance.BorderSize = 1;
                    button.FlatAppearance.BorderColor = GetColor(ColorKey.Border);

                    string nameLower = button.Name.ToLower();
                    if (nameLower.Contains("start") || nameLower.Contains("create") || nameLower.Contains("save") || nameLower.Contains("add") || nameLower.Contains("scan") || nameLower.Contains("search"))
                    {
                        button.BackColor = GetColor(ColorKey.AccentBlue);
                        button.ForeColor = CurrentTheme == "Dark" ? Color.FromArgb(7, 10, 19) : Color.White;
                        button.FlatAppearance.BorderColor = GetColor(ColorKey.AccentBlue);
                        button.FlatAppearance.MouseOverBackColor = GetColor(ColorKey.AccentHover);
                    }
                    else
                    {
                        button.BackColor = GetColor(ColorKey.BgControl);
                        button.ForeColor = GetColor(ColorKey.TextMain);
                        button.FlatAppearance.MouseOverBackColor = CurrentTheme == "Light"
                            ? Color.FromArgb(203, 213, 225)
                            : Color.FromArgb(47, 55, 71);
                    }
                }
                else if (control is TextBox textBox)
                {
                    textBox.BorderStyle = BorderStyle.FixedSingle;
                    textBox.BackColor = GetColor(ColorKey.BgPanel);
                    textBox.ForeColor = GetColor(ColorKey.TextMain);
                }
                else if (control is TreeView treeView)
                {
                    treeView.BorderStyle = BorderStyle.None;
                    treeView.BackColor = GetColor(ColorKey.BgPanel);
                    treeView.ForeColor = GetColor(ColorKey.TextMain);
                    treeView.LineColor = GetColor(ColorKey.TextMuted);

                    try
                    {
                        SetWindowTheme(treeView.Handle, "explorer", null);
                    }
                    catch { }
                }
                else if (control is ListView listView)
                {
                    listView.BorderStyle = BorderStyle.FixedSingle;
                    listView.BackColor = GetColor(ColorKey.BgPanel);
                    listView.ForeColor = GetColor(ColorKey.TextMain);

                    try
                    {
                        SetWindowTheme(listView.Handle, "explorer", null);
                    }
                    catch { }

                    // Custom flat column headers
                    listView.DrawColumnHeader -= ListView_DrawColumnHeader;
                    listView.DrawItem -= ListView_DrawItem;
                    listView.DrawSubItem -= ListView_DrawSubItem;

                    listView.OwnerDraw = true;
                    listView.DrawColumnHeader += ListView_DrawColumnHeader;
                    listView.DrawItem += ListView_DrawItem;
                    listView.DrawSubItem += ListView_DrawSubItem;
                }
                else if (control is StatusStrip statusStrip)
                {
                    statusStrip.BackColor = GetColor(ColorKey.BgPanel);
                    statusStrip.ForeColor = GetColor(ColorKey.TextMain);
                    statusStrip.RenderMode = ToolStripRenderMode.Professional;
                }
                else if (control is MenuStrip menuStrip)
                {
                    menuStrip.BackColor = GetColor(ColorKey.BgPanel);
                    menuStrip.ForeColor = GetColor(ColorKey.TextMain);
                    menuStrip.RenderMode = ToolStripRenderMode.Professional;

                    foreach (ToolStripItem item in menuStrip.Items)
                    {
                        item.ForeColor = GetColor(ColorKey.TextMain);
                        if (item is ToolStripMenuItem menuItem)
                        {
                            ApplyThemeToMenuItems(menuItem);
                        }
                    }
                }
                else if (control is ToolStrip toolStrip)
                {
                    toolStrip.BackColor = GetColor(ColorKey.BgPanel);
                    toolStrip.ForeColor = GetColor(ColorKey.TextMain);
                    toolStrip.RenderMode = ToolStripRenderMode.Professional;
                    foreach (ToolStripItem item in toolStrip.Items)
                    {
                        item.ForeColor = GetColor(ColorKey.TextMain);
                    }
                }
                else if (control is GroupBox groupBox)
                {
                    groupBox.ForeColor = GetColor(ColorKey.AccentBlue);
                    groupBox.BackColor = GetColor(ColorKey.BgDark);
                    ApplyThemeToControls(groupBox.Controls);
                }
                else if (control is TabControl tabControl)
                {
                    tabControl.BackColor = GetColor(ColorKey.BgDark);

                    // Enable owner-draw for custom modern tab headers
                    tabControl.DrawMode = TabDrawMode.OwnerDrawFixed;
                    tabControl.DrawItem -= TabControl_DrawItem;
                    tabControl.DrawItem += TabControl_DrawItem;

                    foreach (TabPage page in tabControl.TabPages)
                    {
                        page.BackColor = GetColor(ColorKey.BgDark);
                        page.ForeColor = GetColor(ColorKey.TextMain);
                        ApplyThemeToControls(page.Controls);
                    }
                }
                else if (control is Label label)
                {
                    if (label.ForeColor == SystemColors.ControlText || label.ForeColor == Color.Black || label.ForeColor == Color.Blue || label.ForeColor == Color.Red)
                    {
                        label.ForeColor = GetColor(ColorKey.TextMain);
                    }
                }
                else if (control is CheckBox checkBox)
                {
                    if (checkBox.ForeColor == SystemColors.ControlText || checkBox.ForeColor == Color.Black)
                        checkBox.ForeColor = GetColor(ColorKey.TextMain);
                }
                else if (control is RadioButton radioButton)
                {
                    if (radioButton.ForeColor == SystemColors.ControlText || radioButton.ForeColor == Color.Black)
                        radioButton.ForeColor = GetColor(ColorKey.TextMain);
                }
                else if (control is CheckedListBox checkedListBox)
                {
                    checkedListBox.BorderStyle = BorderStyle.None;
                    checkedListBox.BackColor = GetColor(ColorKey.BgPanel);
                    checkedListBox.ForeColor = GetColor(ColorKey.TextMain);
                }
                else if (control is ListBox listBox)
                {
                    listBox.BorderStyle = BorderStyle.FixedSingle;
                    listBox.BackColor = GetColor(ColorKey.BgPanel);
                    listBox.ForeColor = GetColor(ColorKey.TextMain);
                }
                else if (control is ComboBox comboBox)
                {
                    comboBox.FlatStyle = FlatStyle.Flat;
                    comboBox.BackColor = GetColor(ColorKey.BgPanel);
                    comboBox.ForeColor = GetColor(ColorKey.TextMain);
                }
                else if (control is DataGridView dataGridView)
                {
                    dataGridView.BorderStyle = BorderStyle.FixedSingle;
                    dataGridView.BackgroundColor = GetColor(ColorKey.BgPanel);
                    dataGridView.ForeColor = GetColor(ColorKey.TextMain);
                    dataGridView.GridColor = GetColor(ColorKey.Border);

                    dataGridView.EnableHeadersVisualStyles = false;
                    dataGridView.ColumnHeadersDefaultCellStyle.BackColor = GetColor(ColorKey.BgControl);
                    dataGridView.ColumnHeadersDefaultCellStyle.ForeColor = GetColor(ColorKey.TextMain);
                    dataGridView.RowHeadersDefaultCellStyle.BackColor = GetColor(ColorKey.BgControl);
                    dataGridView.RowHeadersDefaultCellStyle.ForeColor = GetColor(ColorKey.TextMain);

                    dataGridView.DefaultCellStyle.BackColor = GetColor(ColorKey.BgPanel);
                    dataGridView.DefaultCellStyle.ForeColor = GetColor(ColorKey.TextMain);

                    dataGridView.AlternatingRowsDefaultCellStyle.BackColor = GetColor(ColorKey.BgDark);
                    dataGridView.AlternatingRowsDefaultCellStyle.ForeColor = GetColor(ColorKey.TextMain);
                }
                else if (control is PictureBox pictureBox)
                {
                    if (pictureBox.Name.Equals("picFOCA", StringComparison.OrdinalIgnoreCase))
                    {
                        if (CurrentTheme == "Dark")
                        {
                            pictureBox.BackColor = Color.White;
                            pictureBox.Padding = new Padding(6);
                        }
                        else
                        {
                            pictureBox.BackColor = GetColor(ColorKey.BgDark);
                            pictureBox.Padding = new Padding(0);
                        }
                    }
                }
            }
        }

        private static void ApplyThemeToMenuItems(ToolStripMenuItem parent)
        {
            foreach (ToolStripItem item in parent.DropDownItems)
            {
                item.ForeColor = GetColor(ColorKey.TextMain);
                item.BackColor = GetColor(ColorKey.BgPanel);
                if (item is ToolStripMenuItem subItem)
                {
                    ApplyThemeToMenuItems(subItem);
                }
            }
        }

        // Custom SplitContainer Drawing
        private static void SplitContainer_Paint(object sender, PaintEventArgs e)
        {
            if (sender is SplitContainer splitContainer)
            {
                using (var pen = new Pen(GetColor(ColorKey.Border), 1))
                {
                    if (splitContainer.Orientation == Orientation.Vertical)
                    {
                        int x = splitContainer.SplitterDistance + (splitContainer.SplitterWidth / 2);
                        e.Graphics.DrawLine(pen, x, 0, x, splitContainer.Height);
                    }
                    else
                    {
                        int y = splitContainer.SplitterDistance + (splitContainer.SplitterWidth / 2);
                        e.Graphics.DrawLine(pen, 0, y, splitContainer.Width, y);
                    }
                }
            }
        }

        // Custom ListView Draw Handlers
        private static void ListView_DrawColumnHeader(object sender, DrawListViewColumnHeaderEventArgs e)
        {
            Color headerBg = GetColor(ColorKey.BgControl);
            Color headerText = GetColor(ColorKey.TextMain);
            Color borderColor = GetColor(ColorKey.Border);
            Color accentColor = GetColor(ColorKey.AccentBlue);

            using (var brush = new SolidBrush(headerBg))
            {
                e.Graphics.FillRectangle(brush, e.Bounds);
            }

            using (var pen = new Pen(borderColor))
            {
                e.Graphics.DrawRectangle(pen, e.Bounds.X, e.Bounds.Y, e.Bounds.Width - 1, e.Bounds.Height - 1);
            }

            // Draw thin cyber neon accent line at the bottom of headers
            using (var pen = new Pen(accentColor, 2))
            {
                e.Graphics.DrawLine(pen, e.Bounds.Left, e.Bounds.Bottom - 1, e.Bounds.Right, e.Bounds.Bottom - 1);
            }

            TextFormatFlags flags = TextFormatFlags.VerticalCenter | TextFormatFlags.HorizontalCenter | TextFormatFlags.SingleLine | TextFormatFlags.EndEllipsis;
            TextRenderer.DrawText(e.Graphics, e.Header.Text, e.Font ?? FontDefault, e.Bounds, headerText, flags);
        }

        private static void ListView_DrawItem(object sender, DrawListViewItemEventArgs e)
        {
            e.DrawDefault = true;
        }

        private static void ListView_DrawSubItem(object sender, DrawListViewSubItemEventArgs e)
        {
            e.DrawDefault = true;
        }

        // Custom TabControl Draw Handlers
        private static void TabControl_DrawItem(object sender, DrawItemEventArgs e)
        {
            if (sender is TabControl tabControl && e.Index >= 0 && e.Index < tabControl.TabPages.Count)
            {
                var tabPage = tabControl.TabPages[e.Index];
                var tabRect = tabControl.GetTabRect(e.Index);
                bool isActive = tabControl.SelectedIndex == e.Index;

                Color headerBg = isActive ? GetColor(ColorKey.AccentBlue) : GetColor(ColorKey.BgControl);
                Color headerText = isActive ? Color.FromArgb(7, 10, 19) : GetColor(ColorKey.TextMain); // Dark text for bright selected tab

                using (var brush = new SolidBrush(headerBg))
                {
                    e.Graphics.FillRectangle(brush, tabRect);
                }

                TextFormatFlags flags = TextFormatFlags.VerticalCenter | TextFormatFlags.HorizontalCenter | TextFormatFlags.SingleLine;
                TextRenderer.DrawText(e.Graphics, tabPage.Text, tabControl.Font ?? FontDefault, tabRect, headerText, flags);
            }
        }
    }
}
