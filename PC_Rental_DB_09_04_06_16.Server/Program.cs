using Npgsql;
using System.Text.Json;
using System.Text.Encodings.Web;
using System.Text.Unicode;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        // ���{��iUTF-8�j�𐳂����������邽�߂̐ݒ�
        options.JsonSerializerOptions.Encoder = JavaScriptEncoder.Create(UnicodeRanges.All);
        options.JsonSerializerOptions.PropertyNameCaseInsensitive = true;
    });

// appsettings.json�Ȃǂ���ڑ���������擾����
var connString = builder.Configuration.GetConnectionString("DefaultConnection");

try
{
    using var conn = new Npgsql.NpgsqlConnection(connString);
    conn.Open();
    Console.WriteLine("�f�[�^�x�[�X�ɐڑ��ł��܂����I");
}
catch (Exception ex)
{
    Console.WriteLine($"�f�[�^�x�[�X�ւ̐ڑ��Ɏ��s���܂���: {ex.Message}");
}

builder.Services.AddScoped<NpgsqlConnection>(_ => new NpgsqlConnection(connString));

/*
// PostgreSQL�ڑ��T�[�r�X��ǉ�
// appsettings.json�Ȃǂ���ڑ���������擾����������ǂ����@�ł�
string connString = "Host=localhost;Database=PC_Rental_DB;Username=postgres;Password=Yu20010809";

try
{
    using var conn = new Npgsql.NpgsqlConnection(connString);
    conn.Open();
    Console.WriteLine("�f�[�^�x�[�X�ɐڑ��ł��܂����I");
}
catch (Exception ex)
{
    Console.WriteLine($"�f�[�^�x�[�X�ւ̐ڑ��Ɏ��s���܂���: {ex.Message}");
}

builder.Services.AddScoped<NpgsqlConnection>(_ => new NpgsqlConnection(connString));

*/

var app = builder.Build();

app.UseDefaultFiles();
app.UseStaticFiles();

// Swagger�����S�ɖ������i�J�����ł��N�����Ȃ��j
// Configure the HTTP request pipeline.
// if (app.Environment.IsDevelopment())
// {
//     // Swagger���N�����Ȃ�
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