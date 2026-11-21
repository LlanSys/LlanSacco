# Package management and source formatting

Set direct NuGet versions in [Directory.Packages.props](../../Directory.Packages.props). Projects declare the packages they use without local versions or overrides. Keep all MassTransit packages on the shared `MassTransitVersion` exact pin so the API, workers and EF outboxes use the same contract.

Repository restores use NuGet.org through [nuget.config](../../nuget.config), which clears inherited sources and mappings. Use `dotnet tool restore --configfile nuget.config` to restore the pinned EF and Husky tools. The API and Blazor Dockerfiles copy the central manifest into their restore stage. GitHub Container Registry serves container images through the deployment workflow.

## Format the files you changed

From the repository root, run the StarterTemplate-derived formatter on the relevant feature or file. It combines .NET and Roslynator fixes with C# and Razor layout checks.

```powershell
./scripts/Format-Code.ps1 -Include src/Backend/Application/LS.Application/Features/Membership
./scripts/Format-Code.ps1 -Check -Include src/Backend/Application/LS.Application/Features/Membership
```

Use `-WrapLongLines` when wrapping is part of the edit. The default scope covers backend, web, shared, tests and tools. Select a mobile path explicitly on a machine with the corresponding MAUI workloads. Both formatting passes exclude migrations and generated source.

## CI

The Architecture job in Required / Remediation checks the package policy, exercises the rejection cases, restores the pinned tools and runs the formatter fixtures. Existing source can be formatted as part of its feature work.

The implementation takes the formatter and package policy patterns from [StarterTemplate at a361bda](https://github.com/LlanSys/starter-template/tree/a361bda27aece09199e20f4ddad3847570165aff). LlanSacco's versions and application targets come from its own dependency graph.
