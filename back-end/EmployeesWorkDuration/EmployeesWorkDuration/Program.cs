using EmployeesWorkDuration.Services;
using EmployeesWorkDuration.Services.Interfaces;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddScoped<IStreamReaderService, StreamReaderService>();
builder.Services.AddScoped<IEmployeeDataAnalyser, CSVEmployeeDataAnalyser>();


// Add services to the container.
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowReactApp", policy =>
    {
        policy
            .WithOrigins("http://localhost:5173") // React dev server
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials();
    });
});

builder.Services.AddControllers();

var app = builder.Build();

// Configure the HTTP request pipeline.

app.UseAuthorization();

app.UseRouting();
app.UseCors("AllowReactApp");
app.MapControllers();

app.Run();
