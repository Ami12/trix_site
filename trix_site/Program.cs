using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using trix_site.Filters;
using trix_site.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services + SEO filter
builder.Services.AddSingleton<ISeoMetadataProvider>(sp =>
    new DefaultSeoMetadataProvider("https://12trix.co.il")); // <<< עדכן את ה-Base URL אם צריך

builder.Services.AddControllersWithViews(options =>
{
    options.Filters.Add<SeoMetadataFilter>(); // פילטר גלובלי שמזריק מטא-דאטה ל-ViewData
});

var app = builder.Build();

// Middleware
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

// Simple status code pages (404 וכו') -> re-exec ל-/Home/Error?code=###
app.UseStatusCodePagesWithReExecute("/Home/Error", "?code={0}");

// Routes
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
