# Financial Report Generator - Assessment Documentation

## Project Overview

This Word task pane add-in streamlines the creation of professional financial reports. Users fill out a simple form, and the system automatically generates a fully formatted Word document using C# and the OpenXML SDK. The application demonstrates a modern full-stack architecture combining frontend JavaScript, backend API integration, and server-side document generation.

---

## AI Agent Prompt/API Logic

### Service Endpoint
**Endpoint:** `POST /api/reports/generate`

This is the main API endpoint that receives report generation requests from the Word task pane.

### JSON Payload Structure
When a user submits the form, the frontend sends this JSON payload:

```json
{
  "reportType": "pl",
  "reportTypeName": "Profit & Loss",
  "year": 2024,
  "clientName": "Acme Corporation",
  "generatedDate": "2024-10-22T14:30:00.000Z",
  "reportId": "RPT-1729607400000-X7K9M2A"
}
```

### Backend Agent's Role
The backend service acts as an orchestration layer between the user interface and the document generation engine. When it receives the JSON payload at the `/api/reports/generate` endpoint, it first validates all required fields to ensure nothing is missing or malformed (checking that clientName exists and is at least 2 characters, reportType is valid, and year is a proper integer). After validation passes, the backend extracts the data and calls the C# OpenXML service, passing along the client name, report type, and year. The C# service then uses the OpenXML SDK to construct a Word document with proper formatting, headers, and metadata, ultimately saving it as a `.docx` file that can be returned to the user or stored for later retrieval.

---

## Architectural Explanation: Why C#/OpenXML on the Server?

The C# OpenXML SDK provides deep control over Word document structure that JavaScript simply can't match. While Office.js can insert basic content into an already-open document, it's limited when it comes to creating complex, fully formatted documents from scratch with precise control over styles, tables, headers, and document properties. Running the C# service on the server means we can generate complete, professional-grade Word documents with complex layouts, consistent formatting, and proper document metadata—all without being constrained by the browser's limitations or requiring the user to have Word open and configured in a specific way.

---

## Bonus Feature: Client History Manager with Autocomplete

### What It Does
The Client History Manager stores the last 5 client names or IDs that users have submitted in the browser's localStorage. When users click into the "Client Name or ID" field, a dropdown appears showing their recent clients. As they type, the list filters in real-time to match their query. Users can click any name to instantly fill the field, or they can click "Clear" to reset their history.

### Why It Adds Value
This feature eliminates repetitive typing for users who frequently generate reports for the same clients. Instead of retyping "Acme Corporation" every time, users simply click it from their history—saving time, reducing typos, and making the workflow feel more polished and professional.

---

## Technical Implementation Details

### Frontend (Part 1 & 2)
- **File:** `frontend/index.html` - Clean, modern UI with segmented controls for report types
- **File:** `frontend/report-generator.js` - Form validation, API integration, and Client History Manager
- **Key Features:**
  - Real-time form validation with error messages
  - Unique report ID generation using timestamp + random string
  - localStorage-based autocomplete with filtering
  - Loading states and success feedback

### Backend (Part 3)
- **File:** `backend/Program.cs` - C# console application using OpenXML SDK
- **Key Features:**
  - Creates Word documents programmatically
  - Uses `DocumentFormat.OpenXml` for precise formatting control
  - Generates professional reports with styled titles and metadata
  - Sanitizes filenames to prevent invalid characters

---

## How It Works (End-to-End Flow)

1. User opens the Word task pane and fills out the form
2. Frontend validates the data and creates a JSON payload
3. Payload is sent to `POST /api/reports/generate`
4. Backend validates the request and extracts parameters
5. C# OpenXML service creates a Word document with formatting
6. Document is saved and can be returned to the user
7. Client name is saved to localStorage for future autocomplete suggestions

---

## Running the Project

### Frontend
Open `frontend/index.html` in a browser

### Backend
```bash
cd backend
dotnet run
```

The C# application will generate sample reports demonstrating the OpenXML capabilities.
