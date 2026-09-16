using qweaadAPI;

var builder = WebApplication.CreateBuilder(args);

var commissionSBP = (GlobalData.sbp * 100 - 100);
var commissionOther = (GlobalData.other * 100 - 100);

Console.WriteLine($"Маржа: {(1 + GlobalData.globalMultiplier) * 100 - 100}%");
Console.WriteLine($"Комиссия СБП: {commissionSBP}%");
Console.WriteLine($"Комиссия основная: {commissionOther}%");


LogBuffer.Log($"API Starting. SBP: {commissionSBP:F2}%, Other: {commissionOther:F2}%", LogLevelType.Info);

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", builder =>
    {
        builder.AllowAnyOrigin()
               .AllowAnyMethod()
               .AllowAnyHeader();
    });
});

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();


app.Use(async (context, next) =>
{
    try
    {
        await next();

       
        var statusCode = context.Response.StatusCode;
        var method = context.Request.Method;
        var path = context.Request.Path;

        if (statusCode >= 400)
        {
            LogBuffer.Log($"Response {statusCode}: {method} {path}", LogLevelType.Warning);
        }
    }
    catch (Exception ex)
    {
        LogBuffer.LogError(ex, $"Request: {context.Request.Method} {context.Request.Path}");
        context.Response.StatusCode = 500;
        await context.Response.WriteAsJsonAsync(new { Success = false, Error = "Internal server error" });
    }
});

app.UseCors("AllowAll");

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseAuthorization();
app.MapControllers();

AppDomain.CurrentDomain.ProcessExit += (_, _) =>
{
    LogBuffer.Log("API shutting down...", LogLevelType.Info);
    LogBuffer.FlushAndStop();
};

app.Run();