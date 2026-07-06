import { useEffect, useState } from "react";

import {
  Bar,
  BarChart,
  CartesianGrid,
  ResponsiveContainer,
  Tooltip,
  XAxis,
  YAxis,
  Cell
} from "recharts";

import "./App.css";

type AdapterResponse = {
  count: number;
  adapters: string[];
};

type UploadResponse = {
  artifactId: string;
  originalFileName: string;
  uploadVariant: string;
  sizeInBytes: number;
  durationInMilliseconds: number;
};

type BenchmarkItem = {
  index: number;
  success: boolean;
  durationInMilliseconds?: number;
  sizeInBytes?: number;
  artifactId?: string;
  error?: string;
};

function App() {
  const [adapters, setAdapters] = useState<string[]>([]);
  const [selectedAdapter, setSelectedAdapter] = useState("LocalFile");
  const [file, setFile] = useState<File | null>(null);

  const [uploadCount, setUploadCount] = useState(10);
  const [parallelism, setParallelism] = useState(2);

  const [status, setStatus] = useState("Lade Adapter ...");
  const [isRunning, setIsRunning] = useState(false);
  const [results, setResults] = useState<BenchmarkItem[]>([]);

  const [healthStatus, setHealthStatus] =
    useState<"online" | "offline" | "checking">("checking");

  const [healthText, setHealthText] = useState("Prüfe API ...");

  useEffect(() => {

    async function checkHealth() {
      try {
        const response = await fetch("/health");
        if (!response.ok) throw new Error();

        const data = await response.json();
        setHealthStatus("online");
        setHealthText(`${data.service} ${data.version} ist online`);
      } catch {
        setHealthStatus("offline");
        setHealthText("API nicht erreichbar");
      }
    }

    async function loadAdapters() {
      try {
        const response = await fetch("/api/upload-adapters");
        const data: AdapterResponse = await response.json();

        setAdapters(data.adapters);

        if (data.adapters.includes("LocalFile")) {
          setSelectedAdapter("LocalFile");
        } else if (data.adapters.length > 0) {
          setSelectedAdapter(data.adapters[0]);
        }

        setStatus(`${data.count} Upload-Adapter geladen.`);
      } catch {
        setStatus("Adapter konnten nicht geladen werden.");
      }
    }

    checkHealth();
    loadAdapters();
  }, []);

  async function uploadSingle(index: number): Promise<BenchmarkItem> {
    if (!file) {
      return { index, success: false, error: "Keine Datei ausgewählt." };
    }

    const formData = new FormData();
    formData.append("file", file);

    const startedAt = performance.now();

    try {
      const response = await fetch(`/api/uploads/${selectedAdapter}`, {
        method: "POST",
        body: formData,
      });

      if (!response.ok) {
        const text = await response.text();
        throw new Error(text || `HTTP ${response.status}`);
      }

      const data: UploadResponse = await response.json();

      return {
        index,
        success: true,
        durationInMilliseconds:
          data.durationInMilliseconds ??
          Math.round(performance.now() - startedAt),
        sizeInBytes: data.sizeInBytes,
        artifactId: data.artifactId,
      };
    } catch (error) {
      return {
        index,
        success: false,
        error: error instanceof Error ? error.message : "Unbekannter Fehler",
      };
    }
  }

  async function runBenchmark() {
    if (!file) {
      alert("Bitte zuerst eine Datei auswählen.");
      return;
    }

    setIsRunning(true);
    setResults([]);
    setStatus("Benchmark läuft ...");

    const queue = Array.from({ length: uploadCount }, (_, i) => i + 1);
    const collectedResults: BenchmarkItem[] = [];
    const startedAt = performance.now();

    async function worker() {
      while (queue.length > 0) {
        const index = queue.shift();
        if (!index) return;

        const result = await uploadSingle(index);
        collectedResults.push(result);

        setResults([...collectedResults].sort((a, b) => a.index - b.index));
        setStatus(
          `Benchmark läuft ... ${collectedResults.length} von ${uploadCount} Uploads abgeschlossen.`
        );
      }
    }

    const workerCount = Math.min(parallelism, uploadCount);
    await Promise.all(Array.from({ length: workerCount }, () => worker()));

    const totalClientDuration = Math.round(performance.now() - startedAt);

    setIsRunning(false);
    setStatus(`Benchmark abgeschlossen. Gesamtzeit: ${totalClientDuration} ms`);
  }

  const successfulResults = results.filter((item) => item.success);
  const failedResults = results.filter((item) => !item.success);

  const durations = successfulResults
    .map((item) => item.durationInMilliseconds)
    .filter((value): value is number => value !== undefined);

  const average =
    durations.length > 0
      ? Math.round(durations.reduce((sum, value) => sum + value, 0) / durations.length)
      : 0;

  const min = durations.length > 0 ? Math.min(...durations) : 0;
  const max = durations.length > 0 ? Math.max(...durations) : 0;

  const totalBytes = successfulResults.reduce(
    (sum, item) => sum + (item.sizeInBytes ?? 0),
    0
  );

  const progress =
    uploadCount > 0 ? Math.round((results.length / uploadCount) * 100) : 0;

  const successRate =
    results.length > 0
      ? Math.round((successfulResults.length / results.length) * 100)
      : 0;

  function exportBenchmarkJson() {
    const exportData = {
      createdAt: new Date().toISOString(),
      adapter: selectedAdapter,
      fileName: file?.name,
      fileSizeInBytes: file?.size,
      uploadCount,
      parallelism,
      successfulUploads: successfulResults.length,
      failedUploads: failedResults.length,
      successRate,
      averageDurationInMilliseconds: average,
      minDurationInMilliseconds: min,
      maxDurationInMilliseconds: max,
      totalBytes,
      results,
    };

    const blob = new Blob([JSON.stringify(exportData, null, 2)], {
      type: "application/json",
    });

    const url = URL.createObjectURL(blob);
    const link = document.createElement("a");

    link.href = url;
    link.download = `multiportupload-benchmark-${selectedAdapter}-${Date.now()}.json`;
    link.click();

    URL.revokeObjectURL(url);
  }

  const getBarColor = (duration: number) => {
    if (duration < average * 0.8) {
      return "#16a34a"; // schnell
    }
  
    if (duration > average * 1.2) {
      return "#dc2626"; // langsam
    }
  
    return "#2563eb"; // normal
  };

  const chartData = results.map((item) => ({
    name: `#${item.index}`,
    duration: item.durationInMilliseconds ?? 0,
  }));

  if (window.location.pathname === "/fieldtest") {
    return <FieldTestPage />;
  }

  return (
    <main className="page">
      <h1>🚀 MultiPortUpload Demo Console</h1>

      <div className={`health ${healthStatus}`}>
        <span className="health-dot"></span>
        <span>{healthText}</span>
      </div>

      <p>{status}</p>

      <section className="card">
        <h2>Benchmark starten</h2>

        <label>
          Upload-Adapter
          <select
            value={selectedAdapter}
            onChange={(e) => setSelectedAdapter(e.target.value)}
            disabled={isRunning}
          >
            {adapters.map((adapter) => (
              <option key={adapter} value={adapter}>
                {adapter}
              </option>
            ))}
          </select>
        </label>

        <label>
          Datei
          <input
            type="file"
            disabled={isRunning}
            onChange={(e) => setFile(e.target.files?.[0] ?? null)}
          />
        </label>

        <label>
          Anzahl Uploads
          <input
            type="number"
            min={1}
            max={500}
            value={uploadCount}
            disabled={isRunning}
            onChange={(e) => setUploadCount(Number(e.target.value))}
          />
        </label>

        <label>
          Parallelität
          <input
            type="number"
            min={1}
            max={50}
            value={parallelism}
            disabled={isRunning}
            onChange={(e) => setParallelism(Number(e.target.value))}
          />
        </label>

        <button onClick={runBenchmark} disabled={isRunning}>
          {isRunning ? "Benchmark läuft ..." : "Benchmark starten"}
        </button>
      </section>

      <section className="card">
        <h2>Fortschritt</h2>

        <div className="progress-track">
          <div className="progress-bar" style={{ width: `${progress}%` }}></div>
        </div>

        <p>
          {results.length} von {uploadCount} Uploads abgeschlossen ({progress}%)
        </p>
      </section>

      <section className="card">
        <h2>Benchmark-Daten</h2>

        <div className="stats">
          <div>
            <strong>{successfulResults.length}</strong>
            <span>erfolgreich</span>
          </div>

          <div>
            <strong>{failedResults.length}</strong>
            <span>Fehler</span>
          </div>

          <div>
            <strong>{successRate} %</strong>
            <span>Erfolgsquote</span>
          </div>

          <div>
            <strong>{average} ms</strong>
            <span>Durchschnitt</span>
          </div>

          <div>
            <strong>{min} ms</strong>
            <span>Minimum</span>
          </div>

          <div>
            <strong>{max} ms</strong>
            <span>Maximum</span>
          </div>

          <div>
            <strong>{(totalBytes / 1024 / 1024).toFixed(2)} MB</strong>
            <span>Datenmenge</span>
          </div>
        </div>

        <button
          onClick={exportBenchmarkJson}
          disabled={results.length === 0 || isRunning}
        >
          Benchmark als JSON herunterladen
        </button>
      </section>

      <section className="card">
        <h2>Upload-Dauer</h2>

        <div className="chart">
          {results.map((item) => {
            const duration = item.durationInMilliseconds ?? 0;
            const width = max > 0 ? Math.max(5, (duration / max) * 100) : 0;

            return (
              <div className="bar-row" key={item.index}>
                <span className="bar-label">#{item.index}</span>

                <div className="bar-track">
                  <div
                    className={`bar ${item.success ? "success" : "error"}`}
                    style={{ width: `${width}%` }}
                  ></div>
                </div>

                <span className="bar-value">
                  {item.success ? `${duration} ms` : "Fehler"}
                </span>
              </div>
            );
          })}
        </div>
      </section>

      <section className="card">
        <h2>Upload-Dauer pro Datei</h2>

        {results.length === 0 ? (
          <p>Noch keine Upload-Daten vorhanden.</p>
        ) : (
          <div className="chart-container">
            <ResponsiveContainer width="100%" height={320}>
              <BarChart data={chartData}>
                <CartesianGrid strokeDasharray="3 3" />
                <XAxis dataKey="name" />
                <YAxis />
                <Tooltip />
                <Bar dataKey="duration" name="Dauer in ms">
                  {chartData.map((entry, index) => (
                    <Cell
                      key={`cell-${index}`}
                      fill={getBarColor(entry.duration)}
                    />
                  ))}
                </Bar>
              </BarChart>
            </ResponsiveContainer>
          </div>
        )}
      </section>

      <section className="card">
        <h2>Einzelergebnisse</h2>

        <table>
          <thead>
            <tr>
              <th>#</th>
              <th>Status</th>
              <th>Dauer</th>
              <th>Größe</th>
              <th>Artifact Id</th>
            </tr>
          </thead>

          <tbody>
            {results.map((item) => (
              <tr key={item.index}>
                <td>{item.index}</td>
                <td>{item.success ? "OK" : "Fehler"}</td>
                <td>
                  {item.durationInMilliseconds !== undefined
                    ? `${item.durationInMilliseconds} ms`
                    : item.error}
                </td>
                <td>
                  {item.sizeInBytes
                    ? `${(item.sizeInBytes / 1024 / 1024).toFixed(2)} MB`
                    : "-"}
                </td>
                <td>{item.artifactId ?? "-"}</td>
              </tr>
            ))}
          </tbody>
        </table>
      </section>
    </main>
  );
}

export default App;