﻿using System;
using System.Collections.Generic;
using Emgu.CV;
using Emgu.CV.Structure;
using Emgu.CV.CvEnum;
using Emgu.CV.Util;
using Emgu.CV.Dnn;
using System.Drawing;
using DlibDotNet;
using Emgu.CV.Features2D;
using System.Numerics;
using HarfBuzzSharp;
using FaceRecognitionDotNet;

namespace EmoShift
{
    public class HeadMotionAnalysis
    {
        private readonly CascadeClassifier _faceCascade;
        private readonly ShapePredictor _predictor;
        private readonly double _eyeARThresh = 0.25;
        private readonly double _mouthARThresh = 0.79;

        //For Head Tilt
        System.Drawing.PointF[] imagePoints = new System.Drawing.PointF[]
        {
            new System.Drawing.PointF(359f, 391f),     // Nose tip 34
            new System.Drawing.PointF(399, 561),     // Chin 9
            new System.Drawing.PointF(337, 297),     // Left eye left corner 37
            new System.Drawing.PointF(513, 301),     // Right eye right corner 46
            new System.Drawing.PointF(345, 465),     // Left Mouth corner 49
            new System.Drawing.PointF(453, 469)      // Right mouth corner 55
        };

        MCvPoint3D32f[] modelPoints = new MCvPoint3D32f[] {
            new MCvPoint3D32f(0.0f, 0.0f, 0.0f),             // Nose tip 34
            new MCvPoint3D32f(0.0f, -330.0f, -65.0f),        // Chin 9
            new MCvPoint3D32f(-225.0f, 170.0f, -135.0f),     // Left eye left corner 37
            new MCvPoint3D32f(225.0f, 170.0f, -135.0f),      // Right eye right corner 46
            new MCvPoint3D32f(-150.0f, -150.0f, -125.0f),    // Left Mouth corner 49
            new MCvPoint3D32f(150.0f, -150.0f, -125.0f)      // Right mouth corner 55
        };

        Dictionary<string, Tuple<int, int>> FACIAL_LANDMARKS_68_IDXS = new Dictionary<string, Tuple<int, int>>()
        {
            // corrected for base 0, some points also looked wrong
            {"mouth", Tuple.Create(48, 67)},
            {"inner_mouth", Tuple.Create(60, 67)},
            {"right_eyebrow", Tuple.Create(17, 22)},
            {"left_eyebrow", Tuple.Create(22, 26)},
            {"right_eye", Tuple.Create(36, 41)},
            {"left_eye", Tuple.Create(42, 47)},
            {"nose", Tuple.Create(27, 35)},
            {"jaw", Tuple.Create(0, 16)}
        };

        public HeadMotionAnalysis(string faceCascadeXmlFile, string shapePredictorDatFile)
        {
            // Load face cascade and shape predictor
            _faceCascade = new CascadeClassifier(faceCascadeXmlFile); // Update with actual path
            _predictor = ShapePredictor.Deserialize(shapePredictorDatFile);

        }

        public Dictionary<string, string> IsYawning(Mat frame, Mat grayFrame)
        {
            Dictionary<string, string> toReturn = new Dictionary<string, string>
            {
                { "eyes", "None" },
                { "eyesAR", "None" },
                { "head_tilt", "None" },
                { "mouth", "None" },
                { "mouthAR", "None" },
                { "yawn", "None" }
            };


            // Detect faces
            //TODO: _faceCascade reuse from main?
            System.Drawing.Rectangle[] faces = _faceCascade.DetectMultiScale(frame, 1.1, 3, System.Drawing.Size.Empty);

            //Convert to Array2d
            // TODO: Use memory/file stream instead?
            Bitmap bitmapGrayFrame = grayFrame.ToBitmap();
            bitmapGrayFrame.Save("grayFrameYawn.png");
            var grayFrame2d = Dlib.LoadImage<BgrPixel>("grayFrameYawn.png");


            // TODO: Move to main and have libs process frame by frame?
            foreach (System.Drawing.Rectangle face in faces)
            {
                DlibDotNet.Rectangle faceRect = new DlibDotNet.Rectangle();
                //Convert System.Drawing.Rectangle To DlibDotNet.Rectamgle
                faceRect.Top = face.Top;
                faceRect.Left = face.Left;
                faceRect.Bottom = face.Bottom;
                faceRect.Right = face.Right;

                // Detect facial landmarks
                FullObjectDetection shape = _predictor.Detect(grayFrame2d, faceRect);
                System.Drawing.Point[] shapeArray = new System.Drawing.Point[68];
                for (uint i = 0; i < shape.Parts; i++)
                {
                    shapeArray[i].X = shape.GetPart(i).X;
                    shapeArray[i].Y = shape.GetPart(i).Y;
                }


                // Extract the left and right eye coordinates
                var leftEye = shapeArray[FACIAL_LANDMARKS_68_IDXS["left_eye"].Item1..(FACIAL_LANDMARKS_68_IDXS["left_eye"].Item2+1)];
                var rightEye = shapeArray[FACIAL_LANDMARKS_68_IDXS["right_eye"].Item1..(FACIAL_LANDMARKS_68_IDXS["right_eye"].Item2+1)];


                // Calculate eye aspect ratio
                var leftEyeAR = EyeAspectRatio(leftEye);
                var rightEyeAR = EyeAspectRatio(rightEye);
                //?
                var eyeAR = (leftEyeAR + rightEyeAR) / 2.0;

                // Check if eyes are closed
                if (eyeAR < _eyeARThresh)
                {
                    toReturn["eyes"] = "Closed";
                    toReturn["eyesAR"] = eyeAR.ToString();
                }
                else
                {
                    toReturn["eyes"] = "Open";
                    toReturn["eyesAR"] = eyeAR.ToString();
                }
                // Calculate mouth aspect ratio
                // Extract mouth coordinates
                var mouth = shapeArray[FACIAL_LANDMARKS_68_IDXS["mouth"].Item1..(FACIAL_LANDMARKS_68_IDXS["mouth"].Item2 + 1)];
                var mAR = MouthAspectRatio(mouth);
                toReturn["mouthAR"] = mAR.ToString();

                // Check if mouth is open
                if (mAR > _mouthARThresh)
                    toReturn["mouth"] = "Open";
                else
                    toReturn["mouth"] = "Closed";

                // Calculate head tilt
                //var headTilt = GetHeadTilt(grayFrame.Size, faceRect);
                Mat headPoseFrame = new Mat(mat: grayFrame, roi: face);
                (double[] head_tilt_degree, System.Drawing.PointF a, System.Drawing.Point b, System.Drawing.Point c) = 
                    GetHeadTiltAndCoords(headPoseFrame, headPoseFrame.Height);

                if (head_tilt_degree != null)
                    toReturn["head_tilt"] = head_tilt_degree[0].ToString();

                // Check for yawn
                if (eyeAR < _eyeARThresh && mAR > _mouthARThresh)
                    toReturn["yawn"] = "True";
                else
                    toReturn["yawn"] = "False";
            }

            return toReturn;
        }

        private double EyeAspectRatio(IEnumerable<System.Drawing.Point> eye)
        {
            // Compute the euclidean distances between the two sets of
            // vertical eye landmarks (x, y)-coordinates
            // https://medium.com/analytics-vidhya/eye-aspect-ratio-ear-and-drowsiness-detector-using-dlib-a0b2c292d706
            double a = Math.Sqrt(Math.Pow(eye.ElementAt(1).X - eye.ElementAt(5).X, 2) + Math.Pow(eye.ElementAt(1).Y - eye.ElementAt(5).Y, 2));
            double b = Math.Sqrt(Math.Pow(eye.ElementAt(2).X - eye.ElementAt(4).X, 2) + Math.Pow(eye.ElementAt(2).Y - eye.ElementAt(4).Y, 2));
            double c = Math.Sqrt(Math.Pow(eye.ElementAt(0).X - eye.ElementAt(3).X, 2) + Math.Pow(eye.ElementAt(0).Y - eye.ElementAt(3).Y, 2));

            // Compute the eye aspect ratio
            double eyeAR = (a + b) / (2.0 * c);
            // Return the eye aspect ratio
            return eyeAR;
        }

        private double MouthAspectRatio(IEnumerable<System.Drawing.Point> mouth)
        {
            double a = Math.Sqrt(Math.Pow(mouth.ElementAt(2).X - mouth.ElementAt(10).X, 2) + Math.Pow(mouth.ElementAt(2).Y - mouth.ElementAt(10).Y, 2));
            double b = Math.Sqrt(Math.Pow(mouth.ElementAt(4).X - mouth.ElementAt(8).X, 2) + Math.Pow(mouth.ElementAt(4).Y - mouth.ElementAt(8).Y, 2));
            double c = Math.Sqrt(Math.Pow(mouth.ElementAt(0).X - mouth.ElementAt(6).X, 2) + Math.Pow(mouth.ElementAt(0).Y - mouth.ElementAt(6).Y, 2));

            // Compute the mouth aspect ratio
            double mAR = (a + b) / (2.0 * c);
            // Return the mouth aspect ratio
            return mAR;
        }


        public (double[], System.Drawing.PointF, System.Drawing.Point, System.Drawing.Point) GetHeadTiltAndCoords(Mat frame, int frameHeight)
        {
            //https://learnopencv.com/head-pose-estimation-using-opencv-and-dlib/
            System.Drawing.PointF startingPoint;
            System.Drawing.Point endingPoint;
            System.Drawing.Point endingPointAlternate;

            float focalLength = frame.Width;
            System.Drawing.PointF center = new System.Drawing.PointF(frame.Width / 2, frame.Height / 2);

            Emgu.CV.Matrix<float> CamMat = new Emgu.CV.Matrix<float>(3, 3);
            CamMat.Data[0, 0] = focalLength;
            CamMat.Data[0, 1] = 0;
            CamMat.Data[0, 2] = center.X;
            CamMat.Data[1, 0] = 0;
            CamMat.Data[1, 1] = focalLength;
            CamMat.Data[1, 2] = center.Y;
            CamMat.Data[2, 0] = 0;
            CamMat.Data[2, 1] = 0;
            CamMat.Data[2, 2] = 1;

            Emgu.CV.Matrix<float> distCoeffs = new Emgu.CV.Matrix<float>(4, 1);
            distCoeffs.Data[0, 0] = 0;
            distCoeffs.Data[1, 0] = 0;
            distCoeffs.Data[2, 0] = 0;
            distCoeffs.Data[3, 0] = 0;

            Mat rotationVector = new Mat();
            Mat translationVector = new Mat();

            CvInvoke.SolvePnP(
                modelPoints,
                imagePoints,
                CamMat,
                distCoeffs, rotationVector, translationVector);

            // TODO: Verify
            MCvPoint3D32f[] noseEndPoint3D = new MCvPoint3D32f[] { new MCvPoint3D32f(0.0f, 0.0f, 1000.0f) };
            System.Drawing.PointF[] noseEndPoint2D = CvInvoke.ProjectPoints(
                noseEndPoint3D, 
                rotationVector, 
                translationVector, 
                CamMat, 
                distCoeffs);
            
            Emgu.CV.Matrix<double> rotationMatrix = new Emgu.CV.Matrix<double>(3, 3);
            CvInvoke.Rodrigues(rotationVector, rotationMatrix);

            double[] rotationAngles = RotationMatrixToEulerAngles(rotationMatrix);
            double headTilt = Math.Abs(-180 - Helper.RadiansToDegrees(rotationAngles[0]));

            startingPoint = imagePoints[0];
            endingPoint = new System.Drawing.Point((int)noseEndPoint2D[0].X, (int)noseEndPoint2D[0].Y);
            endingPointAlternate = new System.Drawing.Point(endingPoint.X, frameHeight / 2);

            return (new double[] { headTilt }, startingPoint, endingPoint, endingPointAlternate);
        }

        private double[] RotationMatrixToEulerAngles(Emgu.CV.Matrix<double> matrix)
        {
            // TODO: Check my math
            double sy = Math.Sqrt(matrix[0, 0] * matrix[0, 0] + matrix[1, 0] * matrix[1, 0]);
            bool singular = sy < 1e-6;

            double x, y, z;

            if (!singular)
            {
                x = Math.Atan2(matrix[2, 1], matrix[2, 2]);
                y = Math.Atan2(-matrix[2, 0], sy);
                z = Math.Atan2(matrix[1, 0], matrix[0, 0]);
            }
            else
            {
                x = Math.Atan2(-matrix[1, 2], matrix[1, 1]);
                y = Math.Atan2(-matrix[2, 0], sy);
                z = 0;
            }

            return new double[] { x, y, z };
        }
    }

}
