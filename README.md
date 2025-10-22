# Financial Report Generator - Assessment Documentation

## Project Overview

This Word task pane add-in streamlines the creation of professional financial reports. Users fill out a simple form, and the system automatically generates a fully formatted Word document using C# and the OpenXML SDK. The application demonstrates a modern full-stack architecture combining frontend JavaScript, backend API integration, and server-side document generation.

---

## Recent Improvements

### Production-Ready HTTP Endpoint with .docx Download
The application now features a **complete end-to-end download flow** where the backend generates a real Word document in memory and streams it directly to the browser. The frontend automatically triggers the download with a properly named file, providing a seamless user experience.

### Full Keyboard Accessibility
The segmented control (report type selector) implements the **WAI-ARIA radiogroup pattern** with complete keyboard navigation:
- Arrow keys (←/→/↑/↓) to navigate between options
- Space/Enter to select an option
- Proper `aria-checked` states and `tabindex` management
- Visual focus indicators for keyboard users

These improvements ensure the application meets modern web standards for both functionality and accessibility.

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
The backend service acts as an orchestration layer between the user interface and the document generation engine. When it receives the JSON payload at the `/api/reports/generate` endpoint, it first validates all required fields to ensure nothing is missing or malformed (checking that clientName exists and is at least 2 characters, reportType is valid, and year is a proper integer). After validation passes, the backend uses the C# OpenXML SDK to construct a Word document **in memory** with proper formatting, headers, and metadata. The document is then **streamed directly to the client** as a downloadable .docx file with appropriate Content-Type and Content-Disposition headers, eliminating the need for file system storage.

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
- **File:** `frontend/index.html` - Clean, modern UI with accessible segmented controls for report types
- **File:** `frontend/report-generator.js` - Form validation, API integration, download handling, and Client History Manager
- **Key Features:**
  - Real-time form validation with error messages
  - Unique report ID generation using timestamp + random string
  - localStorage-based autocomplete with filtering
  - **Automatic .docx download** with blob handling and filename extraction
  - **Full keyboard navigation** for segmented control (arrow keys, space/enter)
  - **WAI-ARIA radiogroup pattern** with proper focus management
  - Loading states and success feedback

### Backend (Part 3)
- **File:** `backend/Program.cs` - ASP.NET Core minimal API using OpenXML SDK
- **Key Features:**
  - **HTTP endpoint** that returns actual .docx files as downloadable streams
  - Creates Word documents programmatically **in memory** (no file system needed)
  - Uses `DocumentFormat.OpenXml` for precise formatting control
  - Generates professional reports with styled titles and metadata
  - Sanitizes filenames to prevent invalid characters
  - Includes Content-Disposition headers for proper download behavior
  - CORS support for frontend integration

---

## How It Works (End-to-End Flow)

1. User opens the Word task pane and fills out the form
2. Frontend validates the data and creates a JSON payload
3. Payload is sent via `POST /api/reports/generate`
4. Backend validates the request (client name, report type, year)
5. C# OpenXML service creates a Word document **in memory** with formatting
6. Backend streams the .docx file to the frontend with proper headers
7. Frontend **automatically triggers browser download** with the generated filename
8. Success message displays with download confirmation
9. Client name is saved to localStorage for future autocomplete suggestions

---

## Accessibility Features

### Keyboard Navigation
The segmented control (report type selector) is fully keyboard accessible:
- **Tab** to focus the control group
- **Arrow keys** (←/→/↑/↓) to navigate between options
- **Space or Enter** to select an option
- Visual focus indicator (blue outline) for keyboard users

### Screen Reader Support
- `role="radiogroup"` on the container for proper semantic structure
- `role="radio"` on each option
- `aria-labelledby` linking the group to its label
- `aria-checked` states that update dynamically
- Proper tab index management (0 for selected, -1 for unselected)

This implementation follows the **WAI-ARIA 1.2 Radio Group Pattern** for maximum compatibility with assistive technologies.

---

## Running the Project

### Backend
Start the API server first:
```bash
cd backend
dotnet run
```
The server will start on `http://localhost:5000`

### Frontend
Open `frontend/index.html` in a browser (can use a local server or open the file directly)

### Testing the Download Flow
1. Fill out the form (select report type, year, and enter a client name)
2. Click "Generate Report"
3. The backend will generate a Word document and stream it to your browser
4. Your browser should automatically download the `.docx` file
5. Open the downloaded file in Microsoft Word to view the formatted report

### Testing Keyboard Accessibility
1. Press **Tab** to focus on the segmented control
2. Use **Arrow keys** to navigate between P&L, Balance Sheet, and Cash Flow
3. Press **Space** or **Enter** to select an option
4. Notice the visual focus indicator and smooth transitions
