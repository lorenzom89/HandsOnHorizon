using BaseLibrary.Entities;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Server.Controllers;
using ServerLibrary.Data;
using ServerLibrary.Helpers;
using ServerLibrary.Repositories.Contracts;
using ServerLibrary.Repositories.Implementations;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.Configure<JwtSection>(builder.Configuration.GetSection("JwtSection"));
var jwtSection = builder.Configuration.GetSection(nameof(JwtSection)).Get<JwtSection>();
// Starting database
builder.Services.AddDbContext<AppDbContext>(options =>
{
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection") ??
        throw new InvalidOperationException(" Sorry, your connection is not found"));
});


builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
}).AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateIssuerSigningKey = true,
        ValidateLifetime = true,
        ValidIssuer = jwtSection!.Issuer,
        ValidAudience = jwtSection!.Audience,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSection.Key!))
    };
});


builder.Services.AddScoped<IUserAccount, UserAccountRepository>();

builder.Services.AddScoped<IGenericRepositoryInterface<Airport>, AirportRepository>();
builder.Services.AddScoped<IGenericRepositoryInterface<Baggage>, BaggageRepository>();
builder.Services.AddScoped<IGenericRepositoryInterface<City>, CityRepository>();
builder.Services.AddScoped<IGenericRepositoryInterface<FlightClass>, FlightClassRepository>();
builder.Services.AddScoped<IGenericRepositoryInterface<Flight>, FlightRepository>();
builder.Services.AddScoped<IGenericRepositoryInterface<Passenger>, PassengerRepository>();
builder.Services.AddScoped<IGenericRepositoryInterface<Ticket>, TicketRepository>();

builder.Services.AddCors(options =>
    {
        options.AddPolicy("AllowBlazorWasm",
            builder => builder
            .WithOrigins("https://localhost:7265", "http://localhost:5262")
                .AllowAnyMethod()
                .AllowAnyHeader()
                .AllowCredentials());
    });

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseCors("AllowBlazorWasm");

app.UseAuthentication();

app.UseAuthorization();

app.MapControllers();

app.Run();
