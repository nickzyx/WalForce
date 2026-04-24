import { useEffect, useState } from "react";
import Navbar from "../components/Navbar";
import api from "../services/api";

function EmployeeSchedule() {
  const [schedule, setSchedule] = useState([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState("");

useEffect(() => {
  const fetchSchedule = async () => {
    try {
      const response = await api.get("/me/schedule", {
      params: {
      from: "2026-04-13",
      to: "2026-04-26",
      },
    });
      setSchedule(response.data);
    } catch (err) {
      console.error(err);
      setError("Failed to load schedule.");
    } finally {
      setLoading(false);
    }
  };

  fetchSchedule();
}, []);


  return (
    <div>
      <Navbar role="employee" />

      <div style={styles.container}>
        <h1>My Schedule</h1>

        {loading && <p>Loading schedule...</p>}
        {error && <p style={styles.error}>{error}</p>}

        {!loading && !error && schedule.length === 0 && <p>No shifts found.</p>}

        {!loading && !error && schedule.length > 0 && (
          <table style={styles.table}>
            <thead>
              <tr>
                <th style={styles.th}>Date</th>
                <th style={styles.th}>Start Time</th>
                <th style={styles.th}>End Time</th>
                <th style={styles.th}>Location</th>
              </tr>
            </thead>
            <tbody>
              {schedule.map((shift, index) => (
                <tr key={index}>
                  <td style={styles.td}>{shift.date}</td>
                  <td style={styles.td}>{shift.startTime}</td>
                  <td style={styles.td}>{shift.endTime}</td>
                  <td style={styles.td}>{shift.location}</td>
                </tr>
              ))}
            </tbody>
          </table>
        )}
      </div>
    </div>
  );
}

const styles = {
  container: {
    padding: "24px",
  },
  error: {
    color: "red",
  },
  table: {
    width: "100%",
    borderCollapse: "collapse",
    marginTop: "20px",
  },
  th: {
    border: "1px solid #d1d5db",
    padding: "12px",
    backgroundColor: "#f3f4f6",
    textAlign: "left",
  },
  td: {
    border: "1px solid #d1d5db",
    padding: "12px",
  },
};

export default EmployeeSchedule;