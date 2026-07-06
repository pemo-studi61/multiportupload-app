import DashboardPage from "./pages/DashboardPage";
import FieldTestPage from "./pages/FieldTestPage";

function App() {
  if (window.location.pathname.endsWith("/fieldtest")) {
    return <FieldTestPage />;
  }

  return <DashboardPage />;
}

export default App;