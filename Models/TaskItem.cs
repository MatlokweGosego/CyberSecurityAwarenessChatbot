using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;
using CyberSecurityAwarenessChatbot.Utility;
using System.Runtime.CompilerServices;

namespace CyberSecurityAwarenessChatbot.Models
{

    public class TaskItem : ObservableObject
    {
        private string _title;
        public string Title
        {
            get => _title;
            set => SetProperty(ref _title, value);
        }

        private string _description;
        public string Description
        {
            get => _description;
            set => SetProperty(ref _description, value);
        }

        private DateTime? _reminderDate;
        public DateTime? ReminderDate
        {
            get => _reminderDate;
            set
            {
                SetProperty(ref _reminderDate, value);
                OnPropertyChanged(nameof(HasReminder));
                OnPropertyChanged(nameof(ReminderInfo));
            }
        }

        private bool _isCompleted;
        public bool IsCompleted
        {
            get => _isCompleted;
            set => SetProperty(ref _isCompleted, value);
        }

        // Helper property to check if a reminder is set
        public bool HasReminder => ReminderDate.HasValue;

        // Formatted string for displaying reminder information
        public string ReminderInfo
        {
            get
            {
                if (HasReminder)
                {
                    return $"Reminder set for: {ReminderDate.Value:yyyy-MM-dd HH:mm}";
                }
                return string.Empty;
            }
        }

        // Default constructor for serialization (if needed)
        public TaskItem() { }

        // Constructor for creating new tasks
        public TaskItem(string title, string description = "", DateTime? reminderDate = null)
        {
            Title = title;
            Description = description;
            ReminderDate = reminderDate;
            IsCompleted = false;
        }
    }
}
