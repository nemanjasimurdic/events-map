using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using EventsApp.Pages;
using EventsApp.ViewModel;

namespace EventsApp
{
    public partial class MainWindow : Window
    {
        private readonly MainWindowViewModel _vm;

        // List → Map drag state
        private Point _listDragStart;
        private EventCardItem _pendingListDrag;

        // Map → List drag state
        private readonly Dictionary<FrameworkElement, EventCardItem> _mapMarkers
            = new Dictionary<FrameworkElement, EventCardItem>();
        private FrameworkElement _markerBeingDragged;
        private EventCardItem _markerDragItem;
        private Point _markerDragStart;

        public MainWindow()
        {
            InitializeComponent();
            _vm = (MainWindowViewModel)DataContext;
            Loaded += MainWindow_Loaded;
        }

        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            foreach (var item in _vm.MapMarkerItems)
                if (item.MapX.HasValue && item.MapY.HasValue)
                    PlaceMarker(item, new Point(item.MapX.Value, item.MapY.Value));
        }

        // ── Hamburger menu ─────────────────────────────────────────────────

        private void HamburgerButton_Click(object sender, RoutedEventArgs e)
        {
            HamburgerPopup.IsOpen = !HamburgerPopup.IsOpen;
        }

        private void CloseHamburgerMenu(object sender, RoutedEventArgs e)
        {
            HamburgerPopup.IsOpen = false;
        }

        // ── Navigation ────────────────────────────────────────────────────

        private void NavigateTo(Page page, string pageTitle)
        {
            PageTitleText.Text = pageTitle;
            PageTitleText.Visibility = Visibility.Visible;
            PageBackButton.Visibility = Visibility.Visible;
            MapWorkspace.Visibility = Visibility.Collapsed;
            MainFrame.Visibility = Visibility.Visible;
            MainFrame.Navigate(page);
        }

        public void ShowMapWorkspace()
        {
            PageTitleText.Visibility = Visibility.Collapsed;
            PageBackButton.Visibility = Visibility.Collapsed;
            MainFrame.Visibility = Visibility.Collapsed;
            MapWorkspace.Visibility = Visibility.Visible;
        }

        private void EventMapTitle_Click(object sender, RoutedEventArgs e)
        {
            ShowMapWorkspace();
        }

        private void MenuEvents_Click(object sender, RoutedEventArgs e)
        {
            HamburgerPopup.IsOpen = false;
            NavigateTo(new EventsPage(), "Events");
        }

        private void MenuEventTypes_Click(object sender, RoutedEventArgs e)
        {
            HamburgerPopup.IsOpen = false;
            NavigateTo(new EventTypesPage(), "Event types");
        }

        private void MenuTags_Click(object sender, RoutedEventArgs e)
        {
            HamburgerPopup.IsOpen = false;
            NavigateTo(new TagsPage(), "Tags");
        }

        private void MenuStatistics_Click(object sender, RoutedEventArgs e)
        {
            HamburgerPopup.IsOpen = false;
            NavigateTo(new StatisticsPage(), "Statistics");
        }

        // ── Drag initiation from list ──────────────────────────────────────

        private void ListBorder_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            _listDragStart    = e.GetPosition(null);
            _pendingListDrag  = GetCardUnderMouse(e.OriginalSource as DependencyObject);
        }

        private void ListBorder_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            _pendingListDrag = null;
        }

        private void ListBorder_PreviewMouseMove(object sender, MouseEventArgs e)
        {
            if (e.LeftButton != MouseButtonState.Pressed || _pendingListDrag == null)
                return;

            var pos  = e.GetPosition(null);
            var diff = pos - _listDragStart;
            if (Math.Abs(diff.X) > SystemParameters.MinimumHorizontalDragDistance ||
                Math.Abs(diff.Y) > SystemParameters.MinimumVerticalDragDistance)
            {
                var item = _pendingListDrag;
                _pendingListDrag = null;
                DragDrop.DoDragDrop(ListBorder,
                    new DataObject("ListCard", item), DragDropEffects.Move);
            }
        }

        // ── Drop onto list (marker → list) ────────────────────────────────

        private void ListBorder_DragOver(object sender, DragEventArgs e)
        {
            e.Effects = e.Data.GetDataPresent("MapMarker")
                ? DragDropEffects.Move
                : DragDropEffects.None;
            e.Handled = true;
        }

        private void ListBorder_Drop(object sender, DragEventArgs e)
        {
            if (!e.Data.GetDataPresent("MapMarker")) return;

            var item = (EventCardItem)e.Data.GetData("MapMarker");
            _vm.EventItems.Add(item);
            _vm.MapMarkerItems.Remove(item);
            item.IsVisibleOnMap = true;

            FrameworkElement markerToRemove = null;
            foreach (var kvp in _mapMarkers)
            {
                if (kvp.Value == item) { markerToRemove = kvp.Key; break; }
            }
            if (markerToRemove != null)
            {
                MapCanvas.Children.Remove(markerToRemove);
                _mapMarkers.Remove(markerToRemove);
            }
            e.Handled = true;
        }

        // ── Drop onto map (list → map) ────────────────────────────────────

        private void MapCanvas_DragOver(object sender, DragEventArgs e)
        {
            e.Effects = (e.Data.GetDataPresent("ListCard") || e.Data.GetDataPresent("MapMarker"))
                ? DragDropEffects.Move
                : DragDropEffects.None;
            e.Handled = true;
        }

        private void MapCanvas_Drop(object sender, DragEventArgs e)
        {
            var dropPos = e.GetPosition(MapCanvas);

            if (e.Data.GetDataPresent("ListCard"))
            {
                var item = (EventCardItem)e.Data.GetData("ListCard");
                if (!IsOverlapping(dropPos))
                {
                    PlaceMarker(item, dropPos);
                    _vm.EventItems.Remove(item);
                    _vm.MapMarkerItems.Add(item);
                }
            }
            else if (e.Data.GetDataPresent("MapMarker"))
            {
                var item = (EventCardItem)e.Data.GetData("MapMarker");
                FrameworkElement panel = null;
                foreach (var kvp in _mapMarkers)
                    if (kvp.Value == item) { panel = kvp.Key; break; }
                if (panel != null && !IsOverlapping(dropPos, panel))
                    RepositionMarker(panel, dropPos);
            }
            e.Handled = true;
        }

        // ── Marker drag back to list ──────────────────────────────────────

        private void Marker_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            _markerBeingDragged = (FrameworkElement)sender;
            _markerDragItem     = _mapMarkers[_markerBeingDragged];
            _markerDragStart    = e.GetPosition(null);
            e.Handled = true;
        }

        private void Marker_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.LeftButton != MouseButtonState.Pressed || _markerBeingDragged == null)
                return;

            var pos  = e.GetPosition(null);
            var diff = pos - _markerDragStart;
            if (Math.Abs(diff.X) > SystemParameters.MinimumHorizontalDragDistance ||
                Math.Abs(diff.Y) > SystemParameters.MinimumVerticalDragDistance)
            {
                var marker = _markerBeingDragged;
                var item   = _markerDragItem;
                _markerBeingDragged = null;
                DragDrop.DoDragDrop(marker,
                    new DataObject("MapMarker", item), DragDropEffects.Move);
            }
        }

        private void Marker_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            // If _markerBeingDragged is still set, no drag was initiated — treat as a click
            if (_markerBeingDragged == null) return;
            var marker = _markerBeingDragged;
            var item   = _markerDragItem;
            _markerBeingDragged = null;
            _markerDragItem     = null;
            ShowMarkerInfoPopup(marker, item);
            e.Handled = true;
        }

        // ── Helpers ───────────────────────────────────────────────────────

        private EventCardItem GetCardUnderMouse(DependencyObject source)
        {
            var dep = source;
            while (dep != null && !(dep is ContentPresenter))
                dep = VisualTreeHelper.GetParent(dep);
            return (dep as ContentPresenter)?.DataContext as EventCardItem;
        }

        private bool IsOverlapping(Point dropPos, FrameworkElement exclude = null)
        {
            const double threshold = 40.0;
            foreach (var marker in _mapMarkers.Keys)
            {
                if (marker == exclude) continue;
                double cx = Canvas.GetLeft(marker) + 11;
                double cy = Canvas.GetTop(marker) + 11;
                double dist = Math.Sqrt(
                    Math.Pow(dropPos.X - cx, 2) + Math.Pow(dropPos.Y - cy, 2));
                if (dist < threshold) return true;
            }
            return false;
        }

        private void RepositionMarker(FrameworkElement panel, Point dropPos)
        {
            Canvas.SetLeft(panel, dropPos.X - 11);
            Canvas.SetTop(panel, dropPos.Y - 22);
        }

        private void PlaceMarker(EventCardItem item, Point dropPos)
        {
            var panel = new StackPanel { HorizontalAlignment = HorizontalAlignment.Center };

            // Bind Visibility to IsVisibleOnMap so the map filter can hide/show this marker
            panel.SetBinding(UIElement.VisibilityProperty, new Binding(nameof(EventCardItem.IsVisibleOnMap))
            {
                Source    = item,
                Converter = new BooleanToVisibilityConverter()
            });

            var img = new Image
            {
                Width = 22, Height = 22,
                HorizontalAlignment = HorizontalAlignment.Center
            };
            if (item.IconPath != null)
                img.Source = new BitmapImage(new Uri(item.IconPath, UriKind.Absolute));
            panel.Children.Add(img);

            panel.Children.Add(new TextBlock
            {
                Text = item.Name,
                FontSize = 9,
                HorizontalAlignment = HorizontalAlignment.Center,
                Background = Brushes.White,
                Padding = new Thickness(2, 1, 2, 1)
            });

            Canvas.SetLeft(panel, dropPos.X - 11);
            Canvas.SetTop(panel, dropPos.Y - 22);

            panel.MouseLeftButtonDown += Marker_MouseLeftButtonDown;
            panel.MouseMove           += Marker_MouseMove;
            panel.MouseLeftButtonUp   += Marker_MouseLeftButtonUp;

            MapCanvas.Children.Add(panel);
            _mapMarkers[panel] = item;
        }

        private void ShowMarkerInfoPopup(FrameworkElement marker, EventCardItem item)
        {
            PopupNameText.Text     = item.Name;
            PopupLocationText.Text = item.Location;
            PopupDateText.Text     = item.DateText;
            MarkerInfoPopup.PlacementTarget = marker;
            MarkerInfoPopup.IsOpen = true;
        }

        private void PopupClose_Click(object sender, RoutedEventArgs e)
        {
            MarkerInfoPopup.IsOpen = false;
        }

        private void ResetButton_Click(object sender, RoutedEventArgs e)
        {
            _vm.MapFilterText = "";
        }
    }
}
