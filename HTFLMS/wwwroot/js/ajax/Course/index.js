$(document).ready(function () {

    var successMessage = sessionStorage.getItem("successMessage");

    if (successMessage) {
        showSuccessPopup(successMessage);
        sessionStorage.removeItem("successMessage");
    }

    loadCourses();

    $('#courseSearchInput').on('keyup', function () {
        var searchText = $(this).val().toLowerCase();

        $('.trainer-courses-table-row').each(function () {
            var rowText = $(this).text().toLowerCase();
            $(this).toggle(rowText.includes(searchText));
        });
    });
});

$(document).ready(function () {
    loadCourses();

    $('#courseSearchInput').on('keyup', function () {
        var searchText = $(this).val().toLowerCase();

        $('.trainer-courses-table-row').each(function () {
            var rowText = $(this).text().toLowerCase();
            $(this).toggle(rowText.includes(searchText));
        });
    });
});

function loadCourses() {
    $.ajax({
        url: '/api/Course',
        type: 'GET',

        success: function (courses) {
            renderCourses(courses);
            updateCourseStats(courses);
        },

        error: function () {
            $('#coursesTableBody').html(`
                <div class="dashboard-table-row courses-table-row trainer-courses-table-row">
                    <div class="text-danger">Failed to load courses.</div>
                </div>
            `);
        }
    });
}

function renderCourses(courses) {
    var body = $('#coursesTableBody');
    body.empty();

    if (!courses || courses.length === 0) {
        body.html(`
            <div class="dashboard-table-row courses-table-row trainer-courses-table-row">
                <div>No courses found.</div>
            </div>
        `);

        $('#courseCountText').text('0 course(s)');
        return;
    }

    $('#courseCountText').text(courses.length + ' course(s)');

    $.each(courses, function (index, course) {
        var title = course.title || 'Untitled Course';
        var category = course.category || 'N/A';
        var students = course.totalStudents || 0;

        var imagePath = course.courseImagePath || '/img/course/course-1.webp';

        var status = course.isPublished ? 'Active' : 'Draft';
        var statusClass = course.isPublished ? 'pill-green' : 'pill-yellow';

        var createdDate = course.createdAt
            ? new Date(course.createdAt).toLocaleDateString('en-US', {
                month: 'short',
                day: '2-digit',
                year: 'numeric'
            })
            : 'N/A';

        var row = `
            <div class="dashboard-table-row courses-table-row trainer-courses-table-row">
                <div class="courses-coursecell dashboard-table-cell-ellipsis" title="${title}">
                    <div class="courses-title-wrap">
                        <div class="course-thumb">
                            <img src="${imagePath}" alt="${title}" />
                        </div>
                        <div class="courses-title-block">
                            <div class="courses-title" title="${title}">${title}</div>
                            <div class="courses-sub-text dashboard-table-cell-ellipsis" title="${category}">${category}</div>
                        </div>
                    </div>
                </div>

                <div class="dashboard-table-cell-ellipsis" title="${category}">
                    <span class="trainer-course-chip">${category}</span>
                </div>

                <div class="dashboard-table-cell-ellipsis" title="${students}">
                    ${students}
                </div>

                <div title="${status}">
                    <span class="pill ${statusClass}">${status}</span>
                </div>

                <div class="dashboard-table-cell-ellipsis" title="${createdDate}">
                    ${createdDate}
                </div>

                <div class="trainer-courses-actions">
                    <a href="/Trainer/Courses/Edit/${course.id}"
                       class="dashboard-btn dashboard-btn-outline trainer-course-action-btn">
                        Edit
                    </a>

                    <button type="button"
                            class="dashboard-btn trainer-delete-soft-btn trainer-course-action-btn">
                        Delete
                    </button>
                </div>
            </div>
        `;

        body.append(row);
    });
}

function updateCourseStats(courses) {
    var total = courses.length;
    var active = courses.filter(c => c.isPublished === true).length;
    var draft = total - active;

    var students = courses.reduce(function (sum, course) {
        return sum + (course.totalStudents || 0);
    }, 0);

    $('#totalCoursesCount').text(total);
    $('#activeCoursesCount').text(active);
    $('#draftCoursesCount').text(draft);
    $('#totalStudentsCount').text(students);
}