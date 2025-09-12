using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

var builder = WebApplication.CreateBuilder(args);

// Add services
builder.Services.AddControllersWithViews();

var app = builder.Build();

// Configure middleware
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");   // generic error handler
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

// Simple status code pages (404 etc) -> re-exec to /Home/Error?code=###
app.UseStatusCodePagesWithReExecute("/Home/Error", "?code={0}");

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
