using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;
using CyberSecurityAwarenessChatbot.ViewModels;
using System.Runtime.CompilerServices;

namespace CyberSecurityAwarenessChatbot.Utility
{
 
    // Base class for ViewModels to implement INotifyPropertyChanged.
    // This allows the UI to automatically update when bound properties change.
    public class ObservableObject : INotifyPropertyChanged
    {
        // Event that consumers (like WPF UI) can subscribe to for property change notifications.
        public event PropertyChangedEventHandler PropertyChanged;

        // Helper method to raise the PropertyChanged event.
        // [CallerMemberName] attribute automatically injects the name of the calling property.
        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            // Safely invoke the event handlers if any are subscribed.
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        // Helper method to set a property's value and automatically raise PropertyChanged
        // if the new value is different from the old value.
        protected bool SetProperty<T>(ref T storage, T value, [CallerMemberName] string propertyName = null)
        {
            // Check if the new value is equal to the existing value to prevent unnecessary updates.
            if (Equals(storage, value))
            {
                return false; // Value has not changed, no need to update or notify.
            }

            // Update the backing field with the new value.
            storage = value;
            // Notify subscribers that the property has changed.
            OnPropertyChanged(propertyName);
            return true; // Value has changed and notification has been sent.
        }
    }
}
