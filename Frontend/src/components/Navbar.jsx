import { NavLink, useNavigate } from "react-router-dom";

// Shared navigation bar component
function Navbar({ role }) {
  const navigate = useNavigate();

  const handleLogout = () => {
    localStorage.removeItem("token");
    localStorage.removeItem("role");
    navigate("/");
  };

  return (
    <nav style={styles.nav}>
      <div style={styles.logo}>WalForce</div>

      <div style={styles.links}>
        {role === "employee" && (
          <>
            <NavLink
              to="/employee"
              style={({ isActive }) =>
                isActive ? { ...styles.link, ...styles.activeLink } : styles.link
              }
            >
              Employee Home
            </NavLink>

            <NavLink
              to="/employee/schedule"
              style={({ isActive }) =>
                isActive ? { ...styles.link, ...styles.activeLink } : styles.link
              }
            >
              My Schedule
            </NavLink>

            <NavLink
              to="/employee/availability"
              style={({ isActive }) =>
                isActive ? { ...styles.link, ...styles.activeLink } : styles.link
              }
            >
              Availability
            </NavLink>
          </>
        )}
      </div>

      <button onClick={handleLogout} style={styles.button}>
        Logout
      </button>
    </nav>
  );
}

const styles = {
  nav: {
    display: "flex",
    justifyContent: "space-between",
    alignItems: "center",
    padding: "16px 24px",
    backgroundColor: "#1f2937",
    color: "white",
  },
  logo: {
    fontSize: "20px",
    fontWeight: "bold",
  },
  links: {
    display: "flex",
    gap: "16px",
  },
  link: {
    color: "white",
    textDecoration: "none",
    padding: "8px 12px",
    borderRadius: "6px",
  },
  activeLink: {
    backgroundColor: "#374151",
  },
  button: {
    backgroundColor: "#ef4444",
    color: "white",
    border: "none",
    padding: "8px 12px",
    borderRadius: "6px",
    cursor: "pointer",
  },
};

export default Navbar;