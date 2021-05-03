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

			Entry e = item as Entry;
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

			if (e.FailedStatus) return element.FindResource("Icon_Failed_x32") as DataTemplate;
			if (!e.StatusKnown) return element.FindResource("Icon_Unknown_x32") as DataTemplate;
			if (!e.Available) return element.FindResource("Icon_Unavailable_x32") as DataTemplate;

			return element.FindResource(
				String.Format("Icon_{0}{1}{2}_x32",
				e.LocalChanges ? 'x' : 'n',
				e.IncomingChanges ? 'i' : 'n',
				e.OutgoingChanges ? 'o' : 'n'
				)) as DataTemplate;
		}
	}
}
