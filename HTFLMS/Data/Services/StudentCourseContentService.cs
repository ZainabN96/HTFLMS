using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using HTFLMS.Data.IServices;
using HTFLMS.Dtos.StudentCourseContent;
using HTFLMS.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace HTFLMS.Data.Services
{
    public class StudentCourseContentService : IStudentCourseContentService
    {
        private readonly ApplicationDbContext context;

        private const int QuizAttemptsAllowed = 3;
        private const int QuizPassingPercentage = 60;
        private const int QuizLockHours = 1;
        private const long MaxSubmissionFileSize = 50 * 1024 * 1024;

        private static readonly HashSet<string> AllowedSubmissionExtensions = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            ".pdf",
            ".doc",
            ".docx",
            ".ppt",
            ".pptx",
            ".xls",
            ".xlsx",
            ".jpg",
            ".jpeg",
            ".png",
            ".zip",
            ".rar",
            ".mp4",
            ".webm"
        };

        public StudentCourseContentService(ApplicationDbContext context)
        {
            this.context = context;
        }

        public async Task<StudentCourseContentHeaderDto?> GetHeaderAsync(int studentId, int courseId)
        {
            var course = await GetAllowedCourseAsync(studentId, courseId);

            if (course == null)
            {
                return null;
            }

            var progress = await CalculateCourseProgressAsync(studentId, courseId);

            return new StudentCourseContentHeaderDto
            {
                CourseId = course.Id,
                Title = course.Title,
                CourseImagePath = course.CourseImagePath,
                TrainerName = course.Trainer?.Name ?? "No Trainer",
                CertificateIncluded = course.CertificateIncluded,
                ProgressPercentage = progress
            };
        }

        public async Task<StudentCourseContentInfoDto?> GetInfoAsync(int studentId, int courseId)
        {
            var course = await GetAllowedCourseAsync(studentId, courseId);

            if (course == null)
            {
                return null;
            }

            var visibleModules = await GetVisibleModulesWithQuizAsync(courseId);

            var totalLessons = visibleModules
                .SelectMany(m => m.Lessons ?? new List<Lesson>())
                .Count(l => l.IsActive);

            return new StudentCourseContentInfoDto
            {
                CourseId = course.Id,
                Title = course.Title,
                Category = course.Category,
                Description = course.Description,
                CourseImagePath = course.CourseImagePath,
                TrainerName = course.Trainer?.Name ?? "No Trainer",
                CertificateIncluded = course.CertificateIncluded,
                BatchStartDate = course.BatchStartDate,
                BatchEndDate = course.BatchEndDate,
                DurationText = course.DurationText,
                TotalModules = visibleModules.Count,
                TotalLessons = totalLessons
            };
        }

        public async Task<StudentCourseContentModulesDto?> GetModulesAndLessonsAsync(int studentId, int courseId)
        {
            var course = await GetAllowedCourseAsync(studentId, courseId);

            if (course == null)
            {
                return null;
            }

            var visibleModules = await GetVisibleModulesWithQuizAsync(courseId);

            var lessonProgresses = await context.LessonProgresses
                .Where(x => x.StudentId == studentId)
                .ToListAsync();

            var quizAttempts = await context.Set<StudentQuizAttempt>()
                .Where(x => x.StudentId == studentId)
                .ToListAsync();

            var quizResets = await context.QuizAttemptsResets
                .Where(x => x.StudentId == studentId)
                .ToListAsync();

            var moduleDtos = new List<StudentCourseContentModuleDto>();

            foreach (var module in visibleModules)
            {
                var activeLessons = module.Lessons?
                    .Where(x => x.IsActive)
                    .OrderBy(x => x.DisplayOrder)
                    .ToList() ?? new List<Lesson>();

                var completedLessons = activeLessons.Count(lesson =>
                    lessonProgresses.Any(progress =>
                        progress.LessonId == lesson.Id &&
                        progress.IsCompleted));

                var totalLessons = activeLessons.Count;

                var lessonProgressPercentage = totalLessons > 0
                    ? (int)Math.Round(((decimal)completedLessons / totalLessons) * 100)
                    : 0;

                var quizDtos = new List<StudentCourseContentModuleQuizDto>();

                if (module.Quiz != null && module.Quiz.IsActive && module.Quiz.IsAccessible)
                {
                    var state = GetQuizAttemptState(module.Quiz.Id, quizAttempts, quizResets);

                    quizDtos.Add(new StudentCourseContentModuleQuizDto
                    {
                        Id = module.Quiz.Id,
                        ModuleId = module.Id,
                        Title = module.Quiz.Title,
                        Instructions = module.Quiz.Instructions,
                        AttemptsAllowed = QuizAttemptsAllowed,
                        AttemptsUsed = state.AttemptsUsed,
                        AttemptsLeft = state.AttemptsLeft,
                        QuestionsCount = module.Quiz.Questions?.Count ?? 0,
                        LastScorePercentage = state.LastScorePercentage,
                        IsPassed = state.IsPassed,
                        IsLocked = state.IsLocked,
                        LockedUntil = state.LockedUntil,
                        CanViewAttempt = state.CanViewAttempt,
                        CanRetake = state.CanRetake,
                        StatusText = state.StatusText
                    });
                }

                var isCompleted = IsModuleCompleted(module, lessonProgresses, quizAttempts);

                moduleDtos.Add(new StudentCourseContentModuleDto
                {
                    Id = module.Id,
                    Title = module.Title,
                    Description = module.Description,
                    DisplayOrder = module.DisplayOrder,
                    IsAccessible = false,
                    IsCompleted = isCompleted,
                    TotalLessons = totalLessons,
                    CompletedLessons = completedLessons,
                    ProgressPercentage = lessonProgressPercentage,
                    StatusText = GetModuleStatusText(totalLessons, completedLessons, isCompleted),
                    Lessons = activeLessons.Select(lesson => new StudentCourseContentLessonDto
                    {
                        Id = lesson.Id,
                        Title = lesson.Title,
                        Description = lesson.Description,
                        DisplayOrder = lesson.DisplayOrder,
                        IsCompleted = lessonProgresses.Any(progress =>
                            progress.LessonId == lesson.Id &&
                            progress.IsCompleted)
                    }).ToList(),
                    Quizzes = quizDtos
                });
            }

            ApplyModuleAccess(moduleDtos);

            var progress = await CalculateCourseProgressAsync(studentId, courseId);

            return new StudentCourseContentModulesDto
            {
                CourseId = courseId,
                ProgressPercentage = progress,
                Modules = moduleDtos
            };
        }

        public async Task<bool> MarkLessonDoneAsync(int studentId, int lessonId)
        {
            var lesson = await context.Lessons
                .Include(x => x.Module)
                .FirstOrDefaultAsync(x =>
                    x.Id == lessonId &&
                    x.IsActive);

            if (lesson == null || lesson.Module == null)
            {
                return false;
            }

            var isEnrolled = await context.CourseEnrollments
                .AnyAsync(x =>
                    x.StudentId == studentId &&
                    x.CourseId == lesson.Module.CourseId &&
                    x.Status == "Active");

            if (!isEnrolled)
            {
                return false;
            }

            var progress = await context.LessonProgresses
                .FirstOrDefaultAsync(x =>
                    x.StudentId == studentId &&
                    x.LessonId == lessonId);

            if (progress == null)
            {
                progress = new LessonProgress
                {
                    StudentId = studentId,
                    LessonId = lessonId,
                    IsViewed = true,
                    ViewedAt = DateTime.UtcNow,
                    IsCompleted = true,
                    CompletedAt = DateTime.UtcNow
                };

                context.LessonProgresses.Add(progress);
            }
            else
            {
                progress.IsViewed = true;
                progress.ViewedAt ??= DateTime.UtcNow;
                progress.IsCompleted = true;
                progress.CompletedAt ??= DateTime.UtcNow;

                context.LessonProgresses.Update(progress);
            }

            await UpdateModuleProgressIfCompletedAsync(studentId, lesson.ModuleId);

            return await context.SaveChangesAsync() >= 0;
        }

        public async Task<StudentCourseContentQuizDto?> GetQuizAsync(int studentId, int moduleId)
        {
            var module = await context.Modules
                .Include(x => x.Course)
                .Include(x => x.Lessons)
                .Include(x => x.Quiz!)
                .ThenInclude(q => q.Questions!)
                .ThenInclude(qn => qn.Options)
                .FirstOrDefaultAsync(x =>
                    x.Id == moduleId &&
                    x.IsActive);

            if (module == null || module.Course == null || module.Quiz == null)
            {
                return null;
            }

            var course = await GetAllowedCourseAsync(studentId, module.CourseId);

            if (course == null)
            {
                return null;
            }

            var isAccessible = await IsModuleAccessibleAsync(studentId, module.CourseId, moduleId);

            if (!isAccessible)
            {
                return null;
            }

            var quiz = module.Quiz;

            if (!quiz.IsActive || !quiz.IsAccessible)
            {
                return null;
            }

            var attempts = await GetStudentQuizAttemptsAsync(studentId, quiz.Id);
            var resets = await GetStudentQuizResetsAsync(studentId, quiz.Id);
            var state = GetQuizAttemptState(quiz.Id, attempts, resets);

            return new StudentCourseContentQuizDto
            {
                QuizId = quiz.Id,
                ModuleId = module.Id,
                Title = quiz.Title,
                Instructions = quiz.Instructions,
                AttemptsAllowed = QuizAttemptsAllowed,
                AttemptsUsed = state.AttemptsUsed,
                AttemptsLeft = state.AttemptsLeft,
                PassingPercentage = QuizPassingPercentage,
                IsPassed = state.IsPassed,
                IsLocked = state.IsLocked,
                LockedUntil = state.LockedUntil,
                Questions = quiz.Questions == null
                    ? new List<StudentCourseContentQuizQuestionDto>()
                    : quiz.Questions
                        .OrderBy(q => q.DisplayOrder)
                        .Select(q => new StudentCourseContentQuizQuestionDto
                        {
                            QuestionId = q.Id,
                            QuestionText = q.QuestionText,
                            DisplayOrder = q.DisplayOrder,
                            Options = q.Options == null
                                ? new List<StudentCourseContentQuizOptionDto>()
                                : q.Options.Select(o => new StudentCourseContentQuizOptionDto
                                {
                                    OptionId = o.Id,
                                    OptionText = o.OptionText
                                }).ToList()
                        }).ToList()
            };
        }

        public async Task<StudentCourseContentQuizResultDto?> SubmitQuizAsync(
            int studentId,
            StudentCourseContentQuizSubmitDto dto)
        {
            var module = await context.Modules
                .Include(x => x.Course)
                .Include(x => x.Lessons)
                .Include(x => x.Quiz!)
                .ThenInclude(q => q.Questions!)
                .ThenInclude(qn => qn.Options)
                .FirstOrDefaultAsync(x =>
                    x.Id == dto.ModuleId &&
                    x.IsActive);

            if (module == null || module.Course == null || module.Quiz == null)
            {
                return null;
            }

            var quiz = module.Quiz;

            if (quiz.Id != dto.QuizId || !quiz.IsActive || !quiz.IsAccessible)
            {
                return null;
            }

            var course = await GetAllowedCourseAsync(studentId, module.CourseId);

            if (course == null)
            {
                return null;
            }

            var isAccessible = await IsModuleAccessibleAsync(studentId, module.CourseId, module.Id);

            if (!isAccessible)
            {
                return null;
            }

            var alreadyPassed = await context.Set<StudentQuizAttempt>()
                .AnyAsync(x =>
                    x.StudentId == studentId &&
                    x.QuizId == quiz.Id &&
                    x.IsPassed);

            if (alreadyPassed)
            {
                return new StudentCourseContentQuizResultDto
                {
                    Success = false,
                    IsPassed = true,
                    CanRetake = false,
                    CanViewAttempt = true,
                    Message = "You have already passed this quiz."
                };
            }

            var activeReset = await GetActiveQuizResetAsync(studentId, quiz.Id);

            if (activeReset != null)
            {
                return new StudentCourseContentQuizResultDto
                {
                    Success = false,
                    IsPassed = false,
                    IsLocked = true,
                    CanRetake = false,
                    CanViewAttempt = true,
                    LockedUntil = activeReset.LockedUntil,
                    AttemptsAllowed = QuizAttemptsAllowed,
                    AttemptsUsed = QuizAttemptsAllowed,
                    AttemptsLeft = 0,
                    Message = "Quiz is locked. Please try again after the lock time ends."
                };
            }

            var attempts = await GetStudentQuizAttemptsAsync(studentId, quiz.Id);
            var resets = await GetStudentQuizResetsAsync(studentId, quiz.Id);
            var stateBeforeSubmit = GetQuizAttemptState(quiz.Id, attempts, resets);

            if (stateBeforeSubmit.AttemptsLeft <= 0)
            {
                var reset = new QuizAttemptsReset
                {
                    StudentId = studentId,
                    QuizId = quiz.Id,
                    LockedUntil = DateTime.UtcNow.AddHours(QuizLockHours),
                    CreatedAt = DateTime.UtcNow
                };

                context.QuizAttemptsResets.Add(reset);
                await context.SaveChangesAsync();

                return new StudentCourseContentQuizResultDto
                {
                    Success = false,
                    IsPassed = false,
                    IsLocked = true,
                    CanRetake = false,
                    CanViewAttempt = true,
                    LockedUntil = reset.LockedUntil,
                    AttemptsAllowed = QuizAttemptsAllowed,
                    AttemptsUsed = QuizAttemptsAllowed,
                    AttemptsLeft = 0,
                    Message = "Your quiz attempts are completed. Quiz is locked for 1 hour."
                };
            }

            var questions = quiz.Questions?
                .OrderBy(x => x.DisplayOrder)
                .ToList() ?? new List<QuizQuestion>();

            if (!questions.Any())
            {
                return null;
            }

            var submittedQuestionIds = dto.Answers
                .Select(x => x.QuestionId)
                .Distinct()
                .ToList();

            var allAnswered = questions.All(q => submittedQuestionIds.Contains(q.Id));

            if (!allAnswered)
            {
                return new StudentCourseContentQuizResultDto
                {
                    Success = false,
                    IsPassed = false,
                    CanRetake = true,
                    CanViewAttempt = false,
                    AttemptsAllowed = QuizAttemptsAllowed,
                    AttemptsUsed = stateBeforeSubmit.AttemptsUsed,
                    AttemptsLeft = stateBeforeSubmit.AttemptsLeft,
                    Message = "Please answer all questions before submitting the quiz."
                };
            }

            foreach (var question in questions)
            {
                var submittedAnswer = dto.Answers.FirstOrDefault(x => x.QuestionId == question.Id);

                if (submittedAnswer == null)
                {
                    return new StudentCourseContentQuizResultDto
                    {
                        Success = false,
                        IsPassed = false,
                        CanRetake = true,
                        CanViewAttempt = false,
                        AttemptsAllowed = QuizAttemptsAllowed,
                        AttemptsUsed = stateBeforeSubmit.AttemptsUsed,
                        AttemptsLeft = stateBeforeSubmit.AttemptsLeft,
                        Message = "Invalid quiz answers submitted."
                    };
                }

                var selectedOptionExists = question.Options != null &&
                    question.Options.Any(x => x.Id == submittedAnswer.SelectedOptionId);

                if (!selectedOptionExists)
                {
                    return new StudentCourseContentQuizResultDto
                    {
                        Success = false,
                        IsPassed = false,
                        CanRetake = true,
                        CanViewAttempt = false,
                        AttemptsAllowed = QuizAttemptsAllowed,
                        AttemptsUsed = stateBeforeSubmit.AttemptsUsed,
                        AttemptsLeft = stateBeforeSubmit.AttemptsLeft,
                        Message = "Invalid quiz option selected."
                    };
                }
            }

            var correctCount = 0;
            var now = DateTime.UtcNow;

            var attempt = new StudentQuizAttempt
            {
                StudentId = studentId,
                QuizId = quiz.Id,
                AttemptNumber = stateBeforeSubmit.AttemptsUsed + 1,
                Score = 0,
                IsPassed = false,
                StartedAt = now,
                SubmittedAt = now
            };

            context.Set<StudentQuizAttempt>().Add(attempt);
            await context.SaveChangesAsync();

            var answerEntities = new List<StudentQuizAttemptAnswer>();

            foreach (var question in questions)
            {
                var submittedAnswer = dto.Answers.First(x => x.QuestionId == question.Id);
                var selectedOption = question.Options!.First(x => x.Id == submittedAnswer.SelectedOptionId);
                var isCorrect = selectedOption.IsCorrect;

                if (isCorrect)
                {
                    correctCount++;
                }

                answerEntities.Add(new StudentQuizAttemptAnswer
                {
                    AttemptId = attempt.Id,
                    QuestionId = question.Id,
                    SelectedOptionId = submittedAnswer.SelectedOptionId,
                    IsCorrect = isCorrect
                });
            }

            var scorePercentage = (int)Math.Round(((decimal)correctCount / questions.Count) * 100);
            var isPassed = scorePercentage >= QuizPassingPercentage;

            attempt.Score = scorePercentage;
            attempt.IsPassed = isPassed;

            context.Set<StudentQuizAttempt>().Update(attempt);
            context.Set<StudentQuizAttemptAnswer>().AddRange(answerEntities);

            var attemptsUsedAfterSubmit = stateBeforeSubmit.AttemptsUsed + 1;
            var attemptsLeft = QuizAttemptsAllowed - attemptsUsedAfterSubmit;

            QuizAttemptsReset? newReset = null;

            if (!isPassed && attemptsLeft <= 0)
            {
                newReset = new QuizAttemptsReset
                {
                    StudentId = studentId,
                    QuizId = quiz.Id,
                    LockedUntil = DateTime.UtcNow.AddHours(QuizLockHours),
                    CreatedAt = DateTime.UtcNow
                };

                context.QuizAttemptsResets.Add(newReset);
            }

            if (isPassed)
            {
                await UpdateModuleProgressIfCompletedAsync(studentId, module.Id);
            }

            await context.SaveChangesAsync();

            return new StudentCourseContentQuizResultDto
            {
                Success = true,
                IsPassed = isPassed,
                IsLocked = newReset != null,
                CanRetake = !isPassed && attemptsLeft > 0,
                CanViewAttempt = true,
                ScorePercentage = scorePercentage,
                CorrectAnswers = correctCount,
                TotalQuestions = questions.Count,
                AttemptsAllowed = QuizAttemptsAllowed,
                AttemptsUsed = attemptsUsedAfterSubmit,
                AttemptsLeft = Math.Max(0, attemptsLeft),
                LockedUntil = newReset?.LockedUntil,
                Message = isPassed
                    ? $"You passed the quiz with {scorePercentage}%. Next module is now unlocked if all lessons are completed."
                    : newReset != null
                        ? $"You failed the quiz with {scorePercentage}%. Your attempts are completed. Quiz is locked for 1 hour."
                        : $"You failed the quiz with {scorePercentage}%. Attempts left: {attemptsLeft}."
            };
        }

        public async Task<StudentCourseContentQuizReviewDto?> GetQuizReviewAsync(int studentId, int quizId)
        {
            var attempt = await context.Set<StudentQuizAttempt>()
                .Include(x => x.Quiz)
                .ThenInclude(q => q!.Module)
                .Include(x => x.Quiz)
                .ThenInclude(q => q!.Questions!)
                .ThenInclude(qn => qn.Options)
                .Include(x => x.Answers)
                .Where(x =>
                    x.StudentId == studentId &&
                    x.QuizId == quizId)
                .OrderByDescending(x => x.SubmittedAt)
                .FirstOrDefaultAsync();

            if (attempt == null || attempt.Quiz == null || attempt.Quiz.Module == null)
            {
                return null;
            }

            var course = await GetAllowedCourseAsync(studentId, attempt.Quiz.Module.CourseId);

            if (course == null)
            {
                return null;
            }

            var quiz = attempt.Quiz;

            var questions = quiz.Questions?
                .OrderBy(x => x.DisplayOrder)
                .ToList() ?? new List<QuizQuestion>();

            var answers = attempt.Answers?.ToList() ?? new List<StudentQuizAttemptAnswer>();

            var revealCorrect = attempt.IsPassed;

            return new StudentCourseContentQuizReviewDto
            {
                QuizId = quiz.Id,
                QuizTitle = quiz.Title,
                IsPassed = attempt.IsPassed,
                ScorePercentage = attempt.Score,
                CorrectAnswers = answers.Count(x => x.IsCorrect),
                TotalQuestions = questions.Count,
                RevealCorrectAnswers = revealCorrect,
                SubmittedAt = attempt.SubmittedAt,
                Questions = questions.Select(question =>
                {
                    var selectedAnswer = answers.FirstOrDefault(x => x.QuestionId == question.Id);
                    var selectedOptionId = selectedAnswer?.SelectedOptionId;

                    return new StudentCourseContentQuizReviewQuestionDto
                    {
                        QuestionId = question.Id,
                        QuestionText = question.QuestionText,
                        SelectedOptionId = selectedOptionId,
                        IsSelectedAnswerCorrect = selectedAnswer?.IsCorrect ?? false,
                        Options = question.Options == null
                            ? new List<StudentCourseContentQuizReviewOptionDto>()
                            : question.Options.Select(option =>
                            {
                                var isSelected = selectedOptionId == option.Id;

                                return new StudentCourseContentQuizReviewOptionDto
                                {
                                    OptionId = option.Id,
                                    OptionText = option.OptionText,
                                    IsSelected = isSelected,
                                    IsSelectedCorrect = isSelected && (selectedAnswer?.IsCorrect ?? false),
                                    IsCorrectAnswer = revealCorrect && option.IsCorrect
                                };
                            }).ToList()
                    };
                }).ToList()
            };
        }

        public async Task<StudentCourseContentMaterialsAssignmentsDto?> GetMaterialsAndAssignmentsAsync(int studentId, int courseId)
        {
            var course = await GetAllowedCourseAsync(studentId, courseId);

            if (course == null)
            {
                return null;
            }

            var moduleAccessMap = await GetModuleAccessMapAsync(studentId, courseId);

            var activeModules = await context.Modules
                .Where(x => x.CourseId == courseId && x.IsActive)
                .Select(x => new
                {
                    x.Id,
                    x.Title
                })
                .ToListAsync();

            var activeModuleIds = activeModules.Select(x => x.Id).ToList();

            var materials = await context.Materials
                .Include(x => x.Module)
                .Where(x =>
                    x.CourseId == courseId &&
                    x.IsActive &&
                    (x.ModuleId == null || activeModuleIds.Contains(x.ModuleId.Value)))
                .OrderBy(x => x.ModuleId == null ? 0 : 1)
                .ThenBy(x => x.Module != null ? x.Module.DisplayOrder : 0)
                .ThenBy(x => x.CreatedAt)
                .ToListAsync();

            var assignments = await context.Assignments
                .Include(x => x.Module)
                .Include(x => x.Submissions)
                .Where(x =>
                    x.CourseId == courseId &&
                    x.IsActive &&
                    (x.ModuleId == null || activeModuleIds.Contains(x.ModuleId.Value)))
                .OrderBy(x => x.ModuleId == null ? 0 : 1)
                .ThenBy(x => x.Module != null ? x.Module.DisplayOrder : 0)
                .ThenBy(x => x.DueDateTime)
                .ToListAsync();

            return new StudentCourseContentMaterialsAssignmentsDto
            {
                CourseId = courseId,
                Materials = materials.Select(material =>
                {
                    var isLocked = material.ModuleId.HasValue &&
                        moduleAccessMap.ContainsKey(material.ModuleId.Value) &&
                        moduleAccessMap[material.ModuleId.Value] == false;

                    return new StudentCourseContentMaterialDto
                    {
                        Id = material.Id,
                        ModuleId = material.ModuleId,
                        ModuleTitle = material.Module?.Title ?? "Module not specified",
                        IsLocked = isLocked,
                        Title = material.Title,
                        ContentType = material.ContentType,
                        FilePath = material.FilePath,
                        ExternalUrl = material.ExternalUrl,
                        Pages = material.Pages,
                        Slides = material.Slides,
                        Minutes = material.Minutes
                    };
                }).ToList(),
                Assignments = assignments.Select(assignment =>
                {
                    var latestSubmission = assignment.Submissions?
                        .Where(x => x.StudentId == studentId)
                        .OrderByDescending(x => x.SubmittedAt)
                        .FirstOrDefault();

                    var isLocked = assignment.ModuleId.HasValue &&
                        moduleAccessMap.ContainsKey(assignment.ModuleId.Value) &&
                        moduleAccessMap[assignment.ModuleId.Value] == false;

                    var isSubmissionClosed = DateTime.Now > assignment.DueDateTime;
                    var isSubmitted = latestSubmission != null;

                    var message = "";

                    if (isLocked)
                    {
                        message = "This assignment is locked until its module is unlocked.";
                    }
                    else if (isSubmissionClosed)
                    {
                        message = "Submission closed. Due date has passed.";
                    }
                    else if (isSubmitted)
                    {
                        message = "Assignment submitted. You can unsubmit before due date.";
                    }
                    else
                    {
                        message = "Submission is open.";
                    }

                    return new StudentCourseContentAssignmentDto
                    {
                        Id = assignment.Id,
                        ModuleId = assignment.ModuleId,
                        ModuleTitle = assignment.Module?.Title ?? "Module not specified",
                        IsLocked = isLocked,
                        Title = assignment.Title,
                        Description = assignment.Description,
                        Marks = assignment.Marks,
                        DueDateTime = assignment.DueDateTime,
                        FilePath = assignment.FilePath,
                        IsSubmitted = isSubmitted,
                        IsGraded = latestSubmission?.IsGraded ?? false,
                        ObtainedMarks = latestSubmission?.ObtainedMarks,
                        Feedback = latestSubmission?.Feedback,
                        SubmissionStatus = latestSubmission == null
                            ? "Pending Submission"
                            : latestSubmission.IsGraded
                                ? "Graded"
                                : "Submitted",
                        SubmittedFilePath = latestSubmission?.SubmittedFilePath,
                        SubmittedText = latestSubmission?.SubmittedText,
                        SubmittedAt = latestSubmission?.SubmittedAt,
                        CanSubmit = !isLocked && !isSubmissionClosed && latestSubmission == null,
                        IsSubmissionClosed = isSubmissionClosed,
                        SubmissionMessage = message
                    };
                }).ToList()
            };
        }

        public async Task<StudentCourseContentAssignmentSubmitResultDto?> SubmitAssignmentAsync(
            int studentId,
            int assignmentId,
            IFormFile? file,
            string? solutionLink)
        {
            var assignment = await context.Assignments
                .Include(x => x.Course)
                .Include(x => x.Module)
                .FirstOrDefaultAsync(x =>
                    x.Id == assignmentId &&
                    x.IsActive);

            if (assignment == null || assignment.Course == null)
            {
                return null;
            }

            var course = await GetAllowedCourseAsync(studentId, assignment.CourseId);

            if (course == null)
            {
                return null;
            }

            if (assignment.ModuleId.HasValue)
            {
                var isAccessible = await IsModuleAccessibleAsync(studentId, assignment.CourseId, assignment.ModuleId.Value);

                if (!isAccessible)
                {
                    return new StudentCourseContentAssignmentSubmitResultDto
                    {
                        Success = false,
                        Message = "This assignment is locked until its module is unlocked."
                    };
                }
            }

            if (DateTime.Now > assignment.DueDateTime)
            {
                return new StudentCourseContentAssignmentSubmitResultDto
                {
                    Success = false,
                    Message = "Submission closed. Due date has passed."
                };
            }

            var existingSubmission = await context.AssignmentSubmissions
                .Where(x =>
                    x.AssignmentId == assignmentId &&
                    x.StudentId == studentId)
                .OrderByDescending(x => x.SubmittedAt)
                .FirstOrDefaultAsync();

            if (existingSubmission != null)
            {
                return new StudentCourseContentAssignmentSubmitResultDto
                {
                    Success = false,
                    Message = "You have already submitted this assignment. Please unsubmit first if you want to submit again."
                };
            }

            var hasFile = file != null && file.Length > 0;
            var hasLink = !string.IsNullOrWhiteSpace(solutionLink);

            if (!hasFile && !hasLink)
            {
                return new StudentCourseContentAssignmentSubmitResultDto
                {
                    Success = false,
                    Message = "Please upload a file or paste a solution link."
                };
            }

            if (hasFile && hasLink)
            {
                return new StudentCourseContentAssignmentSubmitResultDto
                {
                    Success = false,
                    Message = "Please submit either a file or a link, not both."
                };
            }

            string? savedFilePath = null;
            string? cleanedLink = null;

            if (hasFile && file != null)
            {
                var fileValidationMessage = ValidateSubmissionFile(file);

                if (!string.IsNullOrWhiteSpace(fileValidationMessage))
                {
                    return new StudentCourseContentAssignmentSubmitResultDto
                    {
                        Success = false,
                        Message = fileValidationMessage
                    };
                }

                savedFilePath = await SaveSubmissionFileAsync(file, assignmentId, studentId);
            }

            if (hasLink)
            {
                cleanedLink = solutionLink!.Trim();

                if (!IsValidHttpUrl(cleanedLink))
                {
                    return new StudentCourseContentAssignmentSubmitResultDto
                    {
                        Success = false,
                        Message = "Please enter a valid http or https link."
                    };
                }
            }

            var submission = new AssignmentSubmission
            {
                AssignmentId = assignmentId,
                StudentId = studentId,
                SubmittedFilePath = savedFilePath,
                SubmittedText = cleanedLink,
                SubmittedAt = DateTime.UtcNow,
                IsGraded = false
            };

            context.AssignmentSubmissions.Add(submission);
            await context.SaveChangesAsync();

            return new StudentCourseContentAssignmentSubmitResultDto
            {
                Success = true,
                Message = "Assignment solution submitted successfully."
            };
        }

        public async Task<StudentCourseContentAssignmentUnsubmitResultDto?> UnsubmitAssignmentAsync(
            int studentId,
            int assignmentId)
        {
            var assignment = await context.Assignments
                .FirstOrDefaultAsync(x =>
                    x.Id == assignmentId &&
                    x.IsActive);

            if (assignment == null)
            {
                return null;
            }

            var course = await GetAllowedCourseAsync(studentId, assignment.CourseId);

            if (course == null)
            {
                return null;
            }

            if (DateTime.Now > assignment.DueDateTime)
            {
                return new StudentCourseContentAssignmentUnsubmitResultDto
                {
                    Success = false,
                    Message = "You cannot unsubmit this assignment because the due date has passed."
                };
            }

            var submission = await context.AssignmentSubmissions
                .Where(x =>
                    x.AssignmentId == assignmentId &&
                    x.StudentId == studentId)
                .OrderByDescending(x => x.SubmittedAt)
                .FirstOrDefaultAsync();

            if (submission == null)
            {
                return new StudentCourseContentAssignmentUnsubmitResultDto
                {
                    Success = false,
                    Message = "No submitted solution was found for this assignment."
                };
            }

            if (submission.IsGraded)
            {
                return new StudentCourseContentAssignmentUnsubmitResultDto
                {
                    Success = false,
                    Message = "This assignment is already graded, so it cannot be unsubmitted."
                };
            }

            DeleteSubmittedFileIfExists(submission.SubmittedFilePath);

            context.AssignmentSubmissions.Remove(submission);
            await context.SaveChangesAsync();

            return new StudentCourseContentAssignmentUnsubmitResultDto
            {
                Success = true,
                Message = "Assignment solution removed successfully. You can submit again before the due date."
            };
        }

        public async Task<StudentCourseContentNotesDto?> GetNotesAsync(int studentId, int courseId)
        {
            var course = await GetAllowedCourseAsync(studentId, courseId);

            if (course == null)
            {
                return null;
            }

            var notes = await context.StudentCourseNotes
                .Where(x =>
                    x.StudentId == studentId &&
                    x.CourseId == courseId)
                .OrderByDescending(x => x.UpdatedAt ?? x.CreatedAt)
                .ToListAsync();

            var latestDate = notes
                .Select(x => x.UpdatedAt ?? x.CreatedAt)
                .OrderByDescending(x => x)
                .FirstOrDefault();

            return new StudentCourseContentNotesDto
            {
                CourseId = courseId,
                TotalNotes = notes.Count,
                RecentActivityText = notes.Count == 0 ? "No activity yet" : GetSimpleActivityText(latestDate),
                LastUpdatedText = notes.Count == 0 ? "No notes yet" : GetSimpleActivityText(latestDate),
                Notes = notes.Select(MapNoteDto).ToList()
            };
        }

        public async Task<StudentCourseContentNoteActionResultDto?> CreateNoteAsync(
            int studentId,
            int courseId,
            StudentCourseContentNoteSaveDto dto)
        {
            var course = await GetAllowedCourseAsync(studentId, courseId);

            if (course == null)
            {
                return null;
            }

            var validationMessage = ValidateNoteDto(dto);

            if (!string.IsNullOrWhiteSpace(validationMessage))
            {
                return new StudentCourseContentNoteActionResultDto
                {
                    Success = false,
                    Message = validationMessage
                };
            }

            var note = new StudentCourseNote
            {
                StudentId = studentId,
                CourseId = courseId,
                Title = dto.Title.Trim(),
                Description = dto.Description.Trim(),
                CreatedAt = DateTime.UtcNow
            };

            context.StudentCourseNotes.Add(note);
            await context.SaveChangesAsync();

            return new StudentCourseContentNoteActionResultDto
            {
                Success = true,
                Message = "Note saved successfully.",
                Note = MapNoteDto(note)
            };
        }

        public async Task<StudentCourseContentNoteActionResultDto?> UpdateNoteAsync(
            int studentId,
            int noteId,
            StudentCourseContentNoteSaveDto dto)
        {
            var note = await context.StudentCourseNotes
                .FirstOrDefaultAsync(x =>
                    x.Id == noteId &&
                    x.StudentId == studentId);

            if (note == null)
            {
                return null;
            }

            var course = await GetAllowedCourseAsync(studentId, note.CourseId);

            if (course == null)
            {
                return null;
            }

            var validationMessage = ValidateNoteDto(dto);

            if (!string.IsNullOrWhiteSpace(validationMessage))
            {
                return new StudentCourseContentNoteActionResultDto
                {
                    Success = false,
                    Message = validationMessage
                };
            }

            note.Title = dto.Title.Trim();
            note.Description = dto.Description.Trim();
            note.UpdatedAt = DateTime.UtcNow;

            context.StudentCourseNotes.Update(note);
            await context.SaveChangesAsync();

            return new StudentCourseContentNoteActionResultDto
            {
                Success = true,
                Message = "Note updated successfully.",
                Note = MapNoteDto(note)
            };
        }

        public async Task<StudentCourseContentNoteActionResultDto?> DeleteNoteAsync(
            int studentId,
            int noteId)
        {
            var note = await context.StudentCourseNotes
                .FirstOrDefaultAsync(x =>
                    x.Id == noteId &&
                    x.StudentId == studentId);

            if (note == null)
            {
                return null;
            }

            var course = await GetAllowedCourseAsync(studentId, note.CourseId);

            if (course == null)
            {
                return null;
            }

            context.StudentCourseNotes.Remove(note);
            await context.SaveChangesAsync();

            return new StudentCourseContentNoteActionResultDto
            {
                Success = true,
                Message = "Note deleted successfully."
            };
        }

        private async Task<Course?> GetAllowedCourseAsync(int studentId, int courseId)
        {
            var isEnrolled = await context.CourseEnrollments
                .AnyAsync(x =>
                    x.StudentId == studentId &&
                    x.CourseId == courseId &&
                    x.Status == "Active");

            if (!isEnrolled)
            {
                return null;
            }

            return await context.Courses
                .Include(x => x.Trainer)
                .FirstOrDefaultAsync(x =>
                    x.Id == courseId &&
                    x.IsActive == true &&
                    x.IsPublished == true);
        }

        private async Task<List<Module>> GetVisibleModulesWithQuizAsync(int courseId)
        {
            var modules = await context.Modules
                .Include(x => x.Lessons)
                .Include(x => x.Quiz!)
                .ThenInclude(q => q.Questions)
                .Where(x =>
                    x.CourseId == courseId &&
                    x.IsActive == true)
                .OrderBy(x => x.DisplayOrder)
                .ToListAsync();

            return modules
                .Where(x => x.Lessons != null && x.Lessons.Any(l => l.IsActive))
                .ToList();
        }

        private async Task<Dictionary<int, bool>> GetModuleAccessMapAsync(int studentId, int courseId)
        {
            var modules = await GetVisibleModulesWithQuizAsync(courseId);

            var lessonProgresses = await context.LessonProgresses
                .Where(x => x.StudentId == studentId)
                .ToListAsync();

            var quizAttempts = await context.Set<StudentQuizAttempt>()
                .Where(x => x.StudentId == studentId)
                .ToListAsync();

            var moduleDtos = modules.Select(module => new StudentCourseContentModuleDto
            {
                Id = module.Id,
                IsCompleted = IsModuleCompleted(module, lessonProgresses, quizAttempts)
            }).ToList();

            ApplyModuleAccess(moduleDtos);

            return moduleDtos.ToDictionary(x => x.Id, x => x.IsAccessible);
        }

        private async Task<int> CalculateCourseProgressAsync(int studentId, int courseId)
        {
            var visibleModules = await GetVisibleModulesWithQuizAsync(courseId);

            if (!visibleModules.Any())
            {
                return 0;
            }

            var lessonProgresses = await context.LessonProgresses
                .Where(x => x.StudentId == studentId)
                .ToListAsync();

            var quizAttempts = await context.Set<StudentQuizAttempt>()
                .Where(x => x.StudentId == studentId)
                .ToListAsync();

            var completedModules = visibleModules.Count(module =>
                IsModuleCompleted(module, lessonProgresses, quizAttempts));

            return (int)Math.Round(((decimal)completedModules / visibleModules.Count) * 100);
        }

        private static bool IsModuleCompleted(
            Module module,
            List<LessonProgress> lessonProgresses,
            List<StudentQuizAttempt> quizAttempts)
        {
            var activeLessons = module.Lessons?
                .Where(x => x.IsActive)
                .ToList() ?? new List<Lesson>();

            if (!activeLessons.Any())
            {
                return false;
            }

            var allLessonsCompleted = activeLessons.All(lesson =>
                lessonProgresses.Any(progress =>
                    progress.LessonId == lesson.Id &&
                    progress.IsCompleted));

            if (!allLessonsCompleted)
            {
                return false;
            }

            if (module.Quiz == null || !module.Quiz.IsActive || !module.Quiz.IsAccessible)
            {
                return true;
            }

            return quizAttempts.Any(attempt =>
                attempt.QuizId == module.Quiz.Id &&
                attempt.IsPassed);
        }

        private static void ApplyModuleAccess(List<StudentCourseContentModuleDto> modules)
        {
            if (!modules.Any())
            {
                return;
            }

            modules[0].IsAccessible = true;

            for (var i = 1; i < modules.Count; i++)
            {
                modules[i].IsAccessible = modules[i - 1].IsCompleted;
            }
        }

        private static string GetModuleStatusText(int totalLessons, int completedLessons, bool isCompleted)
        {
            if (isCompleted)
            {
                return "Completed";
            }

            if (completedLessons <= 0)
            {
                return "Not Started";
            }

            if (completedLessons < totalLessons)
            {
                return "In Progress";
            }

            return "Quiz Pending";
        }

        private async Task UpdateModuleProgressIfCompletedAsync(int studentId, int moduleId)
        {
            var module = await context.Modules
                .Include(x => x.Lessons)
                .Include(x => x.Quiz)
                .FirstOrDefaultAsync(x => x.Id == moduleId);

            if (module == null)
            {
                return;
            }

            var lessonProgresses = await context.LessonProgresses
                .Where(x => x.StudentId == studentId)
                .ToListAsync();

            var quizAttempts = await context.Set<StudentQuizAttempt>()
                .Where(x => x.StudentId == studentId)
                .ToListAsync();

            var isCompleted = IsModuleCompleted(module, lessonProgresses, quizAttempts);

            if (!isCompleted)
            {
                return;
            }

            var moduleProgress = await context.ModuleProgresses
                .FirstOrDefaultAsync(x =>
                    x.StudentId == studentId &&
                    x.ModuleId == moduleId);

            if (moduleProgress == null)
            {
                moduleProgress = new ModuleProgress
                {
                    StudentId = studentId,
                    ModuleId = moduleId,
                    IsCompleted = true,
                    CompletedAt = DateTime.UtcNow
                };

                context.ModuleProgresses.Add(moduleProgress);
            }
            else
            {
                moduleProgress.IsCompleted = true;
                moduleProgress.CompletedAt ??= DateTime.UtcNow;

                context.ModuleProgresses.Update(moduleProgress);
            }
        }

        private async Task<bool> IsModuleAccessibleAsync(int studentId, int courseId, int moduleId)
        {
            var moduleAccessMap = await GetModuleAccessMapAsync(studentId, courseId);

            return moduleAccessMap.ContainsKey(moduleId) && moduleAccessMap[moduleId];
        }

        private async Task<List<StudentQuizAttempt>> GetStudentQuizAttemptsAsync(int studentId, int quizId)
        {
            return await context.Set<StudentQuizAttempt>()
                .Where(x =>
                    x.StudentId == studentId &&
                    x.QuizId == quizId)
                .OrderBy(x => x.SubmittedAt)
                .ToListAsync();
        }

        private async Task<List<QuizAttemptsReset>> GetStudentQuizResetsAsync(int studentId, int quizId)
        {
            return await context.QuizAttemptsResets
                .Where(x =>
                    x.StudentId == studentId &&
                    x.QuizId == quizId)
                .OrderBy(x => x.LockedUntil)
                .ToListAsync();
        }

        private async Task<QuizAttemptsReset?> GetActiveQuizResetAsync(int studentId, int quizId)
        {
            return await context.QuizAttemptsResets
                .Where(x =>
                    x.StudentId == studentId &&
                    x.QuizId == quizId &&
                    x.LockedUntil > DateTime.UtcNow)
                .OrderByDescending(x => x.LockedUntil)
                .FirstOrDefaultAsync();
        }

        private static QuizAttemptStateDto GetQuizAttemptState(
            int quizId,
            List<StudentQuizAttempt> attempts,
            List<QuizAttemptsReset> resets)
        {
            var quizAttempts = attempts
                .Where(x => x.QuizId == quizId)
                .OrderBy(x => x.SubmittedAt)
                .ToList();

            var quizResets = resets
                .Where(x => x.QuizId == quizId)
                .OrderBy(x => x.LockedUntil)
                .ToList();

            var activeReset = quizResets
                .Where(x => x.LockedUntil > DateTime.UtcNow)
                .OrderByDescending(x => x.LockedUntil)
                .FirstOrDefault();

            var isPassed = quizAttempts.Any(x => x.IsPassed);

            var latestAttempt = quizAttempts
                .OrderByDescending(x => x.SubmittedAt)
                .FirstOrDefault();

            var latestExpiredReset = quizResets
                .Where(x => x.LockedUntil <= DateTime.UtcNow)
                .OrderByDescending(x => x.LockedUntil)
                .FirstOrDefault();

            var cycleStart = latestExpiredReset?.LockedUntil ?? DateTime.MinValue;

            var attemptsUsed = quizAttempts.Count(x =>
                x.SubmittedAt >= cycleStart &&
                !x.IsPassed);

            var attemptsLeft = isPassed
                ? 0
                : Math.Max(0, QuizAttemptsAllowed - attemptsUsed);

            var isLocked = activeReset != null;

            return new QuizAttemptStateDto
            {
                IsPassed = isPassed,
                IsLocked = isLocked,
                LockedUntil = activeReset?.LockedUntil,
                AttemptsUsed = isPassed ? latestAttempt?.AttemptNumber ?? attemptsUsed : attemptsUsed,
                AttemptsLeft = isPassed || isLocked ? 0 : attemptsLeft,
                LastScorePercentage = latestAttempt?.Score,
                CanViewAttempt = latestAttempt != null,
                CanRetake = !isPassed && !isLocked && latestAttempt != null && attemptsLeft > 0,
                StatusText = isPassed
                    ? "Completed"
                    : isLocked
                        ? "Quiz Locked"
                        : latestAttempt == null
                            ? "Start Quiz"
                            : attemptsLeft > 0
                                ? "Retake Quiz"
                                : "Quiz Locked"
            };
        }

        private static string ValidateSubmissionFile(IFormFile file)
        {
            if (file.Length <= 0)
            {
                return "Please upload a valid file.";
            }

            if (file.Length > MaxSubmissionFileSize)
            {
                return "File size limit exceeded. Please upload your file/video to Google Drive, OneDrive, or any cloud storage and paste the link here.";
            }

            var extension = Path.GetExtension(file.FileName);

            if (string.IsNullOrWhiteSpace(extension))
            {
                return "File type is not valid.";
            }

            if (!AllowedSubmissionExtensions.Contains(extension))
            {
                return "This file type is not allowed. Please upload PDF, Word, PPT, Excel, image, ZIP/RAR, MP4, WEBM, or paste a valid link.";
            }

            return "";
        }

        private static async Task<string> SaveSubmissionFileAsync(IFormFile file, int assignmentId, int studentId)
        {
            var webRootPath = GetWebRootPath();

            var folderPath = Path.Combine(
                webRootPath,
                "uploads",
                "assignments",
                "submissions");

            if (!Directory.Exists(folderPath))
            {
                Directory.CreateDirectory(folderPath);
            }

            var extension = Path.GetExtension(file.FileName);
            var fileName = $"assignment_{assignmentId}_student_{studentId}_{Guid.NewGuid():N}{extension}";
            var fullPath = Path.Combine(folderPath, fileName);

            await using (var stream = new FileStream(fullPath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            return $"/uploads/assignments/submissions/{fileName}";
        }

        private static void DeleteSubmittedFileIfExists(string? submittedFilePath)
        {
            if (string.IsNullOrWhiteSpace(submittedFilePath))
            {
                return;
            }

            var cleanPath = submittedFilePath.TrimStart('/').Replace('/', Path.DirectorySeparatorChar);

            var allowedFolder = Path.Combine(
                "uploads",
                "assignments",
                "submissions");

            if (!cleanPath.StartsWith(allowedFolder, StringComparison.OrdinalIgnoreCase))
            {
                return;
            }

            var fullPath = Path.Combine(GetWebRootPath(), cleanPath);

            if (File.Exists(fullPath))
            {
                File.Delete(fullPath);
            }
        }

        private static string GetWebRootPath()
        {
            return Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
        }

        private static bool IsValidHttpUrl(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return false;
            }

            return Uri.TryCreate(value, UriKind.Absolute, out var uriResult) &&
                (uriResult.Scheme == Uri.UriSchemeHttp ||
                 uriResult.Scheme == Uri.UriSchemeHttps);
        }

        private static StudentCourseContentNoteDto MapNoteDto(StudentCourseNote note)
        {
            return new StudentCourseContentNoteDto
            {
                Id = note.Id,
                CourseId = note.CourseId,
                Title = note.Title,
                Description = note.Description,
                CreatedAt = note.CreatedAt,
                UpdatedAt = note.UpdatedAt
            };
        }

        private static string ValidateNoteDto(StudentCourseContentNoteSaveDto dto)
        {
            if (dto == null)
            {
                return "Note data is required.";
            }

            if (string.IsNullOrWhiteSpace(dto.Title))
            {
                return "Please enter note title.";
            }

            if (dto.Title.Trim().Length > 200)
            {
                return "Note title cannot be more than 200 characters.";
            }

            if (string.IsNullOrWhiteSpace(dto.Description))
            {
                return "Please write your note.";
            }

            return "";
        }

        private static string GetSimpleActivityText(DateTime date)
        {
            var localDate = date.ToLocalTime().Date;
            var today = DateTime.Now.Date;

            if (localDate == today)
            {
                return "Today";
            }

            if (localDate == today.AddDays(-1))
            {
                return "Yesterday";
            }

            return date.ToLocalTime().ToString("dd MMM, yyyy");
        }

        private class QuizAttemptStateDto
        {
            public bool IsPassed { get; set; }

            public bool IsLocked { get; set; }

            public DateTime? LockedUntil { get; set; }

            public int AttemptsUsed { get; set; }

            public int AttemptsLeft { get; set; }

            public int? LastScorePercentage { get; set; }

            public bool CanViewAttempt { get; set; }

            public bool CanRetake { get; set; }

            public string StatusText { get; set; } = "Start Quiz";
        }
    }
}