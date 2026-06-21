# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

**events-map** is a WPF desktop application targeting .NET Framework 4.7.2, built in Visual Studio 2022. It is intended to display events (film, music, sports, art) on a world map. The project is in early development — the UI shell exists but the core features are not yet implemented.

## Build & Run

Open and build via Visual Studio 2022:
```
EventsApp\EventsApp.sln
```

Or via MSBuild from the repo root:
```
msbuild EventsApp\EventsApp.sln /p:Configuration=Debug
msbuild EventsApp\EventsApp.sln /p:Configuration=Release
```

Output lands in `EventsApp\bin\Debug\` or `EventsApp\bin\Release\`. There are no automated tests yet.

## Architecture

The project follows **MVVM** (Model-View-ViewModel). The intended folder layout (declared in the `.csproj`) is:

- `Model/` — domain data classes
- `View/` — XAML user controls and windows
- `ViewModel/` — ViewModels bound to Views
- `VML/` — infrastructure (ViewModelLocator)

### ViewModelLocator convention

`VML/ViewModelLocator.cs` provides an attached property `AutoHookedUpViewModel`. When set to `true` on a View, it automatically resolves and instantiates the matching ViewModel by convention:

- Takes the View's full type name
- Replaces `.Views` with `.ViewModel` in the namespace
- Appends `Model` to the class name
- Example: `EventsApp.Views.MapView` → `EventsApp.ViewModel.MapViewModel`

To wire a View to its ViewModel, set the attached property in XAML:
```xml
<UserControl vml:ViewModelLocator.AutoHookedUpViewModel="True" ...>
```

### Resources

Event category icons live in `EventsApp/Resources/Images/` and are compiled as WPF `Resource` items: `world-map.png`, `film.png`, `music.png`, `tennis.png`, `basketball.png`, `art.png`.

Reference them in XAML as pack URIs:
```xml
Source="/Resources/Images/film.png"
```
