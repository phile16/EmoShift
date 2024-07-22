using System;
using System.Globalization;
using CommunityToolkit.Maui.Media;


namespace EmoShift
{
    class SpeechAnalysis
    {
        //private readonly ISpeechToText speechToText;
        public string RecognitionText;

        public async Task StartListening(CancellationToken cancellationToken)
        {
            var isGranted = await SpeechToText.RequestPermissions(cancellationToken);
            if (!isGranted)
            {
                RecognitionText = "Permission to mic not granted";
                //await Toast.Make("Permission not granted").Show(CancellationToken.None);
                return;
            }

            var recognitionResult = await SpeechToText.ListenAsync(
                                                CultureInfo.GetCultureInfo("en-us"),
                                                new Progress<string>(partialText =>
                                                {
                                                    RecognitionText += partialText + " ";
                                                }), cancellationToken);

            if (recognitionResult.IsSuccessful)
            {
                RecognitionText = recognitionResult.Text;
            }
            else
            {
                RecognitionText = "Not Understood - " + System.DateTime.Now.ToString("dd-MM-yyyy HH:mm:ss");
                //await Toast.Make(recognitionResult.Exception?.Message ?? "Unable to recognize speech").Show(CancellationToken.None);
            }
        }

        public async Task StopListening(CancellationToken cancellationToken)
        {
            await SpeechToText.StopListenAsync(CancellationToken.None);
            SpeechToText.Default.RecognitionResultUpdated -= OnRecognitionTextUpdated;
            SpeechToText.Default.RecognitionResultCompleted -= OnRecognitionTextCompleted;
        }

        public void OnRecognitionTextUpdated(object? sender, SpeechToTextRecognitionResultUpdatedEventArgs args)
        {
            RecognitionText += args.RecognitionResult;
        }

        public void OnRecognitionTextCompleted(object? sender, SpeechToTextRecognitionResultCompletedEventArgs args)
        {
            RecognitionText = args.RecognitionResult;
        }

    }



}
