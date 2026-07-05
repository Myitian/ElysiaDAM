using Microsoft.EntityFrameworkCore;
using Myitian.ElysiaDAM.CoreService;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);
builder.Services.AddDbContext<AppDbContext>(options => options
    .UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection"))
    .UseSnakeCaseNamingConvention());
builder.Services.AddControllers();
builder.Services.AddOpenApi();
WebApplication app = builder.Build();
if (app.Environment.IsDevelopment())
    app.MapOpenApi();
app.MapControllers();
app.UseStaticFiles();
app.Run();