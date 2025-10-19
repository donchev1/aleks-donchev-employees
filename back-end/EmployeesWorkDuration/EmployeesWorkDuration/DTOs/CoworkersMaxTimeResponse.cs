using System.Text;
using System.Text.Json.Serialization;

namespace EmployeesWorkDuration.DTOs;

public class CoworkersMaxTimeResponse
{
    public EmployeeCoworkTimeDto CoworkingInfo { get; set; }
    public string Status { get; set; }
    public int ElapsedTime { get; set; }
    public string Errors => ErrorsBuilder.ToString();

    [JsonIgnore]
    public StringBuilder ErrorsBuilder { get; set; } = new();
}
