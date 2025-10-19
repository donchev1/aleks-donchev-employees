using EmployeesWorkDuration.DTOs;
using EmployeesWorkDuration.Enums;
using EmployeesWorkDuration.Services.Interfaces;
using System.Globalization;

namespace EmployeesWorkDuration.Services;

public class CSVEmployeeDataAnalyser : EmployeeDataAnalyser<IFormFile>
{
    private readonly IStreamReaderService _streamReaderService;

    public CSVEmployeeDataAnalyser(IStreamReaderService streamReaderService)
    {
        _streamReaderService = streamReaderService;
    }
    protected override async Task LoadData(IFormFile file, string dateTimeFormat)
    {
        UpdateTaskStatus(TaskProgressStatus.LoadingData);

        await foreach (var line in _streamReaderService.ReadLineAsync(file))
        {
            await Task.Delay(1000); //we delay here so that the super duper amazing ui visual effects are visible

            if (string.IsNullOrWhiteSpace(line) || line.ToLower().StartsWith("empid")) continue;

            var records = line.Split(',');
            if (records.Length < 4) continue;

            try
            {
                if (!int.TryParse(records[0], out int employeeId) || 
                    !int.TryParse(records[1], out int projectId))//without any of these the record is broken
                    continue;
                
                var record = new EmployeeProjectTimeDto
                {
                    EmployeeId = employeeId, 
                    ProjectId = projectId,
                    DateFrom = records[2]?.ToLower() == "null" ? DateTime.Today : DateTime.ParseExact(records[2], dateTimeFormat, CultureInfo.InvariantCulture),
                    DateTo = records[3]?.ToLower() == "null" ? DateTime.Today : DateTime.ParseExact(records[3], dateTimeFormat, CultureInfo.InvariantCulture),
                };

                if (record.DateFrom > record.DateTo) continue;

                if (_employeesCoworkTimeByProjectId.ContainsKey(record.ProjectId))
                {
                    _employeesCoworkTimeByProjectId[record.ProjectId].Add(record);
                }
                else
                {
                    _employeesCoworkTimeByProjectId.TryAdd(record.ProjectId, new List<EmployeeProjectTimeDto> { record });
                }

            }
            catch (Exception ex)
            {
                //We don't want to throw exception because of a broken record, that's why we simply continue.
                continue;
            }

        }
    }
}
