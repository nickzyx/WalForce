import { Link } from "react-router-dom";
import Navbar from "../components/Navbar";

function EmployeePage() {
  const employeeName = localStorage.getItem("employeeName") || "Employee";

  return (
    <div>
      <Navbar role="employee" />

      <div style={styles.container}>
        <h1>Welcome, {employeeName}</h1>
        <p>This is the employee dashboard.</p>

        <div style={styles.cardContainer}>
          <Link to="/employee/schedule" style={styles.cardLink}>
            <div style={styles.card}>
              <h3>View Schedule</h3>
              <p>Check your assigned work schedule.</p>
            </div>
          </Link>

          <Link to="/employee/availability" style={styles.cardLink}>
            <div style={styles.card}>
              <h3>Update Availability</h3>
              <p>Submit your available days and times.</p>
            </div>
          </Link>
        </div>
      </div>
    </div>
  );
}

const styles = {
  container: {
    padding: "24px",
  },
  cardContainer: {
    display: "flex",
    gap: "20px",
    marginTop: "24px",
    flexWrap: "wrap",
  },
  cardLink: {
    textDecoration: "none",
    color: "inherit",
  },
  card: {
    border: "1px solid #d1d5db",
    borderRadius: "10px",
    padding: "20px",
    width: "260px",
    boxShadow: "0 2px 6px rgba(0, 0, 0, 0.08)",
    cursor: "pointer",
  },
};

export default EmployeePage;