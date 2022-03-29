using Dapper;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;

namespace BaseSource.AppCode.Service
{
    public class Dapper : IDapper
    {
        private readonly IConfiguration _config;
        private string Connectionstring = "DefaultConnection";

        public Dapper(IConfiguration config) => _config = config;
        public void Dispose()
        {

        }

        public async Task<int> Execute(string sp, DynamicParameters parms, CommandType commandType = CommandType.StoredProcedure)
        {
            throw new NotImplementedException();
        }

        public async Task<T> Get<T>(string sp, DynamicParameters parms, CommandType commandType = CommandType.Text)
        {
            using (IDbConnection db = new SqlConnection(_config.GetConnectionString(Connectionstring)))
            {
                var result = await db.QueryAsync<T>(sp, parms, commandType: commandType);
                return result.FirstOrDefault();
            }
        }

        public async Task<List<T>> GetAll<T>(string sp, DynamicParameters parms, CommandType commandType = CommandType.StoredProcedure)
        {
            using (IDbConnection db = new SqlConnection(_config.GetConnectionString(Connectionstring)))
            {
                var result = await db.QueryAsync<T>(sp, parms, commandType: commandType);
                return result.ToList();
            }

        }

        

        public async Task<T> Insert<T>(string sp, DynamicParameters parms, CommandType commandType = CommandType.StoredProcedure)
        {
            T response;
            using IDbConnection db = new SqlConnection(_config.GetConnectionString(Connectionstring));
            try
            {
                if (db.State == ConnectionState.Closed)
                    db.Open();

                using var tran = db.BeginTransaction();
                try
                {
                    var result = await db.QueryAsync<T>(sp, parms, commandType: commandType, transaction: tran);
                    response = result.FirstOrDefault();
                    tran.Commit();
                }
                catch (Exception ex)
                {
                    tran.Rollback();
                    throw ex;
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                if (db.State == ConnectionState.Open)
                    db.Close();
            }
            return response;
        }

        public async Task<T> Update<T>(string sp, DynamicParameters parms, CommandType commandType = CommandType.StoredProcedure)
        {
            T response;
            using IDbConnection db = new SqlConnection(_config.GetConnectionString(Connectionstring));
            try
            {
                if (db.State == ConnectionState.Closed)
                    db.Open();

                using var tran = db.BeginTransaction();
                try
                {
                    var result = await db.QueryAsync<T>(sp, parms, commandType: commandType, transaction: tran);
                    response = result.FirstOrDefault();
                    tran.Commit();
                }
                catch (Exception ex)
                {
                    tran.Rollback();
                    throw ex;
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                if (db.State == ConnectionState.Open)
                    db.Close();
            }

            return response;
        }

        public DbConnection GetDbconnection()
        {
            return new SqlConnection(_config.GetConnectionString(Connectionstring));
        }
    }
}
