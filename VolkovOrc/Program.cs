var builder = WebApplication.CreateBuilder(args);



// Добавляем ключ лицензии IronOCR
IronOcr.License.LicenseKey = "IRONSUITE.AVDORS610.GMAIL.COM.8147-E37C42A195-A672ZZSKSEUQ7BCK-OZRUJT3TOY23-PS5O4WNEPTEH-KJJ3WHMEDDMW-IJRRGA3VDAMA-SRDQOCI53WKC-4QDEB4-TBCVKFTSS2GOEA-DEPLOYMENT.TRIAL-W4I2AK.TRIAL.EXPIRES.18.JAN.2025";
// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
