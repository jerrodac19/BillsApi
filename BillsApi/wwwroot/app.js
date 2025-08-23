// A simple object to hold all your data and application state
const state = {
    bills: [],
    accountInfo: {},
    expectedIncome: [], // Renamed from 'income' for clarity
    actualTransactions: [] // New array to hold actual income transactions
};

// Main function to fetch all data and render the bill list page
async function renderBillList() {
    try {
        // Step 1: Fetch the HTML for the bill list view
        const viewResponse = await fetch('/views/bill-list.html');
        if (!viewResponse.ok) throw new Error('Failed to load bill list view.');
        const viewHtml = await viewResponse.text();

        // Step 2: Inject the HTML into the main container
        document.getElementById('app-container').innerHTML = viewHtml;
        
        const billsTableBody = document.querySelector("#billsTable tbody");
        billsTableBody.innerHTML = `<tr><td colspan="3" class="statusRow">Loading bills...</td></tr>`;

        // Step 3: Fetch the data from your new ASP.NET Core API endpoints
        const [accountResponse, billsResponse, expectedIncomeResponse, actualTransactionsResponse] = await Promise.all([
            fetch('/api/accountbalances/latest'),
            fetch('/api/bills'),
            fetch('/api/income'), // Assuming this endpoint gives you expected income
            fetch('/api/transactions/monthlyIncome') // New endpoint for actual income transactions
        ]);

        state.accountInfo = await accountResponse.json();
        state.bills = await billsResponse.json();
        state.expectedIncome = await expectedIncomeResponse.json();
        state.actualTransactions = await actualTransactionsResponse.json();

        // calculate the bill total
        const totalDue = state.bills.reduce((sum, bill) => sum + bill.amount, 0);

        const { totalActual, totalExpected, reconciledTotal } = getReconciledIncome();
        const remaining = reconciledTotal - totalDue; // Use the reconciled total here

        const now = new Date();
        const lastUpdatedAccount = new Date(state.accountInfo.updated + "Z");
        const accountAmountClass = (now.getTime() - lastUpdatedAccount.getTime() > 7200000) ? "color-gray" : "color-ok";

        // Step 4: Populate the dynamic content of the page
        document.getElementById("balance").innerHTML = `<span class="${accountAmountClass}">$${state.accountInfo.amount.toFixed(2)}</span>`;
        document.getElementById("updated").innerHTML = `<span class="${accountAmountClass}">${lastUpdatedAccount.toLocaleString()}</span>`;
        document.getElementById("billtotal").innerHTML = formatDollarAmount(totalDue);
        document.getElementById("remain").innerHTML = formatDollarAmount(remaining);
        document.getElementById("expected-income").innerHTML = formatDollarAmount(totalExpected); // Update the ID
        document.getElementById("actual-income").innerHTML = formatDollarAmount(totalActual); // Populate the new ID

        // Step 5: Render the bills table
        billsTableBody.innerHTML = state.bills.map(renderBillRow).join('');

        // Step 6: Attach event listeners to the dynamically created elements
        attachEventListeners();

    } catch (error) {
        console.error("Error rendering bill list:", error);
        document.getElementById("app-container").innerHTML = `<p>Error loading data. Please try again later.</p>`;
    }
}

function formatDollarAmount(amount) {
    if (amount >= 0) {
        return `$${amount.toFixed(2)}`;
    }
    else {
        return `<span class="color-red">-$${(amount * -1).toFixed(2)}</span>`;
    }
}

// app.js

function getReconciledIncome() {
    const matchedTransactionIds = new Set();
    let reconciledTotal = 0;

    // Step 1: Calculate all the expected income payments for the current month
    const expectedPayments = calculateExpectedIncome(state.expectedIncome);

    // Sum up the total expected income
    const totalExpected = expectedPayments.reduce((sum, income) => sum + income.amount, 0);

    // Step 2: Loop through the calculated expected payments to find matches
    for (const expectedPayment of expectedPayments) {
        let isMatched = false;

        for (const actual of state.actualTransactions) {
            // Check if this transaction has already been used for a match
            if (matchedTransactionIds.has(actual.id)) continue;

            const dateDifference = Math.abs(new Date(expectedPayment.date) - new Date(actual.date)) / (1000 * 60 * 60 * 24);
            const amountDifference = Math.abs(expectedPayment.amount - actual.deposit);

            if (dateDifference <= 3 && amountDifference <= 50) {
                reconciledTotal += actual.deposit;
                matchedTransactionIds.add(actual.id); // Mark this transaction as used
                isMatched = true;
                break;
            }
        }

        // If no actual transaction was found, use the expected amount
        if (!isMatched) {
            reconciledTotal += expectedPayment.amount;
        }
    }

    // Step 3: Add any actual transactions that didn't match an expected income
    for (const actual of state.actualTransactions) {
        if (!matchedTransactionIds.has(actual.id)) {
            reconciledTotal += actual.deposit;
        }
    }

    // Sum up the total actual income for separate display
    const totalActual = state.actualTransactions.reduce((sum, transaction) => sum + transaction.deposit, 0);

    return { totalActual, totalExpected, reconciledTotal };
}

function calculateExpectedIncome(incomes) {
    const today = new Date();
    const endOfMonth = new Date(today.getFullYear(), today.getMonth() + 1, 0); // Gets the last day of the current month

    let expectedPayments = [];

    for (const income of incomes) {
        const startDate = new Date(income.startDate);
        const frequency = income.frequency;

        // If frequency is 0, it's a one-time income on a specific day
        if (frequency === 0) {
            // Check if the one-time income date is between now and the end of the month
            if (startDate >= today && startDate <= endOfMonth) {
                expectedPayments.push({ amount: income.amount, date: startDate });
            }
        } else {
            // Periodic income
            // Calculate the first occurrence after today
            const diffInDays = (today - startDate) / (1000 * 60 * 60 * 24);
            let nextOccurenceDays = 0;

            if (diffInDays > 0) {
                const intervalsPassed = Math.floor(diffInDays / frequency);
                nextOccurenceDays = (intervalsPassed + 1) * frequency;
            }

            let nextPaymentDate = new Date(startDate);
            nextPaymentDate.setDate(startDate.getDate() + nextOccurenceDays);

            // Loop and add all payments that fall within the rest of the month
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
    //console.log(b.updated)
    nextPayDay = new Date();
    dueDate = new Date(b.dueDate + "T07:00:00.000Z");
    updated = new Date(b.updated);
    dueDate.setDate(dueDate.getDate() - b.payEarly);
    //console.log(b.dueDate)
    const colorClass = dueDate < nextPayDay ? "color-red" : "color-orange";
    dueDate.setDate(dueDate.getDate() + b.payEarly);
    return b.payed ? "<b>Payed (" + (updated.getMonth() + 1) + "-" + updated.getDate() + ")</b>" : `<b class="${colorClass}">$${b.amount.toFixed(2)} - Due ${(dueDate.getMonth() + 1)}-${dueDate.getDate()}</b>`;
}

// Helper function to render a single bill table row
function renderBillRow(bill) {
    // This function remains the same as before
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

// Helper function to attach all event listeners
function attachEventListeners() {
    // Event listener for checkboxes
    document.querySelectorAll('#billsTable input[type="checkbox"]').forEach(checkbox => {
        checkbox.addEventListener('change', async (event) => {
            const billId = event.target.dataset.billId;
            const isChecked = event.target.checked;

            // Call your API to update the bill status
            const response = await fetch(`/api/bills/${billId}`, {
                method: 'PUT', // Use PUT for updating a status
                headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify({ id: billId, payed: isChecked })
            });

            if (response.ok) {
                // Re-render the page to reflect the updated state
                renderBillList();
            } else {
                console.error('Failed to update bill status');
                event.target.checked = !isChecked; // Revert checkbox state on error
            }
        });
    });

    // Event listener for edit buttons
    document.querySelectorAll('.edit-bill-button').forEach(button => {
        button.addEventListener('click', (event) => {
            const billId = event.target.dataset.billId;
            // Use history.pushState() to change the URL without a page reload
            history.pushState({ view: 'edit', id: billId }, '', `/edit/${billId}`);
            //Call a function to render the edit form here
            renderEditForm(billId);
        });
    });
}

async function renderEditForm(billId) {
    try {
        console.log("loading edit form");
        // Step 1: Fetch the HTML for the edit form
        const viewResponse = await fetch('/views/edit-bill.html');
        if (!viewResponse.ok) throw new Error('Failed to load edit bill view.');
        const viewHtml = await viewResponse.text();

        // Step 2: Inject the HTML into the main container
        document.getElementById('app-container').innerHTML = viewHtml;

        // Step 3: Fetch the specific bill's data from your API
        const billResponse = await fetch(`/api/bills/${billId}`);
        if (!billResponse.ok) throw new Error('Failed to fetch bill data.');
        const bill = await billResponse.json();

        // Step 4: Populate the form fields with the fetched data
        document.getElementById('edit-form-title').innerHTML = `Update <span class='title-orange'>${bill.title}</span> Bill`;
        document.getElementById('amount').value = bill.amount.toFixed(2);
        document.getElementById('duedate').value = new Date(bill.dueDate + "T07:00:00.000Z").toLocaleDateString('en-US', { year: 'numeric', month: '2-digit', day: '2-digit' });

        // Step 5: Attach the event listener for form submission
        document.getElementById('submit-button').addEventListener('click', () => submitEditForm(bill.id));

    } catch (error) {
        console.error("Error rendering edit form:", error);
        document.getElementById("app-container").innerHTML = `<p>Error loading form. Please try again later.</p>`;
    }
}

// Function to handle the form submission and API call
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

        // Format the Date object into a YYYY-MM-DD string
        const formattedDate = dateObject.toISOString().slice(0, 10);

        // Prepare the data to be sent as JSON in the request body
        const updateData = {
            id: id,
            amount: parseFloat(amount),
            dueDate: formattedDate
        };

        jsonContent = JSON.stringify(updateData);
        console.log(jsonContent);

        const response = await fetch(`/api/bills/${id}`, {
            method: 'PUT', // Use PUT for updating a resource
            headers: {
                'Content-Type': 'application/json',
            },
            body: jsonContent
        });

        if (response.ok) {
            // Navigate back to the home page on success
            history.pushState(null, '', '/');
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

// This is your router. We need to update it to call renderEditForm.
function handleRouting() {
    const path = window.location.pathname;
    if (path.startsWith('/edit/')) {
        const billId = path.split('/')[2];
        renderEditForm(billId);
    } else {
        renderBillList();
    }
}

// Add event listener for browser's back/forward buttons
window.addEventListener('popstate', (event) => {
    handleRouting();
});

// Call the router when the page first loads
window.addEventListener('DOMContentLoaded', handleRouting);