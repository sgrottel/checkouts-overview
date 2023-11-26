using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SG.Checkouts_Overview
{
	public class EntryStatus : INotifyPropertyChanged
	{
		public event PropertyChangedEventHandler? PropertyChanged;

		private bool available = true;
		private bool evaluating = true;
		private bool failedStatus = false;
		private bool localChanges = false;
		private bool incomingChanges = false;
		private bool outgoingChanges = false;
		private bool onBranch = false;
		private string? branchName = null;
		private bool remoteTracked = false;

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
		/// True indicates that the status of the entry is currently being evaluated
		/// </summary>
		public bool Evaluating {
			get {
				return evaluating;
			}
			set {
				if (evaluating != value)
				{
					evaluating = value;
					PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Evaluating)));
				}
			}
		}


		/// <summary>
		/// True indicates that the checkout has changed files (not committed)
		/// </summary>
		public bool FailedStatus {
			get {
				return failedStatus;
			}
			set {
				if (failedStatus != value)
				{
					failedStatus = value;
					PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(FailedStatus)));
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

		/// <summary>
		/// True indicates that the checkout is on a non-main branch
		/// </summary>
		public bool OnBranch {
			get {
				return onBranch;
			}
			set {
				if (onBranch != value)
				{
					onBranch = value;
					PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(OnBranch)));
				}
			}
		}

		/// <summary>
		/// True indicates that the checkout is on a non-main branch
		/// </summary>
		public string? BranchName {
			get {
				return branchName;
			}
			set {
				if (branchName != value)
				{
					branchName = value;
					PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(BranchName)));
				}
			}
		}

		/// <summary>
		/// True indicates that the local branch is tracking a remote branch
		/// </summary>
		public bool RemoteTracked {
			get {
				return remoteTracked;
			}
			set {
				if (remoteTracked != value)
				{
					remoteTracked = value;
					PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(RemoteTracked)));
				}
			}
		}

	}
}
