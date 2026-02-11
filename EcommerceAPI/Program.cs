using EcommerceAPI.Configuration;
using EcommerceAPI.Services;
using FluentValidation;
using FluentValidation.AspNetCore;
using System.Threading.RateLimiting;

var builder = WebApplication.CreateBuilder(args);

// Environment'a göre configuration dosyası yükle
if (builder.Environment.IsProduction())
{
    builder.Configuration.AddJsonFile("appsettings.Production.json", optional: false, reloadOnChange: true);
}

// Firebase'i başlat - ortam değişkenlerini kontrol et
try
{
    FirestoreService.Initialize(builder.Configuration, builder.Environment);
}
catch (Exception ex)
{
    throw new InvalidOperationException(
        "Firebase başlatılamadı. Lütfen ortam değişkenlerini kontrol edin:\n" +
        "FIREBASE_PROJECT_ID: Firebase project ID\n" +
        "FIREBASE_CREDENTIALS: Firebase JSON credentials (dosya yolu veya JSON string)\n" +
        "Veya appsettings.json dosyasını kontrol edin.", ex);
}

// Add services to the container
builder.Services.AddControllers();
builder.Services.AddFluentValidationAutoValidation();
builder.Services.AddValidatorsFromAssemblyContaining<Program>();
builder.Services.Configure<SellerSettings>(builder.Configuration.GetSection("SellerSettings"));

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddHttpContextAccessor();

// Firestore servisini ekle
builder.Services.AddSingleton<IFirestoreService, FirestoreService>();

// Auth servislerini ekle
builder.Services.AddScoped<FirebaseAuthService>();
builder.Services.AddScoped<ICurrentUserService, CurrentUserService>();

// Excel servisini ekle
builder.Services.AddScoped<IExcelService, ExcelService>();

// Domain servislerini ekle
builder.Services.AddScoped<IProductService, ProductService>();
builder.Services.AddScoped<IOrderService, OrderService>();
builder.Services.AddScoped<IShippingService, ShippingService>();

// CORS politikası ekle
var allowedOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>()
                    ?? new[] { "http://localhost:5174" };

builder.Services.AddCors(options =>
{
    options.AddPolicy("FrontendPolicy", policy =>
    {
        policy.WithOrigins(allowedOrigins)
              .AllowAnyMethod()
              .AllowAnyHeader()
              .AllowCredentials();
    });
});

// IP başına dakikada 100 istek
builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
    options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(context =>
        RateLimitPartition.GetFixedWindowLimiter(
            context.Connection.RemoteIpAddress?.ToString() ?? "unknown",
            _ => new FixedWindowRateLimiterOptions { PermitLimit = 100, Window = TimeSpan.FromMinutes(1) }));
});

var app = builder.Build();

// Global Exception Middleware
app.UseMiddleware<EcommerceAPI.Middleware.ExceptionMiddleware>();

app.UseRateLimiter();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// CORS'u kullan
app.UseCors("FrontendPolicy");

// Custom Firebase Auth Middleware
app.UseMiddleware<EcommerceAPI.Middleware.FirebaseAuthMiddleware>();

// Health check endpoint
app.MapGet("/health", () => Results.Ok(new { status = "healthy", timestamp = DateTime.UtcNow }));

app.UseAuthorization();
app.MapControllers();

app.Run();
