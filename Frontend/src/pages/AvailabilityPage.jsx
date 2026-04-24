import { useState } from "react";
import Navbar from "../components/Navbar";
import api from "../services/api";

function AvailabilityPage() {
  const [formData, setFormData] = useState({
    day: "",
    startTime: "",
    endTime: "",
  });

  const [message, setMessage] = useState("");
  const [error, setError] = useState("");

  const handleChange = (event) => {
    const { name, value } = event.target;

    setFormData((prevData) => ({
      ...prevData,
      [name]: value,
    }));
  };

const handleSubmit = async (event) => {
  event.preventDefault();
  setMessage("");
  setError("");

  const allDays = [
    "Monday",
    "Tuesday",
    "Wednesday",
    "Thursday",
    "Friday",
    "Saturday",
    "Sunday",
  ];

  const payload = {
    notes: null,
    days: allDays.map((currentDay) => {
      if (currentDay === formData.day) {
        return {
          day: currentDay,
          windows: [
            {
              startTime: `${formData.startTime}:00`,
              endTime: `${formData.endTime}:00`,
            },
          ],
        };
      }

      return {
        day: currentDay,
        windows: [],
      };
    }),
  };

  try {
    await api.put("/me/availability", payload);
    setMessage("Availability submitted successfully.");
    setFormData({
      day: "",
      startTime: "",
      endTime: "",
    });
  } catch (err) {
    console.error(err);
    setError("Failed to submit availability.");
  }
};


  return (
    <div>
      <Navbar role="employee" />

      <div style={styles.container}>
        <h1>Availability</h1>

        <form onSubmit={handleSubmit} style={styles.form}>
          <label style={styles.label}>Day</label>
          <select
            name="day"
            value={formData.day}
            onChange={handleChange}
            style={styles.input}
            required
          >
            <option value="">Select a day</option>
            <option value="Monday">Monday</option>
            <option value="Tuesday">Tuesday</option>
            <option value="Wednesday">Wednesday</option>
            <option value="Thursday">Thursday</option>
            <option value="Friday">Friday</option>
            <option value="Saturday">Saturday</option>
            <option value="Sunday">Sunday</option>
          </select>

          <label style={styles.label}>Start Time</label>
          <input
            type="time"
            name="startTime"
            value={formData.startTime}
            onChange={handleChange}
            style={styles.input}
            required
          />

          <label style={styles.label}>End Time</label>
          <input
            type="time"
            name="endTime"
            value={formData.endTime}
            onChange={handleChange}
            style={styles.input}
            required
          />

          <button type="submit" style={styles.button}>
            Submit Availability
          </button>
        </form>

        {message && <p style={styles.success}>{message}</p>}
        {error && <p style={styles.error}>{error}</p>}
      </div>
    </div>
  );
}

const styles = {
  container: {
    padding: "24px",
    maxWidth: "500px",
  },
  form: {
    display: "flex",
    flexDirection: "column",
    gap: "12px",
    marginTop: "20px",
  },
  label: {
    fontWeight: "bold",
  },
  input: {
    padding: "10px",
    fontSize: "16px",
  },
  button: {
    marginTop: "12px",
    padding: "12px",
    border: "none",
    cursor: "pointer",
    borderRadius: "6px",
    backgroundColor: "#2563eb",
    color: "white",
  },
  success: {
    color: "green",
    marginTop: "16px",
  },
  error: {
    color: "red",
    marginTop: "16px",
  },
};

export default AvailabilityPage;