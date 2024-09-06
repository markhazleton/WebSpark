'use strict';
const fs = require('fs');
const path = require('path');
const UglifyJS = require('uglify-js');

module.exports = function renderScripts() {
    minifyAndCopyJS();
};

function minifyAndCopyJS() {
    const inputPath = path.join(__dirname, '../src/js/');  // Custom scripts
    const bootstrapJSPath = path.join(__dirname, '../node_modules/bootstrap/dist/js/bootstrap.bundle.js');  // Bootstrap JS
    const jqueryJSPath = path.join(__dirname, '../node_modules/jquery/dist/jquery.js');  // jQuery
    const featherJSPath = path.join(__dirname, '../node_modules/feather-icons/dist/feather.min.js');  // Feather Icons
    const outputPath = path.join(__dirname, '../wwwroot/dist/js/');  // Output directory
    const outputFile = 'webspark.min.js';

    // DataTables paths
    const dataTablesJSPath = path.join(__dirname, '../node_modules/datatables.net/js/dataTables.js');  // DataTables core
    const dataTablesBS5Path = path.join(__dirname, '../node_modules/datatables.net-bs5/js/dataTables.bootstrap5.js');  // DataTables Bootstrap 5 integration

    // Ensure the output directory exists
    if (!fs.existsSync(outputPath)) {
        fs.mkdirSync(outputPath, { recursive: true });
    }

    // Order of loading scripts
    const files = {
        'jquery.js': fs.readFileSync(jqueryJSPath, 'utf8'),                          // jQuery
        'datatables.js': fs.readFileSync(dataTablesJSPath, 'utf8'),                  // DataTables core
        'datatables-bs5.js': fs.readFileSync(dataTablesBS5Path, 'utf8'),             // DataTables Bootstrap 5
        'bootstrap.js': fs.readFileSync(bootstrapJSPath, 'utf8'),                    // Bootstrap JS
        'feather.js': fs.readFileSync(featherJSPath, 'utf8'),                        // Feather Icons
        'webspark.js': fs.readFileSync(path.join(inputPath, 'webspark.js'), 'utf8'), // Custom script
    };

    // Minify and combine JavaScript files
    const result = UglifyJS.minify(files);

    if (!result.error) {
        fs.writeFileSync(path.join(outputPath, outputFile), result.code);
        console.log(`Minified JS files successfully written to ${path.join(outputPath, outputFile)}`);
    } else {
        console.error('Error minifying JS:', result.error);
    }
}
