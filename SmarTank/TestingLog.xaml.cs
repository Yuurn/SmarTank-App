namespace SmarTank;

public partial class TestingLog : ContentPage
{
    private const string KeysListKey = "KeysList";

    public TestingLog()
    {
        InitializeComponent();
        myPicker.SelectedIndexChanged += OnPickerSelectedIndexChanged;
        myPicker2.SelectedIndexChanged += OnPickerSelectedIndexChanged;
        myPicker3.SelectedIndexChanged += OnPickerSelectedIndexChanged;
        myPicker4.SelectedIndexChanged += OnPickerSelectedIndexChanged;
    }

    private void OnPickerSelectedIndexChanged(object sender, EventArgs e)
    {
        var picker = (Picker)sender;
        if (picker.SelectedItem != null)
        {
            var selectedOption = picker.SelectedItem.ToString();
            var currentDate = DateTime.Now.ToString("yyyy-MM-dd");
            var key = "SelectedOption_"+ picker.StyleId + currentDate;

            // Save the selected option with the date
            Preferences.Set(key, selectedOption);

            // Save the key to the list of keys
            var keysList = Preferences.Get(KeysListKey, string.Empty);
            var keys = keysList.Split(',').ToList();
            if (!keys.Contains(key))
            {
                keys.Add(key);
                Preferences.Set(KeysListKey, string.Join(",", keys));
            }

            DisplayAlert("Selected Option", $"You selected: {selectedOption} on {currentDate}", "OK");
        }
    }
    private void OnViewPreviousSelectionsClicked1(object sender, EventArgs e)
    {
        DisplayPreviousSelections("pick1");
    }

    private void OnViewPreviousSelectionsClicked2(object sender, EventArgs e)
    {
        DisplayPreviousSelections("pick2");
    }

    private void OnViewPreviousSelectionsClicked3(object sender, EventArgs e)
    {
        DisplayPreviousSelections("pick3");
    }

    private void OnViewPreviousSelectionsClicked4(object sender, EventArgs e)
    {
        DisplayPreviousSelections("pick4");
    }

    private void DisplayPreviousSelections(string pickerId)
    {
        var keysList = Preferences.Get(KeysListKey, string.Empty);
        var keys = keysList.Split(',').ToList();
        var savedOptions = new List<string>();

        foreach (var key in keys)
        {
            if (!string.IsNullOrEmpty(key) && key.Contains(pickerId))
            {
                var date = key.Replace("SelectedOption_" + pickerId + "_", "");
                var option = Preferences.Get(key, "No option selected");
                savedOptions.Add($"{date}: {option}");
            }
        }

        var savedOptionsText = string.Join("\n", savedOptions);
        DisplayAlert("Previous Selections", savedOptionsText, "OK");
    }
    //not sure if I need this method
    private void OnViewSavedOptionsClicked(object sender, EventArgs e)
    {
        var keysList = Preferences.Get(KeysListKey, string.Empty);
        var keys = keysList.Split(',').ToList();
        var savedOptions = new List<string>();

        foreach (var key in keys)
        {
            if (!string.IsNullOrEmpty(key))
            {
                var date = key.Replace("SelectedOption_", "");
                var option = Preferences.Get(key, "No option selected");
                savedOptions.Add($"{date}: {option}");
            }
        }

        var savedOptionsText = string.Join("\n", savedOptions);
        DisplayAlert("Saved Options", savedOptionsText, "OK");
    }
    
    private void OnClearAllSavedOptionsClicked(object sender, EventArgs e)
    {
        Preferences.Clear();
        DisplayAlert("Cleared", "All saved options have been cleared.", "OK");
    }
}