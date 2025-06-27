using CyberSecurityAwarenessChatbot.Models; 
using CyberSecurityAwarenessChatbot.Utility; 
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Media; 
using System.Windows.Media;
using System.Text.RegularExpressions; // For more advanced NLP simulation
using System.Text; // For StringBuilder

namespace CyberSecurityAwarenessChatbot.ViewModels // Corrected namespace
{
    // MainViewModel orchestrates all the logic for the chatbot GUI.
    // It implements ObservableObject to notify the View of property changes.
    public class MainViewModel : ObservableObject
    {
        private string _userName = "User"; // Default user name
        public string UserName
        {
            get => _userName;
            set => SetProperty(ref _userName, value);
        }

        // Chat History for display in the UI
        private ObservableCollection<ChatMessage> _chatHistory;
        public ObservableCollection<ChatMessage> ChatHistory
        {
            get => _chatHistory;
            set => SetProperty(ref _chatHistory, value);
        }

        // User input text box content
        private string _userInput;
        public string UserInput
        {
            get => _userInput;
            set => SetProperty(ref _userInput, value);
        }

        // Task Management Properties
        private ObservableCollection<TaskItem> _tasks;
        public ObservableCollection<TaskItem> Tasks
        {
            get => _tasks;
            set => SetProperty(ref _tasks, value);
        }

        private string _newTaskTitle;
        public string NewTaskTitle
        {
            get => _newTaskTitle;
            set
            {
                SetProperty(ref _newTaskTitle, value);
                // Automatically set focus when the NewTaskTitle property is updated programmatically
                if (!string.IsNullOrEmpty(value) && value.Length == 1) // Just a simple trigger
                {
                    IsAddTaskFocused = true;
                }
            }
        }

        private string _newTaskDescription;
        public string NewTaskDescription
        {
            get => _newTaskDescription;
            set => SetProperty(ref _newTaskDescription, value);
        }

        private bool _setReminder;
        public bool SetReminder
        {
            get => _setReminder;
            set => SetProperty(ref _setReminder, value);
        }

        private ObservableCollection<string> _reminderTimeframes;
        public ObservableCollection<string> ReminderTimeframes
        {
            get => _reminderTimeframes;
            set => SetProperty(ref _reminderTimeframes, value);
        }

        private string _selectedReminderTimeframe;
        public string SelectedReminderTimeframe
        {
            get => _selectedReminderTimeframe;
            set => SetProperty(ref _selectedReminderTimeframe, value);
        }

        private bool _isAddTaskFocused;
        public bool IsAddTaskFocused
        {
            get => _isAddTaskFocused;
            set => SetProperty(ref _isAddTaskFocused, value);
        }


        // Quiz Game Properties
        private List<QuizQuestion> _allQuizQuestions;
        private int _currentQuestionIndex;

        private QuizQuestion _currentQuestion;
        public QuizQuestion CurrentQuestion
        {
            get => _currentQuestion;
            set => SetProperty(ref _currentQuestion, value);
        }

        private bool _isQuizInProgress;
        public bool IsQuizInProgress
        {
            get => _isQuizInProgress;
            set
            {
                SetProperty(ref _isQuizInProgress, value);
                OnPropertyChanged(nameof(IsQuizNotStarted));
            }
        }
        public bool IsQuizNotStarted => !_isQuizInProgress;


        private bool _isQuizFinished;
        public bool IsQuizFinished
        {
            get => _isQuizFinished;
            set => SetProperty(ref _isQuizFinished, value);
        }

        private int _quizScore;
        public int QuizScore
        {
            get => _quizScore;
            set => SetProperty(ref _quizScore, value);
        }

        private string _quizFeedback;
        public string QuizFeedback
        {
            get => _quizFeedback;
            set => SetProperty(ref _quizFeedback, value);
        }

        private string _finalScoreMessage;
        public string FinalScoreMessage
        {
            get => _finalScoreMessage;
            set => SetProperty(ref _finalScoreMessage, value);
        }

        // Activity Log Properties
        private ObservableCollection<LogEntry> _activityLog;
        public ObservableCollection<LogEntry> ActivityLog
        {
            get => _activityLog;
            set => SetProperty(ref _activityLog, value);
        }

        public ObservableCollection<LogEntry> DisplayedActivityLog =>
            new ObservableCollection<LogEntry>(ActivityLog.Reverse().Take(10)); // Display last 10 entries

        // Commands
        public RelayCommand SendCommand { get; private set; }
        public RelayCommand AddTaskCommand { get; private set; }
        public RelayCommand DeleteTaskCommand { get; private set; }
        public RelayCommand StartQuizCommand { get; private set; }
        public RelayCommand SubmitAnswerCommand { get; private set; }
        public RelayCommand ClearActivityLogCommand { get; private set; }

        // Chatbot's knowledge base
        private Dictionary<string, List<string>> _keywordResponses;
        private Dictionary<string, List<string>> _sentimentResponses;
        private Random _random = new Random();
        private string _currentTopic = null; // Stores the current cybersecurity topic for context
        private string _favoriteTopic = null;
        private bool _askedAboutFavorite = false;


        // Constructor
        public MainViewModel()
        {
            ChatHistory = new ObservableCollection<ChatMessage>();
            Tasks = new ObservableCollection<TaskItem>();
            ReminderTimeframes = new ObservableCollection<string> { "None", "1 day", "3 days", "7 days", "1 month" };
            SelectedReminderTimeframe = "None"; // Default selection
            ActivityLog = new ObservableCollection<LogEntry>();

            InitializeCommands();
            InitializeResponses();
            InitializeQuizQuestions();

            // Initial greeting
            AppendChatbotMessage($"Hello! Welcome to the Cybersecurity Chatbot. I am your Assistant.");
            AppendChatbotMessage($"What is your name?:)");
        }

        // Initializes all RelayCommands
        private void InitializeCommands()
        {
            SendCommand = new RelayCommand(ExecuteSendCommand, CanExecuteSendCommand);
            AddTaskCommand = new RelayCommand(ExecuteAddTaskCommand, CanExecuteAddTaskCommand);
            DeleteTaskCommand = new RelayCommand(ExecuteDeleteTaskCommand, CanExecuteDeleteTaskCommand);
            StartQuizCommand = new RelayCommand(ExecuteStartQuizCommand, CanExecuteStartQuizCommand);
            SubmitAnswerCommand = new RelayCommand(ExecuteSubmitAnswerCommand, CanExecuteSubmitAnswerCommand);
            ClearActivityLogCommand = new RelayCommand(ExecuteClearActivityLogCommand);
        }

        // Initializes the chatbot's knowledge base and sentiment responses
        private void InitializeResponses()
        {
            _keywordResponses = new Dictionary<string, List<string>>(StringComparer.OrdinalIgnoreCase)
            {
                { "password", new List<string>
                    {
                        "A strong password should be at least 12 characters long and include a mix of uppercase/lowercase letters, numbers, and symbols.",
                        "Avoid using personal information like names or birthdays in your passwords.",
                        "Random passwords are much harder to guess for both humans and computers.",
                        "Never write down your password or store it in easily accessible digital files. Consider a password manager!",
                        "Multi-factor authentication (MFA) adds a crucial layer of security, even if your password is compromised."
                    }
                },
                { "phishing", new List<string>
                    {
                        "Phishing attempts often use urgent language to trick you into clicking suspicious links or sharing personal info.",
                        "Always examine the sender's email address closely for inconsistencies. It might look legitimate at first glance.",
                        "Look for poor grammar, spelling errors, or awkward phrasing in emails and messages.",
                        "If an email asks for sensitive information, verify it through an official channel (e.g., call the company directly) before responding.",
                        "Never click on suspicious links. Hover over them to see the actual URL before clicking."
                    }
                },
                { "safe browsing", new List<string>
                    {
                        "Keep your browser and operating system updated to patch security vulnerabilities.",
                        "Use a reputable antivirus software and ensure it's always up to date.",
                        "Be cautious about websites asking for sensitive information; always check for 'https://' and a padlock icon in the URL.",
                        "Avoid downloading files or clicking on pop-ups from untrusted sources.",
                        "A firewall acts as a barrier between your computer and the internet, controlling incoming and outgoing network traffic."
                    }
                },
                { "mfa", new List<string>
                    {
                        "Multi-factor authentication (MFA) requires more than one method to verify your identity, significantly increasing security.",
                        "It typically combines something you know (like a password), something you have (like a phone or security key), or something you are (like a fingerprint).",
                        "MFA makes it much harder for attackers to gain access to your accounts, even if they steal your password.",
                        "Always enable MFA wherever it's available for your online accounts."
                    }
                },
                { "malware", new List<string>
                    {
                        "Malware is malicious software designed to harm or exploit devices, services, or networks.",
                        "Common types of malware include viruses, worms, Trojans, ransomware, and spyware.",
                        "Keep all your software, including operating systems and applications, updated to protect against known malware vulnerabilities.",
                        "Install and regularly scan with reputable antivirus software.",
                        "Be very careful about what you download, especially from unofficial sources, and always scan downloads before opening them."
                    }
                }
            };

            _sentimentResponses = new Dictionary<string, List<string>>(StringComparer.OrdinalIgnoreCase)
            {
                { "worried", new List<string>
                    {
                        "It's understandable to feel worried about online threats. I'm here to help you understand how to feel more secure.",
                        "Don't worry, we can tackle these concerns together. What specifically is on your mind?"
                    }
                },
                { "curious", new List<string>
                    {
                        "That's great that you're curious! Asking questions is the first step to staying informed about cybersecurity.",
                        "Being curious about cybersecurity is a good thing! What aspects are you most interested in exploring?"
                    }
                },
                { "frustrated", new List<string>
                    {
                        "I understand it can be frustrating dealing with online security complexities. Let's try to break it down step by step.",
                        "It's okay to feel frustrated. I'll do my best to make this clearer and easier for you to understand."
                    }
                },
                { "unsure", new List<string>
                    {
                        "It's alright to feel unsure. That's what I'm here for! Please, ask away and I'll do my best to clarify.",
                        "No problem at all. Let me reassure you and clarify things. What would you like to know more about?"
                    }
                },
                { "help", new List<string>
                    {
                        "I'm here to assist you with cybersecurity awareness. What specifically do you need help with today?",
                        "How can I help you stay safer online? Just ask me about topics like passwords, phishing, malware, or safe browsing."
                    }
                }
            };
        }

        // Initializes the quiz questions
        private void InitializeQuizQuestions()
        {
            _allQuizQuestions = new List<QuizQuestion>
            {
                new QuizQuestion(
                    "What is the primary purpose of Multi-Factor Authentication (MFA)?",
                    new List<string> { "A) To make passwords longer", "B) To add an extra layer of security beyond just a password", "C) To speed up login times", "D) To store passwords more securely" },
                    "B) To add an extra layer of security beyond just a password",
                    "Explanation: MFA requires more than one verification method, significantly enhancing security."
                ),
                new QuizQuestion(
                    "True or False: It is safe to click on any link in an email as long as it comes from a recognized sender.",
                    new List<string> { "True", "False" },
                    "False",
                    "Explanation: Phishing emails can spoof sender addresses. Always hover over links to check their true destination, or verify with the sender directly."
                ),
                new QuizQuestion(
                    "Which of these is NOT a good practice for creating a strong password?",
                    new List<string> { "A) Using a mix of uppercase, lowercase, numbers, and symbols", "B) Making it at least 12 characters long", "C) Using easily memorable personal information like your birthdate", "D) Using a unique password for each account" },
                    "C) Using easily memorable personal information like your birthdate",
                    "Explanation: Personal information is easy for others to guess or find. Passwords should be random and unique."
                ),
                new QuizQuestion(
                    "What is 'malware'?",
                    new List<string> { "A) A type of secure web browser", "B) Software designed to harm or exploit computer systems", "C) A method of securely storing data online", "D) A secure network protocol" },
                    "B) Software designed to harm or exploit computer systems",
                    "Explanation: Malware is a broad term for malicious software like viruses, ransomware, and spyware."
                ),
                new QuizQuestion(
                    "True or False: Public Wi-Fi networks are generally safe for accessing sensitive information like banking.",
                    new List<string> { "True", "False" },
                    "False",
                    "Explanation: Public Wi-Fi is often unsecured, making your data vulnerable to interception by others on the same network. Use a VPN if you must use public Wi-Fi for sensitive tasks."
                ),
                new QuizQuestion(
                    "What is the term for deceiving individuals into divulging sensitive information, often through fake websites or emails?",
                    new List<string> { "A) Hacking", "B) Phishing", "C) Firewalling", "D) Encryption" },
                    "B) Phishing",
                    "Explanation: Phishing uses deceptive means to trick users into revealing confidential data."
                ),
                new QuizQuestion(
                    "Which action helps protect against ransomware?",
                    new List<string> { "A) Regularly backing up your data", "B) Disabling your firewall", "C) Clicking on suspicious attachments", "D) Using simple passwords" },
                    "A) Regularly backing up your data",
                    "Explanation: Regular backups ensure you can restore your files if they are encrypted by ransomware."
                ),
                new QuizQuestion(
                    "True or False: It's okay to reuse the same password for multiple less important online accounts.",
                    new List<string> { "True", "False" },
                    "False",
                    "Explanation: If one account is compromised, all accounts using that password become vulnerable. Use unique passwords or a password manager."
                ),
                new QuizQuestion(
                    "What does 'HTTPS' indicate in a website's URL?",
                    new List<string> { "A) The website is very popular", "B) The connection to the website is encrypted and secure", "C) The website is under construction", "D) The website contains images" },
                    "B) The connection to the website is encrypted and secure",
                    "Explanation: HTTPS (Hypertext Transfer Protocol Secure) means communication is encrypted, protecting your data."
                ),
                new QuizQuestion(
                    "Which of these is a social engineering technique?",
                    new List<string> { "A) Using a firewall", "B) Software updates", "C) Pretending to be someone trustworthy to gain information", "D) Encrypting files" },
                    "C) Pretending to be someone trustworthy to gain information",
                    "Explanation: Social engineering manipulates people into performing actions or divulging confidential information."
                )
            };
        }


        // Chatbot interaction logic
        private bool CanExecuteSendCommand(object parameter)
        {
            return !string.IsNullOrWhiteSpace(UserInput);
        }

        private void ExecuteSendCommand(object parameter)
        {
            string input = UserInput.Trim();
            AppendUserMessage(input);
            LogActivity($"User input: \"{input}\""); // Log user input

            // Handle initial name input
            if (UserName == "User" && !string.IsNullOrWhiteSpace(input) && !input.Equals("exit", StringComparison.OrdinalIgnoreCase))
            {
                UserName = input.Split(' ')[0]; // Take first word as name
                AppendChatbotMessage($"Hello {UserName}! I am your cybersecurity assistant. I am here to educate you about cybersecurity and help keep you safe online!");
                AppendChatbotMessage($"You can ask me about cybersecurity topics like passwords, phishing, malware, safe browsing, or multi-factor authentication (MFA).");
                AppendChatbotMessage($"Type 'exit' to end this conversation.");
                LogActivity($"Chatbot greeted user: {UserName}");
                UserInput = string.Empty;
                return;
            }

            ProcessChatbotResponse(input);
            UserInput = string.Empty; // Clear input field
        }

        // Main logic for processing user input and generating chatbot responses (NLP Simulation)
        private void ProcessChatbotResponse(string userInput)
        {
            bool responded = false;
            string detectedTopic = null; // To track the most recently discussed cybersecurity topic

            // 1. Check for commands (Quiz, Tasks, Log) first
            if (userInput.Contains("start quiz") || userInput.Contains("play quiz") || userInput.Contains("quiz me"))
            {
                ExecuteStartQuizCommand(null);
                responded = true;
            }
            else if (userInput.Contains("show tasks") || userInput.Contains("view tasks") || userInput.Contains("my tasks"))
            {
                DisplayTasksSummary();
                responded = true;
            }
            else if (userInput.Contains("add task") || userInput.Contains("create task") || userInput.Contains("new task"))
            {
                HandleAddTaskCommandFromChat(userInput);
                responded = true;
            }
            else if (userInput.Contains("show activity log") || userInput.Contains("view log") || userInput.Contains("what have you done"))
            {
                DisplayActivityLog();
                responded = true;
            }
            else if (userInput.Contains("clear log") || userInput.Contains("reset log"))
            {
                ExecuteClearActivityLogCommand(null);
                responded = true;
            }
            else if (userInput.Contains("delete task") || userInput.Contains("remove task"))
            {
                HandleDeleteTaskFromChat(userInput);
                responded = true;
            }
            else if (userInput.Contains("mark task completed") || userInput.Contains("complete task"))
            {
                HandleCompleteTaskFromChat(userInput);
                responded = true;
            }
            else if (userInput.Contains("remind me to") || userInput.Contains("set reminder for"))
            {
                HandleReminderFromChat(userInput);
                responded = true;
            }


            // 2. Check for sentiment keywords
            if (!responded)
            {
                foreach (var pair in _sentimentResponses)
                {
                    if (userInput.Contains(pair.Key))
                    {
                        AppendChatbotMessage(GetRandomResponse(pair.Value));
                        responded = true;
                        LogActivity($"Chatbot responded to sentiment: {pair.Key}");
                        break;
                    }
                }
            }

            // 3. Check for core cybersecurity keywords
            if (!responded)
            {
                foreach (var pair in _keywordResponses)
                {
                    if (userInput.Contains(pair.Key))
                    {
                        AppendChatbotMessage(GetRandomResponse(pair.Value));
                        detectedTopic = pair.Key; // Update detected topic
                        responded = true;
                        LogActivity($"Chatbot responded to topic: {pair.Key}");
                        break;
                    }
                }
            }

            // 4. Handle specific "favourite" or "interested" queries to set favoriteTopic
            if (!responded)
            {
                if ((userInput.Contains("favorite") && userInput.Contains("topic")) || (userInput.Contains("interested in") && !string.IsNullOrWhiteSpace(userInput.Replace("interested in", "").Trim())))
                {
                    string potentialTopic = null;
                    Match match;

                    // Try to extract topic after "interested in"
                    match = Regex.Match(userInput, @"interested in\s+(.*)");
                    if (match.Success)
                    {
                        potentialTopic = match.Groups[1].Value.Trim();
                    }
                    else
                    {
                        // Fallback: If they say "favorite topic is phishing"
                        foreach (var key in _keywordResponses.Keys)
                        {
                            if (userInput.Contains(key))
                            {
                                potentialTopic = key;
                                break;
                            }
                        }
                    }

                    if (!string.IsNullOrEmpty(potentialTopic) && (_keywordResponses.ContainsKey(potentialTopic) || _sentimentResponses.ContainsKey(potentialTopic)))
                    {
                        _favoriteTopic = potentialTopic;
                        AppendChatbotMessage($"Great, {UserName}! I'll remember that you're interested in '{_favoriteTopic}'. It's a crucial part of staying safe online.");
                        _askedAboutFavorite = true;
                        responded = true;
                        LogActivity($"User expressed interest in favorite topic: {_favoriteTopic}");
                    }
                    else if (_askedAboutFavorite) // If we previously asked and they give a vague answer
                    {
                        AppendChatbotMessage("Could you tell me what specific topic you're interested in? (e.g., password, phishing, malware)");
                        responded = true;
                    }
                }
            }


            // 5. Handle follow-up questions related to the current topic or favorite topic
            if (!responded && _currentTopic != null)
            {
                if ((userInput.Contains("more") || userInput.Contains("explain") || userInput.Contains("tell me about")) &&
                    _keywordResponses.ContainsKey(_currentTopic))
                {
                    AppendChatbotMessage($"More on '{_currentTopic}': {GetRandomResponse(_keywordResponses[_currentTopic])}");
                    responded = true;
                    LogActivity($"Chatbot provided more info on current topic: {_currentTopic}");
                }
            }

            if (!responded && _favoriteTopic != null)
            {
                if ((userInput.Contains(_favoriteTopic) && (userInput.Contains("tell me") || userInput.Contains("what about"))) ||
                    (userInput.Contains("my favorite") && userInput.Contains("topic") && userInput.Contains(_favoriteTopic)))
                {
                    if (_keywordResponses.ContainsKey(_favoriteTopic))
                    {
                        AppendChatbotMessage($"Related to your interest in '{_favoriteTopic}': {GetRandomResponse(_keywordResponses[_favoriteTopic])}");
                        _currentTopic = _favoriteTopic; // Set current topic to favorite if they ask about it directly
                        responded = true;
                        LogActivity($"Chatbot provided info on favorite topic: {_favoriteTopic}");
                    }
                }
            }

            // 6. Handle general questions
            if (!responded)
            {
                string[] words = userInput.Split(' ');
                if (words.Contains("how") && words.Contains("are") && words.Contains("you"))
                {
                    AppendChatbotMessage("I am a computer program, so I don't have feelings, but I am here to help you with any cybersecurity questions you may have.");
                    responded = true;
                    LogActivity("Chatbot responded to 'how are you' query.");
                }
                else if (words.Contains("what") && words.Contains("can") && words.Contains("i") && words.Contains("ask") && words.Contains("about"))
                {
                    AppendChatbotMessage("You can ask me about topics like: Password safety, Phishing, Safe browsing, Malware and ransomware, Multi-factor authentication (MFA).");
                    responded = true;
                    LogActivity("Chatbot listed available topics.");
                }
                else if (words.Contains("who") && words.Contains("are") && words.Contains("you"))
                {
                    AppendChatbotMessage("I am your Cybersecurity Assistant chatbot, designed to educate you about online safety.");
                    responded = true;
                    LogActivity("Chatbot responded to 'who are you' query.");
                }
            }

            // 7. Final fallback if nothing matched
            if (!responded)
            {
                AppendChatbotMessage("I'm not sure I understand that. Can you try rephrasing or ask about a different cybersecurity topic?");
                // Do NOT reset currentTopic here, it might still be relevant for future follow-ups.
                LogActivity("Chatbot sent 'did not understand' message.");
            }
            else
            {
                // If a specific topic was detected and responded to, set it as the currentTopic
                if (detectedTopic != null)
                {
                    _currentTopic = detectedTopic;
                }
                // Occasionally offer more tips on their favorite topic
                if (_favoriteTopic != null && _random.Next(3) == 0) // 1 in 3 chance
                {
                    if (_keywordResponses.ContainsKey(_favoriteTopic))
                    {
                        AppendChatbotMessage($"Just a thought related to your interest in '{_favoriteTopic}': {GetRandomResponse(_keywordResponses[_favoriteTopic])}");
                        LogActivity($"Chatbot offered follow-up on favorite topic: {_favoriteTopic}");
                    }
                }
            }
        }

        // Helper to append a chatbot message to the chat history
        private void AppendChatbotMessage(string message)
        {
            ChatHistory.Add(new ChatMessage { Sender = "Chatbot", Message = message, SenderColor = (SolidColorBrush)App.Current.FindResource("ChatbotColor") });
        }

        // Helper to append a user message to the chat history
        private void AppendUserMessage(string message)
        {
            ChatHistory.Add(new ChatMessage { Sender = UserName, Message = message, SenderColor = (SolidColorBrush)App.Current.FindResource("UserColor") });
        }

        // Helper to get a random response from a list
        private string GetRandomResponse(List<string> responses)
        {
            return responses != null && responses.Any() ?
                responses[_random.Next(responses.Count)] :
                "I'm not sure how to respond to that right now.";
        }

        // Task Assistant Logic
        private bool CanExecuteAddTaskCommand(object parameter)
        {
            return !string.IsNullOrWhiteSpace(NewTaskTitle);
        }

        private void ExecuteAddTaskCommand(object parameter)
        {
            DateTime? reminderDate = null;
            if (SetReminder && SelectedReminderTimeframe != "None")
            {
                int days = 0;
                if (SelectedReminderTimeframe.Contains("day"))
                {
                    // Handles "1 day", "3 days", "7 days"
                    int.TryParse(SelectedReminderTimeframe.Split(' ')[0], out days);
                }
                else if (SelectedReminderTimeframe.Contains("month"))
                {
                    days = 30; // Approximation for a month
                }
                reminderDate = DateTime.Now.AddDays(days);
            }

            var newTask = new TaskItem(NewTaskTitle, NewTaskDescription, reminderDate);
            Tasks.Add(newTask);
            AppendChatbotMessage($"Task added: \"{newTask.Title}\".");
            LogActivity($"Task added: \"{newTask.Title}\"{(newTask.HasReminder ? $" (Reminder set for {newTask.ReminderDate.Value:yyyy-MM-dd})" : "")}");

            // Reset input fields
            NewTaskTitle = string.Empty;
            NewTaskDescription = string.Empty;
            SetReminder = false;
            SelectedReminderTimeframe = "None";
            IsAddTaskFocused = false; // Reset focus helper
        }

        // Handles "Add task" command from chat input (NLP)
        private void HandleAddTaskCommandFromChat(string userInput)
        {
            // Regex to extract task title and optional reminder
            // Examples:
            // "add task - Review privacy settings"
            // "add task - Set up two-factor authentication reminder in 7 days"
            // "add task - Check firewall settings, remind me in 3 days"
            Match match = Regex.Match(userInput, @"add task\s+-\s*(.+?)(?: (?:remind me in|reminder in|set reminder for)\s+(?:(\d+)\s+(day|days|month|months)|tomorrow|next week))?$", RegexOptions.IgnoreCase);

            if (match.Success)
            {
                string title = match.Groups[1].Value.Trim();
                DateTime? reminderDate = null;

                // Handle reminder part
                if (match.Groups.Count > 2) // Check if the reminder group exists
                {
                    string quantityStr = match.Groups[2].Value; // "3", "7"
                    string unit = match.Groups[3].Value.ToLower(); // "day", "days", "month", "months"
                    string specificTime = match.Groups[0].Value.ToLower(); // "tomorrow", "next week" (full match)

                    if (!string.IsNullOrEmpty(quantityStr))
                    {
                        int quantity = int.Parse(quantityStr);
                        if (unit.Contains("day"))
                        {
                            reminderDate = DateTime.Now.AddDays(quantity);
                        }
                        else if (unit.Contains("month"))
                        {
                            reminderDate = DateTime.Now.AddMonths(quantity);
                        }
                    }
                    else if (specificTime.Contains("tomorrow"))
                    {
                        reminderDate = DateTime.Now.AddDays(1);
                    }
                    else if (specificTime.Contains("next week"))
                    {
                        reminderDate = DateTime.Now.AddDays(7);
                    }
                }

                var newTask = new TaskItem(title, "", reminderDate);
                Tasks.Add(newTask);
                AppendChatbotMessage($"Task added: \"{newTask.Title}\".");
                LogActivity($"Task added via chat: \"{newTask.Title}\"{(newTask.HasReminder ? $" (Reminder set for {newTask.ReminderDate.Value:yyyy-MM-dd})" : "")}");

                if (reminderDate.HasValue)
                {
                    AppendChatbotMessage($"Got it! I'll remind you about \"{newTask.Title}\" on {reminderDate.Value:yyyy-MM-dd HH:mm}.");
                }
                else
                {
                    AppendChatbotMessage($"Would you like to set a reminder for this task?");
                }
            }
            else
            {
                AppendChatbotMessage("To add a task via chat, please use the format: 'add task - [Task Title]' or 'add task - [Task Title] remind me in X days/months'.");
            }
        }

        private void HandleReminderFromChat(string userInput)
        {
            // First, try to link to the very latest task if it doesn't already have a reminder
            TaskItem latestUnremindedTask = Tasks.LastOrDefault(t => !t.HasReminder);

            // Regex to find "in X days/months" or "tomorrow", "next week"
            Match match = Regex.Match(userInput, @"(in|after)\s+(?:(\d+)\s+(day|days|month|months)|(tomorrow|next week))", RegexOptions.IgnoreCase);

            DateTime? extractedReminderDate = null;
            if (match.Success)
            {
                if (!string.IsNullOrEmpty(match.Groups[2].Value)) // Numeric quantity (e.g., "3 days")
                {
                    int quantity = int.Parse(match.Groups[2].Value);
                    string unit = match.Groups[3].Value.ToLower();

                    if (unit.Contains("day"))
                    {
                        extractedReminderDate = DateTime.Now.AddDays(quantity);
                    }
                    else if (unit.Contains("month"))
                    {
                        extractedReminderDate = DateTime.Now.AddMonths(quantity);
                    }
                }
                else if (!string.IsNullOrEmpty(match.Groups[4].Value)) // Specific time (e.g., "tomorrow")
                {
                    string specificTime = match.Groups[4].Value.ToLower();
                    if (specificTime.Contains("tomorrow"))
                    {
                        extractedReminderDate = DateTime.Now.AddDays(1);
                    }
                    else if (specificTime.Contains("next week"))
                    {
                        extractedReminderDate = DateTime.Now.AddDays(7);
                    }
                }
            }

            // If a valid reminder date was extracted and there's a task to link it to
            if (extractedReminderDate.HasValue && latestUnremindedTask != null)
            {
                latestUnremindedTask.ReminderDate = extractedReminderDate.Value;
                AppendChatbotMessage($"Got it! I'll remind you about \"{latestUnremindedTask.Title}\" on {latestUnremindedTask.ReminderDate.Value:yyyy-MM-dd HH:mm}.");
                LogActivity($"Reminder set for task \"{latestUnremindedTask.Title}\" via chat.");
            }
            else
            {
                // Fallback for general reminder setting or if no recent un-reminded task
                // Try to create a new task based on the "remind me to" phrase
                Match specificTaskReminderMatch = Regex.Match(userInput, @"remind me to\s+(.+?)(?: (?:in|after)\s+(?:(\d+)\s+(day|days|month|months)|(tomorrow|next week)))?$", RegexOptions.IgnoreCase);
                if (specificTaskReminderMatch.Success)
                {
                    string taskTitle = specificTaskReminderMatch.Groups[1].Value.Trim();
                    DateTime? newReminderDate = null;

                    if (specificTaskReminderMatch.Groups.Count > 2)
                    {
                        string quantityStr = specificTaskReminderMatch.Groups[2].Value;
                        string unit = specificTaskReminderMatch.Groups[3].Value.ToLower();
                        string specificTime = specificTaskReminderMatch.Groups[4].Value.ToLower(); // This might be empty if a quantity was found

                        if (!string.IsNullOrEmpty(quantityStr))
                        {
                            int quantity = int.Parse(quantityStr);
                            if (unit.Contains("day"))
                            {
                                newReminderDate = DateTime.Now.AddDays(quantity);
                            }
                            else if (unit.Contains("month"))
                            {
                                newReminderDate = DateTime.Now.AddMonths(quantity);
                            }
                        }
                        else if (!string.IsNullOrEmpty(specificTime))
                        {
                            if (specificTime.Contains("tomorrow"))
                            {
                                newReminderDate = DateTime.Now.AddDays(1);
                            }
                            else if (specificTime.Contains("next week"))
                            {
                                newReminderDate = DateTime.Now.AddDays(7);
                            }
                        }
                    }

                    var newTask = new TaskItem(taskTitle, "", newReminderDate);
                    Tasks.Add(newTask);
                    AppendChatbotMessage($"Reminder set for '{newTask.Title}'" + (newReminderDate.HasValue ? $" on {newReminderDate.Value:yyyy-MM-dd HH:mm}." : ". Would you like to specify a time?"));
                    LogActivity($"Reminder task added via chat: \"{newTask.Title}\"{(newTask.HasReminder ? $" (Reminder set for {newTask.ReminderDate.Value:yyyy-MM-dd})" : "")}");
                }
                else
                {
                    AppendChatbotMessage("I can set a reminder for a task. Please tell me what you need to be reminded about, e.g., 'remind me to update my software in 3 days'.");
                }
            }
        }


        private bool CanExecuteDeleteTaskCommand(object parameter)
        {
            return parameter is TaskItem; // Can delete if a TaskItem is selected
        }

        private void ExecuteDeleteTaskCommand(object parameter)
        {
            if (parameter is TaskItem taskToDelete)
            {
                Tasks.Remove(taskToDelete);
                AppendChatbotMessage($"Task deleted: \"{taskToDelete.Title}\".");
                LogActivity($"Task deleted: \"{taskToDelete.Title}\"");
            }
        }

        private void HandleDeleteTaskFromChat(string userInput)
        {
            Match match = Regex.Match(userInput, @"delete task\s+-\s*(.+)$", RegexOptions.IgnoreCase);
            if (match.Success)
            {
                string titleToDelete = match.Groups[1].Value.Trim();
                // Find first matching task (case-insensitive)
                TaskItem taskFound = Tasks.FirstOrDefault(t => t.Title.Equals(titleToDelete, StringComparison.OrdinalIgnoreCase));

                if (taskFound != null)
                {
                    Tasks.Remove(taskFound);
                    AppendChatbotMessage($"Task deleted: \"{taskFound.Title}\".");
                    LogActivity($"Task deleted via chat: \"{taskFound.Title}\"");
                }
                else
                {
                    AppendChatbotMessage($"I couldn't find a task titled \"{titleToDelete}\". Please check the title.");
                }
            }
            else
            {
                AppendChatbotMessage("To delete a task via chat, please use the format: 'delete task - [Task Title]'.");
            }
        }

        private void HandleCompleteTaskFromChat(string userInput)
        {
            Match match = Regex.Match(userInput, @"(?:mark|set)\s+task\s+(.+?)\s+completed", RegexOptions.IgnoreCase);
            if (match.Success)
            {
                string titleToComplete = match.Groups[1].Value.Trim();
                TaskItem taskFound = Tasks.FirstOrDefault(t => t.Title.Equals(titleToComplete, StringComparison.OrdinalIgnoreCase));

                if (taskFound != null)
                {
                    taskFound.IsCompleted = true; // Mark as completed
                    AppendChatbotMessage($"Task \"{taskFound.Title}\" marked as completed. Great job!");
                    LogActivity($"Task completed via chat: \"{taskFound.Title}\"");
                }
                else
                {
                    AppendChatbotMessage($"I couldn't find a task titled \"{titleToComplete}\". Please check the title.");
                }
            }
            else
            {
                AppendChatbotMessage("To mark a task completed, please use the format: 'mark task [Task Title] completed'.");
            }
        }

        private void DisplayTasksSummary()
        {
            if (!Tasks.Any())
            {
                AppendChatbotMessage("You currently have no cybersecurity tasks added. Why not add one?");
                LogActivity("Chatbot displayed empty task list.");
                return;
            }

            StringBuilder sb = new StringBuilder();
            sb.AppendLine("Here are your current cybersecurity tasks:");
            for (int i = 0; i < Tasks.Count; i++)
            {
                var task = Tasks[i];
                string status = task.IsCompleted ? " (Completed)" : "";
                string reminder = task.HasReminder ? $" - Reminder: {task.ReminderDate.Value:yyyy-MM-dd}" : "";
                sb.AppendLine($"{i + 1}. {task.Title}{status}{reminder}");
                if (!string.IsNullOrEmpty(task.Description))
                {
                    sb.AppendLine($"   Description: {task.Description}");
                }
            }
            AppendChatbotMessage(sb.ToString());
            LogActivity("Chatbot displayed task summary.");
        }


        // Quiz Game Logic
        private bool CanExecuteStartQuizCommand(object parameter)
        {
            return !IsQuizInProgress && _allQuizQuestions.Any();
        }

        private void ExecuteStartQuizCommand(object parameter)
        {
            IsQuizInProgress = true;
            IsQuizFinished = false;
            QuizScore = 0;
            _currentQuestionIndex = 0;
            // Shuffle and take 10 questions to ensure variety if more questions are added later
            _allQuizQuestions = _allQuizQuestions.OrderBy(q => _random.Next()).Take(10).ToList();

            DisplayNextQuizQuestion();
            QuizFeedback = string.Empty;
            FinalScoreMessage = string.Empty;
            LogActivity("Quiz started.");
        }

        private void DisplayNextQuizQuestion()
        {
            if (_currentQuestionIndex < _allQuizQuestions.Count)
            {
                CurrentQuestion = _allQuizQuestions[_currentQuestionIndex];
                AppendChatbotMessage($"Quiz Question {_currentQuestionIndex + 1}/{_allQuizQuestions.Count}: {CurrentQuestion.QuestionText}");
                LogActivity($"Quiz question {_currentQuestionIndex + 1} displayed.");
            }
            else
            {
                EndQuiz();
            }
        }

        private bool CanExecuteSubmitAnswerCommand(object parameter)
        {
            // Only allow submission if a quiz is in progress, there's a current question, and an option is selected.
            return IsQuizInProgress && CurrentQuestion != null && !string.IsNullOrEmpty(CurrentQuestion.SelectedOption);
        }

        private void ExecuteSubmitAnswerCommand(object parameter)
        {
            if (CurrentQuestion == null) return; // Safeguard

            if (CurrentQuestion.CheckAnswer())
            {
                QuizScore++;
                QuizFeedback = "Correct! " + CurrentQuestion.Explanation;
                AppendChatbotMessage("Correct!");
            }
            else
            {
                QuizFeedback = $"Wrong. The correct answer was: {CurrentQuestion.CorrectAnswer}. {CurrentQuestion.Explanation}";
                AppendChatbotMessage("Incorrect.");
            }
            LogActivity($"Quiz answer submitted. Current Score: {QuizScore}");

            _currentQuestionIndex++;
            CurrentQuestion.SelectedOption = null; // Clear selection for next question via UI binding
            if (_currentQuestionIndex < _allQuizQuestions.Count)
            {
                // Delay displaying next question slightly to allow user to read feedback
                System.Windows.Application.Current.Dispatcher.Invoke(() =>
                {
                    AppendChatbotMessage($"Next question...");
                }, System.Windows.Threading.DispatcherPriority.Background);

                System.Windows.Threading.DispatcherTimer timer = new System.Windows.Threading.DispatcherTimer();
                timer.Interval = TimeSpan.FromSeconds(2); // 2 seconds delay
                timer.Tick += (s, e) =>
                {
                    timer.Stop();
                    DisplayNextQuizQuestion();
                };
                timer.Start();
            }
            else
            {
                EndQuiz();
            }
        }

        private void EndQuiz()
        {
            IsQuizInProgress = false;
            IsQuizFinished = true;
            string feedback = "";
            if (QuizScore >= 8)
            {
                feedback = "Excellent job! You're a cybersecurity pro! Keep up the great work.";
            }
            else if (QuizScore >= 5)
            {
                feedback = "Good effort! You have a solid understanding of cybersecurity. Keep learning to enhance your knowledge!";
            }
            else
            {
                feedback = "Keep learning to stay safe online! Cybersecurity is a continuous journey, and I'm here to help you improve.";
            }
            FinalScoreMessage = $"Quiz Completed! Your score: {QuizScore} out of {_allQuizQuestions.Count}. {feedback}";
            AppendChatbotMessage(FinalScoreMessage);
            LogActivity($"Quiz ended. Final score: {QuizScore}/{_allQuizQuestions.Count}.");
        }


        // Activity Log Logic
        private void LogActivity(string description)
        {
            // Ensure this is run on the UI thread as it modifies an ObservableCollection
            // This is crucial if LogActivity is called from a background thread (e.g., if you had timers or async operations)
            System.Windows.Application.Current.Dispatcher.Invoke(() =>
            {
                ActivityLog.Add(new LogEntry(description));
                OnPropertyChanged(nameof(DisplayedActivityLog)); // Notify UI to refresh the log display
            });
        }

        private void DisplayActivityLog()
        {
            if (!ActivityLog.Any())
            {
                AppendChatbotMessage("The activity log is currently empty.");
                return;
            }

            StringBuilder sb = new StringBuilder();
            sb.AppendLine("Here's a summary of recent actions (last 10):");
            int count = 1;
            // Iterate over DisplayedActivityLog which already handles reversal and taking top 10
            foreach (var entry in DisplayedActivityLog)
            {
                sb.AppendLine($"{count++}. {entry.Timestamp:yyyy-MM-dd HH:mm}: {entry.Description}");
            }
            AppendChatbotMessage(sb.ToString());
            LogActivity("Chatbot displayed activity log.");
        }

        private void ExecuteClearActivityLogCommand(object parameter)
        {
            ActivityLog.Clear();
            OnPropertyChanged(nameof(DisplayedActivityLog)); // Refresh the view of the log
            AppendChatbotMessage("Activity log cleared.");
            // Don't log clearing the log to the log itself, as it's just been cleared!
            // If you wanted to log it, you'd do so *before* clearing or to a separate persistent log.
        }
    }

    // Helper class for chat messages (for display in UI with different colors)
    public class ChatMessage
    {
        public string Sender { get; set; }
        public string Message { get; set; }
        public SolidColorBrush SenderColor { get; set; } // Color for the sender's name
    }
}
