import { useState } from "react";
import { employeePairMaxTime } from "../HTTP/ApiControllers/EmployeeStatistics/ApiCalls";
import type { UploadProgressDto } from "../HTTP/ApiControllers/EmployeeStatistics/Models/UploadProgressDto";

export default function FileUploadForm() {
  const [file, setFile] = useState<File | null>(null);
  const [dateTimeFormat, setDateTimeFormat] = useState("yyyy-MM-dd");
  const [uploadProgress, setUploadProgress] = useState<
    UploadProgressDto | undefined
  >();

  const getStatusStyle = (status: string) => {
     switch (status) {
        case "LoadingData":
          return "dark-yellow";
        case "Analysing":
          return "brighter-yellow";
        case "Completed":
          return "green";
        default:
          return "default-color";
    }
  }

  const handleUpload = async () => {
    if (!file) return alert("Select a CSV file");

    try {
      const response = await employeePairMaxTime(
        "http://localhost:5124",
        file,
        dateTimeFormat
      );

      if (!response.body) throw new Error("No response body");

      const reader = response.body.getReader();
      const decoder = new TextDecoder();
      let buffer = "";
      let readingDone = false;

      while (!readingDone) {
        const { value, done } = await reader.read();
        readingDone = done;

        buffer += decoder.decode(value, { stream: true });
        const lines = buffer.split(";");
        buffer = lines.pop()!; // keep incomplete line

        for (const line of lines) {
          if (!line) continue;
          const dto: UploadProgressDto = JSON.parse(line);

          if (dto.errors !== "")
          {
            readingDone= true;
            alert(dto.errors);
          }

          setUploadProgress(dto);
        }
      }
    } catch (err) {
      console.error(err);
      alert("Upload failed");
    }
  };

  return (
    <div>
      <input
        type="file"
        accept=".csv"
        onChange={(e) => setFile(e.target.files?.[0] ?? null)}
      />
      <input
        type="text"
        value={dateTimeFormat}
        onChange={(e) => setDateTimeFormat(e.target.value)}
      ></input>
      <button onClick={handleUpload}>Calculate</button>

      {uploadProgress && uploadProgress.errors === "" && (
        <div className="results-info">
          <div className={getStatusStyle(uploadProgress.status)}>Calculation status: {uploadProgress.status}</div>
          <div>Elapsed Time: {uploadProgress.elapsedTime}</div>
          {uploadProgress.coworkingInfo && (
            <div>
              <div>
                Employee One: {uploadProgress.coworkingInfo?.employeeOneId}
              </div>
              <div>
                Employee Two: {uploadProgress.coworkingInfo?.employeeTwoId}
              </div>
              
                {Object.entries(
                  uploadProgress.coworkingInfo.projectIdDaysSpent || {}
                ).map(([projectId, daysSpent]) => (
                  <div key={projectId}>
                    Project Id: {projectId}, Coworking time: {daysSpent} days
                  </div>
                ))}
            </div>
          )}
        </div>
      )}
    </div>
  );
}
