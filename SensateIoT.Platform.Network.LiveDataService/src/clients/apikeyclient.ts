/*
 * API database client.
 *
 * @author Michel Megens
 * @email  michel@michelmegens.net
 */

import { Pool } from "pg";

export class ApiKeyClient {
    public constructor(private readonly pool: Pool) { }

    public validateApiKey(userid: string, key: string): Promise<string> {
        console.debug(`Attempting to verify key ${key} for user ${userid}.`)
        return new Promise<string>((resolve, reject) => {
            this.pool.connect().then(client => {
                client.query('SELECT * FROM livedataservice_getapikey($1, $2)', [userid, key]).then(res => {
                    client.release();

                    if (res.rowCount === 1) {
                        console.log(`Found API key for user ${userid}.`);
                        resolve(res.rows[0].UserId);
                    } else {
                        console.warn(`API key authorization failed. Found ${res.rowCount} results.`)
                        resolve(null);
                    }
                }).catch(error => {
                    console.error(`Unable to complete API key validation for user: ${userid}.`)
                    console.error(error);
                });
            });
        });
    }
}
