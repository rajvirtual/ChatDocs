# ChatDocs - AI-Powered Search for Team Documentation

ChatDocs is an AI-powered search tool for team documentation, enabling efficient retrieval and augmented generation of information using Retrieval-Augmented Generation (RAG).

## Overview

This application leverages the power of RAG to perform semantic searches across team documentation. It uses OpenAI for generating embeddings and performing semantic searches.

## Technologies Used

- **Frontend**: React + Vite
- **Backend**: .NET 9.0
- **Vector Database**: Azure Cosmos DB
- **Embeddings and Semantic Search**: OpenAI

## Features

- **Semantic Search**: Perform advanced searches across documentation using AI-generated embeddings.
- **Document Upload**: Upload documents to be indexed and searched.
- **Document Management**: List and delete documents from the database.

## Getting Started

### Prerequisites

- Node.js
- .NET 9.0 SDK
- Azure Cosmos DB account
- OpenAI API key

### Installation

1. **Clone the repository**:

   ```sh
   git clone https://github.com/your-repo/chatdocs.git
   cd chatdocs
   ```

2. **Install frontend dependencies**:

   ```sh
   cd src/chatdocs-ui
   npm install
   ```

3. **Install backend dependencies**:
   ```sh
   cd ../ChatDocsBackEnd
   dotnet restore
   ```

### Configuration

1. **Frontend**:

   - Update the `.env.local` file in [chatdocs-ui](http://_vscodecontentref_/0) with your API base URL.

2. **Backend**:
   - Update the `appsettings.json` file in [ChatDocsBackEnd](http://_vscodecontentref_/1) with your Azure Cosmos DB and OpenAI API credentials.

### Running the Application

1. **Start the backend**:

   ```sh
   cd src/ChatDocsBackEnd
   dotnet run
   ```

2. **Start the frontend**:
   ```sh
   cd ../chatdocs-ui
   npm run dev
   ```

## License

This project is licensed under the MIT License.
