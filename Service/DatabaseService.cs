using Dapper;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;

public class DatabaseService
{
    private readonly string _connectionString;

    public DatabaseService(IConfiguration configuration)
    {
        _connectionString = configuration.GetConnectionString("DefaultConnection");
    }

    public async Task<IEnumerable<dynamic>> GetApprovedCustomersTodayAsync()
    {
        using (IDbConnection db = new SqlConnection(_connectionString))
        {
            string query = "EXEC sp_GetApprovedCustomersTodayExportWorker";
            return await db.QueryAsync<dynamic>(query);
        }

    }
    public async Task<List<string>> GetEmailsForExportWorkerAsync()
    {
        using (IDbConnection db = new SqlConnection(_connectionString))
        {
            string query = "EXEC sp_GetEmailForExportWorker";
            var result = await db.QueryAsync<dynamic>(query);

            // ดึงเฉพาะ email ออกมาเป็น List<string>
            List<string> emailList = new List<string>();
            foreach (var row in result)
            {
                emailList.Add(row.email.ToString());
            }

            return emailList;
        }
    }
}
