var BaseUrl = window.BaseURL;
var token = localStorage.getItem("token");
var role = localStorage.getItem("roleName");

// NOTE: This assumes 'toastr' is available for non-blocking notifications.
// If toastr is not available, you would need to implement a custom notification function.

// Helper function for showing non-blocking messages instead of alert()
function showMessage(type, message) {
    if (typeof toastr !== 'undefined') {
        toastr[type](message);
    } else {
        console.error(type.toUpperCase() + ": " + message);
        // Fallback for demonstration purposes if toastr isn't available
        // alert(message); 
    }
}

$(document).ready(function () {
    showA4(); // default

    // Check if jQuery element exists before getting the value
    let txnIdElement = $("#txnId");
    let txnId = txnIdElement.val();

    if (!txnId || txnId <= 0) {
        showMessage('error', "Invalid Transaction ID provided.");
        return;
    }

    loadBillDetails(txnId);
});

function showA4() {
    $("#printA4").show();
    $("#printThermal").hide();
}

function showThermal() {
    $("#printA4").hide();
    $("#printThermal").show();
}

function loadBillDetails(id) {
    $.ajax({
        url: `${BaseUrl}api/Stock/GetTransactionById?TransactionId=${id}`,
        type: 'GET',
        headers: {
            "Authorization": "Bearer " + token,
            "Role": role
        },
        success: function (response) {
            if (!response || !response.valid) {
                showMessage('error', "Unable to load bill details. Response invalid.");
                return;
            }

            let t = response.item;

            // Fill common fields
            $("#a4OrderId, #thOrder").text(t.orderId);
            $("#a4TransactionId, #thTxn").text(t.transactionId);
            $("#a4Date, #thDate").text(formatDate(t.transactionDate));
            $("#a4Customer, #thCustomer").text(t.customerName || "--");
            $("#a4Type, #thType").text(t.type);
            $("#a4Payment").text(t.paymentMode || "--");

            // Format amount to 2 decimal places or default to "0.00"
            const grandTotal = t.amount != null ? parseFloat(t.amount).toFixed(2) : "0.00";
            $("#a4Grand, #thGrand").text(grandTotal);

            // Build A4 item list
            $("#a4Items").empty();
            $("#thItems").empty();

            t.transactionDetails.forEach(d => {
                $("#a4Items").append(`
                <tr>
                    <td>${d.productName}</td>
                    <td>${d.variantName}</td>
                    <td class="text-right">${d.quantity}</td>
                    <td class="text-right">${parseFloat(d.rate).toFixed(2)}</td>
                    <td class="text-right">${parseFloat(d.totalAmount).toFixed(2)}</td>
                </tr>
                `);

                $("#thItems").append(`
                <tr>
                    <td>${d.productName}-${d.variantName}</td>
                    <td style="text-align:right">${d.quantity}</td>
                    <td style="text-align:right">${parseFloat(d.totalAmount).toFixed(2)}</td>
                </tr>
                `);
            });

            generateQR(t);
        },
        error: function () {
            showMessage('error', "Failed to load transaction details from the API.");
        }
    });
}

function formatDate(date) {
    if (!date) return "--";
    try {
        let d = new Date(date);
        return d.toLocaleDateString("en-GB");
    } catch (e) {
        console.error("Date formatting error:", e);
        return "--";
    }
}

function generateQR(t) {
    // Check if the QR containers exist and clear them before regenerating
    $("#qrContainerA4").empty();
    $("#qrContainerThermal").empty();

    let payload = {
        transactionId: t.transactionId,
        orderId: t.orderId,
        amount: t.amount
    };

    // The variable that holds the data to be encoded in the QR code
    let qrDataText = JSON.stringify(payload);

    // A4 QR
    if (typeof QRCode !== 'undefined') {
        new QRCode(document.getElementById("qrContainerA4"), { // Use document.getElementById for native JS element passing
            text: qrDataText, // FIX: Changed qrData to qrDataText
            width: 120,
            height: 120
        });

        // Thermal QR
        new QRCode(document.getElementById("qrContainerThermal"), { // Use document.getElementById for native JS element passing
            text: qrDataText, // FIX: Changed qrData to qrDataText
            width: 90,
            height: 90
        });
    } else {
        console.error("QRCode library is not loaded. Cannot generate QR codes.");
    }
}