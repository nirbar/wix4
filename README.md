<img src="https://github.com/wixtoolset/Home/raw/master/imgs/wix-white-bg.png" alt="WiX Toolset" height="128" />

[![latest version](https://img.shields.io/nuget/vpre/wix)](https://www.nuget.org/packages/PanelSwWix4)
[![download count](https://img.shields.io/nuget/dt/wix)](https://www.nuget.org/stats/packages/PanelSwWix4?groupby=Version)
[![build status](https://img.shields.io/github/actions/workflow/status/nirbar/wix4/build.yml?branch=develop)](https://github.com/wixtoolset/wix/actions/workflows/build.yml?query=branch%3Adevelop)

# PanelSwWix4: Custom WiX Toolset

This repository contains the PanelSwWix4: A custom WiX Toolset codebase

# Customizations:

- [Harvest AdditionalOptions](https://github.com/wixtoolset/issues/issues/7427): Allow customized AdditionalOptions for all HarvestX MSBuild ItemGroups
- [Fix MultiSzInsertString](https://github.com/wixtoolset/issues/issues/7311) to create a double-null terminated buffer
- [6298](https://github.com/wixtoolset/issues/issues/6298): Extract detached containers with "wix burn extract"
- ExePackage/@DetectVersionVariable: Support using one of the XxxSearch elements to provide a version number in a variable to test against the package's version.
- [4889](https://github.com/wixtoolset/issues/issues/4889): Support custom container compressions using bundle extensions
