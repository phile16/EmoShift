using CommunityToolkit.Maui.Media;
using DlibDotNet;
using DlibDotNet.Extensions;
using System;
using System.Drawing;
using System.IO;
using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV.Face;
using Emgu.CV.Structure;
using Emgu.Util;
using FaceRecognitionDotNet;
using FaceRecognitionDotNet.Extensions;
using LiveChartsCore;
using LiveChartsCore.SkiaSharpView;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Layouts;
using Microsoft.Maui.Storage;

using System.Globalization;
using System.Threading;
using System.Xml.Linq;

using EmoShift.models;
using System.Collections.ObjectModel;
using SkiaSharp;



#if WINDOWS
using Microsoft.UI;
using Microsoft.UI.Windowing;
using Windows.Graphics;
#endif

namespace EmoShift
{

    public partial class MainPage : ContentPage
    {

        bool _recording = false;
        int frameCounter = 0;
        static int _fps = Preferences.Default.Get("videoFPS", 24);
        static int _EmoShiftDiffAlert = Preferences.Default.Get("shiftAlert", 24);

        VideoCapture _capture = new VideoCapture();
        // TODO: Save this file somewhere with some unique name
        VideoWriter _videoWriter = new VideoWriter(Preferences.Default.Get("saveFileLocation", Directory.GetCurrentDirectory()) + "/test.avi", _fps, new System.Drawing.Size(640, 480), true);

        // TODO: grab the dat/xml from kaleidoscope
        CascadeClassifier faceCascade = new CascadeClassifier("hrv/haarcascade_frontalface_default.xml");
        CascadeClassifier handCascade = new CascadeClassifier("hand/haarcascade_hand.xml");

        FaceAnalysis faceAnalysis = new FaceAnalysis("face/shape_predictor_68_face_landmarks.dat");
        HeadMotionAnalysis headMotionAnalysis = new HeadMotionAnalysis("hrv/haarcascade_frontalface_default.xml", "face/shape_predictor_68_face_landmarks.dat");
        SimpleEmotionEstimator emotionEstimator = new SimpleEmotionEstimator("emotion/Corrective_re-annotation_of_FER_CK+_KDEF-cnn_300_0.001_1E-05_256_train_best_0.944568452380952.dat");
        FaceRecognition faceRecognition = FaceRecognition.Create("face/");

        //Speech Analysis
//        SpeechAnalysis speechAnalysis = new SpeechAnalysis();
        // Define the cancellation token.

        //CancellationToken token;
        private ISpeechToText speechToText;
        private CancellationTokenSource tokenSource = new CancellationTokenSource();
        public Command ListenCommand { get; set; }
        public Command ListenCancelCommand { get; set; }
        public string RecognitionText { get; set; }
        Thread listenThread;

        public MainPage()
        {
            InitializeComponent();

            this.speechToText = speechToText;

            ListenCommand = new Command(Listen);
            ListenCancelCommand = new Command(ListenCancel);
            BindingContext = this;

        }

        private async void recordVideo()
        {
            Mat frame = new Mat();
            _capture = new VideoCapture();
            var prevEmoScale = 5; //Start at neutral

            while (_recording)
            {
                _capture.Read(frame);

                //Reuseable grayframe
                Mat grayFrame = new Mat();
                CvInvoke.CvtColor(frame, grayFrame, ColorConversion.Bgr2Gray);
                Bitmap bitmapGrayFrame = grayFrame.ToBitmap();
                bitmapGrayFrame.Save("grayframe.png");

                //Reusable bitmapFrame and arrayImage
                string tmpImageName = "frame_" + string.Format("{0,6:00000}", frameCounter) + ".png";
                Bitmap bitmapFrame = frame.ToBitmap();
                bitmapFrame.Save(tmpImageName);
                var arrayImage = Dlib.LoadImage<BgrPixel>(tmpImageName);

                //Check interval (currently 1 a second)
                if (frameCounter % _fps == 0)
                {
                    string EmoLoglineText = frameCounter.ToString() + ": ";

                    //Do Humate frame by frame analysis Here!
                    //TODO: should probably set all analysis to run async with frame number

                    // DETECT FACES
                    System.Drawing.Rectangle[] faces = faceCascade.DetectMultiScale(frame, 1.1, 3, System.Drawing.Size.Empty);
                    // DRAW RECT AROUND FACE
                    int i = 0;
                    if (Preferences.Default.Get("showFaceRect", false))
                    {
                        System.Drawing.Rectangle[] faceRect = new System.Drawing.Rectangle[faces.Length];
                        foreach (var faceRectTmp in faces)
                        {
                            CvInvoke.Rectangle(frame, faceRectTmp, new Bgr(System.Drawing.Color.Red).MCvScalar, 2);
                            faceRect[i] = faceRectTmp;
                            i++;
                        }
                    }

                    // IS BLINKING
                    EmoLoglineText += "Blinking=" + faceAnalysis.IsBlinking(ref frame, arrayImage, Preferences.Default.Get("showFaceLandmarks", false)).ToString() + ", ";

                    // IS YAWNING
                    Dictionary<string, string> yawning = headMotionAnalysis.IsYawning(frame, grayFrame);
                    EmoLoglineText += ", Eyes=" + yawning["eyes"];
                    EmoLoglineText += ", EyesAR=" + yawning["eyesAR"];
                    EmoLoglineText += ", HeadTilt=" + yawning["head_tilt"];
                    EmoLoglineText += ", Mouth=" + yawning["mouth"];
                    EmoLoglineText += ", MouthAR=" + yawning["mouthAR"];
                    EmoLoglineText += ", Yawn=" + yawning["yawn"];
                    if ((yawning["mouthAR"]) != "None")
                        MouthEllipse.HeightRequest = MouthEllipse.WidthRequest * float.Parse(yawning["mouthAR"]);
                    else
                        MouthEllipse.HeightRequest = MouthEllipse.WidthRequest * .33;

                    if ((yawning["eyesAR"]) != "None")
                        EyeEllipse.HeightRequest = EyeEllipse.WidthRequest * float.Parse(yawning["eyesAR"]);
                    else
                        EyeEllipse.HeightRequest = EyeEllipse.WidthRequest * .33;



                    // DETECT HANDS
                    System.Drawing.Size minSlideWindow = new System.Drawing.Size(30, 30);
                    System.Drawing.Rectangle[] hands = handCascade.DetectMultiScale(frame, 1.05, 6, minSlideWindow);
                    // DRAW RECT AROUND HANDS
                    System.Drawing.Rectangle[] handRect = new System.Drawing.Rectangle[hands.Length];
                    i = 0;
                    foreach (var handRectTmp in hands)
                    {
                        if (Preferences.Default.Get("showHandRect", false))
                        {
                            CvInvoke.Rectangle(frame, handRectTmp, new Bgr(System.Drawing.Color.Yellow).MCvScalar, 2);
                        }
                        handRect[i] = handRectTmp;
                        EmoLoglineText += ", Hands[" + i + "]=" + handRect[i].Top.ToString() + ", " + handRect[i].Right.ToString();
                        i++;
                    }

                    //FER - Face Emo Recognition
                    FaceRecognitionDotNet.Image frImage = FaceRecognition.LoadImage(bitmapFrame);
                    //Hog is much faster than CNN
                    ChartModels models = new ChartModels();
                    foreach (var frLocations in faceRecognition.FaceLocations(frImage, 1, Model.Hog))
                    {
                        faceRecognition.CustomEmotionEstimator = emotionEstimator;
                        //var emotion = faceRecognition.PredictEmotion(frImage, frLocations);
                        var emotionProb = faceRecognition.PredictProbabilityEmotion(frImage, frLocations);
                        var emotion = emotionProb.OrderByDescending(kvp => kvp.Value).First().Key;
                        var emoScale = Helper.FERScale[emotion];
                        //Angular guage
                        models.UpdateFERGuage(emoScale);
                        //Emo over time graph
                        ChartModels.FERobservableValues.Add(emoScale);
                        //Bar charts
                        models.UpdateEmotionProbValues([.. emotionProb.Values]);
                        //_EmoShiftDiffAlert
                        if (Math.Abs(prevEmoScale - emoScale) >= _EmoShiftDiffAlert)
                        {
                            if ((prevEmoScale - emoScale) >= 0)
                                EmoBorder.Background = Microsoft.Maui.Controls.Brush.Red;
                            else
                                EmoBorder.Background = Microsoft.Maui.Controls.Brush.Green;
                        }
                        else {
                            EmoBorder.Background = Microsoft.Maui.Controls.Brush.Gray;
                        }


                        prevEmoScale = emoScale;

                        EmoLoglineText += ", Emotion=" + emotion;
                    }






                    //Unique File based
                    string tmpImageNameA = "frame_" + string.Format("{0,6:00000}", frameCounter) + ".png";
                    Bitmap bitmapFrameA = frame.ToBitmap();
                    bitmapFrameA.Save(tmpImageNameA);
                    VideoImg.Source = ImageSource.FromFile(tmpImageNameA);

                    //Convert the Mat frame to a Bitmap Stream for display in image control
                    //try
                    //{
                    //    using (Bitmap bitmapFrameSave = frame.ToBitmap())
                    //    {
                    //        // Create an ImageSource from the Bitmap
                    //        var imageSource = ImageSource.FromStream(() =>
                    //        {
                    //            var imgStream = new MemoryStream();
                    //            //TODO: will need to add OS specific commands for this
                    //            bitmapFrameSave.Save(stream: imgStream, format: System.Drawing.Imaging.ImageFormat.Png);
                    //            imgStream.Seek(0, SeekOrigin.Begin);
                    //            return imgStream;
                    //        });
                    //        VideoImg.Source = imageSource;
                    //        bitmapFrameSave.Dispose();
                    //    }
                    //}
                    //catch
                    //{
                    //    continue;
                    //}


                    //Adding frame to video
                    _videoWriter.Write(frame);


                    EmoLoglineText += "\n";
                    EmoLog.Text = EmoLoglineText + EmoLog.Text;
                }
                // Needs this to interrupt
                // Checking frame every 1 sec... CACI security doesn't like it going too fast
                Emgu.CV.CvInvoke.WaitKey((1 / _fps) * 1000);




#if WINDOWS
                            Console.WriteLine("WINDOWS");
#endif
#if MACCATALYST
                Console.WriteLine("MACCATALYST");
#endif
#if ANDROID
                            Console.WriteLine("ANDROID");
#endif
#if IOS
                    Console.WriteLine("IOS");
#endif


                frameCounter++;
            }
        }

        private void OnRecordBtnClicked(object sender, EventArgs e)
        {
            _recording = !_recording;
            if (_recording)
            {
                RecordBtn.Text = "Stop Recording";
                
                //Threaded SpeechToText
                listenThread = new Thread(Listen);
                listenThread.Start();
                //////////////////////////////////
                
                _videoWriter = new VideoWriter(Preferences.Default.Get("saveFileLocation", Directory.GetCurrentDirectory()) + "/test.avi", _fps, new System.Drawing.Size(640, 480), true);
                recordVideo();
            }
            else
            {
                RecordBtn.Text = "Start Recording";
                ListenCancel();

                _capture.Dispose();
                _videoWriter.Dispose();
            }
        }

        private void OnEditorTextChanged(object sender, TextChangedEventArgs e)
        {

        }

        private void OnEditorCompleted(object sender, EventArgs e)
        {

        }

        private void OnShowSystemSettingsClicked(object sender, EventArgs e)
        {
            this.Navigation.PushAsync(new SystemSettings());
        }

        private void OnDrawSettingsClicked(object sender, EventArgs e)
        {
            this.Navigation.PushAsync(new DrawSettings());
        }

        private async void Listen()
        {
            var isAuthorized = await SpeechToText.RequestPermissions();
            if (isAuthorized)
            {
                try
                {
                    var RecognitionTextResult = await SpeechToText.ListenAsync(CultureInfo.GetCultureInfo("en-us"),
                        new Progress<string>(partialText =>
                        {
                            if (DeviceInfo.Platform == DevicePlatform.Android)
                                RecognitionText = partialText;
                            else
                                RecognitionText += partialText + " ";

                            OnPropertyChanged(nameof(RecognitionText));
                        }), tokenSource.Token);
                }
                catch (Exception ex)
                {
                    await DisplayAlert("Error", ex.Message, "OK");
                }
            }
            else
            {
                await DisplayAlert("Permission Error", "No microphone access", "OK");
            }
        }

        private void ListenCancel()
        {
            tokenSource?.Cancel();
        }
    }

}
