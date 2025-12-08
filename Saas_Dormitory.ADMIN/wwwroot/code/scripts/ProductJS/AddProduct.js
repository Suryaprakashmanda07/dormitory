var BaseUrl = window.BaseURL;
var token = localStorage.getItem("token");
var role = localStorage.getItem("roleName");
var variants = [];

$(document).ready(function () {

    const Id = parseInt($("#hdnId").val()) || 0;

    if (Id > 0) {
        $(".page-title").text("Update Product");
        $("#btn_save").text("Update");
    }

    if (Id > 0) {
        LoadProduct(Id);
    }
});

function addVariant() {
    var qty = parseFloat($("#txtVarQty").val());
    var unit = $("#ddlUnit").val();
    var stock = parseInt($("#txtVarStock").val());
    var price = parseFloat($("#txtVarPrice").val());

    if (!qty || qty <= 0) return toastr.error("Enter valid quantity");
    if (!unit) return toastr.error("Select unit");
    if (!price || price <= 0) return toastr.error("Enter valid price");
    if (stock < 0) return toastr.error("Invalid stock");

    variants.push({
        variantId: 0,
        quantity: qty,
        unit: parseInt(unit),
        stock: stock,
        price: price
    });

    renderVariants();
    $("#txtVarQty, #txtVarStock, #txtVarPrice").val("");
    $("#ddlUnit").val("");
}

function renderVariants() {
    var tbody = $("#tblVariantsBody");
    tbody.empty();

    if (variants.length === 0) {
        $("#tblVariants").hide();
        return;
    }

    $("#tblVariants").show();

    variants.forEach((v, i) => {
        tbody.append(`
            <tr>
                <td>${v.quantity}</td>
                <td>${getUnit(v.unit)}</td>
                <td>${v.stock}</td>
                <td>${v.price}</td>
                <td><button class="btn btn-danger btn-sm" onclick="removeVariant(${i})">Remove</button></td>
            </tr>
        `);
    });
}

function removeVariant(i) {
    variants.splice(i, 1);
    renderVariants();
}

function getUnit(u) {
    return {
        0: "Piece",
        1: "Kg",
        2: "Gram",
        3: "Litre",
        4: "Ml"
    }[u] || "";
}

function saveProduct() {
    var name = $("#txtProductName").val().trim();
    if (!name) return toastr.error("Enter product name");
    if (variants.length === 0) return toastr.error("Add at least one variant");

    var payload = {
        productId: parseInt($("#hdnId").val()) || 0,
        productName: name,
        variants: variants
    };

    $.ajax({
        url: BaseUrl +"api/Products/InsertOrUpdateProduct",
        type: "POST",
        headers: {
            "Authorization": "Bearer " + localStorage.getItem("token")
        },
        data: JSON.stringify(payload),
        contentType: "application/json",
        success: function (res) {
            if (res.valid) {
                toastr.success("Product saved successfully!");
                window.location.href = "/Products/ProductsList";
            } else {
                toastr.error(res.msg);
            }
        },
        error: function () {
            toastr.error("Error saving product");
        }
    });
}

function LoadProduct(id) {

    $.ajax({
        url: BaseUrl + "api/Products/GetProductById?productId=" + id,
        type: "GET",
        headers: {
            "Authorization": "Bearer " + token,
            "Role": role
        },
        success: function (res) {
            if (!res || !res.valid) return toastr.error(res?.msg || "Failed to load");
            console.log("Get", res);

            let t = res.item;

            $("#txtProductName").val(t.productName);

            // FIXED HERE ⬇⬇⬇
            variants = t.variants.map(d => ({
                variantId: d.variantId,
                quantity: d.quantity,
                unit: d.unit,
                stock: d.stock,
                price: d.price
            }));

            renderVariants();
        },

        error: () => toastr.error("Error loading product details")
    });
}

