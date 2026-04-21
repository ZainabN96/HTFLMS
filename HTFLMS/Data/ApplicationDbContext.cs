using HTFLMS.Models;
using HTFLMS.Models.Auth;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace HTFLMS.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options) { }

        public DbSet<User> Users { get; set; }
        public DbSet<PasswordResetOtp> PasswordResetOtps { get; set; }
        public DbSet<Course> Courses => Set<Course>();

        public DbSet<Module> Modules => Set<Module>();
        public DbSet<Lesson> Lessons => Set<Lesson>();
        public DbSet<Material> Materials => Set<Material>();
        public DbSet<Assignment> Assignments => Set<Assignment>();
        public DbSet<AssignmentSubmission> AssignmentSubmissions => Set<AssignmentSubmission>();
        public DbSet<CourseEnrollment> CourseEnrollments => Set<CourseEnrollment>();
        public DbSet<LessonProgress> LessonProgresses => Set<LessonProgress>();
        public DbSet<ModuleProgress> ModuleProgresses => Set<ModuleProgress>();
        public DbSet<Quiz> Quizzes => Set<Quiz>();
        public DbSet<QuizQuestion> QuizQuestions => Set<QuizQuestion>();
        public DbSet<QuizOption> QuizOptions => Set<QuizOption>();
        public DbSet<StudentQuizAttempt> StudentQuizAttempts => Set<StudentQuizAttempt>();
        public DbSet<StudentQuizAttemptAnswer> StudentQuizAttemptAnswers => Set<StudentQuizAttemptAnswer>();
        public DbSet<Notification> Notifications => Set<Notification>();
        public DbSet<CertificateRequest> CertificateRequests => Set<CertificateRequest>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Unique Email
            modelBuilder.Entity<User>()
                .HasIndex(u => u.Email)
                .IsUnique();

            modelBuilder.Entity<User>()
                .HasIndex(u => u.UserId)
                .IsUnique();

            // Course -> Trainer (User)
            modelBuilder.Entity<Course>()
                .HasOne(c => c.Trainer)
                .WithMany(u => u.TrainerCourses)
                .HasForeignKey(c => c.TrainerId)
                .OnDelete(DeleteBehavior.Restrict);

            // Module -> Course
            modelBuilder.Entity<Module>()
                .HasOne(m => m.Course)
                .WithMany(c => c.Modules)
                .HasForeignKey(m => m.CourseId)
                .OnDelete(DeleteBehavior.Cascade);

            // Lesson -> Module
            modelBuilder.Entity<Lesson>()
                .HasOne(l => l.Module)
                .WithMany(m => m.Lessons)
                .HasForeignKey(l => l.ModuleId)
                .OnDelete(DeleteBehavior.Cascade);

            // Material relations
            modelBuilder.Entity<Material>()
                .HasOne(m => m.Course)
                .WithMany(c => c.Materials)
                .HasForeignKey(m => m.CourseId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Material>()
                .HasOne(m => m.Module)
                .WithMany(md => md.Materials)
                .HasForeignKey(m => m.ModuleId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Material>()
                .HasOne(m => m.Lesson)
                .WithMany(l => l.Materials)
                .HasForeignKey(m => m.LessonId)
                .OnDelete(DeleteBehavior.Restrict);

            // Assignment relations
            modelBuilder.Entity<Assignment>()
                .HasOne(a => a.Course)
                .WithMany(c => c.Assignments)
                .HasForeignKey(a => a.CourseId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Assignment>()
                .HasOne(a => a.Module)
                .WithMany(m => m.Assignments)
                .HasForeignKey(a => a.ModuleId)
                .OnDelete(DeleteBehavior.Restrict);

            // AssignmentSubmission relations
            modelBuilder.Entity<AssignmentSubmission>()
                .HasOne(s => s.Assignment)
                .WithMany(a => a.Submissions)
                .HasForeignKey(s => s.AssignmentId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<AssignmentSubmission>()
                .HasOne(s => s.Student)
                .WithMany(u => u.AssignmentSubmissions)
                .HasForeignKey(s => s.StudentId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<AssignmentSubmission>()
                .HasOne(s => s.GradedByUser)
                .WithMany()
                .HasForeignKey(s => s.GradedByUserId)
                .OnDelete(DeleteBehavior.Restrict);

            // Enrollment relations
            modelBuilder.Entity<CourseEnrollment>()
                .HasOne(e => e.Student)
                .WithMany(u => u.Enrollments)
                .HasForeignKey(e => e.StudentId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<CourseEnrollment>()
                .HasOne(e => e.Course)
                .WithMany(c => c.Enrollments)
                .HasForeignKey(e => e.CourseId)
                .OnDelete(DeleteBehavior.Restrict);

            // LessonProgress relations
            modelBuilder.Entity<LessonProgress>()
                .HasOne(lp => lp.Student)
                .WithMany(u => u.LessonProgresses)
                .HasForeignKey(lp => lp.StudentId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<LessonProgress>()
                .HasOne(lp => lp.Lesson)
                .WithMany(l => l.LessonProgresses)
                .HasForeignKey(lp => lp.LessonId)
                .OnDelete(DeleteBehavior.Cascade);

            // ModuleProgress relations
            modelBuilder.Entity<ModuleProgress>()
                .HasOne(mp => mp.Student)
                .WithMany(u => u.ModuleProgresses)
                .HasForeignKey(mp => mp.StudentId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<ModuleProgress>()
                .HasOne(mp => mp.Module)
                .WithMany(m => m.ModuleProgresses)
                .HasForeignKey(mp => mp.ModuleId)
                .OnDelete(DeleteBehavior.Cascade);

            // Quiz -> Module (1 to 1)
            modelBuilder.Entity<Quiz>()
                .HasOne(q => q.Module)
                .WithOne(m => m.Quiz)
                .HasForeignKey<Quiz>(q => q.ModuleId)
                .OnDelete(DeleteBehavior.Cascade);

            // QuizQuestion -> Quiz
            modelBuilder.Entity<QuizQuestion>()
                .HasOne(qq => qq.Quiz)
                .WithMany(q => q.Questions)
                .HasForeignKey(qq => qq.QuizId)
                .OnDelete(DeleteBehavior.Cascade);

            // QuizOption -> Question
            modelBuilder.Entity<QuizOption>()
                .HasOne(qo => qo.Question)
                .WithMany(qq => qq.Options)
                .HasForeignKey(qo => qo.QuestionId)
                .OnDelete(DeleteBehavior.Cascade);

            // StudentQuizAttempt relations
            modelBuilder.Entity<StudentQuizAttempt>()
                .HasOne(a => a.Quiz)
                .WithMany(q => q.Attempts)
                .HasForeignKey(a => a.QuizId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<StudentQuizAttempt>()
                .HasOne(a => a.Student)
                .WithMany(u => u.StudentQuizAttempts)
                .HasForeignKey(a => a.StudentId)
                .OnDelete(DeleteBehavior.Restrict);

            // StudentQuizAttemptAnswer relations
            modelBuilder.Entity<StudentQuizAttemptAnswer>()
                .HasOne(a => a.Attempt)
                .WithMany(at => at.Answers)
                .HasForeignKey(a => a.AttemptId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<StudentQuizAttemptAnswer>()
                .HasOne(a => a.Question)
                .WithMany(q => q.AttemptAnswers)
                .HasForeignKey(a => a.QuestionId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<StudentQuizAttemptAnswer>()
                .HasOne(a => a.SelectedOption)
                .WithMany()
                .HasForeignKey(a => a.SelectedOptionId)
                .OnDelete(DeleteBehavior.Restrict);

            // Notification -> User
            modelBuilder.Entity<Notification>()
                .HasOne(n => n.User)
                .WithMany(u => u.Notifications)
                .HasForeignKey(n => n.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            // CertificateRequest relations
            modelBuilder.Entity<CertificateRequest>()
                .HasOne(cr => cr.Student)
                .WithMany(u => u.RequestedCertificates)
                .HasForeignKey(cr => cr.StudentId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<CertificateRequest>()
                .HasOne(cr => cr.Course)
                .WithMany(c => c.CertificateRequests)
                .HasForeignKey(cr => cr.CourseId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<CertificateRequest>()
                .HasOne(cr => cr.ApprovedByUser)
                .WithMany(u => u.ApprovedCertificates)
                .HasForeignKey(cr => cr.ApprovedByUserId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}



