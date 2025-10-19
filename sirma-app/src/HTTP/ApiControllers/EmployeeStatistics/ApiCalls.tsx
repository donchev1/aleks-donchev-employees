import type {UploadProgressDto} from "./Models/UploadProgressDto";

export function employeePairMaxTime(baseUrl: string, file: File, dateTimeFormat: string)
: Promise<Response> {
  const form = new FormData();
  form.append("fileUpload", file);
  form.append("dateTimeFormat", dateTimeFormat);

  return fetch(baseUrl + "/employee-statistics/coworkers-max-time", {
      method: "POST",
      body: form,
    });
}