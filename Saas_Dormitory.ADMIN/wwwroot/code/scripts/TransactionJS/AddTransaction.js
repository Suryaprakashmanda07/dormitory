// AddTransaction.js
var BaseUrl = window.BaseURL;
var token = localStorage.getItem("token");
var role = localStorage.getItem("roleName");
var productList = [];
var AllProducts = [];

$(document).ready(function () {

    const Id = parseInt($("#hdnId").val()) || 0;
    const convert = ($("#hdnConvert").val() + "").trim().toLowerCase() === "true";
    const isEstimation = ($("#hdnIsEstimation").val() + "").trim().toLowerCase() === "true";

    if (Id === 0) {
        $("input[name='transType'][value='sale']").prop("checked", true);
        handleTypeChange("sale");
        $("#ProductTable").show();
    }

    if (Id > 0) {
        $(".page-title").text("Update Transaction");
        $("#btn_save").text("Update");
    }

    const today = new Date();
    $('#txtDate').val(today.toLocaleDateString('en-GB'));

    $('#txtDate').datepicker({
        format: 'dd-mm-yyyy',
        autoclose: true,
        todayHighlight: true
    });

    LoadProducts();

    if (Id > 0) {
        LoadTransaction(Id, isEstimation, convert);
    }

    $("input[name='transType']").change(function () {
        handleTypeChange($(this).val());
    });

    $("input[name='paymentMode']").change(function () {
        handlePaymentModeChange($(this).val());
    });

    $('#txtQuantity, #txtRate').on('input', function () {
        let qty = parseFloat($('#txtQuantity').val()) || 0;
        let rate = parseFloat($('#txtRate').val()) || 0;
        $('#txtAmount').val((qty * rate).toFixed(2));
    });
});

/* --------------------------- LOAD PRODUCTS --------------------------- */
function LoadProducts() {
   
    $.ajax({
        url: BaseUrl + "api/Products/GetAllProductsPublic",
        type: "GET",
        headers: {
            "Authorization": "Bearer " + token,
            "Role": role
        },
        success: function (res) {
            if (!res || !res.items) return toastr.error("Failed to load products");

            AllProducts = res.items;

            var ddl = $("#ddlProduct");
            ddl.empty().append('<option value="">-- Select Product --</option>');

            AllProducts.forEach(p =>
                ddl.append(`<option value="${p.productId}">${p.productName}</option>`)
            );

            $("#ddlProduct").change(loadVariants);
        },
        error: function () {
            toastr.error("Error updating status.");
        }
    });
}

/* --------------------------- LOAD VARIANTS --------------------------- */
function loadVariants() {
    var pid = $("#ddlProduct").val();

    var ddl = $("#ddlVariant");
    ddl.empty().append('<option value="">-- Select Variant --</option>');

    if (!pid) return;

    var product = AllProducts.find(x => x.productId == pid);
    if (!product || !product.variants) return;

    product.variants.forEach(v => {
        ddl.append(
            `<option value="${v.variantId}" data-price="${v.price}">
                ${v.quantity} ${getUnitName(v.unit)}
            </option>`
        );
    });

    $("#ddlVariant").change(function () {
        var price = $("#ddlVariant option:selected").data("price") || 0;
        $("#txtRate").val(price);
        calcAmount();
    });
}

/* Unit text */
function getUnitName(id) {
    const units = ["Piece", "Kg", "Gram", "Litre", "Ml"];
    return units[id] || "";
}

function calcAmount() {
    let qty = parseFloat($("#txtQuantity").val()) || 0;
    let rate = parseFloat($("#txtRate").val()) || 0;
    $("#txtAmount").val((qty * rate).toFixed(2));
}

/* ---------------------------- LOAD TRANSACTION ---------------------------- */
function LoadTransaction(id, isEstimation, convert) {
    $.get(BaseUrl + "api/Stock/GetTransactionById?TransactionId=" + id, function (res) {

        if (!res || !res.valid) return toastr.error(res?.msg || "Failed to load");

        let t = res.item;

        $("#txtTransactionID").val(t.transactionId);
        $("#txtCustomerName").val(t.customerName || "");
        $("#txtDate").val(formatDateUI(t.transactionDate));
        $("#cashAmount").val(t.cashAmount);
        $("#upiAmount").val(t.upiamount);

        productList = t.transactionDetails.map(d => ({
            productId: d.productId,
            variantId: d.variantId,
            productName: d.productName,
            variantName: d.variantName,
            quantity: d.quantity,
            rate: d.rate,
            totalAmount: d.totalAmount
        }));

        RenderProductTable();

        if (convert) {
            $("input[name='transType'][value='sale']").prop("checked", true);
            $("input[name='transType']").prop("disabled", true);
            $("#paymentSection").show();
        }
        else if (isEstimation) {
            $("input[name='transType'][value='estimation']").prop("checked", true);
            $("#paymentSection").hide();
        }
        else {
            $("input[name='transType'][value='" + t.type + "']").prop("checked", true);

            if (t.paymentMode) {
                $(`input[name='paymentMode'][value='${t.paymentMode}']`).prop("checked", true);
                handlePaymentModeChange(t.paymentMode);
            }
        }

        $("#ProductTable").show();
    });
}

/* --------------------------- TYPE CHANGE --------------------------- */
function handleTypeChange(type) {
    $("#AmountInput, #cashInput, #upiInput").hide();

    if (type === "estimation") {
        $("#paymentSection").hide();
    } else {
        $("#paymentSection").show();
    }

    $("#ProductTable").show();
}

/* --------------------------- PAYMENT MODE --------------------------- */
function handlePaymentModeChange(mode) {
    $("#AmountInput, #cashInput, #upiInput").hide();

    let total = productList.reduce((sum, p) => sum + p.totalAmount, 0);

    if (mode === "Cash+UPI") {
        $("#cashInput").show();
        $("#upiInput").show();
    }
    else if (mode === "Jama") {
        $("#AmountInput").show();
        $("#Amount").val(total.toFixed(2));
    }
}

/* --------------------------- ADD PRODUCT --------------------------- */
function AddProduct() {

    var pid = $("#ddlProduct").val();
    var pname = $("#ddlProduct option:selected").text();

    var vid = $("#ddlVariant").val();
    var vname = $("#ddlVariant option:selected").text();

    var qty = parseFloat($("#txtQuantity").val());
    var rate = parseFloat($("#txtRate").val());

    if (!pid) return toastr.error("Select product");
    if (!vid) return toastr.error("Select variant");
    if (!qty || qty <= 0) return toastr.error("Enter valid quantity");
    if (!rate || rate <= 0) return toastr.error("Enter valid rate");

    productList.push({
        productId: pid,
        variantId: vid,
        productName: pname,
        variantName: vname,
        quantity: qty,
        rate: rate,
        totalAmount: qty * rate
    });

    RenderProductTable();

    $("#ddlProduct").val('');
    $("#ddlVariant").empty().append('<option value="">-- Select Variant --</option>');
    $("#txtQuantity, #txtRate, #txtAmount").val('');
}

/* --------------------------- RENDER TABLE --------------------------- */
function RenderProductTable() {
    var tbody = $("#Tablerow");
    tbody.empty();
    var total = 0;

    productList.forEach((p, i) => {
        tbody.append(`
            <tr>
                <td>${escapeHtml(p.productName)}</td>
                <td>${escapeHtml(p.variantName)}</td>
                <td>${p.quantity}</td>
                <td>${p.rate}</td>
                <td>${p.totalAmount.toFixed(2)}</td>
                <td>
                    <button class="btn btn-danger btn-sm" onclick="RemoveProduct(${i})">
                        Remove
                    </button>
                </td>
            </tr>
        `);
        total += p.totalAmount;
    });

    $("#totalLabel").text(`Total: ₹${total.toFixed(2)}`);

    if ($("input[name='paymentMode']:checked").val() === "Jama") {
        $("#Amount").val(total.toFixed(2));
    }
}


/* --------------------------- REMOVE PRODUCT --------------------------- */
function RemoveProduct(i) {
    productList.splice(i, 1);
    RenderProductTable();
}

/* --------------------------- SAVE TRANSACTION --------------------------- */
function AddTransaction() {
    debugger;
    const type = $("input[name='transType']:checked").val();
    const date = $("#txtDate").val();
    const payment = $("input[name='paymentMode']:checked").val();

    if (!type) return toastr.error("Select type");
    if (!date) return toastr.error("Enter valid date");
    if (!$("#txtCustomerName").val().trim()) return toastr.error("Enter customer name");
    if (!productList.length) return toastr.error("Add at least one product");
    if (type !== "estimation" && !payment) return toastr.error("Select payment mode");

    let totalAmount = productList.reduce((sum, p) => sum + p.totalAmount, 0);

    const trans = {
        transactionId: parseInt($("#txtTransactionID").val()) || 0,
        type: type,
        transactionDate: formatDateToISO(date),
        customerName: $("#txtCustomerName").val(),

        orderId: parseInt($("#hdnOrderId").val()) || 0, 
        paymentMode: type === "estimation" ? null : payment,
        amount: type === "estimation" ? null : totalAmount,

        cashAmount: payment === "Cash+UPI" ? parseFloat($("#cashAmount").val()) : null,
        upiamount: payment === "Cash+UPI" ? parseFloat($("#upiAmount").val()) : null,

        transactionDetails: productList.map(p => ({
            productId: Number(p.productId),
            variantId: Number(p.variantId),
            quantity: Number(p.quantity),
            rate: Number(p.rate),
            productName: p.productName.trim(),
            variantName: p.variantName.trim(),
            totalAmount: Number(p.totalAmount)     
        }))
        
    };
    console.log("trans", trans);

    $.ajax({
        url: BaseUrl + "api/Stock/InsertOrUpdateTransaction",
        type: "POST",
        data: JSON.stringify(trans),
        contentType: "application/json",
        success: function (res) {
            if (res.valid) {
                toastr.success(res.msg || "Saved successfully");
                window.location.href = "/Stock/TransactionDetails";
            } else {
                toastr.error(res.msg || "Failed to save");
            }
        },
        error: function () {
            toastr.error("Error saving transaction.");
        }
    });
}

/* --------------------------- DATE HELPERS --------------------------- */
function formatDateUI(date) {
    if (!date) return "";
    const d = new Date(date);
    return d.toLocaleDateString("en-GB");
}

function formatDateToISO(d) {
    if (!d) return null;
    d = d.replace(/\//g, "-");
    const p = d.split("-");
    if (p.length !== 3) return null;
    return `${p[2]}-${p[1]}-${p[0]}`;
}

function escapeHtml(text) {
    if (!text) return "";
    return text
        .replace(/&/g, "&amp;")
        .replace(/</g, "&lt;")
        .replace(/>/g, "&gt;")
        .replace(/"/g, "&quot;")
        .replace(/'/g, "&#039;")
        .replace(/`/g, "&#096;");
}