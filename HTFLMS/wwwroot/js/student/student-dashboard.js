$(document).ready(function () {
    loadStudentDashboard();
});

function loadStudentDashboard() {
    $.ajax({
        url: '/api/StudentDashboard',
        type: 'GET',

        success: function (data) {
            renderStudentDashboardStats(data);
            renderUpcomingDeadlines(data.upcomingDeadlines || []);
        },

        error: function (xhr) {
            var err = xhr.responseJSON;
            var msg = err?.errorMessage || err?.message || err?.title || 'Dashboard data could not be loaded.';
            console.log(msg);

            renderUpcomingDeadlines([]);
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

    if ((data.pendingTasks || 0) > 0) {
        $('#pendingTasksMeta').text('Assignments need attention');
    } else {
        $('#pendingTasksMeta').text('No pending assignments');
    }
}

function renderUpcomingDeadlines(deadlines) {
    var table = $('#upcomingDeadlinesTable');

    if (!table.length) {
        return;
    }

    table.find('.dashboard-table-row:not(.dashboard-table-head)').remove();

    if (!deadlines || deadlines.length === 0) {
        table.append(`
            <div class="dashboard-table-row">
                <div class="dashboard-cell-strong">No upcoming deadlines</div>
                <div>-</div>
                <div>-</div>
                <div><span class="pill pill-green">Clear</span></div>
            </div>
        `);

        return;
    }

    deadlines.forEach(function (item) {
        var statusText = item.status || 'Pending';
        var statusClass = getDeadlinePillClass(item.statusClass || item.status);
        var redirectUrl = item.redirectUrl || '#';

        table.append(`
            <div class="dashboard-table-row dashboard-deadline-row" data-url="${escapeHtmlAttribute(redirectUrl)}">
                <div class="dashboard-cell-strong">${escapeHtml(item.assignmentTitle)}</div>
                <div>${escapeHtml(item.courseTitle)}</div>
                <div>${escapeHtml(item.dueText)}</div>
                <div><span class="${statusClass}">${escapeHtml(statusText)}</span></div>
            </div>
        `);
    });

    $('.dashboard-deadline-row').off('click').on('click', function () {
        var url = $(this).data('url');

        if (url && url !== '#') {
            window.location.href = url;
        }
    });
}

function getDeadlinePillClass(status) {
    var value = (status || '').toString().toLowerCase();

    if (value === 'danger' || value === 'overdue') {
        return 'pill pill-red';
    }

    return 'pill pill-yellow';
}

function escapeHtml(value) {
    if (value === null || value === undefined) {
        return '';
    }

    return value
        .toString()
        .replace(/&/g, '&amp;')
        .replace(/</g, '&lt;')
        .replace(/>/g, '&gt;')
        .replace(/"/g, '&quot;')
        .replace(/'/g, '&#039;');
}

function escapeHtmlAttribute(value) {
    return escapeHtml(value).replace(/`/g, '&#096;');
}







//$(document).ready(function () {
//    loadStudentDashboard();
//});

//function loadStudentDashboard() {
//    $.ajax({
//        url: '/api/StudentDashboard',
//        type: 'GET',

//        success: function (data) {
//            renderStudentDashboardStats(data);
//        },

//        error: function (xhr) {
//            var err = xhr.responseJSON;
//            var msg = err?.errorMessage || err?.message || err?.title || 'Dashboard data could not be loaded.';
//            console.log(msg);
//        }
//    });
//}

//function renderStudentDashboardStats(data) {
//    $('#totalEnrolledCourses').text(data.totalEnrolledCourses || 0);
//    $('#activeCourseCount').text((data.activeCourseCount || 0) + ' active right now');

//    $('#lessonsCompleted').text(data.lessonsCompleted || 0);
//    $('#lessonsCompletedMeta').text('Will update after lesson progress starts');

//    $('#averageGrade').text((data.averageGrade || 0) + '%');
//    $('#averageGradeMeta').text('Will update after grading starts');

//    $('#pendingTasks').text(data.pendingTasks || 0);
//    $('#pendingTasksMeta').text('Will update after assignments start');
//}