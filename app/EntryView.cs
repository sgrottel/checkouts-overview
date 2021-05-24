using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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

				string s = (status.LocalChanges) ? "Modified" : "Unchanged";
				if (status.OutgoingChanges) s += "; ahead";
				if (status.IncomingChanges) s += "; behind";

				return s;
			}
		}


	}

}
