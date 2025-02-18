import React, { useEffect, useRef } from "react";
import { XMarkIcon } from "@heroicons/react/24/outline";
import PdfViewer from "./PdfViewer";

interface PdfViewerModalProps {
  pdfUrl: string;
  pageNumber: number;
  onClose: () => void;
}

const PdfViewerModal: React.FC<PdfViewerModalProps> = ({
  pdfUrl,
  pageNumber,
  onClose,
}) => {
  useEffect(() => {
    // Handle Escape key
    const handleEsc = (e: KeyboardEvent) => {
      if (e.key === "Escape") onClose();
    };
    window.addEventListener("keydown", handleEsc);
    return () => window.removeEventListener("keydown", handleEsc);
  }, [onClose]);

  // Handle click outside
  const modalRef = useRef<HTMLDivElement>(null);
  const handleClickOutside = (e: React.MouseEvent) => {
    if (modalRef.current && !modalRef.current.contains(e.target as Node)) {
      onClose();
    }
  };
  return (
    <div
      className="fixed inset-0 bg-black/50 z-50 flex items-center justify-center"
      onClick={handleClickOutside}
    >
      <div
        ref={modalRef}
        className="bg-white w-[95vw] h-[90vh] rounded-lg flex flex-col relative"
      >
        <button
          onClick={onClose}
          className="absolute top-4 right-4 p-2 hover:bg-gray-100 rounded-full"
        >
          <XMarkIcon className="w-5 h-5" />
        </button>
        <div className="flex-1 p-4">
          <PdfViewer pdfUrl={pdfUrl} pageNumber={pageNumber} />
        </div>
      </div>
    </div>
  );
};

export default PdfViewerModal;
