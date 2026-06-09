using System;
using System.Net;
using System.Text;
using VkNet;
using VkNet.Model;
using Newtonsoft.Json;
using VkNet.Utils;

namespace TeaBotSimple
{
    class Program
    {
        static void Main(string[] args)
        {
            VkApi api = new VkApi();

            string token = "vk1.a.oTmK-e2WnXXVARnONTVUMz7JkJNMTK6DAP20GhHmv_CjIOeEcuNp6MTjesAntB0VT2-QxOhBnsHNxjFY6kua8XljDPY5JlBj-UWdEUgqKNdZMXc0iv2VAqaHJDanWF00qxB6pwxg1pgvaUsMkMA-rPa9RfSxJQlyxMDeSL-cFpNlWEmf6cQ0gNzjSS_8vCYCO3iDcdCxGUX2ZpLe6YSvQw";

            api.Authorize(new ApiAuthParams { AccessToken = token });

            Console.WriteLine("Авторизация успешна!");
            Console.WriteLine("Бот слушает сообщения...");

            var server = api.Groups.GetLongPollServer(239436285);

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
                                    // === ИСПРАВЛЕНО: безопасное получение ID пользователя ===
                                    long userId = (long)update.@object.message.peer_id;

                                    string text = (update.@object.text ?? "").ToString().ToLower();

                                    Console.WriteLine($"Сообщение от {userId}: {text}");

                                    string answer = "";

                                    // Проверяем что написал пользователь
                                    if (text == "меню" || text == "привет" || text == "start" || text == "начать")
                                    {
                                        answer = "🍵 ДОБРО ПОЖАЛОВАТЬ В TEA!\n\n" +
                                                 "Выберите действие:\n" +
                                                 "━━━━━━━━━━━━━━━━━━━━━━\n" +
                                                 "1️⃣ Выбрать язык\n" +
                                                 "2️⃣ Пройти тест уровня\n" +
                                                 "3️⃣ Подобрать обучение\n" +
                                                 "4️⃣ Позвать человека\n" +
                                                 "5️⃣ Акции и сертификаты\n" +
                                                 "━━━━━━━━━━━━━━━━━━━━━━\n\n" +
                                                 "Напишите номер 1, 2, 3, 4 или 5";
                                    }
                                    else if (text == "1" || text == "выбрать язык" || text.Contains("язык"))
                                    {
                                        answer = "📚 ДОСТУПНЫЕ ЯЗЫКИ:\n\n" +
                                                 "• Английский 🇬🇧\n" +
                                                 "• Китайский 🇨🇳\n" +
                                                 "• Японский 🇯🇵\n" +
                                                 "• Корейский 🇰🇷\n" +
                                                 "• Испанский 🇪🇸\n" +
                                                 "• Французский 🇫🇷\n\n" +
                                                 "Напишите название языка, который вас интересует.\n" +
                                                 "Или напишите МЕНЮ для возврата в главное меню.";
                                    }
                                    else if (text == "2" || text.Contains("тест"))
                                    {
                                        answer = "📝 ТЕСТ УРОВНЯ ЯЗЫКА\n\n" +
                                                 "Вопрос 1 из 5:\n" +
                                                 "Как давно вы изучаете язык?\n\n" +
                                                 "А) Меньше года\n" +
                                                 "Б) 1-3 года\n" +
                                                 "В) Более 3 лет\n\n" +
                                                 "Напишите букву ответа (А, Б или В)";
                                    }
                                    else if (text == "3" || text.Contains("подобрать") || text.Contains("обучение"))
                                    {
                                        answer = "🎓 ПОДБОР ОБУЧЕНИЯ\n\n" +
                                                 "Ответьте на несколько вопросов, чтобы мы подобрали идеальный формат:\n\n" +
                                                 "1. Какой у вас возраст?\n" +
                                                 "2. Какая цель изучения?\n" +
                                                 "3. Удобное время для занятий?\n\n" +
                                                 "Напишите ответы через запятую, например: 25, работа, вечер";
                                    }
                                    else if (text == "4" || text.Contains("позвать") || text.Contains("менеджер") || text.Contains("помощь"))
                                    {
                                        answer = "👩‍💼 ЗАЯВКА ПЕРЕДАНА МЕНЕДЖЕРУ\n\n" +
                                                 "Опишите ваш вопрос одним сообщением.\n" +
                                                 "Наш специалист свяжется с вами в ближайшее время.\n\n" +
                                                 "Время работы: Пн-Пт с 10:00 до 20:00";
                                    }
                                    else if (text == "5" || text.Contains("акции") || text.Contains("скидка"))
                                    {
                                        answer = "🎁 АКЦИИ И СЕРТИФИКАТЫ\n\n" +
                                                 "🔥 Скидка 20% на первый месяц обучения!\n" +
                                                 "🎫 Подарочный сертификат — отличный подарок\n" +
                                                 "👥 Приведи друга и получи месяц бесплатно\n\n" +
                                                 "Напишите МЕНЕДЖЕР для подробностей или МЕНЮ для возврата.";
                                    }
                                    else if (text.Contains("английский"))
                                    {
                                        answer = "🇬🇧 АНГЛИЙСКИЙ ЯЗЫК\n\n" +
                                                 "У нас есть:\n" +
                                                 "• Групповые занятия\n" +
                                                 "• Индивидуальные уроки\n" +
                                                 "• Разговорный клуб\n" +
                                                 "• Подготовка к экзаменам\n\n" +
                                                 "Напишите 3 чтобы подобрать обучение или МЕНЮ для возврата.";
                                    }
                                    else if (text.Contains("китайский") || text.Contains("кит"))
                                    {
                                        answer = "🇨🇳 КИТАЙСКИЙ ЯЗЫК\n\n" +
                                                 "Иероглифика, тоны и культура.\n" +
                                                 "Занятия с носителями и опытными преподавателями.\n\n" +
                                                 "Напишите 3 чтобы подобрать обучение или МЕНЮ для возврата.";
                                    }
                                    else if (text.Contains("японский") || text.Contains("япон"))
                                    {
                                        answer = "🇯🇵 ЯПОНСКИЙ ЯЗЫК\n\n" +
                                                 "Хирагана, катакана, кандзи и разговорная практика.\n" +
                                                 "Подготовка к JLPT.\n\n" +
                                                 "Напишите 3 чтобы подобрать обучение или МЕНЮ для возврата.";
                                    }
                                    else if (text.Contains("корейский") || text.Contains("корей"))
                                    {
                                        answer = "🇰🇷 КОРЕЙСКИЙ ЯЗЫК\n\n" +
                                                 "Хангыль, грамматика и живое общение.\n" +
                                                 "К-рор, дорамы и TOPIK.\n\n" +
                                                 "Напишите 3 чтобы подобрать обучение или МЕНЮ для возврата.";
                                    }
                                    else
                                    {
                                        answer = "🍵 Я бот TEA — школы иностранных языков.\n\n" +
                                                 "Напишите МЕНЮ, чтобы увидеть все возможности.\n" +
                                                 "Или просто задайте вопрос — я помогу!\n\n" +
                                                 "Доступные команды:\n" +
                                                 "• МЕНЮ — главное меню\n" +
                                                 "• ПОМОЩЬ — связаться с менеджером";
                                    }

                                    var parameters = new VkParameters
                                    {
                                        { "peer_id", userId },
                                        { "message", answer },
                                        { "random_id", rnd.Next() }
                                    };
                                    api.Call("messages.send", parameters);
                                    Console.WriteLine("  → Ответ отправлен");
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
    }
}