using EmployeesWorkDuration.Services.Interfaces;

namespace EmployeesWorkDuration.Services
{
    public class StreamReaderService : IStreamReaderService
    {
        public async IAsyncEnumerable<string> ReadLineAsync(IFormFile file)
        {
            using var streamReader = new StreamReader(file.OpenReadStream());

            string? line;

            while ((line = await streamReader.ReadLineAsync()) != null)
            {
                yield return line;
            }
        }
    }
}
