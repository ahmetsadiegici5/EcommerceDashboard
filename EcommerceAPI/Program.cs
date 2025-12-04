using EcommerceAPI.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Firestore servisini ekle
builder.Services.AddSingleton<FirestoreService>();

// Auth servislerini ekle
builder.Services.AddScoped<FirebaseAuthService>();

// Excel servisini ekle
builder.Services.AddScoped<ExcelService>();

// CORS politikası ekle
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

// Servislerimizi DI container'a ekle
builder.Services.AddSingleton<FirestoreService>();
builder.Services.AddSingleton<ExcelService>();
builder.Services.AddScoped<FirebaseAuthService>();

var app = builder.Build();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

// CORS'u kullan
app.UseCors("AllowAll");

app.UseAuthorization();
app.MapControllers();

app.Run();
