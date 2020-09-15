/*
 * MongoDB connector.
 * 
 * @author Michel Megens
 * @email  michel.megens@sonatolabs.com
 */

var mongoose = require('mongoose');

(<any>mongoose).Promise = global.Promise;

export function connect(url: string) {
    mongoose.connect(url, { useNewUrlParser: true, useUnifiedTopology: true }).then(() => {
        console.log("Connected to MongoDB!");
    }).catch((err) => {
        console.warn("Unable to connect to MongoDB: " + err.toString());
    });
}
