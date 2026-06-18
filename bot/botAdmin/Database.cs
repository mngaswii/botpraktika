using Microsoft.Data.Sqlite;
using System;
using System.Collections.Generic;

namespace TeaBotSimple
{
    public static class Database
    {
        private static string connectionString = "Data Source=C:\\TEA_Data\\teabot.db";

        public static void Initialize()
        {
            System.IO.Directory.CreateDirectory(@"C:\TEA_Data\");

            using (var connection = new SqliteConnection(connectionString))
            {
                connection.Open();
                string sql = @"
                    CREATE TABLE IF NOT EXISTS Users (
                        VkUserId INTEGER PRIMARY KEY,
                        Language TEXT,
                        Level TEXT,
                        RequestText TEXT,
                        Platform TEXT DEFAULT 'VK'
                    );
                    CREATE TABLE IF NOT EXISTS ManagerRequests (
                        Id INTEGER PRIMARY KEY AUTOINCREMENT,
                        VkUserId INTEGER,
                        RequestText TEXT,
                        Status TEXT DEFAULT 'new',
                        CreatedAt DATETIME DEFAULT CURRENT_TIMESTAMP,
                        Platform TEXT DEFAULT 'VK'
                    )";
                using (var cmd = new SqliteCommand(sql, connection))
                    cmd.ExecuteNonQuery();
            }
        }

        public static void GetOrCreateUser(long vkUserId, string platform = "VK")
        {
            using (var connection = new SqliteConnection(connectionString))
            {
                connection.Open();
                string sql = "INSERT OR IGNORE INTO Users (VkUserId, Platform) VALUES (@id, @platform)";
                using (var cmd = new SqliteCommand(sql, connection))
                {
                    cmd.Parameters.AddWithValue("@id", vkUserId);
                    cmd.Parameters.AddWithValue("@platform", platform);
                    cmd.ExecuteNonQuery();
                }
            }
        }

        public static void SaveLanguage(long vkUserId, string language, string platform = "VK")
        {
            using (var connection = new SqliteConnection(connectionString))
            {
                connection.Open();
                string sql = "UPDATE Users SET Language = @lang, Platform = @platform WHERE VkUserId = @id";
                using (var cmd = new SqliteCommand(sql, connection))
                {
                    cmd.Parameters.AddWithValue("@id", vkUserId);
                    cmd.Parameters.AddWithValue("@lang", language);
                    cmd.Parameters.AddWithValue("@platform", platform);
                    cmd.ExecuteNonQuery();
                }
            }
        }

        public static void SaveLevel(long vkUserId, string level, string platform = "VK")
        {
            using (var connection = new SqliteConnection(connectionString))
            {
                connection.Open();
                string sql = "UPDATE Users SET Level = @lvl, Platform = @platform WHERE VkUserId = @id";
                using (var cmd = new SqliteCommand(sql, connection))
                {
                    cmd.Parameters.AddWithValue("@id", vkUserId);
                    cmd.Parameters.AddWithValue("@lvl", level);
                    cmd.Parameters.AddWithValue("@platform", platform);
                    cmd.ExecuteNonQuery();
                }
            }
        }

        public static void AddManagerRequest(long vkUserId, string requestText, string platform = "VK")
        {
            using (var connection = new SqliteConnection(connectionString))
            {
                connection.Open();
                string insert = "INSERT INTO ManagerRequests (VkUserId, RequestText, Platform) VALUES (@id, @text, @platform)";
                using (var cmd = new SqliteCommand(insert, connection))
                {
                    cmd.Parameters.AddWithValue("@id", vkUserId);
                    cmd.Parameters.AddWithValue("@text", requestText);
                    cmd.Parameters.AddWithValue("@platform", platform);
                    cmd.ExecuteNonQuery();
                }
            }
        }

        public static List<User> GetUsers(string platform = "")
        {
            var users = new List<User>();
            using (var connection = new SqliteConnection(connectionString))
            {
                connection.Open();
                string sql = "SELECT VkUserId, Language, Level, RequestText, Platform FROM Users";
                if (!string.IsNullOrEmpty(platform))
                    sql += $" WHERE Platform = '{platform}'";

                using (var cmd = new SqliteCommand(sql, connection))
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        users.Add(new User
                        {
                            VkUserId = reader.GetInt64(0),
                            Language = reader.IsDBNull(1) ? "" : reader.GetString(1),
                            Level = reader.IsDBNull(2) ? "" : reader.GetString(2),
                            Request = reader.IsDBNull(3) ? "" : reader.GetString(3),
                            Platform = reader.IsDBNull(4) ? "VK" : reader.GetString(4)
                        });
                    }
                }
            }
            return users;
        }

        public static List<ManagerRequest> GetRequests(string platform = "")
        {
            var requests = new List<ManagerRequest>();
            using (var connection = new SqliteConnection(connectionString))
            {
                connection.Open();
                string sql = "SELECT Id, VkUserId, RequestText, Status, CreatedAt, Platform FROM ManagerRequests ORDER BY CreatedAt DESC";
                if (!string.IsNullOrEmpty(platform))
                    sql = sql.Replace("ORDER BY", $"WHERE Platform = '{platform}' ORDER BY");

                using (var cmd = new SqliteCommand(sql, connection))
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        requests.Add(new ManagerRequest
                        {
                            Id = reader.GetInt64(0),
                            VkUserId = reader.GetInt64(1),
                            RequestText = reader.GetString(2),
                            Status = reader.GetString(3),
                            CreatedAt = reader.GetDateTime(4),
                            Platform = reader.IsDBNull(5) ? "VK" : reader.GetString(5)
                        });
                    }
                }
            }
            return requests;
        }
    }

    public class User
    {
        public long VkUserId { get; set; }
        public string Language { get; set; } = "";
        public string Level { get; set; } = "";
        public string Request { get; set; } = "";
        public string Platform { get; set; } = "VK";
    }

    public class ManagerRequest
    {
        public long Id { get; set; }
        public long VkUserId { get; set; }
        public string RequestText { get; set; } = "";
        public string Status { get; set; } = "new";
        public DateTime CreatedAt { get; set; }
        public string Platform { get; set; } = "VK";
    }
}