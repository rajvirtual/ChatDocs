const MessageContent = ({ content }: { content: string }) => {
  const formatContent = (text: string) => {
    let formattedText = text;

    formattedText = formattedText.replace(
      /\[DocumentId=[^,]+,\s*/g, 
      '['
    );
    
    // Convert PDF links correctly
    formattedText = formattedText.replace(
      /\[Document: ([^,]+), Page: ([\d\-]+), Link: ([^\]]+)\]/g,
      (_, filename, page, url) => {
        return `<a href="${url}" target="_blank" class="text-blue-500 hover:text-blue-700 underline">
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

  return (
    <div
      className="prose dark:prose-invert max-w-none text-sm"
      dangerouslySetInnerHTML={{ __html: formatContent(content) }}
    />
  );
};

export default MessageContent;
