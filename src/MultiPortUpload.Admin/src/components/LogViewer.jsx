import { useEffect, useState } from "react";
import { getLogFiles, getLogDownloadUrl } from "../services/adminApi";
import "../styles/tables.css";

const PAGE_SIZE = 10;

const API_BASE_URL = import.meta.env.VITE_API_BASE_URL ?? "";

function formatDate(value) {
    return new Date(value).toLocaleString("de-DE");
}

function formatBytes(bytes) {
    if (!bytes) return "-";
    return `${(bytes / 1024).toFixed(1)} KB`;
}

const cellStyle = {
    padding: "0.75rem",
    borderBottom: "1px solid #e5e7eb",
    textAlign: "left"
};

export default function LogViewer() {
    const [logFiles, setLogFiles] = useState([]);
    const [page, setPage] = useState(1);

    useEffect(() => {
        getLogFiles()
            .then(setLogFiles)
            .catch(console.error);
    }, []);

    const totalPages = Math.ceil(logFiles.length / PAGE_SIZE);
    const startIndex = (page - 1) * PAGE_SIZE;

    const visibleLogFiles = logFiles.slice(
        startIndex,
        startIndex + PAGE_SIZE
    );

    return (
        <section style={{ marginTop: "2rem" }}>
            <h2>Logdateien</h2>

            <table className="admin-table">
                <thead>
                    <tr>
                        <th className="log-column-filename">Dateiname</th>
                        <th className="log-column-date">Geändert am</th>
                        <th className="log-column-size">Größe</th>
                        <th className="log-column-action">Aktion</th>
                    </tr>
                </thead>

                <tbody>
                    {visibleLogFiles.map(file => (
                        <tr key={file.fileName}>
                            <td style={cellStyle}>{file.fileName}</td>
                            <td style={cellStyle}>{formatDate(file.lastModified)}</td>
                            <td style={cellStyle}>{formatBytes(file.sizeBytes)}</td>
                            <td style={cellStyle}>
                                <a
                                    href={getLogDownloadUrl(file.fileName)}
                                    download={file.fileName}
                                >
                                    Download
                                </a>
                            </td>
                        </tr>
                    ))}
                </tbody>
            </table>

            <div style={{ marginTop: "1rem", display: "flex", gap: "1rem" }}>
                <button
                    onClick={() => setPage(page - 1)}
                    disabled={page === 1}
                >
                    Zurück
                </button>

                <span>
                    Seite {page} von {totalPages}
                </span>

                <button
                    onClick={() => setPage(page + 1)}
                    disabled={page === totalPages}
                >
                    Weiter
                </button>
            </div>
        </section>
    );
}