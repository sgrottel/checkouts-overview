using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SG.Checkouts_Overview
{

    /// <summary>
    /// Observable collection of EntryView objects, and providing a dirty flag
    /// </summary>
    internal class EntryViewsCollection: ObservableCollection<EntryView>
    {
        private bool isDirty = false;

        /// <summary>
        /// Flag set to true, when the collection is changed.
        /// This does only include properties for the `Entry`s in the view, not the properties of `EntryStatus`
        /// </summary>
        public bool IsDirty {
            get { return isDirty; }
            set
            {
                if (value != isDirty)
                {
                    isDirty = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsDirty)));
                }
            }
        }

        protected override event PropertyChangedEventHandler PropertyChanged;

        protected override void OnCollectionChanged(NotifyCollectionChangedEventArgs e)
        {
            base.OnCollectionChanged(e);
            if (e.OldItems != null)
                foreach (var item in e.OldItems)
                {
                    EntryView ev = item as EntryView;
                    if (ev == null) continue;
                    ev.Entry.PropertyChanged -= Entry_PropertyChanged;
                }
            if (e.NewItems != null)
                foreach (var item in e.NewItems)
                {
                    EntryView ev = item as EntryView;
                    if (ev == null) continue;
                    ev.Entry.PropertyChanged += Entry_PropertyChanged;
                }
            IsDirty = true;
        }

        private void Entry_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            IsDirty = true;
        }
    }
}
