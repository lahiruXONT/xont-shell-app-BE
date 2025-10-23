using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Serilog;
using System.Text;
using XONT.Ventura.ShellApp.BLL;
using XONT.Ventura.ShellApp.DAL;
using XONT.Ventura.ShellApp.DOMAIN;
using XONT.Ventura.ShellApp.Hubs;
using XONT.Ventura.ShellApp.Infrastructure;
using XONT.Ventura.ShellApp.Middlewares;

var builder = WebApplication.CreateBuilder(args);

#region Configure Logging
Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .Enrich.FromLogContext()
    .CreateLogger();

builder.Host.UseSerilog();
#endregion

#region Configure Controllers and JSON
builder.Services.AddControllers()
    .AddNewtonsoftJson(options =>
    {
        options.SerializerSettings.NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore;
        options.SerializerSettings.TypeNameHandling = Newtonsoft.Json.TypeNameHandling.Auto;
    });

#endregion

#region Configure CORS
// CORS Configuration
var corsSettings = builder.Configuration.GetSection("CorsSettings").Get<CorsSettings>();
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAngularApp", policy =>
    {
        policy.WithOrigins(corsSettings!.AllowedOrigins)
              .AllowAnyMethod()
              .AllowAnyHeader()
              .AllowCredentials();
    });
});
builder.Services.AddSignalR();
#endregion

#region Configure Swagger
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "Task Gateway API", Version = "v1" });
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "Enter Bearer token",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT"
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});
#endregion

#region Configure session
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(Convert.ToInt32(builder.Configuration["Jwt:AccessTokenExpirationMinutes"]));
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});
#endregion

#region Register services
// Register custom services
builder.Services.AddScoped<JwtTokenService>();
builder.Services.AddSingleton<SessionManager>();
builder.Services.AddScoped<IUserDAO,UserManager>();
builder.Services.AddScoped<AuthDAL>();
builder.Services.AddScoped<UserDAL>();
builder.Services.AddScoped<DBHelper>();
builder.Services.AddHttpContextAccessor();
#endregion

#region Load Plugin Assemblies
try
{
    var pluginPath = Path.Combine(AppContext.BaseDirectory, "TaskDlls");
    PluginLoader.LoadAssembliesAndRegisterServices(builder.Services, pluginPath);
}
catch (Exception ex)
{
    Log.Logger.Error(ex, "Failed to load plugin assemblies.");
}
#endregion

#region Configure authentication & Authorization
var jwtSettings = builder.Configuration.GetRequiredSection("Jwt").Get<JwtSettings>();

builder.Services.Configure<JwtSettings>(builder.Configuration.GetRequiredSection("Jwt"));

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.RequireHttpsMetadata = false;
    options.SaveToken = true;
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = jwtSettings.Issuer,
        ValidAudience = jwtSettings.Audience,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings.Key)),
        ClockSkew = TimeSpan.Zero
    };

    // SignalR support for JWT
    options.Events = new JwtBearerEvents
    {
        OnMessageReceived = context =>
        {
            var accessToken = context.Request.Query["access_token"];
            var path = context.HttpContext.Request.Path;

            if (!string.IsNullOrEmpty(accessToken) && path.StartsWithSegments("/hubs/"))
            {
                context.Token = accessToken;
            }

            return Task.CompletedTask;
        }
    };
});



//builder.Services.AddAuthorization(options =>
//{
//    options.AddPolicy("TaskAccess", policy =>
//        policy.RequireAuthenticatedUser().AddRequirements(new TaskAuthorizationRequirement()));
//    options.FallbackPolicy = new AuthorizationPolicyBuilder()
//                               .RequireAuthenticatedUser()
//                               .AddRequirements(new TaskAuthorizationRequirement())
//                               .Build();
//});
#endregion

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Task Gateway API V1");
    });
}

app.UseMiddleware<GlobalExceptionHandlingMiddleware>();
app.UseMiddleware<RequestLoggingMiddleware>();
app.UseHttpsRedirection();
app.UseCors("AllowAngularApp");
app.UseRouting();

app.UseSession();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapFallback(() =>
    Results.Json(new { Message = "Endpoint not found." }, statusCode: 404));

app.MapHub<NotificationHub>("/hubs/notification");
app.Run();