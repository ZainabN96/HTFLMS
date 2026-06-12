$(document).ready(function () {
    loadStudentCourseDetail();
    bindStudentCourseTabs();
});

function loadStudentCourseDetail() {
    var courseId = window.location.pathname.split('/').pop();

    $.ajax({
        url: '/api/student/course-details/' + courseId,
        type: 'GET',

        success: function (course) {
            $('#studentCourseDetailLoader').hide();
            $('#studentCourseDetailContent').show();

            renderCourseHero(course);
            renderModulesAndLessons(course.modules || []);
            renderSlidesAndAssignments(course.modules || []);
            renderCourseInfo(course);
        },

        error: function (xhr) {
            var err = xhr.responseJSON;
            var msg = err?.errorMessage || err?.message || err?.title || 'Course details could not be loaded.';

            $('#studentCourseDetailLoader').html(
                '<div class="text-center p-4 text-danger">' + escapeHtml(msg) + '</div>'
            );
        }
    });
}

function renderCourseHero(course) {
    var title = course.title || 'Untitled Course';
    var image = course.courseImagePath || '/img/course/course-4.webp';
    var trainer = course.trainerName || 'No Trainer';
    var progress = course.progressPercentage || 0;

    $('#breadcrumbCourseTitle').text(title);

    $('#studentCourseHero').html(`
        <div class="dashboard-panel student-course-hero">
            <div class="student-course-hero-header">
                <div class="student-course-hero-title-wrap">
                    <div class="student-course-hero-thumb">
                        <img src="${image}" alt="${escapeHtml(title)}" />
                    </div>

                    <div class="student-course-hero-content">
                        <h1 class="student-course-hero-title">${escapeHtml(title)}</h1>

                        <div class="student-course-hero-meta-row">
                            <span><i class="bi bi-person"></i> Instructor: <strong>${escapeHtml(trainer)}</strong></span>
                            <span><i class="bi bi-award"></i> ${course.certificateIncluded ? 'Certificate Included' : 'Certificate Not Included'}</span>
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

function renderModulesAndLessons(modules) {
    var container = $('#studentModulesContainer');
    container.empty();

    if (!modules.length) {
        container.html('<div class="dashboard-panel text-center p-4">No modules added yet.</div>');
        return;
    }

    modules.forEach(function (module, index) {
        var lessons = module.lessons || [];
        var quizzes = module.quizzes || [];
        var total = lessons.length + quizzes.length;
        var locked = module.isAccessible === false;

        var itemsHtml = '';

        lessons.forEach(function (lesson, lessonIndex) {
            itemsHtml += `
                <div class="student-lesson-item js-lesson">
                    <div class="student-lesson-main">
                        <button type="button" class="student-lesson-toggle js-lesson-toggle">
                            <div class="student-lesson-left">
                                <div class="student-lesson-type-icon">
                                    <i class="bi bi-file-earmark-text"></i>
                                </div>
                                <div>
                                    <div class="student-lesson-title">${escapeHtml(lesson.title)}</div>
                                    <div class="student-lesson-meta">Lesson ${lessonIndex + 1}</div>
                                </div>
                            </div>

                            <span class="student-lesson-chevron">
                                <i class="bi bi-chevron-down"></i>
                            </span>
                        </button>

                        <div class="student-lesson-side">
                            <button type="button" class="student-done-btn js-done-btn">Mark as done</button>
                        </div>
                    </div>

                    <div class="student-lesson-dropdown">
                        <p>${escapeHtml(lesson.description || 'No description added.')}</p>
                    </div>
                </div>
            `;
        });

        quizzes.forEach(function (quiz, quizIndex) {
            var questions = quiz.questions || [];

            var questionsHtml = questions.length
                ? questions.map(function (q, qIndex) {
                    var options = q.options || [];

                    var optionsHtml = options.map(function (option) {
                        return `
                    <label>
                        <input type="radio"
                               name="quiz_${quiz.id}_q_${q.id}"
                               value="${option.id}"
                               data-correct="${option.isCorrect}">
                        ${escapeHtml(option.optionText)}
                    </label>
                `;
                    }).join('');

                    return `
                <div class="student-quiz-question">
                    <p>${qIndex + 1}. ${escapeHtml(q.questionText)}</p>
                    ${optionsHtml}
                </div>
            `;
                }).join('')
                : '<p>No questions added in this quiz yet.</p>';

            itemsHtml += `
        <div class="student-lesson-item js-lesson js-quiz-lesson" data-max-attempts="${quiz.attemptsAllowed || 5}">
            <div class="student-lesson-main">
                <button type="button" class="student-lesson-toggle js-lesson-toggle">
                    <div class="student-lesson-left">
                        <div class="student-lesson-type-icon">
                            <i class="bi bi-patch-question"></i>
                        </div>

                        <div>
                            <div class="student-lesson-title">${escapeHtml(quiz.title)}</div>
                            <div class="student-lesson-meta">Quiz • ${quiz.questionsCount || questions.length} Questions</div>
                        </div>
                    </div>

                    <span class="student-lesson-chevron">
                        <i class="bi bi-chevron-down"></i>
                    </span>
                </button>

                <div class="student-lesson-side">
                    <button type="button" class="student-done-btn" disabled>Complete quiz to finish</button>
                </div>
            </div>

            <div class="student-lesson-dropdown">
                <div class="student-quiz-box">
                    <div class="student-quiz-attempts">
                        Attempts left: <span class="js-attempts-left">${quiz.attemptsAllowed || 5}</span>
                    </div>

                    <div class="student-quiz-instructions">
                        <strong>Instructions:</strong> ${escapeHtml(quiz.instructions || 'Answer the following questions.')}
                    </div>

                    ${questionsHtml}

                    <div class="student-quiz-actions">
                        <button type="button" class="dashboard-btn dashboard-btn-outline js-submit-quiz">
                            Submit Quiz
                        </button>
                        <div class="student-quiz-result js-quiz-result"></div>
                    </div>
                </div>
            </div>
        </div>
    `;
        });
        container.append(`
            <div class="student-module-card ${index === 0 ? 'expanded' : ''} ${locked ? 'locked' : ''} js-module" data-total="${total}">
                <button type="button" class="student-module-head">
                    <div class="student-module-head-left">
                        <div class="student-module-icon ${locked ? 'muted-lock' : 'primary'} js-module-icon">
                            <i class="bi ${locked ? 'bi-lock' : 'bi-book'}"></i>
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
                    <span class="js-module-count">0/${total} lessons completed</span>
                    <div class="student-module-progress-right">
                        <span class="js-module-percent">0%</span>
                        <span class="student-status-badge muted js-module-status">${locked ? 'Locked' : 'Not Started'}</span>
                    </div>
                </div>

                <div class="student-module-progress">
                    <div class="student-module-progress-fill js-module-fill" style="width:0%;"></div>
                </div>

                ${locked ? '' : `<div class="student-lesson-list">${itemsHtml}</div>`}
            </div>
        `);
    });

    bindDynamicModuleEvents();
}

function renderSlidesAndAssignments(modules) {
    var container = $('#studentSlidesAssignmentsContainer');
    container.empty();

    if (!modules.length) {
        container.html('<div class="dashboard-panel text-center p-4">No materials or assignments added yet.</div>');
        return;
    }

    modules.forEach(function (module, index) {
        var materials = module.materials || [];
        var assignments = module.assignments || [];

        var materialsHtml = materials.length
            ? materials.map(function (m) {
                var link = m.filePath || m.externalUrl || '#';

                return `
                    <div class="student-material-card">
                        <div class="student-material-main">
                            <div class="student-material-left">
                                <div class="student-material-icon ${getMaterialClass(m.contentType)}">
                                    <i class="bi ${getMaterialIcon(m.contentType)}"></i>
                                </div>

                                <div class="student-material-content">
                                    <h4 class="student-material-title">${escapeHtml(m.title)}</h4>
                                    <div class="student-material-meta">
                                        <span>Type: ${escapeHtml(m.contentType || 'File')}</span>
                                        ${getMaterialExtra(m)}
                                    </div>
                                </div>
                            </div>

                            <div class="student-material-right">
                                <a href="${link}" target="_blank" class="student-material-btn">
                                    <i class="bi bi-eye"></i> View File
                                </a>

                                ${m.filePath ? `
                                    <a href="${m.filePath}" download class="student-material-btn download">
                                        <i class="bi bi-download"></i> Download
                                    </a>
                                ` : ''}
                            </div>
                        </div>
                    </div>
                `;
            }).join('')
            : '<p class="student-panel-sub">No slides or videos added.</p>';

        var assignmentsHtml = assignments.length
            ? assignments.map(function (a) {
                var due = a.dueDateTime ? new Date(a.dueDateTime).toLocaleString() : 'No due date';

                return `
                    <div class="student-assignment-card">
                        <div class="student-assignment-main">
                            <div class="student-assignment-left">
                                <h4 class="student-assignment-title">${escapeHtml(a.title)}</h4>

                                <div class="student-assignment-meta">
                                    <span>Marks: ${a.marks || 0}</span>
                                    <span>•</span>
                                    <span>Due: ${due}</span>
                                </div>

                                <p class="student-panel-sub">${escapeHtml(a.description || '')}</p>
                            </div>

                            <div class="student-assignment-right">
                                <span class="student-assignment-status pending">Pending Submission</span>

                                ${a.filePath ? `
                                    <a href="${a.filePath}" target="_blank" class="student-assignment-link">
                                        <i class="bi bi-download"></i> View Assignment
                                    </a>
                                ` : ''}
                            </div>
                        </div>
                    </div>
                `;
            }).join('')
            : '<p class="student-panel-sub">No assignments added.</p>';

        container.append(`
            <div class="dashboard-panel">
                <div class="dashboard-panel-head student-panel-head">
                    <div>
                        <h3 class="page-h2 student-panel-title">${escapeHtml(module.title)}</h3>
                        <p class="student-panel-sub">${escapeHtml(module.description || 'Slides, videos, and assignments for this module.')}</p>
                    </div>
                </div>

                <div class="student-module-section-block">
                    <div class="dashboard-panel-head student-panel-head">
                        <div>
                            <h4 class="page-h2 student-panel-title">Slides &amp; Videos</h4>
                            <p class="student-panel-sub">Students can view course material uploaded by the trainer.</p>
                        </div>

                        <div class="student-count-pill">Total: ${materials.length} files</div>
                    </div>

                    <div class="student-material-list">${materialsHtml}</div>
                </div>

                <div class="student-module-section-block">
                    <div class="dashboard-panel-head student-panel-head">
                        <div>
                            <h4 class="page-h2 student-panel-title">Assignments</h4>
                            <p class="student-panel-sub">Students can view assignments for this module.</p>
                        </div>

                        <div class="student-count-pill">Total: ${assignments.length} assignments</div>
                    </div>

                    <div class="student-assignment-list">${assignmentsHtml}</div>
                </div>
            </div>
        `);
    });
}

function renderCourseInfo(course) {
    $('#studentCourseInfoContainer').html(`
        <div class="student-course-info-layout">
            <div class="dashboard-panel student-course-info-side-panel">
                <div class="student-course-side-image-wrap">
                    <img src="${course.courseImagePath || '/img/course/course-4.webp'}" alt="${escapeHtml(course.title)}" class="student-course-side-image" />
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
                            <div class="info-val">${(course.modules || []).length}</div>
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
                    <h3 class="page-h2 big-title">${escapeHtml(course.title || 'Untitled Course')}</h3>
                    <p class="student-course-info-subtext">
                        Explore the full details of this course including description, instructor, structure, and learning information.
                    </p>
                </div>

                <div class="student-course-info-section">
                    <div class="info-h">About this course</div>
                    <p class="student-course-info-text">${escapeHtml(course.description || 'No description added.')}</p>
                </div>
            </div>
        </div>
    `);
}

function bindStudentCourseTabs() {
    $(document).on('click', '.student-course-tab', function () {
        var target = $(this).data('tab');

        $('.student-course-tab').removeClass('active');
        $('.student-course-tab-panel').removeClass('active');

        $(this).addClass('active');
        $('#tab-' + target).addClass('active');
    });
}

function bindDynamicModuleEvents() {
    $(document).off('click', '.student-module-head').on('click', '.student-module-head', function () {
        var module = $(this).closest('.student-module-card');

        if (!module.hasClass('locked')) {
            module.toggleClass('expanded');
        }
    });

    $(document).off('click', '.js-lesson-toggle').on('click', '.js-lesson-toggle', function (e) {
        e.stopPropagation();
        $(this).closest('.student-lesson-item').toggleClass('open');
    });

    $(document).off('click', '.js-done-btn').on('click', '.js-done-btn', function (e) {
        e.stopPropagation();

        var lesson = $(this).closest('.js-lesson');
        var module = $(this).closest('.js-module');

        lesson.addClass('done');
        $(this).text('Completed').addClass('done').prop('disabled', true);

        updateModuleProgress(module);
    });

    $(document).off('click', '.js-submit-quiz').on('click', '.js-submit-quiz', function (e) {
        e.stopPropagation();

        var quizLesson = $(this).closest('.js-quiz-lesson');
        var module = $(this).closest('.js-module');
        var resultBox = quizLesson.find('.js-quiz-result');
        var attemptsBox = quizLesson.find('.js-attempts-left');

        var maxAttempts = parseInt(quizLesson.attr('data-max-attempts')) || 5;
        var usedAttempts = parseInt(quizLesson.attr('data-used-attempts')) || 0;

        var questions = quizLesson.find('.student-quiz-question');
        var allAnswered = true;
        var allCorrect = true;

        questions.each(function () {
            var selected = $(this).find('input[type="radio"]:checked');

            if (selected.length === 0) {
                allAnswered = false;
                allCorrect = false;
                return;
            }

            if (selected.attr('data-correct') !== 'true') {
                allCorrect = false;
            }
        });

        if (!allAnswered) {
            resultBox
                .text('Please answer all questions first.')
                .removeClass('success')
                .addClass('error');
            return;
        }

        usedAttempts++;
        quizLesson.attr('data-used-attempts', usedAttempts);

        var attemptsLeft = maxAttempts - usedAttempts;
        attemptsBox.text(attemptsLeft);

        if (allCorrect) {
            resultBox
                .text('Great job. Quiz completed successfully.')
                .removeClass('error')
                .addClass('success');

            quizLesson.addClass('done');

            quizLesson.find('.student-done-btn')
                .text('Completed')
                .addClass('done')
                .prop('disabled', true);

            $(this).prop('disabled', true);

            updateModuleProgress(module);
        } else {
            if (attemptsLeft > 0) {
                resultBox
                    .text('Some answers are incorrect. Try again.')
                    .removeClass('success')
                    .addClass('error');
            } else {
                resultBox
                    .text('You have used all attempts.')
                    .removeClass('success')
                    .addClass('error');

                $(this).prop('disabled', true);
                quizLesson.find('input[type="radio"]').prop('disabled', true);
            }
        }
    });
}

function updateModuleProgress(module) {
    var total = parseInt(module.attr('data-total')) || 0;
    var done = module.find('.js-lesson.done').length;
    var percent = total > 0 ? Math.round((done / total) * 100) : 0;

    module.find('.js-module-count').text(done + '/' + total + ' lessons completed');
    module.find('.js-module-percent').text(percent + '%');
    module.find('.js-module-fill').css('width', percent + '%');

    var status = module.find('.js-module-status');
    status.removeClass('muted light dark');

    if (done === 0) {
        status.text('Not Started').addClass('muted');
    } else if (done < total) {
        status.text('In Progress').addClass('light');
    } else {
        status.text('Completed').addClass('dark');
    }
}

function getMaterialExtra(m) {
    if (m.pages) return '<span>•</span><span>' + m.pages + ' Pages</span>';
    if (m.slides) return '<span>•</span><span>' + m.slides + ' Slides</span>';
    if (m.minutes) return '<span>•</span><span>' + m.minutes + ' Min</span>';
    return '';
}

function getMaterialClass(type) {
    type = (type || '').toLowerCase();

    if (type.includes('pdf')) return 'pdf';
    if (type.includes('ppt') || type.includes('slide')) return 'ppt';
    if (type.includes('video')) return 'video';

    return 'pdf';
}

function getMaterialIcon(type) {
    type = (type || '').toLowerCase();

    if (type.includes('pdf')) return 'bi-file-earmark-pdf';
    if (type.includes('ppt') || type.includes('slide')) return 'bi-file-earmark-slides';
    if (type.includes('video')) return 'bi-play-btn';

    return 'bi-file-earmark-text';
}

function escapeHtml(value) {
    if (value === null || value === undefined) return '';

    return String(value)
        .replaceAll('&', '&amp;')
        .replaceAll('<', '&lt;')
        .replaceAll('>', '&gt;')
        .replaceAll('"', '&quot;')
        .replaceAll("'", '&#039;');
}