using System.IdentityModel.Tokens.Jwt;
using System.Text;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Viagium.Configurations;
using Viagium.Data;
using Viagium.EntitiesDTO;
using Viagium.Repository;
using Viagium.Repository.Interface;
using Viagium.Services;
using Viagium.Services.Interfaces;
using Viagium.Services.Auth;
using AutoMapper;
using Viagium.Services.Auth.Affiliate;


var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// configura a ignoração de ciclos de referência no JSON
builder.Services.AddControllers().AddJsonOptions(options =>
{
    options.JsonSerializerOptions.ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles;
});

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
    var xmlFile = $"{System.Reflection.Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    c.IncludeXmlComments(xmlPath);
});

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Registra os repositórios
builder.Services.AddScoped<ITravelPackageRepository, TravelPackageRepository>();
builder.Services.AddScoped<IAddressRepository, AddressRepository>();
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IReservationRepository, ReservationRepository>();
builder.Services.AddScoped<IRoomTypeRepository, RoomTypeRepository>();
builder.Services.AddScoped<IAmenityRepository, AmenityRepository>();
builder.Services.AddScoped<IHotelRepository, HotelRepository>();
builder.Services.AddScoped<IReviewRepository, ReviewRepository>();
builder.Services.AddScoped<IPaymentRepository, PaymentRepository>();


// Registra o UnitOfWork e o serviços
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
builder.Services.AddScoped<TravelPackageService>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IAffiliateService, AffiliateService>();
builder.Services.AddScoped<IAffiliateRepository, AffiliateRepository>();
builder.Services.AddScoped<UserService>();
builder.Services.AddScoped<AddressService>();
builder.Services.AddScoped<IAdminService, AdminService>();
builder.Services.AddScoped<IReservationService, ReservationService>();
builder.Services.AddScoped<IAmenityService, AmenityService>();
builder.Services.AddScoped<IPaymentService, PaymentService>();
builder.Services.AddScoped<PaymentService>();
builder.Services.AddScoped<IHotelService, HotelService>();
builder.Services.AddScoped<IReviewService, ReviewService>();

builder.Services.AddScoped<IRoomTypeService, RoomTypeService>(provider =>
{
    var roomTypeRepo = provider.GetRequiredService<IRoomTypeRepository>();
    var amenityRepo = provider.GetRequiredService<IAmenityRepository>();
    var mapper = provider.GetRequiredService<IMapper>();
    return new RoomTypeService(roomTypeRepo, amenityRepo, mapper);
});


builder.Services.AddScoped<IAuthAffiliateService, AuthAffiliateService>();

builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IAuthService, AuthService>();

// Configuração do AuthService
builder.Services.Configure<JwtSettings>(builder.Configuration.GetSection("Jwt"));



//Configura a AutoMapper para mapear as entidades para os DTOs
builder.Services.AddAutoMapper(typeof(Viagium.ProfileAutoMapper.EntitiesMappingProfile));

//Configura do JWT Bearer Authentication

// le as configurações do appsettings.json
var jwtSettings = builder.Configuration.GetSection("Jwt");
var jwtKey = jwtSettings["Key"] ?? throw new InvalidOperationException("JWT Key não configurada em appsettings.json");
var key = Encoding.UTF8.GetBytes(jwtKey);

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
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = jwtSettings["Issuer"],
        ValidAudience = jwtSettings["Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(key)
    };
    // Verificação da blacklist
    options.Events = new JwtBearerEvents
    {
        OnTokenValidated = async context =>
        {
            var tokenBlacklistService = context.HttpContext.RequestServices.GetRequiredService<ITokenBlacklistService>();
            var loggerFactory = context.HttpContext.RequestServices.GetService(typeof(ILoggerFactory)) as ILoggerFactory;
            var logger = loggerFactory?.CreateLogger("JwtBlacklist");
            var authHeader = context.HttpContext.Request.Headers["Authorization"].FirstOrDefault();
            logger?.LogInformation("[JWT] OnTokenValidated chamado. Authorization header: {Header}", authHeader);
            if (!string.IsNullOrEmpty(authHeader) && authHeader.StartsWith("Bearer "))
            {
                var rawToken = authHeader.Substring("Bearer ".Length).Trim().ToLowerInvariant();
                logger?.LogInformation("[JWT] Token extraído do header: {Token}", rawToken);
                if (await tokenBlacklistService.IsTokenBlacklistedAsync(rawToken))
                {
                    logger?.LogWarning("[JWT] Token bloqueado por blacklist: {Token}", rawToken);
                    context.Fail("Token revogado (logout). Faça login novamente.");
                }
                else
                {
                    logger?.LogInformation("[JWT] Token NÃO está na blacklist: {Token}", rawToken);
                }
            }
            else
            {
                logger?.LogWarning("[JWT] Authorization header ausente ou mal formatado.");
            }
        }
    };
});

// Adiciona autorização (para usar [Authorize] no controller)
builder.Services.AddAuthorization();

// Configuração de CORS para permitir requisições do frontend
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend",
        policy => policy
            .WithOrigins("http://localhost:5173")
            .AllowAnyHeader()
            .AllowAnyMethod());
});

builder.Services.AddHttpClient<ImgbbService>();
builder.Services.Configure<EmailSettings>(builder.Configuration.GetSection("EmailSettings"));
builder.Services.AddScoped<IEmailService, Viagium.Services.Email.EmailService>();
builder.Services.AddScoped<ITokenBlacklistService, InMemoryTokenBlacklistService>();

var app = builder.Build(); 
// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseCors("AllowFrontend");
app.UseAuthentication();         // Habilita o middleware que realiza a autenticação (verifica token na requisição)
app.UseAuthorization();          // Habilita o middleware que faz a autorização (verifica se o usuário pode acessar o recurso)// Habilita o CORS com a política definida

app.MapControllers();            // Mapeia os controllers para as rotas

app.Run();
