var gulp = require('gulp');
var less = require('gulp-less');
var jshint = require('gulp-jshint');
var cssmin = require('gulp-cssmin');
var rename = require('gulp-rename');
var es = require('event-stream');

gulp.task('styles', function () {
    return gulp.src(['Content/*.less'])
      .pipe(less())
      .pipe(cssmin())
      .pipe(rename({ suffix: '.min' }))
      .pipe(gulp.dest('./Content/'));
});

gulp.task('scripts', function () {
    return es.merge(
        gulp.src(['lib/*']).pipe(gulp.dest('./build')),
        gulp.src(['Scripts/calendar-proxy-form.js'])
          .pipe(rename({ suffix: '.min' }))
          .pipe(gulp.dest('./Scripts/'))
      );
});


gulp.task('validate', function () {
    return gulp.src(['Scripts/calendar-proxy-form.js'])
      .pipe(jshint())
      .pipe(jshint.reporter('jshint-stylish', { 'verbose': true }))
      .pipe(jshint.reporter('fail'));
});

gulp.task('watch', function () {
    // library assets
    gulp.watch(['Scripts/*.js'], ['validate', 'scripts']); 
    gulp.watch(['Content/*.less'], ['styles']);


});
