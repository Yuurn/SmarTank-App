namespace SmarTank;

public partial class TestingLog : ContentPage
{
    private const string KeysListKey = "KeysList";

    public TestingLog()
    {
        InitializeComponent();
        myPicker.SelectedIndexChanged += OnPickerSelectedIndexChanged;
    }

    private void OnPickerSelectedIndexChanged(object sender, EventArgs e)
    {
        var picker = (Picker)sender;
        if (picker.SelectedItem != null)
        {
            var selectedOption = picker.SelectedItem.ToString();
            var currentDate = DateTime.Now.ToString("yyyy-MM-dd");
            var key = "SelectedOption_" + currentDate;

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
}