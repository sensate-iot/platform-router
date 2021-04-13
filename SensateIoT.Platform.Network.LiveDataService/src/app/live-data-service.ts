
import * as app from "./app";

function getTimestamp() {
    const now = new Date();
    return now.toISOString();
}

function setupInfo() {
    const origLog = console.log;

    console.log = function () {
        const args = [].slice.call(arguments);
        const ts = `[${getTimestamp()}][INFO]: `;
        origLog.apply(console.log, [ts].concat(args));
    }
}

function setupDebug() {
    const origLog = console.debug;

    console.debug = function () {
        const args = [].slice.call(arguments);
        const ts = `[${getTimestamp()}][DEBUG]: `;
        origLog.apply(console.debug, [ts].concat(args));
    }
}

function setupError() {
    const origLog = console.error;

    console.error = function () {
        const args = [].slice.call(arguments);
        const ts = `[${getTimestamp()}][ERROR]: `;
        origLog.apply(console.error, [ts].concat(args));
    }
}

function setEnvironment() {
    var env = process.env.NODE_ENV || 'development';

    setupInfo();
    setupDebug();
    setupError();

    if (env === 'production') {
        console.debug = function () { };
    }
}

setEnvironment();
app.main();
