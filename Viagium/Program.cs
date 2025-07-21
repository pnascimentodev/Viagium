using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Viagium.Configurations;
using Viagium.Data;
using Viagium.EntitiesDTO;
using Viagium.Repository;
using Viagium.Services;
using Viagium.Services.Auth;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
//builder.Services.AddSwaggerGen();

// Habilitando o Recebimento do Token de autenticação no Swagger 
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "Viagium", Version = "v1" });
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Insira o token JWT no formato: Bearer {seu token}"
    });
    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            new string[] {}
        }
    });
});

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Registra os repositórios
builder.Services.AddScoped<ITravelPackageRepository, TravelPackageRepository>();

// Registra o UnitOfWork e o serviço TravelPackageService
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<TravelPackageService>();
builder.Services.AddScoped<UserService>();

// Configuração do AuthService
builder.Services.Configure<JwtSettings>(builder.Configuration.GetSection("Jwt"));
builder.Services.AddScoped<IAuthService, AuthService>();

//Configura a AutoMapper para mapear as entidades para os DTOs
builder.Services.AddAutoMapper(typeof(EntitiesMappingDTO));

//Configura do JWT Bearer Authentication

// le as configurações do appsettings.json
var jwtSettings = builder.Configuration.GetSection("Jwt");
var key = Encoding.UTF8.GetBytes(jwtSettings["Key"]);

// Configura o esquema de autenticação JWT Bearer
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme; // Define o esquema de autenticação padrão
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme; // define o esqueva que requisita o token de autenticação
    
    //Configura os parametro de validação do token JWT
}).AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,                      // Valida se o emissor do token é o esperado
        ValidateAudience = true,                    // Valida se o token foi criado para o público esperado
        ValidateLifetime = true,                    // Valida se o token não está expirado
        ValidateIssuerSigningKey = true,            // Valida se a assinatura do token está correta

        ValidIssuer = jwtSettings["Issuer"],       // Define o valor esperado para o emissor do token
        ValidAudience = jwtSettings["Audience"],   // Define o valor esperado para o público do token
        IssuerSigningKey = new SymmetricSecurityKey(key)  // Define a chave secreta para validar a assinatura do token
    };
});

// Adiciona autorização (para usar [Authorize] no controller)
builder.Services.AddAuthorization();

var app = builder.Build(); 
// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthentication();         // Habilita o middleware que realiza a autenticação (verifica token na requisição)
app.UseAuthorization();          // Habilita o middleware que faz a autorização (verifica se o usuário pode acessar o recurso)

app.MapControllers();            // Mapeia os controllers para as rotas

app.Run();
