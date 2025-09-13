// A simple object to hold all your data and application state
const state = {
    bills: [],
    accountInfo: {},
    expectedIncome: [],
    actualTransactions: []
};
const FETCH_TIMEOUT = 5000; // 5 seconds

// Main function to fetch all data and render the bill list page
async function renderBillList() {
    try {
        // Step 1: Fetch the HTML for the bill list view
        const viewResponse = await fetch('/BillsApp/views/bill-list.html');
        if (!viewResponse.ok) throw new Error('Failed to load bill list view.');
        const viewHtml = await viewResponse.text();

        // Step 2: Inject the HTML into the main container
        document.getElementById('app-container').innerHTML = viewHtml;

        const billsTableBody = document.querySelector("#billsTable tbody");
        billsTableBody.innerHTML = `<tr><td colspan="3" class="statusRow">Loading bills...</td></tr>`;

        // Step 3: Use Promise.race to handle timeout and network request
        const controller = new AbortController();
        const signal = controller.signal;

        const networkFetch = fetch('/api/dashboard', { signal });
        const timeout = new Promise((_, reject) => {
            setTimeout(() => {
                controller.abort();
                reject(new Error('timeout'));
            }, FETCH_TIMEOUT);
        });

        const response = await Promise.race([networkFetch, timeout]);

        if (response.status === 200) {
            const dashboardData = await response.json();
            renderDashboardData(dashboardData);
            storeDataInIndexedDB(dashboardData);
        } else {
            throw new Error('Failed to fetch dashboard data.');
        }

    } catch (error) {
        console.log('Network request timed out. Loading from IndexedDB.');
        document.querySelector("#pagetitle").innerHTML += " (Offline)";
        loadDataFromIndexedDB();
    }
}

function loadDataFromIndexedDB() {
    // Attempt to load from IndexedDB first
    const request = indexedDB.open('BillsDB', 1);

    request.onsuccess = (event) => {
        const db = event.target.result;
        const transaction = db.transaction('bills', 'readonly');
        const store = transaction.objectStore('bills');
        const getRequest = store.get(1); // Retrieve the record with key '1'

        getRequest.onsuccess = () => {
            if (getRequest.result) {
                // If data is found, render the chart immediately
                const data = getRequest.result.content;
                renderDashboardData(data); // A new function to encapsulate chart rendering logic
                console.log('Loaded data from IndexedDB.');
            }
        };
    };
}

function renderDashboardData(dashboardData) {
    const billsTableBody = document.querySelector("#billsTable tbody");
    
    // Update the state with the combined data from the DashboardController
    state.accountInfo = dashboardData.latestBalance;
    state.bills = dashboardData.bills;
    state.actualTransactions = dashboardData.monthlyIncome;
    state.expectedIncome = dashboardData.expectedIncome;

    // calculate the bill total
    const totalDue = state.bills.reduce((sum, bill) => sum + bill.amount, 0);
    const totalPayed = state.bills.filter(b => b.payed === true).reduce((sum, bill) => sum + bill.amount, 0);
    const otherSpending = dashboardData.monthlySpendingTotal - totalPayed;

    const { totalActual, totalExpected, reconciledTotal } = getReconciledIncome();
    const remaining = reconciledTotal - totalDue - otherSpending;

    const now = new Date();
    const lastUpdatedAccount = new Date(state.accountInfo.updated + "Z");
    const accountAmountClass = (now.getTime() - lastUpdatedAccount.getTime() > 7200000) ? "color-gray" : "color-ok";

    // Step 4: Populate the dynamic content of the page
    document.getElementById("balance").innerHTML = `<span class="${accountAmountClass}">$${state.accountInfo.amount.toFixed(2)}</span>`;
    document.getElementById("updated").innerHTML = `<span class="${accountAmountClass}">${lastUpdatedAccount.toLocaleString()}</span>`;
    document.getElementById("billtotal").innerHTML = formatDollarAmount(totalDue);
    document.getElementById("remain").innerHTML = formatDollarAmount(remaining);
    document.getElementById("expected-income").innerHTML = formatDollarAmount(totalExpected);
    document.getElementById("actual-income").innerHTML = formatDollarAmount(totalActual);
    document.getElementById("otherspending").innerHTML = formatDollarAmount(otherSpending);

    // Step 5: Render the bills table
    billsTableBody.innerHTML = state.bills.map(renderBillRow).join('');

    // Step 6: Attach event listeners to the dynamically created elements
    attachEventListeners();
}

// All other functions (`formatDollarAmount`, `getReconciledIncome`, `calculateExpectedIncome`, `getBillStatus`, `renderBillRow`, `attachEventListeners`, `renderEditForm`, `submitEditForm`, `handleRouting`, and event listeners) remain the same.

function storeDataInIndexedDB(data) {
    const request = indexedDB.open('BillsDB', 1);
    
    request.onupgradeneeded = (event) => {
        const db = event.target.result;
        // This is the correct place to create the object store
        db.createObjectStore('bills', { keyPath: 'id' });
    };

    request.onsuccess = (event) => {
        const db = event.target.result;
        const transaction = db.transaction('bills', 'readwrite');
        const store = transaction.objectStore('bills');

        // Assuming your data has a unique identifier
        const record = { id: 1, content: data, timestamp: Date.now() };
        store.put(record);

        transaction.oncomplete = () => {
            console.log('Data stored successfully in IndexedDB');
        };
    };

    request.onerror = (event) => {
        console.error('IndexedDB error:', event.target.errorCode);
    };
}


function formatDollarAmount(amount) {
    if (amount >= 0) {
        return `$${amount.toFixed(2)}`;
    }
    else {
        return `<span class="color-red">-$${(amount * -1).toFixed(2)}</span>`;
    }
}

function getReconciledIncome() {
    const matchedTransactionIds = new Set();
    let reconciledTotal = 0;

    const expectedPayments = calculateExpectedIncome(state.expectedIncome);

    const totalExpected = expectedPayments.reduce((sum, income) => sum + income.amount, 0);

    for (const expectedPayment of expectedPayments) {
        let isMatched = false;

        for (const actual of state.actualTransactions) {
            if (matchedTransactionIds.has(actual.id)) continue;

            const dateDifference = Math.abs(new Date(expectedPayment.date) - new Date(actual.date)) / (1000 * 60 * 60 * 24);
            const amountDifference = Math.abs(expectedPayment.amount - actual.deposit);

            if (dateDifference <= 3 && amountDifference <= 50) {
                reconciledTotal += actual.deposit;
                matchedTransactionIds.add(actual.id);
                isMatched = true;
                break;
            }
        }

        if (!isMatched) {
            reconciledTotal += expectedPayment.amount;
        }
    }

    for (const actual of state.actualTransactions) {
        if (!matchedTransactionIds.has(actual.id)) {
            reconciledTotal += actual.deposit;
        }
    }

    const totalActual = state.actualTransactions.reduce((sum, transaction) => sum + transaction.deposit, 0);

    return { totalActual, totalExpected, reconciledTotal };
}

function calculateExpectedIncome(incomes) {
    const today = new Date();
    const endOfMonth = new Date(today.getFullYear(), today.getMonth() + 1, 0);

    let expectedPayments = [];

    for (const income of incomes) {
        const startDate = new Date(income.startDate);
        const frequency = income.frequency;

        if (frequency === 0) {
            if (startDate >= today && startDate <= endOfMonth) {
                expectedPayments.push({ amount: income.amount, date: startDate });
            }
        } else {
            const diffInDays = (today - startDate) / (1000 * 60 * 60 * 24);
            let nextOccurenceDays = 0;

            if (diffInDays > 0) {
                const intervalsPassed = Math.floor(diffInDays / frequency);
                nextOccurenceDays = (intervalsPassed + 1) * frequency;
            }

            let nextPaymentDate = new Date(startDate);
            nextPaymentDate.setDate(startDate.getDate() + nextOccurenceDays);

            while (nextPaymentDate <= endOfMonth) {
                expectedObject = { amount: income.amount, date: new Date(nextPaymentDate) };
                expectedPayments.push(expectedObject);
                nextPaymentDate.setDate(nextPaymentDate.getDate() + frequency);
            }
        }
    }
    return expectedPayments;
}

function getBillStatus(b) {
    nextPayDay = new Date();
    dueDate = new Date(b.dueDate + "T07:00:00.000Z");
    updated = new Date(b.updated);
    dueDate.setDate(dueDate.getDate() - b.payEarly);
    const colorClass = dueDate < nextPayDay ? "color-red" : "color-orange";
    dueDate.setDate(dueDate.getDate() + b.payEarly);
    return b.payed ? "<b>Payed (" + (updated.getMonth() + 1) + "-" + updated.getDate() + ")</b>" : `<b class="${colorClass}">$${b.amount.toFixed(2)} - Due ${(dueDate.getMonth() + 1)}-${dueDate.getDate()}</b>`;
}

function renderBillRow(bill) {
    const status = getBillStatus(bill);
    return `
        <tr>
            <td rowspan='2'>
                <input type='checkbox' data-bill-id="${bill.id}" ${bill.payed ? "checked" : ""}>
            </td>
            <td>
                <a href='${bill.configuration.website}' class='link-button' target='_blank'>${bill.title}</a>
            </td>
            <td class='edit-button-cell'>
                <button data-bill-id="${bill.id}" class="edit-bill-button">Edit</button>
            </td>
        </tr>
        <tr>
            <td class='statusRow'>Status: <span class='status'>${status}</span></td>
        </tr>
        <tr>
            <td class='padding'></td>
        </tr>
    `;
}

function attachEventListeners() {
    document.querySelectorAll('#billsTable input[type="checkbox"]').forEach(checkbox => {
        checkbox.addEventListener('change', async (event) => {
            const billId = event.target.dataset.billId;
            const isChecked = event.target.checked;

            const response = await fetch(`/api/bills/${billId}`, {
                method: 'PUT',
                headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify({ id: billId, payed: isChecked })
            });

            if (response.ok) {
                renderBillList();
            } else {
                console.error('Failed to update bill status');
                event.target.checked = !isChecked;
            }
        });
    });

    document.querySelectorAll('.edit-bill-button').forEach(button => {
        button.addEventListener('click', (event) => {
            const billId = event.target.dataset.billId;
            history.pushState({ view: 'edit', id: billId }, '', `/BillsApp/edit/${billId}`);
            renderEditForm(billId);
        });
    });
}

async function renderEditForm(billId) {
    try {
        console.log("loading edit form");
        const viewResponse = await fetch('/BillsApp/views/edit-bill.html');
        if (!viewResponse.ok) throw new Error('Failed to load edit bill view.');
        const viewHtml = await viewResponse.text();

        document.getElementById('app-container').innerHTML = viewHtml;

        let bill = null;
        if (state.bills.length === 0) {
            console.log(`fetching bill id: ${billId}`);
            const billResponse = await fetch(`/api/bills/${billId}`);
            if (!billResponse.ok) throw new Error('Failed to fetch bill data.');
            bill = await billResponse.json();
        }
        else {
            bill = state.bills.find(b => b.id == billId);
        }

        document.getElementById('edit-form-title').innerHTML = `Update <span class='title-orange'>${bill.title}</span> Bill`;
        document.getElementById('amount').value = bill.amount.toFixed(2);
        document.getElementById('duedate').value = new Date(bill.dueDate + "T07:00:00.000Z").toLocaleDateString('en-US', { year: 'numeric', month: '2-digit', day: '2-digit' });

        document.getElementById('submit-button').addEventListener('click', () => submitEditForm(bill.id));

    } catch (error) {
        console.error("Error rendering edit form:", error);
        document.getElementById("app-container").innerHTML = `<p>Error loading form. Please try again later.</p>`;
    }
}

async function submitEditForm(id) {
    const amount = document.getElementById("amount").value;
    const duedate = document.getElementById("duedate").value;
    const statusSpan = document.getElementById("status");

    if (!amount && !duedate) {
        statusSpan.innerHTML = "Please enter data in either of the above fields";
        statusSpan.classList.add('error');
        statusSpan.classList.remove('success');
        return;
    }

    statusSpan.innerHTML = "Updating...";
    statusSpan.classList.remove('error');
    statusSpan.classList.add('success');

    try {
        const dateObject = new Date(duedate);
        const formattedDate = dateObject.toISOString().slice(0, 10);

        const updateData = {
            id: id,
            amount: parseFloat(amount),
            dueDate: formattedDate
        };

        jsonContent = JSON.stringify(updateData);
        console.log(jsonContent);

        const response = await fetch(`/api/bills/${id}`, {
            method: 'PUT',
            headers: {
                'Content-Type': 'application/json',
            },
            body: jsonContent
        });

        if (response.ok) {
            history.pushState(null, '', '/BillsApp/index.html');
            handleRouting();
        } else {
            const errorText = await response.text();
            statusSpan.innerHTML = `Update failed: ${errorText}`;
            statusSpan.classList.add('error');
            statusSpan.classList.remove('success');
        }
    } catch (error) {
        console.error('Fetch error:', error);
        statusSpan.innerHTML = "Network error, please try again.";
        statusSpan.classList.add('error');
        statusSpan.classList.remove('success');
    }
}

function handleRouting() {
    const path = window.location.pathname;
    if (path.startsWith('/BillsApp/edit/')) {
        const billId = path.split('/')[3];
        renderEditForm(billId);
    } else {
        renderBillList();
    }
}

window.addEventListener('popstate', (event) => {
    handleRouting();
});

window.addEventListener('DOMContentLoaded', handleRouting);