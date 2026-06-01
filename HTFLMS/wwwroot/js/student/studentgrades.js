$(document).ready(function () {
    loadStudentGrades();
});

function loadStudentGrades() {
    setGradesLoadingState();

    $.ajax({
        url: '/api/StudentGrades',
        type: 'GET',
        success: function (response) {
            if (!response || response.success !== true || !response.data) {
                showGradesError('Unable to load grades.');
                return;
            }

            bindGradesPage(response.data);
        },
        error: function () {
            showGradesError('Unable to load grades. Please refresh the page and try again.');
        }
    });
}

function bindGradesPage(data) {
    bindSummaryCards(data.summary);
    bindCourseGrades(data.courses, data.emptyMessage);
    bindRecentResults(data.recentResults);
}

function bindSummaryCards(summary) {
    $('#averageTitle').text(summary.averageTitle || 'Current Average');
    $('#averageValue').text(formatPercentage(summary.averagePercentage));
    $('#averageMeta').text(summary.averageMeta || 'Grades will appear after your work is marked.');

    $('#coursesCompletedValue').text(summary.coursesCompleted || 0);

    $('#completedAssignmentsValue').text(summary.completedAssignments || 0);

    var pendingReviews = summary.pendingReviews || 0;
    $('#completedAssignmentsMeta').text(pendingReviews + ' pending review');

    $('#pendingReviewsValue').text(pendingReviews);
    $('#pendingReviewsMeta').text('Submitted work awaiting marks');
}

function bindCourseGrades(courses, emptyMessage) {
    var $list = $('#studentCourseGradeList');
    var $count = $('#studentCourseCount');

    $list.empty();

    if (!courses || courses.length === 0) {
        $count.text('0 course(s)');
        $list.html(
            '<div class="student-grade-empty">' +
            escapeHtml(emptyMessage || 'No grade record found yet.') +
            '</div>'
        );
        return;
    }

    $count.text(courses.length + ' course(s)');

    courses.forEach(function (course) {
        var imagePath = getCourseImagePath(course.courseImagePath);
        var percentage = Number(course.averagePercentage || 0);
        var progress = Number(course.assignmentProgressPercentage || 0);

        var html =
            '<div class="student-course-grade-card">' +
            '<div class="student-course-grade-top">' +
            '<div class="student-course-grade-left">' +
            '<div class="student-course-grade-thumb">' +
            '<img src="' + escapeAttribute(imagePath) + '" alt="' + escapeAttribute(course.courseTitle || 'Course') + '" />' +
            '</div>' +

            '<div class="student-course-grade-info">' +
            '<h4 class="student-course-grade-title">' + escapeHtml(course.courseTitle || '') + '</h4>' +
            '<div class="student-course-grade-meta">' +
            '<span><i class="bi bi-person"></i> ' + escapeHtml(course.trainerName || 'Not assigned') + '</span>' +
            '<span>•</span>' +
            '<span><i class="bi bi-check2-square"></i> ' + (course.gradedAssignments || 0) + ' graded assignment(s)</span>' +
            '<span>•</span>' +
            '<span><i class="bi bi-hourglass-split"></i> ' + (course.pendingReviews || 0) + ' pending</span>' +
            '</div>' +
            '</div>' +
            '</div>' +

            '<div class="student-course-grade-right">' +
            '<span class="student-course-grade-badge ' + escapeAttribute(course.gradeBadgeClass || 'fair') + '">' + escapeHtml(course.gradeBadgeText || 'No Grade') + '</span>' +
            '<div class="student-course-grade-score">' + formatPercentage(percentage) + '</div>' +
            '</div>' +
            '</div>' +

            '<div class="student-course-grade-progress-wrap">' +
            '<div class="student-course-grade-progress-top">' +
            '<span>Assignment grading progress</span>' +
            '<span>' + (course.gradedAssignments || 0) + '/' + (course.totalAssignments || 0) + ' graded</span>' +
            '</div>' +
            '<div class="student-course-grade-progress">' +
            '<div class="student-course-grade-progress-fill" data-progress="' + progress + '"></div>' +
            '</div>' +
            '</div>' +
            '</div>';

        $list.append(html);
    });

    $('.student-course-grade-progress-fill').each(function () {
        var progress = Number($(this).attr('data-progress') || 0);
        if (progress < 0) progress = 0;
        if (progress > 100) progress = 100;

        $(this).css('width', progress + '%');
    });
}

function bindRecentResults(results) {
    var $list = $('#studentRecentResultsList');
    $list.empty();

    if (!results || results.length === 0) {
        $list.html(
            '<div class="student-grade-empty">No marked assignments yet.</div>'
        );
        return;
    }

    results.forEach(function (item) {
        var html =
            '<div class="student-grade-activity-item">' +
            '<div class="student-grade-activity-top">' +
            '<div class="student-grade-activity-title">' + escapeHtml(item.assignmentTitle || 'Assignment') + ' graded</div>' +
            '<span class="student-grade-activity-mark ' + escapeAttribute(item.gradeClass || 'fair') + '">' + escapeHtml(item.markText || '') + '</span>' +
            '</div>' +
            '<div class="student-grade-activity-sub">' + escapeHtml(item.courseTitle || '') + '</div>' +
            '</div>';

        $list.append(html);
    });
}

function setGradesLoadingState() {
    $('#studentCourseGradeList').html('<div class="student-grade-empty">Loading grades...</div>');
    $('#studentRecentResultsList').html('<div class="student-grade-empty">Loading recent results...</div>');
}

function showGradesError(message) {
    $('#studentCourseGradeList').html('<div class="student-grade-empty">' + escapeHtml(message) + '</div>');
    $('#studentRecentResultsList').html('<div class="student-grade-empty">No recent results available.</div>');
}

function formatPercentage(value) {
    var number = Number(value || 0);

    if (Number.isInteger(number)) {
        return number + '%';
    }

    return number.toFixed(1) + '%';
}

function getCourseImagePath(path) {
    if (!path || path.trim() === '') {
        return '/img/course/course-3.webp';
    }

    if (path.startsWith('/')) {
        return path;
    }

    return '/' + path;
}

function escapeHtml(value) {
    if (value === null || value === undefined) {
        return '';
    }

    return String(value)
        .replace(/&/g, '&amp;')
        .replace(/</g, '&lt;')
        .replace(/>/g, '&gt;')
        .replace(/"/g, '&quot;')
        .replace(/'/g, '&#039;');
}

function escapeAttribute(value) {
    return escapeHtml(value);
}