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

        // Modern Premium Dark Palette (Harmonious zinc and blue shades)
        private static readonly Color[] DarkPalette = new Color[]
        {
            Color.FromArgb(24, 24, 27),      // BgDark (zinc-900)
            Color.FromArgb(39, 39, 42),      // BgPanel (zinc-800)
            Color.FromArgb(63, 63, 70),      // BgControl (zinc-700)
            Color.FromArgb(244, 244, 245),   // TextMain (zinc-100)
            Color.FromArgb(161, 161, 170),   // TextMuted (zinc-400)
            Color.FromArgb(37, 99, 235),     // AccentBlue (blue-600)
            Color.FromArgb(29, 78, 216),     // AccentHover (blue-700)
            Color.FromArgb(82, 82, 91)       // Border (zinc-600)
        };

        // Modern Premium Light Palette (Clean zinc and slate shades)
        private static readonly Color[] LightPalette = new Color[]
        {
            Color.FromArgb(244, 244, 245),   // BgDark (zinc-100)
            Color.FromArgb(255, 255, 255),   // BgPanel (white)
            Color.FromArgb(228, 228, 231),   // BgControl (zinc-200)
            Color.FromArgb(24, 24, 27),      // TextMain (zinc-900)
            Color.FromArgb(113, 113, 122),   // TextMuted (zinc-500)
            Color.FromArgb(37, 99, 235),     // AccentBlue (blue-600)
            Color.FromArgb(219, 234, 254),   // AccentHover (light blue highlights)
            Color.FromArgb(212, 212, 216)    // Border (zinc-300)
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
                if (control is Panel panel)
                {
                    if (panel.BorderStyle == BorderStyle.Fixed3D)
                        panel.BorderStyle = BorderStyle.FixedSingle;

                    panel.BackColor = GetColor(ColorKey.BgDark); // Match form background
                    panel.ForeColor = GetColor(ColorKey.TextMain);
                }
                else if (control is Button button)
                {
                    button.FlatStyle = FlatStyle.Flat;
                    button.FlatAppearance.BorderSize = 0;

                    // Cohesive style based on naming convention
                    string nameLower = button.Name.ToLower();
                    if (nameLower.Contains("start") || nameLower.Contains("create") || nameLower.Contains("save") || nameLower.Contains("add") || nameLower.Contains("scan"))
                    {
                        button.BackColor = GetColor(ColorKey.AccentBlue);
                        button.ForeColor = Color.White;
                        button.FlatAppearance.MouseOverBackColor = GetColor(ColorKey.AccentHover);
                    }
                    else
                    {
                        button.BackColor = GetColor(ColorKey.BgControl);
                        button.ForeColor = GetColor(ColorKey.TextMain);
                        button.FlatAppearance.MouseOverBackColor = CurrentTheme == "Light" 
                            ? Color.FromArgb(212, 212, 216) 
                            : Color.FromArgb(82, 82, 91);
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
                    treeView.BorderStyle = BorderStyle.FixedSingle;
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
                    groupBox.ForeColor = GetColor(ColorKey.AccentBlue); // Accent header
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
                    checkedListBox.BorderStyle = BorderStyle.FixedSingle;
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

        // Custom ListView Draw Handlers
        private static void ListView_DrawColumnHeader(object sender, DrawListViewColumnHeaderEventArgs e)
        {
            Color headerBg = GetColor(ColorKey.BgControl);
            Color headerText = GetColor(ColorKey.TextMain);
            Color borderColor = GetColor(ColorKey.Border);

            using (var brush = new SolidBrush(headerBg))
            {
                e.Graphics.FillRectangle(brush, e.Bounds);
            }

            using (var pen = new Pen(borderColor))
            {
                e.Graphics.DrawRectangle(pen, e.Bounds.X, e.Bounds.Y, e.Bounds.Width - 1, e.Bounds.Height - 1);
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
                Color headerText = isActive ? Color.White : GetColor(ColorKey.TextMain);

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
