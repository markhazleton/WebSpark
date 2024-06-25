'use strict';
const fs = require('fs');
const upath = require('upath');
const sh = require('shelljs');
const glob = require("glob");
const path = require("path");


module.exports = function renderAssets() {
    const sourcePath = upath.resolve(upath.dirname(__filename), '../src/assets');
    const destPath = upath.resolve(upath.dirname(__filename), '../wwwroot/.');

    // Copy all files from src/assets to wwwroot
    const assetFiles = glob.sync('**/*', { cwd: sourcePath, nodir: true });
    assetFiles.forEach((file) => {
        const sourceFile = path.join(sourcePath, file);
        const destFile = path.join(destPath, file);
        sh.mkdir('-p', path.dirname(destFile));
        sh.cp(sourceFile, destFile);
    });

    const srcPath = upath.resolve(
        upath.dirname(__filename),
        "../src"
    );

};
