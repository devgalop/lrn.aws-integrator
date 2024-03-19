using lrn.devgalop.awsintegrator.Infrastructure.AWS.SQS.Extensions;

var builder = WebApplication.CreateBuilder(args);

//Implements SQS into your solution
builder.Services.AddSQS(builder.Configuration);

builder.Services.AddControllers();
builder.Services.AddHealthChecks();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.MapControllers();
app.MapHealthChecks("/healthy");

app.Run();

