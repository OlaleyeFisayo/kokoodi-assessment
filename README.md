# Financial Report Generator - Architecture Documentation

## Overview

This application helps users generate professional financial reports directly from a Word task pane. Instead of manually creating documents, users can simply fill out a form, and the system automatically generates a properly formatted Word document with their financial data.

---

## Service Endpoint & API Logic

**Endpoint:** `POST /api/reports/generate`

When the user clicks "Generate Report" in the Word task pane, the frontend sends a JSON payload to this endpoint. The payload includes everything needed to create the report: the client name, report type (like Profit & Loss or Balance Sheet), the year, and a unique report ID. The backend service receives this request, validates the data to make sure nothing is missing or incorrect, and then passes it along to the C# OpenXML service. That service uses the information to build an actual Word document with proper formatting, headers, and the financial data structured just right.

---

## Why C# on the Server Instead of JavaScript in the Task Pane?

The OpenXML SDK is a powerful Microsoft library built specifically for C# that lets you create complex Word documents with precise control over formatting, tables, and structure. While Office.js (the JavaScript library) can insert content into an open document, it's limited in what it can create from scratch and doesn't give you the same level of control. Running the C# service on the server means we can generate complete, professionally formatted documents that users can download or open directly, without being restricted by what JavaScript can do inside the task pane.

---

## What This Feature Does & Why It Matters

**The Feature:** Users select a report type, enter a client name and year in the task pane, then click one button to generate a complete financial report document.

**Why It Adds Value:** This saves time and reduces errors by automating what would otherwise be tedious manual formatting work. Instead of spending 20 minutes building a report template and filling in data, users get a polished, ready-to-use document in seconds.
