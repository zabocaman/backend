using backend.Services;
using Microsoft.Extensions.Options;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews();
builder.Services.Configure<FlightApiOptions>(builder.Configuration.GetSection("FlightApi"));
builder.Services.AddHttpClient<FlightApiService>((sp, client) =>
{
    var options = sp.GetRequiredService<IOptions<FlightApiOptions>>().Value;

    if (Uri.TryCreate(options.BaseUrl, UriKind.Absolute, out var baseUri))
    {
        client.BaseAddress = baseUri;
    }
});

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Flights}/{action=Arrivals}/{id?}");

app.Run();
