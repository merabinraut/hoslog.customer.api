using Dapper;
using Microsoft.Extensions.Configuration;
using System.Data;
using System.Data.SqlClient;

namespace hoslog.customer.domain
{
    public class RepositoryDao
    {
        private readonly IConfiguration _config;
        private readonly string? _connectionName;
        private const int _commandTimeOut = 120;
        public RepositoryDao(IConfiguration config,string? connectionName = "DefaultConnection")
        {
            _connectionName = connectionName;
            _config = config;
        }

        private string GetConnectionString()
        {
            string connectionString = _config.GetConnectionString(_connectionName);
            if (!string.IsNullOrEmpty(connectionString))
                return connectionString;
            throw new ArgumentException(nameof(connectionString));
        }

        public async Task<T> ExecuteAsync<T>(string proc, DynamicParameters param)
        {
            try
            {
                using IDbConnection connection = new SqlConnection(GetConnectionString());
                var response = await connection.QueryAsync<T>(proc, param, commandType: CommandType.StoredProcedure, commandTimeout: _commandTimeOut);
                return response.SingleOrDefault();
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<List<T>> ExecuteAsyncList<T>(string proc, DynamicParameters param)
        {
            try
            {
                using IDbConnection connection = new SqlConnection(GetConnectionString());
                var response = await connection.QueryAsync<T>(proc, param, commandType: CommandType.StoredProcedure, commandTimeout: _commandTimeOut);
                return response.ToList();
            }
            catch (Exception)
            {
                throw;
            }
        }
    }
}
