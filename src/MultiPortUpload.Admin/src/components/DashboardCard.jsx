export default function DashboardCard({ title, value }) {
    return (
      <div
        style={{
          border: "1px solid #ddd",
          borderRadius: "10px",
          padding: "20px",
          minWidth: "200px",
          boxShadow: "0 2px 5px rgba(0,0,0,0.1)"
        }}
      >
        <h3>{title}</h3>
        <div
          style={{
            fontSize: "2rem",
            fontWeight: "bold"
          }}
        >
          {value}
        </div>
      </div>
    );
  }