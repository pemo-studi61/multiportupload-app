import "../styles/status.css";

export default function SystemStatus({ health }) {
    if (!health) {
      return <p>Lade Systemstatus...</p>;
    }
  
    return (
      <section className="system-status">
        <h2>Systemstatus</h2>
  
        <div className="status-grid">
          <StatusCard
            title="PostgreSQL"
            status={health.checks.postgreSql.status}
            detail="Datenbankverbindung"
          />
  
          <StatusCard
            title="Logs"
            status={health.checks.logs.status}
            detail={`${health.checks.logs.fileCount} Dateien`}
          />
  
          <StatusCard
            title="Uploads"
            status={health.checks.uploads.status}
            detail={`${health.checks.uploads.fileCount} Dateien`}
          />
  
          <StatusCard
            title="Version"
            status={health.status}
            detail={health.version}
          />
        </div>
      </section>
    );
  }
  
  function StatusCard({ title, status, detail }) {
    const isOk = status === "OK";
  
    return (
      <div className="status-card">
        <div className="status-card-header">
          <span className={isOk ? "status-dot ok" : "status-dot error"}></span>
          <span className="status-title">{title}</span>
        </div>
  
        <div className={isOk ? "status-value ok" : "status-value error"}>
          {status}
        </div>
  
        <div className="status-detail">{detail}</div>
      </div>
    );
  }
  
  