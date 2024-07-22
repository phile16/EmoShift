﻿//----------------------------------------------------------------------------
//  Copyright (C) 2004-2024 by EMGU Corporation. All rights reserved.       
//----------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;
using Emgu.CV;
using Emgu.CV.Cuda;
using Emgu.CV.CvEnum;
using Emgu.CV.Dnn;
using Emgu.CV.ML;


namespace EmoShift
{
    public class SystemSettings : ContentPage
    {
        public SystemSettings()
        {
#if DEBUG
            CvInvoke.LogLevel = LogLevel.Verbose; //LogLevel.Debug;
#endif

            String lineBreak = "<br/>";

            String openclTxt = String.Format("Has OpenCL: {0}", CvInvoke.HaveOpenCL);
            if (CvInvoke.HaveOpenCL)
            {
                openclTxt = String.Format("{0}{1}Use OpenCL: {2}{1}<textarea rows=\"5\">{3}</textarea>{1}",
                   openclTxt, lineBreak,
                   CvInvoke.UseOpenCL,
                   CvInvoke.OclGetPlatformsSummary());
            }

            String cudaTxt = String.Format("Has CUDA: {0}", CudaInvoke.HasCuda);
            if (CudaInvoke.HasCuda)
            {
                cudaTxt = String.Format("{0}{1}<textarea rows=\"5\">{2}</textarea>{1}",
                    cudaTxt,
                    lineBreak,
                    CudaInvoke.GetCudaDevicesSummary());
            }

            var openCVConfigDict = CvInvoke.ConfigDict;
            bool haveDNN = (openCVConfigDict["HAVE_OPENCV_DNN"] != 0);
            String dnnText;
            if (haveDNN)
            {
                var dnnBackends = DnnInvoke.AvailableBackends;
                List<String> dnnBackendsText = new List<string>();
                foreach (var dnnBackend in dnnBackends)
                {
                    dnnBackendsText.Add(String.Format("<p>{0} - {1}</p>", dnnBackend.Backend, dnnBackend.Target));
                }

                dnnText = String.Join("", dnnBackendsText.ToArray());
            }
            else
            {
                dnnText = "DNN is not available";
            }

            bool haveVideoio = (openCVConfigDict["HAVE_OPENCV_VIDEOIO"] != 0);

            String osDescription = Emgu.Util.Platform.OperationSystem.ToString();

            String parallelText;
            List<String> parallelBackendText = new List<string>();
            foreach (var parallelBackend in CvInvoke.AvailableParallelBackends)
            {
                parallelBackendText.Add(String.Format("<p>{0}</p>", parallelBackend));
            }

            parallelText = String.Join("", parallelBackendText.ToArray());

            String tesseractText;
            String tesseractVersion = String.Empty;
            bool haveTesseract = (openCVConfigDict["HAVE_EMGUCV_TESSERACT"] != 0);

            if (haveTesseract)
            {
                tesseractVersion = Emgu.CV.OCR.Tesseract.VersionString;
                if (tesseractVersion.Length == 0)
                    haveTesseract = false;
            }

            if (haveTesseract)
            {
                tesseractText = String.Format("Version: {0}", tesseractVersion);
            }
            else
            {
                tesseractText = "Tesseract OCR is not available";
            }

            Content =
                new Microsoft.Maui.Controls.ScrollView()
                {
                    Content =
                        new WebView()
                        {
                            //WidthRequest = 1000,
                            //HeightRequest = 1000,
                            MinimumWidthRequest = 100,
                            MinimumHeightRequest = 100,
                            Source = new HtmlWebViewSource()
                            {
                                Html =
                                    @"<html>
            <head>
            <style>body { background-color: #EEEEEE; }</style>
            <style type=""text/css"">
            textarea { width: 100%; margin: 0; padding: 0; border - width: 0; }
            </style>
            </head>
            <body>
            <H2> Emgu CV for MAUI</H2>
            <a href=http://www.emgu.com>Visit our website</a> <br/><br/>
            <a href=mailto:support@emgu.com>Email Support</a> <br/><br/>
            <H4> OpenCL Info </H4>
            " + openclTxt + @"
            <H4> Cuda Info </H4>
            " + cudaTxt + @"
            <H4> OS: </H4>
            " + osDescription + @"
            <H4> Available Parallel Backends: </H4>
            " + parallelText + @"
            <H4> Dnn Backends: </H4>
            " + dnnText + @"
            <H4> Capture Backends (VideoCapture from device): </H4>
            " + (haveVideoio ? GetBackendInfo(CvInvoke.Backends) : "Videoio backend not supported.") + @"
            <H4> Stream Backends (VideoCapture from file/Stream): </H4>
            " + (haveVideoio ? GetBackendInfo(CvInvoke.StreamBackends) : "Videoio backend not supported.") + @"
            <H4> VideoWriter Backends: </H4>
            " + (haveVideoio ? GetBackendInfo(CvInvoke.WriterBackends) : "Videoio backend not supported.") + @"
            <H4> Tesseract OCR: </H4>
            " + tesseractText + @"
            <H4> Build Info </H4>
            <textarea rows=""30"">"
                                    + CvInvoke.BuildInformation + @"
            </textarea>
			<H4> RuntimeInformation.OSArchitecture: </H4>
            " + RuntimeInformation.OSArchitecture + @"
            <H4> RuntimeInformation.OSDescription: </H4>
            " + RuntimeInformation.OSDescription + @"
            <H4> RuntimeInformation.FrameworkDescription: </H4>
            " + RuntimeInformation.FrameworkDescription + @"
            <H4> RuntimeInformation.ProcessArchitecture: </H4>
            " + RuntimeInformation.ProcessArchitecture + @"
            <H4> RuntimeInformation.RuntimeIdentifier: </H4>
            " + RuntimeInformation.RuntimeIdentifier + @"
            <H4> Microsoft.Maui.Devices.DeviceInfo.Current.Model: </H4>
            " + Microsoft.Maui.Devices.DeviceInfo.Current.Model + @"
            <H4> Microsoft.Maui.Devices.DeviceInfo.Current.Manufacturer: </H4>
            " + Microsoft.Maui.Devices.DeviceInfo.Current.Manufacturer + @"
            <H4> Microsoft.Maui.Devices.DeviceInfo.Current.Name: </H4>
            " + Microsoft.Maui.Devices.DeviceInfo.Current.Name + @"
            <H4> Microsoft.Maui.Devices.DeviceInfo.Current.VersionString: </H4>
            " + Microsoft.Maui.Devices.DeviceInfo.Current.VersionString + @"
            <H4> Microsoft.Maui.Devices.DeviceInfo.Current.Idiom: </H4>
            " + Microsoft.Maui.Devices.DeviceInfo.Current.Idiom + @"
            <H4> Microsoft.Maui.Devices.DeviceInfo.Current.Platform: </H4>
            " + Microsoft.Maui.Devices.DeviceInfo.Current.Platform+ @"
            <H4> Microsoft.Maui.Devices.DeviceInfo.Current.DeviceType: </H4>
            " + Microsoft.Maui.Devices.DeviceInfo.Current.DeviceType+ @"
            </body>
            </html>"
                            }

                        }
                };
            Content.BackgroundColor = Color.FromRgb(1.0, 0.0, 0.0);
        }

        private static String GetBackendInfo(Emgu.CV.Backend[] backends)
        {
            List<String> backendsText = new List<string>();
            foreach (var backend in backends)
            {
                backendsText.Add(String.Format("<p>{0} - {1}</p>", backend.ID, backend.Name));
            }

            return String.Join("", backendsText.ToArray());
        }
    }
}
