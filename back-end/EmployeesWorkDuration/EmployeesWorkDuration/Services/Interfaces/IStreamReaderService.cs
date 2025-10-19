namespace EmployeesWorkDuration.Services.Interfaces;

public interface IStreamReaderService
{
    IAsyncEnumerable<string> ReadLineAsync(IFormFile file);
}
