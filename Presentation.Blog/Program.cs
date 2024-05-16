using Application.Blog;
using Domain.Blog;
using Domain.Blog.entity;
using Infrastructure.Blog;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.DependencyInjection;
var builder = WebApplication.CreateBuilder(args);
 



//for email
builder.Services.AddTransient<IEmailService, EmailService>();
//for notification
builder.Services.AddSignalR();
builder.Services.AddControllers();
 
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddDbContext<ApplicationDBContext>();
builder.Services.AddScoped<IBlogsService, BlogsService>();
builder.Services.AddScoped<ICommentService, CommentService>();
builder.Services.AddScoped<IReactService, ReactService>();
builder.Services.AddScoped<INotificationService,NotificationService>();
 



//addAuthentication
builder.Services.AddAuthentication()
    .AddBearerToken(IdentityConstants.BearerScheme); 
//Add authorization
builder.Services.AddAuthorizationBuilder();


//configure DbContext
builder.Services.AddDbContext<ApplicationDBContext>();

builder.Services.AddIdentity<User, IdentityRole>()
    .AddEntityFrameworkStores<ApplicationDBContext>()
    .AddApiEndpoints();

var app = builder.Build();


 


 


// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();


//this may be



app.UseCors(builder =>
{
    builder.WithOrigins("http://localhost:4000")
           .AllowAnyMethod()
           .AllowAnyHeader()
           .AllowCredentials();
});

app.UseStaticFiles();
app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new PhysicalFileProvider(Path.Combine(Directory.GetCurrentDirectory(), "Stores/Images")),
    
    RequestPath = "/Stores/Images"
});
 

app.UseRouting();
app.UseAuthorization();
app.UseEndpoints(endpoints =>
{
    endpoints.MapControllers();
    endpoints.MapHub<SignalRNoti>("/hubs/signalR");
    app.MapIdentityApi<User>();
});
 




await app.RunAsync();
