using System;
using System.Net;
using System.Text;
using System.Collections.Generic;
using VkNet;
using VkNet.Model;
using Newtonsoft.Json;
using VkNet.Utils;

namespace TeaBotSimple
{
    class Program
    {
        // ===== НОВЫЙ ТОКЕН =====
        static string TOKEN = "vk1.a.OJKfI6n69LasJ8Y_AO2dLapZm7TRp-5XdIPw11Rl1sQboMzvEJhXMbLEewSgRV6IhJchygRBgibH2pAFmVezMfFWYfOl2Mcwlvor0FY-lgyqtKjeXDmUNU7YRoufEb52HS5zg83eiaG4pQt_GH4p5iWZjoqLHS9rSNfI1KHflKSA5275S5UYVOmeSoP17O5NkHEUFca5YmlnXn2mhDTcXQ";
        static ulong GROUP_ID = 239436285;

        static Dictionary<long, string> userState = new Dictionary<long, string>();
        static Dictionary<long, string> userLanguage = new Dictionary<long, string>();
        static Dictionary<long, List<string>> testAnswers = new Dictionary<long, List<string>>();

        static void Main(string[] args)
        {
            Database.Initialize();

            VkApi api = new VkApi();
            api.Authorize(new ApiAuthParams { AccessToken = TOKEN });
            Console.WriteLine("🍵 VK Бот TEA запущен!");

            var server = api.Groups.GetLongPollServer(GROUP_ID);
            string lastTs = server.Ts.ToString();
            Random rnd = new Random();

            while (true)
            {
                try
                {
                    string url = $"{server.Server}?act=a_check&key={server.Key}&ts={lastTs}&wait=25";
                    using (WebClient wc = new WebClient())
                    {
                        wc.Encoding = Encoding.UTF8;
                        string json = wc.DownloadString(url);
                        dynamic data = JsonConvert.DeserializeObject(json);
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
                                        string payload = update.@object.message.payload.ToString().ToLower();
                                        if (payload.Contains("command"))
                                        {
                                            if (payload.Contains("language")) text = "1";
                                            else if (payload.Contains("test")) text = "2";
                                            else if (payload.Contains("course")) text = "3";
                                            else if (payload.Contains("help")) text = "4";
                                            else if (payload.Contains("sales")) text = "5";
                                            else if (payload.Contains("schools")) text = "6";
                                            else if (payload.Contains("start")) text = "start";
                                        }
                                        else if (payload.Contains("answer"))
                                        {
                                            if (payload.Contains("a")) text = "а";
                                            else if (payload.Contains("b")) text = "б";
                                            else if (payload.Contains("c")) text = "в";
                                            else if (payload.Contains("d")) text = "г";
                                        }
                                        else if (payload.Contains("lang"))
                                        {
                                            if (payload.Contains("english")) text = "английский";
                                            else if (payload.Contains("chinese")) text = "китайский";
                                            else if (payload.Contains("japanese")) text = "японский";
                                            else if (payload.Contains("korean")) text = "корейский";
                                        }
                                        else if (payload.Contains("level"))
                                        {
                                            if (payload.Contains("beginner")) text = "а";
                                            else if (payload.Contains("intermediate")) text = "б";
                                            else if (payload.Contains("advanced")) text = "в";
                                        }
                                    }

                                    Console.WriteLine($"[VK] {userId}: {text}");

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
                    Console.WriteLine($"Ошибка: {ex.Message}");
                    System.Threading.Thread.Sleep(5000);
                }
            }
        }

        static string GetKeyboardType(long userId)
        {
            if (!userState.ContainsKey(userId)) return "menu";
            string state = userState[userId];
            if (state == "waiting_language") return "language";
            if (state == "waiting_level") return "level";
            if (state.StartsWith("waiting_test")) return "test";
            return "menu";
        }

        static void SendMessageWithKeyboard(VkApi api, long userId, string message, Random rnd, string keyboardType = "menu")
        {
            string keyboard = "";
            if (keyboardType == "test")
            {
                keyboard = @"{""one_time"":true,""buttons"":[[{""action"":{""type"":""text"",""label"":""А"",""payload"":""{\""answer\"":\""a\""}""},""color"":""primary""},{""action"":{""type"":""text"",""label"":""Б"",""payload"":""{\""answer\"":\""b\""}""},""color"":""primary""},{""action"":{""type"":""text"",""label"":""В"",""payload"":""{\""answer\"":\""c\""}""},""color"":""primary""}],[{""action"":{""type"":""text"",""label"":""Г"",""payload"":""{\""answer\"":\""d\""}""},""color"":""primary""}]]}";
            }
            else if (keyboardType == "language")
            {
                keyboard = @"{""one_time"":true,""buttons"":[[{""action"":{""type"":""text"",""label"":""🇬🇧 Английский"",""payload"":""{\""lang\"":\""english\""}""},""color"":""primary""},{""action"":{""type"":""text"",""label"":""🇨🇳 Китайский"",""payload"":""{\""lang\"":\""chinese\""}""},""color"":""primary""}],[{""action"":{""type"":""text"",""label"":""🇯🇵 Японский"",""payload"":""{\""lang\"":\""japanese\""}""},""color"":""primary""},{""action"":{""type"":""text"",""label"":""🇰🇷 Корейский"",""payload"":""{\""lang\"":\""korean\""}""},""color"":""primary""}]]}";
            }
            else if (keyboardType == "level")
            {
                keyboard = @"{""one_time"":true,""buttons"":[[{""action"":{""type"":""text"",""label"":""Начальный"",""payload"":""{\""level\"":\""beginner\""}""},""color"":""primary""},{""action"":{""type"":""text"",""label"":""Средний"",""payload"":""{\""level\"":\""intermediate\""}""},""color"":""primary""}],[{""action"":{""type"":""text"",""label"":""Продвинутый"",""payload"":""{\""level\"":\""advanced\""}""},""color"":""primary""}]]}";
            }
            else
            {
                keyboard = @"{""one_time"":false,""buttons"":[[{""action"":{""type"":""text"",""label"":""📚 Выбрать язык"",""payload"":""{\""command\"":\""language\""}""},""color"":""primary""},{""action"":{""type"":""text"",""label"":""📝 Тест уровня"",""payload"":""{\""command\"":\""test\""}""},""color"":""primary""}],[{""action"":{""type"":""text"",""label"":""🎓 Подобрать обучение"",""payload"":""{\""command\"":\""course\""}""},""color"":""secondary""},{""action"":{""type"":""text"",""label"":""👩‍💼 Позвать человека"",""payload"":""{\""command\"":\""help\""}""},""color"":""secondary""}],[{""action"":{""type"":""text"",""label"":""🏫 Наши школы"",""payload"":""{\""command\"":\""schools\""}""},""color"":""positive""}],[{""action"":{""type"":""text"",""label"":""🎁 Акции"",""payload"":""{\""command\"":\""sales\""}""},""color"":""positive""}]]}";
            }

            var parameters = new VkParameters
            {
                { "peer_id", userId },
                { "message", message },
                { "random_id", rnd.Next() },
                { "keyboard", keyboard }
            };
            api.Call("messages.send", parameters);
        }

        static string GetAnswer(long userId, string text)
        {
            if (!userState.ContainsKey(userId))
            {
                userState[userId] = "menu";
                testAnswers[userId] = new List<string>();
            }

            string state = userState[userId];

            // ======== ПРИВЕТСТВИЕ (НАЧАТЬ) ========
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

            // ======== ПРЯМАЯ ОБРАБОТКА КНОПОК ========
            if (text == "1")
            {
                userState[userId] = "waiting_language";
                return "📚 ВЫБЕРИТЕ ЯЗЫК:\n\nНажмите на кнопку ниже:";
            }
            if (text == "2")
            {
                userState[userId] = "waiting_test_question1";
                testAnswers[userId] = new List<string>();
                return "📝 ТЕСТ УРОВНЯ ЯЗЫКА\n\nВопрос 1 из 5:\n\nКак давно вы изучаете язык?\n\nА) Меньше года\nБ) 1-3 года\nВ) Более 3 лет\n\nВыберите ответ:";
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
                return "🏫 НАШИ ШКОЛЫ TEA\n\n" +
                       "📍 Мы работаем в нескольких городах:\n\n" +
                       "🏛️ **Москва**\n" +
                       "   ул. Тверская, д. 15\n" +
                       "   📞 +7 (495) 123-45-67\n\n" +
                       "🏛️ **Санкт-Петербург**\n" +
                       "   Невский проспект, д. 22\n" +
                       "   📞 +7 (812) 234-56-78\n\n" +
                       "🏛️ **Казань**\n" +
                       "   ул. Баумана, д. 10\n" +
                       "   📞 +7 (843) 345-67-89\n\n" +
                       "🌐 **Онлайн-школа** — учитесь из любого города!\n\n" +
                       "Нажмите 👩‍💼 Позвать человека для записи на пробный урок!";
            }

            // ======== ОБРАБОТКА PAYLOAD ========
            if (text.Contains("command"))
            {
                if (text.Contains("language")) return GetAnswer(userId, "1");
                if (text.Contains("test")) return GetAnswer(userId, "2");
                if (text.Contains("course")) return GetAnswer(userId, "3");
                if (text.Contains("help")) return GetAnswer(userId, "4");
                if (text.Contains("sales")) return GetAnswer(userId, "5");
                if (text.Contains("schools")) return GetAnswer(userId, "6");
            }

            // ======== ОБРАБОТКА СОСТОЯНИЙ ========
            if (state == "waiting_language")
            {
                string lang = "";
                if (text.Contains("английский")) { lang = "английский"; }
                else if (text.Contains("китайский")) { lang = "китайский"; }
                else if (text.Contains("японский")) { lang = "японский"; }
                else if (text.Contains("корейский")) { lang = "корейский"; }
                else
                {
                    return "📚 Пожалуйста, выберите язык из кнопок ниже:";
                }

                Database.SaveLanguage(userId, lang, "VK");
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

                Database.SaveLevel(userId, level, "VK");
                userState[userId] = "menu";
                return $"✅ Спасибо! {level} уровень сохранён.\n\nЧто дальше?\n• Нажмите 🎓 Подобрать обучение\n• Или 📚 Меню";
            }

            // ======== ТЕСТ ========
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
                userState[userId] = "menu";

                int score = 0;
                foreach (var ans in testAnswers[userId])
                {
                    if (ans == "а" || ans == "a") score++;
                }

                return $"🎉 ТЕСТ ЗАВЕРШЁН! 🎉\n\n⭐ Ваш результат: {score} из 5\n⭐ Рекомендация: групповые занятия 2 раза в неделю\n⭐ Уровень: средний (B1)\n⭐ Следующий шаг: пробный урок бесплатно\n\nНажмите 👩‍💼 Позвать человека для записи";
            }

            if (state == "waiting_selection")
            {
                userState[userId] = "menu";
                return "🎓 Спасибо! Мы подобрали для вас обучение.\n\nСпециалист свяжется с вами в ближайшее время.\n\nНажмите 📚 Меню для возврата";
            }

            // ======== ГЛАВНОЕ МЕНЮ ========
            return "🍵 ДОБРО ПОЖАЛОВАТЬ В TEA!\n\n👇 Нажмите на кнопку ниже, чтобы выбрать действие:\n\n📚 Выбрать язык\n📝 Пройти тест уровня\n🎓 Подобрать обучение\n👩‍💼 Позвать человека\n🏫 Наши школы\n🎁 Акции и сертификаты";
        }
    }
}