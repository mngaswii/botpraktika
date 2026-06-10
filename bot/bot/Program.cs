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
        static string TOKEN = "vk1.a.oTmK-e2WnXXVARnONTVUMz7JkJNMTK6DAP20GhHmv_CjIOeEcuNp6MTjesAntB0VT2-QxOhBnsHNxjFY6kua8XljDPY5JlBj-UWdEUgqKNdZMXc0iv2VAqaHJDanWF00qxB6pwxg1pgvaUsMkMA-rPa9RfSxJQlyxMDeSL-cFpNlWEmf6cQ0gNzjSS_8vCYCO3iDcdCxGUX2ZpLe6YSvQw";
        static ulong GROUP_ID = 239436285;

        static Dictionary<long, string> userState = new Dictionary<long, string>();
        static Dictionary<long, string> userLanguage = new Dictionary<long, string>();

        static void Main(string[] args)
        {
            VkApi api = new VkApi();
            api.Authorize(new ApiAuthParams { AccessToken = TOKEN });
            Console.WriteLine("Бот запущен!");

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
                                    Console.WriteLine($"{userId}: {text}");

                                    string answer = GetAnswer(userId, text);

                                    api.Call("messages.send", new VkParameters
                                    {
                                        { "peer_id", userId },
                                        { "message", answer },
                                        { "random_id", rnd.Next() }
                                    });
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

        static string GetAnswer(long userId, string text)
        {
            if (!userState.ContainsKey(userId))
                userState[userId] = "menu";

            string state = userState[userId];
            string answer = "";

            if (state == "waiting_language")
            {
                if (text.Contains("английский"))
                {
                    userLanguage[userId] = "english";
                    answer = "🇬🇧 Отлично! Вы выбрали английский.\n\nКакой у вас уровень?\nА) Начальный\nБ) Средний\nВ) Продвинутый\n\nНапишите А, Б или В";
                    userState[userId] = "waiting_level";
                }
                else if (text.Contains("китайский"))
                {
                    userLanguage[userId] = "chinese";
                    answer = "🇨🇳 Отлично! Вы выбрали китайский.\n\nКакой у вас уровень?\nА) Начальный\nБ) Средний\nВ) Продвинутый\n\nНапишите А, Б или В";
                    userState[userId] = "waiting_level";
                }
                else if (text.Contains("японский"))
                {
                    userLanguage[userId] = "japanese";
                    answer = "🇯🇵 Отлично! Вы выбрали японский.\n\nКакой у вас уровень?\nА) Начальный\nБ) Средний\nВ) Продвинутый\n\nНапишите А, Б или В";
                    userState[userId] = "waiting_level";
                }
                else if (text.Contains("корейский"))
                {
                    userLanguage[userId] = "korean";
                    answer = "🇰🇷 Отлично! Вы выбрали корейский.\n\nКакой у вас уровень?\nА) Начальный\nБ) Средний\nВ) Продвинутый\n\nНапишите А, Б или В";
                    userState[userId] = "waiting_level";
                }
                else
                {
                    answer = "📚 Доступные языки:\n• Английский 🇬🇧\n• Китайский 🇨🇳\n• Японский 🇯🇵\n• Корейский 🇰🇷\n\nНапишите название языка";
                }
            }
            else if (state == "waiting_level")
            {
                string level = "";
                if (text == "а" || text == "a") level = "начальный";
                else if (text == "б" || text == "b") level = "средний";
                else if (text == "в" || text == "v" || text == "c") level = "продвинутый";
                else level = text;

                answer = $"✅ Спасибо! {level} уровень сохранён.\n\nЧто дальше?\n• 3 — подобрать обучение\n• МЕНЮ — вернуться\n• ПОМОЩЬ — менеджер";
                userState[userId] = "menu";
            }
            else if (state == "waiting_test_question1")
            {
                answer = "📝 Вопрос 2 из 5:\n\nКакая у вас цель изучения языка?\n\nА) Работа\nБ) Путешествия\nВ) Экзамен\nГ) Для себя\n\nНапишите А, Б, В или Г";
                userState[userId] = "waiting_test_question2";
            }
            else if (state == "waiting_test_question2")
            {
                answer = "📝 Вопрос 3 из 5:\n\nСколько часов в неделю готовы заниматься?\n\nА) 1-2\nБ) 3-4\nВ) 5-6\nГ) Более 6\n\nНапишите А, Б, В или Г";
                userState[userId] = "waiting_test_question3";
            }
            else if (state == "waiting_test_question3")
            {
                answer = "📝 Вопрос 4 из 5:\n\nКакой формат удобнее?\n\nА) Групповой\nБ) Индивидуальный\nВ) Самостоятельно\nГ) Смешанный\n\nНапишите А, Б, В или Г";
                userState[userId] = "waiting_test_question4";
            }
            else if (state == "waiting_test_question4")
            {
                answer = "📝 Вопрос 5 из 5:\n\nКакой бюджет в месяц?\n\nА) До 3000₽\nБ) 3000-6000₽\nВ) 6000-10000₽\nГ) Более 10000₽\n\nНапишите А, Б, В или Г";
                userState[userId] = "waiting_test_result";
            }
            else if (state == "waiting_test_result")
            {
                answer = "🎉 ТЕСТ ЗАВЕРШЁН! 🎉\n\n⭐ Рекомендация: групповые занятия 2 раза в неделю\n⭐ Уровень: средний (B1)\n⭐ Следующий шаг: пробный урок бесплатно\n\nНапишите МЕНЕДЖЕР для записи или МЕНЮ";
                userState[userId] = "menu";
            }
            else if (state == "waiting_selection")
            {
                answer = "🎓 Спасибо! Мы подобрали для вас обучение.\n\nСпециалист свяжется с вами в ближайшее время.\n\nНапишите МЕНЮ для возврата";
                userState[userId] = "menu";
            }
            else
            {
                if (text == "меню" || text == "привет" || text == "start" || text == "начать")
                {
                    answer = "🍵 ДОБРО ПОЖАЛОВАТЬ В TEA!\n\n" +
                             "━━━━━━━━━━━━━━━━━━━━━━\n" +
                             "1️⃣ Выбрать язык\n" +
                             "2️⃣ Пройти тест уровня\n" +
                             "3️⃣ Подобрать обучение\n" +
                             "4️⃣ Позвать человека\n" +
                             "5️⃣ Акции и сертификаты\n" +
                             "━━━━━━━━━━━━━━━━━━━━━━\n\n" +
                             "Напишите номер 1, 2, 3, 4 или 5";
                    userState[userId] = "menu";
                }
                else if (text == "1")
                {
                    answer = "📚 ДОСТУПНЫЕ ЯЗЫКИ:\n\n• Английский 🇬🇧\n• Китайский 🇨🇳\n• Японский 🇯🇵\n• Корейский 🇰🇷\n\nНапишите название языка";
                    userState[userId] = "waiting_language";
                }
                else if (text == "2" || text.Contains("тест"))
                {
                    answer = "📝 ТЕСТ УРОВНЯ ЯЗЫКА\n\nВопрос 1 из 5:\n\nКак давно вы изучаете язык?\n\nА) Меньше года\nБ) 1-3 года\nВ) Более 3 лет\n\nНапишите А, Б или В";
                    userState[userId] = "waiting_test_question1";
                }
                else if (text == "3" || text.Contains("подобрать"))
                {
                    answer = "🎓 ПОДБОР ОБУЧЕНИЯ\n\n" +
                             "Ответьте на вопросы:\n" +
                             "1. Ваш возраст?\n" +
                             "2. Цель изучения?\n" +
                             "3. Удобное время?\n\n" +
                             "Напишите через запятую, например: 25, работа, вечер";
                    userState[userId] = "waiting_selection";
                }
                else if (text == "4" || text.Contains("помощь") || text.Contains("менеджер") || text.Contains("позвать"))
                {
                    answer = "👩‍💼 ЗАЯВКА ПЕРЕДАНА МЕНЕДЖЕРУ\n\nОпишите вопрос. Специалист свяжется с вами.\n\nВремя работы: Пн-Пт 10:00-20:00";
                }
                else if (text == "5" || text.Contains("акции"))
                {
                    answer = "🎁 АКЦИИ!\n\n🔥 Скидка 20% на первый месяц\n🎫 Подарочные сертификаты\n👥 Приведи друга — месяц бесплатно\n\nНапишите МЕНЕДЖЕР";
                }
                else
                {
                    answer = "🍵 Напишите МЕНЮ, чтобы увидеть команды.\n\nДоступно: МЕНЮ, 1, 2, 3, 4, 5";
                }
            }

            return answer;
        }
    }
}