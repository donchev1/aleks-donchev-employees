using EmployeesWorkDuration.DTOs;
using EmployeesWorkDuration.Enums;
using EmployeesWorkDuration.Helpers;
using EmployeesWorkDuration.Services;
using EmployeesWorkDuration.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace EmployeesWorkDuration.Controllers;

[ApiController]
[Route("employee-statistics")]
public class EmployeeStatisticsController : ControllerBase
{
    private readonly IEnumerable<IEmployeeDataAnalyser> _employeeDataAnalysers;
    public EmployeeStatisticsController(IStreamReaderService streamReaderService,
        IEnumerable<IEmployeeDataAnalyser> employeeDataAnalysers)
    {
        _employeeDataAnalysers = employeeDataAnalysers;
    }

    [HttpPost("coworkers-max-time")]
    public async Task EmployeePairMaxTime([FromForm] EmployeeProjectWorkDurationDTO data)
    {
        Stopwatch stopwatch = Stopwatch.StartNew();
        var coworkersMaxTimeResponse = new CoworkersMaxTimeResponse();
        Response.ContentType = "application/json";
        Response.Headers.CacheControl = "no-cache";
        using var cancellationToken = new CancellationTokenSource();

        try
        {
            var dataAnalyser = _employeeDataAnalysers.OfType<CSVEmployeeDataAnalyser>().FirstOrDefault();

            if (dataAnalyser == null)
            {
                coworkersMaxTimeResponse.ErrorsBuilder.AppendLine("No analyser found");
                await UpdateUi(Serializer.SerializeDataForContinuousUIUpdates(coworkersMaxTimeResponse));
                return;
            }

            var analysisTask = dataAnalyser.GetMaxTimeCoworkers(data.FileUpload, data.DateTimeFormat);

            var uiUpdateTask = Task.Run(async () =>
                {
                    try
                    {
                        while (!cancellationToken.Token.IsCancellationRequested && !HttpContext.RequestAborted.IsCancellationRequested)
                        {
                            var status = dataAnalyser.GetStatus();

                            if (status == TaskProgressStatus.Completed)
                                break;

                            coworkersMaxTimeResponse.ElapsedTime = stopwatch.Elapsed.Seconds;
                            coworkersMaxTimeResponse.Status = status.ToString();
                            await UpdateUi(Serializer.SerializeDataForContinuousUIUpdates(coworkersMaxTimeResponse));
                            await Task.Delay(500, cancellationToken.Token);
                        }
                    }
                    catch (TaskCanceledException)
                    {
                        //expected
                    }
                }
            );

            var analysisTaskResult = await analysisTask;
            cancellationToken.Cancel();

            coworkersMaxTimeResponse.Status = TaskProgressStatus.Completed.ToString();
            coworkersMaxTimeResponse.CoworkingInfo = analysisTaskResult;
            await UpdateUi(Serializer.SerializeDataForContinuousUIUpdates(coworkersMaxTimeResponse));
            return;
        }
        catch (Exception ex)
        {
            cancellationToken.Cancel();
            coworkersMaxTimeResponse.ErrorsBuilder.AppendLine(ex.Message);
            await UpdateUi(Serializer.SerializeDataForContinuousUIUpdates(coworkersMaxTimeResponse));
            return;
        }
    }

    private async Task UpdateUi(string jsonData)
    {
        await Response.WriteAsync(jsonData);
        await Response.Body.FlushAsync();
    }
}
