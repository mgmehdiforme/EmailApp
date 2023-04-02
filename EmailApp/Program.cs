using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using EmailApp.Data;
using EmailApp.Services;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddDbContext<EmailAppContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("EmailAppContext") ?? throw new InvalidOperationException("Connection string 'EmailAppContext' not found.")));

// Add services to the container.
builder.Services.AddScoped<IEmailService, EmailService>();
builder.Services.AddControllersWithViews();
builder.Services.AddRazorPages().AddRazorRuntimeCompilation(); 
var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
}
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
