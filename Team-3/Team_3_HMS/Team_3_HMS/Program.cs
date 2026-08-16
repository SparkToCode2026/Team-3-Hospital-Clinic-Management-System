using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Security.Claims;
using System.Text;
using Team_3_HMS.Controllers;

namespace Team_3_HMS
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.
            // DbContext
            builder.Services.AddDbContext<ProjectContext>(options =>
                options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

            builder.Services.AddScoped<IEmailService, EmailService>();

            builder.Services.AddHostedService<AppointmentReminderService>();

            builder.Services.AddControllers(options =>
            {
                options.SuppressImplicitRequiredAttributeForNonNullableReferenceTypes = true;
            });

            // Configure CORS policy to allow frontend cross-origin requests
            builder.Services.AddCors(options =>
            {
                options.AddPolicy("AllowFrontend", policy =>
                {
                    policy.SetIsOriginAllowed(_ => true)
                          .AllowAnyHeader()
                          .AllowAnyMethod()
                          .AllowCredentials();
                });
            });

            // JWT Authentication & Authorization Setup
            string jwtKey = builder.Configuration["Jwt:Key"] ?? "ThisIsAVerySecretKeyForTeam3HospitalManagementSystem2026!";

            builder.Services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey)),
                    ValidateIssuer = false,
                    ValidateAudience = false,
                    // Maps the Role claim from JWT tokens to [Authorize(Roles = "...")]
                    RoleClaimType = ClaimTypes.Role
                };

                // C# Event: Extract JWT Token from Cookie automatically
                options.Events = new JwtBearerEvents
                {
                    OnMessageReceived = context =>
                    {
                        if (context.Request.Cookies.ContainsKey("jwt_token"))
                        {
                            context.Token = context.Request.Cookies["jwt_token"];
                        }
                        return Task.CompletedTask;
                    }
                };
            });

            // Swagger setup
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen(c =>
            {
                c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                {
                    Name = "Authorization",
                    Type = SecuritySchemeType.Http,
                    Scheme = "bearer",
                    BearerFormat = "JWT",
                    In = ParameterLocation.Header,
                    Description = "Enter your JWT token in the box below"
                });

                c.AddSecurityRequirement(new OpenApiSecurityRequirement
                {
                    {
                        new OpenApiSecurityScheme
                        {
                            Reference = new OpenApiReference
                            {
                                Type = ReferenceType.SecurityScheme,
                                Id   = "Bearer"
                            }
                        },
                        new List<string>()
                    }
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

            // Enable CORS Middleware (MUST come before Authentication & Authorization)
            app.UseCors("AllowFrontend");

            // Middleware Pipeline Order (Authentication MUST come before Authorization)
            app.UseAuthentication();
            app.UseAuthorization();

            app.MapControllers();

            app.Run();
        }
    }

    public interface IEmailService
    {
        Task SendEmailAsync(string toEmail, string subject, string body);
    }

    public class EmailService : IEmailService
    {
        private readonly IConfiguration _config;

        public EmailService(IConfiguration config)
        {
            _config = config;
        }

        public async Task SendEmailAsync(string toEmail, string subject, string body)
        {
            if (string.IsNullOrWhiteSpace(toEmail)) return;

            var senderEmail = _config["EmailSettings:SenderEmail"];
            var password = _config["EmailSettings:Password"];

            if (string.IsNullOrWhiteSpace(senderEmail) || senderEmail == "test@gmail.com" || password == "password")
            {
                Console.WriteLine($"[EmailService] Simulated Email to <{toEmail}> | Subject: {subject}");
                return;
            }

            try
            {
                var message = new MimeKit.MimeMessage();
                message.From.Add(new MimeKit.MailboxAddress(
                    _config["EmailSettings:SenderName"] ?? "MedCore HMS Clinic",
                    senderEmail));
                message.To.Add(MimeKit.MailboxAddress.Parse(toEmail));
                message.Subject = subject;

                var bodyBuilder = new MimeKit.BodyBuilder
                {
                    HtmlBody = body.Contains("<div") || body.Contains("<p>") ? body : $"<div style='font-family: Arial, sans-serif; line-height: 1.6; color: #333;'>{body.Replace("\n", "<br/>")}</div>",
                    TextBody = System.Text.RegularExpressions.Regex.Replace(body, "<.*?>", string.Empty)
                };
                message.Body = bodyBuilder.ToMessageBody();

                using var client = new MailKit.Net.Smtp.SmtpClient();
                // Bypass TLS certificate validation issues in dev environments
                client.ServerCertificateValidationCallback = (s, c, h, e) => true;

                int port = int.TryParse(_config["EmailSettings:Port"], out int p) ? p : 587;
                await client.ConnectAsync(
                    _config["EmailSettings:SmtpServer"] ?? "smtp.gmail.com",
                    port,
                    MailKit.Security.SecureSocketOptions.Auto);

                // Remove XOAUTH2 so standard App Password authentication succeeds reliably
                client.AuthenticationMechanisms.Remove("XOAUTH2");

                await client.AuthenticateAsync(senderEmail, password);
                await client.SendAsync(message);
                await client.DisconnectAsync(true);
                Console.WriteLine($"[EmailService] Successfully sent email to <{toEmail}> | Subject: {subject}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[EmailService Warning] Failed to send email via SMTP to <{toEmail}>: {ex.Message}");
            }
        }
    }
}