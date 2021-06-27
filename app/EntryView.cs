using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;

namespace SG.Checkouts_Overview
{

	public class EntryView : INotifyPropertyChanged
	{
		public event PropertyChangedEventHandler PropertyChanged;

		private Entry entry = null;
		private EntryStatus status = null;
		private bool isSelected = false;
		private string lastMessage = null;

		public Entry Entry {
			get { return entry; }
			set {
				if (entry != value)
				{
					if (entry != null) { entry.PropertyChanged -= Entry_PropertyChanged; }
					entry = value;
					if (entry != null) { entry.PropertyChanged += Entry_PropertyChanged; }
					PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Entry)));
					Entry_PropertyChanged(entry, null);
				}
			}
		}

		public EntryStatus Status {
			get { return status; }
			set {
				if (status != value)
				{
					if (status != null) { status.PropertyChanged -= EntryStatus_PropertyChanged; }
					status = value;
					if (status != null) { status.PropertyChanged += EntryStatus_PropertyChanged; }
					PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Status)));
					EntryStatus_PropertyChanged(status, null);
				}
			}
		}

		private void Entry_PropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			if (e == null || string.IsNullOrWhiteSpace(e.PropertyName) || e.PropertyName == nameof(Entry.Name))
			{
				PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Name)));
			}
		}

		private void EntryStatus_PropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(StatusText)));
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IconUnknownVisibility)));
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IconEvaluatingVisibility)));
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IconFailedVisibility)));
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IconUnavailableVisibility)));
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IconNormalVisibility)));
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(SubIconBranchVisibility)));
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(SubIconMainBranchVisibility)));
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(SubIconUpVisibility)));
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(SubIconDownVisibility)));
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(SubIconChangesBrush)));
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(SubIconUntrackedVisibility)));
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(SubIconBase2Visibility)));
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(SubIconBase2X)));
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(SubIconBase2H)));
		}

		public Visibility IconUnknownVisibility {
			get {
				return (status == null) ? Visibility.Visible : Visibility.Hidden;
			}
		}
		public Visibility IconEvaluatingVisibility {
			get {
				return ((status?.Evaluating)??false) ? Visibility.Visible : Visibility.Hidden;
			}
		}
		public Visibility IconFailedVisibility {
			get {
				if (status == null) return Visibility.Hidden;
				return (status.FailedStatus) ? Visibility.Visible : Visibility.Hidden;
			}
		}
		public Visibility IconUnavailableVisibility {
			get {
				if (status == null) return Visibility.Hidden;
				if (status.FailedStatus) return Visibility.Hidden;
				return (!status.Available) ? Visibility.Visible : Visibility.Hidden;
			}
		}
		public Visibility IconNormalVisibility {
			get {
				if (status == null) return Visibility.Hidden;
				if (status.FailedStatus) return Visibility.Hidden;
				return (status.Available) ? Visibility.Visible : Visibility.Hidden;
			}
		}
		public Visibility SubIconBranchVisibility {
			get {
				if (status == null) return Visibility.Hidden;
				return (status.OnBranch) ? Visibility.Visible : Visibility.Hidden;
			}
		}
		public Visibility SubIconMainBranchVisibility {
			get {
				if (status == null) return Visibility.Hidden;
				return (!status.OnBranch) ? Visibility.Visible : Visibility.Hidden;
			}
		}
		public Visibility SubIconUpVisibility {
			get {
				if (status == null) return Visibility.Hidden;
				return (status.OutgoingChanges) ? Visibility.Visible : Visibility.Hidden;
			}
		}
		public Visibility SubIconDownVisibility {
			get {
				if (status == null) return Visibility.Hidden;
				return (status.IncomingChanges) ? Visibility.Visible : Visibility.Hidden;
			}
		}
		public Brush SubIconChangesBrush {
			get {
				if (status == null) return Brushes.Transparent;
				return
					Application.Current.MainWindow.FindResource(
						status.LocalChanges
						? "StatusX"
						: "StatusOk"
					) as Brush;
			}
		}
		public Visibility SubIconUntrackedVisibility {
			get {
				if (status == null) return Visibility.Hidden;
				return (!status.RemoteTracked) ? Visibility.Visible : Visibility.Hidden;
			}
		}
		public Visibility SubIconBase2Visibility {
			get {
				if (status == null) return Visibility.Hidden;
				if (!status.RemoteTracked) return Visibility.Hidden;
				if (status.IncomingChanges && status.OutgoingChanges) return Visibility.Hidden;
				return Visibility.Visible;
			}
		}
		public int SubIconBase2X {
			get {
				if (status == null) return 0;
				if (status.IncomingChanges) return 17;
				if (status.OutgoingChanges) return -17;
				return 0;
			}
		}
		public int SubIconBase2H {
			get {
				if (status == null) return 0;
				if (!status.LocalChanges && !status.OnBranch) return 18;
				return 15;
			}
		}

		/// <summary>
		/// Entry is selected in the main list view
		/// </summary>
		public bool IsSelected {
			get { return isSelected; }
			set {
				if (isSelected != value)
				{
					isSelected = value;
					PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsSelected)));
				}
			}
		}

		public string Name {
			get { return entry?.Name; }
		}

		/// <summary>
		/// The (error/warning) messages of the last operation.
		/// </summary>
		public string LastMessage {
			get {
				return lastMessage;
			}
			set {
				if (!string.Equals(lastMessage, value))
				{
					lastMessage = value;
					PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(LastMessage)));
					PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(HasLastMessage)));
				}
			}
		}

		/// <summary>
		/// True indicates that there is a last message
		/// </summary>
		public bool HasLastMessage {
			get {
				return !string.IsNullOrWhiteSpace(lastMessage);
			}
		}


		/// <summary>
		/// gets a status text message, which summarizes the above flags
		/// </summary>
		public string StatusText {
			get {
				if (status == null) return "Unknown";
				if (status.Evaluating) return "Evaluating...";
				if (status.FailedStatus) return "Failed to evaluate";
				if (!status.Available) return "Not available";

				List<string> items = new List<string>();
				if (status.LocalChanges) items.Add("local changes");
				if (status.OnBranch) items.Add("⎇ " + status.BranchName);
				if (!status.RemoteTracked) items.Add("untracked");
				else
				{
					if (status.OutgoingChanges) items.Add("ahead");
					if (status.IncomingChanges) items.Add("behind");
				}

				if (items.Count <= 0) return "up to date";

				return string.Join("; ", items);
			}
		}


	}

}
