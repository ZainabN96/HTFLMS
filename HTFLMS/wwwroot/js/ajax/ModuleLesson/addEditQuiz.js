$(document).ready(function () {

    var courseId = getInitialCourseId();
    var moduleId = getInitialModuleId();
    var quizId = $('#quizId').val();
    var isEdit = quizId && quizId !== '' && quizId !== '0';

    if (!courseId || courseId === '0') {
        $('.error-box').html('<div>Please go back and select a course first.</div>');
        return;
    }

    if (isEdit) {
        setEditMode();
    }

    loadModulesForQuiz(courseId, moduleId, function () {
        if (isEdit) {
            loadQuiz(quizId);
        } else {
            addQuestion();
        }
    });

    $('#addQuestionBtn').on('click', function () {
        addQuestion();
    });

    $(document).on('click', '.remove-question-btn', function () {
        $(this).closest('.trainer-question-builder-card').remove();
        renumberQuestions();
    });

    $('#quizCreateForm').on('submit', function (e) {
        e.preventDefault();

        var selectedModuleId = $('#moduleSelect').val();
        var btn = $('#saveQuizBtn');

        setButtonLoading(btn, isEdit);

        $.ajax({
            url: getQuizSaveUrl(quizId, isEdit),
            type: isEdit ? 'PUT' : 'POST',
            contentType: 'application/json',
            data: JSON.stringify(buildQuizDto()),

            success: function (response) {
                sessionStorage.setItem("selectedCourseId", courseId);

                if (selectedModuleId && selectedModuleId !== '0') {
                    sessionStorage.setItem("selectedModuleId", selectedModuleId);
                } else {
                    sessionStorage.removeItem("selectedModuleId");
                }

                if (response?.message) {
                    sessionStorage.setItem("successMessage", response.message);
                }

                window.location.href = "/Trainer/ModulesLessons";
            },

            error: function (xhr) {
                console.log(xhr.responseText);
                showErrorMessage(xhr);
            },

            complete: function () {
                resetSaveButton(btn, isEdit);
            }
        });
    });

    $(document).on('click', '#cancelBtn', function (e) {
        e.preventDefault();

        var courseId = $('#courseId').val();
        var moduleId = $('#moduleSelect').val() || $('#moduleId').val();

        if (courseId && courseId !== '0') {
            sessionStorage.setItem("selectedCourseId", courseId);
        }

        if (moduleId && moduleId !== '0') {
            sessionStorage.setItem("selectedModuleId", moduleId);
        }

        sessionStorage.setItem("restoreSelectionAfterCancel", "true");

        window.location.href = "/Trainer/ModulesLessons";
    });
});

function getInitialCourseId() {
    var courseId = $('#courseId').val();

    if (!courseId || courseId === '0') {
        courseId = sessionStorage.getItem("selectedCourseId") || '0';
        $('#courseId').val(courseId);
    }

    return courseId;
}

function getInitialModuleId() {
    var moduleId = $('#moduleId').val();

    if (!moduleId || moduleId === '' || moduleId === '0') {
        moduleId = '0';
        $('#moduleId').val(moduleId);
    }

    return moduleId;
}

function setEditMode() {
    $('.dashboard-h1').text('Edit Quiz');
    $('.dashboard-sub').text('Update the quiz details and questions.');
    $('#saveQuizBtn').html('<i class="bi bi-check-circle"></i> Update Quiz');
}

function getQuizSaveUrl(quizId, isEdit) {
    return isEdit
        ? '/api/Quiz/edit/' + quizId
        : '/api/Quiz/create';
}

function setButtonLoading(btn, isEdit) {
    btn.prop('disabled', true).text(isEdit ? 'Updating...' : 'Saving...');
}

function resetSaveButton(btn, isEdit) {
    btn.prop('disabled', false);

    btn.html(
        isEdit
            ? '<i class="bi bi-check-circle"></i> Update Quiz'
            : '<i class="bi bi-plus-circle"></i> Save Quiz'
    );
}

function showErrorMessage(xhr) {
    var err = xhr.responseJSON;
    var msg = '';

    if (err?.errors) {
        msg = Object.values(err.errors).flat().join('<br>');
    } else {
        msg = err?.errorMessage || err?.title || err?.message || xhr.responseText;
    }

    $('.error-box').html('<div>' + msg + '</div>');
}

function loadModulesForQuiz(courseId, selectedModuleId, callback) {
    $.ajax({
        url: '/api/Module/course/' + courseId,
        type: 'GET',

        success: function (modules) {
            var moduleSelect = $('#moduleSelect');

            moduleSelect.html('');
            moduleSelect.append('<option value="">Select module</option>');

            if (!modules || modules.length === 0) {
                moduleSelect.html('<option value="">No modules found</option>');
                return;
            }

            $.each(modules, function (index, module) {
                moduleSelect.append(`
                    <option value="${module.id}">
                        ${module.title}
                    </option>
                `);
            });

            if (selectedModuleId && selectedModuleId !== '0') {
                moduleSelect.val(selectedModuleId);
            }

            if (callback) {
                callback();
            }
        },

        error: function (xhr) {
            console.log(xhr.responseText);
            showErrorMessage(xhr);
        }
    });
}

function addQuestion(questionData) {
    var index = $('.trainer-question-builder-card').length;

    var questionText = questionData?.questionText || '';
    var optionA = questionData?.optionA || '';
    var optionB = questionData?.optionB || '';
    var optionC = questionData?.optionC || '';
    var optionD = questionData?.optionD || '';
    var correctAnswer = questionData?.correctAnswer || '';

    var card = `
        <div class="trainer-question-builder-card">
            <div class="trainer-question-builder-head">
                <div class="trainer-question-builder-title">Question ${index + 1}</div>

                <button type="button"
                        class="dashboard-btn dashboard-btn-outline trainer-course-action-btn remove-question-btn">
                    Remove
                </button>
            </div>

            <div class="trainer-form-grid">
                <div class="trainer-form-group trainer-form-group-full">
                    <label class="trainer-form-label">Question Text</label>
                    <textarea class="course-input trainer-textarea question-text"
                              placeholder="Enter question text">${questionText}</textarea>
                </div>

                <div class="trainer-form-group">
                    <label class="trainer-form-label">Option A</label>
                    <input type="text" class="course-input option-a" placeholder="Option A" value="${optionA}" />
                </div>

                <div class="trainer-form-group">
                    <label class="trainer-form-label">Option B</label>
                    <input type="text" class="course-input option-b" placeholder="Option B" value="${optionB}" />
                </div>

                <div class="trainer-form-group">
                    <label class="trainer-form-label">Option C</label>
                    <input type="text" class="course-input option-c" placeholder="Option C" value="${optionC}" />
                </div>

                <div class="trainer-form-group">
                    <label class="trainer-form-label">Option D</label>
                    <input type="text" class="course-input option-d" placeholder="Option D" value="${optionD}" />
                </div>

                <div class="trainer-form-group">
                    <label class="trainer-form-label">Correct Answer</label>
                    <select class="course-input correct-answer">
                        <option value="">Select correct option</option>
                        <option value="A" ${correctAnswer === 'A' ? 'selected' : ''}>Option A</option>
                        <option value="B" ${correctAnswer === 'B' ? 'selected' : ''}>Option B</option>
                        <option value="C" ${correctAnswer === 'C' ? 'selected' : ''}>Option C</option>
                        <option value="D" ${correctAnswer === 'D' ? 'selected' : ''}>Option D</option>
                    </select>
                </div>
            </div>
        </div>
    `;

    $('#questionsContainer').append(card);
    renumberQuestions();
}

function renumberQuestions() {
    $('.trainer-question-builder-card').each(function (index) {
        $(this).find('.trainer-question-builder-title').text('Question ' + (index + 1));
    });
}

function buildQuizDto() {
    return {
        moduleId: parseInt($('#moduleSelect').val()),
        title: $('#title').val(),
        instructions: $('#instructions').val(),
        attemptsAllowed: parseInt($('#attemptsAllowed').val()) || 1,
        isActive: $('#isActive').val() === 'true',
        isAccessible: $('#isAccessible').is(':checked'),
        questions: getQuestionDtos()
    };
}

function getQuestionDtos() {
    var questions = [];

    $('.trainer-question-builder-card').each(function (index) {
        questions.push({
            questionText: $(this).find('.question-text').val(),
            optionA: $(this).find('.option-a').val(),
            optionB: $(this).find('.option-b').val(),
            optionC: $(this).find('.option-c').val(),
            optionD: $(this).find('.option-d').val(),
            correctAnswer: $(this).find('.correct-answer').val(),
            displayOrder: index + 1
        });
    });

    return questions;
}

function loadQuiz(quizId) {
    $.ajax({
        url: '/api/Quiz/' + quizId,
        type: 'GET',

        success: function (quiz) {
            $('#moduleSelect').val(quiz.moduleId);
            $('#moduleId').val(quiz.moduleId);
            $('#title').val(quiz.title);
            $('#instructions').val(quiz.instructions);
            $('#attemptsAllowed').val(quiz.attemptsAllowed);
            $('#isAccessible').prop('checked', quiz.isAccessible);

            $('#questionsContainer').html('');

            if (!quiz.questions || quiz.questions.length === 0) {
                addQuestion();
                return;
            }

            $.each(quiz.questions, function (index, question) {
                addQuestion(mapQuestionForForm(question));
            });
        },

        error: function (xhr) {
            console.log(xhr.responseText);
            showErrorMessage(xhr);
        }
    });
}

function mapQuestionForForm(question) {
    var options = question.options || [];

    return {
        questionText: question.questionText,
        optionA: getOptionText(options, 0),
        optionB: getOptionText(options, 1),
        optionC: getOptionText(options, 2),
        optionD: getOptionText(options, 3),
        correctAnswer: getCorrectAnswer(options)
    };
}

function getOptionText(options, index) {
    return options.length > index ? options[index].optionText : '';
}

function getCorrectAnswer(options) {
    var letters = ['A', 'B', 'C', 'D'];
    var correctAnswer = '';

    $.each(options, function (index, option) {
        if (option.isCorrect) {
            correctAnswer = letters[index] || '';
        }
    });

    return correctAnswer;
}