using System;
using System.Threading;
using OneNoteMindMap.Logging;
using OneNoteMindMap.OneNote;
using OneNoteMindMap.UI;

namespace OneNoteMindMap.Features
{
    public static class EditorLauncher
    {
        public static void OpenEditor(string pageId)
        {
            try
            {
                var doc = LoadMindMapWithRetry(pageId);
                if (doc == null)
                {
                    Msg.Warn(Strings.NoMapDataFound);
                    return;
                }

                UiThread.Invoke(() =>
                {
                    try
                    {
                        var editor = new OneNoteMindMap.Editor.MindMapEditorWindow(doc, pageId);
                        editor.ShowDialog();

                        if (editor.IsSaved)
                        {
                            if (MindMapPageStore.SaveMindMapToPage(pageId, editor.Document))
                            {
                                InsertPreviewCommand.InsertOrUpdatePreview(pageId, editor.Document);
                                Logger.Info("Mind map saved from editor: " + pageId);
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Logger.Error("Editor window failed", ex);
                        Msg.Error(Strings.If("打开编辑器失败：" + ex.Message,
                            "Failed to open editor: " + ex.Message));
                    }
                });
            }
            catch (Exception ex)
            {
                Logger.Error("OpenEditor failed", ex);
            }
        }

        private static OneNoteMindMap.Core.Model.MindMapDocument LoadMindMapWithRetry(string pageId)
        {
            for (int i = 0; i < 5; i++)
            {
                var doc = MindMapPageStore.LoadMindMapFromPage(pageId);
                if (doc != null)
                    return doc;

                Thread.Sleep(150);
            }

            return null;
        }
    }
}
