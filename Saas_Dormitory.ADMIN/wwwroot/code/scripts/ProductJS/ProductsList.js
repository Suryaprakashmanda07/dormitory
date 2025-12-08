var BaseUrl = window.BaseURL;
var token = localStorage.getItem("token");
var role = localStorage.getItem("roleName");

$(document).ready(function () {
    buildProductsTable();
});

/*----------------------------------- Build Products Table -----------------------------------*/
function buildProductsTable() {
    $('#ProductsTable').DataTable({
        destroy: true,
        paging: true,
        serverSide: true,
        pageLength: 10,
        processing: true,

        ajax: {
            url: BaseUrl + 'api/Products/GetAllProducts',
            type: 'POST',
            contentType: 'application/json',
            headers: {
                "Authorization": "Bearer " + token,
                "Role": role
            },

            data: function (d) {
                return JSON.stringify({
                    PageNumber: Math.floor(d.start / d.length) + 1,
                    PageSize: d.length,
                    SearchTerm: d.search.value,
                    SortColumn: "productName",
                    SortDirection: "asc"
                });
            },

            dataSrc: function (response) {
                response.recordsTotal = response.iTotalRecords;
                response.recordsFiltered = response.iTotalDisplayRecords;
                return response.items;
            }
        },

        columns: [
            {
                data: "productName",
                className: "text-left",
                render: function (data, type, row) {
                    return `
                            <i class="fa fa-angle-right text-warning action-icon mr-2"
                               title="View Variants"
                               onclick="toggleVariants(${row.productId}, this)">
                            </i>
                            ${data}
                        `;
                }
            },


            {
                data: "variants",
                className: "text-center",
                render: v => v?.length ?? 0
            },

            {
                data: "variants",
                className: "text-center",
                render: function (v) {
                    if (!v) return 0;
                    return v.reduce((sum, x) => sum + (x.stock || 0), 0);
                }
            },

            {
                data: null,
                className: "text-center no-sort",
                render: function (row) {

                    let viewIcon = `
                        <i class="fa fa-eye text-info mx-1 action-icon"
                           title="View"
                           onclick="viewProduct(${row.productId})"></i>
                    `;

                    let editIcon = `
                        <i class="fa fa-edit text-primary mx-1 action-icon"
                           title="Edit"
                           onclick="editProduct(${row.productId})"></i>
                    `;

                    let toggleIcon = row.isActive
                        ? `<i class="fa fa-toggle-on text-success mx-1 action-icon"
                     title="Deactivate"
                     onclick="toggleStatus(${row.productId}, true)"></i>`
                        : `<i class="fa fa-toggle-off text-danger mx-1 action-icon"
                     title="Activate"
                     onclick="toggleStatus(${row.productId}, false)"></i>`;

                    return viewIcon + editIcon + toggleIcon;
                }
            }
        ],


        rowCallback: function (row, data) {
            if (!data.isActive) {
                $(row).css("background-color", "#ffe6e6");
            }
        },

        language: {
            search: "",
            searchPlaceholder: "Search Product...",
            processing: `<div class="spinner-border text-primary"></div>`
        }
    });
}

/*----------------------------------- View Product -----------------------------------*/
function viewProduct(id) {
    $.ajax({
        url: BaseUrl + "api/Products/GetProductById?productId=" + id,
        type: "GET",
        headers: {
            "Authorization": "Bearer " + token,
            "Role": role
        },
        success: function (res) {
            if (!res.valid) return toastr.error("Failed to load product");

            let p = res.item;

            let html = `
                <h4>${p.productName}</h4>
                <p><strong>Status:</strong> ${p.isActive ? "Active" : "Inactive"}</p>
                <hr/>
                <h5>Variants</h5>

                <table class="table table-bordered">
                    <thead class="bg-dark text-white">
                        <tr>
                            <th>Qty</th>
                            <th>Unit</th>
                            <th>Stock</th>
                            <th>Price</th>
                        </tr>
                    </thead>
                    <tbody>
            `;

            p.variants.forEach(v => {
                html += `
                    <tr>
                        <td>${v.quantity}</td>
                        <td>${getUnit(v.unit)}</td>
                        <td>${v.stock}</td>
                        <td>${v.price}</td>
                    </tr>
                `;
            });

            html += "</tbody></table>";

            $("#productDetailsBody").html(html);
            $("#productDetailsModal").modal("show");
        },

        error: () => toastr.error("Error loading product details")
    });
}

/*----------------------------------- Edit Product -----------------------------------*/
function editProduct(id) {
    window.location.href = "/Products/AddProducts?Id=" + id;
}

/*----------------------------------- Toggle Status -----------------------------------*/
function toggleStatus(id, current) {
    let newStatus = !current;

    Swal.fire({
        title: "Are you sure?",
        text: `Do you want to ${newStatus ? "activate" : "deactivate"} this product?`,
        icon: "warning",
        showCancelButton: true,
        confirmButtonColor: "#28a745",
        cancelButtonColor: "#d33",
        confirmButtonText: "Yes"
    }).then((result) => {
        if (!result.isConfirmed) return;

        $.ajax({
            url: BaseUrl + "api/Products/UpdateProductStatus?productId=" + id + "&isActive=" + newStatus,
            type: "POST",
            headers: {
                "Authorization": "Bearer " + token,
                "Role": role
            },
            success: function (res) {
                if (res.valid) {
                    toastr.success("Status updated.");

                    // IMPORTANT: FIXED TABLE NAME
                    $('#ProductsTable').DataTable().ajax.reload(null, false);

                } else {
                    toastr.error(res.msg);
                }
            },
            error: function () {
                toastr.error("Error updating status.");
            }
        });

    });
}

/*----------------------------------- Units -----------------------------------*/
function getUnit(u) {
    return { 0: "Piece", 1: "Kg", 2: "Gram", 3: "Litre", 4: "Ml" }[u] || "";
}

function toggleVariants(productId, icon) {
    var $icon = $(icon);
    var table = $('#ProductsTable').DataTable();
    var tr = $icon.closest('tr');
    var row = table.row(tr);

    // COLLAPSE
    if (row.child.isShown()) {
        let child = $(row.child());
        child.slideUp(200, function () {
            row.child.hide();
        });

        $icon.removeClass("fa-angle-down").addClass("fa-angle-right"); // Icon back to right
        return;
    }

    // EXPAND
    $.ajax({
        url: BaseUrl + "api/Products/GetProductById?productId=" + productId,
        type: "GET",
        headers: {
            "Authorization": "Bearer " + token,
            "Role": role
        },
        success: function (res) {

            if (!res.valid) {
                toastr.error("Failed to load variants");
                return;
            }

            let p = res.item;

            let html = `
                <div class="p-2 variant-wrapper" style="display:none;">
                    <h6 class="text-primary mb-2"><strong>Variants</strong></h6>

                    <table class="table table-bordered table-sm">
                        <thead class="bg-dark text-white">
                            <tr>
                                <th>Qty</th>
                                <th>Unit</th>
                                <th>Stock</th>
                                <th>Price</th>
                            </tr>
                        </thead>
                        <tbody>
            `;

            p.variants.forEach(v => {
                html += `
                    <tr>
                        <td>${v.quantity}</td>
                        <td>${getUnit(v.unit)}</td>
                        <td>${v.stock}</td>
                        <td>${v.price}</td>
                    </tr>
                `;
            });

            html += `</tbody></table></div>`;

            // Insert child row
            row.child(html).show();

            // Smooth slide
            tr.next('tr').find('.variant-wrapper').slideDown(200);

            // Change icon to DOWN
            $icon.removeClass("fa-angle-right").addClass("fa-angle-down");
        },

        error: function () {
            toastr.error("Error loading variants");
        }
    });
}


