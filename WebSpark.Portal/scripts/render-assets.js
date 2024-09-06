'use strict';
const fs = require('fs');
const upath = require('upath');
const sh = require('shelljs');
const glob = require('glob');
const path = require('path');

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

    // Define paths for Bootstrap Icons fonts, Feather Icons SVGs, and Font Awesome fonts
    const bootstrapIconsFontPath = upath.resolve(upath.dirname(__filename), '../node_modules/bootstrap-icons/font/fonts');
    const featherIconsSvgPath = upath.resolve(upath.dirname(__filename), '../node_modules/feather-icons/dist/icons'); // Path to Feather SVGs
    const fontAwesomeFontPath = upath.resolve(upath.dirname(__filename), '../node_modules/fontawesome-free/webfonts'); // Path to Font Awesome fonts

    // Updated destination path for Bootstrap Icons fonts, Feather Icons SVGs, and Font Awesome fonts
    const fontsDestPath = upath.resolve(destPath, 'dist/css/fonts'); // Directly in fonts without sub-folder
    const featherIconsDestPath = upath.resolve(destPath, 'dist/css/icons/feather-icons'); // Destination for Feather Icons SVGs
    const fontAwesomeDestPath = upath.resolve(destPath, 'dist/webfonts'); // Destination for Font Awesome fonts

    // Copy Bootstrap Icons fonts directly into the fonts directory
    if (fs.existsSync(bootstrapIconsFontPath)) {
        const bootstrapFontFiles = glob.sync('**/*.{woff,woff2,ttf,svg,eot}', { cwd: bootstrapIconsFontPath, nodir: true });
        bootstrapFontFiles.forEach((file) => {
            const sourceFile = path.join(bootstrapIconsFontPath, file);
            const destFile = path.join(fontsDestPath, file); // Direct copy without sub-folder
            sh.mkdir('-p', path.dirname(destFile)); // Ensure directory exists
            sh.cp(sourceFile, destFile);
            console.log(`Copied: ${sourceFile} to ${destFile}`);
        });
    } else {
        console.error(`Bootstrap Icons fonts path not found: ${bootstrapIconsFontPath}`);
    }

    // Copy Feather Icons SVGs
    if (fs.existsSync(featherIconsSvgPath)) {
        const featherSvgFiles = glob.sync('**/*.svg', { cwd: featherIconsSvgPath, nodir: true });
        featherSvgFiles.forEach((file) => {
            const sourceFile = path.join(featherIconsSvgPath, file);
            const destFile = path.join(featherIconsDestPath, file); // Keeping original names for SVGs
            sh.mkdir('-p', path.dirname(destFile));
            sh.cp(sourceFile, destFile);
            console.log(`Copied: ${sourceFile} to ${destFile}`);
        });
    } else {
        console.error(`Feather Icons SVGs path not found: ${featherIconsSvgPath}`);
    }

    // Copy Font Awesome fonts
    if (fs.existsSync(fontAwesomeFontPath)) {
        const fontAwesomeFiles = glob.sync('**/*.{woff,woff2,ttf,eot,svg}', { cwd: fontAwesomeFontPath, nodir: true });
        fontAwesomeFiles.forEach((file) => {
            const sourceFile = path.join(fontAwesomeFontPath, file);
            const destFile = path.join(fontAwesomeDestPath, file); // Direct copy to /dist/webfonts
            sh.mkdir('-p', path.dirname(destFile)); // Ensure directory exists
            sh.cp(sourceFile, destFile);
            console.log(`Copied: ${sourceFile} to ${destFile}`);
        });
    } else {
        console.error(`Font Awesome fonts path not found: ${fontAwesomeFontPath}`);
    }

    console.log('Assets and fonts (including SVGs and Font Awesome) copying completed!');
};
