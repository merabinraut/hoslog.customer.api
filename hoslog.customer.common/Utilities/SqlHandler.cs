using System.Data;
using Dapper;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;

namespace hoslog.customer.common.Utilities
{
    public class SqlHandler : IDisposable
    {
        private readonly IConfiguration _configuration;
        private IDbConnection _connection;
        private readonly string _connectionName;
        private IDbTransaction _transaction;
        private const int _commandTimeOut = 120;
        private bool _disposed;

        public SqlHandler(IConfiguration? configuration,string connectionName = "DefaultConnection")
        {
            _connectionName = connectionName;
            _configuration = configuration;
        }

        private string GetConnectionString()
        {
            string connectionString = _configuration.GetConnectionString(_connectionName);
            if (string.IsNullOrEmpty(connectionString))
                throw new ArgumentException("Connection string is not configured.", nameof(connectionString));

            return connectionString;
        }

        public void OpenConnection()
        {
            if (_connection == null || _connection.State != ConnectionState.Open)
            {
                _connection = new SqlConnection(GetConnectionString());
                _connection.Open();
            }
        }

        public void BeginTransaction()
        {
            OpenConnection();
            if (_transaction == null)
                _transaction = _connection.BeginTransaction();
        }

        public void CommitTransaction()
        {
            if (_transaction == null)
                throw new InvalidOperationException("Transaction has not been started.");

            _transaction?.Commit();
            CloseConnection();
        }

        public void RollbackTransaction()
        {
            if (_transaction == null)
                throw new InvalidOperationException("Transaction has not been started.");

            _transaction?.Rollback();
            CloseConnection();
        }

        private void CloseConnection()
        {
            _transaction = null;
            if (_connection?.State == ConnectionState.Open)
                _connection?.Close();
        }

        public void Dispose()
        {
            if (_disposed) return;

            if (_transaction != null)
                RollbackTransaction();

            _connection?.Dispose();
            _disposed = true;
        }

        public async Task<T> ExecuteAsync<T>(string procedure, DynamicParameters parameters)
        {
            OpenConnection(); // Ensure connection is open

            try
            {
                var response = await _connection.QueryAsync<T>(
                    procedure, parameters,
                    commandType: CommandType.StoredProcedure,
                    commandTimeout: _commandTimeOut
                );

                return response.SingleOrDefault();
            }
            catch (Exception ex)
            {
                // throw new DataException("An error occurred while executing the transaction.", ex);
                throw new DataException($"An error occurred while executing the transaction: {ex.Message}", ex);
            }
        }

        public async Task<List<T>> ExecuteListAsync<T>(string procedure, DynamicParameters parameters)
        {
            OpenConnection(); // Ensure connection is open

            try
            {
                var response = await _connection.QueryAsync<T>(
                    procedure,
                    parameters,
                    commandType: CommandType.StoredProcedure,
                    commandTimeout: _commandTimeOut
                );

                return response.ToList();
            }
            catch (Exception ex)
            {
                // throw new DataException("An error occurred while executing the transaction.", ex);
                throw new DataException($"An error occurred while executing the transaction: {ex.Message}", ex);
            }
        }

        public async Task<T> ExecuteWithTransactionAsync<T>(string procedure, DynamicParameters parameters)
        {
            OpenConnection(); // Ensure connection is open

            try
            {
                var response = await _connection.QueryAsync<T>(
                    procedure, parameters,
                    commandType: CommandType.StoredProcedure,
                    commandTimeout: _commandTimeOut,
                    transaction: _transaction
                );

                return response.SingleOrDefault();
            }
            catch (Exception ex)
            {
                // throw new DataException("An error occurred while executing the transaction.", ex);
                throw new DataException($"An error occurred while executing the transaction: {ex.Message}", ex);
            }
        }

        public async Task<List<T>> ExecuteListWithTransactionAsync<T>(string procedure, DynamicParameters parameters)
        {
            OpenConnection(); // Ensure connection is open

            try
            {
                var response = await _connection.QueryAsync<T>(
                    procedure,
                    parameters,
                    commandType: CommandType.StoredProcedure,
                    commandTimeout: _commandTimeOut,
                    transaction: _transaction
                );

                return response.ToList();
            }
            catch (Exception ex)
            {
                // throw new DataException("An error occurred while executing the transaction.", ex);
                throw new DataException($"An error occurred while executing the transaction: {ex.Message}", ex);
            }
        }
    }
}