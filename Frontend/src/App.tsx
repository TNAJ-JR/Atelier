import { Navigate, Route, Routes } from "react-router";
import LoginPage from "./pages/Auth/Login/LoginPage";
import ProfilePage from "./pages/Profile/ProfilePage";

function App() {
  return (
    <Routes>
      <Route path="/login" element={<LoginPage />} />
      <Route path="/profile" element={<ProfilePage />} />
      <Route path="*" element={<Navigate to="/login" replace />} />
    </Routes>
  );
}

export default App;
