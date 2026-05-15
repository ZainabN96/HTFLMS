$(document).ready(function () {
    loadStudentMyCourses();
});

function loadStudentMyCourses() {
    $.ajax({
        url: '/api/StudentDashboard',
        type: 'GET',

        success: function (data) {
            renderStudentMyCourses(data.enrolledCourses);
        },

        error: function (xhr) {
            var err = xhr.responseJSON;
            var msg = err?.errorMessage || err?.message || err?.title || 'My courses could not be loaded.';

            $('#studentMyCoursesContainer').html(`
                <div class="dashboard-panel">
                    <div class="text-danger">${msg}</div>
                </div>
            `);
        }
    });
}

function renderStudentMyCourses(courses) {
    var container = $('#studentMyCoursesContainer');
    container.empty();

    if (!courses || courses.length === 0) {
        container.html(`
            <div class="dashboard-panel">
                <div class="text-center p-4">
                    <h4>No enrolled course yet</h4>
                    <p class="dashboard-muted-small">Browse available courses and enroll to start learning.</p>

                    <a href="/Courses/CoursesIndex" class="dashboard-btn dashboard-btn-outline">
                        Browse Courses
                    </a>
                </div>
            </div>
        `);

        return;
    }

    $.each(courses, function (index, course) {
        var imagePath = course.courseImagePath || '/img/course/course-1.webp';
        var title = course.title || 'Untitled Course';
        var category = course.category || 'N/A';
        var trainerName = course.trainerName || 'No Trainer';
        var progress = course.progressPercentage || 0;

        var batchStart = course.batchStartDate
            ? new Date(course.batchStartDate).toLocaleDateString('en-US', {
                month: 'short',
                day: '2-digit',
                year: 'numeric'
            })
            : 'N/A';

        var card = `
            <a href="/Student/Courses/Details/${course.courseId}"
               class="student-course-card">

                <div class="student-course-image-wrap">
                    <img src="${imagePath}" alt="${title}" class="student-course-image" />
                    <span class="student-course-tag">${category}</span>
                </div>

                <div class="student-course-body">
                    <h3 class="student-course-title">${title}</h3>
                    <p class="student-course-author">by ${trainerName}</p>

                    <div class="student-course-progress-top">
                        <span>Progress</span>
                        <span>${progress}%</span>
                    </div>

                    <div class="student-course-progress">
                        <div class="student-course-progress-fill" style="width: ${progress}%;"></div>
                    </div>

                    <div class="student-course-meta">
                        <div class="student-course-meta-item">
                            <i class="bi bi-book"></i>
                            <span>${course.enrollmentStatus || 'Active'}</span>
                        </div>

                        <div class="student-course-meta-item">
                            <i class="bi bi-clock"></i>
                            <span>${batchStart}</span>
                        </div>
                    </div>
                </div>
            </a>
        `;

        container.append(card);
    });
}