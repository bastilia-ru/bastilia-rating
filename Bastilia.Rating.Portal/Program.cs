using Bastilia.Rating.Database;
using Bastilia.Rating.Domain;
using Bastilia.Rating.Domain.DomainServices;
using Bastilia.Rating.Portal.Common;
using Bastilia.Rating.Portal.Components;
using JoinRpg.Client;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents()
    .AddInteractiveWebAssemblyComponents();

builder.Services.AddHealthChecks();

builder.Services.RegisterRatingDal(builder.Configuration, builder.Environment);
builder.Services.AddLocalization();

builder.Services.AddOptions<PasswordOptions>().BindConfiguration("PasswordOptions");
builder.Services.AddOptions<JoinConnectOptions>().BindConfiguration("JoinConnectOptions");
builder.Services.AddTransient<ProjectNavigateHelper>();
builder.Services.AddTransient<UserLoaderHelper>();

builder.Services.AddTransient<UserImportService>();


builder.Services.AddHttpClient<JoinUserInfoClient>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseWebAssemblyDebugging();
}
else
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
}

app.UseAntiforgery();

app.UseRequestLocalization("ru-RU");

app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode()
    .AddInteractiveWebAssemblyRenderMode()
    .AddAdditionalAssemblies(typeof(Bastilia.Rating.Portal.Client._Imports).Assembly);

app.MapBrHealthChecks();

app.Run();

