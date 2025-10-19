namespace EmployeesWorkDuration.DTOs
{
    public class EmployeeProjectTimeDto
    {
        public int EmployeeId { get; set; }
        public int ProjectId { get; set; }
        public DateTime DateFrom { get; set; }
        public DateTime DateTo { get; set; }
    }
}
