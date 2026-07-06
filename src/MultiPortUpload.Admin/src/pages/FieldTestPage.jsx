import { useEffect, useState } from "react";
import "../styles/fieldtest.css";

const API_BASE_URL = import.meta.env.VITE_API_BASE_URL ?? "";

export default function FieldTestPage() {
  const [adapters, setAdapters] = useState([]);
  const [selectedAdapter, setSelectedAdapter] = useState("");
  const [adapterError, setAdapterError] = useState("");
  const [selectedFile, setSelectedFile] = useState(null);
  const [isUploading, setIsUploading] = useState(false);
  const [uploadResult, setUploadResult] = useState(null);
  const [uploadedFiles, setUploadedFiles] = useState([]);
  const [networkType, setNetworkType] = useState("5G");
  const [locationComment, setLocationComment] = useState("");

  useEffect(() => {

    async function loadAdapters() {

      try {
        const allowedAdapters = ["LocalFile", "Streaming", "S3", "S3Presigned"];

        const response = await fetch(`${API_BASE_URL}/api/upload-adapters`);

        if (!response.ok) {
          throw new Error(`HTTP ${response.status}`);
        }

        const data = await response.json();
        const adapterNames = Array.isArray(data.adapters) ? data.adapters : [];

        const fieldTestAdapters = adapterNames.filter((adapter) =>
          allowedAdapters.includes(adapter)
        );

        setAdapters(fieldTestAdapters);

        if (fieldTestAdapters.length > 0) {
          setSelectedAdapter(fieldTestAdapters[0]);
        }
      } catch (error) {
        setAdapterError(error.message);
        setAdapters([]);
      }
    }
    loadAdapters();
  }, []);

  function startSpeechInput() {
    const SpeechRecognition = window.SpeechRecognition || window.webkitSpeechRecognition;

    if (!SpeechRecognition) {
      alert("Spracherkennung wird von diesem Browser nicht unterstützt.");
      return;
    }

    const recognition = new SpeechRecognition();
    recognition.lang = "de-DE";
    recognition.interimResults = false;
    recognition.maxAlternatives = 1;

    recognition.onresult = (event) => {
      const spokenText = event.results[0][0].transcript;

      setLocationComment((currentText) =>
        currentText
          ? `${currentText} ${spokenText}`
          : spokenText
      );
    };

    recognition.start();
  }

  async function handleUpload() {
    if (!selectedFile) {
      alert("Bitte zuerst eine Datei auswählen.");
      return;
    }

    setIsUploading(true);
    setUploadResult(null);

    // Zeitmarke setzen
    const timestamp = new Date().toLocaleString("de-DE");

    try {
      const formData = new FormData();
      formData.append("file", selectedFile);
      const startedAt = performance.now();
      const response = await fetch(
        `${API_BASE_URL}/api/uploads/${selectedAdapter}`,
        {
          method: "POST",
          body: formData,
        }
      );

      const durationMs = performance.now() - startedAt;

      if (!response.ok) {
        const errorText = await response.text();
        throw new Error(`HTTP ${response.status}: ${errorText}`);
      }

      const result = await response.json();

      setUploadResult({
        success: true,
        durationMs,
        response: result,
      });

      setUploadedFiles((currentFiles) => [
        {
          id: result.artifactId,
          fileName: result.originalFileName,
          adapter: result.uploadVariant,
          networkType,
          timestamp,
          sizeMb: (result.sizeInBytes / 1024 / 1024).toFixed(2),
          clientDurationSeconds: (durationMs / 1000).toFixed(2),
          serverDurationSeconds: (
            result.durationInMilliseconds / 1000
          ).toFixed(2),
          storagePath: result.storagePath,
        },
        ...currentFiles,
      ]);
    } catch (error) {
      setUploadResult({
        success: false,
        error: error.message,
      });
    } finally {
      setIsUploading(false);
    }
  }

  return (
    <main className="fieldtest-page">
      <section className="fieldtest-card">
        <h1 className="fieldtest-title">📱🚴🐶 MultiPortUpload Feldtest</h1>

        <p className="fieldtest-subtitle">
          📶 Mobilfunkqualität testen • ☁️ Upload-Adapter vergleichen • 📊 Messdaten erfassen
        </p>

        {adapterError && (
          <p className="fieldtest-error">
            ❌ {adapterError}
          </p>
        )}

        <div className="fieldtest-group">
          <label className="fieldtest-label">Upload-Adapter</label>

          <select
            className="fieldtest-select"
            value={selectedAdapter}
            onChange={(event) => setSelectedAdapter(event.target.value)}
          >
            {adapters.map((adapter) => (
              <option key={adapter} value={adapter}>
                {adapter}
              </option>
            ))}
          </select>
        </div>

        <div className="fieldtest-group">
          <label className="fieldtest-label">Datei</label>

          <label className="fieldtest-file-button">
            📷 Foto aufnehmen
            <input
              type="file"
              accept="image/*"
              capture="environment"
              hidden
              onChange={(event) =>
                setSelectedFile(event.target.files?.[0] ?? null)
              }
            />
          </label>

          <label className="fieldtest-file-button">
            🎥 Video aufnehmen
            <input
              type="file"
              accept="video/*"
              capture="environment"
              hidden
              onChange={(event) =>
                setSelectedFile(event.target.files?.[0] ?? null)
              }
            />
          </label>
        </div>

        <div className="fieldtest-group">
          <label className="fieldtest-label">
            📶 Mobilfunknetz
          </label>

          <select
            className="fieldtest-select"
            value={networkType}
            onChange={(event) => setNetworkType(event.target.value)}
          >
            <option value="5G">5G</option>
            <option value="4G">4G / LTE</option>
            <option value="3G">3G</option>
            <option value="WLAN">WLAN</option>
            <option value="Unbekannt">Unbekannt</option>
          </select>
        </div>

        <div className="fieldtest-group">
          <label className="fieldtest-label">📍 Standort / Kommentar</label>

          <div className="fieldtest-comment-wrapper">
            <textarea
              className="fieldtest-textarea fieldtest-textarea-with-button"
              placeholder="z.B. Jägerhaus, Waldweg Richtung Aichwald..."
              value={locationComment}
              onChange={(event) => setLocationComment(event.target.value)}
            />

            <button
              type="button"
              className="fieldtest-mic-button"
              onClick={startSpeechInput}
              aria-label="Kommentar sprechen"
              title="Kommentar sprechen"
            >
              🎙️
            </button>
          </div>
        </div>
        {selectedFile && (
          <p className="fieldtest-info">
            📁 {selectedFile.name} (
            {(selectedFile.size / 1024 / 1024).toFixed(2)} MB)
          </p>
        )}

        <button
          className="fieldtest-button"
          onClick={handleUpload}
          disabled={isUploading || !selectedAdapter}
        >
          {isUploading ? "⏳ Upload läuft..." : "🚀 Upload starten"}
        </button>

        {uploadResult?.success && uploadResult.response && (

          <div className="fieldtest-result-grid">

            <div className="fieldtest-metric">
              <div className="fieldtest-metric-label">📦 Größe</div>
              <div className="fieldtest-metric-value">2.73 MB</div>
            </div>

            <div className="fieldtest-metric">
              <div className="fieldtest-metric-label">📡 Adapter</div>
              <div className="fieldtest-metric-value">LocalFile</div>
            </div>

            <div className="fieldtest-metric">
              <div className="fieldtest-metric-label">⏱️ Client</div>
              <div className="fieldtest-metric-value">0.43 s</div>
            </div>

            <div className="fieldtest-metric">
              <div className="fieldtest-metric-label">📶 Uploadrate</div>
              <div className="fieldtest-metric-value">51.25</div>
              <div>Mbit/s</div>
            </div>

          </div>)}

        {uploadResult && !uploadResult.success && (
          <div className="fieldtest-error">
            <p>❌ Upload fehlgeschlagen</p>
            <p>{uploadResult.error}</p>
          </div>
        )}

        <div className="fieldtest-list">
          <h2>
            📁 Hochgeladene Dateien ({uploadedFiles.length})
          </h2>

          <ul className="fieldtest-upload-list">
            {uploadedFiles.map((file) => (
              <li
                key={file.id}
                className="fieldtest-upload-item"
              >
                <div className="fieldtest-upload-name">
                  📁 {file.fileName}
                </div>

                <div className="fieldtest-upload-meta">
                  <span>📦 {file.sizeMb} MB</span>
                  <span>📡 {file.adapter}</span>
                  <span>⏱️ {file.clientDurationSeconds} s</span>
                </div>
              </li>
            ))}
          </ul>
        </div>
      </section>
    </main>
  );
}