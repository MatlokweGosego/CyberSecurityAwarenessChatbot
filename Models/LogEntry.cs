using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CyberSecurityAwarenessChatbot.Models
{
    // LogEntry represents an event recorded in the chatbot's activity log.
    public class LogEntry
    {
        public DateTime Timestamp { get; set; } // When the action occurred
        public string Description { get; set; }  // A brief description of the action

        // Constructor for creating a new log entry
        public LogEntry(string description)
        {
            Timestamp = DateTime.Now; // Log the current time
            Description = description;
        }
    }
}
