using System;
using OneNoteMindMap.Logging;
using OneNoteMindMap.OneNote;
using OneNoteMindMap.UI;

namespace OneNoteMindMap.Features
{
    public static class EditCurrentMindMapCommand
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

                if (!MindMapPageStore.HasMindMapData(pageId))
                {
                    Msg.Warn(Strings.NoMapDataFound);
                    return;
                }

                EditorLauncher.OpenEditor(pageId);
            }
            catch (Exception ex)
            {
                Logger.Error("EditCurrentMindMapCommand failed", ex);
                Msg.Error(Strings.If("打开编辑器失败：" + ex.Message, "Failed to open editor: " + ex.Message));
            }
        }
    }
}
