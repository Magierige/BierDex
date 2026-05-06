using BierDex.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.EntityFrameworkCore;
using BierDex.Controllers;

var builder = WebApplication.CreateBuilder(args);

//initialization of the database
builder.Services.AddDbContext<BierdexDBContext>(options =>
    options.UseNpgsql(
        builder.Configuration.GetConnectionString("localhost")));

builder.Services.AddAuthorization();

// Add services to the container.
builder.Services
    .AddIdentityApiEndpoints<IdentityUser>(options => 
    {
        options.User.RequireUniqueEmail = true;
    })
    .AddRoles<IdentityRole>()
    .AddEntityFrameworkStores<BierdexDBContext>();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddSingleton<IEmailSender, SmtpControler>();

builder.Services.AddControllers();

builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

var app = builder.Build();

app.UseCors();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        // This makes Swagger UI available at the application root (/)
        options.RoutePrefix = string.Empty;

        // Ensure the endpoint still points to the correct JSON definition
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "v1");
    });
}

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

//seeders
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;

    var userManager = services.GetRequiredService<UserManager<IdentityUser>>();
    var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
    var context = services.GetRequiredService<BierdexDBContext>();

    await IdentitySeeder.SeedAsync(userManager, roleManager);
    await BeerSeeder.SeedAsync(context, userManager);
}

app.MapControllers();

var authGroup = app.MapGroup("/api/auth");
authGroup.MapIdentityApi<IdentityUser>();

app.Run();
