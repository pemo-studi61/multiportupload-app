const API_BASE_URL = "";

export function getLogDownloadUrl(fileName) {
    return `${API_BASE_URL}/api/logs/${encodeURIComponent(fileName)}/download`;
}

export async function getAdminSummary() {
  const response = await fetch(`${API_BASE_URL}/api/admin/summary`);

  if (!response.ok) {
    throw new Error("Admin summary could not be loaded.");
  }

  return response.json();
}

// Abrufen der Liste der Benchmarkläufe
export async function getBenchmarks() {
    const response = await fetch(`${API_BASE_URL}/api/admin/benchmarks`);
  
    if (!response.ok) {
      throw new Error("Could not load benchmarks.");
    }
  
    return response.json();
}

// Abrufen der Details eines einzelnen Benchmarklaufs
export async function getBenchmark(id) {
    const response = await fetch(`/api/admin/benchmarks/${id}`);
  
    if (!response.ok) {
      throw new Error("Benchmark could not be loaded.");
    }
  
    return response.json();
}

// Abrufen des Health-Status der API
export async function getHealth() {
    const response = await fetch(`${API_BASE_URL}/health`);

    if (!response.ok) {
        throw new Error("Health status could not be loaded.");
    }

    return response.json();
}

// Abrufen der Liste der Logdateien
export async function getLogFiles() {
    const response = await fetch(`${API_BASE_URL}/api/logs`);

    if (!response.ok) {
        throw new Error("Could not load log files.");
    }

    return response.json();
}

// Abrufen des Inhalts einer Logdatei (mit optionalem Tail-Parameter)
export async function getLogFileContent(fileName, tail = 300) {
    const response = await fetch(`${API_BASE_URL}/api/admin/logs` + `/${encodeURIComponent(fileName)}?tail=${tail}` );
    if (!response.ok) {
      throw new Error("Could not load log file content.");
    }
  
    return response.json();
}

