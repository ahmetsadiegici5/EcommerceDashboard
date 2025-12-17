using EcommerceAPI.Configuration;
using EcommerceAPI.Services;
using FluentValidation;
using FluentValidation.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

// Firebase'i başlat
FirestoreService.Initialize(builder.Configuration);

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



var app = builder.Build();

// Global Exception Middleware
app.UseMiddleware<EcommerceAPI.Middleware.ExceptionMiddleware>();

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

app.UseAuthorization();
app.MapControllers();

app.Run();
