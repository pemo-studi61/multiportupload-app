import "../styles/benchmark-dialog.css";

export default function BenchmarkDialog({
    benchmark,
    onClose
}) {
    return (
        <div className="modal-overlay">
            <div className="modal">

                <h2>Benchmarkdetails</h2>

                <table className="detail-table">
                    <tbody>
                        <tr>
                            <td>Adapter</td>
                            <td>{benchmark.uploadVariant}</td>
                        </tr>

                        <tr>
                            <td>Persona</td>
                            <td>{benchmark.personaName ?? "–"}</td>
                        </tr>

                        <tr>
                            <td>Datei</td>
                            <td>{benchmark.originalFileName}</td>
                        </tr>

                        <tr>
                            <td>Artifact Id</td>
                            <td>{benchmark.artifactId}</td>
                        </tr>

                        <tr>
                            <td>Größe</td>
                            <td>{benchmark.sizeInBytes.toLocaleString()} Byte</td>
                        </tr>

                        <tr>
                            <td>Dauer</td>
                            <td>{benchmark.durationInMilliseconds} ms</td>
                        </tr>

                        <tr>
                            <td>Durchsatz</td>
                            <td>
                                {benchmark.throughputMbPerSecond.toFixed(2)}
                                {" "}
                                MB/s
                            </td>
                        </tr>

                        <tr>
                            <td>Start</td>
                            <td>{benchmark.startedAtUtc}</td>
                        </tr>

                        <tr>
                            <td>Ende</td>
                            <td>{benchmark.finishedAtUtc}</td>
                        </tr>
                    </tbody>
                </table>

                <button onClick={onClose}>
                    Schließen
                </button>

            </div>
        </div>
    );
}