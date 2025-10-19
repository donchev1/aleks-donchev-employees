using EmployeesWorkDuration.DTOs;
using EmployeesWorkDuration.Enums;
using EmployeesWorkDuration.Services.Interfaces;
using System.Collections.Concurrent;

namespace EmployeesWorkDuration.Services;

public abstract class EmployeeDataAnalyser<DataType> : IEmployeeDataAnalyser<DataType>
{
    protected ConcurrentDictionary<int, List<EmployeeProjectTimeDto>> _employeesCoworkTimeByProjectId { get; set; } = new();
    protected TaskProgressStatus _taskStatus { get; set; }
    protected void UpdateTaskStatus(TaskProgressStatus status) => _taskStatus = status;
    protected abstract Task LoadData(DataType file, string dateTimeFormat);
    public TaskProgressStatus GetStatus() => _taskStatus;

    public async Task<EmployeeCoworkTimeDto> GetMaxTimeCoworkers(DataType data, string dateTimeFormat)
    {
        await LoadData(data, dateTimeFormat);

        UpdateTaskStatus(TaskProgressStatus.Analysing);

        var employeePairCoworkDurationDays = new List<EmployeeCoworkTimeDto>();

        var tasks = new List<Task>();

        foreach (var group in _employeesCoworkTimeByProjectId)
        {
            tasks.Add(Task.Run(async () => //If we are dealing with large ammounts of data we should consider batching here so we avoid creating too many tasks
            {
                await Task.Delay(5000); //we delay here so that the super duper amazing ui visual effects are visible

                var employeesProjectDuration = group.Value;

                for (int i = 0; i < employeesProjectDuration.Count; i++)
                {
                    for (int j = i + 1; j < employeesProjectDuration.Count; j++)
                    {
                        var employeeRecord1 = employeesProjectDuration[i];
                        var employeeRecord2 = employeesProjectDuration[j];

                        if (employeeRecord1.EmployeeId == employeeRecord2.EmployeeId)
                            continue;

                        var overlapStart = employeeRecord1.DateFrom > employeeRecord2.DateFrom ? employeeRecord1.DateFrom : employeeRecord2.DateFrom;
                        var overlapEnd = employeeRecord1.DateTo < employeeRecord2.DateTo ? employeeRecord1.DateTo : employeeRecord2.DateTo;

                        if (overlapStart <= overlapEnd)
                        {
                            var duration = (overlapEnd - overlapStart).TotalDays;
                            var overlapTotalDays = (int)(duration + 1);

                            var firstEmployeeId = employeeRecord1.EmployeeId < employeeRecord2.EmployeeId ? employeeRecord1.EmployeeId : employeeRecord2.EmployeeId;
                            var secondEmployeeId = employeeRecord1.EmployeeId < employeeRecord2.EmployeeId ? employeeRecord2.EmployeeId : employeeRecord1.EmployeeId;

                            var employeeCoworkDto = employeePairCoworkDurationDays.FirstOrDefault(x => x.EmployeeOneId == firstEmployeeId && x.EmployeeTwoId == secondEmployeeId);

                            if (employeeCoworkDto != null)
                            {
                                employeeCoworkDto.TotalDays += overlapTotalDays;

                                if (employeeCoworkDto.ProjectIdDaysSpent.ContainsKey(group.Key))
                                {
                                    employeeCoworkDto.ProjectIdDaysSpent[group.Key] += overlapTotalDays;
                                }
                                else
                                {
                                    employeeCoworkDto.ProjectIdDaysSpent.Add(group.Key, overlapTotalDays);
                                }
                            }
                            else
                            {
                                employeePairCoworkDurationDays.Add(new EmployeeCoworkTimeDto
                                {
                                    EmployeeOneId = firstEmployeeId,
                                    EmployeeTwoId = secondEmployeeId,
                                    ProjectIdDaysSpent = new Dictionary<int, int> { { group.Key, overlapTotalDays } },
                                    TotalDays = overlapTotalDays
                                });
                            }
                        }
                    }
                }
            }));

        }

        await Task.WhenAll(tasks);

        UpdateTaskStatus(TaskProgressStatus.Completed);

        return employeePairCoworkDurationDays
            .OrderByDescending(p => p.TotalDays)
            .FirstOrDefault();
    }
}


