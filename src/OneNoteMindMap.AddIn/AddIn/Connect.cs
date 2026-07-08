using System;
using System.Drawing;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using Extensibility;
using Microsoft.Office.Core;
using OneNoteMindMap.Features;
using OneNoteMindMap.Logging;
using OneNoteMindMap.UI;
using OneNoteMindMap.OneNote;

namespace OneNoteMindMap.AddIn
{
    [ComVisible(true)]
    [Guid("A4CEC0EF-4C6C-4CBD-9112-B83545EEADE8")]
    [ProgId("OneNoteMindMap.Connect")]
    public class Connect : IDTExtensibility2, IRibbonExtensibility
    {
        private const string EmptyRibbonXml = "<customUI xmlns=\"http://schemas.microsoft.com/office/2006/01/customui\"></customUI>";
        private const string RibbonResourceName = "OneNoteMindMap.Ribbon.CustomUI.xml";

        private static Connect _instance;
        private static string _addInDirectory;
        private object _oneNoteApp;

        static Connect()
        {
            _addInDirectory = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            AppDomain.CurrentDomain.AssemblyResolve += OnAssemblyResolve;
        }

        public Connect()
        {
            _instance = this;
        }

        public static Connect Instance => _instance;
        public object OneNoteApp => _oneNoteApp;

        public string InstalledPath => _addInDirectory;

        private static Assembly OnAssemblyResolve(object sender, ResolveEventArgs args)
        {
            try
            {
                var name = new AssemblyName(args.Name);
                var path = Path.Combine(_addInDirectory, name.Name + ".dll");
                if (File.Exists(path))
                    return Assembly.LoadFrom(path);
            }
            catch { }
            return null;
        }

        public void OnConnection(object application, ext_ConnectMode connectMode, object addInInst, ref Array custom)
        {
            try
            {
                _instance = this;
                _oneNoteApp = application;
                Logger.Initialize();
                Logger.Info("OneNote脑图 connected (" + connectMode + ")");

                try
                {
                    Microsoft.Win32.Registry.SetValue(
                        @"HKEY_CURRENT_USER\Software\Microsoft\Office\OneNote\AddIns\OneNoteMindMap.Connect",
                        "LoadBehavior", 3, Microsoft.Win32.RegistryValueKind.DWord);
                }
                catch { }

                UiThread.EnsureStarted();
            }
            catch (Exception ex)
            {
                try { Logger.Error("OnConnection failed", ex); } catch { }
            }
        }

        public void OnStartupComplete(ref Array custom) { }
        public void OnAddInsUpdate(ref Array custom) { }
        public void OnBeginShutdown(ref Array custom) { }

        public void OnDisconnection(ext_DisconnectMode removeMode, ref Array custom)
        {
            try
            {
                UiThread.Shutdown();
            }
            catch { }
            finally
            {
                try
                {
                    if (_oneNoteApp != null && Marshal.IsComObject(_oneNoteApp))
                        Marshal.ReleaseComObject(_oneNoteApp);
                }
                catch { }

                _oneNoteApp = null;
                if (ReferenceEquals(_instance, this))
                    _instance = null;

                GC.Collect();
                GC.WaitForPendingFinalizers();
            }
        }

        public string GetCustomUI(string ribbonId)
        {
            try
            {
                var assembly = Assembly.GetExecutingAssembly();
                using (var stream = assembly.GetManifestResourceStream(RibbonResourceName))
                {
                    if (stream == null)
                    {
                        Logger.Warn("Ribbon resource not found: " + RibbonResourceName);
                        return EmptyRibbonXml;
                    }
                    using (var reader = new StreamReader(stream))
                        return reader.ReadToEnd();
                }
            }
            catch (Exception ex)
            {
                Logger.Error("GetCustomUI failed", ex);
                return EmptyRibbonXml;
            }
        }

        public void OnRibbonLoad(IRibbonUI ribbonUI)
        {
            RibbonState.RibbonUI = ribbonUI;
        }

        public string GetLabel(IRibbonControl control)
        {
            return Strings.RibbonLabel(control?.Id);
        }

        public string GetScreentip(IRibbonControl control)
        {
            return Strings.RibbonScreentip(control?.Id);
        }

        public string GetSupertip(IRibbonControl control)
        {
            return Strings.RibbonSupertip(control?.Id);
        }

        public System.Runtime.InteropServices.ComTypes.IStream GetImage(IRibbonControl control)
        {
            try
            {
                string iconName = MapControlToIcon(control?.Id);
                if (string.IsNullOrEmpty(iconName)) return null;

                var assembly = Assembly.GetExecutingAssembly();
                string resourceName = "OneNoteMindMap.Ribbon.Icons." + iconName + ".png";
                using (var stream = assembly.GetManifestResourceStream(resourceName))
                {
                    if (stream == null) return null;
                    byte[] bytes = new byte[stream.Length];
                    stream.Read(bytes, 0, bytes.Length);
                    return SHCreateMemStream(bytes, (uint)bytes.Length);
                }
            }
            catch
            {
                return null;
            }
        }

        [DllImport("shlwapi.dll")]
        private static extern System.Runtime.InteropServices.ComTypes.IStream SHCreateMemStream(
            [MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 1)] byte[] pInit, uint cbInit);

        private static string MapControlToIcon(string controlId)
        {
            return controlId switch
            {
                "btnNewMap" => "New",
                "btnGenerateFromPage" => "Generate",
                "btnEditCurrentMap" => "Edit",
                "btnInsertPreview" => "Insert",
                "btnExportPng" => "Export",
                "btnExportSvg" => "Svg",
                "btnMindMapToOutline" => "Outline",
                "btnHelp" => "Help",
                _ => null
            };
        }

        public void OnNewMap(IRibbonControl c) => UiThread.Post(NewMindMapCommand.Execute);
        public void OnGenerateFromPage(IRibbonControl c) => UiThread.Post(GenerateFromPageCommand.Execute);
        public void OnEditCurrentMap(IRibbonControl c) => UiThread.Post(EditCurrentMindMapCommand.Execute);
        public void OnInsertPreview(IRibbonControl c) => UiThread.Post(InsertPreviewCommand.Execute);
        public void OnExportPng(IRibbonControl c) => UiThread.Post(ExportPngCommand.Execute);
        public void OnExportSvg(IRibbonControl c) => UiThread.Post(ExportSvgCommand.Execute);
        public void OnMindMapToOutline(IRibbonControl c) => UiThread.Post(MindMapToOutlineCommand.Execute);
        public void OnHelp(IRibbonControl c) => UiThread.Post(HelpCommand.Execute);
    }

    public static class HelpCommand
    {
        public static void Execute()
        {
            Msg.Info(
                "OneNote 脑图 v0.0.1\n\n" +
                "把 OneNote 页面一键生成思维导图，\n" +
                "也可编辑、保存、回写大纲。\n\n" +
                "快捷键：\n" +
                "  Enter  添加同级节点\n" +
                "  Tab    添加子节点\n" +
                "  Delete 删除节点\n" +
                "  F2     编辑节点\n" +
                "  Space  折叠/展开\n" +
                "  Ctrl+S 保存",
                "OneNote 脑图");
        }
    }
}
