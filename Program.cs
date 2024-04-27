using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
var connstring = builder.Configuration.GetConnectionString("DefaultMySqlConnectionString");
builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseMySql(connstring,ServerVersion.AutoDetect(connstring))
);
string jwt_token = builder.Configuration["Jwt:SigningKey"]!;
string issuer = builder.Configuration["Jwt:Issuer"]!;
string audience = builder.Configuration["Jwt:Audience"]!;
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true, // Ensure the issuer matches the server
            ValidateAudience = true, // Ensure the audience matches the API
            ValidIssuer = issuer,
            ValidAudience = audience,
            ValidateLifetime = true, // Validate token expiration
            ValidateIssuerSigningKey = true, // Validate the signature using the secret key
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwt_token!)), // Replace with your secret key
        };
    });
builder.Services.AddCors(options =>
{
    options.AddPolicy("Policy", policy =>
    {
        policy.WithOrigins("*")
              .AllowAnyMethod()
              .AllowAnyHeader();
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

app.UseAuthentication();
app.UseAuthorization();
app.UseCors("Policy");
app.MapControllers();

app.Run();
