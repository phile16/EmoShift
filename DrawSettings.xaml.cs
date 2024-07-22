namespace EmoShift;


public partial class DrawSettings : ContentPage
{
    public DrawSettings()
	{
		InitializeComponent();
        cbShowFaceRectangle.IsChecked = Preferences.Default.Get("showFaceRect", false);
        cbShowHandRectangle.IsChecked = Preferences.Default.Get("showHandRect", false);
        cbShowFaceLandmarks.IsChecked = Preferences.Default.Get("showFaceLandmarks", false);
        cbShowEmotionRectangle.IsChecked = Preferences.Default.Get("showEmotionRect", false);
        entryFPS.Text =  Preferences.Default.Get("videoFPS", 24).ToString();
        entryFerShift.Text = Preferences.Default.Get("shiftAlert", 2).ToString();
        entrySaveFiles.Text =  Preferences.Default.Get("saveFileLocation", Directory.GetCurrentDirectory());
    }

    private void OnEntryFPSTextChanged(object sender, TextChangedEventArgs e)
    {
        Preferences.Default.Set("videoFPS", int.Parse(entryFPS.Text));
    }

    private void OnEntryFPSCompleted(object sender, EventArgs e)
    {

    }

    private void OnEntrySaveFilesTextChanged(object sender, TextChangedEventArgs e)
    {   
        //TODO: Use file saver
        Preferences.Default.Set("saveFileLocation", entrySaveFiles.Text);
    }

    private void OnEntrySaveFilesCompleted(object sender, EventArgs e)
    {

    }

    private void cbShowFaceLandmarks_CheckedChanged(object sender, CheckedChangedEventArgs e)
    {
        Preferences.Default.Set("showFaceLandmarks", cbShowFaceLandmarks.IsChecked);
    }

    private void cbShowFaceRectangle_CheckedChanged(object sender, CheckedChangedEventArgs e)
    {
        Preferences.Default.Set("showFaceRect", cbShowFaceRectangle.IsChecked);
    }

    private void cbShowHandRectangle_CheckedChanged(object sender, CheckedChangedEventArgs e)
    {
        Preferences.Default.Set("showHandRect", cbShowHandRectangle.IsChecked);
    }

    private void cbShowEmotionRectangle_CheckedChanged(object sender, CheckedChangedEventArgs e)
    {
        Preferences.Default.Set("showEmotionRect", cbShowEmotionRectangle.IsChecked);
    }

    private void entryFerShift_TextChanged(object sender, TextChangedEventArgs e)
    {
        Preferences.Default.Set("shiftAlert", entryFerShift.Text);
    }

    private void entryFerShift_Completed(object sender, EventArgs e)
    {

    }
}