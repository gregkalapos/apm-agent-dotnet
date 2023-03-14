// Licensed to Elasticsearch B.V under one or more agreements.
// Elasticsearch B.V licenses this file to you under the Apache 2.0 License.
// See the LICENSE file in the project root for more information


#if NET7_0
using System;
using Elastic.Apm.NetCoreAll;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using SampleAspNetCoreApp.Data;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();


const string connection = @"Data Source=blogging.db";
builder.Services.AddDbContext<SampleDataContext>
	(options => options.UseSqlite(connection));

builder.Services.AddDefaultIdentity<IdentityUser>()
	.AddEntityFrameworkStores<SampleDataContext>();

builder.Services.Configure<IdentityOptions>(options =>
{
	// Password settings
	// Not meant for production! To make testing/playing with the sample app we use very simple,
	// but insecure settings
	options.Password.RequireDigit = false;
	options.Password.RequireLowercase = false;
	options.Password.RequireNonAlphanumeric = false;
	options.Password.RequireUppercase = false;
	options.Password.RequiredLength = 5;
});

// builder.Services.AddOpenTelemetry()
// 	.ConfigureResource(builder => builder
// 		.AddService(serviceName: "OTel.NET Getting Started"))
// 		.WithTracing(builder => builder
// 		.AddAspNetCoreInstrumentation()
// 		.AddHttpClientInstrumentation()
// 		.AddEntityFrameworkCoreInstrumentation()
// 		.AddSource("HomeController")
// 		.AddOtlpExporter( s => s.Endpoint = new Uri("http://localhost:8200")));

var app = builder.Build();

app.UseAllElasticApm();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
	app.UseExceptionHandler("/Home/Error");
	// The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
	app.UseHsts();
}



app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.MapControllerRoute(
	name: "default",
	pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();

#else
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;

namespace SampleAspNetCoreApp
{
	public class Program
	{
		public static void Main(string[] args) => CreateWebHostBuilder(args).Build().Run();

		public static IWebHostBuilder CreateWebHostBuilder(string[] args) =>
			WebHost.CreateDefaultBuilder(args)
				.UseStartup<Startup>();
	}
}

#endif
