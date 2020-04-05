using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace ReadATextFile
{

    class Program
    {

        static void Main(string[] args)
        {

            Console.WriteLine("Тази апликация ще изчисли кои са двамата служители, работили най-дълго време в екип.");
            Console.WriteLine("За целта ще Ви е необходим текстов файл в следния формат : (EmpID, ProjectID, DateFrom, DateTo)");

            Console.WriteLine("Моля въведете локацията на файла на Вашият хард диск в следния формат: C:\\Documents\\datafile.txt");
            string fileLocation = Console.ReadLine();

            if (File.Exists(fileLocation))
            {
                Console.WriteLine("Моля въведете валиден DateTime формат. Пример: dd-MM-yyyy");
                string dateFormat = Console.ReadLine();

                var dataIsOk = true;
                string[] rawData = File.ReadAllLines(fileLocation);
                List<EmployeeProjectEngagement> employeeProjectEngagementList = new List<EmployeeProjectEngagement>();
                var lineCount = 0;

                foreach (string line in rawData)
                {
                    lineCount++;
                    string lineNoWhiteSpace = line.Replace(" ", "");
                    string[] dataCells = lineNoWhiteSpace.Split(',');
                    if (dataCells.Length != 4)
                    {
                        dataIsOk = false;
                        Console.WriteLine($"Грешен формат на данните. Вижте ред {lineCount}.");
                        Console.ReadLine();
                        break;
                    }

                    int _empId;
                    int _projId;
                    DateTime dateFrom;
                    DateTime dateTo;

                    if (!int.TryParse(dataCells[0], out _empId))
                    {
                        dataIsOk = false;
                        Console.WriteLine($"Грешен формат на данните (EmpId) на ред {lineCount}.");
                        Console.ReadLine();
                        break;
                    }

                    if (!int.TryParse(dataCells[1], out _projId))
                    {
                        dataIsOk = false;
                        Console.WriteLine($"Грешен формат на данните (ProjectId) на ред {lineCount}.");
                        Console.ReadLine();
                        break;
                    }

                    if (!DateTime.TryParseExact(dataCells[2], dateFormat, null, System.Globalization.DateTimeStyles.None, out dateFrom))
                    {
                        dataIsOk = false;
                        Console.WriteLine($"Грешен формат на данните (DateFrom) на ред {lineCount}.");
                        Console.ReadLine();
                        break;
                    }

                    if (dataCells[3] == "NULL")
                    {
                        dateTo = DateTime.Now;
                    }
                    else
                    {
                        if (!DateTime.TryParseExact(dataCells[3], dateFormat, null, System.Globalization.DateTimeStyles.None, out dateTo))
                        {
                            dataIsOk = false;
                            Console.WriteLine($"Грешен формат на данните (DateTo) на ред {lineCount}.");
                            Console.ReadLine();
                            break;
                        }
                    }

                    if (dateTo < dateFrom)
                    {
                        dataIsOk = false;
                        Console.WriteLine($"Грешен формат на данните на ред {lineCount}. [DateTo] трябва да е по-късно от [DateFrom].");
                        Console.ReadLine();
                        break;
                    }

                    employeeProjectEngagementList.Add(new EmployeeProjectEngagement
                    {
                        EmpId = _empId,
                        ProjectId = _projId,
                        DateFrom = dateFrom,
                        DateTo = dateTo
                    });

                }

                if (dataIsOk)
                {
                    // We are only interested in records that have matching projectIds with other records,
                    // because if they don't we only know of one employee working on them.
                    List<IGrouping<int, EmployeeProjectEngagement>> EPEGroupedByProjectId = employeeProjectEngagementList.GroupBy(x => x.ProjectId)
                   .Where(g => g.Count() > 1)
                   .ToList();

                    var _EPCIList = new List<EmployeePairCoworkInfo>();

                    // Iterate over only those records that have repeating project ids.
                    foreach (IGrouping<int, EmployeeProjectEngagement> recordGroup in EPEGroupedByProjectId)
                    {
                        List<EmployeeProjectEngagement> recordGroupList = recordGroup.ToList();
                        for (int i = 0; i < recordGroupList.Count() - 1; i++)
                        {
                            for (int n = i + 1; n < recordGroupList.Count(); n++)
                            {
                                // you might have 2 records of the same employee working on the same project on 2 diffrent time periods.
                                if (recordGroupList[i].EmpId != recordGroupList[n].EmpId)
                                {
                                    bool periodsOverlap = (recordGroupList[i].DateFrom <= recordGroupList[n].DateTo &&
                                        recordGroupList[n].DateFrom <= recordGroupList[i].DateTo);

                                    if (periodsOverlap)
                                    {
                                        var existingEPCDRecord = _EPCIList.FirstOrDefault(x => (x.FirstEmployeeId == recordGroupList[i].EmpId && x.SecondEmployeeId == recordGroupList[n].EmpId) ||
                                        (x.FirstEmployeeId == recordGroupList[n].EmpId && x.SecondEmployeeId == recordGroupList[i].EmpId));

                                        DateTime overlapPeriodStart = recordGroupList[i].DateFrom > recordGroupList[n].DateFrom ? recordGroupList[i].DateFrom : recordGroupList[n].DateFrom;
                                        DateTime overlapPeriodEnd = recordGroupList[i].DateTo < recordGroupList[n].DateTo ? recordGroupList[i].DateTo : recordGroupList[n].DateTo;

                                        int coworkDuration = (overlapPeriodEnd - overlapPeriodStart).Days + 1;

                                        if (existingEPCDRecord != null)
                                        {
                                            existingEPCDRecord.CoworkDuration += coworkDuration;
                                            if (!existingEPCDRecord.ProjectIds.Contains(recordGroupList[i].ProjectId))
                                            {
                                                existingEPCDRecord.ProjectIds.Add(recordGroupList[i].ProjectId);
                                            }
                                        }
                                        else
                                        {
                                            _EPCIList.Add(new EmployeePairCoworkInfo
                                            {
                                                FirstEmployeeId = recordGroupList[i].EmpId,
                                                SecondEmployeeId = recordGroupList[n].EmpId,
                                                CoworkDuration = coworkDuration,
                                                ProjectIds = new List<int>() {recordGroupList[i].ProjectId }
                                            });
                                        }

                                    }
                                }
                            }
                        }
                    }

                    if (_EPCIList.Count > 0)
                    {
                        EmployeePairCoworkInfo pairWithLongestCoworkInfo = _EPCIList.OrderByDescending(item => item.CoworkDuration).First();
                        Console.WriteLine("Employee ID #1, Employee ID #2, Project ID(s), Days worked");
                        Console.WriteLine($"{pairWithLongestCoworkInfo.FirstEmployeeId}, {pairWithLongestCoworkInfo.SecondEmployeeId}, {pairWithLongestCoworkInfo.CoworkDuration}");

                        var projectIds = "";
                        for (int i = 0; i < pairWithLongestCoworkInfo.ProjectIds.Count; i++)
                        {
                            projectIds += pairWithLongestCoworkInfo.ProjectIds[i] + (i == pairWithLongestCoworkInfo.ProjectIds.Count - 1 ? "" : ", ");
                        }
                        // 2 employees might have worked on more than 1 project together.
                        Console.WriteLine($"ProjectIDs #: {projectIds}");
                    }
                    else
                    {
                        Console.WriteLine("Според данните няма служители, които да са работили заедно.");
                    }
                }
            }
            else
            {
                Console.WriteLine("Файлът не е намерен.");
            }
            Console.ReadKey();
        }
    }

    public class EmployeeProjectEngagement
    {
        public int EmpId { get; set; }
        public int ProjectId { get; set; }
        public DateTime DateFrom { get; set; }
        public DateTime DateTo { get; set; }
    }

    public class EmployeePairCoworkInfo
    {
        public int FirstEmployeeId { get; set; }
        public int SecondEmployeeId { get; set; }
        public int CoworkDuration { get; set; }
        public List<int> ProjectIds { get; set; }
    }

}