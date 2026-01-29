using Bastilia.Rating.Database;
using Bastilia.Rating.Domain;
using Bastilia.Rating.Domain.DomainServices;
using Bastilia.Rating.Portal.AppServices;
using Bastilia.Rating.Portal.Common;
using Bastilia.Rating.Portal.Components;
using JoinRpg.Client;
using JoinRpg.Common.KogdaIgraClient;
using Microsoft.AspNetCore.Mvc;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents()
    .AddInteractiveWebAssemblyComponents();

builder.Services.AddHealthChecks();

builder.Services.AddRatingDal(builder.Configuration, builder.Environment);
builder.Services.AddLocalization();

builder.Services.AddOptions<PasswordOptions>().BindConfiguration("PasswordOptions");
builder.Services.AddOptions<JoinConnectOptions>().BindConfiguration("JoinConnectOptions");
builder.Services.AddOptions<KogdaIgraOptions>().BindConfiguration("KogdaIgra");
builder.Services.AddTransient<ProjectNavigateHelper>();
builder.Services.AddTransient<UserLoaderHelper>();

builder.Services.AddTransient<UserImportService>();
builder.Services.AddTransient<CalendarService>();

builder.Services.AddTransient<ICalService>();
builder.Services.AddTransient<KiAddService>();


builder.Services.AddHttpClient<JoinUserInfoClient>();
builder.Services.AddKogdaIgraClient();

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

app.MapGet("/api/calendar/ical", async ([FromServices] ICalService calendarService) => TypedResults.File(await calendarService.GetCurrentIcalCalendar(), "text/calendar"));

// TODO Ёта апишка без авторизации, но самое что может случитьс€ Ч мы просто зальем к нам всю базу  ».
app.MapGet("/api/kogda-igra/add/{id}", async (int id, [FromServices] KiAddService kiAddService) =>
{
    await kiAddService.AddKogdaIgraGame(id);
    return TypedResults.Ok();
});

app.MapGet("/api/members/actual", async ([FromServices] IBastiliaMemberRepository memberRepository) => (await memberRepository.GetActualAsync()).Select(x => x.JoinrpgUserId));

app.MapBrHealthChecks();

app.Run();

