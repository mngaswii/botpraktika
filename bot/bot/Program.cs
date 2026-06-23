using System;
using System.Net;
using System.Text;
using System.Collections.Concurrent;
using System.Collections.Generic;
using VkNet;
using VkNet.Model;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using VkNet.Utils;

namespace TeaBotSimple
{
    // Класс для вопроса теста
    class TestQuestion
    {
        public string Text { get; set; }
        public string[] Options { get; set; }
        public string CorrectAnswer { get; set; } // "а", "б", "в", "г"
    }

    class Program
    {
        static string TOKEN = "vk1.a.OJKfI6n69LasJ8Y_AO2dLapZm7TRp-5XdIPw11Rl1sQboMzvEJhXMbLEewSgRV6IhJchygRBgibH2pAFmVezMfFWYfOl2Mcwlvor0FY-lgyqtKjeXDmUNU7YRoufEb52HS5zg83eiaG4pQt_GH4p5iWZjoqLHS9rSNfI1KHflKSA5275S5UYVOmeSoP17O5NkHEUFca5YmlnXn2mhDTcXQ";
        static ulong GROUP_ID = 239436285;

        static ConcurrentDictionary<long, string> userState = new ConcurrentDictionary<long, string>();
        static ConcurrentDictionary<long, string> userLanguage = new ConcurrentDictionary<long, string>();
        static ConcurrentDictionary<long, List<string>> testAnswers = new ConcurrentDictionary<long, List<string>>();
        static ConcurrentDictionary<long, int> testCorrectAnswers = new ConcurrentDictionary<long, int>();

        // ===== ВОПРОСЫ ТЕСТА ДЛЯ КАЖДОГО ЯЗЫКА =====
        static Dictionary<string, List<TestQuestion>> languageTests;

        static Program()
        {
            languageTests = new Dictionary<string, List<TestQuestion>>();

            // ===== АНГЛИЙСКИЙ =====
            languageTests["английский"] = new List<TestQuestion>
            {
                new TestQuestion {
                    Text = "Вопрос 1 из 5 (Уровень A1):\n\nКак переводится слово 'Hello'?",
                    Options = new[] { "Пока", "Привет", "Спасибо", "Пожалуйста" },
                    CorrectAnswer = "б"
                },
                new TestQuestion {
                    Text = "Вопрос 2 из 5 (Уровень A2):\n\nВставьте пропущенное слово:\nI ___ a student.",
                    Options = new[] { "is", "am", "are", "be" },
                    CorrectAnswer = "б"
                },
                new TestQuestion {
                    Text = "Вопрос 3 из 5 (Уровень B1):\n\nВыберите правильную форму:\nIf I ___ rich, I would travel the world.",
                    Options = new[] { "am", "was", "were", "would be" },
                    CorrectAnswer = "в"
                },
                new TestQuestion {
                    Text = "Вопрос 4 из 5 (Уровень B2):\n\nВставьте фразовый глагол:\nThe meeting was called ___ due to bad weather.",
                    Options = new[] { "out", "off", "up", "away" },
                    CorrectAnswer = "б"
                },
                new TestQuestion {
                    Text = "Вопрос 5 из 5 (Уровень C1):\n\nВыберите правильное слово:\nDespite ___ tired, she continued working.",
                    Options = new[] { "she was", "of being", "being", "to be" },
                    CorrectAnswer = "в"
                }
            };

            // ===== КИТАЙСКИЙ =====
            languageTests["китайский"] = new List<TestQuestion>
            {
                new TestQuestion {
                    Text = "Вопрос 1 из 5 (Уровень A1):\n\nЧто означает '你好' (nǐ hǎo)?",
                    Options = new[] { "Спасибо", "Привет", "Пока", "Извините" },
                    CorrectAnswer = "б"
                },
                new TestQuestion {
                    Text = "Вопрос 2 из 5 (Уровень A2):\n\nЧто означает '谢谢' (xiè xie)?",
                    Options = new[] { "Пожалуйста", "Привет", "Спасибо", "Да" },
                    CorrectAnswer = "в"
                },
                new TestQuestion {
                    Text = "Вопрос 3 из 5 (Уровень B1):\n\nПереведите: '我昨天去了商店'",
                    Options = new[] { "Я завтра пойду в магазин", "Вчера я ходил в магазин", "Я люблю магазин", "Где магазин?" },
                    CorrectAnswer = "б"
                },
                new TestQuestion {
                    Text = "Вопрос 4 из 5 (Уровень B2):\n\nЧто означает '学习' (xuéxí)?",
                    Options = new[] { "Работать", "Отдыхать", "Учиться", "Играть" },
                    CorrectAnswer = "в"
                },
                new TestQuestion {
                    Text = "Вопрос 5 из 5 (Уровень C1):\n\nЧто означает конструкция '虽然...但是'?",
                    Options = new[] { "Если...то", "Хотя...но", "Потому что", "Так что" },
                    CorrectAnswer = "б"
                }
            };

            // ===== ЯПОНСКИЙ =====
            languageTests["японский"] = new List<TestQuestion>
            {
                new TestQuestion {
                    Text = "Вопрос 1 из 5 (Уровень A1):\n\nЧто означает 'こんにちは' (konnichiwa)?",
                    Options = new[] { "Спасибо", "Привет/Добрый день", "Извините", "Спокойной ночи" },
                    CorrectAnswer = "б"
                },
                new TestQuestion {
                    Text = "Вопрос 2 из 5 (Уровень A2):\n\nЧто означает 'ありがとう' (arigatou)?",
                    Options = new[] { "Пожалуйста", "Привет", "Спасибо", "Да" },
                    CorrectAnswer = "в"
                },
                new TestQuestion {
                    Text = "Вопрос 3 из 5 (Уровень B1):\n\nПереведите: '私は学生です' (watashi wa gakusei desu)",
                    Options = new[] { "Я учитель", "Я студент", "Я работаю", "Я живу здесь" },
                    CorrectAnswer = "б"
                },
                new TestQuestion {
                    Text = "Вопрос 4 из 5 (Уровень B2):\n\nЧто означает '食べる' (taberu)?",
                    Options = new[] { "Пить", "Спать", "Есть/кушать", "Идти" },
                    CorrectAnswer = "в"
                },
                new TestQuestion {
                    Text = "Вопрос 5 из 5 (Уровень C1):\n\nЧто означает '勉強する' (benkyou suru)?",
                    Options = new[] { "Работать", "Отдыхать", "Учиться/изучать", "Путешествовать" },
                    CorrectAnswer = "в"
                }
            };

            // ===== КОРЕЙСКИЙ =====
            languageTests["корейский"] = new List<TestQuestion>
            {
                new TestQuestion {
                    Text = "Вопрос 1 из 5 (Уровень A1):\n\nЧто означает '안녕하세요' (annyeonghaseyo)?",
                    Options = new[] { "Спасибо", "Привет/Здравствуйте", "Извините", "Пока" },
                    CorrectAnswer = "б"
                },
                new TestQuestion {
                    Text = "Вопрос 2 из 5 (Уровень A2):\n\nЧто означает '감사합니다' (gamsahamnida)?",
                    Options = new[] { "Пожалуйста", "Привет", "Спасибо", "Да" },
                    CorrectAnswer = "в"
                },
                new TestQuestion {
                    Text = "Вопрос 3 из 5 (Уровень B1):\n\nПереведите: '저는 학생입니다' (jeoneun haksaengimnida)",
                    Options = new[] { "Я учитель", "Я студент", "Я работаю", "Я живу здесь" },
                    CorrectAnswer = "б"
                },
                new TestQuestion {
                    Text = "Вопрос 4 из 5 (Уровень B2):\n\nЧто означает '먹다' (meoktta)?",
                    Options = new[] { "Пить", "Спать", "Есть/кушать", "Идти" },
                    CorrectAnswer = "в"
                },
                new TestQuestion {
                    Text = "Вопрос 5 из 5 (Уровень C1):\n\nЧто означает '공부하다' (gongbuhada)?",
                    Options = new[] { "Работать", "Отдыхать", "Учиться/изучать", "Путешествовать" },
                    CorrectAnswer = "в"
                }
            };
        }

        static void Main(string[] args)
        {
            Database.Initialize();
            Log("🍵 VK Бот TEA запускается...");

            VkApi api = new VkApi();
            try
            {
                api.Authorize(new ApiAuthParams { AccessToken = TOKEN });
                Log("✅ Бот подключён к VK API");
            }
            catch (Exception ex)
            {
                LogError($"Не удалось подключиться: {ex.Message}");
                return;
            }

            var server = api.Groups.GetLongPollServer(GROUP_ID);
            string lastTs = server.Ts.ToString();
            string serverKey = server.Key;
            string serverUrl = server.Server;
            Random rnd = new Random();

            Log("🔄 Бот слушает сообщения...");

            while (true)
            {
                try
                {
                    string url = $"{serverUrl}?act=a_check&key={serverKey}&ts={lastTs}&wait=25";
                    using (WebClient wc = new WebClient())
                    {
                        wc.Encoding = Encoding.UTF8;
                        string json = wc.DownloadString(url);
                        dynamic data = JsonConvert.DeserializeObject(json);

                        if (data.failed != null)
                        {
                            int failedCode = (int)data.failed;
                            if (failedCode == 1)
                            {
                                lastTs = data.ts.ToString();
                            }
                            else if (failedCode == 2 || failedCode == 3)
                            {
                                var newServer = api.Groups.GetLongPollServer(GROUP_ID);
                                serverKey = newServer.Key;
                                serverUrl = newServer.Server;
                                lastTs = newServer.Ts.ToString();
                            }
                            continue;
                        }

                        lastTs = data.ts.ToString();

                        if (data.updates != null)
                        {
                            foreach (var update in data.updates)
                            {
                                if (update.type == "message_new")
                                {
                                    long userId = update.@object.message.peer_id;
                                    string text = (update.@object.message.text ?? "").ToString().ToLower();

                                    if (update.@object.message.payload != null)
                                    {
                                        string payload = update.@object.message.payload.ToString();
                                        text = ParsePayload(payload, text);
                                    }

                                    Log($"[VK] {userId}: {text}");

                                    Database.GetOrCreateUser(userId, "VK");

                                    string answer = GetAnswer(userId, text);
                                    string keyboardType = GetKeyboardType(userId);
                                    SendMessageWithKeyboard(api, userId, answer, rnd, keyboardType);
                                }
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    LogError($"Ошибка обработки: {ex.Message}");
                    System.Threading.Thread.Sleep(5000);
                }
            }
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

        #region Парсинг payload
        static string ParsePayload(string payload, string originalText)
        {
            try
            {
                JObject json = JObject.Parse(payload);

                if (json["command"] != null)
                {
                    string command = json["command"].ToString().ToLower();
                    if (command == "language") return "1";
                    else if (command == "test") return "2";
                    else if (command == "course") return "3";
                    else if (command == "help") return "4";
                    else if (command == "sales") return "5";
                    else if (command == "schools") return "6";
                    else if (command == "start") return "start";
                    else if (command == "menu") return "меню";
                }
                else if (json["answer"] != null)
                {
                    string answer = json["answer"].ToString().ToLower();
                    if (answer == "a") return "а";
                    else if (answer == "b") return "б";
                    else if (answer == "c") return "в";
                    else if (answer == "d") return "г";
                }
                else if (json["lang"] != null)
                {
                    string lang = json["lang"].ToString().ToLower();
                    if (lang == "english") return "английский";
                    else if (lang == "chinese") return "китайский";
                    else if (lang == "japanese") return "японский";
                    else if (lang == "korean") return "корейский";
                }
                else if (json["level"] != null)
                {
                    string level = json["level"].ToString().ToLower();
                    if (level == "beginner") return "а";
                    else if (level == "intermediate") return "б";
                    else if (level == "advanced") return "в";
                }
            }
            catch
            {
                // Если не удалось распарсить JSON, возвращаем оригинальный текст
            }

            return originalText;
        }
        #endregion

        static string GetKeyboardType(long userId)
        {
            if (!userState.TryGetValue(userId, out string state)) return "menu";
            if (state == "waiting_language") return "language";
            if (state == "waiting_test_language") return "language";
            if (state == "waiting_level") return "level";
            if (state.StartsWith("waiting_test")) return "test";
            return "menu";
        }

        static void SendMessageWithKeyboard(VkApi api, long userId, string message, Random rnd, string keyboardType = "menu")
        {
            string keyboard = GetKeyboardJson(keyboardType);

            var parameters = new VkParameters
            {
                { "peer_id", userId },
                { "message", message },
                { "random_id", rnd.Next() },
                { "keyboard", keyboard }
            };

            try
            {
                api.Call("messages.send", parameters);
                Log($"  → Ответ отправлен пользователю {userId}");
            }
            catch (Exception ex)
            {
                LogError($"Ошибка отправки сообщения: {ex.Message}");
            }
        }

        #region Клавиатуры
        static string GetKeyboardJson(string type)
        {
            switch (type)
            {
                case "test":
                    return @"{""one_time"":false,""buttons"":[[{""action"":{""type"":""text"",""label"":""А"",""payload"":""{\""answer\"":\""a\""}""},""color"":""primary""},{""action"":{""type"":""text"",""label"":""Б"",""payload"":""{\""answer\"":\""b\""}""},""color"":""primary""},{""action"":{""type"":""text"",""label"":""В"",""payload"":""{\""answer\"":\""c\""}""},""color"":""primary""},{""action"":{""type"":""text"",""label"":""Г"",""payload"":""{\""answer\"":\""d\""}""},""color"":""primary""}],[{""action"":{""type"":""text"",""label"":""🏠 Главное меню"",""payload"":""{\""command\"":\""menu\""}""},""color"":""negative""}]]}";

                case "language":
                    return @"{""one_time"":false,""buttons"":[[{""action"":{""type"":""text"",""label"":""🇬🇧 Английский"",""payload"":""{\""lang\"":\""english\""}""},""color"":""primary""},{""action"":{""type"":""text"",""label"":""🇨🇳 Китайский"",""payload"":""{\""lang\"":\""chinese\""}""},""color"":""primary""}],[{""action"":{""type"":""text"",""label"":""🇯 Японский"",""payload"":""{\""lang\"":\""japanese\""}""},""color"":""primary""},{""action"":{""type"":""text"",""label"":""🇰 Корейский"",""payload"":""{\""lang\"":\""korean\""}""},""color"":""primary""}],[{""action"":{""type"":""text"",""label"":""🏠 Главное меню"",""payload"":""{\""command\"":\""menu\""}""},""color"":""negative""}]]}";

                case "level":
                    return @"{""one_time"":false,""buttons"":[[{""action"":{""type"":""text"",""label"":""Начальный"",""payload"":""{\""level\"":\""beginner\""}""},""color"":""primary""},{""action"":{""type"":""text"",""label"":""Средний"",""payload"":""{\""level\"":\""intermediate\""}""},""color"":""primary""}],[{""action"":{""type"":""text"",""label"":""Продвинутый"",""payload"":""{\""level\"":\""advanced\""}""},""color"":""primary""}],[{""action"":{""type"":""text"",""label"":""🏠 Главное меню"",""payload"":""{\""command\"":\""menu\""}""},""color"":""negative""}]]}";

                default: // menu
                    return @"{""one_time"":false,""buttons"":[[{""action"":{""type"":""text"",""label"":""📚 Выбрать язык"",""payload"":""{\""command\"":\""language\""}""},""color"":""primary""},{""action"":{""type"":""text"",""label"":""📝 Тест уровня"",""payload"":""{\""command\"":\""test\""}""},""color"":""primary""}],[{""action"":{""type"":""text"",""label"":""🎓 Подобрать обучение"",""payload"":""{\""command\"":\""course\""}""},""color"":""secondary""},{""action"":{""type"":""text"",""label"":""👩‍💼 Позвать человека"",""payload"":""{\""command\"":\""help\""}""},""color"":""secondary""}],[{""action"":{""type"":""text"",""label"":""🏫 Наши школы"",""payload"":""{\""command\"":\""schools\""}""},""color"":""positive""}],[{""action"":{""type"":""text"",""label"":""🎁 Акции"",""payload"":""{\""command\"":\""sales\""}""},""color"":""positive""}]]}";
            }
        }
        #endregion

        // ===== ВСПОМОГАТЕЛЬНЫЙ МЕТОД ДЛЯ ФОРМАТИРОВАНИЯ ВОПРОСА =====
        static string FormatQuestion(TestQuestion question)
        {
            return $"{question.Text}\n\n" +
                   $"А) {question.Options[0]}\n" +
                   $"Б) {question.Options[1]}\n" +
                   $"В) {question.Options[2]}\n" +
                   $"Г) {question.Options[3]}\n\n" +
                   $"Выберите ответ:";
        }

        static string GetAnswer(long userId, string text)
        {
            if (!userState.ContainsKey(userId))
            {
                userState.TryAdd(userId, "menu");
                testAnswers.TryAdd(userId, new List<string>());
                testCorrectAnswers.TryAdd(userId, 0);
            }

            string state = userState[userId];

            // ======== КНОПКА ГЛАВНОГО МЕНЮ ========
            if (text == "меню" || text == "главное меню" || text == "отмена" || text == "выход")
            {
                if (testAnswers.ContainsKey(userId))
                {
                    testAnswers.TryRemove(userId, out _);
                }
                if (testCorrectAnswers.ContainsKey(userId))
                {
                    testCorrectAnswers.TryRemove(userId, out _);
                }
                userState[userId] = "menu";
                return "🏠 Вы вернулись в главное меню!\n\n👇 Выберите действие:";
            }

            // ======== ПРИВЕТСТВИЕ ========
            if (text == "start" || text == "начать" || text == "привет")
            {
                userState[userId] = "menu";
                return "🍵 Здравствуйте! Добро пожаловать в TEA — школу иностранных языков! 🌍\n\n" +
                       "Я помогу вам:\n" +
                       "• Выбрать язык для изучения\n" +
                       "• Определить ваш уровень\n" +
                       "• Подобрать обучение\n" +
                       "• Связаться с менеджером\n\n" +
                       "👇 Нажмите на кнопку ниже, чтобы начать!";
            }

            // ======== КОМАНДЫ МЕНЮ ========
            if (text == "1")
            {
                userState[userId] = "waiting_language";
                return "📚 ВЫБЕРИТЕ ЯЗЫК:\n\nНажмите на кнопку ниже:";
            }

            // ===== ТЕСТ УРОВНЯ =====
            if (text == "2")
            {
                // Проверяем, выбран ли язык
                if (!userLanguage.ContainsKey(userId) || string.IsNullOrEmpty(userLanguage[userId]))
                {
                    userState[userId] = "waiting_test_language";
                    return "📝 ТЕСТ УРОВНЯ ЯЗЫКА\n\nСначала выберите язык для тестирования:\n\nНажмите на кнопку ниже:";
                }
                else
                {
                    // Язык выбран, начинаем тест
                    string lang = userLanguage[userId];
                    testAnswers[userId] = new List<string>();
                    testCorrectAnswers[userId] = 0;
                    userState[userId] = "waiting_test_q1";

                    var questions = languageTests[lang];
                    return $"📝 ТЕСТ УРОВНЯ ({lang.ToUpper()})\n\n{FormatQuestion(questions[0])}";
                }
            }

            if (text == "3")
            {
                userState[userId] = "waiting_selection";
                return "🎓 ПОДБОР ОБУЧЕНИЯ\n\nОтветьте на вопросы:\n1. Ваш возраст?\n2. Цель изучения?\n3. Удобное время?\n\nНапишите через запятую, например: 25, работа, вечер";
            }
            if (text == "4")
            {
                Database.AddManagerRequest(userId, text, "VK");
                return "👩‍💼 ЗАЯВКА ПЕРЕДАНА МЕНЕДЖЕРУ\n\nОпишите вопрос. Специалист свяжется с вами.\n\nВремя работы: Пн-Пт 10:00-20:00";
            }
            if (text == "5")
            {
                return "🎁 АКЦИИ!\n\n🔥 Скидка 20% на первый месяц\n🎫 Подарочные сертификаты\n👥 Приведи друга — месяц бесплатно\n\nНажмите 👩‍💼 Позвать человека для подробностей";
            }
            if (text == "6" || text == "школы")
            {
                return "🏫 НАШИ ШКОЛЫ TEA\n\n🌐 **Онлайн-школа** — учитесь из любого города!\n\nНажмите 👩‍ Позвать человека для записи на пробный урок!";
            }

            // ======== ВЫБОР ЯЗЫКА ========
            if (state == "waiting_language" || state == "waiting_test_language")
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

                Database.SaveLanguage(userId, lang, "VK");
                userLanguage[userId] = lang;

                // Если это был выбор языка для теста, начинаем тест
                if (state == "waiting_test_language")
                {
                    testAnswers[userId] = new List<string>();
                    testCorrectAnswers[userId] = 0;
                    userState[userId] = "waiting_test_q1";

                    var questions = languageTests[lang];
                    return $"📝 ТЕСТ УРОВНЯ ({lang.ToUpper()})\n\n{FormatQuestion(questions[0])}";
                }
                else
                {
                    userState[userId] = "waiting_level";
                    return $"🇬🇧 Отлично! Вы выбрали {lang}.\n\nКакой у вас уровень?\n\nВыберите уровень:";
                }
            }

            if (state == "waiting_level")
            {
                string level = "";
                if (text == "а" || text == "a") level = "начальный";
                else if (text == "б" || text == "b") level = "средний";
                else if (text == "в" || text == "v" || text == "c") level = "продвинутый";
                else return "Пожалуйста, выберите уровень из кнопок ниже:";

                Database.SaveLevel(userId, level, "VK");
                userState[userId] = "menu";
                return $"✅ Спасибо! {level} уровень сохранён.\n\nЧто дальше?\n• Нажмите 🎓 Подобрать обучение\n• Или 📚 Меню";
            }

            // ======== НАСТОЯЩИЙ ТЕСТ ЯЗЫКА ========
            if (state.StartsWith("waiting_test_q"))
            {
                return HandleLanguageTest(userId, state, text);
            }

            if (state == "waiting_selection")
            {
                userState[userId] = "menu";
                return "🎓 Спасибо! Мы подобрали для вас обучение.\n\nСпециалист свяжется с вами в ближайшее время.\n\nНажмите 📚 Меню для возврата";
            }

            // ======== ГЛАВНОЕ МЕНЮ ========
            return "🍵 ДОБРО ПОЖАЛОВАТЬ В TEA!\n\n👇 Нажмите на кнопку ниже, чтобы выбрать действие:\n\n📚 Выбрать язык\n📝 Пройти тест уровня\n🎓 Подобрать обучение\n👩‍💼 Позвать человека\n🏫 Наши школы\n🎁 Акции и сертификаты";
        }

        #region Настоящий тест языка
        static string HandleLanguageTest(long userId, string state, string text)
        {
            if (!testAnswers.ContainsKey(userId))
            {
                testAnswers.TryAdd(userId, new List<string>());
            }
            if (!testCorrectAnswers.ContainsKey(userId))
            {
                testCorrectAnswers.TryAdd(userId, 0);
            }

            int questionNum = int.Parse(state.Replace("waiting_test_q", ""));
            string lang = userLanguage[userId];
            var questions = languageTests[lang];

            string userAnswer = text;
            string correctAnswer = questions[questionNum - 1].CorrectAnswer;

            testAnswers[userId].Add(userAnswer);

            if (userAnswer == correctAnswer)
            {
                testCorrectAnswers[userId]++;
            }

            if (questionNum < 5)
            {
                int nextQuestionNum = questionNum + 1;
                userState[userId] = $"waiting_test_q{nextQuestionNum}";

                var nextQuestion = questions[nextQuestionNum - 1];
                return FormatQuestion(nextQuestion);
            }
            else
            {
                int correctCount = testCorrectAnswers[userId];

                string level = "";
                string recommendation = "";

                if (correctCount <= 1)
                {
                    level = "A1 (Начальный)";
                    recommendation = "Рекомендуем базовый курс для начинающих";
                }
                else if (correctCount == 2)
                {
                    level = "A2 (Элементарный)";
                    recommendation = "Рекомендуем курс для продолжающих начинающих";
                }
                else if (correctCount == 3)
                {
                    level = "B1 (Средний)";
                    recommendation = "Рекомендуем стандартный курс среднего уровня";
                }
                else if (correctCount == 4)
                {
                    level = "B2 (Выше среднего)";
                    recommendation = "Рекомендуем интенсивный курс продвинутого уровня";
                }
                else
                {
                    level = "C1 (Продвинутый)";
                    recommendation = "Рекомендуем курс для продвинутых студентов";
                }

                // Очистка временных данных
                testAnswers.TryRemove(userId, out _);
                testCorrectAnswers.TryRemove(userId, out _);
                userState[userId] = "menu";

                return $"🎉 ТЕСТ ЗАВЕРШЁН! 🎉\n\n" +
                       $"🌐 Язык: {lang}\n" +
                       $"✅ Правильных ответов: {correctCount} из 5\n" +
                       $"📊 Ваш уровень: {level}\n" +
                       $"💡 {recommendation}\n\n" +
                       $"⭐ Следующий шаг: пробный урок бесплатно\n\n" +
                       $"Нажмите 👩‍💼 Позвать человека для записи";
            }
        }
        #endregion
    }
}