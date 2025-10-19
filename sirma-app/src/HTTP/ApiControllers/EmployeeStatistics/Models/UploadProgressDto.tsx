interface EmployeePairDto {
    employeeOneId: number,
    employeeTwoId: number,
    totalDays: number,
    projectIdDaysSpent: { [key: number]: number}
}

export interface UploadProgressDto {
    status: string,
    errors: string,
    elapsedTime: number,
    coworkingInfo: EmployeePairDto
}

