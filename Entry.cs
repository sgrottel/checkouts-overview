using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace SG.Checkouts_Overview
{

	/// <summary>
	/// An entry in the list of local checkouts
	/// </summary>
	public class Entry : INotifyPropertyChanged
	{
		private string name;
		private string path;
		private string type;
		private bool available;
		private bool evaluating;
		private bool statusKnown;
		private bool localChanges;
		private bool incomingChanges;
		private bool outgoingChanges;
		private string lastMessage = null;

		public event PropertyChangedEventHandler PropertyChanged;

		/// <summary>
		/// Display name of the checkout
		/// </summary>
		public string Name {
			get {
				return name;
			}
			set {
				if (!string.Equals(name, value))
				{
					name = value;
					PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Name)));
				}
			}
		}

		/// <summary>
		/// File system path of the checkout
		/// </summary>
		public string Path {
			get {
				return path;
			}
			set {
				if (!string.Equals(path, value))
				{
					path = value;
					PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Path)));
				}
			}
		}

		/// <summary>
		/// Repository type of the checkout
		/// </summary>
		public string Type {
			get {
				return type;
			}
			set {
				if (!string.Equals(type, value))
				{
					type = value;
					PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Type)));
				}
			}
		}

		/// <summary>
		/// True indicates that the checkout file system path and type-base analysis is available
		/// </summary>
		[XmlIgnore]
		public bool Available {
			get {
				return available;
			}
			set {
				if (available != value)
				{
					available = value;
					PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Available)));
					PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(StatusText)));
				}
			}
		}

		/// <summary>
		/// True indicates that the status of the entry is currently being evaluated
		/// </summary>
		[XmlIgnore]
		public bool Evaluating {
			get {
				return evaluating;
			}
			set {
				if (evaluating != value)
				{
					evaluating = value;
					PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Evaluating)));
					PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(StatusText)));
				}
			}
		}

		/// <summary>
		/// True indicates that the status of the available checkout has not been determined yet
		/// </summary>
		[XmlIgnore]
		public bool StatusKnown {
			get {
				return statusKnown;
			}
			set {
				if (statusKnown != value)
				{
					statusKnown = value;
					PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(StatusKnown)));
					PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(StatusText)));
				}
			}
		}

		/// <summary>
		/// True indicates that the checkout has changed files (not committed)
		/// </summary>
		[XmlIgnore]
		public bool LocalChanges {
			get {
				return localChanges;
			}
			set {
				if (localChanges != value)
				{
					localChanges = value;
					PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(LocalChanges)));
					PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(StatusText)));
				}
			}
		}

		/// <summary>
		/// True indicates that the checkout has incoming changes (need to pull/fast forward)
		/// </summary>
		[XmlIgnore]
		public bool IncomingChanges {
			get {
				return incomingChanges;
			}
			set {
				if (incomingChanges != value)
				{
					incomingChanges = value;
					PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IncomingChanges)));
					PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(StatusText)));
				}
			}
		}

		/// <summary>
		/// True indicates that the checkout has outgoing changes (need to push)
		/// </summary>
		[XmlIgnore]
		public bool OutgoingChanges {
			get {
				return outgoingChanges;
			}
			set {
				if (outgoingChanges != value)
				{
					outgoingChanges = value;
					PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(OutgoingChanges)));
					PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(StatusText)));
				}
			}
		}

		/// <summary>
		/// gets a status text message, which summarizes the above flags
		/// </summary>
		[XmlIgnore]
		public string StatusText {
			get {
				return ">> Not Impelemented <<";
			}
		}

		/// <summary>
		/// The (error/warning) messages of the last operation.
		/// </summary>
		[XmlIgnore]
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

	}
}
