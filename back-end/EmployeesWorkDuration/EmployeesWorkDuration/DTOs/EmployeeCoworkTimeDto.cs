namespace EmployeesWorkDuration.DTOs;

public class EmployeeCoworkTimeDto
{
    public int EmployeeOneId { get; set; }
    public int EmployeeTwoId { get; set; }
    public Dictionary<int, int> ProjectIdDaysSpent { get; set; } = new();
    public int TotalDays { get; set; }
}
