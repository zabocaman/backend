using backend.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews();
builder.Services.Configure<RapidApiOptions>(builder.Configuration.GetSection("RapidApi"));
builder.Services.AddHttpClient<MovieApiService>(client =>
{
    client.BaseAddress = new Uri("https://movie-database-alternative.p.rapidapi.com/");
    client.DefaultRequestHeaders.Add("x-rapidapi-host", "movie-database-alternative.p.rapidapi.com");
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
    pattern: "{controller=Movies}/{action=Index}/{id?}");

app.Run();
