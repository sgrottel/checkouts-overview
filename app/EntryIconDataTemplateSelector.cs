using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace SG.Checkouts_Overview
{
	public class EntryIconDataTemplateSelector : DataTemplateSelector
	{
		public override DataTemplate SelectTemplate(object item, DependencyObject container)
		{
			FrameworkElement element = container as FrameworkElement;

			EntryView e = item as EntryView;
			if (e == null) return element.FindResource("Icon_Empty_x32") as DataTemplate;

			// refresh content template selector when any property of entry changes
			e.PropertyChanged += (o, a) =>
			{
				try
				{
					var cp = container as ContentPresenter;
					if (cp.Dispatcher.HasShutdownStarted) return;
					cp.Dispatcher.Invoke(() =>
					{
						cp.ContentTemplateSelector = null;
						cp.ContentTemplateSelector = this;
					});
				}
				catch { }
			};

			if (e.Status == null) return element.FindResource("Icon_Unknown_x32") as DataTemplate;

			if (e.Status.FailedStatus) return element.FindResource("Icon_Failed_x32") as DataTemplate;
			if (!e.Status.Available) return element.FindResource("Icon_Unavailable_x32") as DataTemplate;

			return element.FindResource(
				String.Format("Icon_{0}{1}{2}_x32",
				e.Status.LocalChanges ? 'x' : 'n',
				e.Status.IncomingChanges ? 'i' : 'n',
				e.Status.OutgoingChanges ? 'o' : 'n'
				)) as DataTemplate;
		}
	}
}
