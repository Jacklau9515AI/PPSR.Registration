export interface UploadResult {
    submittedRecords: number;
    addedRecords: number;
    updatedRecords: number;
    invalidRecords: number;
    processedRecords: number;
    failedRecords: number;
    errorMessage?: string;
    warningMessages?: string[];
    processedAt?: string;
    successRecords?: Array<object>;
    errorRecords?: Array<object>;
}
  