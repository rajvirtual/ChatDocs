import { BrowserRouter } from "react-router";
import "./App.css";
import Header from "./components/Header";
import CreateRoutes from "./routes/routes";

function App() {
  return (
    <>
      <BrowserRouter>
        <Header />
        <main className="flex justify-center w-full bg-[#f3efeb]  overflow-hidden h-[calc(100vh-64px)]">
          <CreateRoutes />
        </main>
      </BrowserRouter>
    </>
  );
}

export default App;
