﻿using System;
using Windows.Foundation;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Shapes;

namespace XamlBrewer.Uwp.Controls
{
    /// <summary>
    /// A Percentage Ring Control.
    /// </summary>
    //// All calculations are for a 200x200 square. The ViewBox control will do the rest.
    [TemplatePart(Name = ContainerPartName, Type = typeof(Grid))]
    [TemplatePart(Name = ScalePartName, Type = typeof(Path))]
    [TemplatePart(Name = TrailPartName, Type = typeof(Path))]
    [TemplatePart(Name = ValueTextPartName, Type = typeof(TextBlock))]
    public class PercentageRing : Control
    {
        /// <summary>
        /// Identifies the optional StepSize property.
        /// </summary>
        public static readonly DependencyProperty StepSizeProperty =
            DependencyProperty.Register(nameof(StepSize), typeof(double), typeof(PercentageRing), new PropertyMetadata(0.0));

        // Identifies the IsInteractive dependency property.
        public static readonly DependencyProperty IsInteractiveProperty =
            DependencyProperty.Register(nameof(IsInteractive), typeof(bool), typeof(PercentageRing), new PropertyMetadata(false, OnInteractivityChanged));

        /// <summary>
        /// Identifies the ScaleWidth dependency property.
        /// </summary>
        public static readonly DependencyProperty ScaleWidthProperty =
            DependencyProperty.Register(nameof(ScaleWidth), typeof(double), typeof(PercentageRing), new PropertyMetadata(25.0, OnScaleChanged));

        /// <summary>
        /// Identifies the Value dependency property.
        /// </summary>
        public static readonly DependencyProperty ValueProperty =
            DependencyProperty.Register(nameof(Value), typeof(double), typeof(PercentageRing), new PropertyMetadata(0.0, OnValueChanged));

        /// <summary>
        /// Identifies the TrailBrush dependency property.
        /// </summary>
        public static readonly DependencyProperty TrailBrushProperty =
            DependencyProperty.Register(nameof(TrailBrush), typeof(Brush), typeof(PercentageRing), new PropertyMetadata(new SolidColorBrush(Colors.Orange)));

        /// <summary>
        /// Identifies the ScaleBrush dependency property.
        /// </summary>
        public static readonly DependencyProperty ScaleBrushProperty =
            DependencyProperty.Register(nameof(ScaleBrush), typeof(Brush), typeof(PercentageRing), new PropertyMetadata(new SolidColorBrush(Colors.DarkGray)));

        /// <summary>
        /// Identifies the ValueBrush dependency property.
        /// </summary>
        public static readonly DependencyProperty ValueBrushProperty =
            DependencyProperty.Register(nameof(ValueBrush), typeof(Brush), typeof(PercentageRing), new PropertyMetadata(new SolidColorBrush(Colors.Black)));

        /// <summary>
        /// Identifies the ValueStringFormat dependency property.
        /// </summary>
        public static readonly DependencyProperty ValueStringFormatProperty =
            DependencyProperty.Register(nameof(ValueStringFormat), typeof(string), typeof(PercentageRing), new PropertyMetadata("0\\%"));

        /// <summary>
        /// Identifies the MinAngle dependency property.
        /// </summary>
        public static readonly DependencyProperty MinAngleProperty =
            DependencyProperty.Register(nameof(MinAngle), typeof(int), typeof(PercentageRing), new PropertyMetadata(0, OnScaleChanged));

        /// <summary>
        /// Identifies the MaxAngle dependency property.
        /// </summary>
        public static readonly DependencyProperty MaxAngleProperty =
            DependencyProperty.Register(nameof(MaxAngle), typeof(int), typeof(PercentageRing), new PropertyMetadata(360, OnScaleChanged));

        /// <summary>
        /// Identifies the ValueAngle dependency property.
        /// </summary>
        protected static readonly DependencyProperty ValueAngleProperty =
            DependencyProperty.Register(nameof(ValueAngle), typeof(double), typeof(PercentageRing), new PropertyMetadata(null));

        // Template Parts.
        private const string ContainerPartName = "PART_Container";
        private const string ScalePartName = "PART_Scale";
        private const string TrailPartName = "PART_Trail";
        private const string ValueTextPartName = "PART_ValueText";

        // For convenience.
        private const double Degrees2Radians = Math.PI / 180;
        private const double Minimum = 0;
        private const double Maximum = 100;
        private const double ScalePadding = 0;

        private double _normalizedMinAngle;
        private double _normalizedMaxAngle;

        /// <summary>
        /// Initializes a new instance of the <see cref="PercentageRing"/> class.
        /// </summary>
        public PercentageRing()
        {
            DefaultStyleKey = typeof(PercentageRing);
        }

        /// <summary>
        /// Gets or sets the rounding interval for the Value.
        /// </summary>
        public double StepSize
        {
            get { return (double)GetValue(StepSizeProperty); }
            set { SetValue(StepSizeProperty, value); }
        }

        /// <summary>
        /// Gets or sets a value indicating whether the control accepts setting its value through interaction.
        /// </summary>
        public bool IsInteractive
        {
            get { return (bool)GetValue(IsInteractiveProperty); }
            set { SetValue(IsInteractiveProperty, value); }
        }

        /// <summary>
        /// Gets or sets the width of the scale, in percentage of the radius.
        /// </summary>
        public double ScaleWidth
        {
            get { return (double)GetValue(ScaleWidthProperty); }
            set { SetValue(ScaleWidthProperty, value); }
        }

        /// <summary>
        /// Gets or sets the current value.
        /// </summary>
        public double Value
        {
            get { return (double)GetValue(ValueProperty); }
            set { SetValue(ValueProperty, value); }
        }

        /// <summary>
        /// Gets or sets the trail brush.
        /// </summary>
        public Brush TrailBrush
        {
            get { return (Brush)GetValue(TrailBrushProperty); }
            set { SetValue(TrailBrushProperty, value); }
        }

        /// <summary>
        /// Gets or sets the scale brush.
        /// </summary>
        public Brush ScaleBrush
        {
            get { return (Brush)GetValue(ScaleBrushProperty); }
            set { SetValue(ScaleBrushProperty, value); }
        }

        /// <summary>
        /// Gets or sets the brush for the displayed value.
        /// </summary>
        public Brush ValueBrush
        {
            get { return (Brush)GetValue(ValueBrushProperty); }
            set { SetValue(ValueBrushProperty, value); }
        }

        /// <summary>
        /// Gets or sets the value string format.
        /// </summary>
        public string ValueStringFormat
        {
            get { return (string)GetValue(ValueStringFormatProperty); }
            set { SetValue(ValueStringFormatProperty, value); }
        }

        /// <summary>
        /// Gets or sets the start angle of the scale, which corresponds with the Minimum value, in degrees. It's typically on the right hand side of the control. The proposed value range is from -180 (bottom) to 0° (top).
        /// </summary>
        public int MinAngle
        {
            get { return (int)GetValue(MinAngleProperty); }
            set { SetValue(MinAngleProperty, value); }
        }

        /// <summary>
        /// Gets or sets the end angle of the scale, which corresponds with the Maximum value, in degrees. It 's typically on the left hand side of the control. The proposed value range is from 0° (top) to 180° (bottom).
        /// </summary>
        public int MaxAngle
        {
            get { return (int)GetValue(MaxAngleProperty); }
            set { SetValue(MaxAngleProperty, value); }
        }

        /// <summary>
        /// Gets or sets the current angle of the needle (between MinAngle and MaxAngle). Setting the angle will update the Value.
        /// </summary>
        protected double ValueAngle
        {
            get { return (double)GetValue(ValueAngleProperty); }
            set { SetValue(ValueAngleProperty, value); }
        }

        /// <summary>
        /// Gets the normalized minimum angle.
        /// </summary>
        /// <value>The minimum angle in the range from -180 to 180.</value>
        protected double NormalizedMinAngle => _normalizedMinAngle;

        /// <summary>
        /// Gets the normalized maximum angle.
        /// </summary>
        /// <value>The maximum angle in the range from 180 to 540.</value>
        protected double NormalizedMaxAngle => _normalizedMaxAngle;

        /// <summary>
        /// Update the visual state of the control when its template is changed.
        /// </summary>
        protected override void OnApplyTemplate()
        {
            OnScaleChanged(this);

            base.OnApplyTemplate();
        }

        private static void OnValueChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            OnValueChanged(d);
        }

        private static void OnValueChanged(DependencyObject d)
        {
            var percentageRing = (PercentageRing)d;
            if (!double.IsNaN(percentageRing.Value))
            {
                if (percentageRing.StepSize > 0)
                {
                    percentageRing.Value = percentageRing.RoundToMultiple(percentageRing.Value, percentageRing.StepSize);
                }

                var middleOfScale = 100 - ScalePadding - (percentageRing.ScaleWidth / 2);
                var valueText = percentageRing.GetTemplateChild(ValueTextPartName) as TextBlock;
                percentageRing.ValueAngle = percentageRing.ValueToAngle(percentageRing.Value);

                // Trail
                var trail = percentageRing.GetTemplateChild(TrailPartName) as Path;
                if (trail != null)
                {
                    if (percentageRing.ValueAngle > percentageRing.NormalizedMinAngle)
                    {
                        trail.Visibility = Visibility.Visible;

                        if (percentageRing.ValueAngle - percentageRing.NormalizedMinAngle == 360)
                        {
                            // Draw full circle.
                            var eg = new EllipseGeometry
                            {
                                Center = new Point(100, 100),
                                RadiusX = 100 - ScalePadding - (percentageRing.ScaleWidth / 2)
                            };

                            eg.RadiusY = eg.RadiusX;
                            trail.Data = eg;
                        }
                        else
                        {
                            // Draw arc.
                            var pg = new PathGeometry();
                            var pf = new PathFigure
                            {
                                IsClosed = false,
                                StartPoint = percentageRing.ScalePoint(percentageRing.NormalizedMinAngle, middleOfScale)
                            };

                            var seg = new ArcSegment
                            {
                                SweepDirection = SweepDirection.Clockwise,
                                IsLargeArc = percentageRing.ValueAngle > (180 + percentageRing.NormalizedMinAngle),
                                Size = new Size(middleOfScale, middleOfScale),
                                Point =
                                    percentageRing.ScalePoint(
                                        Math.Min(percentageRing.ValueAngle, percentageRing.NormalizedMaxAngle), middleOfScale)
                            };

                            pf.Segments.Add(seg);
                            pg.Figures.Add(pf);
                            trail.Data = pg;
                        }
                    }
                    else
                    {
                        trail.Visibility = Visibility.Collapsed;
                    }
                }

                // Value Text
                if (valueText != null)
                {
                    valueText.Text = percentageRing.Value.ToString(percentageRing.ValueStringFormat);
                }
            }
        }

        private static void OnInteractivityChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var percentageRing = (PercentageRing)d;

            if (percentageRing.IsInteractive)
            {
                percentageRing.Tapped += percentageRing.PercentageRing_Tapped;
                percentageRing.ManipulationDelta += percentageRing.PercentageRing_ManipulationDelta;
                percentageRing.ManipulationMode = ManipulationModes.TranslateX | ManipulationModes.TranslateY;
            }
            else
            {
                percentageRing.Tapped -= percentageRing.PercentageRing_Tapped;
                percentageRing.ManipulationDelta -= percentageRing.PercentageRing_ManipulationDelta;
                percentageRing.ManipulationMode = ManipulationModes.None;
            }
        }

        private static void OnScaleChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            OnScaleChanged(d);
        }

        private static void OnScaleChanged(DependencyObject d)
        {
            var percentageRing = (PercentageRing)d;

            percentageRing.UpdateNormalizedAngles();

            var scale = percentageRing.GetTemplateChild(ScalePartName) as Path;
            if (scale != null)
            {
                if (percentageRing.NormalizedMaxAngle - percentageRing.NormalizedMinAngle == 360)
                {
                    // Draw full circle.
                    var eg = new EllipseGeometry
                    {
                        Center = new Point(100, 100),
                        RadiusX = 100 - ScalePadding - (percentageRing.ScaleWidth / 2)
                    };

                    eg.RadiusY = eg.RadiusX;
                    scale.Data = eg;
                }
                else
                {
                    // Draw arc.
                    var pg = new PathGeometry();
                    var pf = new PathFigure { IsClosed = false };
                    var middleOfScale = 100 - ScalePadding - (percentageRing.ScaleWidth / 2);
                    pf.StartPoint = percentageRing.ScalePoint(percentageRing.NormalizedMinAngle, middleOfScale);
                    var seg = new ArcSegment
                    {
                        SweepDirection = SweepDirection.Clockwise,
                        IsLargeArc = percentageRing.NormalizedMaxAngle > (percentageRing.NormalizedMinAngle + 180),
                        Size = new Size(middleOfScale, middleOfScale),
                        Point = percentageRing.ScalePoint(percentageRing.NormalizedMaxAngle, middleOfScale)
                    };

                    pf.Segments.Add(seg);
                    pg.Figures.Add(pf);
                    scale.Data = pg;
                }
            }

            OnValueChanged(percentageRing);
        }

        private void UpdateNormalizedAngles()
        {
            var result = Mod(MinAngle, 360);

            if (result >= 180)
            {
                result = result - 360;
            }

            _normalizedMinAngle = result;

            result = Mod(MaxAngle, 360);

            if (result < 180)
            {
                result = result + 360;
            }

            if (result > NormalizedMinAngle + 360)
            {
                result = result - 360;
            }

            _normalizedMaxAngle = result;
        }

        private void PercentageRing_ManipulationDelta(object sender, ManipulationDeltaRoutedEventArgs e)
        {
            SetValueFromPoint(e.Position);
        }

        private void PercentageRing_Tapped(object sender, TappedRoutedEventArgs e)
        {
            SetValueFromPoint(e.GetPosition(this));
        }

        private void SetValueFromPoint(Point p)
        {
            var pt = new Point(p.X - (ActualWidth / 2), -p.Y + (ActualHeight / 2));

            var angle = Math.Atan2(pt.X, pt.Y) / Degrees2Radians;
            var divider = Mod(NormalizedMaxAngle - NormalizedMinAngle, 360);
            if (divider == 0)
            {
                divider = 360;
            }

            var value = Minimum + ((Maximum - Minimum) * (Mod(angle - NormalizedMinAngle, 360)) / divider);
            if (value < Minimum || value > Maximum)
            {
                // Ignore positions outside the scale angle.
                return;
            }

            Value = value;
        }

        private Point ScalePoint(double angle, double middleOfScale)
        {
            return new Point(100 + (Math.Sin(Degrees2Radians * angle) * middleOfScale), 100 - (Math.Cos(Degrees2Radians * angle) * middleOfScale));
        }

        private double ValueToAngle(double value)
        {
            // Off-scale on the left.
            if (value < Minimum)
            {
                return MinAngle;
            }

            // Off-scale on the right.
            if (value > Maximum)
            {
                return MaxAngle;
            }

            return ((value - Minimum) / (Maximum - Minimum) * (NormalizedMaxAngle - NormalizedMinAngle)) + NormalizedMinAngle;
        }

        private double RoundToMultiple(double number, double multiple)
        {
            var modulo = number % multiple;
            if ((multiple - modulo) <= modulo)
            {
                modulo = multiple - modulo;
            }
            else
            {
                modulo *= -1;
            }

            return number + modulo;
        }

        private static double Mod(double number, double divider)
        {
            var result = number % divider;
            result = result < 0 ? result + divider : result;
            return result;
        }
    }
}
