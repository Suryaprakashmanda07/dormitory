var BaseUrl = window.BaseURL;
var ImageUrl = window.imageURl;
const authData = JSON.parse(localStorage.getItem("authData"));

$(document).ready(function () {

    $.validator.addMethod("strongPassword", function (value, element) {
        return this.optional(element) || /^(?=.*[A-Za-z])(?=.*\d)[A-Za-z\d@$!%*?&]+$/.test(value);
    }, "Password must include at least one letter and one number.");



    $("#ChangePasswordForm").validate({
        errorClass: 'text-danger',
        rules: {
            CurrentPassword: {
                required: true,
            },
            NewPassword: {
                required: true,
                strongPassword: true
            },
            ConfirmPassword: {
                required: true,
                equalTo: "#txtNewPassword"
            }
        },
        messages: {
            CurrentPassword: {
                required: "Current Password is mandatory",
            },
            NewPassword: {
                required: "New password is mandatory",
                minlength: "Password must be at least 10 characters"
            },
            ConfirmPassword: {
                equalTo: "Passwords do not match"
            }
        },
        highlight: function (element) {
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
            // No inline validation messages
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



    $("#btnChangePasswordSubmit").click(function (e) {
        e.preventDefault();


        if ($("#ChangePasswordForm").valid()) {

            var model = {
                OldPassword: $("#txtCurrentPassword").val(),
                NewPassword: $("#txtNewPassword").val(),
                ConfirmPassword: $("#txtConfirmPassword").val(),
                Id: authData.userId
            };


            $.ajax({
                url: BaseUrl + '/api/auth/ChangePassword',
                type: 'POST',
                contentType: 'application/json',
                data: JSON.stringify(model),
                dataType: "json",
                headers: {
                    "Authorization": `Bearer ${authData.token}`,
                    "User-Role": authData.Roles
                },
                success: function (response) {



                    if (response.valid) {
                        toastr.options.positionClass = 'toast-bottom-right';
                        toastr.success(response.msg);
                        window.location.href = "/Home/Dashboard";


                    } else {
                        toastr.success(response.msg);
                        return;
                    }

                    //console.log(response)
                    //localStorage.setItem("authData", JSON.stringify(response.item));
                    ///*  _token = response.item.token;*/

                    //toastr.options.positionClass = 'toast-bottom-right';
                    //toastr.success(response.msg);
                    //window.location.href = "/Home/Dashboard";

                },
                error: function (response) {

                    if (response.responseJSON.msg == "Your account is inactive. Please contact the administrator.") {
                        toastr.error(response.responseJSON.msg);
                    } else {
                        toastr.error('Invalid Email or password.');
                    }


                },
            });
        } else {

        }
    });


});




