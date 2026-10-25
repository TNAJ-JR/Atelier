import { Outlet } from "react-router";
function Dashboard() {
  return (
    <div className="d-flex justify-content-between">
      <div>
        <Outlet />
      </div>
    </div>
  );
}
export default Dashboard;
