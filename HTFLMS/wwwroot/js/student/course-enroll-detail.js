$(document).ready(function () {

    var courseId = $('#courseId').val();

    loadCourseDetail(courseId);

    $('#openEnrollmentModalBtn').on('click', function () {
        $('#enrollmentConfirmModal').show();
        $('body').css('overflow', 'hidden');
    });

    $('#closeEnrollmentModalBtn, #cancelEnrollmentBtn, #enrollmentModalBackdrop').on('click', function () {
        closeEnrollmentModal();
    });

    $('#confirmEnrollmentBtn').on('click', function () {
        enrollInCourse(courseId);
    });
});

function loadCourseDetail(courseId) {
    $('#enrollmentErrorBox').html('');

    $.ajax({
        url: '/api/student/courses/' + courseId,
        type: 'GET',

        success: function (course) {

            var title = course.title || 'Untitled Course';
            var category = course.category || '-';
            var description = course.description || 'Course description coming soon.';
            var trainerName = course.trainerName || 'No Trainer';
            var certificateText = course.certificateIncluded ? 'Certificate Included' : 'Certificate Not Included';
            var imagePath = course.courseImagePath || '/img/course/course-1.webp';

            $('#courseTitle').text(title);
            $('#overviewTitle').text(title);
            $('#courseCategory').text(category);
            $('#courseDescription').text(description);
            $('#aboutCourse').text(description);

            $('#trainerName').text(trainerName);
            $('#sideTrainerName').text(trainerName);

            $('#certificateIncluded').text(certificateText);
            $('#sideCertificateIncluded').text(course.certificateIncluded ? 'Included' : 'Not Included');

            $('#sideCategory').text(category);
            $('#batchNumber').text(course.batchNumber || '-');
            $('#batchStartDate').text(formatDisplayDate(course.batchStartDate));
            $('#batchEndDate').text(course.batchEndDate ? formatDisplayDate(course.batchEndDate) : 'Not decided');
            $('#durationText').text(course.durationText || '-');

            $('#courseImage').attr('src', imagePath);
            $('#sideCourseImage').attr('src', imagePath);

            if (course.handbookFilePath) {
                $('#handbookBtn').attr('href', course.handbookFilePath);
                $('#handbookBlock').show();
            }
            else {
                $('#handbookBlock').hide();
            }
        },

        error: function (xhr) {
            var msg = getErrorMessage(xhr, 'Course detail could not be loaded.');
            $('#enrollmentErrorBox').html('<div class="text-danger">' + msg + '</div>');
            $('#openEnrollmentModalBtn').prop('disabled', true);
        }
    });
}

function enrollInCourse(courseId) {

    $('#enrollmentErrorBox').html('');

    var btn = $('#confirmEnrollmentBtn');
    btn.prop('disabled', true).text('Enrolling...');

    $.ajax({
        url: '/api/student/enrollment/enroll',
        type: 'POST',
        contentType: 'application/json',
        data: JSON.stringify({
            courseId: parseInt(courseId)
        }),

        success: function (response) {
            sessionStorage.setItem('successMessage', response.message || 'Enrollment completed successfully.');
            window.location.href = '/Student/Student/Index';
        },

        error: function (xhr) {
            btn.prop('disabled', false).text('Yes, Enroll Me');

            var msg = getErrorMessage(xhr, 'Enrollment failed.');

            closeEnrollmentModal();

            $('#enrollmentErrorBox').html('<div class="text-danger">' + msg + '</div>');
        }
    });
}

function closeEnrollmentModal() {
    $('#enrollmentConfirmModal').hide();
    $('body').css('overflow', '');
}

function formatDisplayDate(dateValue) {
    if (!dateValue) return '-';

    var date = new Date(dateValue);

    return date.toLocaleDateString('en-GB', {
        day: '2-digit',
        month: 'short',
        year: 'numeric'
    });
}

function getErrorMessage(xhr, defaultMessage) {
    if (xhr.responseJSON) {
        return xhr.responseJSON.errorMessage ||
            xhr.responseJSON.message ||
            xhr.responseJSON.title ||
            defaultMessage;
    }

    if (xhr.responseText) {
        return xhr.responseText;
    }

    return defaultMessage;
}