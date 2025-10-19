using EmployeesWorkDuration.DTOs;

namespace EmployeesWorkDuration.Services.Interfaces;

public interface IEmployeeDataAnalyser<DataType> : IEmployeeDataAnalyser
{
    Task<EmployeeCoworkTimeDto> GetMaxTimeCoworkers(DataType data, string dateTimeFormat);
}

public interface IEmployeeDataAnalyser;