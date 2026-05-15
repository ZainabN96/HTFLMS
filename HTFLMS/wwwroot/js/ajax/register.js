$(document).ready(function () {

    $('#registerForm').on('submit', function (e) {
        debugger;
        e.preventDefault();

        if (!$(this).valid()) return;

        var payload = {
            email: $('#Email').val(),
            password: $('#Password').val(),
            gender: $('#Gender').val(),
            name: $('#Name').val(),
            qualification: $('#Qualification').val(),
            cnic: $('#CNIC').val(),
            address: $('#Address').val(),
            country: $('#Country').val(),
            city: $('#City').val(),
            mobileNumber: $('#MobileNumber').val(),
            linkedIn: $('#LinkedIn').val(),
            employmentStatus: $('#EmploymentStatus').val()
        };

        var btn = $('#submitBtn');
        btn.prop('disabled', true).text('Submitting...');

        $.ajax({
            url: '/api/User/register',
            type: 'POST',
            contentType: 'application/json',
            data: JSON.stringify(payload),
            success: function (res) {

                var returnUrl = $('#returnUrl').val();

                var loginUrl = '/Account/Login?registered=true';

                if (returnUrl) {
                    loginUrl += '&returnUrl=' + encodeURIComponent(returnUrl);
                }

                window.location.href = loginUrl;
            },
            error: function (xhr) {
                var err = xhr.responseJSON;
                var msg = err?.errorMessage || 'Registration failed. Please try again.';
                $('.error-box').html('<div>' + msg + '</div>');
            },
            complete: function () {
                btn.prop('disabled', false).text('Submit Your Application Today!');
            }
        });
    });

});