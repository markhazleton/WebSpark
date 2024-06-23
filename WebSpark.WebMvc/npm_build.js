const fs = require('fs');
const path = require('path');
const CleanCSS = require('clean-css');
const UglifyJS = require('uglify-js');
const sass = require('sass');

// Function to ensure a directory exists
function ensureDirSync(dirpath) {
    if (!fs.existsSync(dirpath)) {
        fs.mkdirSync(dirpath, { recursive: true });
    }
}

// Function to clean the dist directory
function cleanDist() {
    const distPath = path.join(__dirname, 'wwwroot/dist');

    if (fs.existsSync(distPath)) {
        fs.rmSync(distPath, { recursive: true, force: true });
        console.log('Dist directory cleaned.');
    } else {
        console.log('Dist directory does not exist, no cleaning needed.');
    }
}

// Function to minify CSS and copy original files
function minifyAndCopyCSS() {
    const scssInputPath = path.join(__dirname, 'src/scss/');
    const cssOutputPath = path.join(__dirname, 'src/scss/');
    const distOutputPath = path.join(__dirname, 'wwwroot/dist/css/');
    const scssOutputFile = 'compiled_style.css';  // Temporary output file for compiled SCSS
    const minifiedOutputFile = 'style.min.css';

    // Ensure the output directory exists
    ensureDirSync(distOutputPath);

    // Compile SCSS to CSS
    const result = sass.compile(scssInputPath + 'main.scss', {
        outputStyle: 'expanded'  // Expanded for initial compilation, minification will handle compression
    });

    // Write the compiled CSS to a temporary file
    fs.writeFileSync(cssOutputPath + scssOutputFile, result.css);
//    fs.writeFileSync(distOutputPath + 'main.css', result.css);

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
    const inputPath = path.join(__dirname, 'src/js/');
    const bootstrapJSPath = path.join(__dirname, 'node_modules/bootstrap/dist/js/bootstrap.bundle.js');
    const jqueryJSPath = path.join(__dirname, 'node_modules/jquery/dist/jquery.js');
    const datatablesBaseJSPath = path.join(__dirname, 'node_modules/datatables.net/js/dataTables.js');
    const datatablesBSJSPath = path.join(__dirname, 'node_modules/datatables.net-bs5/js/dataTables.bootstrap5.js');
    const outputPath = path.join(__dirname, 'wwwroot/dist/js/');
    const outputFile = 'scripts.min.js';

    // Ensure the output directory exists
    ensureDirSync(outputPath);

    const files = {
        'jquery.js': fs.readFileSync(jqueryJSPath, 'utf8'),
        'bootstrap.js': fs.readFileSync(bootstrapJSPath, 'utf8'),
        'datatables.js': fs.readFileSync(datatablesBaseJSPath, 'utf8'),
        'datatablesBS.js': fs.readFileSync(datatablesBSJSPath, 'utf8'),
        'site.js': fs.readFileSync(inputPath + 'site.js', 'utf8')
    };

    // Minify JavaScript files
    const result = UglifyJS.minify(files);

    if (!result.error) {
        fs.writeFileSync(outputPath + outputFile, result.code);
    } else {
        console.error('Error minifying JS:', result.error);
    }
}

// Function to copy images
function copyImages() {
    const imagesInputPath = path.join(__dirname, 'src/images/');
    const imagesOutputPath = path.join(__dirname, 'wwwroot/dist/images/');

    // Ensure the output directory exists
    ensureDirSync(imagesOutputPath);

    fs.readdir(imagesInputPath, (err, files) => {
        if (err) {
            console.error('Error reading images directory:', err);
            return;
        }

        files.forEach(file => {
            const srcFile = path.join(imagesInputPath, file);
            const destFile = path.join(imagesOutputPath, file);

            fs.copyFileSync(srcFile, destFile);
        });

        console.log('Images copied successfully.');
    });
}

// Clean the dist directory and run the functions
cleanDist();
minifyAndCopyCSS();
minifyAndCopyJS();
copyImages();
