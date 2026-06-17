using Microsoft.Data.Sqlite;
using System.Collections.Generic;
using System;

namespace botAdmin
{
    public static class Database
    {
        private static string connectionString = "Data Source=teabot.db";

        public static void Initialize()
        {
            using (var connection = new SqliteConnection(connectionString))
            {
                connection.Open();
                string sql = @"
                    CREATE TABLE IF NOT EXISTS Users (
                        VkUserId INTEGER PRIMARY KEY,
                        Language TEXT,
                        Level TEXT,
                        RequestText TEXT
                    );
                    CREATE TABLE IF NOT EXISTS ManagerRequests (
                        Id INTEGER PRIMARY KEY AUTOINCREMENT,
                        VkUserId INTEGER,
                        RequestText TEXT,
                        Status TEXT DEFAULT 'new',
                        CreatedAt DATETIME DEFAULT CURRENT_TIMESTAMP
                    )";
                using (var cmd = new SqliteCommand(sql, connection))
                    cmd.ExecuteNonQuery();
            }
        }

        public static List<User> GetUsers()
        {
            var users = new List<User>();
            using (var connection = new SqliteConnection(connectionString))
            {
                connection.Open();
                var cmd = new SqliteCommand("SELECT VkUserId, Language, Level, RequestText FROM Users", connection);
                var reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    users.Add(new User
                    {
                        VkUserId = reader.GetInt64(0),
                        Language = reader.IsDBNull(1) ? "" : reader.GetString(1),
                        Level = reader.IsDBNull(2) ? "" : reader.GetString(2),
                        Request = reader.IsDBNull(3) ? "" : reader.GetString(3)
                    });
                }
            }
            return users;
        }
    }

    public class User
    {
        public long VkUserId { get; set; }
        public string Language { get; set; } = "";
        public string Level { get; set; } = "";
        public string Request { get; set; } = "";
    }
}