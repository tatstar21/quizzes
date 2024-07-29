namespace C_Examination2
{
    using System.Collections.Generic;
    using System.Diagnostics.Metrics;
    using System.Net.Http.Json;
    using System.Reflection.Metadata;
    using System.Text.Encodings.Web;
    using System.Text.Json;
    using System.Text.Json.Serialization;
    using System.Text.Unicode;
    using System.Xml.Linq;
    using static System.Runtime.InteropServices.JavaScript.JSType;

    public class Program
    {
        public List<User> Users = new List<User>();
        public List<Quiz> Quizzes = new List<Quiz>();

        static void Main(string[] args)
        {
            Program program = new Program();
            program.Users = program.LoadUsers("users.json");
            string quizName = "biology.json";
            program.LoadQuiz(program, quizName);
            User user = new User();
            user.Authorization(program);
        }

        public void ShowStatistic(string login)
        {
            foreach (var user in Users)
            {
                if (user.UserName == login)
                {
                    foreach (var statistic in user.Statics)
                    {
                        statistic.PrintStatistic();
                    }
                }
            }
        }
        public void СhoiceQuiz(string nameQuiz, User user)
        {
            List<Question> questionsForUser = new List<Question>();

            foreach (var quiz in Quizzes)
            {
                if (quiz.QuizName == nameQuiz)
                {
                    Console.WriteLine($"Викторина {quiz.QuizName}");

                    var random = new Random();

                    for (int i = 0; i < 3; i++)
                    {
                        int index = random.Next(0, quiz.Questions.Count);
                        questionsForUser.Add(quiz.Questions[index]);
                    }
                    PrintQuestion(questionsForUser, user, nameQuiz);
                }
                else
                {
                    Console.WriteLine("Нет такой викторины!");
                }
            }
        }
        public void PrintQuestion(List<Question> questionsForUser, User user, string nameQuiz)
        {
            int rightQuestions = 0;

            int allQuestions = questionsForUser.Count;
            foreach (var item in questionsForUser)
            {
                Console.WriteLine(item.QuestionText);

                int expectedNumberRightAnswers = 0;
                int actualNumberRightAnswers = 0;

                foreach (var answer in item.Answers)
                {
                    if (answer.IsCorrect)
                    {
                        expectedNumberRightAnswers++;
                    }
                }

                foreach (var answer in item.Answers)
                {
                    Console.WriteLine($"{answer.Letter}) {answer.AnswerText}");
                }
                Console.WriteLine("Ваш ответ (может быть несколько. Введите через запятую):");
                string[] letters = Console.ReadLine().Split(",");

                bool isCorrect = true;

                for (int i = 0; i < item.Answers.Count; i++)
                {
                    var a = item.Answers[i];
                    foreach (var letterTemp in letters)
                    {
                        if (letterTemp == a.Letter && a.IsCorrect == false)
                        {
                            isCorrect = false;
                        }
                        else if (letterTemp == a.Letter && a.IsCorrect)
                        {
                            actualNumberRightAnswers++;
                        }
                    }
                }

                if (isCorrect == false || expectedNumberRightAnswers != actualNumberRightAnswers)
                {
                    Console.WriteLine("Вы неверно ответили на вопрос!");

                }
                else
                {
                    Console.WriteLine("Вы верно ответили на вопрос!");
                    rightQuestions++;
                }
            }
            Console.WriteLine("Вы прошли викторину!");
            Console.WriteLine($"Всего вопросов: {allQuestions}");
            Console.WriteLine($"Правильных ответов: {rightQuestions}");
            QuizStatics quizStatics = new QuizStatics(nameQuiz, allQuestions, rightQuestions);
            user.Statics.Add(quizStatics);
            this.SaveUsers();

        }

        public void LoadQuiz(Program program, string quizName)
        {
            quizName = "Quizzes.json";
            using (StreamReader r = new StreamReader(quizName))
            {
                string json2 = r.ReadToEnd();
                Quiz deserializedQuiz = JsonSerializer.Deserialize<Quiz>(json2);
                program.Quizzes.Add(deserializedQuiz);
            }
        }
        public List <User> LoadUsers(string fileName)
        {
            fileName = "users.json";

            using (StreamReader r = new StreamReader(fileName))
            {
                string json1 = r.ReadToEnd();
                List <User> users = JsonSerializer.Deserialize<List<User>>(json1);
                return users;
            }
        }

        public void SaveUsers()
        {
            string json = JsonSerializer.Serialize(this.Users, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(@"users.json", json);
        }
    }
    public class User
    {
        [JsonInclude]
        public string? UserName { get; set; }
        [JsonInclude]
        public string? Password { get; set; }
        [JsonInclude]
        public DateTime Date { get; set; } //(yyyy,mm,dd)
        [JsonInclude]
        public List <QuizStatics>? Statics { get;} = new List<QuizStatics>();

        public User() { }
        public User(string userName, string password, DateTime date)
        {
            UserName = userName;   
            Password = password;
            Date = date;
        }

        public void PrintUser()
        {
            Console.WriteLine($"Name: {UserName}");
            Console.WriteLine($"Password: {Password}");
            Console.WriteLine($"Date: {Date}");
        }

        public bool IsYourLogin(string login, Program program)
        {
            for(int i = 0; i < program.Users.Count; i++)
            {
                if (program.Users[i].UserName == login)
                {
                    return true;
                }
            }
            return false;
        }

        public bool IsYourPassword(string password, Program program)
        {
            for (int i = 0; i < program.Users.Count; i++)
            {
                if (program.Users[i].Password == password)
                {
                    return true;
                }
            }
            return false;
        }

        public float successRate(string? quizzName)
        {
            var filtered = quizzName == null ? Statics : Statics.FindAll(s => s.Equals(quizzName));
            float countRight = 0;
            float countAll = 0;

            foreach(var s in filtered){
                countRight += s.CorrectQuestionsCount;
                countAll += s.AllQuestionsCount;
            }

            return countRight / countAll;
        }

        public void Authorization(Program program)
        {
            while (true)
            {
                Console.WriteLine("Авторизация:");
                Console.WriteLine("Введите логин:");
                string? login = Console.ReadLine();

                if (IsYourLogin(login, program))
                {
                    
                    while (true)
                    {
                        Console.WriteLine("Введите пароль:");
                        string? password = Console.ReadLine();

                        if (IsYourPassword(password, program))
                        {
                            Menu();
                            int menuItem = int.Parse(Console.ReadLine());

                            if (menuItem == 1)
                            {
                                Console.WriteLine("Старт викторины!!!");
                                Console.WriteLine("Выберете викторину (Биология)");
                                string nameQuiz = Console.ReadLine();
                                program.СhoiceQuiz(nameQuiz, this);
                                break;

                            }
                            else if (menuItem == 2)
                            {
                                program.ShowStatistic(login);
                            }
                            else if (menuItem == 3)
                            {
                                var bestUsers = program.Users.OrderBy(user => user.successRate(null))
                                    .Select(user => user.UserName);

                                foreach (var item in bestUsers)
                                {
                                    Console.WriteLine($"{item}");
                                }
                            }
                            else 
                            {
                                System.Environment.Exit(0);
                            }
                        }
                        else
                        {
                            NotCorrectData(program); 
                        }
                    }
                }
                else
                {
                    NotCorrectData(program); 
                }
            } 
        }

        public void Menu()
        {
            Console.WriteLine("Выберете пункт меню:");
            Console.WriteLine("Начать викторину - 1");
            Console.WriteLine("Посмотреть свои результаты - 2");
            Console.WriteLine("Вывести Топ-20 - 3");
            Console.WriteLine();
            Console.WriteLine("Другая цифра - Выход из программы!");
        }

        public void NotCorrectData(Program program) //ref bool registration)
        {
            Console.WriteLine("Выберете пункт меню:");
            Console.WriteLine("1 - зарегистрироваться");
            Console.WriteLine("2 - попробовать еще");
            Console.WriteLine("Другая цифра - выход из программы");
            int menuItem = int.Parse(Console.ReadLine());

            if (menuItem == 1)
            {
                Registration(program);//, ref registration);     
            }
            else if (menuItem == 2)
            {
                Console.WriteLine("Попробовать еще");
            }
            else
            {
                Console.WriteLine("Выход из программы!");
                Environment.Exit(0);
            }
        }

        public void Registration(Program program)
        {
            Console.WriteLine("Регистрация:");
            Console.WriteLine("Введите дату рождения");
            DateTime dateTime = DateTime.Parse(Console.ReadLine());
            string? login;

            do
            {
                Console.WriteLine("Введите логин:");
                login = Console.ReadLine();
            }
            while (!(SameLogins(login, program)));
            
            while (true)
            {
                Console.WriteLine("Введите пароль:");
                string? password = Console.ReadLine();

                Console.WriteLine("Повторите пароль:");
                string? password2 = Console.ReadLine();

                if (password == password2)
                {
                    User user = new User(login, password, dateTime);
                    program.Users.Add(user);
                    Console.WriteLine("Вы успешно зарегистрировались!");
                    program.SaveUsers();
                    break;
                }
                else
                {
                    Console.WriteLine("Любая цифра - попробуйте еще " +
                        "или нажмите 0 для выхода из программы!");
                    int zero = int.Parse(Console.ReadLine());

                    if (zero == 0)
                    {
                        Console.WriteLine("Выход из программы!");
                        Environment.Exit(0);
                    }
                    else
                    {
                        Console.WriteLine("Выход в предыдущее меню!");
                    }
                }
            }
        }

        public bool SameLogins(string login, Program program)
        {
            foreach (var user in program.Users)
            {
                if (user.UserName == login)
                {
                    Console.WriteLine("Такой логин уже существует");
                    return false;
                }
            }
            return true;
        }
        public void Rating(Program program)
        {
            
        }
        public void changeTheData(Program program, string login, string newPassword, DateTime newDate)
        {
            foreach(var item in program.Users)
            {
                if(item.UserName == login)
                {
                    item.Password = newPassword;
                    item.Date = newDate;
                    Console.WriteLine("Данные изменены!");
                    break;
                }
                else
                {
                    Console.WriteLine("Выши данные не найдены!");
                }
            }
        }
    }
     
    public class QuizStatics 
    {
        [JsonInclude]
        public string QuizStatisticName;
        [JsonInclude]
        public int CorrectQuestionsCount;
        [JsonInclude]
        public int AllQuestionsCount;
        

        public QuizStatics(string quizStatisticName, 
             int correctQuestionsCount, int allQuestionsCount)
        {
            QuizStatisticName = quizStatisticName;
            CorrectQuestionsCount = correctQuestionsCount;
            AllQuestionsCount = allQuestionsCount;
        }
        public void PrintStatistic()
        {
            Console.WriteLine($"Статистика:");
            Console.WriteLine($"Название: {QuizStatisticName}");
            Console.WriteLine($"Всего вопросов: {CorrectQuestionsCount}");
            Console.WriteLine($"Правильных ответов: {AllQuestionsCount}");
        }
    }

    public class Quiz
    {
        public string QuizName { get; set; }
        [JsonInclude]
        public List<Question> Questions = new List<Question>();
        public Quiz() { }
    }

    public class Answer
    {
        [JsonInclude]
        public string Letter;
        [JsonInclude]
        public bool IsCorrect = false;
        [JsonInclude]
        public string AnswerText; 

        public Answer() { }
        public Answer(string letter, string answerText, bool isCorrect)
        {
            Letter = letter;
            AnswerText = answerText;
            IsCorrect = isCorrect;
        }
    }

    public class Question
    {
        public string QuestionText { get; set; }
        [JsonInclude]
        public List<Answer> Answers = new List<Answer>();
        
        public Question() { }

        public Question(string questionText, List<Answer> answers)
        { 
            QuestionText = questionText;
            Answers = answers;
        }
    } 

   
}
