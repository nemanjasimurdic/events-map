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

        private void NavigateTo(Page page)
        {
            MapWorkspace.Visibility = Visibility.Collapsed;
            MainFrame.Visibility = Visibility.Visible;
            MainFrame.Navigate(page);
        }

        private void MenuEvents_Click(object sender, RoutedEventArgs e)
        {
            HamburgerPopup.IsOpen = false;
            NavigateTo(new EventsPage());
        }

        private void MenuEventTypes_Click(object sender, RoutedEventArgs e)
        {
            HamburgerPopup.IsOpen = false;
            NavigateTo(new EventTypesPage());
        }

        private void MenuTags_Click(object sender, RoutedEventArgs e)
        {
            HamburgerPopup.IsOpen = false;
            NavigateTo(new TagsPage());
        }

        private void MenuStatistics_Click(object sender, RoutedEventArgs e)
        {
            HamburgerPopup.IsOpen = false;
            NavigateTo(new StatisticsPage());
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
            e.Effects = e.Data.GetDataPresent("ListCard")
                ? DragDropEffects.Move
                : DragDropEffects.None;
            e.Handled = true;
        }

        private void MapCanvas_Drop(object sender, DragEventArgs e)
        {
            if (!e.Data.GetDataPresent("ListCard")) return;

            var item    = (EventCardItem)e.Data.GetData("ListCard");
            var dropPos = e.GetPosition(MapCanvas);

            if (!IsOverlapping(dropPos))
            {
                PlaceMarker(item, dropPos);
                _vm.EventItems.Remove(item);
                _vm.MapMarkerItems.Add(item);
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

        // ── Helpers ───────────────────────────────────────────────────────

        private EventCardItem GetCardUnderMouse(DependencyObject source)
        {
            var dep = source;
            while (dep != null && !(dep is ContentPresenter))
                dep = VisualTreeHelper.GetParent(dep);
            return (dep as ContentPresenter)?.DataContext as EventCardItem;
        }

        private bool IsOverlapping(Point dropPos)
        {
            const double threshold = 40.0;
            foreach (var marker in _mapMarkers.Keys)
            {
                double cx = Canvas.GetLeft(marker) + 11;
                double cy = Canvas.GetTop(marker) + 11;
                double dist = Math.Sqrt(
                    Math.Pow(dropPos.X - cx, 2) + Math.Pow(dropPos.Y - cy, 2));
                if (dist < threshold) return true;
            }
            return false;
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
            {
                var uri = new Uri("pack://application:,,,/" +
                    item.IconPath.TrimStart('/'), UriKind.Absolute);
                img.Source = new BitmapImage(uri);
            }
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

            MapCanvas.Children.Add(panel);
            _mapMarkers[panel] = item;
        }

        private void ResetButton_Click(object sender, RoutedEventArgs e)
        {
            _vm.MapFilterText = "";
        }
    }
}
