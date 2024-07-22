using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using LiveChartsCore;
using LiveChartsCore.Defaults;
using LiveChartsCore.Measure;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.Extensions;
using LiveChartsCore.SkiaSharpView.Drawing;
using LiveChartsCore.SkiaSharpView.Painting;
using LiveChartsCore.SkiaSharpView.VisualElements;
using LiveChartsCore.VisualElements;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;


namespace EmoShift.models
{
    internal partial class ChartModels
    {

        //FER Guage
        private static readonly int sectionsOuter = 130;
        private static readonly int sectionsWidth = 20;

        public IEnumerable<ISeries> EmoAngularModelSeries { get; set; }
                = GaugeGenerator.BuildAngularGaugeSections(
                        new GaugeItem(3, s => SetStyle(sectionsOuter, sectionsWidth, s, new SolidColorPaint(new SKColor(255, 51, 51, 255)))),
                        new GaugeItem(1, s => SetStyle(sectionsOuter, sectionsWidth, s, new SolidColorPaint(new SKColor(255, 255, 51, 255)))),
                        new GaugeItem(3, s => SetStyle(sectionsOuter, sectionsWidth, s, new SolidColorPaint(new SKColor(153, 255, 51, 255)))));

        public static NeedleVisual EmoAngularModelNeedle { get; set; }
            = new NeedleVisual
            {
                Value = 5
            };

        public IEnumerable<VisualElement<SkiaSharpDrawingContext>> EmoAngularModelVisualElements { get; set; }
            = new VisualElement<SkiaSharpDrawingContext>[]
            {
                new AngularTicksVisual
                {
                    LabelsSize = 16,
                    LabelsOuterOffset = 15,
                    OuterOffset = 65,
                    TicksLength = 20
                },
                EmoAngularModelNeedle
            };


        public void UpdateFERGuage(int value)
        {
            // modifying the Value property updates and animates the chart automatically
            EmoAngularModelNeedle.Value = value;
        }

        private static void SetStyle(
            double sectionsOuter, double sectionsWidth, PieSeries<ObservableValue> series, SolidColorPaint color)
        {
            series.OuterRadiusOffset = sectionsOuter;
            series.MaxRadialColumnWidth = sectionsWidth;
            series.Fill = color;
        }

        ////////////////////////////
        ///Emotional Shift Chart

        public static ObservableCollection<double> FERobservableValues { get; set; }
            = new ObservableCollection<double>
            {
                5 //Start with 5, neutral
            };

        public ISeries[] EmoShiftSeries { get; set; }
            = new ISeries[] {
                //[0] = FER
                new LineSeries<double>
                {
                    Values = FERobservableValues,
                    Fill = null,
                }
            };
        ////////////////////////////
        ///Emotional Prob Shift Chart


        public void UpdateEmotionProbValues(float[] value) {
            EmotionProbObservableValues[0] = value[0];
            EmotionProbObservableValues[1] = value[1];
            EmotionProbObservableValues[2] = value[2];
            EmotionProbObservableValues[3] = value[3];
            EmotionProbObservableValues[4] = value[4];
            EmotionProbObservableValues[5] = value[5];
            EmotionProbObservableValues[6] = value[6];
            EmotionProbObservableValues[7] = value[7];
        }

        public static ObservableCollection<float> EmotionProbObservableValues { get; set; }
            = new ObservableCollection<float>
            {
                5.0F, 5.0F, 5.0F, 5.0F, 5.0F, 5.0F, 5.0F, 5.0F
            };

        public ISeries[] EmotionProbSeries { get; set; } =
        {
            new ColumnSeries<float>
            {
                Name = "Probability",
                Values = EmotionProbObservableValues
            }
        };

        public Axis[] EmotionProbXAxes { get; set; } =
        {
            new Axis
            {
                Labels = new string[] { "Anger", "Contempt", "Disgust", "Fear", "Happy", "Neutral", "Sad", "Surprise" },
                LabelsRotation = 0,
                SeparatorsPaint = new SolidColorPaint(new SKColor(200, 200, 200)),
                SeparatorsAtCenter = false,
                TicksPaint = new SolidColorPaint(new SKColor(35, 35, 35)),
                TicksAtCenter = true,
                // By default the axis tries to optimize the number of 
                // labels to fit the available space, 
                // when you neeed to force the axis to show all the labels then you must: 
                ForceStepToMin = true,
                MinStep = 1
            }
        };

        ////////////////////////////
        ///Heart Rate Chart




        ////////////////////////////
        ///SpeechTotext/SER
        //public static string RecognitionText { get; set; }
    }
}
