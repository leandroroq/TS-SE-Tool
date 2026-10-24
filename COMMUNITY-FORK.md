# Community fork notice

This repository is a community-maintained derivative of [LIPtoH/TS-SE-Tool](https://github.com/LIPtoH/TS-SE-Tool), originally authored by LIPtoH. The upstream source headers attribute copyright to LIPtoH (2016–2022). The original Apache License 2.0 is retained in `LICENSE`.

This source snapshot contains community changes for compatibility and localization work. It does not claim endorsement by, or affiliation with, LIPtoH, SCS Software, Euro Truck Simulator 2, or American Truck Simulator. Product names and trademarks belong to their respective owners.

## Build

The solution targets .NET Framework 4.7.2 and uses the legacy Visual Studio project format. Open `TS SE Tool.sln` in Visual Studio, restore the NuGet packages listed in `TS SE Tool/packages.config`, then build the solution. The source snapshot intentionally excludes generated `bin` and `obj` folders, game profiles/saves, local configuration, logs, and test fixtures.

## Distribution status

This is a source snapshot, not a verified binary release. Before publishing downloadable executables, review and include the applicable notices/licenses for bundled third-party components, including `OpenPainter.ColorPicker.dll` and `SII_Decrypt.dll`, and complete ETS2/ATS save read/write validation on supported game versions.
