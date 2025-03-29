export const isValidCsv = (file: File): boolean => {
    const fileExtension = file.name.split('.').pop();
    return fileExtension === 'csv';
  };
  
  export const parseCsvToJson = (csv: string): any[] => {
    const lines = csv.split('\n');
    const headers = lines[0].split(',');
  
    return lines.slice(1).map((line) => {
      const values = line.split(',');
      let obj: any = {};
      headers.forEach((header, index) => {
        obj[header.trim()] = values[index]?.trim();
      });
      return obj;
    });
  };
  