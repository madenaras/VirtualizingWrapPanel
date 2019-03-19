using System;
using System.Linq;
using VWPTestApp;

namespace VWPTestApp
{
    /// <summary>
    /// The view model responsible for the <see cref="VWPTestApp.FindWindow"/>.
    /// </summary>
    public class FindViewModel : ViewModelBase
    {
        private SeriesToDisplayViewModel SeriesListViewModel { get; }
        private int currentSearchPosition;
        private bool found;
        #region Public Fields
        private string search;
        /// <summary>
        /// The input search string.
        /// </summary>
        public string Search
        {
            get { return search; }
            set
            {
                search = value;
                RaisePropertyChanged("Search");
                CanFind = String.IsNullOrWhiteSpace(search) ? false : true;
            }
        }

        private bool direction;
        /// <summary>
        /// The direction of the search. True=next / False=previous
        /// </summary>
        public bool Direction
        {
            get { return direction; }
            set { direction = value; RaisePropertyChanged("Direction"); }
        }

        private bool canFind;
        /// <summary>
        /// True if the user can launch a search.
        /// </summary>
        public bool CanFind
        {
            get { return canFind; }
            set
            {
                canFind = value;
                FindNextCommand.RaiseCanExecuteChanged();
                FindAllCommand.RaiseCanExecuteChanged();
            }
        }
        /// <summary>
        /// The command to close the window
        /// </summary>
        public CommandHandler CloseWindowCommand { get; private set; }
        /// <summary>
        /// The command to find the next occurrence.
        /// </summary>
        public CommandHandler FindNextCommand { get; private set; }
        /// <summary>
        /// The command to find all occurrences.
        /// </summary>
        public CommandHandler FindAllCommand { get; private set; }
        #endregion

        #region Events
        /// <summary>
        /// Fires when we want to close the window.
        /// </summary>
        public event EventHandler ClosingRequest;

        #endregion

        #region Constructors
        /// <summary>
        /// The constructor.
        /// </summary>
        /// <param name="seriesListViewModel">A reference to the <see cref="ViewModel.SeriesListViewModel"/></param>
        public FindViewModel()
        {
            direction = true;
            currentSearchPosition = 0;
            FindNextCommand = new CommandHandler(FindByIndex, true);
            FindAllCommand = new CommandHandler(FindAllByIndex, true);
            CloseWindowCommand = new CommandHandler(OnClosingRequest, true);
        }

        public FindViewModel(SeriesToDisplayViewModel seriesVM) : this()
        {
            SeriesListViewModel = seriesVM;
        }
        #endregion

        #region Methods
        private void FindAllByIndex()
        {
            SeriesListViewModel.CurrentIndex = -1;
            SeriesListViewModel.AddSeriesToSelection(SeriesListViewModel.
                RetrievedSeries.Where(x => x.Contains(Search, StringComparison.InvariantCultureIgnoreCase)));
        }

        private void FindByIndex()
        {
            found = false;
            if (Direction) //next
            {
                if (currentSearchPosition == SeriesListViewModel.RetrievedSeries.Count - 1)
                    currentSearchPosition = 0;
                else if (currentSearchPosition < SeriesListViewModel.CurrentIndex)
                    currentSearchPosition = SeriesListViewModel.CurrentIndex;
                for (int i = currentSearchPosition; i < SeriesListViewModel.RetrievedSeries.Count; i++)
                {
                    if (SeriesListViewModel.CurrentIndex == i)
                        continue;
                    if (SeriesListViewModel.RetrievedSeries[i].Contains(Search, StringComparison.InvariantCultureIgnoreCase))
                    {
                        currentSearchPosition = i;
                        SeriesListViewModel.CurrentIndex = i;
                        found = true;
                        break;
                    }
                }
                if (!found)
                {
                    for (int i = 0; i <= currentSearchPosition; i++)
                    {
                        if (SeriesListViewModel.RetrievedSeries[i].Contains(Search, StringComparison.InvariantCultureIgnoreCase))
                        {
                            currentSearchPosition = i;
                            SeriesListViewModel.CurrentIndex = i;
                            found = true;
                            break;
                        }
                    }
                    if (!found)
                        SeriesListViewModel.CurrentIndex = -1;
                }
            }
            else //previous
            {
                if (currentSearchPosition > SeriesListViewModel.CurrentIndex && SeriesListViewModel.CurrentIndex >= 0)
                    currentSearchPosition = SeriesListViewModel.CurrentIndex;
                else if (currentSearchPosition == 0)
                    currentSearchPosition = SeriesListViewModel.RetrievedSeries.Count - 1;

                for (int i = currentSearchPosition; i >= 0; i--)
                {
                    if (SeriesListViewModel.CurrentIndex == i)
                        continue;
                    if (SeriesListViewModel.RetrievedSeries[i].Contains(Search, StringComparison.InvariantCultureIgnoreCase))
                    {
                        currentSearchPosition = i;
                        SeriesListViewModel.CurrentIndex = i;
                        found = true;
                        break;
                    }
                }
                if (!found)
                {
                    for (int i = SeriesListViewModel.RetrievedSeries.Count - 1; i - currentSearchPosition >= 0; i--)
                    {
                        if (SeriesListViewModel.RetrievedSeries[i].Contains(Search, StringComparison.InvariantCultureIgnoreCase))
                        {
                            currentSearchPosition = i;
                            SeriesListViewModel.CurrentIndex = i;
                            found = true;
                            break;
                        }
                    }
                    if (!found)
                        SeriesListViewModel.CurrentIndex = -1;
                }
            }
        }

        private void OnClosingRequest()
        {
            this.ClosingRequest?.Invoke(this, EventArgs.Empty);
        }

        #endregion

    }
}
