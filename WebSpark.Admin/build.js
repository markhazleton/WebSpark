const path = require('path');
const merge = require('merge-stream');
const exec = require('child_process').execSync;
const fs = require('fs');
const { src, dest, parallel, series, watch } = require('gulp');
const webpack = require('webpack');
const sass = require('gulp-dart-sass');
const localSass = require('sass');
const gulpIf = require('gulp-if');
const sourcemaps = require('gulp-sourcemaps');
const colors = require('ansi-colors');
const log = require('fancy-log');
const browserSync = require('browser-sync').create();
colors.enabled = require('color-support').hasBasic;

// Dynamic import functions
async function loadDel() {
  const del = await import('del');
  return del;
}

async function loadAutoprefixer() {
  const autoprefixer = await import('gulp-autoprefixer');
  return autoprefixer;
}

async function loadRename() {
  const rename = await import('gulp-rename');
  return rename;
}

async function loadUglify() {
  const uglify = await import('gulp-uglify');
  return uglify;
}

// Config
// -------------------------------------------------------------------------------

const env = process.env.NODE_ENV || 'development';
const conf = (() => {
  const _conf = require('./build-config');
  return require('deepmerge').all([{}, _conf.base || {}, _conf[env] || {}]);
})();

conf.distPath = path.resolve(__dirname, conf.distPath).replace(/\\/g, '/');

// Utilities
// -------------------------------------------------------------------------------

function normalize(p) {
  return p.replace(/\\/g, '/');
}

function root(p) {
  return p.startsWith('!')
    ? normalize(`!${path.join(__dirname, 'src', p.slice(1))}`)
    : normalize(path.join(__dirname, 'src', p));
}

function srcGlob(...src) {
  return src.map(p => root(p)).concat(conf.exclude.map(d => `!${root(d)}/**/*`));
}

// Tasks
// -------------------------------------------------------------------------------
// Build CSS
// -------------------------------------------------------------------------------
const buildCssTask = async function (cb) {
  const autoprefixer = await loadAutoprefixer();
  const rename = await loadRename();

  return src(srcGlob('**/*.scss', '!**/_*.scss'))
    .pipe(gulpIf(conf.sourcemaps, sourcemaps.init()))
    .pipe(
      gulpIf(
        localSass,
        exec(
          gulpIf(
            conf.minify,
            `sass src/site.scss:${conf.distPath}/css/site.css src/scss:${conf.distPath}/vendor/css src/fonts:${conf.distPath}/vendor/fonts src/libs:${conf.distPath}/vendor/libs --style compressed --no-source-map`,
            gulpIf(
              conf.fastDev,
              `sass  src/site.scss:${conf.distPath}/css/site.css src/scss:${conf.distPath}/vendor/css src/scss/pages:${conf.distPath}/vendor/css/pages src/fonts:${conf.distPath}/vendor/fonts src/libs:${conf.distPath}/vendor/libs --no-source-map`
            )
          ),
          function (err) {
            cb(err);
          }
        ),
        sass({
          outputStyle: conf.minify ? 'compressed' : 'expanded'
        }).on('error', sass.logError)
      )
    )
    .pipe(gulpIf(conf.sourcemaps, sourcemaps.write()))
    .pipe(autoprefixer.default())
    .pipe(rename.default({ extname: '.css' }))
    .pipe(
      rename.default(function (path) {
        path.dirname = path.dirname.replace('scss', 'css');
      })
    )
    .pipe(dest(conf.distPath));
};

// Build JS
// -------------------------------------------------------------------------------
const webpackJsTask = function (cb) {
  setTimeout(function () {
    webpack(require('./webpack.config'), (err, stats) => {
      if (err) {
        log(colors.gray('Webpack error:'), colors.red(err.stack || err));
        if (err.details) log(colors.gray('Webpack error details:'), err.details);
        return cb();
      }

      const info = stats.toJson();

      if (stats.hasErrors()) {
        info.errors.forEach(e => log(colors.gray('Webpack compilation error:'), colors.red(e)));
      }

      if (stats.hasWarnings()) {
        info.warnings.forEach(w => log(colors.gray('Webpack compilation warning:'), colors.yellow(w)));
      }

      log(
        stats.toString({
          colors: colors.enabled,
          hash: false,
          timings: false,
          chunks: false,
          chunkModules: false,
          modules: false,
          children: true,
          version: true,
          cached: false,
          cachedAssets: false,
          reasons: false,
          source: false,
          errorDetails: false
        })
      );

      cb();
      browserSync.reload();
    });
  }, 1);
};
const pageJsTask = async function () {
  const uglify = await loadUglify();

  return src(conf.distPath + `/js/**/*.js`)
    .pipe(gulpIf(conf.minify, uglify.default()))
    .pipe(dest(conf.distPath + `/js`));
};

// Build fonts
// -------------------------------------------------------------------------------

const FONT_TASKS = [
  {
    name: 'boxicons',
    path: 'node_modules/boxicons/fonts/*'
  }
].reduce(function (tasks, font) {
  const functionName = `buildFonts${font.name.replace(/^./, m => m.toUpperCase())}Task`;
  const taskFunction = function () {
    return src(font.path).pipe(dest(path.join(conf.distPath + `/vendor/`, 'fonts', font.name)));
  };

  Object.defineProperty(taskFunction, 'name', {
    value: functionName
  });

  return tasks.concat([taskFunction]);
}, []);

// Formula module requires KaTeX - Quill Editor
const KATEX_FONT_TASK = [
  {
    name: 'katex',
    path: 'node_modules/katex/dist/fonts/*'
  }
].reduce(function (tasks, font) {
  const functionName = `buildFonts${font.name.replace(/^./, m => m.toUpperCase())}Task`;
  const taskFunction = function () {
    return src(font.path).pipe(dest(path.join(conf.distPath, 'vendor/libs/quill/fonts')));
  };

  Object.defineProperty(taskFunction, 'name', {
    value: functionName
  });

  return tasks.concat([taskFunction]);
}, []);

const buildFontsTask = parallel(FONT_TASKS, KATEX_FONT_TASK);

// Clean build directory
// -------------------------------------------------------------------------------

const cleanTask = async function () {
  const del = await loadDel();
  return del.deleteSync([`${conf.distPath}/vendor/**/*`], {
    force: true
  });
};

const cleanSourcemapsTask = async function () {
  const del = await loadDel();
  return del.deleteSync([`${conf.distPath}/**/*.map`], {
    force: true
  });
};

const cleanAllTask = parallel(cleanTask, cleanSourcemapsTask);

// Watch
// -------------------------------------------------------------------------------
const watchTask = function () {
  watch(srcGlob('**/*.scss'), buildCssTask);
  watch(srcGlob('**/*.js', '!js/**/*.js'), webpackJsTask);
  watch(srcGlob('/js/**/*.js'), pageJsTask);
};

// Build (Dev & Prod)
// -------------------------------------------------------------------------------
const buildJsTask = series(webpackJsTask, pageJsTask);

const buildTasks = [buildCssTask, buildJsTask, buildFontsTask];
const buildTask = conf.cleanDist
  ? series(cleanAllTask, parallel(buildTasks))
  : series(cleanAllTask, cleanSourcemapsTask, parallel(buildTasks));

// Exports
// -------------------------------------------------------------------------------
exports.default = buildTask;
exports.clean = cleanAllTask;
exports['build:js'] = buildJsTask;
exports['build:css'] = buildCssTask;
exports['build:fonts'] = buildFontsTask;
exports.build = buildTask;
exports.watch = watchTask;
