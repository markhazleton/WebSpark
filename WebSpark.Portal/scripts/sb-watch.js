'use strict';

const _ = require('lodash');
const chokidar = require('chokidar');
const dotenv = require('dotenv');
const renderAssets = require('./render-assets');
const renderScripts = require('./render-scripts');
const renderSCSS = require('./render-scss');
const upath = require('upath');
const workerpool = require('workerpool');

// Initialize dotenv and parse environment variables
const parsedENV = dotenv.config().parsed || {};

// Define the limitConcurrency function here (as provided in previous responses)

// Instantiate the concurrency limiter with the desired maximum concurrency
const limit = limitConcurrency(parseInt(parsedENV.PARALLEL_PUG_RENDERS || '2', 10));

// Simple concurrency limit implementation
function limitConcurrency(maxConcurrent) {
    let active = 0;
    const queue = [];

    function run(task) {
        return new Promise((resolve, reject) => {
            queue.push(() => task().then(resolve).catch(reject));
            processQueue();
        });
    }

    function processQueue() {
        if (queue.length === 0 || active >= maxConcurrent) {
            return;
        }
        active++;
        const task = queue.shift();
        task().finally(() => {
            active--;
            processQueue();
        });
    }

    return run;
}

const pool = workerpool.pool(__dirname + '/render-pug.js', {
    minWorkers: 'max',
});


const watcher = chokidar.watch('src', {
    persistent: true,
});

let READY = false;

process.title = 'pug-watch';
process.stdout.write('Loading');
let allPugFiles = {};

watcher.on('add', filePath => _processFile(upath.normalize(filePath), 'add'));
watcher.on('change', filePath => _processFile(upath.normalize(filePath), 'change'));
watcher.on('ready', () => {
    READY = true;
    console.log(' READY TO ROLL!');
});

_handleSCSS();

function _processFile(filePath, watchEvent) {
    if (!READY) {
        if (filePath.match(/\.pug$/)) {
            if (!filePath.match(/includes/) && !filePath.match(/mixins/) && !filePath.match(/\/pug\/layouts\//)) {
                allPugFiles[filePath] = true;
            }
        }
        process.stdout.write('.');
        return;
    }

    console.log(`### INFO: File event: ${watchEvent}: ${filePath}`);

    if (filePath.match(/\.pug$/)) {
        return _handlePug(filePath, watchEvent);
    }

    if (filePath.match(/\.scss$/)) {
        if (watchEvent === 'change') {
            return _handleSCSS(filePath, watchEvent);
        }
        return;
    }

    if (filePath.match(/src\/js\//)) {
        return renderScripts();
    }

    if (filePath.match(/src\/assets\//)) {
        return renderAssets();
    }
}

function _handlePug(filePath, watchEvent) {
    if (watchEvent === 'change') {
        if (filePath.match(/includes/) || filePath.match(/mixins/) || filePath.match(/\/pug\/layouts\//)) {
            return _renderAllPug();
        }
        return _renderPug(filePath);
    }
    if (!filePath.match(/includes/) && !filePath.match(/mixins/) && !filePath.match(/\/pug\/layouts\//)) {
        return _renderPug(filePath);
    }
}

function _renderAllPug() {
    console.log('### INFO: Rendering All');
    const promiseArray = [];
    _.each(allPugFiles, (value, filePath) => {
        // Use the limit function for each task you want to limit concurrently
        promiseArray.push(limit(() => _renderPug(filePath)));
    });
    Promise.all(promiseArray).then(() => {
        console.log('All tasks completed');
    }).catch((error) => {
        console.error('An error occurred:', error);
    });
}

function _handleSCSS() {
    renderSCSS();
}

function _renderPug(filePath) {
    return new Promise(function (resolve, reject) {
        pool.exec('renderPug', [filePath])
            .then(function (result) {
                resolve(result);
            })
            .catch(function (err) {
                console.error(err);
                reject(err);
            });
    });
}
