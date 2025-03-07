namespace SmarTank
{
    public partial class MainPage : ContentPage
    {
        int count = 0;

        public MainPage()
        {
            InitializeComponent();
        }

        private async void ContNewTank(object sender, EventArgs e)
        {
            //  count++;

            //if (count == 1)
            //     CounterBtn.Text = $"Clicked {count} time";
            // else
            //    CounterBtn.Text = $"Clicked {count} times";

            //SemanticScreenReader.Announce(CounterBtn.Text);

            await Navigation.PushAsync(new Tutorial());

        }

        private async void ContCurTank(object sender, EventArgs e)
        {

            await Navigation.PushAsync(new Parameters());
        }

    }

}
