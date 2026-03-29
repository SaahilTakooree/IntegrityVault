// Import dependencies.
using IntegrityVault.Repository.Contexts; // DbContext for database access.
using IntegrityVault.Repository.Implementations; // Repository class implementations.
using IntegrityVault.Repository.Interfaces; // Repository interfaces.
using IntegrityVault.Service.Implementations; // Service class implementations.
using IntegrityVault.Service.Interfaces; // // Service interfaces.
using Microsoft.EntityFrameworkCore; // // EF Core for database operations.
using Microsoft.AspNetCore.Authentication.JwtBearer; // JWT authentication.
using Microsoft.IdentityModel.Tokens; // JWT token handling.
using System.Text; //  Encoding for JWT key.
using System.Security.Claims; // ClaimTypes for JWT claims.
using IntegrityVault.Common.Converters; // DateOnly converters.
using Microsoft.OpenApi.Models; //
using QuestPDF.Infrastructure;
using IntegrityVault.Common.Configurations;


// Create web application builder.
var builder = WebApplication.CreateBuilder(args);


// Add services to the container.
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Add(new DateOnlyJsonConverter());
    }); // Allows Swagger to see the created endpoint.
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(); // Adds Swagger generation.


// Configure SQL Server database.
builder.Services.AddDbContext<IntegrityVaultDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Add the blockchain configuration.
builder.Services.Configure<BlockchainSettings>(
    builder.Configuration.GetSection("Blockchain"));


// Register the services and repository.
builder.Services.AddScoped<IHospitalService, HospitalService>();
builder.Services.AddScoped<IHospitalRepository, HospitalRepository>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IAuthRepository, AuthRepository>();
builder.Services.AddScoped<IEpisodeService, EpisodeService>();
builder.Services.AddScoped<IEpisodeRepository, EpisodeRepository>();
builder.Services.AddScoped<IMedicalRecordService, MedicalRecordService>();
builder.Services.AddScoped<IMedicalRecordRepository, MedicalRecordRepository>();
builder.Services.AddScoped<IRecordAccessLogRepository, RecordAccessLogRepository>();
builder.Services.AddScoped<IMedicalRecordAuditLogRepository, MedicalRecordAuditLogRepository>();
builder.Services.AddScoped<IPDFService, PDFService>();
builder.Services.AddScoped<IBlockchainService, BlockchainService>();

// Create jwt configuration.
var jwtKey = builder.Configuration["Jwt:Key"] ?? throw new InvalidOperationException("JWT key is missing.");
var jwtIssuer = builder.Configuration["Jwt:Issuer"] ?? throw new InvalidOperationException("JWT issuer is missing.");

// Store the master key for encryption and decryption.
var cryptoKeyString = builder.Configuration["Crypto:MasterKey"] ?? throw new InvalidOperationException("Crypto master key is missing.");
var cryptoKeyBytes = Encoding.UTF8.GetBytes(cryptoKeyString);
builder.Services.AddSingleton<ICryptoService>(new CryptoService(cryptoKeyBytes)); // Register the ICryptoService implementation.
builder.Services.AddScoped<IIPFSService, IPFSService>();

builder.Services.AddHttpClient();

// Add jtw to the builder.
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
}) 
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = false,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = jwtIssuer,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey!)),
        RoleClaimType = ClaimTypes.Role,
        NameClaimType = ClaimTypes.Name
    };
});


// Configure CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAngular",
        policy =>
        {
            policy.WithOrigins("http://localhost:4200")
                  .AllowAnyHeader()
                  .AllowAnyMethod();
        });
});


// Add JWT authentication to Swagger.
builder.Services.AddSwaggerGen(c =>
{
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        In = ParameterLocation.Header,
        Description = "Please insert JWT token",
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        BearerFormat = "JWT",
        Scheme = "bearer"
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" }
            },
            Array.Empty<string>()
        }
    });
});

// Build app.
var app = builder.Build();


// Configure middleware.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger(); // Enable Swagger JSON endpoint.
    app.UseSwaggerUI(); // Enable Swagger UI at /swagger.
}


// Middleware pipeline.
app.UseCors("AllowAngular"); // Enable CORS policy.
app.UseHttpsRedirection(); // Redirect HTTP to HTTPS.
app.UseAuthentication(); // Enable authentication.
app.UseAuthorization(); // Enable authorisation.
app.MapControllers(); // Map controller routes.

// Set QuestPDF license here at startup.
QuestPDF.Settings.License = LicenseType.Community;

// Run the application.
app.Run();
