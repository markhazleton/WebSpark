const fs = require('fs');
const path = require('path');
const CleanCSS = require('clean-css');
const UglifyJS = require('uglify-js');
const sass = require('sass');

// Function to minify CSS and copy original files
function minifyAndCopyCSS() {
    const scssInputPath = path.join(__dirname, 'scss/');  // Updated: relative to new location
    const cssOutputPath = path.join(__dirname, 'scss/');  // Updated: relative to new location
    const distOutputPath = path.join(__dirname, '../../wwwroot/dist/css/');  // Updated: adjusted for new __dirname
    const scssOutputFile = 'compiled_style.css';  // Temporary output file for compiled SCSS
    const minifiedOutputFile = 'PromptSpark.min.css';

    // Compile SCSS to CSS
    const result = sass.compile(scssInputPath + 'main.scss', {
        outputStyle: 'expanded'  // Expanded for initial compilation, minification will handle compression
    });

    // Write the compiled CSS to a temporary file
    fs.writeFileSync(cssOutputPath + scssOutputFile, result.css);

    // Minify the compiled CSS along with other CSS files
    let output = new CleanCSS({}).minify([
        cssOutputPath + scssOutputFile,
        cssOutputPath + 'site.css'
    ]);

    // Write the final minified CSS to the distribution folder
    fs.writeFileSync(distOutputPath + minifiedOutputFile, output.styles);

    // Optionally, clean up the temporary file
    fs.unlink(cssOutputPath + scssOutputFile, (err) => {
        if (err) {
            console.error('Error deleting file:', err);
        } else {
            console.log('Temporary file deleted successfully.');
        }
    });
}

// Function to minify JS and copy original files
function minifyAndCopyJS() {
    const inputPath = path.join(__dirname, 'js/');  // Relative to new location
    const bootstrapJSPath = path.join(__dirname, '../../node_modules/bootstrap/dist/js/bootstrap.bundle.js');  // Bootstrap
    const jqueryJSPath = path.join(__dirname, '../../node_modules/jquery/dist/jquery.js');  // jQuery
    const featherJSPath = path.join(__dirname, '../../node_modules/feather-icons/dist/feather.min.js');  // Feather JS
    const outputPath = path.join(__dirname, '../../wwwroot/dist/js/');  // Adjusted for new __dirname
    const outputFile = 'PromptSpark.min.js';

    const files = {
        'jquery.js': fs.readFileSync(jqueryJSPath, 'utf8'),              // 1. jQuery
        'bootstrap.js': fs.readFileSync(bootstrapJSPath, 'utf8'),        // 2. Bootstrap
        'feather.js': fs.readFileSync(featherJSPath, 'utf8'),            // 3. Feather Icons JS (if needed)
        'site.js': fs.readFileSync(inputPath + 'site.js', 'utf8')        // 4. Custom Script
    };

    // Minify JavaScript files
    const result = UglifyJS.minify(files);

    if (!result.error) {
        fs.writeFileSync(outputPath + outputFile, result.code);
    } else {
        console.error('Error minifying JS:', result.error);
    }
}

// Run the functions
minifyAndCopyCSS();
minifyAndCopyJS();
