'use strict';
const fs = require('fs');
const packageJSON = require('../package.json');
const upath = require('upath');
const sh = require('shelljs');

module.exports = function renderScripts() {

    const sourcePath = upath.resolve(upath.dirname(__filename), '../src/js');
    const destPath = upath.resolve(upath.dirname(__filename), '../wwwroot/.');

    sh.cp('-R', sourcePath, destPath);

    const sourcePathScriptsJS = upath.resolve(upath.dirname(__filename), '../src/js/scripts.js');
    const destPathScriptsJS = upath.resolve(upath.dirname(__filename), '../wwwroot/js/scripts.js');

    const copyright = `/*!
    * Start Bootstrap - ${packageJSON.title} v${packageJSON.version} (${packageJSON.homepage})
    * Copyright 2013-${new Date().getFullYear()} ${packageJSON.author}
    * Licensed under ${packageJSON.license} (https://github.com/BlackrockDigital/${packageJSON.name}/blob/master/LICENSE)
    */
    `;
    const scriptsJS = fs.readFileSync(sourcePathScriptsJS);

    function writeFileWithRetry(filePath, data, retries = 5, delay = 1000) {
        fs.writeFile(filePath, data, (err) => {
            if (err && err.code === 'EBUSY' && retries > 0) {
                setTimeout(() => {
                    writeFileWithRetry(filePath, data, retries - 1, delay);
                }, delay);
            } else if (err) {
                throw err;
            }
        });
    }

    writeFileWithRetry(destPathScriptsJS, copyright + scriptsJS);
};
