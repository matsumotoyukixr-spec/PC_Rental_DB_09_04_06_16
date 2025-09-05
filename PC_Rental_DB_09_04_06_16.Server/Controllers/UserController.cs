using Microsoft.AspNetCore.Mvc;
using Npgsql;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;

namespace PC_Rental_DB_09_04_06_16.Server.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class UserController : ControllerBase
    {
        private readonly NpgsqlConnection _connection;

        public UserController(NpgsqlConnection connection)
        {
            _connection = connection;
        }

        // ユーザー情報を格納するためのクラス（モデル）
        public class UserInfo
        {
            public string EmployeeNo { get; set; }
            public string Name { get; set; }
            public string NameKana { get; set; }
            public string Department { get; set; }
            public string TelNo { get; set; }
            public string MailAddress { get; set; }
            public int Age { get; set; }
            public string Position { get; set; }
            public string AccountLevel { get; set; }
            public bool DeleteFlag { get; set; }
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<UserInfo>>> Get()
        {
            var users = new List<UserInfo>();

            try
            {
                if (_connection.State != ConnectionState.Open)
                {
                    await _connection.OpenAsync();
                }

                // MST_USERテーブルからデータを取得するSQL
                const string sql = @"
                    SELECT 
                        employee_no, 
                        name, 
                        name_kana, 
                        department, 
                        tel_no, 
                        mail_address, 
                        age, 
                        position, 
                        account_level,
                        delete_flag
                    FROM mst_user
                    WHERE delete_flag = FALSE
                    ORDER BY employee_no";

                using var cmd = new NpgsqlCommand(sql, _connection);
                using var reader = await cmd.ExecuteReaderAsync();

                while (await reader.ReadAsync())
                {
                    var user = new UserInfo
                    {
                        EmployeeNo = reader.GetString(0),
                        Name = reader.GetString(1),
                        NameKana = reader.GetString(2),
                        Department = reader.GetString(3),
                        TelNo = reader.GetString(4),
                        MailAddress = reader.GetString(5),
                        Age = reader.GetInt32(6),
                        Position = reader.GetString(7),
                        AccountLevel = reader.GetString(8),
                        DeleteFlag = reader.GetBoolean(9)
                    };
                    users.Add(user);
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = $"データの取得中にエラーが発生しました: {ex.Message}" });
            }
            finally
            {
                if (_connection.State == ConnectionState.Open)
                {
                    await _connection.CloseAsync();
                }
            }

            return Ok(users);
        }
    }
}