import { useState } from "react";

const PAGE_SIZE = 10;

function formatDate(value) {
    return new Date(value).toLocaleString("de-DE");
}

function formatBytes(bytes) {
    if (!bytes) return "-";
    return `${(bytes / 1024 / 1024).toFixed(1)} MB`;
}

function formatDuration(ms) {
    if (!ms) return "-";
    return `${(ms / 1000).toFixed(2)} s`;
}

const cellStyle = {
    padding: "0.75rem",
    borderBottom: "1px solid #e5e7eb",
    textAlign: "left"
};

export default function BenchmarkTable({ benchmarks,
    openBenchmarkDetails }) {
    const [page, setPage] = useState(1);
    const [sortField, setSortField] = useState("startedAtUtc");
    const [sortDirection, setSortDirection] = useState("desc");
    const [selectedAdapter, setSelectedAdapter] = useState("all");

    const adapterOptions = [
        "all",
        ...new Set(benchmarks.map(item => item.uploadVariant).filter(Boolean))
    ];

    const filteredBenchmarks =
        selectedAdapter === "all"
            ? benchmarks
            : benchmarks.filter(item => item.uploadVariant === selectedAdapter);

    const sortedBenchmarks = [...filteredBenchmarks].sort((a, b) => {
        const aValue = a[sortField];
        const bValue = b[sortField];

        if (aValue < bValue) return sortDirection === "asc" ? -1 : 1;
        if (aValue > bValue) return sortDirection === "asc" ? 1 : -1;

        return 0;
    });

    // paging
    const totalPages = Math.ceil(benchmarks.length / PAGE_SIZE);

    const startIndex = (page - 1) * PAGE_SIZE;
    const visibleBenchmarks = sortedBenchmarks.slice(
        startIndex, startIndex + PAGE_SIZE
    );

    function handleSort(field) {
        if (sortField === field) {
            setSortDirection(sortDirection === "asc" ? "desc" : "asc");
        } else {
            setSortField(field);
            setSortDirection("asc");
            setPage(1);
        }
    }

    function getSortIndicator(field) {
        if (sortField === field) {
            return sortDirection === "asc"
                ? " ▲"
                : " ▼";
        }

        return " ⇅";
    }

    return (
        <section style={{ marginTop: "2rem" }}>
            <h2>Letzte Benchmarkläufe</h2>

            <div className="table-toolbar">
                <label>
                    Upload-Adapter:{" "}
                    <select
                        value={selectedAdapter}
                        onChange={event => {
                            setSelectedAdapter(event.target.value);
                            setPage(1);
                        }}
                    >
                        {adapterOptions.map(adapter => (
                            <option key={adapter} value={adapter}>
                                {adapter === "all" ? "Alle" : adapter}
                            </option>
                        ))}
                    </select>
                </label>

                <div>{filteredBenchmarks.length} Einträge gefunden</div>
            </div>

            <table
                style={{
                    width: "100%",
                    borderCollapse: "collapse",
                    tableLayout: "fixed"
                }}
                >
                <thead>
                    <tr>
                        <th onClick={() => handleSort("startedAtUtc")}>
                            Datum{getSortIndicator("startedAtUtc")}
                        </th>
                        <th onClick={() => handleSort("uploadVariant")}>
                            Adapter{getSortIndicator("uploadVariant")}
                        </th>
                        <th onClick={() => handleSort("personaName")}>
                            Persona{getSortIndicator("personaName")}
                        </th>
                        <th onClick={() => handleSort("originalFileName")}>
                            Datei{getSortIndicator("originalFileName")}
                        </th>
                        <th onClick={() => handleSort("sizeInBytes")} style={{ width: "110px" }}>
                            Größe{getSortIndicator("sizeInBytes")}
                        </th>
                        <th onClick={() => handleSort("durationMs")} style={{ width: "80px" }}>
                            Dauer{getSortIndicator("durationMs")}
                        </th>
                        <th style={{ width: "100px" }}>Status</th>
                    </tr>
                </thead>

                <tbody>
                    {visibleBenchmarks.map(item => (
                        <tr key={item.id ?? item.artifactId}>
                            <td style={cellStyle}>{formatDate(item.startedAtUtc)}</td>
                            <td style={cellStyle}>{item.uploadVariant}</td>
                            <td style={cellStyle}>{item.personaName ?? "–"}</td>
                            <td style={cellStyle}>{item.originalFileName}</td>
                            <td style={cellStyle}>{formatBytes(item.sizeInBytes)}</td>
                            <td style={cellStyle}>{formatDuration(item.durationInMilliseconds)}</td>
                            <td style={cellStyle}>{item.finishedAtUtc ? "OK" : "Offen"}</td>
                            <td style={cellStyle}><button onClick={() => openBenchmarkDetails(item.id)}> Details</button></td>
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

