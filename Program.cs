using TrophiesDisplay.Models;
using TrophiesDisplay.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddRazorPages();
builder.Services.AddHttpContextAccessor();
builder.Services.AddControllers();

//Configuration
builder.Services.Configure<AppSettings>(builder.Configuration);
builder.Services.AddSingleton<IConfiguration>(builder.Configuration);

// Register services for dependency injection
builder.Services.AddSingleton<IQRCodeGenerator, QRCoderGenerator>();

builder.Services.AddSingleton<ITrophyDiscovery, FileSystemTrophyDiscovery>();
builder.Services.AddSingleton<ITrophyFactory, TrophyFactory>();
builder.Services.AddSingleton<ITrophyService, TrophyService>();

builder.Services.AddSingleton<TrophyIndexer>(); 
builder.Services.AddHostedService(sp => sp.GetRequiredService<TrophyIndexer>());

// Build the application
var app = builder.Build();

// Configure the HTTP request pipeline
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();

app.UseAuthorization();

app.MapRazorPages();
app.MapControllers();

app.Run();
