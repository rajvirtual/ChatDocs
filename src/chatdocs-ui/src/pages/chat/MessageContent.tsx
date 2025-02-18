import React, { useState } from "react";
import "@react-pdf-viewer/core/lib/styles/index.css";
import "@react-pdf-viewer/default-layout/lib/styles/index.css";
import PdfViewerModal from "../../components/PdfViewerModal";

const MessageContent = ({ content }: { content: string }) => {
  const [pdfModal, setPdfModal] = useState<{
    url: string;
    page: number;
  } | null>(null);

  const formatContent = (text: string) => {
    let formattedText = text;

    formattedText = formattedText.replace(/\[DocumentId=[^,]+,\s*/g, "[");

    formattedText = formattedText.replace(
      /\[Document: ([^,]+), Page: ([\d\-]+), Link: ([^\]]+)\]/g,
      (_, filename, page, url) => {
        return `<a href="#" class="text-blue-500 hover:text-blue-700 underline" data-filename="${filename}" data-page="${page}" data-url="${url}">
                    [${filename}, Page: ${page}]
                  </a>`;
      }
    );

    // Preserve line breaks
    formattedText = formattedText.replace(/\n\n/g, "</p><p>"); // Paragraphs
    formattedText = formattedText.replace(/\n/g, "<br />"); // Single line breaks

    // Convert bullet points to list
    formattedText = formattedText.replace(/- (.*?)(\n|$)/g, "<li>$1</li>");
    formattedText = formattedText.replace(/(<li>.*?<\/li>)+/g, "<ul>$&</ul>"); // Wrap in <ul>

    // Convert code formatting
    formattedText = formattedText.replace(/`([^`]+)`/g, "<code>$1</code>"); // Inline code
    formattedText = formattedText.replace(
      /```([^`]+)```/gs,
      "<pre><code>$1</code></pre>"
    ); // Multi-line code

    // Bold & Italics
    formattedText = formattedText.replace(
      /\*\*(.*?)\*\*/g,
      "<strong>$1</strong>"
    ); // Bold
    formattedText = formattedText.replace(/_(.*?)_/g, "<em>$1</em>"); // Italics

    return formattedText;
  };

  const handlePdfClick = async (event: React.MouseEvent<HTMLAnchorElement>) => {
    event.preventDefault();
    let target = event.currentTarget;
    var htmlElement = target.getElementsByTagName("a")[0];
    if (htmlElement) {
      target = htmlElement;
    }
    const fileName = target.getAttribute("data-filename");
    const page = parseInt(target.getAttribute("data-page") || "1", 10);
    const url = target.getAttribute("data-url");

    if (fileName && url) {
      try {
        const response = await fetch(
          `${
            import.meta.env.VITE_API_BASE_URL
          }/documents/pdfSasUri/?blobName=${fileName}`
        );
        if (!response.ok) {
          throw new Error("Failed to fetch SAS URI");
        }
        const data = await response.json();
        setPdfModal({
          url: data.sasUri,
          page: page,
        });
      } catch (error) {
        console.error("Error fetching SAS URI:", error);
      }
    }
  };

  return (
    <>
      <div
        className="prose dark:prose-invert max-w-none text-sm"
        dangerouslySetInnerHTML={{ __html: formatContent(content) }}
        onClick={(event) => {
          const target = event.target as HTMLElement;
          if (target.tagName === "A" && target.getAttribute("data-url")) {
            handlePdfClick(
              event as unknown as React.MouseEvent<HTMLAnchorElement>
            );
          }
        }}
      />
      {pdfModal && (
        <PdfViewerModal
          pdfUrl={pdfModal.url}
          pageNumber={pdfModal.page}
          onClose={() => setPdfModal(null)}
        />
      )}
    </>
  );
};

export default MessageContent;
