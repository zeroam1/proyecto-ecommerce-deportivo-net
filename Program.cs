using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using proyecto_ecommerce_deportivo_net.Data;
using Npgsql.EntityFrameworkCore.PostgreSQL;
using System.Diagnostics;
using proyecto_ecommerce_deportivo_net.Models;
using System.Configuration;
using proyecto_ecommerce_deportivo_net.Models.Service;

/* PARA EXPORTAR A PDF Y EXCEL */
using DinkToPdf;
using DinkToPdf.Contracts;

using Microsoft.OpenApi.Models;
using proyecto_ecommerce_deportivo_net.Integrations;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddHealthChecks();

// Add services to the container.
//var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
var connectionString = Environment.GetEnvironmentVariable("RENDER_POSTGRES_CONNECTION");
if (string.IsNullOrEmpty(connectionString))
{
    connectionString = builder.Configuration.GetConnectionString("PostgresSQLConnection") ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
}
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    //options.UseSqlite(connectionString));
    options.UseNpgsql(connectionString));
builder.Services.AddDatabaseDeveloperPageExceptionFilter();

builder.Services.AddAuthentication(options =>
    {
        // Configuración de autenticación predeterminada
        options.DefaultAuthenticateScheme = IdentityConstants.ApplicationScheme;
        options.DefaultChallengeScheme = IdentityConstants.ApplicationScheme;
        options.DefaultSignInScheme = IdentityConstants.ExternalScheme;
    })
    .AddGoogle(options =>
    {
        options.ClientId = builder.Configuration["Authentication:Google:ClientId"];
        options.ClientSecret = builder.Configuration["Authentication:Google:ClientSecret"];
    });


builder.Services.Configure<CookiePolicyOptions>(options =>
{
    options.MinimumSameSitePolicy = SameSiteMode.None;
    options.Secure = CookieSecurePolicy.Always;
});

// Aquí es donde debes agregar la configuración del servicio de carrito.
builder.Services.AddTransient<ICarritoService, CarritoService>();

// Aquí es donde debes hacer el cambio, usa builder.Configuration en lugar de Configuration
builder.Services.AddTransient<IMyEmailSender, EmailSender>(i =>
        new EmailSender(
            builder.Configuration["Email:SmtpServer"],
            int.Parse(builder.Configuration["Email:SmtpPort"]),
            builder.Configuration["Email:SmtpUsername"],
            builder.Configuration["Email:SmtpPassword"]
        )
    );

/* CONFIGURANDO PARA EXPORTAR EN EXCEL Y PDF */
// Registro del convertidor de DinkToPdf
builder.Services.AddSingleton(typeof(IConverter), new SynchronizedConverter(new PdfTools()));


builder.Services.AddDefaultIdentity<ApplicationUser>(options => options.SignIn.RequireConfirmedAccount = true)
    .AddRoles<IdentityRole>()
    .AddEntityFrameworkStores<ApplicationDbContext>();
builder.Services.AddControllersWithViews();

builder.Services.AddScoped<ProductoService, ProductoService>();

builder.Services.AddScoped<ContactanosService, ContactanosService>();
builder.Services.AddScoped<CurrencyExchangeApiIntegration, CurrencyExchangeApiIntegration>();

builder.Services.AddScoped<WeatherAPIIntegration, WeatherAPIIntegration>();
builder.Services.AddScoped<SendGridAPIIntegration, SendGridAPIIntegration>();

builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "API",
        Version = "v1",
        Description = "Descripción de la API"
    });
});



var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseMigrationsEndPoint();
}
else
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

// Generando la documentacion de la API

app.UseSwagger();

app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "API v1");
});

/* LINK PARA PONERLO EN MI URL 
CUANDO ESTA EJECUTADO EL PROYECTO 
http://localhost:5052/swagger/index.html */


app.UseHttpsRedirection();

app.UseCookiePolicy();  // <-- uso de cookies

app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();

app.UseAuthorization();

app.MapControllerRoute(
    name: "custom",
        pattern: "{company}/{controller}/{action}/{id?}",
        defaults: new { company = "AthetiX", controller = "Home", action = "Index" });
app.MapRazorPages();

app.MapHealthChecks("/health");

app.Run();


