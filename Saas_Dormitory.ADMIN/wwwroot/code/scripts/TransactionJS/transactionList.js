var BaseUrl = window.BaseURL;
var token = localStorage.getItem("token");
var role = localStorage.getItem("roleName");

$(document).ready(function () {
    buildTransactionTable();
});

/*----------------------------------- Build Transactions Table -----------------------------------*/
function buildTransactionTable() {
    $('#TransactionTable').DataTable({
        destroy: true,
        paging: true,
        serverSide: true,
        pageLength: 10,
        processing: true,

        ajax: {
            url: BaseUrl + 'api/Stock/GetAllTransactions',
            type: 'POST',
            contentType: 'application/json',
            headers: {
                "Authorization": "Bearer " + token,
                "Role": role
            },

            data: function (d) {
                const sortColumnIndex = d.order?.[0]?.column ?? 0;
                const sortColumn = d.columns?.[sortColumnIndex]?.data ?? "transactionId";
                const sortDirection = d.order?.[0]?.dir ?? "asc";

                return JSON.stringify({
                    PageNumber: Math.floor(d.start / d.length) + 1,
                    PageSize: d.length,
                    SearchTerm: d.search.value,
                    SortColumn: sortColumn,
                    SortDirection: sortDirection,
                });
            },

            dataSrc: function (response) {
                response.recordsTotal = response.iTotalRecords;
                response.recordsFiltered = response.iTotalDisplayRecords;
                return response.items;
            }
        },

        columns: [
            { data: "orderId", className: "text-center" },
            { data: "transactionId", className: "text-center" },
            { data: "customerName", className: "text-center" },

            // Transaction Type Badge
            {
                data: "type",
                className: "text-center",
                render: function (type) {
                    if (!type) return "<span class='badge badge-secondary'>--</span>";

                    switch (type.toLowerCase()) {
                        case "sale": return `<span class="badge badge-success">Sale</span>`;
                        case "purchase": return `<span class="badge badge-primary">Purchase</span>`;
                        case "buy": return `<span class="badge badge-primary">Purchase</span>`;
                        case "estimation": return `<span class="badge badge-warning text-dark">Estimation</span>`;
                        default: return `<span class="badge badge-info">${type}</span>`;
                    }
                }
            },

            {
                data: "transactionDate",
                className: "text-center",
                render: d => d ? new Date(d).toLocaleDateString() : "--"
            },

            {
                data: "amount",
                className: "text-center",
                render: (data, type, row) =>
                    row.type === "estimation" ? "--" : (data ? parseFloat(data).toFixed(2) : "--")
            },

            {
                data: "paymentMode",
                className: "text-center",
                render: (data, type, row) => row.type === "estimation" ? "--" : data
            },

            {
                data: null,
                className: "text-center no-sort",
                render: function (row) {

                    // View Button -> View Icon (fa fa-eye)
                    let viewIcon = `
                        <i class="fa fa-eye text-info mx-1 action-icon"
                           data-toggle="tooltip"
                           title="View Details"
                           onclick="viewTransactionDetails(${row.transactionId})"></i>
                    `;

                    // Edit Button -> Edit Icon (fa fa-edit)
                    let editIcon = `
                        <i class="fa fa-edit text-primary mx-1 action-icon"
                           data-toggle="tooltip"
                           title="Edit Transaction"
                           onclick="editTransaction(${row.transactionId})"></i>
                    `;

                    // Print Button -> Print Icon (fa fa-print)
                    let printIcon = `
                        <i class="fa fa-print text-secondary mx-1 action-icon"
                           data-toggle="tooltip"
                           title="Print Bill"
                           onclick="printBill(${row.transactionId})"></i>
                    `;

                    // Convert Button (Conditional) -> Convert Icon (fa-solid fa-cash-register)
                    let convertIcon = "";

                    if (row.type === "estimation") {
                        convertIcon = `
                        <i class="fa-solid fa-cash-register text-success mx-1 action-icon"
                           data-toggle="tooltip"
                           title="Convert to Sale"
                           onclick="convertToSale(${row.transactionId})"></i>
                    `;
                    }

                    return viewIcon + editIcon + printIcon + convertIcon;
                }
            }

        ],

        language: {
            search: "",
            searchPlaceholder: "Search...",
            processing: `<div class="spinner-border text-primary"></div>`
        }
    });
}

/*----------------------------------- Convert Estimation → Sale -----------------------------------*/
function convertToSale(id) {
    window.location.href = `/Stock/AddTransaction?Id=${id}&Convert=true`;
}

/*----------------------------------- View Transaction Details -----------------------------------*/
function viewTransactionDetails(id) {

    $.ajax({
        url: `${BaseUrl}api/Stock/GetTransactionById?TransactionId=${id}`,
        type: 'GET',
        headers: {
            "Authorization": "Bearer " + token,
            "Role": role
        },

        success: function (response) {
            if (!response.valid) {
                toastr.error("Failed to load details");
                return;
            }

            let t = response.item;

            let html = `
                <h5><strong>Customer:</strong> ${t.customerName ?? "--"}</h5>
                <p><strong>Date:</strong> ${new Date(t.transactionDate).toLocaleDateString()}</p>
                <p><strong>Type:</strong> ${t.type}</p>
            `;

            if (t.type !== "estimation") {
                html += `
                    <p><strong>Payment Mode:</strong> ${t.paymentMode}</p>
                    <p><strong>Total Amount:</strong> ₹${t.amount?.toFixed(2)}</p>
                `;
            }

            html += `
                <hr/>
                <h5>Items</h5>
                <table class="table table-bordered">
                    <thead>
                        <tr>
                            <th>Product</th>
                            <th>Variant</th>
                            <th>Qty</th>
                            <th>Rate</th>
                            <th>Total</th>
                        </tr>
                    </thead>
                    <tbody>
            `;

            t.transactionDetails.forEach(d => {
                html += `
                    <tr>
                        <td>${d.productName}</td>
                        <td>${d.variantName}</td>
                        <td>${d.quantity}</td>
                        <td>${d.rate}</td>
                        <td>${d.totalAmount}</td>
                    </tr>`;
            });

            html += "</tbody></table>";

            $("#transactionDetailsBody").html(html);
            $("#transactionDetailsModal").modal("show");
        },

        error: function () {
            toastr.error("Failed to load transaction details.");
        }
    });
}

/*----------------------------------- Edit Transaction -----------------------------------*/
function editTransaction(id) {
    window.location.href = `/Stock/AddTransaction?Id=${id}`;
}

function printBill(id) {
    window.open('/Stock/PrintBill?id=' + id, '_blank');
}

