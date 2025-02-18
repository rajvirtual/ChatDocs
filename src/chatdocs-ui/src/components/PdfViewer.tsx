import React, { useEffect } from "react";
import { Worker, Viewer, SpecialZoomLevel } from "@react-pdf-viewer/core";
import { defaultLayoutPlugin } from "@react-pdf-viewer/default-layout";
import "@react-pdf-viewer/core/lib/styles/index.css";
import "@react-pdf-viewer/default-layout/lib/styles/index.css";

interface PdfViewerProps {
  pdfUrl: string;
  pageNumber: number;
}

const PdfViewer: React.FC<PdfViewerProps> = ({ pdfUrl, pageNumber }) => {
  const defaultLayoutPluginInstance = defaultLayoutPlugin();

  useEffect(() => {}, [pdfUrl, pageNumber]);

  if (!pdfUrl) {
    return <div>No PDF URL provided</div>;
  }

  return (
    <div className="pdf-viewer" style={{ height: "720px" }}>
      <Worker
        workerUrl={`https://unpkg.com/pdfjs-dist@3.11.174/build/pdf.worker.min.js`}
      >
        <Viewer
          fileUrl={pdfUrl}
          plugins={[defaultLayoutPluginInstance]}
          initialPage={pageNumber - 1}
          defaultScale={SpecialZoomLevel.ActualSize}
        />
      </Worker>
    </div>
  );
};

export default PdfViewer;
