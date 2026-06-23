using MAX.Bot;
using MAX.Bot.Interfaces;
using MAX.Bot.Interfaces.Models;
using MAX.Bot.Interfaces.Models.Request.Message;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SQLitePCL;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading.Tasks;
using TeaBotSimple;

namespace botmax
{
    class Program
    {
        static string TOKEN = "ВАШ_ТОКЕН_ИЗ_MAX";

        // Заменено на ConcurrentDictionary для потокобезопасности
        static ConcurrentDictionary<long, string> userState = new ConcurrentDictionary<long, string>();
        static ConcurrentDictionary<long, string> userLanguage = new ConcurrentDictionary<long, string>();
        static ConcurrentDictionary<long, List<string>> testAnswers = new ConcurrentDictionary<long, List<string>>();

        static async Task Main(string[] args)
        {
            // ===== ИНИЦИАЛИЗАЦИЯ SQLITE =====
            Batteries_V2.Init();

            Database.Initialize();
            Log("🍵 MAX Бот TEA запускается...");

            var botClient = new MaxBotClient(TOKEN);

            try
            {
                var botInfo = await botClient.GetMeAsync();
                Log($"✅ Бот подключён: {botInfo.FirstName}");
            }
            catch (Exception ex)
            {
                LogError($"Не удалось подключиться к боту: {ex.Message}");
                return;
            }

            Log("🔄 Бот слушает сообщения...");

            await botClient.PollUpdatesWithCallback(
                async (update, client) =>
                {
                    try
                    {
                        if (update?.UpdateType == UpdateTypes.MessageCreated)
                        {
                            string json = JsonConvert.SerializeObject(update);
                            var data = JObject.Parse(json);

                            var msgData = data["Object"] ?? data["message"];
                            if (msgData == null) return;

                            long userId = msgData["SenderId"]?.Value<long>() ??
                                          msgData["sender_id"]?.Value<long>() ??
                                          msgData["from_id"]?.Value<long>() ?? 0;

                            string text = msgData["Body"]?["Text"]?.Value<string>() ??
                                          msgData["text"]?.Value<string>() ?? "";
                            text = text.ToLower();

                            if (userId == 0) return;

                            Log($"[MAX] {userId}: {text}");

                            Database.GetOrCreateUser(userId, "MAX");

                            string answer = GetAnswer(userId, text);
                            string keyboardType = GetKeyboardType(userId);

                            await SendMessageWithKeyboard(client, userId, answer, keyboardType);
                        }
                    }
                    catch (Exception ex)
                    {
                        LogError($"Ошибка обработки сообщения: {ex.Message}");
                        LogError($"   {ex.StackTrace}");
                    }
                },
                limit: 100,
                timeout: 90,
                types: new List<string> { UpdateTypes.MessageCreated }
            );
        }

        #region Логирование
        static void Log(string message)
        {
            Console.WriteLine($"[{DateTime.Now:HH:mm:ss}] {message}");
        }

        static void LogError(string message)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"[{DateTime.Now:HH:mm:ss}] ❌ {message}");
            Console.ResetColor();
        }
        #endregion

        static string GetKeyboardType(long userId)
        {
            if (!userState.TryGetValue(userId, out string state)) return "menu";
            if (state == "waiting_language") return "language";
            if (state == "waiting_level") return "level";
            if (state.StartsWith("waiting_test")) return "test";
            return "menu";
        }

        static async Task SendMessageWithKeyboard(IMaxBotClient client, long userId, string message, string keyboardType)
        {
            string keyboardJson = GetKeyboardJson(keyboardType);

            var request = new SendMessageRequest
            {
                ChatId = userId,
                Text = message
            };

            try
            {
                var keyboard = JsonConvert.DeserializeObject(keyboardJson);
                var prop = request.GetType().GetProperty("Keyboard");
                if (prop != null && prop.CanWrite)
                {
                    prop.SetValue(request, keyboard);
                }
            }
            catch (Exception ex)
            {
                LogError($"Ошибка установки клавиатуры: {ex.Message}");
            }

            await client.SendMessageAsync(request);
            Log($"  → Ответ отправлен пользователю {userId}");
        }

        #region Клавиатуры
        static string GetKeyboardJson(string type)
        {
            switch (type)
            {
                case "test":
                    return @"{""one_time"":true,""buttons"":[[{""action"":{""type"":""text"",""label"":""А"",""payload"":""{\""answer\"":\""a\""}""},""color"":""primary""},{""action"":{""type"":""text"",""label"":""Б"",""payload"":""{\""answer\"":\""b\""}""},""color"":""primary""},{""action"":{""type"":""text"",""label"":""В"",""payload"":""{\""answer\"":\""c\""}""},""color"":""primary""}],[{""action"":{""type"":""text"",""label"":""Г"",""payload"":""{\""answer\"":\""d\""}""},""color"":""primary""}]]}";

                case "language":
                    return @"{""one_time"":true,""buttons"":[[{""action"":{""type"":""text"",""label"":""🇬🇧 Английский"",""payload"":""{\""lang\"":\""english\""}""},""color"":""primary""},{""action"":{""type"":""text"",""label"":""🇨🇳 Китайский"",""payload"":""{\""lang\"":\""chinese\""}""},""color"":""primary""}],[{""action"":{""type"":""text"",""label"":""🇯🇵 Японский"",""payload"":""{\""lang\"":\""japanese\""}""},""color"":""primary""},{""action"":{""type"":""text"",""label"":""🇰🇷 Корейский"",""payload"":""{\""lang\"":\""korean\""}""},""color"":""primary""}]]}";

                case "level":
                    return @"{""one_time"":true,""buttons"":[[{""action"":{""type"":""text"",""label"":""Начальный"",""payload"":""{\""level\"":\""beginner\""}""},""color"":""primary""},{""action"":{""type"":""text"",""label"":""Средний"",""payload"":""{\""level\"":\""intermediate\""}""},""color"":""primary""}],[{""action"":{""type"":""text"",""label"":""Продвинутый"",""payload"":""{\""level\"":\""advanced\""}""},""color"":""primary""}]]}";

                default: // menu
                    return @"{""one_time"":false,""buttons"":[[{""action"":{""type"":""text"",""label"":""📚 Выбрать язык"",""payload"":""{\""command\"":\""language\""}""},""color"":""primary""},{""action"":{""type"":""text"",""label"":""📝 Тест уровня"",""payload"":""{\""command\"":\""test\""}""},""color"":""primary""}],[{""action"":{""type"":""text"",""label"":""🎓 Подобрать обучение"",""payload"":""{\""command\"":\""course\""}""},""color"":""secondary""},{""action"":{""type"":""text"",""label"":""👩‍💼 Позвать человека"",""payload"":""{\""command\"":\""help\""}""},""color"":""secondary""}],[{""action"":{""type"":""text"",""label"":""🎁 Акции и сертификаты"",""payload"":""{\""command\"":\""sales\""}""},""color"":""positive""}]]}";
            }
        }
        #endregion

        static string GetAnswer(long userId, string text)
        {
            // Инициализация состояния через TryGetValue для потокобезопасности
            if (!userState.ContainsKey(userId))
            {
                userState.TryAdd(userId, "menu");
                testAnswers.TryAdd(userId, new List<string>());
            }

            string state = userState[userId];

            // Обработка команд меню
            if (text == "1" || text.Contains("language"))
            {
                userState[userId] = "waiting_language";
                return "📚 ВЫБЕРИТЕ ЯЗЫК:\n\nНажмите на кнопку ниже:";
            }
            if (text == "2" || text.Contains("test"))
            {
                userState[userId] = "waiting_test_question1";
                testAnswers[userId] = new List<string>();
                return "📝 ТЕСТ УРОВНЯ ЯЗЫКА\n\nВопрос 1 из 5:\n\nКак давно вы изучаете язык?\n\nА) Меньше года\nБ) 1-3 года\nВ) Более 3 лет\n\nВыберите ответ:";
            }
            if (text == "3" || text.Contains("course"))
            {
                userState[userId] = "waiting_selection";
                return "🎓 ПОДБОР ОБУЧЕНИЯ\n\nОтветьте на вопросы:\n1. Ваш возраст?\n2. Цель изучения?\n3. Удобное время?\n\nНапишите через запятую, например: 25, работа, вечер";
            }
            if (text == "4" || text.Contains("help"))
            {
                Database.AddManagerRequest(userId, text, "MAX");
                return "👩‍💼 ЗАЯВКА ПЕРЕДАНА МЕНЕДЖЕРУ\n\nОпишите вопрос. Специалист свяжется с вами.\n\nВремя работы: Пн-Пт 10:00-20:00";
            }
            if (text == "5" || text.Contains("sales"))
            {
                return "🎁 АКЦИИ!\n\n🔥 Скидка 20% на первый месяц\n🎫 Подарочные сертификаты\n👥 Приведи друга — месяц бесплатно\n\nНажмите 👩‍💼 Позвать человека для подробностей";
            }

            // Парсинг payload от кнопок
            string originalText = text;
            if (text.Contains("lang"))
            {
                if (text.Contains("english")) text = "английский";
                else if (text.Contains("chinese")) text = "китайский";
                else if (text.Contains("japanese")) text = "японский";
                else if (text.Contains("korean")) text = "корейский";
            }

            if (text.Contains("level"))
            {
                if (text.Contains("beginner")) text = "а";
                else if (text.Contains("intermediate")) text = "б";
                else if (text.Contains("advanced")) text = "в";
            }

            if (text.Contains("answer"))
            {
                if (text.Contains("a")) text = "а";
                else if (text.Contains("b")) text = "б";
                else if (text.Contains("c")) text = "в";
                else if (text.Contains("d")) text = "г";
            }

            // Обработка состояний
            if (state == "waiting_language")
            {
                string lang = "";
                if (text.Contains("английский")) lang = "английский";
                else if (text.Contains("китайский")) lang = "китайский";
                else if (text.Contains("японский")) lang = "японский";
                else if (text.Contains("корейский")) lang = "корейский";
                else
                {
                    return "📚 Пожалуйста, выберите язык из кнопок ниже:";
                }

                Database.SaveLanguage(userId, lang, "MAX");
                userLanguage[userId] = lang;
                userState[userId] = "waiting_level";
                return $"🇬🇧 Отлично! Вы выбрали {lang}.\n\nКакой у вас уровень?\n\nВыберите уровень:";
            }

            if (state == "waiting_level")
            {
                string level = "";
                if (text == "а" || text == "a") level = "начальный";
                else if (text == "б" || text == "b") level = "средний";
                else if (text == "в" || text == "v" || text == "c") level = "продвинутый";
                else return "Пожалуйста, выберите уровень из кнопок ниже:";

                Database.SaveLevel(userId, level, "MAX");
                userState[userId] = "menu";
                return $"✅ Спасибо! {level} уровень сохранён.\n\nЧто дальше?\n• Нажмите 🎓 Подобрать обучение\n• Или 📚 Меню";
            }

            // Обработка теста
            if (state.StartsWith("waiting_test"))
            {
                return HandleTestState(userId, state, text);
            }

            if (state == "waiting_selection")
            {
                userState[userId] = "menu";
                return "🎓 Спасибо! Мы подобрали для вас обучение.\n\nСпециалист свяжется с вами в ближайшее время.\n\nНажмите 📚 Меню для возврата";
            }

            // Главное меню
            return "🍵 ДОБРО ПОЖАЛОВАТЬ В TEA!\n\n👇 Нажмите на кнопку ниже, чтобы выбрать действие:\n\n📚 Выбрать язык\n📝 Пройти тест уровня\n🎓 Подобрать обучение\n👩‍💼 Позвать человека\n🎁 Акции и сертификаты";
        }

        #region Обработка теста
        static string HandleTestState(long userId, string state, string text)
        {
            if (!testAnswers.ContainsKey(userId))
            {
                testAnswers.TryAdd(userId, new List<string>());
            }

            if (state == "waiting_test_question1")
            {
                testAnswers[userId].Add(text);
                userState[userId] = "waiting_test_question2";
                return "📝 Вопрос 2 из 5:\n\nКакая у вас цель изучения языка?\n\nА) Работа\nБ) Путешествия\nВ) Экзамен\nГ) Для себя\n\nВыберите ответ:";
            }
            if (state == "waiting_test_question2")
            {
                testAnswers[userId].Add(text);
                userState[userId] = "waiting_test_question3";
                return "📝 Вопрос 3 из 5:\n\nСколько часов в неделю готовы заниматься?\n\nА) 1-2\nБ) 3-4\nВ) 5-6\nГ) Более 6\n\nВыберите ответ:";
            }
            if (state == "waiting_test_question3")
            {
                testAnswers[userId].Add(text);
                userState[userId] = "waiting_test_question4";
                return "📝 Вопрос 4 из 5:\n\nКакой формат удобнее?\n\nА) Групповой\nБ) Индивидуальный\nВ) Самостоятельно\nГ) Смешанный\n\nВыберите ответ:";
            }
            if (state == "waiting_test_question4")
            {
                testAnswers[userId].Add(text);
                userState[userId] = "waiting_test_question5";
                return "📝 Вопрос 5 из 5:\n\nКакой бюджет в месяц?\n\nА) До 3000₽\nБ) 3000-6000₽\nВ) 6000-10000₽\nГ) Более 10000₽\n\nВыберите ответ:";
            }
            if (state == "waiting_test_question5")
            {
                testAnswers[userId].Add(text);

                int score = 0;
                foreach (var ans in testAnswers[userId])
                {
                    if (ans == "а" || ans == "a") score++;
                }

                // Очистка временных данных для предотвращения утечки памяти
                testAnswers.TryRemove(userId, out _);
                userState[userId] = "menu";

                return $"🎉 ТЕСТ ЗАВЕРШЁН! 🎉\n\n⭐ Ваш результат: {score} из 5\n⭐ Рекомендация: групповые занятия 2 раза в неделю\n⭐ Уровень: средний (B1)\n⭐ Следующий шаг: пробный урок бесплатно\n\nНажмите 👩‍💼 Позвать человека для записи";
            }

            return "Произошла ошибка. Нажмите 📚 Меню для возврата.";
        }
        #endregion
    }
}