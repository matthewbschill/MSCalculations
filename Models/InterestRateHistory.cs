using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using FluentValidation;
using System.Text.Json.Serialization;
using System.Globalization;
using System.Text.Json;
using System.Numerics;

namespace MSCalculations.Models
{
    public class InterestRateHistoryItem
    {
        public int Id { get; set; }

        //using a custom JsonConverter for better error messaging if improper date is passed to webservice call.
        [JsonConverter(typeof(DateOnlyJsonConverter))]
        public DateOnly Date { get; set; }
        public decimal Rate { get; set; }

        //Validations using FluentValidator
        // Rate must be between 0 and 100
        // Rate and Date must both be supplied
        // Only one record per date can be in the db.
        //
        //Some Example details:
        //  Purposely avoiding string usage as may be hosted online and don't wish to have to police for content.
        //  Would add if had authorization but wish to keep this open so that viewability is there.
        public class Validator : AbstractValidator<InterestRateHistoryItem>
        {
            private readonly IEnumerable<InterestRateHistoryItem> _rates;
            public Validator(IEnumerable<InterestRateHistoryItem> rates)
            {
                _rates = rates;

                RuleFor(x => x.Date).Cascade(CascadeMode.Stop)
                    .NotEmpty()
                    .WithMessage("Must be a valid date.")
                    .Must(UniqueDate)
                    .WithMessage("There already exists a rate for that date. That rate must be updated or removed instead of adding a new rate.");

                RuleFor(x => x.Rate).Cascade(CascadeMode.Stop)
                    .NotEmpty()
                    .InclusiveBetween(0, 100);
            }
            private bool UniqueDate(InterestRateHistoryItem editedDate, DateOnly newValue)
            {
                return _rates.All(intrateitem =>
                   intrateitem.Equals(editedDate) || intrateitem.Date != newValue);
            }

        }

        //Custom JsonConverter to handle parsing of string to DateOnly
        public class DateOnlyJsonConverter : JsonConverter<DateOnly>
        {
            private const string Format = "yyyy-MM-dd";

            public override DateOnly Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
            {
                DateOnly d;

                if (!DateOnly.TryParse(reader.GetString()!, out d))
                    {
                    throw new InvalidOperationException("Date must be a valid date.");
                    };

                return d;
            }

            public override void Write(Utf8JsonWriter writer, DateOnly value, JsonSerializerOptions options)
            {
                writer.WriteStringValue(value.ToString(Format, CultureInfo.InvariantCulture));
            }
        }
    }

    //DbContext Class for EnitityFrameworkCore
    class InterestRateHistoryDb : DbContext
    {
        public InterestRateHistoryDb(DbContextOptions options) : base(options) { Database.EnsureCreated(); }
        public DbSet<InterestRateHistoryItem> InterestRateHistory { get; set; } = null!;
    }
}
