using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using trix_site.Services;

var builder = WebApplication.CreateBuilder(args);

// ---------- Services (DI) ----------

// MVC controllers + views (required for the pages and forms to render)
builder.Services.AddControllersWithViews();

builder.Services.Configure<RemoteContactOptions>(
    builder.Configuration.GetSection("RemoteContact"));
builder.Services.AddHttpClient<IMarketingContactService, RemoteContactService>();

var app = builder.Build();

// ---------- Middleware ----------
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error"); // generic error handler
    app.UseHsts();
}
else
{
    app.UseDeveloperExceptionPage();
}

// Common
app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();

// Simple status code pages (404 еле') -> re-exec м-/Home/Error?code=###
app.UseStatusCodePagesWithReExecute("/Home/Error", "?code={0}");

// Routes
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();