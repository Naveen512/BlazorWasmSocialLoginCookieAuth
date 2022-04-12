using CookieApi.Data;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddDbContext<CookieAuthContext>(options => {
    options.UseSqlServer(builder.Configuration.GetConnectionString("CookieAuthConnection"));
});

builder.Services.AddAuthentication(
    CookieAuthenticationDefaults.AuthenticationScheme
)
.AddCookie()
.AddFacebook(fb => {
    fb.AppId = builder.Configuration
    .GetSection("FacebookSettings").GetValue<string>("AppID");
    
    fb.AppSecret = builder.Configuration
    .GetSection("FacebookSettings").GetValue<string>("AppSecret");
})
.AddTwitter(tt => {
    tt.ConsumerKey = builder.Configuration
    .GetSection("TwitterSettings").GetValue<string>("ApiKey");

    tt.ConsumerSecret =  builder.Configuration
    .GetSection("TwitterSettings").GetValue<string>("ApiKeySecret");
    
    tt.RetrieveUserDetails = true;
})
.AddMicrosoftAccount(mt => {
    mt.ClientId =  builder.Configuration
    .GetSection("MicrosoftSettings").GetValue<string>("AppId");

    mt.ClientSecret =  builder.Configuration
    .GetSection("MicrosoftSettings").GetValue<string>("SecretId");

}).
AddGoogle(g => {
    g.ClientId = builder.Configuration
    .GetSection("GoogleSettings").GetValue<string>("ClientId");
    g.ClientSecret = builder.Configuration
    .GetSection("GoogleSettings").GetValue<string>("ClientSecret");
});

builder.Services.AddCors(options =>
{
    options.AddPolicy("CorsSpecs",
    builder =>
    {
        builder
            .AllowAnyHeader()
            .AllowAnyMethod()
            .SetIsOriginAllowed(options => true)
            .AllowCredentials();
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseCors("CorsSpecs");
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
