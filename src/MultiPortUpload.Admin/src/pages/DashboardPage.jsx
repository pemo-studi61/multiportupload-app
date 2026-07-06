import { useEffect, useState } from "react";
import { getAdminSummary, getBenchmarks, getBenchmark, getHealth } from "../services/adminApi";
import DashboardCard from "../components/DashboardCard";
import BenchmarkTable from "../components/BenchmarkTable";
import SystemStatus from "../components/SystemStatus";
import LogViewer from "../components/LogViewer";
import BenchmarkDialog from "../components/BenchmarkDialog";
// PM: 10/06/2024 - neu
import FieldTestPage from "./FieldTestPage";

export default function DashboardPage() {
    const [summary, setSummary] = useState(null);
    const [benchmarks, setBenchmarks] = useState([]);
    const [health, setHealth] = useState(null);
    const [selectedBenchmark, setSelectedBenchmark] = useState(null);

    useEffect(() => {

        async function loadDashboard() {

            try {

                const summaryData = await getAdminSummary();
                setSummary(summaryData);

                const benchmarkData = await getBenchmarks();
                setBenchmarks(benchmarkData);

                const healthData = await getHealth();
                setHealth(healthData);

            } catch (error) {
                console.error(error);
            }
        }

        loadDashboard();

    }, []);

    if (!summary) {
        return <p>Lade Admin-Dashboard...</p>;
    }

    async function openBenchmarkDetails(id) {
        const benchmark = await getBenchmark(id);
        setSelectedBenchmark(benchmark);
    }
    
    if (window.location.pathname === "/admin/fieldtest") {
        return <FieldTestPage />;
    }

    return (
        <main style={{ padding: "20px" }}>
            <h1>MultiPortUpload Admin Console</h1>

            <SystemStatus health={health} />

            <div
                style={{
                    display: "flex",
                    gap: "20px",
                    flexWrap: "wrap"
                }}
            >
                <DashboardCard
                    title="Benchmarkläufe"
                    value={summary.benchmarkRuns}
                />

                <DashboardCard
                    title="Upload Adapter"
                    value={summary.uploadAdapters}
                />

                <DashboardCard
                    title="Logdateien"
                    value={summary.logFiles}
                />

                <DashboardCard
                    title="Fehler heute"
                    value={summary.errorsToday}
                />
            </div>
            <BenchmarkTable
                benchmarks={benchmarks}
                openBenchmarkDetails={openBenchmarkDetails}
            />

            {selectedBenchmark && (
                <BenchmarkDialog
                    benchmark={selectedBenchmark}
                    onClose={() => setSelectedBenchmark(null)}
                />
            )}

            <LogViewer />


        </main>
    );
}