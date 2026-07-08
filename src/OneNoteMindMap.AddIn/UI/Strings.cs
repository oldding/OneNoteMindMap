using System;
using System.Collections.Generic;

namespace OneNoteMindMap.UI
{
    public static class Strings
    {
        /// <summary>
        /// Detects if the OneNote UI language is Chinese.
        /// Uses the current UI culture of the thread.
        /// </summary>
        public static bool IsChinese =>
            System.Threading.Thread.CurrentThread.CurrentUICulture.TwoLetterISOLanguageName == "zh" ||
            System.Threading.Thread.CurrentThread.CurrentUICulture.Name.StartsWith("zh");

        public static string RibbonLabel(string id) => id switch
        {
            "tabMindMap" => If("脑图", "Mind Map"),
            "groupCreate" => If("创建", "Create"),
            "groupEdit" => If("编辑", "Edit"),
            "groupExport" => If("导出", "Export"),
            "groupAdvanced" => If("高级", "Advanced"),
            "btnNewMap" => If("新建脑图", "New Mind Map"),
            "btnGenerateFromPage" => If("从页面生成", "From Page"),
            "btnEditCurrentMap" => If("编辑当前脑图", "Edit Map"),
            "btnInsertPreview" => If("插入预览图", "Insert Preview"),
            "btnExportPng" => If("导出 PNG", "Export PNG"),
            "btnExportSvg" => If("导出 SVG", "Export SVG"),
            "btnMindMapToOutline" => If("脑图转大纲", "To Outline"),
            "btnHelp" => If("帮助", "Help"),
            _ => id
        };

        public static string RibbonScreentip(string id) => RibbonLabel(id);

        public static string RibbonSupertip(string id) => id switch
        {
            "btnNewMap" => If("在新建页面中创建一个新的思维导图", "Create a new mind map in a new page"),
            "btnGenerateFromPage" => If("从当前页面的大纲结构自动生成思维导图", "Auto-generate a mind map from the page outline structure"),
            "btnEditCurrentMap" => If("打开当前页面中已保存的思维导图进行编辑", "Open the saved mind map for editing"),
            "btnInsertPreview" => If("将当前编辑的思维导图预览图片插入到页面", "Insert the mind map preview image into the page"),
            "btnExportPng" => If("将思维导图导出为PNG图片文件", "Export the mind map as a PNG image file"),
            "btnExportSvg" => If("将思维导图导出为SVG矢量图片文件", "Export the mind map as an SVG vector image file"),
            "btnMindMapToOutline" => If("将当前脑图内容回写到页面中，生成文档大纲", "Write the mind map content back to the page as an outline"),
            "btnHelp" => If("查看帮助信息", "View help information"),
            _ => ""
        };

        public static string If(string zh, string en) => IsChinese ? zh : en;

        public static string NewPageTitle => If("脑图 - ", "Mind Map - ");
        public static string SaveSuccess => If("保存成功", "Saved successfully");
        public static string SaveFailed => If("保存失败", "Save failed");
        public static string ConfirmDeleteTitle => If("确认删除", "Confirm Delete");
        public static string ConfirmDeleteNode => If("确定要删除这个节点及其子节点？", "Delete this node and its children?");
        public static string NoMapDataFound => If("当前页面中未找到脑图数据", "No mind map data found on this page");
        public static string GenerateSuccess => If("脑图生成成功", "Mind map generated successfully");
        public static string InsertPreviewSuccess => If("预览图已插入页面", "Preview image inserted into page");
        public static string ExportSuccess => If("导出成功", "Export successful");
        public static string MapSavedToPage => If("脑图已保存到页面", "Mind map saved to page");
        public static string HelpTitle => If("帮助", "Help");
        public static string FileDialogFilter => If("PNG 图片|*.png", "PNG Image|*.png");
        public static string SvgFileDialogFilter => If("SVG 文件|*.svg", "SVG File|*.svg");
    }
}
