const fs = require('fs');
const path = require('path');
const CleanCSS = require('clean-css');
const UglifyJS = require('uglify-js');
const sass = require('sass');

// Function to minify CSS and copy original files
function minifyAndCopyCSS() {
    const scssInputPath = path.join(__dirname, 'src/scss/');
    const cssOutputPath = path.join(__dirname, 'src/scss/');
    const distOutputPath = path.join(__dirname, 'wwwroot/dist/css/');
    const scssOutputFile = 'compiled_style.css';  // Temporary output file for compiled SCSS
    const minifiedOutputFile = 'style.min.css';

    // Compile SCSS to CSS
    const result = sass.compile(scssInputPath + 'main.scss', {
        outputStyle: 'expanded'  // Expanded for initial compilation, minification will handle compression
    });

    // Write the compiled CSS to a temporary file
    fs.writeFileSync(cssOutputPath + scssOutputFile, result.css);
    fs.writeFileSync(distOutputPath + 'main.css', result.css);

    // Minify the compiled CSS along with other CSS files
    let output = new CleanCSS({}).minify([
        cssOutputPath + scssOutputFile,
        cssOutputPath + 'site.css'
    ]);

    // Write the final minified CSS to the distribution folder
    fs.writeFileSync(distOutputPath + minifiedOutputFile, output.styles);

    // Copy the original CSS files to the distribution folder
    fs.copyFileSync(cssOutputPath + 'site.css', distOutputPath + 'site.css');

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
    const inputPath = path.join(__dirname, 'src/js/');
    const bootstrapJSPath = path.join(__dirname, 'node_modules/bootstrap/dist/js/bootstrap.bundle.js');
    const jqueryJSPath = path.join(__dirname, 'node_modules/jquery/dist/jquery.js');
    const datatablesJSPath = path.join(__dirname, 'node_modules/datatables.net-bs5/js/dataTables.bootstrap5.js');
    const outputPath = path.join(__dirname, 'wwwroot/dist/js/');
    const outputFile = 'scripts.min.js';

    const files = {
        'jquery.js': fs.readFileSync(jqueryJSPath, 'utf8'),
        'bootstrap.js': fs.readFileSync(bootstrapJSPath, 'utf8'),
        'datatables.js': fs.readFileSync(datatablesJSPath, 'utf8'),
        'site.js': fs.readFileSync(inputPath + 'site.js', 'utf8')
    };

    // Minify JavaScript files
    const result = UglifyJS.minify(files);

    if (!result.error) {
        fs.writeFileSync(outputPath + outputFile, result.code);

        // Copy original JavaScript files to the distribution folder
        fs.copyFileSync(bootstrapJSPath, outputPath + 'bootstrap.bundle.js');
        fs.copyFileSync(jqueryJSPath, outputPath + 'jquery.js');
        fs.copyFileSync(datatablesJSPath, outputPath + 'dataTables.bootstrap5.js');
        fs.copyFileSync(inputPath + 'site.js', outputPath + 'site.js');
    } else {
        console.error('Error minifying JS:', result.error);
    }
}

// Run the functions
minifyAndCopyCSS();
minifyAndCopyJS();
