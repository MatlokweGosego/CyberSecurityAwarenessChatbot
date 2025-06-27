using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Media;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using CyberSecurityAwarenessChatbot.ViewModels;
using System.Globalization;

namespace CyberSecurityAwarenessChatbot
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            // DataContext is set in XAML, but you can also set it here:
            // this.DataContext = new MainViewModel();

            // Auto-scroll chat history to bottom:
            Loaded += (sender, e) =>
            {
                // Play greeting sound
                PlayGreetingSound();

                var chatScrollViewer = FindVisualChild<ScrollViewer>(this);
                if (chatScrollViewer != null)
                {
                    var chatItemsControl = FindVisualChild<ItemsControl>(chatScrollViewer);
                    if (chatItemsControl != null && chatItemsControl.ItemsSource is System.Collections.Specialized.INotifyCollectionChanged observableCollection)
                    {
                        observableCollection.CollectionChanged += (s, ev) =>
                        {
                            if (ev.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Add)
                            {
                                chatScrollViewer.ScrollToEnd();
                            }
                        };
                    }
                }
            };
        }

        // Method to play the greeting sound from resources
        private void PlayGreetingSound()
        {
            try
            {
                // To access a resource, use the pack URI syntax.
                // Assuming your .wav file is in a folder named 'Sounds' in your project,
                // and its Build Action is set to 'Resource'.
                // The 'component' part refers to the current assembly.
                SoundPlayer player = new SoundPlayer(Application.GetResourceStream(new Uri("Sounds/welcome.wav", UriKind.Relative)).Stream);
                player.Play();
            }
            catch (Exception ex)
            {
                // In a real application, you might log this error or display a message box.
                // For a POE, just catching it is fine.
                MessageBox.Show($"Error playing sound: {ex.Message}", "Sound Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }


        // Helper to find visual child (recursive) - IMPORTANT for finding nested controls
        private static T FindVisualChild<T>(DependencyObject parent) where T : DependencyObject
        {
            if (parent == null) return null;

            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(parent); i++)
            {
                DependencyObject child = VisualTreeHelper.GetChild(parent, i);
                if (child != null && child is T)
                {
                    return (T)child;
                }
                else
                {
                    T childOfChild = FindVisualChild<T>(child);
                    if (childOfChild != null)
                    {
                        return childOfChild;
                    }
                }
            }
            return null;
        }

        // Handle Enter key press in the TextBox to send message
        private void UserInput_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                var viewModel = DataContext as MainViewModel;
                if (viewModel != null && viewModel.SendCommand.CanExecute(null))
                {
                    viewModel.SendCommand.Execute(null);
                    e.Handled = true; // Prevent default Enter key behavior (e.g., new line)
                }
            }
        }
    }

    // Converter to invert a boolean value for Visibility
    public class InvertedBooleanToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool boolValue)
            {
                return boolValue ? Visibility.Collapsed : Visibility.Visible;
            }
            return Visibility.Visible; // Default to visible if not a boolean
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    // Attached property to set focus to a TextBox. Useful for automatically focusing input.
    public static class FocusExtension
    {
        public static readonly DependencyProperty IsFocusedProperty =
            DependencyProperty.RegisterAttached(
                "IsFocused", typeof(bool), typeof(FocusExtension),
                new UIPropertyMetadata(false, OnIsFocusedPropertyChanged));

        public static bool GetIsFocused(DependencyObject obj)
        {
            return (bool)obj.GetValue(IsFocusedProperty);
        }

        public static void SetIsFocused(DependencyObject obj, bool value)
        {
            obj.SetValue(IsFocusedProperty, value);
        }

        private static void OnIsFocusedPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var uiElement = d as UIElement;
            if (uiElement != null && (bool)e.NewValue)
            {
                uiElement.Dispatcher.BeginInvoke(
                    new Action(() => uiElement.Focus()),
                    System.Windows.Threading.DispatcherPriority.Input);
            }

        }
    }
}
