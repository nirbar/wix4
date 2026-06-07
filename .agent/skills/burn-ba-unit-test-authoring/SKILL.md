# Skill: Authoring Burn BA Unit Tests

Use this skill when the user asks to write, extend, or fix tests in the
`WixToolsetTest.BurnUnitTest` project (or any project that references
`PanelSwWix4.Burn.UnitTest`).

---

## Framework overview

The test framework drives real burn bundles through a **pipe-based BA host**.
Burn launches and connects to this host instead of a real UI layer.
The host dispatches every BA message to a test class, which can inspect
arguments, call assertions, or override the response before forwarding to the
real BA (e.g. `wixstdba`).

`WixToolset.Burn.UnitTest` implements both `ITestDiscoverer` and `ITestExecutor`
from the Microsoft Test Platform — it **is** the test adapter.  No xUnit or
other test framework is needed.  `dotnet test` discovers and runs
`[BurnBATestClass]`-decorated classes directly.

Key assemblies:

| Assembly | Role |
|---|---|
| `WixToolset.Burn.UnitTest` | Framework, MTP adapter, base class, dispatcher, runner |
| `WixToolsetTest.BurnUnitTest` | Example test project |

---

## Setting up a test project

1. Add a reference to `PanelSwWix4.Burn.UnitTest` (NuGet) or
   `WixToolset.Burn.UnitTest` (project reference).
2. Add a `.runsettings` file pointing to your bundle:  
   ```xml
   <RunSettings>
     <RunConfiguration>
       <TestAdaptersPaths>.</TestAdaptersPaths>
     </RunConfiguration>
     <BurnBATestFramework>
       <BundlePath>$(OutputPath)MyBundle.exe</BundlePath>
       <Password></Password>
     </BurnBATestFramework>
   </RunSettings>
   ```
3. Select the runsettings in Visual Studio (`Test › Configure Run Settings`) or
   pass `--settings BurnBaTests.runsettings` to `dotnet test`.
4. Write `[BurnBATestClass]`-decorated classes — no `[Fact]`, no fixture class.

---

## Test class structure

Every test class:

1. Inherits `BurnBATestBase`
2. Is decorated with `[BurnBATestClass]`
3. Overrides only the lifecycle callbacks it cares about

```csharp
[BurnBATestClass(Order = 1)]
public sealed class SilentInstallTest : BurnBATestBase
{
    public override int OnCreate(IBootstrapperEngine pEngine, ref Command command)
    {
        // Modify the command before the real BA sees it.
        var cmd = new TestBaCommand(command);
        cmd.Action  = LaunchAction.Install;
        cmd.Display = Display.None;
        var modified = cmd.ToCommand();
        return base.OnCreate(pEngine, ref modified);
    }

    public override int OnApplyComplete(
        int hrStatus, ApplyRestart restart,
        BOOTSTRAPPER_APPLYCOMPLETE_ACTION recommendation,
        ref BOOTSTRAPPER_APPLYCOMPLETE_ACTION action)
    {
        if (hrStatus != 0)
        {
            this.AddException(new BurnBAAssertException(
                $"Apply failed: 0x{hrStatus:X8}"), endAutoPilot: false);
        }
        return base.OnApplyComplete(hrStatus, restart, recommendation, ref action);
    }
}
```

Default method behaviour (when not overridden): **forwards to the real BA and
returns its response**.  A class that overrides nothing is a valid smoke test.

---

## `[BurnBATestClass]` attribute

```csharp
[BurnBATestClass(
    Order           = 1,      // global execution order (lower = earlier)
    TimeoutSeconds  = 600,    // per-test timeout (default 10 min)
    StopTestsOnError = true)] // skip remaining tests on failure
```

* `Order` — tests within the same run are sorted by this value; ties run in
  discovery order.
* `StopTestsOnError` — useful for "smoke" tests that must pass before the rest
  can be meaningful (e.g. the bundle must install before you can uninstall it).

---

## Overriding the command passed to the real BA

`TestBaCommand` is a mutable, readable wrapper around the `Command` struct that
the burn engine sends to `OnCreate`.

```csharp
public override int OnCreate(IBootstrapperEngine pEngine, ref Command command)
{
    var tc = new TestBaCommand(command); // copy original values
    tc.Action      = LaunchAction.Install;
    tc.Display     = Display.None;
    tc.CommandLine = "/quiet PROPERTY=Value";

    var modified = tc.ToCommand();       // convert back to Command struct
    return base.OnCreate(pEngine, ref modified);
    // base re-serializes the modified command and forwards it to the real BA.
}
```

`TestBaCommand` properties:

| Property | Type | Notes |
|---|---|---|
| `Action` | `LaunchAction` | Install / Uninstall / Repair / Modify |
| `Display` | `Display` | Full / Passive / None / Embedded |
| `CommandLine` | `string` | Full command line string |
| `CmdShow` | `int` | SW_* window show flag |
| `Resume` | `ResumeType` | Normal / Reboot / Suspend / … |
| `Relation` | `RelationType` | Detects / Dependent / … |
| `Passthrough` | `bool` | |
| `LayoutDirectory` | `string` | |
| `BootstrapperWorkingFolder` | `string` | |
| `BootstrapperApplicationDataPath` | `string` | |

The command is also available as `this.Command` (the `TestBaCommand` set by the
dispatcher before each `OnCreate` call) for read access in later callbacks.

---

## Recording failures: `AddException`

Never `throw` directly from a BA callback override — exceptions are caught by
the dispatcher and recorded.  Use `AddException` instead:

```csharp
this.AddException(new BurnBAAssertException("Expected X but got Y."));
```

Signature:

```csharp
public void AddException(Exception ex, bool endAutoPilot = true);
```

* **First call wins** — subsequent calls while a failure is already recorded are
  no-ops.
* `endAutoPilot = true` (default) — also sets `EndTestAutoPilot` so burn drives
  itself to a clean shutdown without further test involvement.
* Use `endAutoPilot: false` when the test is already past the point where
  autopilot cancels would make sense (e.g. inside `OnApplyComplete` or
  `OnShutdown`).

```csharp
// During detect — autopilot will cancel the apply phase.
public override int OnDetectComplete(int hrStatus, bool fEligibleForCleanup)
{
    if (hrStatus != 0)
        this.AddException(new BurnBAAssertException($"0x{hrStatus:X8}"));
    return base.OnDetectComplete(hrStatus, fEligibleForCleanup);
}

// Inside OnApplyComplete — already past the cancel gate; no autopilot needed.
public override int OnApplyComplete(int hrStatus, ...)
{
    if (hrStatus != 0)
        this.AddException(new BurnBAAssertException("Apply failed."), endAutoPilot: false);
    return base.OnApplyComplete(hrStatus, ...);
}
```

---

## Autopilot: `EndTestAutoPilot`

Set `this.EndTestAutoPilot = true` (or call `AddException`) to tell the
framework: *"the test is done; let burn shut down cleanly without my
involvement."*

**Phase-aware behaviour:**

| Phase | Autopilot effect |
|---|---|
| Before `OnApplyBegin` (detect, plan) | No change — burn proceeds normally through those phases |
| During apply (`OnApplyBegin` → `OnApplyComplete`) | `fCancel = true` on every cancellable message; burn exits the apply phase quickly |
| After `OnApplyComplete` (unregister, shutdown) | No change — burn shuts down normally |

Autopilot is **per-iteration only**.  It does not prevent burn from restarting
for subsequent test iterations.

---

## Multiple iterations: `[BurnBAInlineData]`

Apply `[BurnBAInlineData]` one or more times to run the same bundle process
through multiple test scenarios in a single test class.  Each attribute
represents one iteration.

```csharp
[BurnBATestClass(Order = 2)]
[BurnBAInlineData("install")]
[BurnBAInlineData("repair")]
public sealed class MultiPassTest : BurnBATestBase
{
    // this.TestData[0] is the string from the attribute ("install" or "repair")
    // this.TestIteration is the 0-based iteration index

    public override int OnCreate(IBootstrapperEngine pEngine, ref Command command)
    {
        var tc = new TestBaCommand(command);
        tc.Action = this.TestData[0] is "repair"
            ? LaunchAction.Repair
            : LaunchAction.Install;
        var cmd = tc.ToCommand();
        return base.OnCreate(pEngine, ref cmd);
    }
}
```

All iterations share one burn process; burn restarts between iterations.
The engine-quit message is sent only on the last iteration's `OnShutdown`.

---

## Reporting failures from `Dispose`

`BurnBATestBase.Dispose()` is called both on exception and at the end of each
iteration.  Override it to release per-iteration resources:

```csharp
private IDisposable _resource;

public override void Dispose()
{
    _resource?.Dispose();
    _resource = null;
}
```

Dispose must be **idempotent** (safe to call multiple times).

---

## Quick reference: useful callbacks to override

| Callback | Typical override reason |
|---|---|
| `OnCreate` | Override `Action`, `Display`, or other command-line flags |
| `OnDetectComplete` | Assert detection succeeded; check `hrStatus` |
| `OnPlanComplete` | Assert plan succeeded |
| `OnApplyComplete` | Assert apply result; check `hrStatus` and `restart` |
| `OnShutdown` | Final assertions; check that expected callbacks were called |
| `OnExecutePackageBegin` | Observe which packages are being executed |
| `OnError` | Inspect error codes; optionally suppress/abort |

---

## Assertion helpers

| Type | Purpose |
|---|---|
| `BurnBAAssertException` | Carries a human-readable assertion message through the framework |
| `BurnAssert` (static) | Convenience wrappers — prefer `AddException` over `BurnAssert.X` |

---

## Common patterns

### Assert a property once, then let burn run to completion

```csharp
public override int OnCreate(IBootstrapperEngine pEngine, ref Command command)
{
    if (command.action != LaunchAction.Install)
    {
        this.AddException(new BurnBAAssertException(
            $"Expected Install but got {command.action}.")); // autopilot on by default
    }
    var tc = new TestBaCommand(command);
    tc.Display = Display.None;
    var cmd = tc.ToCommand();
    return base.OnCreate(pEngine, ref cmd);
}
```

### Verify that a specific callback was reached

```csharp
private bool _applyCompleteCalled;

public override int OnApplyComplete(int hrStatus, ...)
{
    this._applyCompleteCalled = true;
    return base.OnApplyComplete(hrStatus, ...);
}

public override int OnShutdown(ref BOOTSTRAPPER_SHUTDOWN_ACTION action)
{
    if (!this._applyCompleteCalled)
        this.AddException(new BurnBAAssertException(
            "OnApplyComplete was not called."), endAutoPilot: false);
    return base.OnShutdown(ref action);
}
```

### Stop all remaining tests on a critical failure

```csharp
[BurnBATestClass(Order = 1, StopTestsOnError = true)]
public sealed class InstallFirstTest : BurnBATestBase { ... }
```

---

## Installing this skill into a project

If your project references `PanelSwWix4.Burn.UnitTest` you can copy this skill
to your repository's `.agent/skills/` directory by running:

```
dotnet build /p:CopyBurnTestSkill=true
```

The skill is then available to the agent at
`.agent/skills/burn-ba-unit-test-authoring/SKILL.md`.
