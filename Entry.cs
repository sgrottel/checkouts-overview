using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
		private bool statusUnknown;
		private bool localChanges;
		private bool incomingChanges;
		private bool outgoingChanges;

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
		public bool Available {
			get {
				return available;
			}
			set {
				if (available != value)
				{
					available = value;
					PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Available)));
				}
			}
		}

		/// <summary>
		/// True indicates that the status of the available checkout has not been determined yet
		/// </summary>
		public bool StatusUnknown {
			get {
				return statusUnknown;
			}
			set {
				if (statusUnknown != value)
				{
					statusUnknown = value;
					PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(StatusUnknown)));
				}
			}
		}

		/// <summary>
		/// True indicates that the checkout has changed files (not committed)
		/// </summary>
		public bool LocalChanges {
			get {
				return localChanges;
			}
			set {
				if (localChanges != value)
				{
					localChanges = value;
					PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(LocalChanges)));
				}
			}
		}

		/// <summary>
		/// True indicates that the checkout has incoming changes (need to pull/fast forward)
		/// </summary>
		public bool IncomingChanges {
			get {
				return incomingChanges;
			}
			set {
				if (incomingChanges != value)
				{
					incomingChanges = value;
					PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IncomingChanges)));
				}
			}
		}

		/// <summary>
		/// True indicates that the checkout has outgoing changes (need to push)
		/// </summary>
		public bool OutgoingChanges {
			get {
				return outgoingChanges;
			}
			set {
				if (outgoingChanges != value)
				{
					outgoingChanges = value;
					PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(OutgoingChanges)));
				}
			}
		}

	}
}
