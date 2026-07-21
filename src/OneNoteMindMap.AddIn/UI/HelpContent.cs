namespace OneNoteMindMap.UI
{
    /// <summary>Provides localized HTML help content for the mind map add-in.</summary>
    internal static class HelpContent
    {
        public static string GetHtml(string section)
        {
            if (!Strings.IsChinese)
                return GetEnglish(section);
            return GetChinese(section);
        }

        private static string GetChinese(string section)
        {
            switch (section)
            {
                case "quickstart": return GetQuickStartZh();
                case "features": return GetFeaturesZh();
                case "feat_new": return GetNewZh();
                case "feat_generate": return GetGenerateZh();
                case "feat_edit": return GetEditZh();
                case "feat_preview": return GetPreviewZh();
                case "feat_export": return GetExportZh();
                case "feat_outline": return GetOutlineZh();
                case "shortcuts": return GetShortcutsZh();
                case "faq": return GetFaqZh();
                default: return GetAboutZh();
            }
        }

        private static string GetEnglish(string section)
        {
            switch (section)
            {
                case "quickstart": return GetQuickStartEn();
                case "features": return GetFeaturesEn();
                case "feat_new": return GetNewEn();
                case "feat_generate": return GetGenerateEn();
                case "feat_edit": return GetEditEn();
                case "feat_preview": return GetPreviewEn();
                case "feat_export": return GetExportEn();
                case "feat_outline": return GetOutlineEn();
                case "shortcuts": return GetShortcutsEn();
                case "faq": return GetFaqEn();
                default: return GetAboutEn();
            }
        }

        // ═══════════════════════════════════════════════════════════
        // CHINESE
        // ═══════════════════════════════════════════════════════════

        private static string GetAboutZh()
        {
            return "<h1>OneNote 脑图 — 使用帮助</h1>"
                + "<p class='version'>版本：1.3.0</p>"
                + "<p>本插件由 OneNote MVP 开发。</p>"
                + "<h2>按钮一览</h2>"
                + "<table>"
                + "<tr><th>按钮</th><th>功能</th></tr>"
                + "<tr><td><b>新建</b></td><td>创建新的空白脑图</td></tr>"
                + "<tr><td><b>生成</b></td><td>将当前页面内容解析为脑图</td></tr>"
                + "<tr><td><b>编辑</b></td><td>打开 WPF 编辑器编辑当前脑图</td></tr>"
                + "<tr><td><b>大纲</b></td><td>将脑图转换为 OneNote 大纲文本</td></tr>"
                + "<tr><td><b>插入预览</b></td><td>插入/更新脑图的 PNG 预览图</td></tr>"
                + "<tr><td><b>导出 PNG</b></td><td>将脑图导出为 PNG 文件</td></tr>"
                + "<tr><td><b>导出 SVG</b></td><td>将脑图导出为 SVG 文件</td></tr>"
                + "<tr><td><b>帮助</b></td><td>查看使用说明</td></tr>"
                + "</table>"
                + "<h2>快捷键</h2>"
                + "<table>"
                + "<tr><th>快捷键</th><th>功能</th></tr>"
                + "<tr><td><code>Enter</code></td><td>添加同级节点</td></tr>"
                + "<tr><td><code>Tab</code></td><td>添加子节点</td></tr>"
                + "<tr><td><code>Delete</code></td><td>删除当前节点及其子节点</td></tr>"
                + "<tr><td><code>F2</code></td><td>编辑当前节点文字</td></tr>"
                + "<tr><td><code>Space</code></td><td>折叠 / 展开当前节点</td></tr>"
                + "<tr><td><code>Ctrl+S</code></td><td>保存脑图</td></tr>"
                + "</table>"
                + "<p><b>鼠标操作：</b>双击节点编辑 · 滚轮缩放 · 拖拽空白区平移</p>"
                + "<h2>编辑器特性</h2>"
                + "<ul>"
                + "<li><b>三种布局</b>：右树 / 对称 / 组织架构</li>"
                + "<li><b>五种主题</b>：默认 / 紫色 / 简约 / 清新 / 暖色</li>"
                + "<li><b>三种节点形状</b>：圆角 / 矩形 / 胶囊</li>"
                + "<li><b>导出格式</b>：PNG（位图）、SVG（矢量图）</li>"
                + "</ul>"
                + "<h2>目前限制</h2>"
                + "<ul>"
                + "<li>脑图数据以隐藏 Meta 存储，不污染页面可见内容，但不会出现在文字搜索中</li>"
                + "<li>切换布局会清除手动调整的节点位置</li>"
                + "<li>撤销 / 重做功能尚未实现</li>"
                + "</ul>"
                + "<h2>故障排查</h2>"
                + "<p>日志文件：</p>"
                + "<p><code>%LOCALAPPDATA%\\OneNoteMindMap\\log.txt</code></p>";
        }

        private static string GetQuickStartZh()
        {
            return "<h1>快速入门</h1>"
                + "<h2>第一步：确认 OneNote 位数</h2>"
                + "<p>打开 OneNote → <code>文件</code> → <code>帐户</code> → 点击 <code>关于 OneNote</code>，"
                + "查看是 32 位还是 64 位。</p>"
                + "<p><b>重要：</b>安装包必须匹配 <b>OneNote 的位数</b>，不是 Windows 的位数。</p>"
                + "<h2>第二步：安装插件</h2>"
                + "<ol>"
                + "<li>32 位 OneNote → 运行 <code>OneNoteMindMapSetup-x86.exe</code></li>"
                + "<li>64 位 OneNote → 运行 <code>OneNoteMindMapSetup-x64.exe</code></li>"
                + "<li>安装完成后重启 OneNote</li>"
                + "<li>功能区出现 \"脑图\" 选项卡</li>"
                + "</ol>"
                + "<h2>第三步：开始使用</h2>"
                + "<ol>"
                + "<li>在 OneNote 中打开或新建一个页面</li>"
                + "<li>点击 \"脑图\" 选项卡</li>"
                + "<li>点击 \"新建脑图\" 创建空白脑图，或点击 \"从页面生成\" 转换当前页面大纲</li>"
                + "<li>在编辑器中调整节点后按 <code>Ctrl+S</code> 保存</li>"
                + "</ol>"
                + "<h2>卸载</h2>"
                + "<p>通过 Windows \"设置 → 应用\" 卸载 \"OneNote 脑图\" 即可。</p>";
        }

        private static string GetFeaturesZh()
        {
            return "<h1>功能说明</h1>"
                + "<p>OneNote 脑图提供以下功能，请从左侧目录选择查看详情：</p>"
                + "<ul>"
                + "<li><b>新建脑图</b> —— 创建新页面并打开空白脑图编辑器</li>"
                + "<li><b>从页面生成</b> —— 解析当前页面大纲为脑图</li>"
                + "<li><b>编辑与保存</b> —— WPF 编辑器支持拖拽、增删、重命名节点</li>"
                + "<li><b>插入预览</b> —— 渲染 PNG 图片插入页面</li>"
                + "<li><b>导出 PNG / SVG</b> —— 导出为图片文件</li>"
                + "<li><b>转为大纲</b> —— 脑图回写为 OneNote 大纲</li>"
                + "</ul>";
        }

        private static string GetNewZh()
        {
            return "<h1>新建脑图</h1>"
                + "<p>点击 \"新建脑图\" 会在当前分区创建一个新页面，并自动打开脑图编辑器。</p>"
                + "<h2>操作步骤</h2>"
                + "<ol>"
                + "<li>切换到目标分区</li>"
                + "<li>点击功能区 \"新建脑图\"</li>"
                + "<li>编辑器打开，中心主题默认为 \"未命名脑图\"</li>"
                + "<li>双击节点编辑文字，用 Enter / Tab 扩展节点</li>"
                + "<li>按 <code>Ctrl+S</code> 保存</li>"
                + "</ol>";
        }

        private static string GetGenerateZh()
        {
            return "<h1>从页面生成</h1>"
                + "<p>读取当前 OneNote 页面的缩进层级和大纲结构，自动生成对应的思维导图。</p>"
                + "<h2>操作步骤</h2>"
                + "<ol>"
                + "<li>打开包含大纲内容的 OneNote 页面</li>"
                + "<li>点击功能区 \"从页面生成\"</li>"
                + "<li>插件解析页面结构并打开编辑器</li>"
                + "</ol>"
                + "<h2>建议</h2>"
                + "<ul>"
                + "<li>使用标题、项目符号或清晰的缩进组织内容，以获得更准确的层级</li>"
                + "<li>生成后仍可在编辑器中自由调整</li>"
                + "<li>脑图数据以隐藏 Meta 存储，不污染页面可见内容</li>"
                + "</ul>";
        }

        private static string GetEditZh()
        {
            return "<h1>编辑与保存</h1>"
                + "<p>点击 \"编辑脑图\" 可打开当前页面中已保存的脑图。</p>"
                + "<h2>编辑器功能</h2>"
                + "<ul>"
                + "<li>添加、删除、重命名节点</li>"
                + "<li>切换布局：右树 / 对称 / 组织架构</li>"
                + "<li>切换主题：默认 / 紫色 / 简约 / 清新 / 暖色</li>"
                + "<li>切换节点形状：圆角 / 矩形 / 胶囊</li>"
                + "<li>鼠标滚轮缩放，拖拽平移画布</li>"
                + "</ul>"
                + "<h2>保存</h2>"
                + "<p>按 <code>Ctrl+S</code> 保存。脑图数据保存在页面隐藏 Meta 中，不会替换或污染页面可见文字。</p>";
        }

        private static string GetPreviewZh()
        {
            return "<h1>插入预览</h1>"
                + "<p>将当前脑图渲染为 PNG 图片并插入 OneNote 页面。</p>"
                + "<h2>使用场景</h2>"
                + "<ul>"
                + "<li>分享、打印或让页面读者快速浏览脑图</li>"
                + "<li>脑图的可编辑数据仍保留在页面中</li>"
                + "</ul>"
                + "<h2>操作步骤</h2>"
                + "<ol>"
                + "<li>确保当前页面已有脑图数据</li>"
                + "<li>点击功能区 \"插入预览图\"</li>"
                + "<li>PNG 图片自动插入到页面中</li>"
                + "</ol>";
        }

        private static string GetExportZh()
        {
            return "<h1>导出 PNG / SVG</h1>"
                + "<h2>导出 PNG</h2>"
                + "<p>生成位图，适合演示文稿、文档和即时分享。</p>"
                + "<h2>导出 SVG</h2>"
                + "<p>生成矢量图，放大后依然清晰，适合进一步编辑或印刷使用。</p>"
                + "<h2>操作步骤</h2>"
                + "<ol>"
                + "<li>确保当前页面已有脑图数据</li>"
                + "<li>点击 \"导出 PNG\" 或 \"导出 SVG\"</li>"
                + "<li>选择保存位置</li>"
                + "</ol>"
                + "<p><b>提示：</b>导出前请先保存最新修改。</p>";
        }

        private static string GetOutlineZh()
        {
            return "<h1>转为大纲</h1>"
                + "<p>将当前脑图按层级写回 OneNote 页面，生成便于继续记录和编辑的文本大纲。</p>"
                + "<h2>操作步骤</h2>"
                + "<ol>"
                + "<li>确保当前页面已有脑图数据</li>"
                + "<li>点击功能区 \"脑图转大纲\"</li>"
                + "<li>页面中生成层级化的文本大纲</li>"
                + "</ol>"
                + "<p><b>注意：</b>此操作不会删除已保存的脑图数据。</p>";
        }

        private static string GetShortcutsZh()
        {
            return "<h1>编辑器快捷键</h1>"
                + "<table>"
                + "<tr><th>快捷键</th><th>功能</th></tr>"
                + "<tr><td><code>Enter</code></td><td>添加同级节点</td></tr>"
                + "<tr><td><code>Tab</code></td><td>添加子节点</td></tr>"
                + "<tr><td><code>Delete</code></td><td>删除当前节点及其子节点</td></tr>"
                + "<tr><td><code>F2</code></td><td>编辑当前节点文字</td></tr>"
                + "<tr><td><code>Space</code></td><td>折叠 / 展开当前节点</td></tr>"
                + "<tr><td><code>Ctrl+S</code></td><td>保存脑图</td></tr>"
                + "</table>"
                + "<h2>鼠标操作</h2>"
                + "<ul>"
                + "<li>双击节点 —— 进入编辑模式</li>"
                + "<li>滚轮 —— 缩放画布</li>"
                + "<li>拖拽空白区域 —— 平移画布</li>"
                + "</ul>";
        }

        private static string GetFaqZh()
        {
            return "<h1>常见问题</h1>"
                + "<h2>Q: 安装后 OneNote 中没有出现 \"脑图\" 选项卡</h2>"
                + "<p><b>A:</b></p>"
                + "<ol>"
                + "<li>确认安装包位数与 OneNote 位数一致</li>"
                + "<li>安装后必须重启 OneNote</li>"
                + "<li>检查 OneNote 是否禁用了插件：文件 → 选项 → 加载项</li>"
                + "</ol>"
                + "<h2>Q: \"编辑脑图\" 提示找不到数据</h2>"
                + "<p><b>A:</b> 请先在当前页面使用 \"新建脑图\" 或 \"从页面生成\" 创建并保存脑图。</p>"
                + "<h2>Q: 生成结果层级不正确</h2>"
                + "<p><b>A:</b> 请检查页面是否使用一致的缩进层级，再重新生成。</p>"
                + "<h2>Q: 脑图数据保存在哪里？</h2>"
                + "<p><b>A:</b> 数据以隐藏 Meta 标记保存在对应 OneNote 页面中，不影响可见内容。</p>"
                + "<h2>日志文件</h2>"
                + "<p><code>%LOCALAPPDATA%\\OneNoteMindMap\\log.txt</code></p>";
        }

        // ═══════════════════════════════════════════════════════════
        // ENGLISH
        // ═══════════════════════════════════════════════════════════

        private static string GetAboutEn()
        {
            return "<h1>OneNote Mind Map — Help</h1>"
                + "<p class='version'>Version: 1.3.0</p>"
                + "<p>This plugin is developed by a OneNote MVP.</p>"
                + "<h2>Button Reference</h2>"
                + "<table>"
                + "<tr><th>Button</th><th>Action</th></tr>"
                + "<tr><td><b>New</b></td><td>Create a new blank mind map</td></tr>"
                + "<tr><td><b>Generate</b></td><td>Parse current page content into a mind map</td></tr>"
                + "<tr><td><b>Edit</b></td><td>Open the WPF editor for the current mind map</td></tr>"
                + "<tr><td><b>Outline</b></td><td>Convert the mind map back into OneNote outline text</td></tr>"
                + "<tr><td><b>Insert Preview</b></td><td>Insert/update a PNG preview image of the mind map</td></tr>"
                + "<tr><td><b>Export PNG</b></td><td>Export the mind map as a PNG file</td></tr>"
                + "<tr><td><b>Export SVG</b></td><td>Export the mind map as an SVG file</td></tr>"
                + "<tr><td><b>Help</b></td><td>Show usage instructions</td></tr>"
                + "</table>"
                + "<h2>Shortcuts</h2>"
                + "<table>"
                + "<tr><th>Key</th><th>Action</th></tr>"
                + "<tr><td><code>Enter</code></td><td>Add sibling node</td></tr>"
                + "<tr><td><code>Tab</code></td><td>Add child node</td></tr>"
                + "<tr><td><code>Delete</code></td><td>Delete node and children</td></tr>"
                + "<tr><td><code>F2</code></td><td>Edit node text</td></tr>"
                + "<tr><td><code>Space</code></td><td>Collapse / Expand</td></tr>"
                + "<tr><td><code>Ctrl+S</code></td><td>Save mind map</td></tr>"
                + "</table>"
                + "<p><b>Mouse:</b> Double-click to edit · Scroll to zoom · Drag to pan</p>"
                + "<h2>Editor Features</h2>"
                + "<ul>"
                + "<li><b>Three layouts</b>: RightTree / BothSides / OrgChart</li>"
                + "<li><b>Five themes</b>: Default / Purple / Minimal / Fresh / Warm</li>"
                + "<li><b>Three node shapes</b>: Rounded / Rectangle / Pill</li>"
                + "<li><b>Export formats</b>: PNG (bitmap), SVG (vector)</li>"
                + "</ul>"
                + "<h2>Current Limitations</h2>"
                + "<ul>"
                + "<li>Mind map data is stored as hidden Meta — does not pollute visible content, but won't appear in text search</li>"
                + "<li>Switching layouts resets manual node positioning</li>"
                + "<li>Undo / Redo not yet implemented</li>"
                + "</ul>"
                + "<h2>Troubleshooting</h2>"
                + "<p>Log file:</p>"
                + "<p><code>%LOCALAPPDATA%\\OneNoteMindMap\\log.txt</code></p>";
        }

        private static string GetQuickStartEn()
        {
            return "<h1>Quick Start</h1>"
                + "<h2>Step 1: Check OneNote Architecture</h2>"
                + "<p>Open OneNote → <code>File</code> → <code>Account</code> → <code>About OneNote</code> to check 32-bit or 64-bit.</p>"
                + "<p><b>Important:</b> The installer must match <b>OneNote's architecture</b>, not Windows.</p>"
                + "<h2>Step 2: Install</h2>"
                + "<ol>"
                + "<li>32-bit OneNote → Run <code>OneNoteMindMapSetup-x86.exe</code></li>"
                + "<li>64-bit OneNote → Run <code>OneNoteMindMapSetup-x64.exe</code></li>"
                + "<li>Restart OneNote after installation</li>"
                + "<li>A \"Mind Map\" tab appears in the Ribbon</li>"
                + "</ol>"
                + "<h2>Step 3: Start Using</h2>"
                + "<ol>"
                + "<li>Open or create a OneNote page</li>"
                + "<li>Click the \"Mind Map\" tab</li>"
                + "<li>Click \"New Mind Map\" or \"From Page\" to generate</li>"
                + "<li>Edit nodes and press <code>Ctrl+S</code> to save</li>"
                + "</ol>";
        }

        private static string GetFeaturesEn()
        {
            return "<h1>Features</h1>"
                + "<p>Select a topic from the left panel for details:</p>"
                + "<ul>"
                + "<li><b>New Mind Map</b> — Create a new page with a blank mind map</li>"
                + "<li><b>From Page</b> — Generate mind map from page outline</li>"
                + "<li><b>Edit & Save</b> — WPF editor with drag, add, delete, rename</li>"
                + "<li><b>Insert Preview</b> — Render PNG image into the page</li>"
                + "<li><b>Export PNG / SVG</b> — Export as image files</li>"
                + "<li><b>To Outline</b> — Convert mind map back to OneNote outline</li>"
                + "</ul>";
        }

        private static string GetNewEn()
        {
            return "<h1>New Mind Map</h1>"
                + "<p>Creates a new page in the current section and opens the mind map editor.</p>"
                + "<h2>Steps</h2>"
                + "<ol>"
                + "<li>Switch to the target section</li>"
                + "<li>Click \"New Mind Map\" in the Ribbon</li>"
                + "<li>The editor opens with a default central topic</li>"
                + "<li>Double-click nodes to edit, use Enter/Tab to expand</li>"
                + "<li>Press <code>Ctrl+S</code> to save</li>"
                + "</ol>";
        }

        private static string GetGenerateEn()
        {
            return "<h1>Generate from Page</h1>"
                + "<p>Reads the current page's indentation hierarchy and generates a mind map.</p>"
                + "<h2>Steps</h2>"
                + "<ol>"
                + "<li>Open a page with outline content</li>"
                + "<li>Click \"From Page\" in the Ribbon</li>"
                + "<li>The plugin parses the structure and opens the editor</li>"
                + "</ol>"
                + "<h2>Tips</h2>"
                + "<ul>"
                + "<li>Use headings, bullets, or clear indentation for better hierarchy</li>"
                + "<li>You can freely adjust the result in the editor</li>"
                + "<li>Data is stored as hidden Meta — visible content is not affected</li>"
                + "</ul>";
        }

        private static string GetEditEn()
        {
            return "<h1>Edit & Save</h1>"
                + "<p>Click \"Edit Map\" to open the saved mind map for the current page.</p>"
                + "<h2>Editor Features</h2>"
                + "<ul>"
                + "<li>Add, delete, rename nodes</li>"
                + "<li>Layouts: Right Tree / Both Sides / Org Chart</li>"
                + "<li>Themes: Default / Purple / Minimal / Fresh / Warm</li>"
                + "<li>Node shapes: Rounded / Rectangle / Pill</li>"
                + "<li>Mouse wheel to zoom, drag to pan</li>"
                + "</ul>"
                + "<h2>Saving</h2>"
                + "<p>Press <code>Ctrl+S</code> to save. Data is stored in hidden page Meta.</p>";
        }

        private static string GetPreviewEn()
        {
            return "<h1>Insert Preview</h1>"
                + "<p>Renders the mind map as a PNG image and inserts it into the OneNote page.</p>"
                + "<h2>Use Cases</h2>"
                + "<ul>"
                + "<li>Sharing, printing, or quick visual reference</li>"
                + "<li>Editable mind map data is preserved</li>"
                + "</ul>";
        }

        private static string GetExportEn()
        {
            return "<h1>Export PNG / SVG</h1>"
                + "<h2>PNG</h2>"
                + "<p>Bitmap image — ideal for presentations and sharing.</p>"
                + "<h2>SVG</h2>"
                + "<p>Vector image — stays sharp at any zoom, suitable for printing.</p>"
                + "<p><b>Tip:</b> Save your latest edits before exporting.</p>";
        }

        private static string GetOutlineEn()
        {
            return "<h1>To Outline</h1>"
                + "<p>Writes the mind map hierarchy back to the OneNote page as a text outline.</p>"
                + "<p><b>Note:</b> This does not delete the saved mind map data.</p>";
        }

        private static string GetShortcutsEn()
        {
            return "<h1>Editor Shortcuts</h1>"
                + "<table>"
                + "<tr><th>Shortcut</th><th>Function</th></tr>"
                + "<tr><td><code>Enter</code></td><td>Add sibling node</td></tr>"
                + "<tr><td><code>Tab</code></td><td>Add child node</td></tr>"
                + "<tr><td><code>Delete</code></td><td>Delete node and children</td></tr>"
                + "<tr><td><code>F2</code></td><td>Edit node text</td></tr>"
                + "<tr><td><code>Space</code></td><td>Collapse / Expand</td></tr>"
                + "<tr><td><code>Ctrl+S</code></td><td>Save mind map</td></tr>"
                + "</table>"
                + "<h2>Mouse</h2>"
                + "<ul>"
                + "<li>Double-click node — Enter edit mode</li>"
                + "<li>Scroll wheel — Zoom canvas</li>"
                + "<li>Drag empty area — Pan canvas</li>"
                + "</ul>";
        }

        private static string GetFaqEn()
        {
            return "<h1>FAQ</h1>"
                + "<h2>Q: \"Mind Map\" tab doesn't appear after installation</h2>"
                + "<p><b>A:</b></p>"
                + "<ol>"
                + "<li>Ensure installer architecture matches OneNote (32-bit vs 64-bit)</li>"
                + "<li>Restart OneNote after installation</li>"
                + "<li>Check if OneNote disabled the plugin: File → Options → Add-ins</li>"
                + "</ol>"
                + "<h2>Q: \"Edit Map\" says no data found</h2>"
                + "<p><b>A:</b> First create a mind map using \"New Mind Map\" or \"From Page\".</p>"
                + "<h2>Q: Generated hierarchy is incorrect</h2>"
                + "<p><b>A:</b> Ensure consistent indentation on the page, then regenerate.</p>"
                + "<h2>Q: Where is mind map data stored?</h2>"
                + "<p><b>A:</b> In hidden Meta tags within the OneNote page. Visible content is not affected.</p>"
                + "<h2>Log File</h2>"
                + "<p><code>%LOCALAPPDATA%\\OneNoteMindMap\\log.txt</code></p>";
        }
    }
}
