using System;
using OneNoteMindMap.Core.Model;
using OneNoteMindMap.Core.Parsing;
using OneNoteMindMap.Logging;
using OneNoteMindMap.OneNote;
using OneNoteMindMap.UI;
using OneNoteMindMap.Core.Serialization;

namespace OneNoteMindMap.Features
{
    public static class GenerateFromPageCommand
    {
        public static void Execute()
        {
            try
            {
                string pageId = OneNoteProvider.GetCurrentPageId();
                if (string.IsNullOrEmpty(pageId))
                {
                    Msg.Warn(Strings.If("无法获取当前页面", "Cannot get current page"));
                    return;
                }

                string pageXml = OneNoteProvider.GetPageContent(pageId, out string pageTitle);
                if (string.IsNullOrEmpty(pageXml))
                {
                    Msg.Warn(Strings.If("无法读取页面内容", "Cannot read page content"));
                    return;
                }

                var doc = OutlineToMindMapBuilder.BuildFromPageXml(pageXml, pageId, pageTitle);
                if (doc == null || doc.Root == null)
                {
                    Msg.Warn(Strings.If("页面中未找到有效的大纲结构", "No valid outline structure found in page"));
                    return;
                }

                if (!MindMapPageStore.SaveMindMapToPage(pageId, doc))
                {
                    Msg.Warn(Strings.If("脑图数据保存失败", "Failed to save mind map data"));
                    return;
                }

                Logger.Info("Mind map generated from page: " + pageId);
                Msg.Info(Strings.GenerateSuccess);

                EditorLauncher.OpenEditor(pageId);
            }
            catch (Exception ex)
            {
                Logger.Error("GenerateFromPageCommand failed", ex);
                Msg.Error(Strings.If("生成脑图失败：" + ex.Message, "Failed to generate mind map: " + ex.Message));
            }
        }
    }
}
