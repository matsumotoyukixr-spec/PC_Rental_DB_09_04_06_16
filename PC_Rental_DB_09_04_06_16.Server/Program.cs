using Npgsql;
using System.Text.Json;
using System.Text.Encodings.Web;
using System.Text.Unicode;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        // 日本語（UTF-8）を正しく処理するための設定
        options.JsonSerializerOptions.Encoder = JavaScriptEncoder.Create(UnicodeRanges.All);
        options.JsonSerializerOptions.PropertyNameCaseInsensitive = true;
    });

// appsettings.jsonなどから接続文字列を取得する
var connString = builder.Configuration.GetConnectionString("DefaultConnection");

try
{
    using var conn = new Npgsql.NpgsqlConnection(connString);
    conn.Open();
    Console.WriteLine("データベースに接続できました！");
}
catch (Exception ex)
{
    Console.WriteLine($"データベースへの接続に失敗しました: {ex.Message}");
}

builder.Services.AddScoped<NpgsqlConnection>(_ => new NpgsqlConnection(connString));

/*
// PostgreSQL接続サービスを追加
// appsettings.jsonなどから接続文字列を取得する方がより良い方法です
string connString = "Host=localhost;Database=PC_Rental_DB;Username=postgres;Password=Yu20010809";

try
{
    using var conn = new Npgsql.NpgsqlConnection(connString);
    conn.Open();
    Console.WriteLine("データベースに接続できました！");
}
catch (Exception ex)
{
    Console.WriteLine($"データベースへの接続に失敗しました: {ex.Message}");
}

builder.Services.AddScoped<NpgsqlConnection>(_ => new NpgsqlConnection(connString));

*/


var app = builder.Build();

app.UseDefaultFiles();
app.UseStaticFiles();

// Swaggerを完全に無効化（開発環境でも起動しない）
// Configure the HTTP request pipeline.
// if (app.Environment.IsDevelopment())
// {
//     // Swaggerを起動しない
// }

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();
app.MapFallbackToFile("/index.html");

app.Run();



/*
var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

app.UseDefaultFiles();
app.UseStaticFiles();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.MapFallbackToFile("/index.html");

app.Run();
*/