using OfficeOpenXml;
using OfficeOpenXml.Drawing.Chart;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SQLite;
using System.IO;
using Telegram.Bot;
using Telegram.Bot.Args;
using Telegram.Bot.Types.InputFiles;
using Telegram.Bot.Types.ReplyMarkups;
namespace main
{
    class mainBot
    {
        #region Переменные паблики и приваты
        public static readonly SQLiteConnection DB = new SQLiteConnection(database.connection);
        private static TelegramBotClient Bot;
        public static int permission = 0;
        public static string curatorId { get; set; }
        public static int ReplyId = 0;
        public static string fileSource = @"C:\MyFiles\Отчет.xlsx";
        public static string curatorName { get; set; }
        public static string consoleOutput { get; set; }
        public static string curatorPerm { get; set; }
        public static string adminName { get; set; }
        public static string adminGroup { get; set; }
        public static string adminId { get; set; }
        public static string curatorCours { get; set; }
        public static List<string> studentNames = new List<string>();
        public static List<string> curatorCourses = new List<string>();
        public static string starostaName { get; set; }
        public static string studentName { get; set; }

        public static string studentDay { get; set; }
        public static string studentHours { get; set; }
        public static string starostaCours { get; set; }
        public static string targetCours { get; set; }
        public static string starostaPerm { get; set; }
        public static string monthTable { get; set; }
        public static string starostaId { get; set; }
        //public string[] StudentsList { get; private set; }
        #endregion
        public static void Main(string[] args)
        {

            DB.OpenAsync();
            if (!Directory.Exists(@"C:\MyFiles"))
            {
                Directory.CreateDirectory(@"C:\MyFiles");
            }
            Bot = new TelegramBotClient($"{ programm.token}");
            Bot.OnMessage += Bot_OnMessageReceived;
            Bot.StartReceiving();
            Console.WriteLine("Bot started");
            Console.ReadLine();

        }

        [Obsolete]
        private static async void Bot_OnMessageReceived(object? sender, MessageEventArgs e)
        {
            #region Кнопачки
            try
            {
                
                var message = e.Message;
                Console.WriteLine($"{message.From.Username} ({message.From.Id}) >> {message.Text}\n");
                
                var starostaBtn = new ReplyKeyboardMarkup
                {
                    Keyboard = new[]
    {
                    new []
                    {
                        new KeyboardButton("Пропуск🔍"),
                        new KeyboardButton("Нет преподавателя"),
                        new KeyboardButton("Отчет🟢")
                    }
                },
                    ResizeKeyboard = true
                };
                var curatorBtn = new ReplyKeyboardMarkup
                {
                    Keyboard = new[]
{
                    new []
                    {
                        new KeyboardButton("Пропуск🔍"),
                        new KeyboardButton("Переключить группу"),
                        new KeyboardButton("Отчет🟢")
                    },
                    new []
                    {
                        new KeyboardButton("Создать месяц"),
                        new KeyboardButton("Создать группу"),
                        new KeyboardButton("Перенести")
                    }

                },
                    ResizeKeyboard = true
                };
                var starostaHoursBtn = new ReplyKeyboardMarkup
                {
                    Keyboard = new[]
    {
                    new []
                    {
                        new KeyboardButton("2"),
                        new KeyboardButton("4"),
                        new KeyboardButton("6")
                    }
                },
                    ResizeKeyboard = true
                };
                var starostaTypeBtn = new ReplyKeyboardMarkup
                {
                    Keyboard = new[]
{
                    new []
                    {
                        new KeyboardButton("Уважительная"),
                        new KeyboardButton("Неуважительная"),
                        new KeyboardButton("Выход")
                    }
                },
                    ResizeKeyboard = true
                };
                #endregion
                switch (message.Text)
                {
                    case "/start":
                        await Bot.SendTextMessageAsync(message.From.Id, "Здравствуйте!");
                        break;
                    case "Команды❓":
                        await Bot.SendTextMessageAsync(message.From.Id, "Здравствуйте! Вот мой список команд:\n\nКураторам:\n1)Создать группу - создает таблицу группы, привязанной к вашему профилю" +
                            "\n2)Создать месяц - создает таблицу для отчета посещаемости студентов\n3)Перенести - переносит с таблицы с ФИО студеннтами в таблицу с отчетом посещаемости" +
                            "\n4)Староста - добавляет старосту в таблицу группы" +
                            "\n5)Переключить группу - дает возможность переключиться по группе" +
                            "\n\nСтаростам:\n1)Пропуск - вносит прогул за ТЕКУЩИЙ день\n2)Прогул/ФИО/число месяца (без указания месяца)/часы - вносит прогул за любой указанный вами день\n3)Отчет - создает отчет за месяц и отправляет вам", replyMarkup: starostaBtn);
                        break;
                    case "Выход":
                        LoadCurator(message.From.Id.ToString());
                        LoadStarosta(message.From.Id.ToString());
                        LoadAdmins(message.From.Id.ToString());
                        if (message.From.Id.ToString() == starostaId)
                        {
                            UpdatePerm(message.From.Id.ToString(), "Старосты", "0");
                            await Bot.SendTextMessageAsync(message.From.Id, "Хорошо)", replyMarkup: starostaBtn);
                        }
                        else if (message.From.Id.ToString() == curatorId)
                        {
                            UpdatePerm(message.From.Id.ToString(), "Кураторы", "0");
                            await Bot.SendTextMessageAsync(message.From.Id, "Хорошо)", replyMarkup: curatorBtn);
                        }                        
                        else if (message.From.Id.ToString() == adminId)
                        {
                            await Bot.SendTextMessageAsync(message.From.Id, "Хорошо)", replyMarkup: curatorBtn);
                        }
                        break;
                }
                
                #region Все команды
                if (message.Text.Contains("Пропуск🔍"))
                {
                    string post= "";
                    Together();
                    LoadStarosta(message.From.Id.ToString());
                    LoadCurator(message.From.Id.ToString());
                    LoadAdmins(message.From.Id.ToString());
                    if (message.From.Id.ToString() == starostaId)
                    {
                        targetCours = starostaCours;
                    }
                    else if (message.From.Id.ToString() == curatorId)
                    {
                        targetCours = curatorCours;

                    }
                    else if (message.From.Id.ToString() == adminId)
                    {
                        targetCours = adminGroup;
                    }
                    studentNames.Clear();
                    SQLiteCommand cmd = new SQLiteCommand($"SELECT name FROM [{targetCours}]", DB);
                    SQLiteDataReader reader = cmd.ExecuteReader();
                    while (reader.Read())
                    {
                        string studentName = reader.GetString(0);
                        if (studentName != null)
                        {
                            studentNames.Add(studentName);
                        }
                    }
                    reader.Close();
                    string table = $"{monthTable}{targetCours}";
                    var uttons = studentNames.Select(name => new KeyboardButton[] { new KeyboardButton(name), new KeyboardButton("Выход") }).ToArray();
                    var namesKb = new ReplyKeyboardMarkup(uttons);
                    await Bot.SendTextMessageAsync(message.From.Id, "🔍Выберите студента:", replyMarkup: namesKb);
                    if (message.From.Id.ToString() == starostaId)
                    {
                        UpdatePerm(message.From.Id.ToString(), "Старосты", "1");
                        starostaPerm = "1";
                    }
                    else if (message.From.Id.ToString() == curatorId)
                    {
                        UpdatePerm(message.From.Id.ToString(), "Кураторы", "1");
                        curatorPerm = "1";

                    }
                }
                if (message.Text.StartsWith("Обновление") && message.From.Id == 1251534440)
                {
                    string target_id;
                    string[] parts = message.Text.Split('/');
                    string updatemessage = parts[1];
                    SQLiteCommand AllSelectCurator = new SQLiteCommand("SELECT * FROM curators", DB);
                    SQLiteDataReader readerCurator = AllSelectCurator.ExecuteReader();
                    while (readerCurator.Read())
                    {
                        target_id = readerCurator.GetString(2);
                        await Bot.SendTextMessageAsync(target_id, $"🔄Обновление!\n {updatemessage}");
                    }
                    readerCurator.Close();
                    Thread.Sleep(700);
                    SQLiteCommand AllSelectStarosta = new SQLiteCommand("SELECT * FROM starosta", DB);
                    SQLiteDataReader readerStarosta = AllSelectStarosta.ExecuteReader();
                    while(readerStarosta.Read())
                    {
                        target_id = readerStarosta.GetString(2);
                        await Bot.SendTextMessageAsync(target_id, $"🔄Обновление!: {updatemessage}");
                    }
                    readerStarosta.Close();
                    await Bot.SendTextMessageAsync(1251534440, "Выслал всем");

                }
                if (message.Text.StartsWith("Нет преподавателя"))
                {
                    LoadStarosta(message.From.Id.ToString());   
                    SQLiteCommand AllSelectCurator = new SQLiteCommand("SELECT * FROM curators", DB);
                    SQLiteDataReader readerCurator = AllSelectCurator.ExecuteReader();
                    while (readerCurator.Read())
                    {
                        var target_id = readerCurator.GetString(2);
                        await Bot.SendTextMessageAsync(target_id, $"ВНИМАНИЕ!\nУ курса {starostaCours} отсутствует преподаватель!\nСрочно нужен преподаватель");
                    }
                    Thread.Sleep(700);
                    await Bot.SendTextMessageAsync(1251534440, "Отправил кураторам, начинаю отправку администраторам..");
                    SQLiteCommand AllSelectStarosta = new SQLiteCommand("SELECT * FROM admins", DB);
                    SQLiteDataReader readerStarosta = AllSelectStarosta.ExecuteReader();
                    while (readerStarosta.Read())
                    {
                        var target_id = readerStarosta.GetString(2);
                        await Bot.SendTextMessageAsync(target_id, $"ВНИМАНИЕ!\nУ курса {starostaCours} отсутствует преподаватель!\nСрочно нужен преподаватель");
                    }
                    await Bot.SendTextMessageAsync(message.From.Id, "Отправил кураторам и администраторам! Они проконтролируют");

                }
                if (studentNames.Contains(message.Text))
                {
                    LoadCurator(message.From.Id.ToString());
                    LoadAdmins(message.From.Id.ToString());
                    LoadStarosta(message.From.Id.ToString());
                    if (starostaPerm == "1" || curatorPerm == "1" || message.From.Id.ToString() == adminId)
                    {
                        studentName = message.Text;
                        await Bot.SendTextMessageAsync(message.From.Id, "Выберите сколько прогулял студент", replyMarkup: starostaHoursBtn);
                    }
                }
                if (message.Text.StartsWith("2") || message.Text.StartsWith("4") || message.Text.StartsWith("6"))
                {
                    LoadCurator(message.From.Id.ToString());
                    LoadStarosta(message.From.Id.ToString());
                    LoadAdmins(message.From.Id.ToString());

                    if (starostaPerm == "1" || curatorPerm == "1" || message.From.Id.ToString() == adminId)
                    {
                        studentHours = message.Text;
                        await Bot.SendTextMessageAsync(message.From.Id, "Выбери вид пропуска:", replyMarkup: starostaTypeBtn);
                    }
                }
                if (message.Text.StartsWith("Уважительная"))
                {
                    Together();
                    LoadCurator(message.From.Id.ToString());
                    LoadAdmins(message.From.Id.ToString());
                    LoadStarosta(message.From.Id.ToString());
                    if (starostaPerm == "1" || curatorPerm == "1" || message.From.Id.ToString() == adminId)
                    {
                        if (message.From.Id.ToString() == starostaId)
                        {
                            targetCours = starostaCours;
                            UpdatePerm(message.From.Id.ToString(), "Старосты", "0");
                            await Bot.SendTextMessageAsync(message.From.Id, $"Пропуск пары!\nСтудент: {studentName}\nЧисло месяца: {DateTime.Today.Day}\nПропустил: {studentHours} часов\nПричина: Уважительная\nГруппа: {targetCours}", replyMarkup: starostaBtn);
                        }
                        else if (message.From.Id.ToString() == curatorId)
                        {
                            targetCours = curatorCours;
                            UpdatePerm(message.From.Id.ToString(), "Кураторы", "0");
                            await Bot.SendTextMessageAsync(message.From.Id, $"Пропуск пары!\nСтудент: {studentName}\nЧисло месяца: {DateTime.Today.Day}\nПропустил: {studentHours} часов\nПричина: Уважительная\nГруппа: {targetCours}", replyMarkup: curatorBtn);
                        }
                        else if (message.From.Id.ToString() == adminId)
                        {
                            targetCours = adminGroup;
                            await Bot.SendTextMessageAsync(message.From.Id, $"Пропуск пары!\nСтудент: {studentName}\nЧисло месяца: {DateTime.Today.Day}\nПропустил: {studentHours} часов\nПричина: Уважительная\nГруппа: {targetCours}", replyMarkup: curatorBtn);
                        }
                        string table = $"{monthTable}{targetCours}";
                        AddStudentNull(table, DB, studentHours, studentName);
                    }

                }
                if (message.Text.StartsWith("Неуважительная"))
                {
                    Together();
                    LoadCurator(message.From.Id.ToString());
                    LoadAdmins(message.From.Id.ToString());
                    LoadStarosta(message.From.Id.ToString());
                    if (starostaPerm == "1" || curatorPerm == "1" || message.From.Id.ToString() == adminId)
                    {
                        if (message.From.Id.ToString() == starostaId)
                        {
                            targetCours = starostaCours;
                            UpdatePerm(message.From.Id.ToString(), "Старосты", "0");
                            await Bot.SendTextMessageAsync(message.From.Id, $"Пропуск пары!\nСтудент: {studentName}\nЧисло месяца: {DateTime.Today.Day}\nПропустил: {studentHours} часов\nПричина: Неуважительная\nГруппа: {targetCours}", replyMarkup: starostaBtn);
                        }
                        else if (message.From.Id.ToString() == curatorId)
                        {
                            targetCours = curatorCours;
                            UpdatePerm(message.From.Id.ToString(), "Кураторы", "0");
                            await Bot.SendTextMessageAsync(message.From.Id, $"Пропуск пары!\nСтудент: {studentName}\nЧисло месяца: {DateTime.Today.Day}\nПропустил: {studentHours} часов\nПричина: Неуважительная\nГруппа: {targetCours}", replyMarkup: curatorBtn);
                        }
                        else if (message.From.Id.ToString() == adminId)
                        {
                            targetCours = adminGroup;
                            await Bot.SendTextMessageAsync(message.From.Id, $"Пропуск пары!\nСтудент: {studentName}\nЧисло месяца: {DateTime.Today.Day}\nПропустил: {studentHours} часов\nПричина: Уважительная\nГруппа: {targetCours}", replyMarkup: curatorBtn);
                        }
                        string table = $"{monthTable}{targetCours}";
                        AddStudentNull(table, DB, $"-{studentHours}", studentName);
                    }
                }
                if (message.Text.Contains("Создать месяц"))
                {
                    string currentMonth = DateTime.Now.ToString("MMMM", new System.Globalization.CultureInfo("en-US"));
                    string monthTable = currentMonth.ToLower();

                    SQLiteCommand command1 = new SQLiteCommand("SELECT name, cours, tg_id FROM curators", DB);
                    SQLiteDataReader sqlite_datareader = command1.ExecuteReader();
                    while (sqlite_datareader.Read())
                    {
                        curatorName = sqlite_datareader.GetString(0);
                        curatorCours = sqlite_datareader.GetString(1);
                        string tg_id = sqlite_datareader.GetString(2);
                        //Console.WriteLine(curatorName + " " + curatorCours);
                        if (tg_id == message.From.Id.ToString())
                        {
                            string tableName = $"{monthTable}{curatorCours}";
                            CreateTableNone(tableName, DB);
                            await Bot.SendTextMessageAsync(message.From.Id, "✅Таблица " + tableName + " создана!\n🔴Это таблица для контроля посещаемости студентов\n🔴Староста теперь работает с новым месяцем", replyMarkup: curatorBtn);
                        }
                    }
                    sqlite_datareader.Close();
                }
                if (message.Text.Contains("Запросить"))
                { 
                    await Bot.SendTextMessageAsync(message.From.Id, $"🕐Ждем ответа от администратора\n⚪Как только вас примут, я отправлю вам сообщение с вашей должностью");
                    await Bot.SendTextMessageAsync(1251534440, $"⚪Пользователь с айди {message.From.Id}, хочет в систему");
                }
                if (message.Text.StartsWith("Куратор"))
                {
                    LoadAdmins(message.From.Id.ToString());
                    if (message.From.Id.ToString() == adminId)
                    {
                        string[] parts = message.Text.Split('/');
                        curatorId = parts[1];
                        curatorName = parts[2];
                        curatorCours = parts[3];
                        SQLiteCommand command = new SQLiteCommand("INSERT INTO curators (name, cours, tg_id) VALUES (@name, @cours, @tg_id)", DB);
                        command.Parameters.AddWithValue("@tg_id", curatorId);
                        command.Parameters.AddWithValue("@name", curatorName);
                        command.Parameters.AddWithValue("@cours", curatorCours);
                        command.ExecuteNonQuery();
                        await Bot.SendTextMessageAsync(message.From.Id, $"👼Куратор с айди {curatorId} \n🔴Имя: {curatorName} \n🔴Группа {curatorCours} \n✅Куратор успешно зарегистрирован!");
                        await Bot.SendTextMessageAsync(curatorId, $"✅Вы успешно зарегистрированы!\n🔴Ваше имя: {curatorName}\n🔴Ваша группа {curatorCours}\n👼Должность: Куратор", replyMarkup: curatorBtn);
                    }

                }
                if (message.Text.StartsWith("Админ"))
                {
                    if (message.From.Id == 1251534440)
                    {
                        string[] parts = message.Text.Split('/');
                        string idAmd = parts[1];
                        string nameAdm = parts[2];
                        string groupAdm = parts[3];
                        SQLiteCommand command = new SQLiteCommand("INSERT INTO admins (name, c, tg_id) VALUES (@name, @c, @tg_id)", DB);
                        command.Parameters.AddWithValue("@tg_id", idAmd);
                        command.Parameters.AddWithValue("@name", nameAdm);
                        command.Parameters.AddWithValue("@c", groupAdm);
                        command.ExecuteNonQuery();
                        await Bot.SendTextMessageAsync(message.From.Id, $"👼Админ с айди {idAmd} \n🔴Имя: {nameAdm} \n🔴Группа {groupAdm} \n✅Админ успешно зарегистрирован!");
                        await Bot.SendTextMessageAsync(idAmd, $"✅Вы успешно зарегистрированы!\n🔴Ваше имя: {nameAdm}\n🔴Ваша группа {groupAdm} (переключиться на другую группу: Выбрать группу/Группа)\n👼Должность: Администратор", replyMarkup: curatorBtn);
                    }

                }
                if (message.Text.StartsWith("Староста"))
                {
                    LoadCurator(message.From.Id.ToString());
                    LoadAdmins(message.From.Id.ToString());
                    if (message.From.Id.ToString() == curatorId || message.From.Id.ToString() == adminId)
                    {
                        string[] parts = message.Text.Split('/');
                        starostaId = parts[1];
                        starostaName = parts[2];
                        starostaCours = parts[3];
                        SQLiteCommand command = new SQLiteCommand("INSERT INTO starosta (name, cours, tg_id) VALUES (@name, @cours, @tg_id)", DB);
                        command.Parameters.AddWithValue("@tg_id", starostaId);
                        command.Parameters.AddWithValue("@name", starostaName);
                        command.Parameters.AddWithValue("@cours", starostaCours);
                        command.ExecuteNonQuery();
                        await Bot.SendTextMessageAsync(message.From.Id, $"👨‍🎤Староста с айди {starostaId} \n🟢Имя: {starostaName} \n🟢Группа {starostaCours} \n✅Староста успешно зарегистрирован!", replyMarkup:curatorBtn);
                        await Bot.SendTextMessageAsync(starostaId, $"✅Вы успешно зарегистрированы!\n🟢Ваше имя: {starostaName}\n🟢Ваша группа {starostaCours}\n👨‍🎤Должность: Староста", replyMarkup: starostaBtn);
                    }
                }
                if (message.Text.StartsWith("Отправь/"))
                {
                    string[] parts = message.Text.Split('/');
                    var id = parts[1];
                    var replyMessage = parts[2];
                    await Console.Out.WriteLineAsync($"Кому: {id}\nСообщение: {replyMessage}\nот: {message.From.Id}\n");
                    await Bot.SendTextMessageAsync(id, $"💬Новое сообщение!\n\n💭{replyMessage}\n\n✅Чтобы ответить на данное сообщение, введите следующую команду: Отправь/{message.From.Id}/текст сообщения");
                    await Bot.DeleteMessageAsync(message.From.Id, message.MessageId);
                    await Bot.SendTextMessageAsync(message.From.Id, $"💌Сообщение отправлено!\nВаше сообщение: {replyMessage}");
                }
                if (message.Text.StartsWith("Клава/"))
                {
                    string[] parts = message.Text.Split('/');
                    var id = parts[1];
                    var keyboard = parts[2];
                    if (keyboard == "1")
                    {
                        await Bot.SendTextMessageAsync(id, "Чиним вас!", replyMarkup: starostaBtn);

                    }
                    else if (keyboard == "2")
                    {
                        await Bot.SendTextMessageAsync(id, "Чиним вас!", replyMarkup: curatorBtn);

                    }
                }
                if (message.Text.Contains("Создать группу"))
                {
                    LoadCurator(message.From.Id.ToString());
                    LoadAdmins(message.From.Id.ToString());
                    if (message.From.Id.ToString() == starostaId || message.From.Id.ToString() == curatorId || message.From.Id.ToString() == adminId)
                    {
                        string tableName = curatorCours;
                        CreateTableGroup(tableName, DB);
                        await Bot.SendTextMessageAsync(message.From.Id, $"✅Группа: {curatorCours} успешно создана!\n🔴Теперь добавьте в нее студентов с помощью команды: Добавить/ФИО Студента", replyMarkup: curatorBtn);

                    }
                }
                if (message.Text.Contains("Отчет"))
                {
                    Together();
                    LoadCurator(message.From.Id.ToString());
                    LoadStarosta(message.From.Id.ToString());
                    LoadAdmins(message.From.Id.ToString());
                    if (message.From.Id.ToString() == starostaId)
                    {
                        string table = $"{monthTable}{starostaCours}";
                        ExportToExcel(fileSource, table);
                        var fileStream = File.OpenRead(fileSource);
                        InputOnlineFile inputOnlineFile = new InputOnlineFile(fileStream, "Отчет.xlsx");
                        await Bot.SendDocumentAsync(message.From.Id, inputOnlineFile, replyMarkup: starostaBtn);
                        fileStream.Close();
                    }
                    else if (message.From.Id.ToString() == curatorId)
                    {
                        string table = $"{monthTable}{curatorCours}";
                        ExportToExcel(fileSource, table);
                        var fileStream = File.OpenRead(fileSource);
                        InputOnlineFile inputOnlineFile = new InputOnlineFile(fileStream, "Отчет.xlsx");
                        await Bot.SendDocumentAsync(message.From.Id, inputOnlineFile, replyMarkup: curatorBtn);
                        fileStream.Close();
                    }
                    else if (message.From.Id.ToString() == adminId)
                    {
                        string table = $"{monthTable}{adminGroup}";
                        ExportToExcel(fileSource, table);
                        var fileStream = File.OpenRead(fileSource);
                        InputOnlineFile inputOnlineFile = new InputOnlineFile(fileStream, "Отчет.xlsx");
                        await Bot.SendDocumentAsync(message.From.Id, inputOnlineFile, replyMarkup: curatorBtn);
                        fileStream.Close();
                    }

                }
                if (message.Text.Contains("Добавить/"))
                {
                    string[] parts = message.Text.Split('/');
                    studentName = parts[1];
                    LoadCurator(message.From.Id.ToString());
                    LoadStarosta(message.From.Id.ToString());
                    await Bot.SendTextMessageAsync(message.From.Id, $"✅Студент: {studentName} - добавлен в таблицу группы {curatorCours}");
                    string table = curatorCours;
                    AddStudent(table, DB, studentName);
                }
                if (message.Text.StartsWith("Удалить старосту"))
                {
                    string[] parts = message.Text.Split('/');
                    var deleteId = parts[1];
                    LoadCurator(message.From.Id.ToString());
                    LoadAdmins(message.From.Id.ToString());
                    if (message.From.Id.ToString() == curatorId || message.From.Id.ToString() == adminId) 
                    { 
                        SQLiteCommand cmd = new SQLiteCommand("DELETE FROM starosta WHERE tg_id=@tg_id", DB);
                        cmd.Parameters.AddWithValue("@tg_id", deleteId);
                        cmd.ExecuteNonQuery();
                        await Bot.SendTextMessageAsync(message.From.Id, "Староста удален");
                        await Bot.SendTextMessageAsync(deleteId, "Вы были отключены от системы, всего хорошего!");
                    }
                }
                if (message.Text.StartsWith("Удалить куратора"))
                {
                    string[] parts = message.Text.Split('/');
                    var deleteId = parts[1];
                    LoadAdmins(message.From.Id.ToString());
                    if (message.From.Id.ToString() == adminId)
                    {
                        SQLiteCommand cmd = new SQLiteCommand("DELETE FROM curators WHERE tg_id=@tg_id", DB);
                        cmd.Parameters.AddWithValue("@tg_id", deleteId);
                        cmd.ExecuteNonQuery();
                        await Bot.SendTextMessageAsync(message.From.Id, "Староста удален");
                        await Bot.SendTextMessageAsync(deleteId, "Вы были отключены от системы, всего хорошего!");
                    }
                }
                if (message.Text.StartsWith("Удалить админа"))
                {
                    string[] parts = message.Text.Split('/');
                    var deleteId = parts[1];
                    LoadAdmins(message.From.Id.ToString());
                    if (message.From.Id == 1251534440)
                    {
                        SQLiteCommand cmd = new SQLiteCommand("DELETE FROM admins WHERE tg_id=@tg_id", DB);
                        cmd.Parameters.AddWithValue("@tg_id", deleteId);
                        cmd.ExecuteNonQuery();
                        await Bot.SendTextMessageAsync(message.From.Id, "Админ удален");
                        await Bot.SendTextMessageAsync(deleteId, "Вы были отключены от системы, всего хорошего!");
                    }
                }
                if (message.Text.StartsWith("Переключить группу"))
                {
                    curatorCourses.Clear();
                    LoadCurator(message.From.Id.ToString());
                    if (message.From.Id.ToString() == curatorId)
                    {
                        SQLiteCommand command = new SQLiteCommand($"SELECT cours1, cours2, cours3 FROM curators WHERE tg_id='{message.From.Id}'", DB);
                        SQLiteDataReader reader = command.ExecuteReader();
                        reader.Read();
                        string cours1 = reader["cours1"].ToString();
                        string cours2 = reader["cours2"].ToString();
                        string cours3 = reader["cours3"].ToString();
                        curatorCourses.Add(cours1);
                        curatorCourses.Add(cours2);
                        curatorCourses.Add(cours3);
                        var replyKeyboard = new ReplyKeyboardMarkup(new[]
                        {
                            new []
                            {
                                new KeyboardButton(cours1),
                            },
                            new []
                            {
                                new KeyboardButton(cours2),
                            },
                            new []
                            {
                                new KeyboardButton(cours3),
                                
                            },
                            new []
                            {
                                new KeyboardButton("Выход"),
                            }
                        });

                        // Отправляем сообщение с кнопками
                        await Bot.SendTextMessageAsync(message.Chat.Id, "Выберите курс:", replyMarkup: replyKeyboard);
                        permission = 10;
                    }
                }
                
                if (curatorCourses.Contains(message.Text) && permission == 10)
                {
                    SQLiteCommand cmd = new SQLiteCommand("UPDATE curators SET cours=@cours WHERE tg_id=@tg_id", DB);
                    cmd.Parameters.AddWithValue("@cours", message.Text);
                    cmd.Parameters.AddWithValue("@tg_id", message.From.Id);
                    cmd.ExecuteNonQuery();
                    await Bot.SendTextMessageAsync(message.From.Id, "Теперь вы взамодействуйте с курсом:" + message.Text, replyMarkup: curatorBtn);
                    permission = 0;
                }

                if (message.Text.StartsWith("Выбрать группу"))
                {
                    string[] parts = message.Text.Split('/');
                    var cours = parts[1];
                    LoadAdmins(message.From.Id.ToString());
                    if (message.From.Id.ToString() == adminId)
                    {
                        SQLiteCommand cmd = new SQLiteCommand("UPDATE admins SET c=@c WHERE tg_id=@tg_id", DB);
                        cmd.Parameters.AddWithValue("@c", cours);
                        cmd.Parameters.AddWithValue("@tg_id", message.From.Id);
                        cmd.ExecuteNonQuery();
                        await Bot.SendTextMessageAsync(message.From.Id, "Теперь вы взамодействуйте с курсом: " + cours, replyMarkup: curatorBtn);
                    }
                }

                if (message.Text.StartsWith("Прогул"))
                {

                    string[] parts = message.Text.Split('/');
                    studentName = parts[1];
                    studentDay = parts[2];
                    studentHours = parts[3];
                    Together();
                    LoadCurator(message.From.Id.ToString());
                    LoadStarosta(message.From.Id.ToString());
                    LoadAdmins(message.From.Id.ToString());
                    if (message.From.Id.ToString() == starostaId)
                    {
                        targetCours = starostaCours;
                        string table = $"{monthTable}{targetCours}";
                        AddStudentNullWithDay(table, DB, studentHours, studentName, studentDay);
                        await Bot.SendTextMessageAsync(message.From.Id, $"👨‍🎓Студент: {studentName} \n🕒Пропустил: {studentHours} часов\n🟢Число месяца:{studentDay}\n✅Успешно внесено в таблицу", replyMarkup: starostaBtn);
                    }
                    else if (message.From.Id.ToString() == curatorId)
                    {
                        targetCours = curatorCours;
                        string table = $"{monthTable}{targetCours}";
                        AddStudentNullWithDay(table, DB, studentHours, studentName, studentDay);
                        await Bot.SendTextMessageAsync(message.From.Id, $"👨‍🎓Студент: {studentName} \n🕒Пропустил: {studentHours} часов\n🟢Число месяца:{studentDay}\n✅Успешно внесено в таблицу", replyMarkup: curatorBtn);
                    }
                    else if (message.From.Id.ToString() == adminId)
                    {
                        targetCours = adminGroup;
                        string table = $"{monthTable}{targetCours}";
                        AddStudentNullWithDay(table, DB, studentHours, studentName, studentDay);
                        await Bot.SendTextMessageAsync(message.From.Id, $"👨‍🎓Студент: {studentName} \n🕒Пропустил: {studentHours} часов\n🟢Число месяца:{studentDay}\n✅Успешно внесено в таблицу", replyMarkup: curatorBtn);

                    }

                }
                if (message.Text.Contains("Перенести"))
                {
                    Together();
                    LoadCurator(message.From.Id.ToString());
                    string table = $"{monthTable}{curatorCours}";
                    SQLiteCommand command1 = new SQLiteCommand($"SELECT name FROM [{curatorCours}]", DB);
                    SQLiteDataReader reader = command1.ExecuteReader();

                    while (reader.Read())
                    {
                        if (reader.HasRows)
                        {
                            studentName = reader.GetString(0);
                            SQLiteCommand command2 = new SQLiteCommand($"INSERT INTO [{table}] (name) VALUES ('{studentName}')", DB);
                            command2.ExecuteNonQuery();
                            await Bot.SendTextMessageAsync(message.From.Id, $"✅Добавил: {studentName} в таблицу {table}", replyMarkup: curatorBtn);
                        }
                    }
                }

            }
            catch (Exception ex)
            {
                await Bot.SendTextMessageAsync(1251534440, "🛑У меня случилась следующая ошибка: \n" + ex.Message + "\n\nВ чате: " + e.Message.From.Id);
            }
        }
        
        #endregion
        
        #region Методы

        private static void CreateTableNone(string tableName, SQLiteConnection connection)
        {
            try
            {
                using (SQLiteCommand command = new SQLiteCommand(connection))
                {
                    command.CommandText = $"CREATE TABLE [{tableName}] (id INTEGER PRIMARY KEY, name TEXT, day1 INTEGER, day2 INTEGER, day3 INTEGER, day4 INTEGER, day5 INTEGER, day6 INTEGER, day7 INTEGER, day8 INTEGER, day9 INTEGER, day10 INTEGER, day11 INTEGER, day12 INTEGER, day13 INTEGER, day14 INTEGER, day15 INTEGER, day16 INTEGER, day17 INTEGER, day18 INTEGER, day19 INTEGER, day20 INTEGER, day21 INTEGER, day22 INTEGER, day23 INTEGER, day24 INTEGER, day25 INTEGER, day26 INTEGER, day27 INTEGER, day28 INTEGER, day29 INTEGER, day30 INTEGER, day31 INTEGER)";
                    command.ExecuteNonQuery();
                }
            }
            catch(Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }
        public static void Together()
        {
            string currentMonth = DateTime.Now.ToString("MMMM", new System.Globalization.CultureInfo("en-US"));
            monthTable = currentMonth.ToLower();
        }
        static void ExportToExcel(string excelFile, string tableName)
        {
            try
            {
                SQLiteCommand command = new SQLiteCommand($"SELECT * FROM [{tableName}]", DB);
                SQLiteDataReader rdr = command.ExecuteReader();
                var dataTable = new DataTable();
                dataTable.Load(rdr);
                using (var package = new ExcelPackage())
                {
                    var worksheet = package.Workbook.Worksheets.Add("Sheet1");
                    for (int j = 0; j < dataTable.Columns.Count; j++)
                    {
                        worksheet.Cells[1, j + 3].Value = (j + 1);
                    }

                    for (int i = 0; i < dataTable.Rows.Count; i++)
                    {
                        for (int j = 0; j < dataTable.Columns.Count; j++)
                        {
                            worksheet.Cells[i + 2, j + 1].Value = dataTable.Rows[i][j];
                        }
                    }

                    package.SaveAs(new System.IO.FileInfo(excelFile));
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }
        private static void CreateTableGroup(string tableName, SQLiteConnection connection)
        {
            using (SQLiteCommand command = new SQLiteCommand(connection))
            {
                command.CommandText = $"CREATE TABLE IF NOT EXISTS [{tableName}] (name TEXT)";
                command.ExecuteNonQuery();
            }
        }
        public static async void LoadStarosta(string tg_id)
        {
            try
            {
                SQLiteCommand command = new SQLiteCommand($"SELECT name, cours, tg_id FROM starosta WHERE tg_id='{tg_id}'", DB);
                SQLiteDataReader datareader = command.ExecuteReader();
                datareader.Read();
                starostaName = datareader.GetString(0);
                starostaCours = datareader.GetString(1);
                starostaId = datareader.GetString(2);
                await Console.Out.WriteLineAsync($"Выполняется старостой:\nИмя: {starostaName}\nTG_ID: {starostaId}\nГруппа: {starostaCours}\n\n");
                datareader.Close();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Староста с таким айди не найден либо в базе данных допущена ошибка>> {ex.Message}!\n");
            }
        }
        public static async void LoadCurator(string tg_id)
        {
            try
            {
                SQLiteCommand command1 = new SQLiteCommand($"SELECT name, cours, tg_id FROM curators WHERE tg_id='{tg_id}'", DB);
                SQLiteDataReader sqlite_datareader = command1.ExecuteReader();
                sqlite_datareader.Read();
                curatorName = sqlite_datareader.GetString(0);
                curatorCours = sqlite_datareader.GetString(1);
                curatorId = sqlite_datareader.GetString(2);
                sqlite_datareader.Close();
                await Console.Out.WriteLineAsync($"Выполняется Куратором:\nИмя: {curatorName}\nTG_ID: {curatorId}\nГруппа: {curatorCours}\n\n");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Куратор с таким айди не найден либо в базе данных допущена ошибка>> {ex.Message}!\n");
            }
        }

        public static async void LoadAdmins(string tg_id)
        {
            try
            {
                SQLiteCommand command1 = new SQLiteCommand($"SELECT name, c, tg_id FROM admins WHERE tg_id='{tg_id}'", DB);
                SQLiteDataReader sqlite_datareader = command1.ExecuteReader();
                sqlite_datareader.Read();
                adminName = sqlite_datareader["name"].ToString();
                adminGroup = sqlite_datareader["c"].ToString();
                adminId = sqlite_datareader["tg_id"].ToString();
                sqlite_datareader.Close();
                await Console.Out.WriteLineAsync($"Выполняется Администратором:\nИмя: {adminName}\nTG_ID: {adminId}\nГруппа: {adminGroup}\n\n");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Администратор с таким айди не найден либо в базе данных допущена ошибка>> {ex.Message}!\n");
            }
        }

        private static void AddStudent(string tableName, SQLiteConnection connection, string name)
        {
            using (SQLiteCommand command = new SQLiteCommand(connection))
            {
                command.CommandText = $"INSERT INTO [{tableName}] (name) VALUES (@name)";
                command.Parameters.AddWithValue("@name", name);
                command.ExecuteNonQuery();
            }
        }
        private static void AddStudentNull(string tableName, SQLiteConnection connection, string hours, string student)
        {
            int day = DateTime.Today.Day;
            string today = $"day{day}";
            using (SQLiteCommand command = new SQLiteCommand(connection))
            {
                command.CommandText = $"UPDATE [{tableName}] SET {today}='{hours}' WHERE name=@name";
                command.Parameters.AddWithValue("@name", student);
                command.Parameters.AddWithValue($"@{today}", hours);
                command.ExecuteNonQuery();
            }
        }

        public static void UpdatePerm(string tg_id, string table, string perm)
        {
            try
            {
                if (table == "Кураторы")
                {
                    SQLiteCommand command = new SQLiteCommand("UPDATE curators SET perm=@perm WHERE tg_id=@tg_id", DB);
                    command.Parameters.AddWithValue("@perm", perm);
                    command.Parameters.AddWithValue($"@tg_id", tg_id);
                    command.ExecuteNonQuery();
                }
                else if (table == "Старосты")
                {
                    SQLiteCommand command = new SQLiteCommand("UPDATE starosta SET perm=@perm WHERE tg_id=@tg_id", DB);
                    command.Parameters.AddWithValue("@perm", perm);
                    command.Parameters.AddWithValue($"@tg_id", tg_id);
                    command.ExecuteNonQuery();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Пользователь с таким айди не найден либо в базе данных допущена ошибка>> {ex.Message}!\n");
            }
        }

        private static void AddStudentNullWithDay(string tableName, SQLiteConnection connection, string hours, string student, string day)
        {
            string today = $"day{day}";
            SQLiteCommand add = new SQLiteCommand($"UPDATE [{tableName}] SET {today}=@hours WHERE name=@name", connection);
            add.Parameters.AddWithValue("@name", student);
            add.Parameters.AddWithValue("@hours", hours);
            add.ExecuteNonQuery();
        }
        #endregion
    }
}