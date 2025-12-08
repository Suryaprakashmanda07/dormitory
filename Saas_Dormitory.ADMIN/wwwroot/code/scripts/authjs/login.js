
var BaseUrl = window.BaseURL;
var ImageUrl = window.imageURl;


$(document).ready(function () {
    $("#Loginform").validate({
        rules: {
            Email: {
                required: true,
                email: true
            },
            Password: {
                required: true
            }
        },
        messages: {
            Email: {
                required: "Email is required",
                email: "Enter a valid email address"
            },
            Password: {
                required: "Password is required"
            }
        },
        highlight: function (element) {
            ;
            $(element)
                .removeClass('border-dark')
                .addClass('is-invalid border-danger');
        },
        unhighlight: function (element) {
            $(element)
                .removeClass('is-invalid border-danger')
                .addClass('border-dark');
        },
        errorPlacement: function () {

        },
        invalidHandler: function (event, validator) {
            if (validator.errorList.length) {
                toastr.error(validator.errorList[0].message);
            }
        },
        submitHandler: function (form) {
            form.submit();
        }



    });



    $("#btnSubmit").click(function (e) {
        e.preventDefault();
        debugger;

        if ($("#Loginform").valid()) {
            var Registerdata = {
                email: $("#txtEmail").val(),
                password: $("#txtPassword").val(),
            };

            $.ajax({
                url: BaseUrl + 'api/auth/login',
                type: 'POST',
                contentType: 'application/json',
                data: JSON.stringify(Registerdata),
                dataType: "json",
                success: function (response) {

                    // Case 1: Invalid user
                    if (!response.valid) {
                        toastr.error(response.msg || "Invalid login attempt.");
                        return;
                    }

                    // Case 3: No roles assigned
                    if (!response.item.roles || response.item.roles.length === 0) {
                        toastr.error("No roles assigned to this user.");
                        return;
                    }

                    localStorage.setItem("email", response.item.email || "");
                    localStorage.setItem("expiration", response.item.expiration || "");
                    localStorage.setItem("roleName", response.item.roleName || "");
                    localStorage.setItem("roles", JSON.stringify(response.item.roles[0] || []));
                    localStorage.setItem("token", response.item.token || "");
                    localStorage.setItem("userId", response.item.userId || "");

                    localStorage.setItem("authData", JSON.stringify(response.item));
                    toastr.options.positionClass = 'toast-top-right';
                    toastr.success('Successfully Login');
                    window.location.href = "/Products/ProductsList";

                },
                error: function (xhr, status, error) {
                    toastr.error('Something went wrong Please try again later!');
                    // xhr = full response object
                    // status = error type (e.g., "timeout", "error", "abort")
                    // error = error message
                },
            });
        } else {

        }
    });


});




