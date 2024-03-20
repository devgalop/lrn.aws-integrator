using lrn.devgalop.awsintegrator.Infrastructure.AWS.SQS.Extensions;

var builder = WebApplication.CreateBuilder(args);

//Implements SQS consumers
builder.Services.AddSQSConsumer(builder.Configuration);

//Implements SQS publisher
builder.Services.AddSQSPublisher();

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

