# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

**events-map** is a WPF desktop application targeting .NET Framework 4.7.2, built in Visual Studio 2022. It displays world events (film, music, sports, art) on a map. The app is an academic project — keep implementation simple enough to explain at a university defense.

## Build & Run

Open and build via Visual Studio 2022:
```
EventsApp\EventsApp.sln
```

Or via MSBuild (use the full VS2022 path — `msbuild` is not on PATH):
```
& "C:\Program Files\Microsoft Visual Studio\2022\Community\MSBuild\Current\Bin\MSBuild.exe" EventsApp\EventsApp.sln /p:Configuration=Debug
```

Output lands in `EventsApp\bin\Debug\` or `EventsApp\bin\Release\`. There are no automated tests.

---

## Architecture Conventions

### MVVM — but keep it simple
- **Models** (`Model/`) are plain POCOs — no `INotifyPropertyChanged`, no logic.
- **ViewModels** (`ViewModel/`) — page ViewModels implement `INotifyPropertyChanged` directly. Add/Edit form ViewModels inherit from `ViewModelBase` (abstract, implements both `INotifyPropertyChanged` + `INotifyDataErrorInfo`). Each page ViewModel owns a flat DTO (row-item class defined in the same file) used as the `ItemsSource` type.
- **Views** (`View/`) are WPF `Page` classes for the main pages (not `UserControl`). Popup forms are `Window` classes. DataContext is always set in the code-behind constructor, not via XAML.
- No ViewModelLocator — the `VML/ViewModelLocator.cs` file was deleted as it was never referenced anywhere.

### Navigation — code-behind content-swapping (no NavigationService)
`MainWindow` has two content areas that swap visibility:
- `MapWorkspace` (Grid) — the default map screen, shown on startup.
- `MainFrame` (WPF `Frame`) — shown when a menu item is clicked, hidden when returning to map.

Navigation is done entirely in `MainWindow.xaml.cs`:
- `NavigateTo(Page page, string title)` — hides MapWorkspace, shows MainFrame, navigates, sets header title, shows back button.
- `ShowMapWorkspace()` — reverses that.
- The "Event map" title button and the back button (`PageBackButton`) both call `ShowMapWorkspace()`. **The back button has no Click handler wired yet.**
- Hamburger menu item click handlers (`MenuEvents_Click`, etc.) are fully wired.

### Data loading and saving — EventService, JSON
`Services/EventService.cs` loads and saves data using three JSON files:
- `events.json` → `List<Event>`
- `eventTypes.json` → `List<EventType>`
- `tags.json` → `List<Tag>`

Uses `System.Web.Script.Serialization.JavaScriptSerializer` (no external NuGet). `Event` requires manual deserialization because of `AttendanceRange` enum and nullable fields; `EventType` and `Tag` are deserialized directly. `SaveEvents` manually builds `List<Dictionary<string,object>>` to preserve ISO date strings. `SaveEventTypes` and `SaveTags` use direct `Serialize()`.

**Data path (important):** In `#if DEBUG` builds, `EventService` reads/writes from the project's source `Data\` folder (`bin\Debug\..\..\Data`). In Release builds, it uses `Data\` next to the exe.

### Window size — fixed 1024×768, no resize
`MainWindow` has `ResizeMode="NoResize"` and `Height="768" Width="1024"`.

### Popup window layout conventions
All Add/Edit/Search/Detail/Delete popup windows follow a strict 5-row Grid layout:
```
Row 0: Header  (Height="52") — StackPanel Orientation="Horizontal" Margin="16,0"
                               back.png (20×20) + TextBlock FontSize="15" FontWeight="SemiBold"
Row 1: Sep     (Height="1")  — Rectangle Fill="#E0E0E0"
Row 2: Body    (Height="*")  — Margin="16,20,16,20" on the content container (Grid or StackPanel)
Row 3: Sep     (Height="1")  — Rectangle Fill="#E0E0E0"
Row 4: Footer  (Height="52") — StackPanel HorizontalAlignment="Right" Margin="0,0,16,0"
                               Primary button: Padding="12,6" Margin="0,0,20,0"
                               Secondary button: Padding="12,6"
```
- All popups: `ResizeMode="NoResize"`, `WindowStartupLocation="CenterOwner"`, `ShowInTaskbar="False"`.
- Primary (Add/OK/Search) button always has `yes.png` (14×14) icon + text label.
- Cancel/Close button always has `close.png` (14×14) icon + text label.
- Reset button (Search dialogs) always has `reset.png` (14×14) icon + text label.

### Add/Edit popup sizes
| Window | Width × Height |
|--------|---------------|
| AddEventWindow / EditEventWindow | 680 × 690 |
| AddEventTypeWindow / EditEventTypeWindow | 460 × 500 |
| AddTagWindow / EditTagWindow | 460 × 420 |
| SearchEventWindow | 580 × 500 |
| SearchEventTypeWindow / SearchTagWindow | 420 × 360 |

### Icon sizing conventions
- Hamburger, back, and action-row buttons: **36×36 button** with **Padding="6"**, image `Stretch="Uniform"` (~24px effective).
- DataGrid action buttons (Info/Edit/Delete): **24×24** image with **Padding="8"** on the button.
- Menu items in hamburger popup: **22×22** image.
- Event card icons in the list: **48×48**.
- Map markers: **22×22** image.
- Inline small icons (location pin, date icon): **13×13**.
- Popup form button icons: **14×14**.

### Color-blind accessibility
- Charts use shape/pattern differentiation, not color alone.
- Validation errors must **not** use red borders or any color-only indicators. Use text/icon-based indicators instead (e.g. "!" symbol + "Required field" text).
- Tag color identification always shows the name alongside the hex (e.g. `"Green (#4CAF50)"`).

### Collection filter + search pattern (EventsViewModel, EventTypesViewModel, TagsViewModel)
Each page ViewModel has two independent filter layers that AND together in `FilterPredicate`:
1. **Text filter** (`FilterText` property) — real-time, bound to the page's `FilterBox` TextBox via `TextChanged`.
2. **Search predicate** (`_searchPredicate` field, `Func<RowItem, bool>`) — set by the Search dialog via `ApplySearch(predicate)`, cleared via `ClearSearch()`.

Each page ViewModel also owns a `SearchState` property (a `SearchXxxViewModel` instance created once at construction). This instance is passed into the Search dialog constructor so the form retains values between openings. `Reset()` on the shared instance clears both form fields and the stored state simultaneously.

### Edit window conventions
- `OriginalId` is stored at construction so the uniqueness check can exclude the current record: `existing.Any(x => x.Id == newId && x.Id != vm.OriginalId)`.
- EditEventWindow additionally stores `_originalMapX` / `_originalMapY` at construction and copies them to the saved `Event` — the edit form has no map-position fields.
- After saving, the page ViewModel's `UpdateRow(originalId, ...)` replaces the item at its index in the `ObservableCollection` (no `INotifyPropertyChanged` on DTOs — replacement fires a WPF Replace notification).

### Icon picker lists
- **AddEventViewModel / EditEventViewModel**: `(None)`, Film, Music, Tennis, Basketball, Art — 6 items. `(None)` → `IconPath = null`; EventType icon is used as the fallback on the map/list.
- **AddEventTypeViewModel / EditEventTypeViewModel**: `(None)`, Event type, Film, Music, Tennis, Basketball, Art — 7 items. Icon is required (used as fallback for events of this type).
- Color picker (**AddTagViewModel**): `(Select color)` (Hex=null, required-field sentinel) + 7 named colors — Green `#4CAF50`, Blue `#2196F3`, Amber `#FF9800`, Red `#F44336`, Purple `#9C27B0`, Teal `#009688`, Indigo `#3F51B5`. Color is a required field; validation checks `Hex ≠ null`. **EditTagViewModel** does not have the "(Select color)" sentinel — it always pre-selects the saved color.

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
│   ├── MainWindowViewModel.cs       ← EventCardItem (DTO), MainWindowViewModel
│   │                                   ObservableCollection<EventCardItem> EventItems (list)
│   │                                   ObservableCollection<EventCardItem> MapMarkerItems (map)
│   │                                   string MapFilterText (real-time filter)
│   ├── EventsViewModel.cs           ← EventRowItem (DTO), EventsViewModel
│   │                                   ObservableCollection<EventRowItem> Events
│   │                                   FilterText, ApplySearch, ClearSearch, UpdateRow, AddRow
│   │                                   SearchEventViewModel SearchState (persistent search state)
│   ├── EventTypesViewModel.cs       ← EventTypeRowItem (DTO), EventTypesViewModel
│   │                                   ObservableCollection<EventTypeRowItem> EventTypes
│   │                                   FilterText, ApplySearch, ClearSearch, UpdateRow, AddRow
│   │                                   SearchEventTypeViewModel SearchState
│   ├── TagsViewModel.cs             ← TagRowItem (DTO), TagsViewModel
│   │                                   ObservableCollection<TagRowItem> Tags
│   │                                   FilterText, ApplySearch, ClearSearch, UpdateRow, AddRow
│   │                                   static ColorNames dict (hex→name); SearchTagViewModel SearchState
│   ├── StatisticsViewModel.cs       ← ChartBarItem (DTO), StatisticsViewModel
│   ├── EventDetailViewModel.cs      ← read-only DTO for EventDetailWindow; TagChipItem
│   ├── EventTypeDetailViewModel.cs  ← read-only DTO for EventTypeDetailWindow
│   ├── TagDetailViewModel.cs        ← read-only DTO for TagDetailWindow
│   ├── ViewModelBase.cs             ← abstract; INotifyPropertyChanged + INotifyDataErrorInfo;
│   │                                   _errors dict; SetProperty<T>; SetErrors/ClearErrors;
│   │                                   ValidateProperty (virtual); ValidateAllProperties (public)
│   ├── AddEventViewModel.cs         ← EventTypeComboItem, AttendanceComboItem (has IsNone flag),
│   │                                   TagCheckItem, HistoryDateItem, IconComboItem DTOs (shared with Edit)
│   │                                   AddEventViewModel : ViewModelBase; validates EventIdText
│   │                                   (required + positive int), Name (required), SelectedEventType
│   │                                   (required, Id≠0), SelectedAttendance (required, IsNone=false),
│   │                                   AverageCostText (empty OK / must be 0 or greater if filled);
│   │                                   computed *HasError / *Error string props;
│   │                                   SetEventIdError(msg) for duplicate check from code-behind
│   ├── AddEventTypeViewModel.cs     ← AddEventTypeViewModel : ViewModelBase; validates IdText,
│   │                                   Name, SelectedIcon (FilePath ≠ null); SetIdError(msg)
│   ├── AddTagViewModel.cs           ← ColorComboItem (DTO), AddTagViewModel : ViewModelBase;
│   │                                   validates IdText (required + positive int),
│   │                                   SelectedColor (required, Hex ≠ null); SetIdError(msg)
│   ├── EditEventViewModel.cs        ← EditEventViewModel; OriginalId; pre-populates all fields
│   ├── EditEventTypeViewModel.cs    ← EditEventTypeViewModel; OriginalId
│   ├── EditTagViewModel.cs          ← EditTagViewModel; OriginalId
│   ├── SearchEventViewModel.cs      ← SearchAttendanceItem, SearchEventTypeItem DTOs
│   │                                   SearchEventViewModel: Name, SelectedEventType, Country,
│   │                                   City, Description, SelectedAttendance, AvgCostText,
│   │                                   IsHumanitarianFilter (bool?), Reset()
│   ├── SearchEventTypeViewModel.cs  ← SearchEventTypeViewModel: Name, Description, Reset()
│   └── SearchTagViewModel.cs        ← SearchTagColorItem (DTO), SearchTagViewModel:
│                                       SelectedColor, Description, Reset()
│
├── View/
│   ├── EventsView.xaml / .cs            ← Page; Search/Add/Edit/Delete/Info/Filter/Reset all wired
│   ├── EventTypesView.xaml / .cs        ← Page; same
│   ├── TagsView.xaml / .cs              ← Page; same
│   ├── StatisticsView.xaml / .cs        ← Page (read-only charts)
│   ├── EventDetailWindow.xaml / .cs     ← 580×650 modal; shows full event detail
│   ├── EventTypeDetailWindow.xaml / .cs ← 420×320 modal
│   ├── TagDetailWindow.xaml / .cs       ← 380×340 modal
│   ├── DeleteConfirmWindow.xaml / .cs   ← 380×200 shared Yes/No; ctor takes message string
│   ├── AddEventWindow.xaml / .cs        ← 680×690; two-column form; DatePicker with TouchCalendar
│   │                                       style; "!" + error text indicators for ID/Name/EventType/
│   │                                       Attendance/AvgCost
│   ├── AddEventTypeWindow.xaml / .cs    ← 460×500; single-column; indicators for Icon/ID/Name
│   ├── AddTagWindow.xaml / .cs          ← 460×420; single-column; indicators for ID/Color
│   ├── EditEventWindow.xaml / .cs       ← 680×690; pre-populated; preserves MapX/MapY
│   ├── EditEventTypeWindow.xaml / .cs   ← 460×460; pre-populated
│   ├── EditTagWindow.xaml / .cs         ← 460×400; pre-populated
│   ├── SearchEventWindow.xaml / .cs     ← 580×500; two-column; AND search; persistent state
│   ├── SearchEventTypeWindow.xaml / .cs ← 420×360; single-column; Name + Description
│   └── SearchTagWindow.xaml / .cs       ← 420×360; single-column; Color (exact) + Description
│
├── Services/
│   └── EventService.cs          ← LoadEvents(), LoadEventTypes(), LoadTags()
│                                   SaveEvents(), SaveEventTypes(), SaveTags()
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
    ├── world-map.png        ← fixed map background; fallback icon for orphaned EventTypeId
    ├── film.png, music.png, tennis.png, basketball.png, art.png   ← event type icons
    ├── hamburger-menu.png, back.png, close.png
    ├── add.png, edit.png, delete.png, info.png, search.png, reset.png
    ├── event.png, event-type.png, tag.png, statistic.png
    ├── location.png, date.png, dollar.png, yes.png, audience.png
    (no dedicated warning/error icon exists — use "!" text character for validation UI)
```

---

## Features Completed

### Map Workspace (MainWindow)
- Left column (280px): scrollable `ItemsControl` of event cards (48×48 icon, name, location, date).
- Right column: real-time filter TextBox + Reset button.
- Drag list→map, map→list, map→map with overlap prevention (40px threshold).
- Click marker → `Popup` info panel (name, location, date).
- 2 events start on the map via `MapX`/`MapY` in JSON.

### Hamburger Menu
- 4 menu items: Events, Event Types, Tags, Statistics.

### Events Page (`EventsView.xaml`)
- DataGrid: Icon, ID, Name, Type, Location, Actions (Info/Edit/Delete).
- **Add**: opens `AddEventWindow`; validates ID (positive int, unique) + Name + EventType + Attendance (required) + AvgCost (must be 0 or greater if filled); saves to JSON, adds row to DataGrid.
- **Edit**: opens `EditEventWindow` pre-populated; validates same rules excluding self; preserves MapX/MapY; updates JSON + DataGrid row in-place.
- **Info**: opens `EventDetailWindow` as modal.
- **Delete**: confirms via `DeleteConfirmWindow`; removes from JSON + DataGrid.
- **Search**: opens `SearchEventWindow`; AND-filters on Name, EventType, Country, City, Description, Attendance, AvgCost (exact), IsHumanitarian; state persists between openings; closes and applies filter.
- **Filter / Reset**: FilterBox TextBox + Reset button; real-time filter by Name/Type/Country/City/Description.

### Event Types Page (`EventTypesView.xaml`)
- DataGrid: Icon, ID (`Code`), Name, Actions.
- **Add**: `AddEventTypeWindow`; validates ID + Name + Icon (required).
- **Edit**: `EditEventTypeWindow`; same validation excluding self.
- **Info**: `EventTypeDetailWindow`.
- **Delete**: `DeleteConfirmWindow` → removes from JSON + DataGrid.
- **Search**: `SearchEventTypeWindow`; AND-filters on Name + Description; persistent state.
- **Filter / Reset**: wired.

### Tags Page (`TagsView.xaml`)
- DataGrid: ID (`Code`), Color (`"Name (Hex)"` display), Actions.
- Note: Tag `Description` is loaded but **not shown** in the DataGrid.
- **Add**: `AddTagWindow`; validates ID (required, positive int, unique) + Color (required, Hex ≠ null).
- **Edit**: `EditTagWindow`; same excluding self.
- **Info**: `TagDetailWindow`.
- **Delete**: `DeleteConfirmWindow` → removes from JSON + DataGrid.
- **Search**: `SearchTagWindow`; AND-filters on Color (exact hex match) + Description; persistent state.
- **Filter / Reset**: wired.

### Detail Dialogs
- `EventDetailWindow` (580×650): two-column layout, tags as chips, falls back to `event-type.png` for orphaned EventTypeId.
- `EventTypeDetailWindow` (420×320): icon, name, ID, description.
- `TagDetailWindow` (380×340): ID, color swatch, color display string, description.
- `DeleteConfirmWindow` (380×200): shared; ctor takes message; `DialogResult = true` on Yes.

### Statistics Page (`StatisticsView.xaml`)
- Events per Type (horizontal bar chart), Event Purpose (pie chart), Events by Attendance (horizontal bar chart). All color-blind-friendly (shape/fill pattern differentiation, `#444444` bars).

### Add Windows — validation approach
All three Add ViewModels inherit `ViewModelBase` and override `ValidateProperty`. On "Add" click, the code-behind calls `vm.ValidateAllProperties()` — if `vm.HasErrors` is true, the save is blocked and error indicators appear inline. Real-time feedback also fires as the user edits fields (via `SetProperty<T>` → `ValidateProperty`).

**Validated fields per window:**
- `AddEventWindow`: EventIdText (required + positive int), Name (required), SelectedEventType (required, Id≠0), SelectedAttendance (required, IsNone=false), AverageCostText (empty=OK; if filled must be a valid number ≥ 0)
- `AddEventTypeWindow`: IdText (required + positive int), Name (required), SelectedIcon (required, FilePath≠null)
- `AddTagWindow`: IdText (required + positive int), SelectedColor (required, Hex≠null)

**"None" sentinel items**: `AttendanceItems[0]` is `{DisplayName="(Select attendance)", IsNone=true}` — the `IsNone` flag on `AttendanceComboItem` is checked in validation. `ColorItems[0]` is `{DisplayName="(Select color)", Hex=null}` — `Hex==null` is the sentinel. Both start selected by default. EditEventViewModel has its own `AttendanceItems` list without the sentinel, so Edit is unaffected.

**Error display**: each validated field has a sibling `StackPanel` beneath it (Visibility bound to `XxxHasError` via `BoolToVis` converter from `App.xaml`). It shows `"! "` (bold) + `{Binding XxxError}` (first error string). No red borders, no color-only indicators. `ValidatesOnNotifyDataErrors=False` is set on validated bindings to suppress WPF's built-in red-border mechanism.

**Uniqueness errors** (ID already exists) are set from code-behind via `vm.SetEventIdError(msg)` / `vm.SetIdError(msg)` — public wrapper methods that call the protected `SetErrors`.

**BoolToVis converter**: registered in `App.xaml` as `<BooleanToVisibilityConverter x:Key="BoolToVis"/>` — accessible from all windows.

---

## What Is NOT Done Yet

- **Back button** (`PageBackButton`) has no `Click` handler — visible on all pages but non-functional.
- **Tag `Description`** is loaded but not displayed in the Tags DataGrid.
- **MapWorkspace in-session consistency**: `MainWindowViewModel` loads once at startup. Deleting an EventType mid-session won't update the map event cards — acceptable for academic scope.

---

## Constraints to Respect (Academic Project)

- No DI containers — instantiate directly.
- No `NavigationService` or `DialogService` abstractions — navigate in code-behind, open dialogs with `new XxxWindow().ShowDialog()`.
- No external NuGet packages beyond what's already used (`System.Web.Script.Serialization` from the framework).
- No interfaces for services — `EventService` is a concrete class, used directly.
- DataContext assignment in code-behind constructor is the established pattern everywhere.
- Keep everything explainable to a professor: prefer obvious code over clever abstractions.
