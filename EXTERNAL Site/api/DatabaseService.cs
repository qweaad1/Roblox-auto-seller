using Dapper;
using MySql.Data.MySqlClient;
using System.Data;

namespace qweaadAPI
{
    public static class DatabaseService
    {
        private static readonly SemaphoreSlim _connectionPool = new(10, 50);

        public static async Task<T?> QueryFirstOrDefaultAsync<T>(string sql, object? parameters = null, CommandType commandType = CommandType.Text)
        {
            await _connectionPool.WaitAsync();
            try
            {
                using var connection = new MySqlConnection(GlobalData.connectionString);
                await connection.OpenAsync();
                return await connection.QueryFirstOrDefaultAsync<T>(sql, parameters, commandType: commandType);
            }
            finally
            {
                _connectionPool.Release();
            }
        }

        public static async Task<IEnumerable<T>> QueryAsync<T>(string sql, object? parameters = null, CommandType commandType = CommandType.Text)
        {
            await _connectionPool.WaitAsync();
            try
            {
                using var connection = new MySqlConnection(GlobalData.connectionString);
                await connection.OpenAsync();
                return await connection.QueryAsync<T>(sql, parameters, commandType: commandType);
            }
            finally
            {
                _connectionPool.Release();
            }
        }

        public static async Task<int> ExecuteAsync(string sql, object? parameters = null, CommandType commandType = CommandType.Text)
        {
            await _connectionPool.WaitAsync();
            try
            {
                using var connection = new MySqlConnection(GlobalData.connectionString);
                await connection.OpenAsync();
                return await connection.ExecuteAsync(sql, parameters, commandType: commandType);
            }
            finally
            {
                _connectionPool.Release();
            }
        }

       
        public static T? QueryFirstOrDefaultSync<T>(string sql, object? parameters = null)
        {
            using var connection = new MySqlConnection(GlobalData.connectionString);
            connection.Open();
            return connection.QueryFirstOrDefault<T>(sql, parameters);
        }

        public static IEnumerable<T> QuerySync<T>(string sql, object? parameters = null)
        {
            using var connection = new MySqlConnection(GlobalData.connectionString);
            connection.Open();
            return connection.Query<T>(sql, parameters);
        }

        public static int ExecuteSync(string sql, object? parameters = null)
        {
            using var connection = new MySqlConnection(GlobalData.connectionString);
            connection.Open();
            return connection.Execute(sql, parameters);
        }
    }
}