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
        public static string Undo => If("撤销", "Undo");
        public static string Redo => If("重做", "Redo");
        public static string AddSibling => If("添加同级", "Add Sibling");
        public static string AddChild => If("添加子节点", "Add Child");
        public static string Delete => If("删除", "Delete");
        public static string CollapseExpand => If("折叠/展开", "Collapse/Expand");
        public static string Properties => If("属性", "Properties");
        public static string BatchFormat => If("批量格式", "Batch Format");
        public static string OpenLink => If("打开链接", "Open Link");
        public static string Copy => If("复制", "Copy");
        public static string Paste => If("粘贴", "Paste");
        public static string MoveUp => If("上移", "Move Up");
        public static string MoveDown => If("下移", "Move Down");
        public static string Save => If("保存", "Save");
        public static string ExportPng => If("导出 PNG", "Export PNG");
        public static string ExportSvg => If("导出 SVG", "Export SVG");
        public static string FitView => If("适应窗口", "Fit View");
        public static string ResetZoom => If("100%", "100%");
        public static string SearchLabel => If("查找:", "Find:");
        public static string FindPrevious => If("上一个", "Previous");
        public static string FindNext => If("下一个", "Next");

        // Toolbar labels
        public static string LayoutLabel => If("布局:", "Layout:");
        public static string ThemeLabel => If("主题:", "Theme:");
        public static string ShapeLabel => If("形状:", "Shape:");
        public static string NodeSizeLabel => If("尺寸:", "Size:");
        public static string ConnectionLabel => If("连接线:", "Line:");
        public static string EndpointLabel => If("端点:", "Endpoint:");

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
        public static string NodeSizeFixed => If("固定尺寸", "Fixed");
        public static string NodeSizeAutoFit => If("适应内容", "Fit Content");

        // Connection combo
        public static string ConnectionCurved => If("曲线", "Curved");
        public static string ConnectionStraight => If("直线", "Straight");
        public static string ConnectionOrthogonal => If("折线", "Orthogonal");
        public static string ConnectionClassicMindMap => If("经典脑图", "Classic Mind Map");

        public static string EndpointNone => If("无", "None");
        public static string EndpointArrow => If("箭头", "Arrow");
        public static string EndpointDoubleArrow => If("双向箭头", "Double Arrow");
        public static string EndpointCircle => If("圆点", "Circle");
        public static string EndpointDiamond => If("菱形", "Diamond");

        // Tooltips
        public static string TipUndo => If("撤销上一步操作 (Ctrl+Z)", "Undo last action (Ctrl+Z)");
        public static string TipRedo => If("重做上一步操作 (Ctrl+Y)", "Redo last action (Ctrl+Y)");
        public static string TipAddSibling => If("添加同级节点 (Enter)", "Add sibling node (Enter)");
        public static string TipAddChild => If("添加子节点 (Tab)", "Add child node (Tab)");
        public static string TipDelete => If("删除节点 (Delete)", "Delete node (Delete)");
        public static string TipCollapse => If("折叠/展开 (Space)", "Collapse/Expand (Space)");
        public static string TipProperties => If("编辑节点属性 (F4)", "Edit node properties (F4)");
        public static string TipBatchFormat => If("批量设置所选节点的颜色和图标", "Set colors and icons for selected nodes");
        public static string TipOpenLink => If("打开节点链接 (Ctrl+Enter)", "Open node link (Ctrl+Enter)");
        public static string TipCopy => If("复制节点及其子节点 (Ctrl+C)", "Copy node and descendants (Ctrl+C)");
        public static string TipPaste => If("粘贴为选中节点的子节点 (Ctrl+V)", "Paste as a child of the selected node (Ctrl+V)");
        public static string TipMoveUp => If("在同级节点中上移 (Alt+↑)", "Move up among siblings (Alt+Up)");
        public static string TipMoveDown => If("在同级节点中下移 (Alt+↓)", "Move down among siblings (Alt+Down)");
        public static string TipSave => If("保存到 OneNote (Ctrl+S)", "Save to OneNote (Ctrl+S)");
        public static string TipExportPng => If("导出 PNG", "Export PNG");
        public static string TipExportSvg => If("导出 SVG", "Export SVG");
        public static string TipFitView => If("缩放以显示完整脑图", "Zoom to show the entire mind map");
        public static string TipResetZoom => If("恢复 100% 缩放；按住 Ctrl 滚轮可缩放", "Reset to 100%; hold Ctrl and use the mouse wheel to zoom");
        public static string TipSearch => If("搜索节点文本、备注和链接 (Ctrl+F)", "Search node text, notes, and links (Ctrl+F)");
        public static string TipNodeSize => If("固定尺寸保持紧凑；适应内容会自动换行并显示全文", "Fixed keeps nodes compact; Fit Content wraps text and shows it in full");

        // Messages
        public static string NewNodeText => If("新节点", "New Node");
        public static string SavedToOneNote => If("已保存到 OneNote 页面", "Saved to OneNote page");
        public static string PngExported => If("PNG 已导出", "PNG exported");
        public static string SvgExported => If("SVG 已导出", "SVG exported");
        public static string SaveFailed => If("保存失败", "Save failed");
        public static string ExportFailed => If("导出失败", "Export failed");
        public static string UnsavedChanges => If("有未保存的修改，是否保存？", "Unsaved changes. Save before closing?");
        public static string UnsavedTitle => If("OneNote 脑图", "OneNote Mind Map");
        public static string ClipboardFailed => If("无法访问剪贴板", "Could not access the clipboard");
        public static string InvalidLink => If("无法打开该链接", "Could not open this link");
        public static string SearchNoResults => If("未找到匹配的节点", "No matching nodes found");
        public static string SearchResultFormat(int current, int total) =>
            If($"查找结果: {current}/{total}", $"Search result: {current}/{total}");
        public static string SelectedStatus(int selectedCount) =>
            If($"已选择: {selectedCount}", $"Selected: {selectedCount}");

        // Node properties
        public static string NodePropertiesTitle => If("节点属性", "Node Properties");
        public static string NodeTextLabel => If("文本:", "Text:");
        public static string NodeIconLabel => If("图标:", "Icon:");
        public static string NodeColorLabel => If("颜色:", "Color:");
        public static string NodeNoteLabel => If("备注:", "Note:");
        public static string NodeLinkLabel => If("链接:", "Link:");
        public static string NodePropertiesHint => If("颜色可填写 #RRGGBB；图标可使用一个 Emoji。备注和链接会显示在节点提示中。", "Use #RRGGBB for color and an emoji for the icon. Notes and links appear in the node tooltip.");
        public static string NodeTextRequired => If("节点文本不能为空。", "Node text cannot be empty.");
        public static string InvalidNodeColor => If("颜色格式无效，请使用 #RRGGBB，或留空使用主题颜色。", "Invalid color. Use #RRGGBB or leave it blank to use the theme color.");
        public static string Clear => If("清除", "Clear");
        public static string Ok => If("确定", "OK");
        public static string Cancel => If("取消", "Cancel");

        // Batch format
        public static string BatchFormatTitle => If("批量格式设置", "Batch Formatting");
        public static string BatchFormatSummary(int count) => If($"将格式应用到 {count} 个节点", $"Apply formatting to {count} nodes");
        public static string ApplyColor => If("应用颜色", "Apply color");
        public static string ApplyIcon => If("应用图标", "Apply icon");
        public static string ClearFormatting => If("清除格式", "Clear Formatting");
        public static string BatchFormatHint => If("勾选要修改的项目；留空会清除对应格式。", "Select the fields to change; leave a value empty to clear that formatting.");
        public static string BatchFormatSelectOption => If("请至少选择一个要应用的格式项目。", "Select at least one formatting option to apply.");

        // File dialogs
        public static string PngFilter => If("PNG 图片|*.png", "PNG Image|*.png");
        public static string SvgFilter => If("SVG 文件|*.svg", "SVG File|*.svg");

        // Status bar
        public static string StatusFormat(int nodeCount, int zoomPercent) =>
            If($"节点数: {nodeCount}  |  缩放: {zoomPercent}%",
               $"Nodes: {nodeCount}  |  Zoom: {zoomPercent}%");
    }
}
