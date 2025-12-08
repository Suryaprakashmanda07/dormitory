var BaseUrl = window.BaseURL;
var token = localStorage.getItem("token");
var EstimateItems = [];

$(document).ready(function () {

    // default date
    const today = new Date();
    $('#txtDate').val(today.toLocaleDateString("en-GB"));

    $('#txtDate').datepicker({
        format: 'dd-mm-yyyy',
        autoclose: true,
        todayHighlight: true
    });

    LoadBrands();

    // auto calc
    $('#txtQty, #txtRate').on('input', function () {
        var q = parseFloat($('#txtQty').val()) || 0;
        var r = parseFloat($('#txtRate').val()) || 0;
        $('#txtTotal').val((q * r).toFixed(2));
    });
});

function LoadBrands() {
    $.ajax({
        url: BaseUrl + 'api/Brands/GetAllBrandsPublic',
        type: 'GET',
        dataType: 'json',
        success: function (res) {
            var list = res.items || [];
            $('#ddlProduct').empty().append('<option value="">-- Select Brand --</option>');

            $.each(list, function (i, b) {
                $('#ddlProduct').append(`<option value="${b.brandId}">${b.brandName}</option>`);
            });
        }
    });
}

function AddEstimateRow() {
    var brandId = $('#ddlProduct').val();
    var brandName = $('#ddlProduct option:selected').text();
    var qty = parseFloat($('#txtQty').val());
    var rate = parseFloat($('#txtRate').val());
    var total = qty * rate;

    if (!brandId) return toastr.error("Select a brand");
    if (qty <= 0) return toastr.error("Enter valid quantity");
    if (rate <= 0) return toastr.error("Enter valid rate");

    var item = {
        brandId: brandId,
        brandName: brandName,
        quantity: qty,
        rate: rate,
        totalAmount: total
    };

    EstimateItems.push(item);
    RenderEstimationTable();

    // Clear inputs
    $('#ddlProduct').val('');
    $('#txtQty').val('');
    $('#txtRate').val('');
    $('#txtTotal').val('');
}

function RenderEstimationTable() {
    var $tbody = $('#estimateRows');
    $tbody.empty();
    var grand = 0;

    $.each(EstimateItems, function (i, item) {
        grand += item.totalAmount;

        var row = `
            <tr>
                <td>${item.brandName}</td>
                <td>${item.quantity}</td>
                <td>${item.rate}</td>
                <td>${item.totalAmount.toFixed(2)}</td>
                <td>
                    <button class="btn btn-danger btn-sm" onclick="RemoveRow(${i})">Remove</button>
                </td>
            </tr>
        `;

        $tbody.append(row);
    });

    $('#lblGrandTotal').text("Total: ₹" + grand.toFixed(2));
}

function RemoveRow(i) {
    EstimateItems.splice(i, 1);
    RenderEstimationTable();
}

function SaveEstimation() {
    if (!EstimateItems.length)
        return toastr.error("Add at least one product.");

    var date = $('#txtDate').val();
    if (!date) return toastr.error("Enter date");

    var estimation = {
        estimationId: parseInt($('#EstimationID').val()) || 0,
        customerName: $('#txtCustomerName').val() || null,
        estimationDate: formatDate(date),
        items: EstimateItems
    };

    Swal.fire({
        title: "Confirm?",
        text: "Do you want to save this estimation?",
        icon: "question",
        showCancelButton: true
    }).then(function (res) {
        if (res.isConfirmed) {
            $.ajax({
                url: BaseUrl + "api/Sales/InsertEstimation",
                type: "POST",
                contentType: "application/json",
                data: JSON.stringify(estimation),
                success: function (resp) {
                    if (resp.valid) {
                        toastr.success(resp.msg);
                        window.location.href = "/Sales/EstimationList";
                    } else {
                        toastr.error(resp.msg);
                    }
                }
            });
        }
    });
}

function formatDate(d) {
    let p = d.split("-");
    return `${p[2]}-${p[1]}-${p[0]}`;
}
