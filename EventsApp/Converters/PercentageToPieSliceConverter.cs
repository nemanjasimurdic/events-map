using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;

namespace EventsApp.Converters
{
    public class PercentageToPieSliceConverter : IValueConverter
    {
        private const double Cx     = 80;
        private const double Cy     = 80;
        private const double Radius = 70;

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            double pct = (double)value;
            if (pct <= 0) return Geometry.Empty;
            if (pct >= 1) return new EllipseGeometry(new Point(Cx, Cy), Radius, Radius);

            double angle = pct * 2 * Math.PI;
            double endX  = Cx + Radius * Math.Sin(angle);
            double endY  = Cy - Radius * Math.Cos(angle);

            var fig = new PathFigure { StartPoint = new Point(Cx, Cy), IsClosed = true };
            fig.Segments.Add(new LineSegment(new Point(Cx, Cy - Radius), true));
            fig.Segments.Add(new ArcSegment(
                new Point(endX, endY),
                new Size(Radius, Radius),
                rotationAngle: 0,
                isLargeArc:    pct > 0.5,
                sweepDirection: SweepDirection.Clockwise,
                isStroked:     true));

            var geo = new PathGeometry();
            geo.Figures.Add(fig);
            return geo;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => throw new NotImplementedException();
    }
}
