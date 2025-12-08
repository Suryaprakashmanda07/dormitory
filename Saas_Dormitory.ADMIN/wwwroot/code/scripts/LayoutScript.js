$(document).ajaxStart(function () {
    $('#overlay').show();
});

$(document).ajaxStop(function () {
    $('#overlay').hide();
});
$(document).ajaxError(function (event, jqXHR) {
    debugger;
    if (jqXHR.status === 401) {
       // alert("Hit");
        // Clear invalid token
        localStorage.removeItem("token");
        localStorage.removeItem("roleName");

        // Redirect to login
        window.location.href = "Auth/Login";
    }
});

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


function logout() {
    // Remove auth data
    localStorage.removeItem("token");
    localStorage.removeItem("roleName");

    // Optional: clear everything
    localStorage.clear();
    // sessionStorage.clear();

}



//$.ajaxSetup({
//    beforeSend: function (xhr) {
//        const token = localStorage.getItem("token");
//        const role = localStorage.getItem("roleName");

//        if (token) {
//            xhr.setRequestHeader("Authorization", "Bearer " + token);
//        }
//        if (role) {
//            xhr.setRequestHeader("User-Role", role);
//        }
//    }
//});



// Global error handling
$.ajaxSetup({
    error: function (jqXHR, textStatus, errorThrown) {
        $('#overlay').hide(); // always stop overlay

        if (jqXHR.responseJSON && jqXHR.responseJSON.msg) {
            // If backend sends JSON error
            toastr.error(jqXHR.responseJSON.msg);
        } else if (jqXHR.status === 404) {
            toastr.error("404: API not found. Check your BaseUrl.");
        } else {
            toastr.error("Request failed (" + jqXHR.status + "): " + errorThrown);
        }
    }
});

$(document).ready(function () {
    if (!token) {
        window.location.href = "/Account/Login"; // change path as needed
        return;
    }


    var currentUrl = window.location.pathname.toLowerCase();

    // Loop through each nav link
    $(".nav-sidebar a.nav-link").each(function () {
        var $this = $(this);
        var href = $this.attr("href");

        if (href) {
            // Normalize the href
            var cleanedLink = href.replace(/^~|^.*\/\/[^\/]+/, '').toLowerCase();

            // Match current URL
            if (currentUrl === cleanedLink || currentUrl.startsWith(cleanedLink)) {
                // Add active to current link
                $this.addClass("active");

                // Handle treeview parent expansion if in a submenu
                var $treeview = $this.closest(".nav-treeview");
                if ($treeview.length) {
                    var $parentItem = $treeview.closest(".nav-item.has-treeview, .nav-item");
                    $parentItem.addClass("menu-open");
                    $parentItem.children("a.nav-link").first().addClass("active");
                }
            }
        }
    });
});






// Global handler for unauthorized requests
$(document).ajaxError(function (event, jqxhr, settings) {
    if (jqxhr.status === 401 || jqxhr.status === 403) {
        /*console.warn("Session expired. Redirecting to login...");*/


        localStorage.removeItem('jwtToken');
        sessionStorage.removeItem('jwtToken');  
        window.location.href = "/Account/Login"; 

    }
});







