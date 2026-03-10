// Import dependencies.
using System.Text.Json; // Provides functionality for JSON serialisation and deserialisation.
using System.Text.Json.Serialization; // Allows creation of custom JSON converts.


// Define the namespace for custom JSON converters in the IntergrityVault project.
namespace IntegrityVault.Common.Converters
{
    // Custom convert to handle serialisation and deserialisation of DateOnly value in JSON.
    public class DateOnlyJsonConverter : JsonConverter<DateOnly>
    {
        // Define the date format that will be used when converting DataOnly value.
        private const string Format = "yyyy-MM-dd";


        // Method used when reading JSON data and converiting it into a DateOnly object.
        public override DateOnly Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            return DateOnly.ParseExact(reader.GetString()!, Format);
        }


        // Method used when writing a DateOnly value in JSON format.
        public override void Write(Utf8JsonWriter writer, DateOnly value, JsonSerializerOptions options)
        {
            writer.WriteStringValue(value.ToString(Format));
        }
    }
}