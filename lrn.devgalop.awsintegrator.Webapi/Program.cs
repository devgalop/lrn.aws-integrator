using lrn.devgalop.awsintegrator.Infrastructure.AWS.SQS.Extensions;
using lrn.devgalop.awsintegrator.Infrastructure.AWS.S3Buckets.Extensions;

var builder = WebApplication.CreateBuilder(args);
using ILoggerFactory loggerFactory = LoggerFactory.Create(builder => builder.AddConsole());
var logger = loggerFactory.CreateLogger("Program");
try
{
    //Implements SQS consumers
    builder.Services.AddSQSConsumer(builder.Configuration);

    //Implements SQS publisher
    builder.Services.AddSQSPublisher();

    //Implements S3 service
    builder.Services.AddS3();

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
}
catch (Exception ex)
{
    logger.LogCritical(ex.ToString());
}


