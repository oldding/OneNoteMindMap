# Maintenance notes

Before editing COM lifecycle, Ribbon references or UI dispatching, read
`docs/2026-09-13-shutdown-fix.md`.

- Release the stored IRibbonUI as well as Application during shutdown.
- Start stopping at OnBeginShutdown, not only OnDisconnection.
- Never synchronously wait for a second STA from a host lifecycle callback.
- Do not release the Application RCW until executing UI/COM operations unwind.
- Late Post/Invoke calls must not restart a stopped dispatcher.
- Borrow the connection's Application, and release temporary Windows/Window RCWs.
- Preserve open editor documents locally if host shutdown prevents COM saving.
- Test repeated real OneNote open/close cycles with Wechat and Search enabled.
- Do not kill all dllhost processes or clear OneNote caches as a blanket fix.

Current fix: file version 1.5.2.0; assembly identity 1.5.1.0.
Lifecycle regression: `tools/LifecycleTests.cs`.
