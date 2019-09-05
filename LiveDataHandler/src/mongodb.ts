/*
 * MongoDB connector.
 * 
 * @author Michel Megens
 * @email  michel.megens@sonatolabs.com
 */

var mongoose = require('mongoose');

(<any>mongoose).Promise = global.Promise;

export function connect(url: string) {
    /*return new Promise((resolve, reject) => {
        mongoose.connect(url, (err: any) => {
            if (err)
                reject(err);
            else
                resolve();
        });
    });*/

    mongoose.connect(url, { useNewUrlParser: true }).then(() => {
        console.log("Connected to MongoDB!");
    }).catch((err) => {
        console.warn("Unable to connect to MongoDB: " + err.toString());
    });
}
