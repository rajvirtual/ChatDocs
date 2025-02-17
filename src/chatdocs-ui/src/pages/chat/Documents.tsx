import React, {
  useEffect,
  useState,
  useRef,
  useImperativeHandle,
  forwardRef,
} from "react";
import Loader from "../../components/Loader";
import { TrashIcon } from "@heroicons/react/24/outline";

interface Document {
  documentId: string;
  documentName: string;
}

interface DocumentsProps {}

export interface DocumentsRef {
  openFileDialog: () => void;
}

const Documents = forwardRef<DocumentsRef, DocumentsProps>((_, ref) => {
  const [documents, setDocuments] = useState<Document[]>([]);
  const [isDataLoading, setIsDataLoading] = useState<boolean>(false);
  const fileInputRef = useRef<HTMLInputElement>(null);

  useImperativeHandle(ref, () => ({
    openFileDialog: () => {
      if (fileInputRef.current) {
        fileInputRef.current.click();
      }
    },
  }));

  const handleFileChange = async (
    event: React.ChangeEvent<HTMLInputElement>
  ) => {
    const file = event.target.files?.[0];
    if (file) {
      const formData = new FormData();
      formData.append("file", file);

      try {
        setIsDataLoading(true);
        const response = await fetch(
          `${import.meta.env.VITE_API_BASE_URL}/documents`,
          {
            method: "POST",
            body: formData,
          }
        );

        if (response.ok) {
          let data: Document = await response.json();

          const newDocument: Document = {
            documentId: data.documentId,
            documentName: data.documentName,
          };
          setDocuments([...documents, newDocument]);
        } else {
          console.error("Error uploading file:", response);
        }
      } catch (error) {
        console.error("Error uploading file:", error);
      } finally {
        setIsDataLoading(false);
      }
    }
  };

  const deleteDocument = (id: string) => {
    const confirmDelete = window.confirm("Are you sure you want to delete this document?");
    if (!confirmDelete) {
      return;
    } 
    setIsDataLoading(true);
    fetch(`${import.meta.env.VITE_API_BASE_URL}/documents/${id}`, {
      method: "DELETE",
    })
      .then((response) => {
        if (response.ok) {
          setDocuments(documents.filter((doc) => doc.documentId !== id));
        } else {
          console.error("Error deleting file:", response);
        }
      })
      .catch((error) => {
        console.error("Error deleting file:", error);
      })
      .finally(() => {
        setIsDataLoading(false);
      });
  };

  useEffect(() => {
    const fetchData = async () => {
      setIsDataLoading(true);
      try {
        const response = await fetch(
          `${import.meta.env.VITE_API_BASE_URL}/documents`
        );
        if (response.ok) {
          const data: any = await response.json();
          setDocuments(data);
        } else {
          console.error("Error Loading Files:", response);
        }
      } catch (error) {
        console.error("Error Loading Files:", error);
      } finally {
        setIsDataLoading(false);
      }
    };
    fetchData();
  }, []);

  return (
    <div className="relative">
      <div className="mb-4">
        <input
          type="file"
          ref={fileInputRef}
          className="hidden"
          onChange={handleFileChange}
          accept=".pdf"
        />
        <button
          className="bg-blue-500 hover:bg-blue-700 text-white font-bold py-2 px-4 rounded hidden"
          onClick={() => fileInputRef.current?.click()}
        ></button>
      </div>
      <table className="min-w-full divide-y divide-gray-200">
        <tbody className="bg-white divide-y divide-gray-200">
          {documents.map((doc: Document) => (
            <tr key={doc.documentId}>
              <td className="px-6 py-4 whitespace-nowrap text-sm font-medium text-gray-900">
                {doc.documentName}
              </td>
              <td className="px-6 py-4 whitespace-nowrap text-sm font-medium text-gray-900">
                <TrashIcon
                  className="h-5 w-5 text-[#d97818] cursor-pointer"
                  title="Delete Document"
                  onClick={() => deleteDocument(doc.documentId)}
                />
              </td>
            </tr>
          ))}
        </tbody>
      </table>
      {isDataLoading && (
        <div className="absolute top-0 left-0 right-0 bottom-0 flex items-center justify-center bg-white/50">
          <Loader className="w-6 h-6" />
        </div>
      )}
    </div>
  );
});

export default Documents;
