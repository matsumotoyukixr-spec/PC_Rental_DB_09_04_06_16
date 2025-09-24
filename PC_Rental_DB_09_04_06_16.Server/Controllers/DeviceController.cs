using Microsoft.AspNetCore.Mvc;
using Npgsql;
using System.Data;
using System.Security.Cryptography;
using System.Text;
using System;
using System.Collections.Generic;

namespace PC_Rental_DB_09_04_06_16.Server.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class DeviceController : ControllerBase
    {
        private readonly NpgsqlConnection _conn;
        public DeviceController(NpgsqlConnection conn) => _conn = conn;

        // ---------- DTO ----------
        public class DeviceDto
        {
            public string AssetNo { get; set; } = string.Empty;
            public string Maker { get; set; } = string.Empty;
            public string? Os { get; set; }
            public string? MemoryGb { get; set; }
            public string? StorageGb { get; set; }
            public string? Gpu { get; set; }
            public string? Location { get; set; }
            public bool BrokenFlag { get; set; } = false;
            public DateTime? LeaseStart { get; set; }
            public DateTime? LeaseEnd { get; set; }
            public string? Remarks { get; set; }
            public DateTime? RegisterDate { get; set; }
            public DateTime? UpdateDate { get; set; }
            public bool DeleteFlag { get; set; } = false;
        }

        public class DeleteDto
        {
            public string AssetNo { get; set; } = string.Empty;
        }

        // ---------- Helper ----------
        // NpgsqlDataReaderから指定した列名のデータを安全に取得するヘルパー関数
        private static string S(NpgsqlDataReader rd, string col)
            => rd.IsDBNull(rd.GetOrdinal(col)) ? "" : rd.GetString(rd.GetOrdinal(col));
        private static int? I(NpgsqlDataReader rd, string col)
            => rd.IsDBNull(rd.GetOrdinal(col)) ? (int?)null : rd.GetInt32(rd.GetOrdinal(col));
        private static DateTime? D(NpgsqlDataReader rd, string col)
            => rd.IsDBNull(rd.GetOrdinal(col)) ? (DateTime?)null : rd.GetDateTime(rd.GetOrdinal(col));
        private static bool B(NpgsqlDataReader rd, string col)
            => !rd.IsDBNull(rd.GetOrdinal(col)) && rd.GetBoolean(rd.GetOrdinal(col));

        // ★ 重複しているため、このAddヘルパー関数は削除します。
        // private static void Add(NpgsqlCommand cmd, string name, object? value)
        //     => cmd.Parameters.AddWithValue(name, value ?? DBNull.Value);

        // ---------- 一覧取得 ----------
        [HttpGet("list")]
        public async Task<IActionResult> List([FromQuery] bool includeDeleted = false)
        {
            if (_conn.State != ConnectionState.Open) await _conn.OpenAsync();

            var sql = @"
SELECT asset_no, maker, os, memory_gb, storage_gb, gpu, location, broken_flag,
       lease_start, lease_end, remarks, register_date, update_date, delete_flag
  FROM mst_device " + (includeDeleted ? "" : "WHERE delete_flag = FALSE ") + @" 
 ORDER BY asset_no";

            using var cmd = new NpgsqlCommand(sql, _conn);
            using var rd = await cmd.ExecuteReaderAsync();

            var list = new List<object>();
            while (await rd.ReadAsync())
            {
                list.Add(new
                {
                    assetNo = rd.GetString(rd.GetOrdinal("asset_no")),
                    maker = rd.GetString(rd.GetOrdinal("maker")),
                    os = rd.IsDBNull(rd.GetOrdinal("os")) ? null : rd.GetString(rd.GetOrdinal("os")),
                    memoryGb = rd.IsDBNull(rd.GetOrdinal("memory_gb")) ? null : rd.GetString(rd.GetOrdinal("memory_gb")),
                    storageGb = rd.IsDBNull(rd.GetOrdinal("storage_gb")) ? null : rd.GetString(rd.GetOrdinal("storage_gb")),
                    gpu = rd.IsDBNull(rd.GetOrdinal("gpu")) ? null : rd.GetString(rd.GetOrdinal("gpu")),
                    location = rd.IsDBNull(rd.GetOrdinal("location")) ? null : rd.GetString(rd.GetOrdinal("location")),
                    brokenFlag = rd.GetBoolean(rd.GetOrdinal("broken_flag")),
                    leaseStart = rd.IsDBNull(rd.GetOrdinal("lease_start")) ? (DateTime?)null : rd.GetDateTime(rd.GetOrdinal("lease_start")),
                    leaseEnd = rd.IsDBNull(rd.GetOrdinal("lease_end")) ? (DateTime?)null : rd.GetDateTime(rd.GetOrdinal("lease_end")),
                    remarks = rd.IsDBNull(rd.GetOrdinal("remarks")) ? null : rd.GetString(rd.GetOrdinal("remarks")),
                    registerDate = rd.IsDBNull(rd.GetOrdinal("register_date")) ? (DateTime?)null : rd.GetDateTime(rd.GetOrdinal("register_date")),
                    updateDate = rd.IsDBNull(rd.GetOrdinal("update_date")) ? (DateTime?)null : rd.GetDateTime(rd.GetOrdinal("update_date")),
                    deleteFlag = rd.GetBoolean(rd.GetOrdinal("delete_flag"))
                });
            }
            return Ok(list);
        }

        // ---------- 新規作成 ----------
        [HttpPost("create")]
        public async Task<IActionResult> Create([FromBody] DeviceDto d)
        {
            var asset = (d.AssetNo ?? "").Trim();
            if (string.IsNullOrEmpty(asset))
                return BadRequest(new { message = "資産番号は必須です" });

            if (_conn.State != ConnectionState.Open) await _conn.OpenAsync();
            using var tx = await _conn.BeginTransactionAsync();

            // 重複チェック
            using (var chk = new NpgsqlCommand("SELECT 1 FROM mst_device WHERE asset_no=@a", _conn, (NpgsqlTransaction)tx))
            {
                chk.Parameters.AddWithValue("a", asset);
                if (await chk.ExecuteScalarAsync() != null)
                    return Conflict(new { message = "その資産番号は既に存在します" });
            }

            var sql = @"
INSERT INTO mst_device
(asset_no, maker, os, memory_gb, storage_gb, gpu, location, broken_flag, 
 lease_start, lease_end, remarks, register_date, update_date, delete_flag)
VALUES
(@asset_no, @maker, @os, @memory_gb, @storage_gb, @gpu, @location, @broken_flag,
 @lease_start, @lease_end, @remarks, CURRENT_DATE, CURRENT_DATE, FALSE)";
            using (var cmd = new NpgsqlCommand(sql, _conn, (NpgsqlTransaction)tx))
            {
                // ここで Add ヘルパー関数を呼び出しているが、削除した場合エラーになる。
                // 暫定的に AddWithValue を直接使うように修正
                cmd.Parameters.AddWithValue("asset_no", asset);
                cmd.Parameters.AddWithValue("maker", d.Maker);
                cmd.Parameters.AddWithValue("os", d.Os ?? (object)DBNull.Value);
                cmd.Parameters.AddWithValue("memory_gb", d.MemoryGb ?? (object)DBNull.Value);
                cmd.Parameters.AddWithValue("storage_gb", d.StorageGb ?? (object)DBNull.Value);
                cmd.Parameters.AddWithValue("gpu", d.Gpu ?? (object)DBNull.Value);
                cmd.Parameters.AddWithValue("location", d.Location ?? (object)DBNull.Value);
                cmd.Parameters.AddWithValue("broken_flag", d.BrokenFlag);
                cmd.Parameters.AddWithValue("lease_start", d.LeaseStart ?? (object)DBNull.Value);
                cmd.Parameters.AddWithValue("lease_end", d.LeaseEnd ?? (object)DBNull.Value);
                cmd.Parameters.AddWithValue("remarks", d.Remarks ?? (object)DBNull.Value);
                await cmd.ExecuteNonQueryAsync();
            }

            await tx.CommitAsync();
            return Ok(new { message = "登録しました" });
        }

        // ---------- 更新（資産番号で上書き） ----------
        [HttpPut("update")]
        public async Task<IActionResult> Update([FromBody] DeviceDto d)
        {
            var asset = (d.AssetNo ?? "").Trim();
            if (string.IsNullOrEmpty(asset))
                return BadRequest(new { message = "資産番号は必須です" });

            if (_conn.State != ConnectionState.Open) await _conn.OpenAsync();

            var sql = @"
UPDATE mst_device SET
  maker=@maker, os=@os, memory_gb=@memory_gb, storage_gb=@storage_gb,
  gpu=@gpu, location=@location, broken_flag=@broken_flag,
  lease_start=@lease_start, lease_end=@lease_end, remarks=@remarks,
  update_date=CURRENT_DATE
WHERE asset_no=@asset_no AND delete_flag=FALSE";
            using var cmd = new NpgsqlCommand(sql, _conn);
            cmd.Parameters.AddWithValue("asset_no", asset);
            cmd.Parameters.AddWithValue("maker", d.Maker);
            cmd.Parameters.AddWithValue("os", d.Os ?? (object)DBNull.Value);
            cmd.Parameters.AddWithValue("memory_gb", d.MemoryGb ?? (object)DBNull.Value);
            cmd.Parameters.AddWithValue("storage_gb", d.StorageGb ?? (object)DBNull.Value);
            cmd.Parameters.AddWithValue("gpu", d.Gpu ?? (object)DBNull.Value);
            cmd.Parameters.AddWithValue("location", d.Location ?? (object)DBNull.Value);
            cmd.Parameters.AddWithValue("broken_flag", d.BrokenFlag);
            cmd.Parameters.AddWithValue("lease_start", d.LeaseStart ?? (object)DBNull.Value);
            cmd.Parameters.AddWithValue("lease_end", d.LeaseEnd ?? (object)DBNull.Value);
            cmd.Parameters.AddWithValue("remarks", d.Remarks ?? (object)DBNull.Value);

            var n = await cmd.ExecuteNonQueryAsync();
            if (n == 0) return NotFound(new { message = "対象が見つかりません（削除済の可能性）" });
            return Ok(new { message = "更新しました" });
        }

        // ---------- 論理削除 ----------
        [HttpPost("delete")]
        public async Task<IActionResult> Delete([FromBody] DeleteDto req)
        {
            var asset = (req.AssetNo ?? "").Trim();
            if (string.IsNullOrEmpty(asset))
                return BadRequest(new { message = "assetNo is required" });

            if (_conn.State != ConnectionState.Open) await _conn.OpenAsync();

            var sql = @"UPDATE mst_device SET delete_flag=TRUE, update_date=CURRENT_DATE WHERE asset_no=@a AND delete_flag=FALSE";
            using var cmd = new NpgsqlCommand(sql, _conn);
            cmd.Parameters.AddWithValue("a", asset);
            var n = await cmd.ExecuteNonQueryAsync();
            if (n == 0) return NotFound(new { message = "対象が見つかりません（既に削除済）" });
            return Ok(new { message = "削除しました" });
        }

        // ---------- 詳細取得（資産番号） ----------
        [HttpGet("{assetNo}")]
        public async Task<IActionResult> GetByAsset(string assetNo)
        {
            if (string.IsNullOrWhiteSpace(assetNo))
                return BadRequest(new { message = "assetNo is required" });

            const string sql = @"
SELECT asset_no, maker, os, memory_gb, storage_gb, gpu, location,
       broken_flag, register_date, update_date, lease_start, lease_end, remarks
  FROM mst_device
 WHERE TRIM(asset_no) = TRIM(@asset)
   AND delete_flag = FALSE
 LIMIT 1;";

            if (_conn.State != ConnectionState.Open) await _conn.OpenAsync();
            using var cmd = new NpgsqlCommand(sql, _conn);
            cmd.Parameters.AddWithValue("asset", assetNo);

            using var r = await cmd.ExecuteReaderAsync(CommandBehavior.SingleRow);
            if (!await r.ReadAsync())
                return NotFound(new { message = $"機器が見つかりません（asset_no='{assetNo}'）" });

            return Ok(new
            {
                assetNo = r.GetString(r.GetOrdinal("asset_no")),
                maker = r.GetString(r.GetOrdinal("maker")),
                os = r.IsDBNull(r.GetOrdinal("os")) ? "" : r.GetString(r.GetOrdinal("os")),
                memoryGb = r.IsDBNull(r.GetOrdinal("memory_gb")) ? null : r.GetString(r.GetOrdinal("memory_gb")),
                storageGb = r.IsDBNull(r.GetOrdinal("storage_gb")) ? null : r.GetString(r.GetOrdinal("storage_gb")),
                gpu = r.IsDBNull(r.GetOrdinal("gpu")) ? "" : r.GetString(r.GetOrdinal("gpu")),
                location = r.IsDBNull(r.GetOrdinal("location")) ? "" : r.GetString(r.GetOrdinal("location")),
                brokenFlag = r.GetBoolean(r.GetOrdinal("broken_flag")),
                registerDate = r.IsDBNull(r.GetOrdinal("register_date")) ? (DateTime?)null : r.GetDateTime(r.GetOrdinal("register_date")),
                updateDate = r.IsDBNull(r.GetOrdinal("update_date")) ? (DateTime?)null : r.GetDateTime(r.GetOrdinal("update_date")),
                leaseStart = r.IsDBNull(r.GetOrdinal("lease_start")) ? (DateTime?)null : r.GetDateTime(r.GetOrdinal("lease_start")),
                leaseEnd = r.IsDBNull(r.GetOrdinal("lease_end")) ? (DateTime?)null : r.GetDateTime(r.GetOrdinal("lease_end")),
                remarks = r.IsDBNull(r.GetOrdinal("remarks")) ? "" : r.GetString(r.GetOrdinal("remarks"))
            });
        }
    }
}