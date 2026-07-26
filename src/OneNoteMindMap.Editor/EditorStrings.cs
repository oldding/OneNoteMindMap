using System.Threading;

namespace OneNoteMindMap.Editor
{
    /// <summary>
    /// Lightweight i18n helper for the WPF editor, mirroring the AddIn's Strings pattern.
    /// </summary>
    public static class EditorStrings
    {
        public static bool IsChinese =>
            Thread.CurrentThread.CurrentUICulture.TwoLetterISOLanguageName == "zh" ||
            Thread.CurrentThread.CurrentUICulture.Name.StartsWith("zh");

        public static string If(string zh, string en) => IsChinese ? zh : en;

        // Window
        public static string EditorTitle => If("脑图编辑器", "Mind Map Editor");
        public static string Ready => If("就绪", "Ready");

        // Toolbar buttons
        public static string AddSibling => If("添加同级", "Add Sibling");
        public static string AddChild => If("添加子节点", "Add Child");
        public static string Delete => If("删除", "Delete");
        public static string CollapseExpand => If("折叠/展开", "Collapse/Expand");
        public static string Save => If("保存", "Save");
        public static string ExportPng => If("导出 PNG", "Export PNG");
        public static string ExportSvg => If("导出 SVG", "Export SVG");

        // Toolbar labels
        public static string LayoutLabel => If("布局:", "Layout:");
        public static string ThemeLabel => If("主题:", "Theme:");
        public static string ShapeLabel => If("形状:", "Shape:");
        public static string ConnectionLabel => If("连接线:", "Line:");

        // Layout combo
        public static string LayoutRightTree => If("右侧树形", "Right Tree");
        public static string LayoutBothSides => If("两侧分布", "Both Sides");
        public static string LayoutOrgChart => If("组织结构图", "Org Chart");

        // Theme combo
        public static string ThemeDefault => If("默认", "Default");
        public static string ThemePurple => If("紫色", "Purple");
        public static string ThemeMinimal => If("简约", "Minimal");
        public static string ThemeFresh => If("清新", "Fresh");
        public static string ThemeWarm => If("暖色", "Warm");

        // Shape combo
        public static string ShapeRounded => If("圆角", "Rounded");
        public static string ShapeRectangle => If("矩形", "Rectangle");
        public static string ShapePill => If("胶囊", "Pill");

        // Connection combo
        public static string ConnectionCurved => If("曲线", "Curved");
        public static string ConnectionStraight => If("直线", "Straight");
        public static string ConnectionOrthogonal => If("折线", "Orthogonal");

        // Tooltips
        public static string TipAddSibling => If("添加同级节点 (Enter)", "Add sibling node (Enter)");
        public static string TipAddChild => If("添加子节点 (Tab)", "Add child node (Tab)");
        public static string TipDelete => If("删除节点 (Delete)", "Delete node (Delete)");
        public static string TipCollapse => If("折叠/展开 (Space)", "Collapse/Expand (Space)");
        public static string TipSave => If("保存到 OneNote (Ctrl+S)", "Save to OneNote (Ctrl+S)");
        public static string TipExportPng => If("导出 PNG", "Export PNG");
        public static string TipExportSvg => If("导出 SVG", "Export SVG");

        // Messages
        public static string NewNodeText => If("新节点", "New Node");
        public static string SavedToOneNote => If("已保存到 OneNote 页面", "Saved to OneNote page");
        public static string PngExported => If("PNG 已导出", "PNG exported");
        public static string SvgExported => If("SVG 已导出", "SVG exported");
        public static string SaveFailed => If("保存失败", "Save failed");
        public static string ExportFailed => If("导出失败", "Export failed");
        public static string UnsavedChanges => If("有未保存的修改，是否保存？", "Unsaved changes. Save before closing?");
        public static string UnsavedTitle => If("OneNote 脑图", "OneNote Mind Map");

        // File dialogs
        public static string PngFilter => If("PNG 图片|*.png", "PNG Image|*.png");
        public static string SvgFilter => If("SVG 文件|*.svg", "SVG File|*.svg");

        // Status bar
        public static string StatusFormat(int nodeCount, int zoomPercent) =>
            If($"节点数: {nodeCount}  |  缩放: {zoomPercent}%",
               $"Nodes: {nodeCount}  |  Zoom: {zoomPercent}%");
    }
}
