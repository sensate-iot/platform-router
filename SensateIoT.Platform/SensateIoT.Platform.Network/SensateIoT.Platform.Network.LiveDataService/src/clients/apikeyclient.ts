/*
 * API database client.
 *
 * @author Michel Megens
 * @email  michel@michelmegens.net
 */

import { Pool } from "pg";

export class ApiKeyClient {
    public constructor(private readonly pool: Pool) { }

    public validateApiKey(userid: string, key: string): Promise<boolean> {
        return new Promise<boolean>((resolve, reject) => {
            this.pool.connect().then(client => {
                client.query('SELECT * FROM livedataservice_getapikey($1, $2)', [userid, key]).then(res => {
                    client.release();

                    if (res.rowCount === 1) {
                        resolve(true);
                    } else {
                        resolve(false);
                    }
                });
            });
        });
    }
}
