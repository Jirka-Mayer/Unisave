using System;
using MySql.Data.MySqlClient;
using Unisave.Utils;

namespace Unisave.Editor.Tests.Database.Support.DatabaseProxy
{
    /// <summary>
    /// Contains helper methods for working with the MySql connection
    /// </summary>
    public static class MySqlDatabase
    {
        /// <summary>
        /// Connects to a MySQL database
        /// </summary>
        public static MySqlConnection OpenConnection(string connectionString)
        {
            var connection = new MySqlConnection(connectionString);
                    
            // connect
            connection.Open();
                    
            // set timezone to UTC
            using (var command = connection.CreateCommand())
            {
                command.CommandText = "SET time_zone = '+00:00';";
                command.ExecuteNonQuery();
            }

            return connection;
        }

        /// <summary>
        /// Sets up the testing database
        /// Returns execution ID for the proxy connection
        /// </summary>
        public static void PrepareDatabase(
            MySqlConnection connection,
            out string developerId, out string gameId, out string databaseId,
            out string executionId
        )
        {
            // clear first
            ClearDatabase(connection);
            
            // create developer
            developerId = Str.Random(16);
            using (var command = connection.CreateCommand())
            {
                command.CommandText = @"
                    INSERT INTO developers (id, email, password, full_name) 
                    VALUES (@id, @email, 'password', 'John Doe');
                ";
                command.Parameters.AddWithValue("id", developerId);
                command.Parameters.AddWithValue("email", developerId);
                command.ExecuteNonQuery();
            }
            
            // create game
            gameId = Str.Random(16);
            using (var command = connection.CreateCommand())
            {
                command.CommandText = @"
                    INSERT INTO games (
                        id, developer_id, title, token, maintenance_mode
                    ) 
                    VALUES (
                        @id, @developer_id, 'Johns game', 'token', FALSE
                    );
                ";
                command.Parameters.AddWithValue("id", gameId);
                command.Parameters.AddWithValue("developer_id", developerId);
                command.ExecuteNonQuery();
            }
            
            // create database
            databaseId = Str.Random(8);
            using (var command = connection.CreateCommand())
            {
                command.CommandText = @"
                    INSERT INTO `databases` (
                        id, game_id, title
                    ) 
                    VALUES (
                        @id, @game_id, 'SomeDatabase'
                    );
                ";
                command.Parameters.AddWithValue("id", databaseId);
                command.Parameters.AddWithValue("game_id", gameId);
                command.ExecuteNonQuery();
            }
            
            // script executions
            executionId = Str.Random(16);
            using (var command = connection.CreateCommand())
            {
                command.CommandText = @"
                    INSERT INTO script_executions (
                        id, database_id, created_at
                    ) 
                    VALUES (
                        @id, @database_id, @now
                    );
                ";
                command.Parameters.AddWithValue("id", executionId);
                command.Parameters.AddWithValue("database_id", databaseId);
                command.Parameters.AddWithValue("now", DateTime.UtcNow);
                command.ExecuteNonQuery();
            }
        }

        private static void ClearDatabase(MySqlConnection connection)
        {
            using (var command = connection.CreateCommand())
            {
                command.CommandText = @"
                    DELETE FROM script_executions;
                    DELETE FROM entities_players;
                    DELETE FROM entities;
                    DELETE FROM `databases`;
                    DELETE FROM games;
                    DELETE FROM developers;
                ";
                command.ExecuteNonQuery();
            }
        }
    }
}