const concurrently = require('concurrently');
const upath = require('upath');

const browserSyncPath = upath.resolve(upath.dirname(__filename), '../node_modules/.bin/browser-sync');

// Define the commands to run concurrently
const commands = [
    { command: 'node scripts/sb-watch.js', name: 'SB_WATCH', prefixColor: 'bgBlue.bold' },
    {
        command: `"${browserSyncPath}" --reload-delay 2000 --reload-debounce 2000 wwwroot -w --no-online`,
        name: 'SB_BROWSER_SYNC',
        prefixColor: 'bgGreen.bold',
    }
];

// Options for concurrently execution
const options = {
    prefix: 'name',
    killOthers: ['failure', 'success'],
};

// Execute commands concurrently
const process = concurrently(commands, options);

process.result.then(
    () => {
        console.log('Success');
    },
    () => {
        console.error('Failure');
    }
);
