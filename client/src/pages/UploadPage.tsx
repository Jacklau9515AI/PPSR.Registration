import { useState, useRef, useEffect } from "react";
import axios from "axios";
import Button from "../components/Button";
import Input from "../components/Input";
import { UploadResult } from "../types/result";

function UploadPage() {
  const [file, setFile] = useState<File | null>(null);
  const [result, setResult] = useState<UploadResult | null>(null);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const summaryRef = useRef<HTMLDivElement | null>(null);

  const handleUpload = async () => {
    if (!file) return;

    const formData = new FormData();
    formData.append("File", file);

    try {
      setLoading(true);
      setError(null);

      const API_BASE = "https://localhost:7066";
      const response = await axios.post(`${API_BASE}/api/Registrations/upload`, formData, {
        headers: { "Content-Type": "multipart/form-data" }
      });

      const resultData: UploadResult = response.data;
      setResult({
        ...resultData,
        processedAt: new Date().toISOString(),
      });
    } catch (err) {
      console.log(err);
      setError("Upload failed. Please check if the backend server is running.");
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    if (result && summaryRef.current) {
      summaryRef.current.scrollIntoView({ behavior: "smooth" });
    }
  }, [result]);

  return (
    <div className="min-h-screen bg-gradient-to-br from-gray-100 to-white flex items-center justify-center px-4">
      <div className="bg-white shadow-2xl rounded-2xl p-8 w-full max-w-xl transition-all">
        <h1 className="text-3xl font-semibold text-gray-800 mb-6">Batch PPSR CSV Upload</h1>

        <div className="space-y-4">
          <Input
            type="file"
            accept=".csv"
            onChange={(e) => setFile(e.target.files ? e.target.files[0] : null)}
          />
          {file && (
            <p className="text-sm text-gray-500 italic">Selected: {file.name}</p>
          )}
          <Button onClick={handleUpload} disabled={loading || !file} loading={loading}>
            Upload CSV
          </Button>
        </div>

        {error && <p className="text-red-600 mt-4">{error}</p>}

        {result && (
          <div
            ref={summaryRef}
            className="mt-8 bg-gray-50 border border-gray-200 rounded-xl p-6 shadow-inner animate-fade-in"
          >
            <h2 className="text-lg font-semibold mb-4 text-gray-800">Upload Summary</h2>
            <ul className="text-sm text-gray-700 space-y-1">
              <li>Total Records Submitted: {result.submittedRecords}</li>
              <li>New Records Added: {result.addedRecords}</li>
              <li>Existing Records Updated: {result.updatedRecords}</li>
              <li>Invalid Records: {result.invalidRecords}</li>
              <li>Total Processed: {result.processedRecords}</li>
              <li>Failed Records: {result.failedRecords}</li>
              {result.warningMessages && result.warningMessages.length > 0 && (
                <li className="mt-2">
                  <div className="bg-yellow-100 border-l-4 border-yellow-500 text-yellow-800 p-4 rounded">
                    <p className="font-medium mb-2">Warnings:</p>
                    <ul className="list-disc list-inside text-sm space-y-1">
                      {result.warningMessages.map((msg, idx) => (
                        <li key={idx}>{msg}</li>
                      ))}
                    </ul>
                  </div>
                </li>
              )}
              <li>Processed At: {result.processedAt}</li>
            </ul>
          </div>
        )}
      </div>
    </div>
  );
}

export default UploadPage;
