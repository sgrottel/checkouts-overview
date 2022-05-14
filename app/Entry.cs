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
		private string name = string.Empty;
		private string path = string.Empty;
		private string type = string.Empty;
		private string mainBranch = string.Empty;
		private Tristate gitFetchOnUpdate = Tristate.Default;

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
		/// Overrides the main branch for this repo
		/// </summary>
		[DefaultValue("")]
		public string MainBranch {
			get {
				return mainBranch;
			}
			set {
				if (!string.Equals(mainBranch, value))
				{
					mainBranch = value;
					PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(MainBranch)));
				}
			}
		}

		/// <summary>
		/// Run "git fetch all" on update; use app-wide setting if null
		/// </summary>
		[DefaultValue(Tristate.Default)]
		public Tristate GitFetchOnUpdate {
            get {
				return gitFetchOnUpdate;
			}
			set {
				if (gitFetchOnUpdate != value) {
					gitFetchOnUpdate = value;
					PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(GitFetchOnUpdate)));
				}
            }
		}

    }
}
