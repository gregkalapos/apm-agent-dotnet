using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Elastic.Apm.Api;
using Microsoft.UI.Xaml.Controls;
using Button = Microsoft.Maui.Controls.Button;
using Page = Microsoft.Maui.Controls.Page;

namespace Elastic.Apm.Maui
{
	internal class MauiEventsBinder
	{
		private readonly IApplication _application;

		public MauiEventsBinder(IApplication application)
		{
			_application = application;
			//TODO: inject this
			_tracer = Agent.Tracer;
		}
		private readonly ITracer _tracer;

		internal void BindEvents()
		{
			if (_application is Application app)
				BindApplicationEvents(app);
		}

		private void BindApplicationEvents(Application application)
		{
#pragma warning disable IDE0022 // Use expression body for methods
			application.DescendantAdded += (_, e) =>
			{
				switch (e.Element)
				{
					case Page page:
						BindPageEvents(page);
						break;
					case Button button:
						BindButtonEvents(button);
						break;
				}
			};
#pragma warning restore IDE0022 // Use expression body for methods]]
		}

		private void BindButtonEvents(Button button)
		{
			button.Pressed += (sender, e) =>
			{
				var transaction = _tracer.StartTransaction($"Button pressed `{(sender as Button).Text}`", "ButtonClick");
				SetDeviceInfo(transaction);
			};

			button.Released += (sender, e) =>
			{
				_tracer.CurrentTransaction?.End();
			};
		}
		private void BindPageEvents(Page page)
		{
#pragma warning disable IDE0022 // Use expression body for methods
			// Lifecycle events
			// https://docs.microsoft.com/dotnet/maui/fundamentals/shell/lifecycle
			page.NavigatedTo += (sender, _) =>
			{
				var transactionName = "Navigated to";
				if (sender is ContentPage page)
				{
					if(!string.IsNullOrEmpty(page.Title))
						transactionName += $" {page.Title}";
					else
						transactionName += $" {page.GetType().Name}";
				}

				var transaction = _tracer.StartTransaction(transactionName, "NavigatedTo");
				SetDeviceInfo(transaction);
				transaction.End();
			};
		}

		private void SetDeviceInfo(ITransaction transaction)
		{
			transaction.SetLabel("device.manufacturer", DeviceInfo.Manufacturer);
			transaction.SetLabel("device.model.name", DeviceInfo.Model);
		}
	}
}
