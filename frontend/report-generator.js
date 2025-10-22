// Client history autocomplete with localStorage
class ClientHistoryManager {
 constructor() {
  this.storageKey = 'financialReports_clientHistory';
  this.maxHistorySize = 5;
 }

 getRecentClients() {
  try {
   const history = localStorage.getItem(this.storageKey);
   return history ? JSON.parse(history) : [];
  } catch (error) {
   console.error('Error reading client history:', error);
   return [];
  }
 }

 saveClient(clientName) {
  if (!clientName || clientName.trim().length < 2) return;

  try {
   let history = this.getRecentClients();

   // Move to front if already exists
   history = history.filter(name => name.toLowerCase() !== clientName.toLowerCase());
   history.unshift(clientName.trim());
   history = history.slice(0, this.maxHistorySize);

   localStorage.setItem(this.storageKey, JSON.stringify(history));
  } catch (error) {
   console.error('Error saving client history:', error);
  }
 }

 filterClients(query) {
  if (!query || query.trim().length === 0) {
   return this.getRecentClients();
  }

  const lowercaseQuery = query.toLowerCase();
  return this.getRecentClients().filter(client =>
   client.toLowerCase().includes(lowercaseQuery)
  );
 }

 clearHistory() {
  try {
   localStorage.removeItem(this.storageKey);
  } catch (error) {
   console.error('Error clearing client history:', error);
  }
 }
}

const clientHistory = new ClientHistoryManager();

// Collect and validate form data before sending to backend
function prepareAgentRequest() {
 const reportTypeElement = document.querySelector('input[name="reportType"]:checked');
 const reportYear = document.getElementById('reportYear').value;
 const clientName = document.getElementById('clientName').value.trim();

 clearErrors();
 let isValid = true;

 if (!reportTypeElement) {
  isValid = false;
 }

 if (!reportYear) {
  showError('reportYear', 'yearError');
  isValid = false;
 }

 if (!clientName || clientName.length < 2) {
  showError('clientName', 'clientError');
  document.getElementById('clientError').textContent =
   clientName.length === 0
    ? 'Please enter a client name or ID'
    : 'Client name must be at least 2 characters';
  isValid = false;
 }

 if (!isValid) return null;

 const reportTypeNames = {
  'pl': 'Profit & Loss',
  'balance_sheet': 'Balance Sheet',
  'cash_flow': 'Cash Flow Statement'
 };

 return {
  reportType: reportTypeElement.value,
  reportTypeName: reportTypeNames[reportTypeElement.value],
  year: parseInt(reportYear, 10),
  clientName: clientName,
  generatedDate: new Date().toISOString(),
  reportId: generateReportId()
 };
}

function generateReportId() {
 const timestamp = Date.now();
 const random = Math.random().toString(36).substring(2, 9).toUpperCase();
 return `RPT-${timestamp}-${random}`;
}

function showError(fieldId, errorId) {
 const field = document.getElementById(fieldId);
 const error = document.getElementById(errorId);

 if (field) field.classList.add('error');
 if (error) error.classList.add('show');
}

function clearErrors() {
 document.querySelectorAll('.error').forEach(el => el.classList.remove('error'));
 document.querySelectorAll('.error-message.show').forEach(el => el.classList.remove('show'));
 document.getElementById('successMessage').classList.remove('show');
}

function showSuccess(reportData) {
 const successMessage = document.getElementById('successMessage');
 const successText = document.getElementById('successText');

 successText.textContent = `${reportData.reportTypeName} for ${reportData.clientName} (${reportData.year}) • ${reportData.reportId}`;
 successMessage.classList.add('show');
}

// Send report request to backend API and download the file
async function sendToBackend(reportData) {
 const apiEndpoint = 'http://localhost:5000/api/reports/generate';

 try {
  const response = await fetch(apiEndpoint, {
   method: 'POST',
   headers: {
    'Content-Type': 'application/json'
   },
   body: JSON.stringify(reportData)
  });

  if (!response.ok) {
   const errorData = await response.json().catch(() => ({}));
   throw new Error(
    errorData.message || `HTTP error! status: ${response.status}`
   );
  }

  // Get the blob (Word document)
  const blob = await response.blob();

  // Extract filename from Content-Disposition header if available
  const contentDisposition = response.headers.get('Content-Disposition');
  let filename = `Report_${reportData.clientName}_${reportData.year}.docx`;

  if (contentDisposition) {
   const filenameMatch = contentDisposition.match(/filename="?(.+)"?/);
   if (filenameMatch) {
    filename = filenameMatch[1];
   }
  }

  // Create a download link and trigger download
  const url = window.URL.createObjectURL(blob);
  const a = document.createElement('a');
  a.style.display = 'none';
  a.href = url;
  a.download = filename;
  document.body.appendChild(a);
  a.click();

  // Cleanup
  window.URL.revokeObjectURL(url);
  document.body.removeChild(a);

  return { success: true, filename };
 } catch (error) {
  console.error('Error sending data to backend:', error);
  throw error;
 }
}

// Form submission handler
document.getElementById('reportForm').addEventListener('submit', async function (e) {
 e.preventDefault();

 const reportData = prepareAgentRequest();
 if (!reportData) return;

 const btn = document.getElementById('generateBtn');
 const originalText = btn.textContent;

 try {
  btn.disabled = true;
  btn.innerHTML = '<span class="spinner"></span>Generating...';

  console.log('Report request:', JSON.stringify(reportData, null, 2));

  // Call backend API and download the file
  const response = await sendToBackend(reportData);

  // Update success message with download confirmation
  const successMessage = document.getElementById('successMessage');
  const successText = document.getElementById('successText');
  successText.textContent = `${reportData.reportTypeName} for ${reportData.clientName} (${reportData.year}) • Download started: ${response.filename}`;
  successMessage.classList.add('show');

  clientHistory.saveClient(reportData.clientName);
  document.getElementById('clientName').value = '';

  setTimeout(() => {
   document.getElementById('successMessage').classList.remove('show');
  }, 7000);

 } catch (error) {
  alert('Failed to generate report. Please try again.');
  console.error('Error:', error);
 } finally {
  btn.disabled = false;
  btn.textContent = originalText;
 }
});

document.getElementById('clientName').addEventListener('blur', function () {
 const value = this.value.trim();
 if (value && value.length < 2) {
  showError('clientName', 'clientError');
  document.getElementById('clientError').textContent = 'Client name must be at least 2 characters';
 }
});

document.getElementById('clientName').addEventListener('input', function () {
 if (this.classList.contains('error')) {
  this.classList.remove('error');
  document.getElementById('clientError').classList.remove('show');
 }
 updateAutocompleteSuggestions(this.value);
});

function updateAutocompleteSuggestions(query) {
 const dropdown = document.getElementById('autocompleteDropdown');
 const suggestionsContainer = document.getElementById('autocompleteSuggestions');
 const emptyState = document.getElementById('autocompleteEmpty');

 const filteredClients = clientHistory.filterClients(query);
 suggestionsContainer.innerHTML = '';

 if (filteredClients.length === 0) {
  emptyState.classList.add('show');
  suggestionsContainer.style.display = 'none';
 } else {
  emptyState.classList.remove('show');
  suggestionsContainer.style.display = 'block';

  filteredClients.forEach(clientName => {
   const item = document.createElement('div');
   item.className = 'autocomplete-item';
   item.textContent = clientName;
   item.addEventListener('click', () => {
    document.getElementById('clientName').value = clientName;
    hideAutocomplete();
    document.getElementById('clientName').classList.remove('error');
    document.getElementById('clientError').classList.remove('show');
   });
   suggestionsContainer.appendChild(item);
  });
 }

 if (filteredClients.length > 0 || clientHistory.getRecentClients().length > 0) {
  dropdown.classList.add('show');
 } else {
  dropdown.classList.remove('show');
 }
}

function hideAutocomplete() {
 document.getElementById('autocompleteDropdown').classList.remove('show');
}

function showAutocomplete() {
 updateAutocompleteSuggestions(document.getElementById('clientName').value);
}

document.getElementById('clientName').addEventListener('focus', showAutocomplete);

document.getElementById('clientName').addEventListener('blur', function () {
 setTimeout(hideAutocomplete, 200);
});

document.getElementById('clearHistoryBtn').addEventListener('click', function (e) {
 e.preventDefault();
 e.stopPropagation();
 if (confirm('Clear all recent client history?')) {
  clientHistory.clearHistory();
  updateAutocompleteSuggestions('');
 }
});

document.addEventListener('DOMContentLoaded', function () {
 const recentClients = clientHistory.getRecentClients();
 if (recentClients.length > 0) {
  console.log('Client history loaded:', recentClients.length, 'recent clients');
 }

 // Setup keyboard navigation for segmented control (radiogroup)
 setupRadioGroupKeyboardNavigation();
});

// Keyboard navigation for segmented control
function setupRadioGroupKeyboardNavigation() {
 const labels = document.querySelectorAll('.segmented-control label[role="radio"]');
 const radios = document.querySelectorAll('input[name="reportType"]');

 labels.forEach((label, index) => {
  // Handle keyboard navigation on labels
  label.addEventListener('keydown', (e) => {
   let targetIndex;

   if (e.key === 'ArrowRight' || e.key === 'ArrowDown') {
    e.preventDefault();
    targetIndex = (index + 1) % labels.length;
   } else if (e.key === 'ArrowLeft' || e.key === 'ArrowUp') {
    e.preventDefault();
    targetIndex = (index - 1 + labels.length) % labels.length;
   } else if (e.key === ' ' || e.key === 'Enter') {
    e.preventDefault();
    radios[index].checked = true;
    updateRadioGroupState();
    return;
   } else {
    return;
   }

   radios[targetIndex].checked = true;
   labels[targetIndex].focus();
   updateRadioGroupState();
  });

  // Handle click on labels
  label.addEventListener('click', () => {
   radios[index].checked = true;
   updateRadioGroupState();
  });
 });

 // Also handle change events on the actual radio inputs
 radios.forEach(radio => {
  radio.addEventListener('change', updateRadioGroupState);
 });

 // Initialize the state
 updateRadioGroupState();
}

function updateRadioGroupState() {
 const radios = document.querySelectorAll('input[name="reportType"]');
 const labels = document.querySelectorAll('.segmented-control label[role="radio"]');

 radios.forEach((radio, index) => {
  const isChecked = radio.checked;
  radio.setAttribute('aria-checked', isChecked.toString());
  labels[index].setAttribute('aria-checked', isChecked.toString());
  labels[index].tabIndex = isChecked ? 0 : -1;
 });
}