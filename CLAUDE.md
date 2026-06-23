# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

**events-map** is a WPF desktop application targeting .NET Framework 4.7.2, built in Visual Studio 2022. It displays world events (film, music, sports, art) on a map. The app is an academic project — keep implementation simple enough to explain at a university defense.

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

Output lands in `EventsApp\bin\Debug\` or `EventsApp\bin\Release\`. There are no automated tests.

---

## Architecture Conventions

### MVVM — but keep it simple
- **Models** (`Model/`) are plain POCOs — no `INotifyPropertyChanged`, no logic.
- **ViewModels** (`ViewModel/`) implement `INotifyPropertyChanged`. Each ViewModel owns a flat DTO (row-item or card-item class defined in the same file) used as the `ItemsSource` type.
- **Views** (`View/`) are WPF `Page` classes (not `UserControl`). DataContext is set in the code-behind constructor (`DataContext = new XxxViewModel()`), not via XAML or ViewModelLocator. The ViewModelLocator exists in `VML/ViewModelLocator.cs` but is **not actively used** by any current view.

### Navigation — code-behind content-swapping (no NavigationService)
`MainWindow` has two content areas that swap visibility:
- `MapWorkspace` (Grid) — the default map screen, shown on startup.
- `MainFrame` (WPF `Frame`) — shown when a menu item is clicked, hidden when returning to map.

Navigation is done entirely in `MainWindow.xaml.cs`:
- `NavigateTo(Page page, string title)` — hides MapWorkspace, shows MainFrame, calls `MainFrame.Navigate(page)`, sets the header title, shows the back button.
- `ShowMapWorkspace()` — reverses that.
- The "Event map" title button and the **back button** (`PageBackButton`) both call `ShowMapWorkspace()`. **The back button currently has no Click handler wired** — it needs one.
- Hamburger menu item click handlers (`MenuEvents_Click`, etc.) are fully wired and call `NavigateTo`.

### Data loading and saving — EventService, JSON
`Services/EventService.cs` loads and saves data using three JSON files:
- `events.json` → `List<Event>`
- `eventTypes.json` → `List<EventType>`
- `tags.json` → `List<Tag>`

Uses `System.Web.Script.Serialization.JavaScriptSerializer` (no external NuGet). `Event` requires manual deserialization because of `AttendanceRange` enum and nullable fields; `EventType` and `Tag` are deserialized directly. `SaveEvents` manually builds `List<Dictionary<string,object>>` to preserve ISO date strings (JavaScriptSerializer emits `/Date(ticks)/` for DateTime). `SaveEventTypes` and `SaveTags` use direct `Serialize()`.

**Data path (important):** In `#if DEBUG` builds, `EventService` reads and writes from the project's source `Data\` folder (`bin\Debug\..\..\Data`). This prevents VS rebuilds from overwriting saved changes via `CopyToOutputDirectory="PreserveNewest"`. In Release builds, it uses `Data\` next to the exe.

### Window size — fixed 1024×768, no resize
`MainWindow` has `ResizeMode="NoResize"` and `Height="768" Width="1024"`.

### Icon sizing conventions
- Hamburger, back, and action-row buttons: **36×36 button** with **Padding="6"**, image inside is `Stretch="Uniform"` (~24px effective).
- DataGrid action buttons (Info/Edit/Delete): **24×24** image with **Padding="8"** on the button.
- Menu items in hamburger popup: **22×22** image.
- Event card icons in the list: **48×48**.
- Map markers: **22×22** image.
- Inline small icons (location pin, date icon): **13×13**.

### Color-blind accessibility
Charts use shape/pattern differentiation, not color alone:
- Events-per-type bar chart and attendance bar chart: single dark fill `#444444`.
- Pie chart: solid `#333333` for humanitarian, diagonal-hatch `DrawingBrush` for standard.

---

## Folder / File Structure

```
EventsApp/
├── App.xaml / App.xaml.cs
├── MainWindow.xaml / MainWindow.xaml.cs      ← shell, navigation, drag-drop, map
│
├── Model/
│   ├── Event.cs          ← Id, Name, Description, EventTypeId, Attendance (enum),
│   │                        IconPath, IsHumanitarian, AverageCost, Country, City,
│   │                        HistoryDates, CurrentYearDate, TagIds, MapX, MapY
│   ├── EventType.cs      ← Id, Name, Description, IconPath
│   ├── Tag.cs            ← Id, Description, ColorHex
│   └── AttendanceRange.cs  ← enum: Upto1000, From1000To5000, From5000To10000, Over10000
│
├── ViewModel/
│   ├── MainWindowViewModel.cs     ← EventCardItem (DTO), MainWindowViewModel
│   │                                 ObservableCollection<EventCardItem> EventItems (list panel)
│   │                                 ObservableCollection<EventCardItem> MapMarkerItems (on map)
│   │                                 string MapFilterText (real-time filter)
│   ├── EventsViewModel.cs         ← EventRowItem (DTO), EventsViewModel
│   │                                 ObservableCollection<EventRowItem> Events
│   │                                 TypeName falls back to "Unknown type" for orphaned EventTypeId
│   ├── EventTypesViewModel.cs     ← EventTypeRowItem (DTO), EventTypesViewModel
│   │                                 ObservableCollection<EventTypeRowItem> EventTypes
│   ├── TagsViewModel.cs           ← TagRowItem (DTO), TagsViewModel
│   │                                 ObservableCollection<TagRowItem> Tags
│   │                                 static ColorNames dictionary (hex → display name)
│   ├── StatisticsViewModel.cs     ← ChartBarItem (DTO), StatisticsViewModel
│   │                                 ObservableCollection<ChartBarItem> EventsPerType
│   │                                 ObservableCollection<ChartBarItem> EventsByAttendance
│   │                                 double HumanitarianPercent, labels
│   ├── EventDetailViewModel.cs    ← read-only DTO for EventDetailWindow; defines TagChipItem
│   │                                 resolves EventTypeId→name+icon, TagIds→chips,
│   │                                 AttendanceRange→string, cost 0→"Free"
│   ├── EventTypeDetailViewModel.cs ← read-only DTO for EventTypeDetailWindow
│   └── TagDetailViewModel.cs      ← read-only DTO for TagDetailWindow
│
├── View/
│   ├── EventsView.xaml / .cs          ← Page, DataContext set in code-behind
│   ├── EventTypesView.xaml / .cs
│   ├── TagsView.xaml / .cs
│   ├── StatisticsView.xaml / .cs
│   ├── EventDetailWindow.xaml / .cs   ← modal info dialog for a single Event
│   ├── EventTypeDetailWindow.xaml / .cs ← modal info dialog for a single EventType
│   ├── TagDetailWindow.xaml / .cs     ← modal info dialog for a single Tag
│   └── DeleteConfirmWindow.xaml / .cs ← shared Yes/No confirmation dialog
│                                          ctor takes message string; DialogResult=true on Yes
│
├── VML/
│   └── ViewModelLocator.cs      ← AutoHookedUpViewModel attached property (defined but not used)
│
├── Services/
│   └── EventService.cs          ← LoadEvents(), LoadEventTypes(), LoadTags()
│                                   SaveEvents(), SaveEventTypes(), SaveTags()
│                                   Data path: source Data\ in Debug, exe Data\ in Release
│
├── Converters/
│   └── PercentageToPieSliceConverter.cs  ← IValueConverter: double [0–1] → PathGeometry pie slice
│
├── Data/
│   ├── events.json        ← 6 sample events; 2 have MapX/MapY pre-set (NBA Finals, Australian Open)
│   ├── eventTypes.json    ← 5 types: Music(1), Film(2), Basketball(3), Tennis(4), Art(5)
│   └── tags.json          ← 5 tags with ColorHex values
│
└── Resources/Images/
    ├── world-map.png        ← fixed map background; also fallback icon for orphaned EventTypeId
    ├── film.png, music.png, tennis.png, basketball.png, art.png   ← event type icons
    ├── hamburger-menu.png, back.png, close.png
    ├── add.png, edit.png, delete.png, info.png, search.png, reset.png
    ├── event.png, event-type.png, tag.png, statistic.png
    ├── location.png, date.png, dollar.png, yes.png
```

---

## Features Completed

### Map Workspace (MainWindow)
- Left column (280px): scrollable `ItemsControl` of event cards showing icon (48×48), name, location, date. Cards start in the list or on the map depending on whether the event has `MapX`/`MapY` in JSON.
- Right column: real-time filter TextBox bound to `MapFilterText` (filters map markers by name via `IsVisibleOnMap` property binding + `BooleanToVisibilityConverter`), Reset button clears filter.
- Fixed world-map image as background; transparent `Canvas` (`MapCanvas`) overlaid for markers.
- **Drag list → map**: drag a card from the list, drop on map canvas → `PlaceMarker` creates a `StackPanel` (22×22 icon + name label) at drop position, card moves from `EventItems` to `MapMarkerItems`.
- **Drag map → list**: drag a marker back to the list border → marker removed from canvas, card moves back to `EventItems`.
- **Drag map → map**: drag a marker to a new canvas position → `RepositionMarker` moves it.
- **Overlap prevention**: `IsOverlapping` checks all existing marker centres with a 40px threshold; drops too close to an existing marker are silently ignored.
- **Click marker**: short-click (no drag initiated) → `ShowMarkerInfoPopup` opens a WPF `Popup` anchored to the marker showing name, location, date. Close button dismisses it.
- 2 events start on the map (NBA Finals at ~100,260; Australian Open at ~550,400) via `MapX`/`MapY` in JSON, placed in `MainWindow_Loaded`.

### Hamburger Menu
- 36×36 button top-left toggles a WPF `Popup` (`StaysOpen="False"`).
- 4 menu items: Events, Event Types, Tags, Statistics — each with 22×22 icon and label.
- Clicking any item closes the popup and calls `NavigateTo(new XxxPage(), "Title")`.
- Clicking the "Event map" title button always returns to the map workspace.

### Events Page (`View/EventsView.xaml`)
- DataGrid columns: Icon (26×26), ID, Name, Type, Location, Actions (Info/Edit/Delete buttons, 24×24 icons).
- Row height 52px, horizontal grid lines only, `#F5F5F5` header.
- Action row: Add event + Search buttons (left), Filter TextBox with placeholder + Reset button (right).
- **Info button wired** — opens `EventDetailWindow` as modal (`ShowDialog`).
- **Delete button wired** — opens `DeleteConfirmWindow`; on Yes: loads list, removes item, `SaveEvents`, removes from ObservableCollection. Add/Edit/Search/Filter/Reset are still visual-only.

### Event Detail Dialog (`View/EventDetailWindow.xaml`)
- `Window`, 580×650, `ResizeMode="NoResize"`, `WindowStartupLocation="CenterOwner"`, `ShowInTaskbar="False"`.
- `DataContext` is `EventDetailViewModel` (set in constructor).
- **Layout**: header row (back icon + "Event details" title) / `Rectangle` separator / `ScrollViewer` content / `Rectangle` separator / footer Close button.
- **Content — two columns** (`2*` / `20` spacer / `1.5*`):
  - Left: 56×56 event icon, name (18px Bold), event ID (muted), type badge (type icon 16×16 + type name), thin rule, description.
  - Right: location (pin icon), current date (calendar icon), attendance (audience icon), average cost (dollar icon), humanitarian status, thin rule, "Past dates" heading, scrollable list (max 130px) newest-first.
- **Tags section** (full width below columns): `WrapPanel` of rounded border chips. Each chip label is `"Description · ColorName"` — no color-only differentiation.
- `EventDetailViewModel` falls back to `"Unknown"` / `event-type.png` if the EventType is missing.

### Event Types Page (`View/EventTypesView.xaml`)
- DataGrid columns: Icon (26×26), ID (`Code`), Name, Actions (Info/Edit/Delete).
- **Info button wired** — opens `EventTypeDetailWindow` as modal.
- **Delete button wired** — opens `DeleteConfirmWindow`; on Yes: loads list, removes item, `SaveEventTypes`, removes from ObservableCollection. Add/Edit/Search/Filter/Reset are still visual-only.

### Event Type Detail Dialog (`View/EventTypeDetailWindow.xaml`)
- `Window`, 420×320, `ResizeMode="NoResize"`, `WindowStartupLocation="CenterOwner"`.
- Shows 48×48 icon, name (bold), ID, description. Close button in footer.

### Tags Page (`View/TagsView.xaml`)
- DataGrid columns: ID (`Code`), Color (displays as `"Green (#4CAF50)"` via `ColorDisplay` property), Actions (Info/Edit/Delete).
- Note: Tag `Description` field is loaded but **not displayed** in the current DataGrid — it's only in the DTO.
- **Info button wired** — opens `TagDetailWindow` as modal.
- **Delete button wired** — opens `DeleteConfirmWindow`; on Yes: loads list, removes item, `SaveTags`, removes from ObservableCollection. Add/Edit/Search/Filter/Reset are still visual-only.

### Tag Detail Dialog (`View/TagDetailWindow.xaml`)
- `Window`, 380×340, `ResizeMode="NoResize"`, `WindowStartupLocation="CenterOwner"`.
- Shows ID, color swatch (Border with ColorHex background), color display string, description. Close button in footer.

### Delete Confirm Dialog (`View/DeleteConfirmWindow.xaml`)
- Shared across all three list pages. Constructor takes a `string message`.
- Grid layout: TextBlock anchored top (Margin="30,30,30,0", FontSize=20), Yes/No StackPanel anchored bottom (Margin="0,0,0,20").
- Yes button: sets `DialogResult = true`. No button: sets `DialogResult = false`.
- Size: 380×230. Callers check `dlg.DialogResult == true` before proceeding.

### Statistics Page (`View/StatisticsView.xaml`)
- 3 charts displayed horizontally centred, each in a rounded border card:
  1. **Events per Type** — horizontal bar chart, bars scaled to max count, label + bar (`#444444`) + count.
  2. **Event Purpose** — pie chart using `PercentageToPieSliceConverter`; solid `#333333` slice = humanitarian events, hatched `DrawingBrush` background = standard events. Legend uses shape fills, no color-only differentiation.
  3. **Events by Attendance** — horizontal bar chart for all 4 `AttendanceRange` buckets.
- All data read from `EventService` at construction time; no interactivity.

---

## What Is NOT Done Yet

- **Back button** (`PageBackButton`) has no `Click` handler — visible on all pages but clicking does nothing.
- **Add button** on Events, Event Types, and Tags pages — no handler, no add form/dialog.
- **Edit button** on Events, Event Types, and Tags pages — no handler, no edit form/dialog.
- **Search button** on all three list pages — no handler.
- **Filter TextBox** on list pages — no binding or handler (the map workspace filter works; per-page ones do not).
- **Reset button** on list pages — no handler.
- **Tag `Description`** is loaded but not shown in the Tags DataGrid.
- **MapWorkspace in-session consistency**: `MainWindowViewModel` loads once at startup. Deleting an EventType mid-session won't update the map event cards until next restart — this is expected and acceptable for the academic scope.

---

## Constraints to Respect (Academic Project)

- No DI containers (no Unity, Autofac, etc.) — instantiate directly.
- No `NavigationService` or `DialogService` abstractions — navigate in code-behind, open dialogs with `new XxxWindow().ShowDialog()`.
- No external NuGet packages beyond what's already used (`System.Web.Script.Serialization` from the framework).
- No interfaces for services — `EventService` is a concrete class, used directly.
- ViewModelLocator is available but not required — DataContext assignment in code-behind is fine and already the established pattern.
- Keep everything explainable to a professor: prefer obvious code over clever abstractions.
