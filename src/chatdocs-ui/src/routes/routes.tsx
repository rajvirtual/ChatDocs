import { Routes, Route } from "react-router";
import Chat from "../pages/chat/Chat";

const CreateRoutes = () => (
  <Routes>
    <Route path="/" element={<Chat />} />
    <Route path="/chat" element={<Chat />} />
  </Routes>
);

export default CreateRoutes;
