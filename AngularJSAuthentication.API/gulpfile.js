/*
This file is the main entry point for defining Gulp tasks and using Gulp plugins.
Click here to learn more. https://go.microsoft.com/fwlink/?LinkId=518007
*/

var gulp = require('gulp');

const jsValidate = require('gulp-jsvalidate');

var uglify = require('gulp-uglify');
const minify = require('gulp-minify');
var bundle = require('gulp-bundle-assets');
const jshint = require('gulp-jshint');
// folders

var folders = {

    root: "/"

};

var config = {
    "globals": {
        /* MOCHA */
        "console": false,
        "moment": false,
        "angular": false,
        "alert": false,
        "app": false,
        "$": false,
        "_": false,
        "window": false,
        "$upload": false,
        "alasql": false
    },
    asi: true
}



gulp.task('js', function () {

    return gulp.src('./controller/CashManagement/*')
        .pipe(jshint(config))
        .pipe(jshint.reporter('default'))
        //.pipe(bundle())
        //.pipe(gulp.dest('./abc/'));
        //.pipe(jsValidate());
        //.pipe(uglify());

        //.pipe(gulp.dest('./abc/'))

});



gulp.task(['js']);