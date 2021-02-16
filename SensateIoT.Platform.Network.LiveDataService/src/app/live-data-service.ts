
import * as app from "./app";

function setEnvironment() {
    var env = process.env.NODE_ENV || 'development';

    if (env === 'production') {
        console.debug = function () { };
    }
}

setEnvironment();
app.main();
