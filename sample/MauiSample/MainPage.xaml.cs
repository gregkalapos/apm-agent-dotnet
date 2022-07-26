namespace MauiSample
{
	public partial class MainPage : ContentPage
	{
		private int count = 0;

		public MainPage() => InitializeComponent();

		private async void OnCounterClicked(object sender, EventArgs e)
		{
			count++;

			var httpClient = new HttpClient();
			await httpClient.GetAsync("https://www.elastic.co");

			if (count == 1)
				CounterBtn.Text = $"Clicked {count} time";
			else
				CounterBtn.Text = $"Clicked {count} times";

			SemanticScreenReader.Announce(CounterBtn.Text);
		}
	}
}
