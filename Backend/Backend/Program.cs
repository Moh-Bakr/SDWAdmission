using System.Text;
using AutoMapper;
using BAL;
using DAL;
using FluentValidation;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.FileProviders;
using Microsoft.IdentityModel.Tokens;

var policyName = "_myAllowSpecificOrigins";
var builder = WebApplication.CreateBuilder(args);

builder.Services.AddCors(options =>
{
	options.AddPolicy(name: policyName,
		builder =>
		{
			builder
				.AllowAnyOrigin()
				.AllowAnyMethod()
				.AllowAnyHeader();
		});
});

// Add JWT configuration.
builder.Services.Configure<JWT>(builder.Configuration.GetSection("JWT"));

// Add ApplicationDbContext.
builder.Services.AddIdentity<ApplicationUser, IdentityRole>().AddEntityFrameworkStores<ApplicationDbContext>();
builder.Services.AddDbContext<ApplicationDbContext>(options =>
	options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"))
);

// Add JWT authentication.
builder.Services.AddAuthentication(options =>
	{
		options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
		options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
	})
	.AddJwtBearer(o =>
	{
		o.RequireHttpsMetadata = false;
		o.SaveToken = false;
		o.TokenValidationParameters = new TokenValidationParameters
		{
			ValidateIssuerSigningKey = true,
			ValidateIssuer = true,
			ValidateAudience = true,
			ValidateLifetime = true,
			ValidIssuer = builder.Configuration["JWT:Issuer"],
			ValidAudience = builder.Configuration["JWT:Audience"],
			IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["JWT:Key"])),
			ClockSkew = TimeSpan.Zero
		};
	});

// Add services to the container.
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IJwtTokenService, JwtTokenService>();
builder.Services.AddScoped<IThreeDModelService, ThreeDModelService>();
builder.Services.AddScoped<IThreeDModelFileManager, ThreeDModelFileManager>();

// Add FluentValidation.
builder.Services.AddValidatorsFromAssemblyContaining<RegisterModelValidator>();
builder.Services.AddValidatorsFromAssemblyContaining<LoginModelValidator>();
builder.Services.AddValidatorsFromAssemblyContaining<AssignRoleRequestModelValidator>();

builder.Services.AddControllers();

// Add AutoMapper.
var mapperConfig = new MapperConfiguration(cfg => { cfg.AddProfile<MappingProfile>(); });
var mapper = mapperConfig.CreateMapper();
builder.Services.AddSingleton(mapper);


builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

var uploadsPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "Uploads");

if (!Directory.Exists(uploadsPath))
{
	Directory.CreateDirectory(uploadsPath);
}

app.UseStaticFiles(new StaticFileOptions
{
	FileProvider = new PhysicalFileProvider(uploadsPath),
	RequestPath = new PathString("/Uploads")
});
;

// Configure the HTTP request pipeline.

app.UseSwagger();
app.UseSwaggerUI();


app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();