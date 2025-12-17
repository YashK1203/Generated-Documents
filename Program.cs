using GeneratingDocs;
using GeneratingDocs.Components;
using GeneratingDocs.Components.Account;
using GeneratingDocs.Data;
using GeneratingDocs.Services;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add Razor components + interactive server mode
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

// Authentication + Identity setup
builder.Services.AddCascadingAuthenticationState();
builder.Services.AddScoped<IdentityUserAccessor>();
builder.Services.AddScoped<IdentityRedirectManager>();
builder.Services.AddScoped<AuthenticationStateProvider, IdentityRevalidatingAuthenticationStateProvider>();

builder.Services.AddAuthentication(options =>
{
    options.DefaultScheme = IdentityConstants.ApplicationScheme;
    options.DefaultSignInScheme = IdentityConstants.ExternalScheme;
})
.AddIdentityCookies();

// Database connection
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(connectionString));

builder.Services.AddDatabaseDeveloperPageExceptionFilter();

// -----------------------------------------------------------
// ✔ FIXED HttpClient (matches your actual running app port)
// -----------------------------------------------------------
builder.Services.AddScoped(sp =>
{
    return new HttpClient
    {
        BaseAddress = new Uri("http://localhost:5182/") // ← YOUR REAL URL
    };
});

// Custom Services
builder.Services.AddScoped<PdfService>();
builder.Services.AddScoped<SalaryCalculator>();
builder.Services.AddScoped<FileUtil>();

// API Controllers
builder.Services.AddControllers();

builder.Services.AddIdentityCore<ApplicationUser>(options =>
{
    options.SignIn.RequireConfirmedAccount = true;
})
.AddEntityFrameworkStores<ApplicationDbContext>()
.AddSignInManager()
.AddDefaultTokenProviders();

// Email sender (no-op)
builder.Services.AddSingleton<IEmailSender<ApplicationUser>, IdentityNoOpEmailSender>();

var app = builder.Build();

// Middleware pipeline
if (app.Environment.IsDevelopment())
{
    app.UseMigrationsEndPoint();
}
else
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseStaticFiles();
app.UseAntiforgery();

// Map API endpoints
app.MapControllers();

// Map Blazor UI
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

// Identity / Account endpoints
app.MapAdditionalIdentityEndpoints();

app.Run();
