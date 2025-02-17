import { useState, useEffect, useRef, KeyboardEventHandler } from "react";
import MessageContent from "./MessageContent";
import "./Chat.css";
import Documents from "./Documents";
import { PlusIcon, UserIcon, ChatBubbleLeftEllipsisIcon  } from "@heroicons/react/24/outline";

interface Message {
  role: "user" | "assistant" | "system";
  content: string;
}

const Chat = () => {
  const messagesEndRef = useRef<HTMLDivElement>(null);

  const [messages, setMessages] = useState<Message[]>([]);
  const [userInput, setUserInput] = useState("");
  const [isLoading, setIsLoading] = useState(false);
  const documentsRef = useRef<{ openFileDialog: () => void }>(null);

  useEffect(() => {
    messagesEndRef.current?.scrollIntoView({ behavior: "smooth" });
  }, [messages]);

  const handleKeyDown: KeyboardEventHandler<HTMLTextAreaElement> = async (
    e
  ) => {
    if (e.key === "Enter" && !e.shiftKey) {
      e.preventDefault();
      handleSubmit();
    }
  };

  const handleSubmit = async () => {
    let hasReceivedContent = false;
    if (!userInput.trim() || isLoading) {
      return;
    }
    try {
      setIsLoading(true);
      setUserInput("");
      setMessages((prev) => [
        ...prev,
        {
          role: "user",
          content: userInput,
        },
      ]);

      setMessages((prev) => [
        ...prev,
        {
          role: "assistant",
          content: "",
        },
      ]);

      const response = await fetch(
        `${import.meta.env.VITE_API_BASE_URL}/chat`,
        {
          method: "POST",
          headers: {
            "Content-Type": "application/json",
            Accept: "text/plain",
          },
          body: JSON.stringify({ userQuery: userInput }),
        }
      );

      if (!response.ok) {
        throw new Error(
          `Failed to get response from the server ${response.status}`
        );
      }

      const reader = response.body?.getReader();
      if (!reader) {
        throw new Error("Failed to get response body reader");
      }
      const decoder = new TextDecoder();
      let assistantMessage = "";

      try {
        while (true) {
          const { done, value } = await reader.read();
          if (done) {
            break;
          }
          const chunk = decoder.decode(value);
          const lines = chunk.split("\n\n");
          for (const line of lines) {
            if (line.trim() == "[DONE]") {
              continue;
            }
            if (line.trim() == "[CANCELLED]") {
              setMessages((prev) => [
                ...prev,
                {
                  role: "system",
                  content: "The request was cancelled.",
                },
              ]);
              return;
            }

            assistantMessage += line;
            setMessages((prev) => [
              ...prev.slice(0, -1),
              {
                role: "assistant",
                content: assistantMessage,
              },
            ]);
            hasReceivedContent = true;
          }
        }
      } catch (readError) {
        if (!hasReceivedContent) {
          console.error("Error reading stream:", readError);
          throw readError;
        }
      } finally {
        reader.releaseLock();
      }
    } catch (error) {
      if (!hasReceivedContent) {
        console.error("Error:", error);
        setMessages((prev) => [
          ...prev,
          {
            role: "system",
            content: "An error occurred. Please try again later.",
          },
        ]);
      }
    } finally {
      setIsLoading(false);
    }
  };

  return (
    <div className="flex p-4 justify-center w-full max-w-7xl mx-auto bg-[#f3efeb]">
      {/* Left Panel */}
      <div className="flex flex-col w-2/3 h-full bg-gray-900 rounded-2xl shadow-lg text-gray-900 mr-4">
        <div className="flex-grow max-h-[calc(100vh-160px)] p-4 overflow-y-auto bg-gray-900 border border-gray-300 rounded-t-lg">
          {messages.length > 0 ? (
            messages.map(
              (msg, index) =>
                msg.content.trim() !== "" && (
                  <div
                    key={index}
                    className={`p-4 mb-4 rounded-lg flex items-center ${
                      msg.role === "user"
                        ? "bg-gray-800 text-white ml-auto max-w-[80%]"
                        : "bg-gray-800 text-white mr-auto max-w-[80%]"
                    }`}
                  >
                    {msg.role === "user" ? (
                      <UserIcon className="h-5 w-5 text-[#d97818] mr-2" />
                    ) : (
                      <ChatBubbleLeftEllipsisIcon className="h-5 w-5 text-[#d97818] mr-2" />
                    )}
                    <MessageContent content={msg.content} />
                  </div>
                )
            )
          ) : (
            <p className="text-gray-400">
              No messages yet. Start the conversation!
            </p>
          )}
          <div ref={messagesEndRef} />
        </div>

        {/* Input Section */}
        <form onSubmit={handleSubmit}>
          <div className="flex items-center h-16 p-4 bg-gray-50 border-t border-gray-300 rounded-b-2xl">
            <textarea
              className="flex-grow h-10 p-2 mr-2 text-sm border border-gray-300 rounded-2xl resize-none focus:outline-none focus:ring-2 focus:ring-gray-500 text-gray-900 bg-gray-100"
              placeholder="Ask the AI assistant..."
              onKeyDown={handleKeyDown}
              value={userInput}
              onChange={(e) => setUserInput(e.target.value)}
            ></textarea>
          </div>
        </form>
      </div>

      {/* Right Panel */}
      <div className="flex flex-col w-1/3 h-full bg-white rounded-2xl shadow-lg text-gray-900 border-l border-gray-300">
        <div className="flex-grow p-2 overflow-y-auto">
          {/* Small Panel with Heading */}
          <div className="p-2 w-full flex items-center border-b border-blue-400">
            <h2 className="text-lg font-semibold text-gray-900 mr-2">
              Documents
            </h2>
            <button
              className="ml-2 bg-[#d97818] hover:bg-[#b56514] text-white rounded-full p-2 shadow-lg"
              onClick={() => documentsRef.current?.openFileDialog()}
              title="Upload Document"
            >
              <PlusIcon className="h-3 w-3" />
            </button>{" "}
          </div>
          {/* Documents */}
          <Documents ref={documentsRef} />
        </div>
      </div>
    </div>
  );
};

export default Chat;
