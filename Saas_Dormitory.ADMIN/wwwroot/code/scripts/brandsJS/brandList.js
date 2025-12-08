var BaseUrl = window.BaseURL;
var token = localStorage.getItem("token");
var role = localStorage.getItem("roleName");

/*----------------------------------- Open Add Brand Modal -----------------------------------*/
function openAddBrandModal() {
    const form = document.getElementById('BrandForm');
    if (form) form.reset();

    if (typeof $ !== 'undefined' && $('#BrandForm').data('validator')) {
        $('#BrandForm').validate().resetForm();
        $('#BrandForm').find('.error').removeClass('error');
    }

    $('#brandModal').modal({
        backdrop: 'static',
        keyboard: false
    });
}

/*----------------------------------- Build Brands Table -----------------------------------*/
function buildBrandTable() {
        $('#BrandTable').DataTable({
            destroy: true,
            paging: true,
            serverSide: true,
            responsive: false,
            pageLength: 10,
            processing: true,
            ajax: {
                url: BaseUrl + 'api/Brands/GetAllBrands',
                type: 'POST',
                contentType: 'application/json', 
                headers: {
                    "Authorization": "Bearer " + token,
                    "Role": role
                },
                data: function (d) {

                    const sortColumnIndex = d.order?.[0]?.column ?? 0;
                    const sortColumn = d.columns?.[sortColumnIndex]?.data ?? "brand_id";
                    const sortDirection = d.order?.[0]?.dir ?? "asc";

                    const payload = {
                        PageNumber: Math.floor(d.start / d.length) + 1,
                        PageSize: d.length,
                        SearchTerm: d.search.value,
                        SortColumn: sortColumn,
                        SortDirection: sortDirection,
                    };

                    return JSON.stringify(payload); 
                },
                dataSrc: function (response) {
                    console.log('data', response);
                    response.recordsTotal = response.totalCount;
                    response.recordsFiltered = response.totalCount;
                    return response.items;
                }
            },
            columns: [
                { data: 'brandName', className: 'text-center' },
                {
                    data: 'price',
                    className: 'text-center',
                    render: function (data) {
                        return data ? `${parseFloat(data).toFixed(2)}` : '--';
                    }
                },
          
                {
                    data: 'createdDate',
                    className: 'text-center',
                    render: function (data) {
                        return data ? new Date(data).toLocaleDateString() : '--';
                    }
                },
                {
                    data: null,
                    className: 'text-center',
                    orderable: false,
                    render: function (data, type, row) {
                        const isActive = !!row.isctive; // normalize
                        const toggleBtn = isActive
                            ? `<button onclick="updateBrandStatus(${row.brandId}, ${isActive})" class="fa-solid fa-toggle-on text-success" title="Deactivate" style="background:none;border:none;"></button>`
                            : `<button onclick="updateBrandStatus(${row.brandId}, ${isActive})" class="fa-solid fa-toggle-off text-danger" title="Activate" style="background:none;border:none;"></button>`;

                        return `
                            <div class="btn-group btn-group-sm">
                                ${toggleBtn}
                                <button class="btn btn-sm btn-primary ml-1" onclick="editBrand(${row.brandId})">
                                    <i class="fa fa-edit"></i> Edit
                                </button>
                            </div>`;
                    }
                }
            ],
            rowCallback: function (row, data) {
                if (!data.isctive) {
                    $(row).css('background-color', '#ffe6e6');
                }
            },
            language: {
                search: "",
                searchPlaceholder: "Search Brands...",
                processing: `<div class="spinner-border text-primary" role="status"></div>`
            }
        });
    }

/*----------------------------------- Add Brand -----------------------------------*/
$(document).ready(function () {
    $("#BrandForm").validate({
        errorClass: 'text-danger',
        rules: {
            brandName: { required: true },
            price: { required: true, number: true, min: 0 }
        },
        messages: {
            brandName: "Brand name is required.",
            price: "Valid price is required."
        },
        invalidHandler: function (event, validator) {
            if (validator.errorList.length) {
                toastr.error(validator.errorList[0].message);
            }
        },
        submitHandler: function (form) {
            const brandData = {
                brandName: $("#txtBrandName").val(),
                price: parseFloat($("#txtPrice").val()),
                capital: 1,
                identity: 1,
                active: true
            };

            $.ajax({
                url: BaseUrl + 'api/Brands/InsertOrUpdateBrand',
                type: 'POST',
                contentType: 'application/json',
                data: JSON.stringify(brandData),
                headers: {
                    "Authorization": "Bearer " + token,
                    "Role": role
                },
                success: function (response) {
                    if (response.valid) {
                        toastr.success("Brand added successfully.");
                        $('#brandModal').modal('hide');
                        buildBrandTable();
                    } else {
                        toastr.error(response.msg);
                    }
                },
                error: function () {
                    toastr.error("Error adding brand.");
                }
            });
        }
    });
});

/*----------------------------------- Edit Brand -----------------------------------*/
function editBrand(brandId) {
    $('#editBrandModal').modal({
        backdrop: 'static',
        keyboard: false
    });

    $.ajax({
        url: `${BaseUrl}api/Brands/GetBrandById?brandId=${brandId}`,
        type: 'GET',
        headers: {
            "Authorization": "Bearer " + token,
            "Role": role
        },
        success: function (response) {
           
            if (response.valid && response.item) {
                const brand = response.item;
                $('#hdnBrandId').val(brand.brandId);
                $('#txtEditBrandName').val(brand.brandName);
                $('#txtEditPrice').val(brand.price);
                $('#txtEditCapital').val(brand.capital);
                $('#txtEditIdentity').val(brand.identity);
            } else {
                toastr.error(response.msg || 'Failed to load brand details.');
            }
        },
        error: function () {
            toastr.error('Error fetching brand details.');
        }
    });
}

/*----------------------------------- Update Brand -----------------------------------*/
$("#btnUpdateBrand").click(function (e) {
    e.preventDefault();

    const updatedBrand = {
        brandId: parseInt($("#hdnBrandId").val()),
        brandName: $("#txtEditBrandName").val(),
        price: parseFloat($("#txtEditPrice").val()),
    };

    $.ajax({
        url: BaseUrl + 'api/Brands/InsertOrUpdateBrand',
        type: 'POST',
        contentType: 'application/json',
        data: JSON.stringify(updatedBrand),
        headers: {
            "Authorization": "Bearer " + token,
            "Role": role
        },
        success: function (response) {
            if (response.valid) {
                toastr.success("Brand updated successfully.");
                $('#editBrandModal').modal('hide');
                buildBrandTable();
            } else {
                toastr.error(response.msg);
            }
        },
        error: function () {
            toastr.error("Failed to update brand.");
        }
    });
});

/*----------------------------------- Toggle Active/Inactive -----------------------------------*/
/*----------------------------------- Toggle Active/Inactive -----------------------------------*/
function updateBrandStatus(brandId, isActive) {
    // Flip the current value (this is the NEW value we want to apply)
    const newStatus = !isActive;
    const action = newStatus ? 'activate' : 'deactivate';

    Swal.fire({
        title: "Are you sure?",
        text: `Do you want to ${action} this brand?`,
        icon: "question",
        showCancelButton: true,
        confirmButtonColor: "#00994d",
        cancelButtonColor: "#d33",
        confirmButtonText: `Yes, ${action} it!`
    }).then((result) => {
        if (result.isConfirmed) {
            $.ajax({
                url: `${BaseUrl}api/Brands/UpdateBrandStatus?BrandId=${brandId}&IsActive=${newStatus}`,
                type: 'POST',
                headers: {
                    "Authorization": "Bearer " + token,
                    "Role": role
                },
                success: function (response) {
                    if (response.valid) {
                        toastr.success(response.msg);
                        buildBrandTable(); // Refresh table
                    } else {
                        toastr.error(response.msg);
                    }
                },
                error: function () {
                    toastr.error("Error updating brand status.");
                }
            });
        }
    });
}


/*----------------------------------- Init Table on Load -----------------------------------*/
$(document).ready(function () {
    buildBrandTable();
});
