using System.Globalization;
using Bastilia.Rating.Portal.Client;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;

var builder = WebAssemblyHostBuilder.CreateDefault(args);

CultureInfo.DefaultThreadCurrentCulture = new CultureInfo("ru-RU");
CultureInfo.DefaultThreadCurrentUICulture = new CultureInfo("ru-RU");

builder.Services.AddUserLinkLocator();

await builder.Build().RunAsync();
