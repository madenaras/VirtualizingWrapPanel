using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace VWPTestApp
{
    public class DragSelectionHelper : DependencyObject
    {
        #region Random Static Properties

        // need a static reference to the listbox otherwise it can't be accessed
        // (this only happened in the project I'm working on, if you're using a regular ListBox, with regular ListBoxItems you can get the ListBox from the ListBoxItems)
        public static ListView ListBox { get; private set; }

        #endregion Random Static Properties

        #region IsDragSelectionEnabledProperty

        public static bool GetIsDragSelectionEnabled(DependencyObject obj)
        {
            return (bool)obj.GetValue(IsDragSelectionEnabledProperty);
        }

        public static void SetIsDragSelectionEnabled(DependencyObject obj, bool value)
        {
            obj.SetValue(IsDragSelectionEnabledProperty, value);
        }

        public static readonly DependencyProperty IsDragSelectionEnabledProperty =
            DependencyProperty.RegisterAttached("IsDragSelectingEnabled", typeof(bool), typeof(DragSelectionHelper), new UIPropertyMetadata(false, IsDragSelectingEnabledPropertyChanged));

        public static void IsDragSelectingEnabledPropertyChanged(DependencyObject o, DependencyPropertyChangedEventArgs e)
        {
            ListView listBox = o as ListView;

            bool isDragSelectionEnabled = DragSelectionHelper.GetIsDragSelectionEnabled(listBox);

            // if DragSelection is enabled
            if (isDragSelectionEnabled)
            {
                // set the listbox's selection mode to multiple ( didn't work with extended )
                listBox.SelectionMode = SelectionMode.Multiple;

                // set the static listbox property
                DragSelectionHelper.ListBox = listBox;

                // and subscribe to the required events to handle the drag selection and the attached properties
                listBox.PreviewMouseLeftButtonDown += new MouseButtonEventHandler(DragSelectionHelper.listBox_PreviewMouseLeftButtonDown);
                listBox.PreviewMouseRightButtonDown += new MouseButtonEventHandler(listBox_PreviewMouseRightButtonDown);
                listBox.MouseLeftButtonUp += new MouseButtonEventHandler(DragSelectionHelper.listBox_MouseLeftButtonUp);
            }
            else // is selection is disabled
            {
                // set selection mode to the default
                listBox.SelectionMode = SelectionMode.Single;

                // dereference the listbox
                DragSelectionHelper.ListBox = null;

                // unsuscribe from the events
                listBox.PreviewMouseLeftButtonDown -= new MouseButtonEventHandler(DragSelectionHelper.listBox_PreviewMouseLeftButtonDown);
                listBox.MouseLeftButtonUp -= new MouseButtonEventHandler(DragSelectionHelper.listBox_MouseLeftButtonUp);
                listBox.MouseLeftButtonUp -= new MouseButtonEventHandler(DragSelectionHelper.listBox_MouseLeftButtonUp);
            }
        }

        static void listBox_PreviewMouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            // to prevent the listbox from selecting / deselecting wells on right click
            e.Handled = true;
        }

        private static void listBox_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            // notify the helper class that the listbox has initiated the drag click
            DragSelectionHelper.SetIsDragClickStarted(DragSelectionHelper.ListBox, true);
        }

        private static void listBox_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            // notify the helper class that the list box has terminated the drag click
            DragSelectionHelper.SetIsDragClickStarted(DragSelectionHelper.ListBox, false);
        }

        #endregion IsDragSelectionEnabledProperty

        #region IsDragSelectinProperty

        public static bool GetIsDragSelecting(DependencyObject obj)
        {
            return (bool)obj.GetValue(IsDragSelectingProperty);
        }

        public static void SetIsDragSelecting(DependencyObject obj, bool value)
        {
            obj.SetValue(IsDragSelectingProperty, value);
        }

        public static readonly DependencyProperty IsDragSelectingProperty =
            DependencyProperty.RegisterAttached("IsDragSelecting", typeof(bool), typeof(DragSelectionHelper), new UIPropertyMetadata(false, IsDragSelectingPropertyChanged));

        public static void IsDragSelectingPropertyChanged(DependencyObject o, DependencyPropertyChangedEventArgs e)
        {
            ListViewItem item = o as ListViewItem;

            bool clickInitiated = DragSelectionHelper.GetIsDragClickStarted(DragSelectionHelper.ListBox);

            // this is where the item.Parent was null, it was supposed to be the ListBox, I guess it's null because items are not
            // really ListBoxItems but are wells
            if (clickInitiated)
            {
                bool isDragSelecting = DragSelectionHelper.GetIsDragSelecting(item);

                if (isDragSelecting)
                {
                    // using the ListBox static reference because could not get to it through the item.Parent property
                    DragSelectionHelper.ListBox.SelectedItems.Add(item);
                }
            }
        }

        #endregion IsDragSelectinProperty

        #region IsDragClickStartedProperty

        public static bool GetIsDragClickStarted(DependencyObject obj)
        {
            return (bool)obj.GetValue(IsDragClickStartedProperty);
        }

        public static void SetIsDragClickStarted(DependencyObject obj, bool value)
        {
            obj.SetValue(IsDragClickStartedProperty, value);
        }

        public static readonly DependencyProperty IsDragClickStartedProperty =
            DependencyProperty.RegisterAttached("IsDragClickStarted", typeof(bool), typeof(DragSelectionHelper), new UIPropertyMetadata(false, IsDragClickStartedPropertyChanged));

        public static void IsDragClickStartedPropertyChanged(DependencyObject o, DependencyPropertyChangedEventArgs e)
        {
            bool isDragClickStarted = DragSelectionHelper.GetIsDragClickStarted(DragSelectionHelper.ListBox);

            // if click has been drag click has started, clear the current selected items and start drag selection operation again
            if (isDragClickStarted)
                DragSelectionHelper.ListBox.SelectedItems.Clear();
        }

        #endregion IsDragClickInitiatedProperty
    }
    public static class StringExtensions
    {
        public static bool Contains(this string source, string toCheck, StringComparison comp)
        {
            return source?.IndexOf(toCheck, comp) >= 0;
        }
    }
}
