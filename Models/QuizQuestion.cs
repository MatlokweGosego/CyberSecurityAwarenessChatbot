using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CyberSecurityAwarenessChatbot.Utility;

namespace CyberSecurityAwarenessChatbot.Models
{
    // QuizQuestion represents a single question in the cybersecurity quiz.  
    public class QuizQuestion : ObservableObject
    {
        public string QuestionText { get; set; }
        public List<string> Options { get; set; } // List of possible answers
        public string CorrectAnswer { get; set; } // The correct answer text
        public string Explanation { get; set; }   // Explanation for the answer

        // Property to hold the user's selected answer (for RadioButton binding)
        private string _selectedOption;
        public string SelectedOption
        {
            get => _selectedOption;
            set => SetProperty(ref _selectedOption, value);
        }

        // Constructor for a new quiz question
        public QuizQuestion(string questionText, List<string> options, string correctAnswer, string explanation)
        {
            QuestionText = questionText;
            Options = options;
            CorrectAnswer = correctAnswer;
            Explanation = explanation;
        }

        // Helper method to check if the user's selected answer is correct
        public bool CheckAnswer()
        {
            // Case-insensitive comparison for flexibility
            return !string.IsNullOrEmpty(SelectedOption) &&
                   SelectedOption.Equals(CorrectAnswer, System.StringComparison.OrdinalIgnoreCase);
        }
    }
}
