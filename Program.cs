using Microsoft.OpenApi.Models;
using MSCalculations.Models;
using Microsoft.EntityFrameworkCore;
using MSCalculations.GeneralValidationClass;
using FluentValidation.Results;

var builder = WebApplication.CreateBuilder(args);
var connectionString = builder.Configuration.GetConnectionString("InterestRateHistory") ?? $"Data Source=InterestRateHistory.db";

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

builder.Services.AddScoped<InterestRateHistoryDb, InterestRateHistoryDb>();

//utilizing swagger for endpoint testing
builder.Services.AddSwaggerGen(c =>
{
    c.MapType<DateOnly>(() => new OpenApiSchema()
    {
        Type = "string",
        Format = "date"
    });
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "Interest Rate History API", Description = "Interest Rate History", Version = "v1" });
});

//if dev environment using local SqlLite instance
//if (builder.Environment.IsDevelopment())
if (true)
{
    builder.Services.AddSqlite<InterestRateHistoryDb>(connectionString);
}

var app = builder.Build();

//simple compound interest calculation
//based on compounding monthly
//rate is annual
//A = P(1 + r / n)^nt
//A = Accrued amount(principal + interest)
//P = Principal amount
//r = Annual nominal interest rate as a decimal
//n = number of compounding periods per unit of time
//t = time in decimal years; e.g., 6 months is calculated as 0.5 years.Divide your partial year number of months by 12 to get the decimal years.
//I = Interest amount
app.MapGet("/TotalAmountofLoan", (double principal, double rate, int nummonthsofloan) =>
        Math.Round(principal * Math.Pow(1d + rate / 12d, nummonthsofloan), 2)
).WithDescription("Principal amount, rate = Annual nominal interest rate as a decimal (not percent), Number months of loan.");

var root = app.MapGroup("");
root.AddEndpointFilterFactory(ValidationFilter.ValidationFilterFactory);

root.MapGet("/InterestRateHistoryItems", async (InterestRateHistoryDb db) => await db.InterestRateHistory.ToListAsync());
root.MapGet("/InterestRateHistoryItem/{id}", async (InterestRateHistoryDb db, int id) => await db.InterestRateHistory.FindAsync(id));
root.MapPost("/InterestRateHistoryItem", async (InterestRateHistoryDb db, InterestRateHistoryItem interestratehistoryitem) =>
{
    //If successful validation, then add item
    var rates = db.InterestRateHistory.ToList();
    InterestRateHistoryItem.Validator validation = new InterestRateHistoryItem.Validator(rates);
    ValidationResult validationResult = await validation.ValidateAsync(interestratehistoryitem);
    if (validationResult.IsValid)
    {
        await db.InterestRateHistory.AddAsync(interestratehistoryitem);
        await db.SaveChangesAsync();
        return Results.Created($"/interestratehistoryitem/{interestratehistoryitem.Id}", interestratehistoryitem);
    }
    else
    {
        var errors = validationResult.Errors.Select(v => $"{v.PropertyName}: {v.ErrorMessage}").ToList();
        return Results.BadRequest(errors); 
    }

});
root.MapPut("/InterestRateHistoryItem/{id}", async (InterestRateHistoryDb db, InterestRateHistoryItem updateintratehist, int id) =>
{
    var intratehistitem = await db.InterestRateHistory.FindAsync(id);
    if (intratehistitem is null) return Results.NotFound();

    //If successful validation, then update item
    var rates = db.InterestRateHistory.ToList();
    InterestRateHistoryItem.Validator validation = new InterestRateHistoryItem.Validator(rates);
    ValidationResult validationResult = await validation.ValidateAsync(updateintratehist);
    if (validationResult.IsValid)
    {
        intratehistitem.Date = updateintratehist.Date;
        intratehistitem.Rate = updateintratehist.Rate;
        await db.SaveChangesAsync();
        return Results.NoContent();
    }
    else
    {
        var errors = validationResult.Errors.Select(v => $"{v.PropertyName}: {v.ErrorMessage}").ToList();
        return Results.BadRequest(errors);
    }

});
root.MapDelete("/InterestRateHistoryItem/{id}", async (InterestRateHistoryDb db, int id) =>
{
    var intratehistitem = await db.InterestRateHistory.FindAsync(id);
    if (intratehistitem is null) return Results.NotFound();
    db.InterestRateHistory.Remove(intratehistitem);
    await db.SaveChangesAsync();
    return Results.Ok();
});

app.UseSwagger();
app.UseSwaggerUI(c =>
{
     c.SwaggerEndpoint("/swagger/v1/swagger.json", "Interest Rate History API V1");
});

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
