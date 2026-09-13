# 1.5.2：关闭 OneNote 后进程残留修复

## 背景

2026-09-13，在微信收集插件项目排查中，旧脑图插件单独启用也会导致
ONENOTE.EXE 在关闭窗口后残留，下次打开出现“无法顺利启动”。
前次对照见 `D:\OpenCode\OneNote wechat syn\docs\2026-09-13-startup-shutdown-fix.md`。

本次任务开始时本机脑图安装目录与注册项已不存在，因此从当前 1.5.1 源码修复并重新安装 1.5.2。
保留用户已有的未跟踪 ZIP 文件，没有提交 Git 或发布远程 Release。

## 发现的生命周期缺陷

- `RibbonState.RibbonUI` 静态保存的 OneNote COM 引用没有任何退出释放路径。
  即使 Application 已释放，这个引用仍可能阻止 OneNote COM 服务器退出。
- OnBeginShutdown 为空，停止消息泵只发生在 OnDisconnection。
- UI 启动同步等待另一 STA，关闭后 Post/Invoke 又可能尝试启动消息泵。
- OneNoteProvider 优先 GetActiveObject，每次从 ROT 获取 Application，且 Windows /
  CurrentWindow 临时 RCW 未配对释放。

本次同时修复以上缺陷；真实测试证明组合修复有效，未通过逐行回退实验断言唯一根因。

## 修复内容

- OnBeginShutdown / OnDisconnection 共用幂等停止路径，先停止对外提供 Application。
- 释放 Ribbon 引用并置空；等待正在执行的 UI/COM 操作返回后，才释放 Application。
- WPF Dispatcher 使用独立会话、异步启动、可取消任务投递和异步退出；宿主回调中不 Wait/Join。
- 迟到命令取消，不重新启动已关闭的消息泵。
- Provider 借用连接持有的 Application；CurrentWindow、Windows 通过 finally 配对释放。
- 跟踪本插件的编辑器和帮助窗口，宿主关闭时停止这些窗口。
- 若宿主在编辑器仍打开时退出，文档保存到本地 recovery JSON 后关闭，
  不在 OneNote 退出期间再调 COM 保存。恢复目录：
  `C:\Users\dinghuqiang\AppData\Local\OneNoteMindMap\recovery`。
  普通编辑器“保存并关闭”流程不变。恢复文件目前保留供手工恢复，未添加自动恢复 UI。
- 添加带 PID、文件版本、COM 释放及 dispatcher 退出阶段的日志。
- AddIn/Editor 文件版本 1.5.2.0；AssemblyVersion 保持 1.5.1.0。
- 安装器版本 1.5.2，注册使用 HKCR，由安装架构选择视图，保留 RegAsm /codebase。

## 验证

1. Release 构建通过，无编译警告。
2. `tools/LifecycleTests.cs` 通过：五次连接/退出、执行中停止、队列取消、
   模态 WPF 窗口关闭后再释放 COM、十次启动/退出竞争。
3. 构建并安装 x64 安装器，退出码 0；注册 CodeBase 指向
   `C:\Program Files\OneNoteMindMap\OneNoteMindMap.AddIn.DLL`。
4. **脑图 1.5.2 + 微信收集 1.0.2 + 搜索**全部启用，连续三次正常开关，
   ONENOTE.EXE 每次均退出。
5. 保持 OneNote 打开，用户发公众号链接，微信自动检查成功采集一篇。
6. 用户确认脑图编辑保存、微信“立即同步”均正常；再次正常关闭 OneNote，
   进程退出成功，重开正常（最后留在“产品发布计划”页面）。

最终三个插件 LoadBehavior 均为 3，脑图不再暂时停用。
未批量结束 dllhost、未清缓存、未修复 Office。

## 安装包与日志

本机验证用 x64 安装包：
`D:\OpenCode\OneNoteMindMap\src\OneNoteMindMap.Installer\Output\OneNoteMindMapSetup-1.5.2-x64.exe`

日志：`C:\Users\dinghuqiang\AppData\Local\OneNoteMindMap\log.txt`

此轮没有构建/验证 x86 安装包，也没有完整回归脑图所有布局、导出功能。
日后涉及 UI/COM 生命周期的修改，必须重复真实开关测试并在多插件环境隔离验证。
