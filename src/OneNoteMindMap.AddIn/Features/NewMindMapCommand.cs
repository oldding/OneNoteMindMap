using System;
using OneNoteMindMap.Core.Model;
using OneNoteMindMap.Logging;
using OneNoteMindMap.OneNote;
using OneNoteMindMap.UI;

namespace OneNoteMindMap.Features
{
    public static class NewMindMapCommand
    {
        public static void Execute()
        {
            try
            {
                string sectionId = OneNoteProvider.GetCurrentSectionId();
                if (string.IsNullOrEmpty(sectionId))
                {
                    Msg.Warn(Strings.If("无法获取当前分区", "Cannot get current section"));
                    return;
                }

                string pageId = OneNoteProvider.CreateNewPage(sectionId);
                if (string.IsNullOrEmpty(pageId))
                {
                    Msg.Warn(Strings.If("创建页面失败", "Failed to create page"));
                    return;
                }

                string title = Strings.If("未命名脑图", "Untitled Mind Map");

                var doc = new MindMapDocument
                {
                    Title = title,
                    Root = new MindMapNode { Text = title },
                    Source = new MindMapSourceInfo
                    {
                        OneNotePageId = pageId,
                        OneNotePageTitle = title,
                        SourceType = "CurrentPage"
                    }
                };

                doc.Root.Children.Add(new MindMapNode { Text = Strings.If("双击编辑", "Double-click to edit") });

                if (!MindMapPageStore.SaveMindMapToPage(pageId, doc))
                {
                    Msg.Warn(Strings.If("脑图数据保存失败", "Failed to save mind map data"));
                    return;
                }
                OneNoteProvider.SetPageTitle(pageId, Strings.If("脑图 - ", "Mind Map - ") + title);
                OneNoteProvider.NavigateTo(pageId);

                Logger.Info("New mind map page created: " + pageId);

                // Open editor
                EditorLauncher.OpenEditor(pageId);
            }
            catch (Exception ex)
            {
                Logger.Error("NewMindMapCommand failed", ex);
                Msg.Error(Strings.If("新建脑图失败：" + ex.Message, "Failed to create mind map: " + ex.Message));
            }
        }
    }
}
