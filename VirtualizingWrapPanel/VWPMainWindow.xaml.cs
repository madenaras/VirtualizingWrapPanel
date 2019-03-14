using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace VWPTestApp
{
    /// <summary>
    /// Interaction logic for VWPMainWindow.xaml
    /// </summary>
    public partial class VWPMainWindow : Window
    {
        bool isMouseDown;
        int maximumIndex;
        private int initialIntex;
        private double scaleValue = 1;
        DataContext dataContext;
        public VWPMainWindow()
        {
            InitializeComponent();
            dataContext = new DataContext();
            SeriesListView.DataContext = dataContext;
            Clean.Command = new CommandHandler(() => Clear(), true);
            load50.Command = new CommandHandler(() => Load(50), true);
            load250.Command = new CommandHandler(() => Load(250), true);
            load1000.Command = new CommandHandler(() => Load(1000), true);
            loadAll.Command = new CommandHandler(() => Load(int.MaxValue), true);
            add250.Command = new CommandHandler(() => Add(250), true);
            add1000.Command = new CommandHandler(() => Add(1000), true);
            toggleVPW.Command = new CommandHandler(() => EnableVWP(), true);
        }

        private void EnableVWP()
        {
            dataContext.TooManyItems = toggleVPW.IsChecked ?? false;
        }

        private void Add(int items)
        {
            Debug.WriteLine($"ADDING {items} ITEMS");
            dataContext.Add(items);
        }
        private void Load(int items)
        {
            Debug.WriteLine($"LOADING {items} ITEMS");
            dataContext.Load(items);
        }

        private void Clear()
        {
            dataContext.RetrievedSeries.Clear();
        }

        protected override void OnPreviewMouseWheel(MouseWheelEventArgs args)
        {
            base.OnPreviewMouseWheel(args);
            if (Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl))
            {
                scaleValue += (args.Delta > 0) ? 0.1 : -0.1;
                this.SetScaling(Scale, scaleValue);
            }
        }

        protected override void OnPreviewMouseDown(MouseButtonEventArgs args)
        {
            base.OnPreviewMouseDown(args);
            if (Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl))
            {
                if (args.MiddleButton == MouseButtonState.Pressed)
                {
                    scaleValue = 1;
                    this.SetScaling(Scale, scaleValue);
                }
            }
        }

        private void TextBlock_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed && dataContext != null)
            {
                if (sender is ListViewItem serie)
                {
                    var visibleSeries = dataContext.RetrievedSeries.ToList();
                    SeriesListView.SelectionMode = SelectionMode.Multiple;

                    var thisSerie = serie.Content.ToString();//serie.DataContext as SeriesViewModel;
                    var currentSelection = dataContext.RetrievedSeries.FirstOrDefault(x => x == thisSerie);
                    if (currentSelection == null)
                        return;

                    var indexOfFirst = SeriesListView.SelectedIndex;
                    var indexOfLast = visibleSeries.IndexOf(currentSelection);

                    if (!isMouseDown)
                    {
                        initialIntex = indexOfFirst;
                        maximumIndex = indexOfLast;
                        isMouseDown = true;
                    }
                    if (indexOfFirst == indexOfLast)
                    {
                        SeriesListView.UnselectAll();
                        SeriesListView.SelectedItems.Add(visibleSeries[indexOfFirst]);
                    }
                    else if (indexOfFirst < indexOfLast)
                    {
                        for (int i = SeriesListView.SelectedItems.Count - 1; i > 0; i--)
                        {
                            SeriesListView.SelectedItems.RemoveAt(i);
                        }
                        for (int i = indexOfFirst; i <= indexOfLast; i++)
                        {
                            //if (visibleSeries[i].IsVisible)
                                SeriesListView.SelectedItems.Add(visibleSeries[i]);
                        }
                    }
                    else if (indexOfFirst > indexOfLast)
                    {
                        for (int i = SeriesListView.SelectedItems.Count - 1; i > 0; i--)
                        {
                            SeriesListView.SelectedItems.RemoveAt(i);
                        }
                        for (int i = indexOfFirst; i >= indexOfLast; i--)
                        {
                            //if (visibleSeries[i].IsVisible)
                                SeriesListView.SelectedItems.Add(visibleSeries[i]);
                        }
                    }
                }
            }
        }

        private void SeriesListView_MouseUp(object sender, MouseButtonEventArgs e)
        {
            SeriesListView.SelectionMode = SelectionMode.Extended;
            if (isMouseDown)
            {
                isMouseDown = false;
                initialIntex = -1;
                Debug.WriteLine("Selection Changed SeriesListView_MouseUp");
                //fire selection changed event
            }
        }

        private void ScrollViewer_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            // Get the border of the listview (first child of a listview)
            if (VisualTreeHelper.GetChild(SeriesListView, 0) is Decorator border)
            {
                ScrollViewer scrollviewer = border.Child as ScrollViewer;
                if (e.Delta > 0)
                {
                    for (int i = 0; i < 5; i++)
                    {
                        scrollviewer.LineLeft();
                    }
                }

                else
                {
                    for (int i = 0; i < 5; i++)
                    {
                        scrollviewer.LineRight();
                    }
                }
            }
            e.Handled = true;
        }
    }
    class DataContext : ViewModelBase
    {
        readonly string dataPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory + @"\..\..\Resources\data.txt");
        readonly string[] potentialSeries;
        public ObservableCollection<string> RetrievedSeries { get; private set; }
        public DataContext()
        {
            potentialSeries = File.ReadAllText(dataPath).Split(new string[] { "\n", "\r" }, StringSplitOptions.RemoveEmptyEntries);
            RetrievedSeries = new ObservableCollection<string>();
        }

        internal void Load(int items)
        {
            if (items > potentialSeries.Length)
                items = potentialSeries.Length;
            RetrievedSeries.Clear();
            for (int i = 0; i < items; i++)
            {
                RetrievedSeries.Add(potentialSeries[i]);
            }
        }

        internal void Add(int items)
        {
            for (int i = 0; i < items; i++)
            {
                RetrievedSeries.Add(potentialSeries[i]);
            }
        }

        private bool tooManyItems;
        public bool TooManyItems
        {
            get { return tooManyItems; }
            set
            {
                if (value != tooManyItems)
                {
                    tooManyItems = value;
                    RaisePropertyChanged("TooManyItems");
                }
            }
        }
    }
    public class CommandHandler : ICommand
    {
        private readonly Action _action;
        private readonly bool _canExecute;
        public CommandHandler(Action action, bool canExecute)
        {
            _action = action;
            _canExecute = canExecute;
        }

        public bool CanExecute(object parameter)
        {
            return _canExecute;
        }

        public event EventHandler CanExecuteChanged;

        public void Execute(object parameter)
        {
            _action();
        }
    }
    public class ViewModelBase : INotifyPropertyChanged
    {
        public void RaisePropertyChanged(string prop)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(prop));
        }
        public event PropertyChangedEventHandler PropertyChanged;
    }
    public static class UserControlExtensions
    {
        public static void SetScaling(this ContentControl control, ScaleTransform scale, double value)
        {
            scale.ScaleX = value;
            scale.ScaleY = value;
        }
    }
}
