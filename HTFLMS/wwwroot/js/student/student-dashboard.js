$(document).ready(function () {
    loadStudentDashboard();
});

function loadStudentDashboard() {
    $.ajax({
        url: '/api/StudentDashboard',
        type: 'GET',

        success: function (data) {
            renderStudentDashboardStats(data);
        },

        error: function (xhr) {
            var err = xhr.responseJSON;
            var msg = err?.errorMessage || err?.message || err?.title || 'Dashboard data could not be loaded.';
            console.log(msg);
        }
    });
}

function renderStudentDashboardStats(data) {
    $('#totalEnrolledCourses').text(data.totalEnrolledCourses || 0);
    $('#activeCourseCount').text((data.activeCourseCount || 0) + ' active right now');

    $('#lessonsCompleted').text(data.lessonsCompleted || 0);
    $('#lessonsCompletedMeta').text('Will update after lesson progress starts');

    $('#averageGrade').text((data.averageGrade || 0) + '%');
    $('#averageGradeMeta').text('Will update after grading starts');

    $('#pendingTasks').text(data.pendingTasks || 0);
    $('#pendingTasksMeta').text('Will update after assignments start');
}