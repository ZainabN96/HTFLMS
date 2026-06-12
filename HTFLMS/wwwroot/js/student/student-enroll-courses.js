$(document).ready(function () {
    loadAvailableCourses();
});

function loadAvailableCourses() {
    $.ajax({
        url: '/api/StudentDashboard/available-courses',
        type: 'GET',

        success: function (courses) {
            renderAvailableCourses(courses);
        },

        error: function (xhr) {
            var err = xhr.responseJSON;
            var msg = err?.errorMessage || err?.message || err?.title || 'Available courses could not be loaded.';

            $('#studentAvailableCoursesContainer').html(`
                <div class="dashboard-panel">
                    <div class="text-danger">${msg}</div>
                </div>
            `);
        }
    });
}

function renderAvailableCourses(courses) {
    var container = $('#studentAvailableCoursesContainer');
    container.empty();

    if (!courses || courses.length === 0) {
        container.html(`
            <div class="dashboard-panel">
                <div class="text-center p-4">
                    <h4>No available courses</h4>
                    <p class="dashboard-muted-small">You are already enrolled in all available courses.</p>
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
        var duration = course.durationText || 'N/A';

        var card = `
            <div class="student-course-card">

                <div class="student-course-image-wrap">
                    <img src="${imagePath}" alt="${title}" class="student-course-image" />
                    <span class="student-course-tag">${category}</span>
                </div>

                <div class="student-course-body">
                    <h3 class="student-course-title">${title}</h3>
                    <p class="student-course-author">by ${trainerName}</p>

                    <div class="student-course-meta">
                        <div class="student-course-meta-item">
                            <i class="bi bi-clock"></i>
                            <span>${duration}</span>
                        </div>

                        <div class="student-course-meta-item">
                            <i class="bi bi-patch-check"></i>
                            <span>${course.certificateIncluded ? 'Certificate' : 'No Certificate'}</span>
                        </div>
                    </div>

                    <button type="button"
                            class="dashboard-btn dashboard-btn-primary w-100 mt-3"
                            onclick="enrollInCourse(${course.courseId})">
                        <i class="bi bi-plus-circle"></i>
                        Enroll Now
                    </button>
                </div>
            </div>
        `;

        container.append(card);
    });
}

function enrollInCourse(courseId) {
    $.ajax({
        url: '/api/student/enrollment',
        type: 'POST',
        contentType: 'application/json',
        data: JSON.stringify({
            courseId: courseId
        }),

        success: function () {
            alert('Course enrolled successfully.');
            loadAvailableCourses();
        },

        error: function (xhr) {
            var err = xhr.responseJSON;
            var msg = err?.errorMessage || err?.message || err?.title || 'Course could not be enrolled.';
            alert(msg);
        }
    });
}