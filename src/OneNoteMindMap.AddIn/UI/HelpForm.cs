using System;
using System.Drawing;
using System.Windows.Forms;

namespace OneNoteMindMap.UI
{
    /// <summary>Help window for the OneNote Mind Map add-in, following the same architecture as OneNote Markdown.</summary>
    internal sealed class HelpForm : Form
    {
        private TreeView _treeView;
        private WebBrowser _browser;

        private HelpForm()
        {
            InitializeComponents();
            PopulateTree();
            ShowContent("about");
        }

        public static void ShowHelp(IWin32Window owner)
        {
            using (var form = new HelpForm())
            {
                if (owner == null) form.ShowDialog(); else form.ShowDialog(owner);
            }
        }

        private void InitializeComponents()
        {
            Text = Strings.If("OneNote 脑图 - 帮助", "OneNote Mind Map - Help");
            Size = new Size(1320, 900);
            MinimumSize = new Size(980, 700);
            StartPosition = FormStartPosition.CenterParent;
            FormBorderStyle = FormBorderStyle.Sizable;
            ShowInTaskbar = true;
            TopMost = true;
            ShowIcon = false;

            // Header panel
            var headerPanel = new Panel
            {
                Dock = DockStyle.Top,
                Height = 56,
                BackColor = Color.FromArgb(104, 33, 122)
            };
            var headerLabel = new Label
            {
                Text = "  ✦  " + Strings.If("使用帮助", "Help"),
                ForeColor = Color.White,
                Font = new Font("Microsoft YaHei UI", 19f, FontStyle.Bold),
                AutoSize = false,
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleLeft,
                Padding = new Padding(12, 0, 0, 0)
            };
            headerPanel.Controls.Add(headerLabel);

            // Bottom panel with close button
            var bottomPanel = new Panel
            {
                Dock = DockStyle.Bottom,
                Height = 58,
                BackColor = Color.FromArgb(248, 248, 248),
                Padding = new Padding(0, 8, 16, 8)
            };
            var closeButton = new Button
            {
                Text = Strings.If("关闭", "Close"),
                Size = new Size(96, 36),
                Anchor = AnchorStyles.Right | AnchorStyles.Bottom,
                FlatStyle = FlatStyle.System
            };
            closeButton.Click += delegate { Close(); };
            bottomPanel.Controls.Add(closeButton);
            closeButton.Location = new Point(bottomPanel.Width - closeButton.Width - 16, 9);
            bottomPanel.Resize += delegate
            {
                closeButton.Location = new Point(bottomPanel.Width - closeButton.Width - 16, 9);
            };

            // Left tree panel (fixed width, no SplitContainer)
            var treePanel = new Panel
            {
                Dock = DockStyle.Left,
                Width = 280,
                BackColor = Color.FromArgb(252, 252, 252)
            };

            _treeView = new TreeView
            {
                Dock = DockStyle.Fill,
                Font = new Font("Microsoft YaHei UI", 9.5f),
                BorderStyle = BorderStyle.None,
                BackColor = Color.FromArgb(252, 252, 252),
                ShowLines = true,
                ShowRootLines = true,
                HideSelection = false,
                ItemHeight = 24
            };
            _treeView.AfterSelect += TreeView_AfterSelect;
            treePanel.Controls.Add(_treeView);

            // Splitter bar (1px visual separator)
            var splitterBar = new Panel
            {
                Dock = DockStyle.Left,
                Width = 1,
                BackColor = Color.FromArgb(220, 220, 220)
            };

            // Right: WebBrowser for HTML content
            _browser = new WebBrowser
            {
                Dock = DockStyle.Fill,
                IsWebBrowserContextMenuEnabled = false,
                ScriptErrorsSuppressed = true
            };

            // Assembly order: Fill first, then non-Fill
            Controls.Add(_browser);
            Controls.Add(splitterBar);
            Controls.Add(treePanel);
            Controls.Add(bottomPanel);
            Controls.Add(headerPanel);
            CancelButton = closeButton;
        }

        private void PopulateTree()
        {
            var root = new TreeNode(Strings.If("目录", "Contents")) { Tag = "about" };
            root.Nodes.Add(new TreeNode(Strings.If("快速入门", "Quick Start")) { Tag = "quickstart" });

            var features = new TreeNode(Strings.If("功能说明", "Features")) { Tag = "features" };
            features.Nodes.Add(new TreeNode(Strings.If("新建脑图", "New Mind Map")) { Tag = "feat_new" });
            features.Nodes.Add(new TreeNode(Strings.If("从页面生成", "From Page")) { Tag = "feat_generate" });
            features.Nodes.Add(new TreeNode(Strings.If("编辑与保存", "Edit & Save")) { Tag = "feat_edit" });
            features.Nodes.Add(new TreeNode(Strings.If("插入预览", "Insert Preview")) { Tag = "feat_preview" });
            features.Nodes.Add(new TreeNode(Strings.If("导出 PNG / SVG", "Export PNG / SVG")) { Tag = "feat_export" });
            features.Nodes.Add(new TreeNode(Strings.If("转为大纲", "To Outline")) { Tag = "feat_outline" });
            root.Nodes.Add(features);

            root.Nodes.Add(new TreeNode(Strings.If("编辑器快捷键", "Shortcuts")) { Tag = "shortcuts" });
            root.Nodes.Add(new TreeNode(Strings.If("常见问题", "FAQ")) { Tag = "faq" });
            root.Nodes.Add(new TreeNode(Strings.If("关于", "About")) { Tag = "about" });

            _treeView.Nodes.Add(root);
            root.ExpandAll();
            _treeView.SelectedNode = root.Nodes[root.Nodes.Count - 1]; // select "关于"
        }

        private void TreeView_AfterSelect(object sender, TreeViewEventArgs e)
        {
            if (e.Node == null) return;
            string tag = e.Node.Tag as string;
            if (!string.IsNullOrEmpty(tag))
                ShowContent(tag);
        }

        private void ShowContent(string section)
        {
            string body = HelpContent.GetHtml(section);
            _browser.DocumentText =
                "<!DOCTYPE html><html><head><meta charset='utf-8'/><style>"
                + "body{font-family:'Microsoft YaHei UI','Segoe UI',sans-serif;font-size:22px;color:#333;margin:34px 40px;line-height:1.8;}"
                + "h1{font-size:48px;color:#222;margin-bottom:12px;}"
                + "h2{font-size:36px;color:#333;margin-top:30px;border-bottom:1px solid #eee;padding-bottom:6px;}"
                + "p{margin:8px 0;}"
                + "ul,ol{padding-left:22px;}"
                + "li{margin:4px 0;}"
                + "code{background:#f5f5f5;padding:2px 6px;border-radius:3px;font-family:Consolas,'Courier New',monospace;font-size:18px;}"
                + "pre{background:#f8f8f8;border:1px solid #e0e0e0;border-radius:4px;padding:12px 16px;overflow-x:auto;margin:10px 0;}"
                + "pre code{background:none;padding:0;font-size:17px;line-height:1.6;}"
                + "table{border-collapse:collapse;margin:12px 0;width:100%;}"
                + "th,td{border:1px solid #ddd;padding:7px 14px;text-align:left;}"
                + "th{background:#f7f7f7;font-weight:600;}"
                + ".version{color:#888;font-size:19px;margin-bottom:20px;}"
                + "</style></head><body>" + body + "</body></html>";
        }
    }
}
