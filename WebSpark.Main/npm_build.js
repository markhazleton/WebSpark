import fs from 'fs';
import path from 'path';
import CleanCSS from 'clean-css';
import UglifyJS from 'uglify-js';
import sass from 'sass';
import { fileURLToPath } from 'url';
import { dirname } from 'path';

const __filename = fileURLToPath(import.meta.url);
const __dirname = dirname(__filename);

// Function to minify CSS and copy original files
function minifyAndCopyCSS() {
    const scssInputPath = path.join(__dirname, 'src/scss/');
    const cssOutputPath = path.join(__dirname, 'src/scss/');
    const distOutputPath = path.join(__dirname, 'wwwroot/dist/css/');
    const scssOutputFile = 'compiled_style.css';  // Temporary output file for compiled SCSS
    const minifiedOutputFile = 'style.min.css';

    const result = sass.compile(scssInputPath + 'main.scss', {
        outputStyle: 'expanded'
    });

    fs.writeFileSync(cssOutputPath + scssOutputFile, result.css);
    fs.writeFileSync(distOutputPath + 'main.css', result.css);

    let output = new CleanCSS({}).minify([
        cssOutputPath + scssOutputFile,
        cssOutputPath + 'site.css'
    ]);

    fs.writeFileSync(distOutputPath + minifiedOutputFile, output.styles);

    fs.copyFileSync(cssOutputPath + 'site.css', distOutputPath + 'site.css');

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
    const popperJSPath = path.join(__dirname, 'node_modules/@popperjs/core/dist/umd/popper.js');
    const jqueryJSPath = path.join(__dirname, 'node_modules/jquery/dist/jquery.js');
    const datatablesJSPath = path.join(__dirname, 'node_modules/datatables.net-bs5/js/dataTables.bootstrap5.js');
    const outputPath = path.join(__dirname, 'wwwroot/dist/js/');
    const outputFile = 'scripts.min.js';

    const files = {
        'jquery.js': fs.readFileSync(jqueryJSPath, 'utf8'),
        'popper.js': fs.readFileSync(popperJSPath, 'utf8'),
        'bootstrap.js': fs.readFileSync(bootstrapJSPath, 'utf8'),
        'datatables.js': fs.readFileSync(datatablesJSPath, 'utf8'),
        'site.js': fs.readFileSync(inputPath + 'site.js', 'utf8')
    };

    const result = UglifyJS.minify(files);

    if (!result.error) {
        fs.writeFileSync(outputPath + outputFile, result.code);

        fs.copyFileSync(jqueryJSPath, outputPath + 'jquery.js');
        fs.copyFileSync(bootstrapJSPath, outputPath + 'bootstrap.bundle.js');
        fs.copyFileSync(popperJSPath, outputPath + 'popper.js');
        fs.copyFileSync(datatablesJSPath, outputPath + 'dataTables.bootstrap5.js');
        fs.copyFileSync(inputPath + 'site.js', outputPath + 'site.js');
    } else {
        console.error('Error minifying JS:', result.error);
    }
}

function combineAndCopyJS() {
    const inputPath = path.join(__dirname, 'src/js/');
    const jqueryJSPath = path.join(__dirname, 'node_modules/jquery/dist/jquery.js');
    const popperJSPath = path.join(__dirname, 'node_modules/@popperjs/core/dist/umd/popper.js');
    const bootstrapJSPath = path.join(__dirname, 'node_modules/bootstrap/dist/js/bootstrap.bundle.js');
    const datatablesJSPath = path.join(__dirname, 'node_modules/datatables.net-bs5/js/dataTables.bootstrap5.js');
    const outputPath = path.join(__dirname, 'wwwroot/dist/js/');
    const outputFile = 'scripts.js';

    // Read the files in the correct order and concatenate them
    const combinedScript = `
        ${fs.readFileSync(jqueryJSPath, 'utf8')}
        ${fs.readFileSync(popperJSPath, 'utf8')}
        ${fs.readFileSync(bootstrapJSPath, 'utf8')}
        ${fs.readFileSync(datatablesJSPath, 'utf8')}
        ${fs.readFileSync(inputPath + 'site.js', 'utf8')}
    `;

    // Write the combined script to the output file
    fs.writeFileSync(outputPath + outputFile, combinedScript);

    // Copy individual files to the output directory (optional)
    fs.copyFileSync(jqueryJSPath, outputPath + 'jquery.js');
    fs.copyFileSync(bootstrapJSPath, outputPath + 'bootstrap.bundle.js');
    fs.copyFileSync(popperJSPath, outputPath + 'popper.js');
    fs.copyFileSync(datatablesJSPath, outputPath + 'dataTables.bootstrap5.js');
    fs.copyFileSync(inputPath + 'site.js', outputPath + 'site.js');
}


minifyAndCopyCSS();
combineAndCopyJS();
