$(document).ready(function () {
    initStudentCourseContentPage();
});

function initStudentCourseContentPage() {
    bindStudentCourseContentTabs();

    var courseId = getStudentCourseIdFromUrl();

    if (!courseId) {
        showStudentCourseContentError('Invalid course selected.');
        return;
    }

    loadStudentCourseContentHeader(courseId);
    loadStudentCourseContentInfo(courseId);
    loadStudentCourseContentModules(courseId);
}

function getStudentCourseIdFromUrl() {
    var parts = window.location.pathname.split('/');
    var lastPart = parts[parts.length - 1];

    var courseId = parseInt(lastPart);

    if (isNaN(courseId) || courseId <= 0) {
        return null;
    }

    return courseId;
}

/* =========================
   HEADER / HERO
========================= */

function loadStudentCourseContentHeader(courseId) {
    $.ajax({
        url: '/api/student/course-content/' + courseId + '/header',
        type: 'GET',
        success: function (header) {
            $('#studentCourseDetailLoader').hide();
            $('#studentCourseDetailContent').show();

            renderStudentCourseContentHero(header);
        },
        error: function (xhr) {
            var message = getStudentCourseContentErrorMessage(
                xhr,
                'Course header could not be loaded.'
            );

            showStudentCourseContentError(message);
        }
    });
}

function renderStudentCourseContentHero(course) {
    var title = course.title || 'Untitled Course';
    var image = course.courseImagePath || '/img/course/course-4.webp';
    var trainer = course.trainerName || 'No Trainer';
    var progress = Number(course.progressPercentage || 0);

    if (progress < 0) {
        progress = 0;
    }

    if (progress > 100) {
        progress = 100;
    }

    $('#breadcrumbCourseTitle').text(title);

    $('#studentCourseHero').html(`
        <div class="dashboard-panel student-course-hero">
            <div class="student-course-hero-header">
                <div class="student-course-hero-title-wrap">
                    <div class="student-course-hero-thumb">
                        <img src="${escapeAttribute(image)}" alt="${escapeAttribute(title)}" />
                    </div>

                    <div class="student-course-hero-content">
                        <h1 class="student-course-hero-title">${escapeHtml(title)}</h1>

                        <div class="student-course-hero-meta-row">
                            <span>
                                <i class="bi bi-person"></i>
                                Instructor: <strong>${escapeHtml(trainer)}</strong>
                            </span>

                            <span>
                                <i class="bi bi-award"></i>
                                ${course.certificateIncluded ? 'Certificate Included' : 'Certificate Not Included'}
                            </span>
                        </div>
                    </div>
                </div>

                <a href="/Student/Courses/Index" class="dashboard-btn dashboard-btn-outline">
                    <i class="bi bi-arrow-left"></i> Back to My Courses
                </a>
            </div>

            <div class="student-course-hero-progress-wrap">
                <div class="student-course-hero-progress-top">
                    <span>Overall Progress</span>
                    <span>${progress}% Completed</span>
                </div>

                <div class="student-course-hero-progress">
                    <div class="student-course-hero-progress-fill" style="width:${progress}%;"></div>
                </div>
            </div>
        </div>
    `);
}

/* =========================
   COURSE INFO TAB
========================= */

function loadStudentCourseContentInfo(courseId) {
    $.ajax({
        url: '/api/student/course-content/' + courseId + '/info',
        type: 'GET',
        success: function (info) {
            renderStudentCourseContentInfo(info);
        },
        error: function (xhr) {
            var message = getStudentCourseContentErrorMessage(
                xhr,
                'Course info could not be loaded.'
            );

            $('#studentCourseInfoContainer').html(
                '<div class="dashboard-panel text-center p-4 text-danger">' +
                escapeHtml(message) +
                '</div>'
            );
        }
    });
}

function renderStudentCourseContentInfo(course) {
    var image = course.courseImagePath || '/img/course/course-4.webp';
    var title = course.title || 'Untitled Course';

    $('#studentCourseInfoContainer').html(`
        <div class="student-course-info-layout">
            <div class="dashboard-panel student-course-info-side-panel">
                <div class="student-course-side-image-wrap">
                    <img src="${escapeAttribute(image)}"
                         alt="${escapeAttribute(title)}"
                         class="student-course-side-image" />
                </div>

                <div class="student-course-side-block">
                    <div class="info-h">Instructor</div>

                    <div class="taught-by student-course-instructor-card">
                        <div>
                            <div class="teacher-name">${escapeHtml(course.trainerName || 'No Trainer')}</div>
                            <div class="muted">Course Instructor</div>
                        </div>
                    </div>
                </div>

                <div class="student-course-side-block">
                    <div class="info-h">Course details</div>

                    <div class="info-table student-course-details-table">
                        <div class="info-row">
                            <div class="info-key">Category</div>
                            <div class="info-val">${escapeHtml(course.category || 'N/A')}</div>
                        </div>

                        <div class="info-row">
                            <div class="info-key">Total Modules</div>
                            <div class="info-val">${course.totalModules || 0}</div>
                        </div>

                        <div class="info-row">
                            <div class="info-key">Total Lessons</div>
                            <div class="info-val">${course.totalLessons || 0}</div>
                        </div>

                        <div class="info-row">
                            <div class="info-key">Duration</div>
                            <div class="info-val">${escapeHtml(course.durationText || 'N/A')}</div>
                        </div>

                        <div class="info-row">
                            <div class="info-key">Batch Start</div>
                            <div class="info-val">${formatStudentCourseDate(course.batchStartDate)}</div>
                        </div>

                        <div class="info-row">
                            <div class="info-key">Batch End</div>
                            <div class="info-val">${formatStudentCourseDate(course.batchEndDate)}</div>
                        </div>

                        <div class="info-row">
                            <div class="info-key">Certificate</div>
                            <div class="info-val">${course.certificateIncluded ? 'Included' : 'Not Included'}</div>
                        </div>
                    </div>
                </div>
            </div>

            <div class="dashboard-panel student-course-info-main-panel">
                <div class="course-info-hero">
                    <div class="student-course-info-label">Course Overview</div>

                    <h3 class="page-h2 big-title">${escapeHtml(title)}</h3>

                    <p class="student-course-info-subtext">
                        Explore the full details of this course including description, instructor,
                        structure, and learning information.
                    </p>
                </div>

                <div class="student-course-info-section">
                    <div class="info-h">About this course</div>

                    <p class="student-course-info-text">
                        ${escapeHtml(course.description || 'No description added.')}
                    </p>
                </div>
            </div>
        </div>
    `);
}

/* =========================
   MODULES & LESSONS TAB
========================= */

function loadStudentCourseContentModules(courseId) {
    $('#studentModulesContainer').html(
        '<div class="dashboard-panel text-center p-4">Loading modules...</div>'
    );

    $.ajax({
        url: '/api/student/course-content/' + courseId + '/modules-lessons',
        type: 'GET',
        success: function (data) {
            renderStudentCourseContentModules(data.modules || []);
        },
        error: function (xhr) {
            var message = getStudentCourseContentErrorMessage(
                xhr,
                'Modules and lessons could not be loaded.'
            );

            $('#studentModulesContainer').html(
                '<div class="dashboard-panel text-center p-4 text-danger">' +
                escapeHtml(message) +
                '</div>'
            );
        }
    });
}

function renderStudentCourseContentModules(modules) {
    var container = $('#studentModulesContainer');
    container.empty();

    if (!modules || modules.length === 0) {
        container.html(
            '<div class="dashboard-panel text-center p-4">No modules added yet.</div>'
        );
        return;
    }

    modules.forEach(function (module, index) {
        var lessons = module.lessons || [];
        var quizzes = module.quizzes || [];
        var total = module.totalLessons || lessons.length;
        var completed = module.completedLessons || 0;
        var percent = module.progressPercentage || 0;
        var locked = module.isAccessible === false;

        var itemsHtml = '';

        lessons.forEach(function (lesson, lessonIndex) {
            var doneClass = lesson.isCompleted ? 'done' : '';
            var titleClass = lesson.isCompleted ? 'done-text' : '';
            var buttonText = lesson.isCompleted ? 'Completed' : 'Mark as done';
            var buttonClass = lesson.isCompleted ? 'done' : '';
            var disabled = lesson.isCompleted ? 'disabled' : '';

            itemsHtml += `
                <div class="student-lesson-item js-lesson ${doneClass}">
                    <div class="student-lesson-main">
                        <button type="button" class="student-lesson-toggle js-lesson-toggle">
                            <div class="student-lesson-left">
                                <div class="student-lesson-type-icon">
                                    <i class="bi bi-file-earmark-text"></i>
                                </div>

                                <div>
                                    <div class="student-lesson-title ${titleClass}">
                                        ${escapeHtml(lesson.title)}
                                    </div>
                                    <div class="student-lesson-meta">Lesson ${lessonIndex + 1}</div>
                                </div>
                            </div>

                            <span class="student-lesson-chevron">
                                <i class="bi bi-chevron-down"></i>
                            </span>
                        </button>

                        <div class="student-lesson-side">
                            <button type="button"
                                    class="student-done-btn js-done-btn ${buttonClass}"
                                    data-lesson-id="${lesson.id}"
                                    ${disabled}>
                                ${buttonText}
                            </button>
                        </div>
                    </div>

                    <div class="student-lesson-dropdown">
                        <p>${escapeHtml(lesson.description || 'No description added.')}</p>
                    </div>
                </div>
            `;
        });

        quizzes.forEach(function (quiz) {
            var quizStatus = quiz.isPassed
                ? 'Completed'
                : quiz.isLocked
                    ? 'Locked'
                    : 'Quiz Required';

            var quizMeta = quiz.isLocked && quiz.lockedUntil
                ? 'Locked until ' + new Date(quiz.lockedUntil).toLocaleString()
                : (quiz.questionsCount || 0) + ' Questions • Passing: 60% • Attempts: 3';

            itemsHtml += `
                <div class="student-lesson-item js-lesson js-quiz-lesson ${quiz.isPassed ? 'done' : ''}">
                    <div class="student-lesson-main">
                        <button type="button" class="student-lesson-toggle js-lesson-toggle">
                            <div class="student-lesson-left">
                                <div class="student-lesson-type-icon">
                                    <i class="bi bi-patch-question"></i>
                                </div>

                                <div>
                                    <div class="student-lesson-title ${quiz.isPassed ? 'done-text' : ''}">
                                        ${escapeHtml(quiz.title)}
                                    </div>
                                    <div class="student-lesson-meta">
                                        ${escapeHtml(quizStatus)} • ${escapeHtml(quizMeta)}
                                    </div>
                                </div>
                            </div>

                            <span class="student-lesson-chevron">
                                <i class="bi bi-chevron-down"></i>
                            </span>
                        </button>

                        <div class="student-lesson-side">
                            <button type="button" class="student-done-btn" disabled>
                                ${quiz.isPassed ? 'Completed' : 'Complete Quiz'}
                            </button>
                        </div>
                    </div>

                    <div class="student-lesson-dropdown">
                        <div class="student-quiz-box">
                            <div class="student-quiz-instructions">
                                <strong>Instructions:</strong>
                                ${escapeHtml(quiz.instructions || 'Complete this quiz to unlock the next module.')}
                            </div>

                            <div class="student-quiz-result">
                                Quiz attempt will be connected in the next step.
                            </div>
                        </div>
                    </div>
                </div>
            `;
        });

        container.append(`
            <div class="student-module-card ${index === 0 ? 'expanded' : ''} ${locked ? 'locked' : ''} js-module"
                 data-total="${total}">
                <button type="button" class="student-module-head">
                    <div class="student-module-head-left">
                        <div class="student-module-icon ${locked ? 'muted-lock' : module.isCompleted ? 'success' : 'primary'} js-module-icon">
                            <i class="bi ${locked ? 'bi-lock' : module.isCompleted ? 'bi-check-lg' : 'bi-book'}"></i>
                        </div>

                        <div>
                            <div class="student-module-title">${escapeHtml(module.title)}</div>
                            <div class="student-module-desc">${escapeHtml(module.description || '')}</div>
                        </div>
                    </div>

                    <div class="student-module-chevron">
                        <i class="bi bi-chevron-down"></i>
                    </div>
                </button>

                <div class="student-module-progress-row">
                    <span class="js-module-count">${completed}/${total} lessons completed</span>

                    <div class="student-module-progress-right">
                        <span class="js-module-percent">${percent}%</span>
                        <span class="student-status-badge ${locked ? 'muted' : module.isCompleted ? 'dark' : completed > 0 ? 'light' : 'muted'} js-module-status">
                            ${locked ? 'Locked' : escapeHtml(module.statusText || 'Not Started')}
                        </span>
                    </div>
                </div>

                <div class="student-module-progress">
                    <div class="student-module-progress-fill js-module-fill" style="width:${percent}%;"></div>
                </div>

                ${locked ? '' : '<div class="student-lesson-list">' + itemsHtml + '</div>'}
            </div>
        `);
    });

    bindStudentCourseContentModuleEvents();
}

function bindStudentCourseContentModuleEvents() {
    $(document).off('click', '.student-module-head').on('click', '.student-module-head', function () {
        var module = $(this).closest('.student-module-card');

        if (!module.hasClass('locked')) {
            module.toggleClass('expanded');
        }
    });

    $(document).off('click', '.js-lesson-toggle').on('click', '.js-lesson-toggle', function (e) {
        e.stopPropagation();
        $(this).closest('.js-lesson').toggleClass('open');
    });

    $(document).off('click', '.js-done-btn').on('click', '.js-done-btn', function (e) {
        e.stopPropagation();

        var button = $(this);
        var lessonId = button.data('lesson-id');

        if (!lessonId || button.prop('disabled')) {
            return;
        }

        button.prop('disabled', true).text('Saving...');

        $.ajax({
            url: '/api/student/course-content/lessons/' + lessonId + '/mark-done',
            type: 'POST',
            success: function () {
                var courseId = getStudentCourseIdFromUrl();

                loadStudentCourseContentModules(courseId);
                loadStudentCourseContentHeader(courseId);
                loadStudentCourseContentInfo(courseId);
            },
            error: function (xhr) {
                var message = getStudentCourseContentErrorMessage(
                    xhr,
                    'Lesson could not be marked as completed.'
                );

                alert(message);
                button.prop('disabled', false).text('Mark as done');
            }
        });
    });
}

/* =========================
   TABS
========================= */

function bindStudentCourseContentTabs() {
    $(document).off('click', '.student-course-tab').on('click', '.student-course-tab', function () {
        var target = $(this).data('tab');

        $('.student-course-tab').removeClass('active');
        $('.student-course-tab-panel').removeClass('active');

        $(this).addClass('active');
        $('#tab-' + target).addClass('active');
    });
}

/* =========================
   HELPERS
========================= */

function showStudentCourseContentError(message) {
    $('#studentCourseDetailLoader').html(
        '<div class="text-center p-4 text-danger">' +
        escapeHtml(message) +
        '</div>'
    );

    $('#studentCourseDetailLoader').show();
    $('#studentCourseDetailContent').hide();
}

function getStudentCourseContentErrorMessage(xhr, fallbackMessage) {
    var err = xhr.responseJSON;

    return err?.errorMessage ||
        err?.message ||
        err?.title ||
        fallbackMessage;
}

function formatStudentCourseDate(value) {
    if (!value) {
        return 'N/A';
    }

    var date = new Date(value);

    if (isNaN(date.getTime())) {
        return 'N/A';
    }

    return date.toLocaleDateString('en-US', {
        month: 'short',
        day: '2-digit',
        year: 'numeric'
    });
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
    return escapeHtml(value).replace(/`/g, '&#096;');
}