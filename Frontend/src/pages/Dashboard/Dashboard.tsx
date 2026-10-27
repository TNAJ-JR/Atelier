import { Button } from "../../components/ui";

function Dashboard() {
  return (
    <div className="d-flex flex-column justify-content-around">
      <div className="d-flex justify-content-between align-items-center">
        <h1>Tableau de bord</h1>
      </div>
      <div className="d-flex flex-column justify-content-between">
        <Button>Nouvelle commande</Button>
        <div></div>
      </div>
    </div>
  );
}
export default Dashboard;
