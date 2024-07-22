//using System;
//using System.Collections.Generic;
//using System.Formats.Asn1;
//using System.Globalization;
//using System.IO;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;
//using Emgu.CV;
//using Emgu.CV.Structure;


//namespace EmoShift
//{
//    public class HeartRate
//    {
//        private VideoCapture _video;
//        private double _frameRate;
//        private string _person;
//        private int _framesPassed;
//        private List<double> _times;
//        private int _bufSize;
//        private const int MinBpm = 40;
//        private const int MaxBpm = 220;
//        private List<double> _heartRates;
//        private string _outputDir;

//        //public HeartRate(VideoCapture vid, string outputDir, string person, double frameRate = 0)
//        //{
//        //    _video = vid;
//        //    _frameRate = (vid != null) ? vid.GetCaptureProperty(Emgu.CV.CvEnum.CapProp.Fps) : frameRate;
//        //    _person = person;
//        //    _framesPassed = 0;
//        //    _times = new List<double>();
//        //    _bufSize = (int)(4 * _frameRate);
//        //    _minBpm = 40;
//        //    _maxBpm = 220;
//        //    _heartRates = new List<double>();
//        //    _outputDir = outputDir;
//        //}

//        //~HeartRate()
//        //{
//        //    if (_video != null)
//        //    {
//        //        _video.Dispose();
//        //    }
//        //}

//        public double GetHeartRate(Mat frame, int framesPassed, int frameRate)
//        {
//            double heartrate = 0.0;
//            Emgu.CV.Image<Rgb, byte> imgHRV = frame.ToImage<Rgb, byte>();


//            try
//            {
//                double seconds = framesPassed / frameRate;
//                _times.Add(seconds);

//                // Assuming green_rect = fr for simplification

//                Rgb rgbAvg;
//                MCvScalar vScalar;
//                imgHRV.AvgSdv(out rgbAvg, out vScalar); //frame.GetAverage().Intensity;
//                List<double> times = new List<double>();

//                if (times.Count > _bufSize)
//                {
//                    times = times.Skip(times.Count - _bufSize).ToList();
//                }

//                //I STOPPED HERE
//                var times = _times.ToArray();
//                //var vals = DATA["greenVals"].ToArray();
//                if (times.Length > 0 && vals.Length > 0)
//                {
//                    var L = vals.Length;
//                    var processed = vals.ToArray();
//                    if (L == _bufSize)
//                    {
//                        var fps = L / (times.Last() - times.First());
//                        var even_times = MathNet.Numerics.Interpolate.LinearSpaced(L, times.First(), times.Last()).ToArray();

//                        processed = MathNet.Numerics.SignalProcessing.Filtering.OnlineFilter.Detrend(processed);
//                        processed = MathNet.Numerics.SignalProcessing.Filtering.IirFilter.BandPass(processed, 0.8, 3, fps);
//                        var interpolated = MathNet.Numerics.Interpolate.LinearInterpolation.InterpolateSorted(times, processed, even_times);
//                        interpolated = MathNet.Numerics.Window.Hamming(L).Select((x, i) => x * interpolated[i]).ToArray();
//                        var norm = MathNet.Numerics.NormalizeL2(interpolated);
//                        var raw = Fourier.Forward(norm).Select(x => x.MagnitudeSquared()).ToArray();
//                        var freqs = MathNet.Numerics.Generate.LinearSpaced(L / 2 + 1, 0, fps / 2).Select(x => x * 60).ToArray();
//                        var idx = Enumerable.Range(0, freqs.Length).Where(i => freqs[i] > MinBpm && freqs[i] < MaxBpm).ToArray();
//                        var pruned = idx.Select(i => raw[i]).ToArray();
//                        var pfreq = idx.Select(i => freqs[i]).ToArray();
//                        freqs = pfreq;
//                        raw = pruned;
//                        if (pruned.Length > 0)
//                        {
//                            var idx2 = Array.IndexOf(pruned, pruned.Max());
//                            heartrate = freqs[idx2];
//                        }
//                    }
//                }

//                _heartRates.Add(_heartRates.TakeLast(19).Concat(new double[] { heartrate }).Average());
//                //COLLECTED.Add(new Dictionary<string, double> { { "time", seconds }, { "green", avg_green }, { "bpm", _heartRates.TakeLast(20).Average() } });
//            }
//            catch (Exception e)
//            {
//                Console.WriteLine($"Frame {_framesPassed}: {e.Message}");
//            }

//            _framesPassed++;
//            return heartrate;
//        }

//        //public void SavePlot()
//        //{
//        //    var justBpms = COLLECTED.Select(point => point["bpm"]).ToList();
//        //    var avg = justBpms.Mean();
//        //    var std = justBpms.StandardDeviation();

//        //    using (var writer = new StreamWriter(Path.Combine(_outputDir, $"outputs/hrv_{_person.Split('.')[0]}.csv")))
//        //    using (var csv = new CsvWriter(writer, CultureInfo.InvariantCulture))
//        //    {
//        //        csv.WriteHeader(typeof(Dictionary<string, double>));
//        //        foreach (var point in COLLECTED)
//        //        {
//        //            csv.WriteRecord(new Dictionary<string, double> { { "Time", Math.Floor(point["time"] * 100) / 100 }, { "BPM", point["bpm"] }, { "Average", avg }, { "SD above", avg + std }, { "SD below", avg - std } });
//        //            csv.NextRecord();
//        //        }
//        //    }
//        //}
//    }
//}
