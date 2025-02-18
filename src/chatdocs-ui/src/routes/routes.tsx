import { Routes, Route } from "react-router";
import Chat from "../pages/chat/Chat";
import PdfViewer from "../components/PdfViewer";

const CreateRoutes = () => (
  <Routes>
    <Route path="/" element={<Chat />} />
    <Route path="/chat" element={<Chat />} />
    <Route
      path="/pdf-viewer"
      element={<PdfViewer pdfUrl="" pageNumber={1} />}
    />
  </Routes>
);

export default CreateRoutes;
