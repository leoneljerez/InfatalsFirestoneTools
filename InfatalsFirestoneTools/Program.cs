using BlazorBlueprint.Components;
using InfatalsFirestoneTools;
using InfatalsFirestoneTools.Services;
using Magic.IndexedDb;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;

WebAssemblyHostBuilder builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });

// Static and made once
builder.Services.AddSingleton<MachineService>();
builder.Services.AddSingleton<AbilityService>();
builder.Services.AddSingleton<HeroService>();
builder.Services.AddSingleton<ArtifactService>();
builder.Services.AddSingleton<OptimizerDataFactory>();
builder.Services.AddSingleton<AppState>();

// Dynamic
//builder.Services.AddScoped<ThemeService>();
builder.Services.AddScoped<ProfileService>();
builder.Services.AddScoped<OptimizerService>();

// Language Detection / UI
builder.Services.AddLocalization();
builder.Services.AddBlazorBlueprintComponents();
builder.Services.AddMagicBlazorDB(BlazorInteropMode.WASM, builder.HostEnvironment.IsProduction());

await builder.Build().RunAsync();
