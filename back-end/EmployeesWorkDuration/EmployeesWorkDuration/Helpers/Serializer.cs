using System.Text.Json;

namespace EmployeesWorkDuration.Helpers;

public static class Serializer
{
    public static string SerializeDataForContinuousUIUpdates(object obj)
    {
        return SerializeData(obj) + ";";
    }

    public static string SerializeData(object obj)
    {
        return JsonSerializer.Serialize(obj, options: new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });
    }
}
