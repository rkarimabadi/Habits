using Habits.MobileUI.Models;
using System.Net.Http.Json;
using static System.Net.WebRequestMethods;

namespace Habits.MobileUI.Share
{
    public class Game
    {
        private HttpClient _httpClient;
        public Game(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        private List<string> Names = new List<string>{"بازیکن اول", "بازیکن دوم", "بازیکن سوم", "بازیکن چهارم", "بازیکن پنجم"};
        public Player CurrentPlayer { get; set; }
        public int TotalQuestions { get; set; }
        public int CurrentQuestionIndex { get; set; }
        public List<Question> Questions { get; set; }
        public List<Player> Players {get; private set; }  = new List<Player>();
        public bool IsSetup {get; private set; } = false;
        public async Task SetupGame(GameConfig config)
        {
            if (config.NumberOfPlayers < 1) throw new ArgumentOutOfRangeException("تعداد بازیکن‌ها حداقل باید یک نفر باشد");
            if (config.NumberOfPlayers > 5) throw new ArgumentOutOfRangeException("تعداد بازیکن‌ها حداکثر می‌تواند پنج نفر باشد");
            Players = new List<Player>();
            for (int i = 0; i < config.NumberOfPlayers; i++) 
            {
                Players.Add(new Player{ Score = 0, Name= Names[i] });
            }
            CurrentPlayer = Players[0];
            Questions = await SetQuestions(config.QuestionCountPerRound);
            TotalQuestions = Math.Min(Questions.Count(), config.QuestionCountPerRound);
            CurrentQuestionIndex = 0;
            IsSetup = true;
        }
        private async Task<List<Question>> SetQuestions(int numberOfQuestions)
        {
            var questions = await _httpClient.GetFromJsonAsync<List<Question>>("Data/Questions.json");
            if (!questions.Any()) throw new InvalidOperationException("دریافت سئوالات امکانپذیر نیست");
            if(questions.Count() <= numberOfQuestions) { return questions; }
            Random rand = new Random();            
            List<Question> result = new List<Question>();

            for (int i = 0; i < numberOfQuestions; i++)
            {
                int index = rand.Next(questions.Count());
                result.Add(questions[index]);
                questions.RemoveAt(index);
            }
            return result;
        }

        public bool IsGameOver {get { return CurrentQuestionIndex >= TotalQuestions; } }
        public Question GetCurrentQuestion { get {return Questions[CurrentQuestionIndex] ?? new Question();}}

        public void SubmitAnswer(string userAnswer)
        {
            var currentQuestion = GetCurrentQuestion;
            if (currentQuestion.Answer == userAnswer)
            {
                CurrentPlayer.Score++;
            }
            else
            {
                //CurrentPlayer.Score--;
                SwitchPlayer();
            }
            CurrentQuestionIndex++;
        }
        public void SwitchPlayer()
        {
            var currentPlayerIndex = Players.FindIndex( p => p == CurrentPlayer);
            CurrentPlayer = currentPlayerIndex < (Players.Count() - 1)  ? Players[currentPlayerIndex + 1] : Players[0];
        }
    }
}
