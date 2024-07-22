
# Facial Expression Recognition (FER)
1. `Corrective_re-annotation_of_FER_CK+_KDEF-cnn_300_0.001_1E-05_256_train_best_0.944568452380952.dat`
1. Get it from: s3://kscope-secrets/Corrective_re-annotation_of_FER_CK+_KDEF-cnn_300_0.001_1E-05_256_train_best_0.944568452380952.zip
1. This is trained data for the emotion detection. Due to licensing issues with the dataset we had to train our own data using the below dataset
	1. See info here: https://www.kaggle.com/datasets/sudarshanvaidya/corrective-reannotation-of-fer-ck-kdef
1. Create a "emotion" directory in `Resources/Raw` and upzip the file there
1. This was trained using https://github.com/takuya-takeuchi/FaceRecognitionDotNet

# Install Charts
1. https://github.com/beto-rodriguez/LiveCharts2
1. dotnet add package LiveChartsCore.SkiaSharpView --version 2.0.0-rc2
1. dotnet add package LiveChartsCore.SkiaSharpView.Maui --version 2.0.0-rc2
1. dotnet add package LiveChartsCore.SkiaSharpView.Uno --version 2.0.0-rc2

# Known Issues
1. Head Tilt not working
1. Heart Rate not working
1. Hand tracking seems pretty awful