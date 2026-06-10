using Microsoft.Data.Sqlite;
using System;

namespace TeaBotAdmin
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

        public static void GetOrCreateUser(long vkUserId)
        {
            using (var connection = new SqliteConnection(connectionString))
            {
                connection.Open();
                string sql = "INSERT OR IGNORE INTO Users (VkUserId) VALUES (@id)";
                using (var cmd = new SqliteCommand(sql, connection))
                {
                    cmd.Parameters.AddWithValue("@id", vkUserId);
                    cmd.ExecuteNonQuery();
                }
            }
        }

        public static void SaveLanguage(long vkUserId, string language)
        {
            using (var connection = new SqliteConnection(connectionString))
            {
                connection.Open();
                string sql = "UPDATE Users SET Language = @lang WHERE VkUserId = @id";
                using (var cmd = new SqliteCommand(sql, connection))
                {
                    cmd.Parameters.AddWithValue("@id", vkUserId);
                    cmd.Parameters.AddWithValue("@lang", language);
                    cmd.ExecuteNonQuery();
                }
            }
        }

        public static void SaveLevel(long vkUserId, string level)
        {
            using (var connection = new SqliteConnection(connectionString))
            {
                connection.Open();
                string sql = "UPDATE Users SET Level = @lvl WHERE VkUserId = @id";
                using (var cmd = new SqliteCommand(sql, connection))
                {
                    cmd.Parameters.AddWithValue("@id", vkUserId);
                    cmd.Parameters.AddWithValue("@lvl", level);
                    cmd.ExecuteNonQuery();
                }
            }
        }

        public static void SaveRequest(long vkUserId, string request)
        {
            using (var connection = new SqliteConnection(connectionString))
            {
                connection.Open();
                string sql = "UPDATE Users SET RequestText = @req WHERE VkUserId = @id";
                using (var cmd = new SqliteCommand(sql, connection))
                {
                    cmd.Parameters.AddWithValue("@id", vkUserId);
                    cmd.Parameters.AddWithValue("@req", request);
                    cmd.ExecuteNonQuery();
                }
            }
        }
    }
}