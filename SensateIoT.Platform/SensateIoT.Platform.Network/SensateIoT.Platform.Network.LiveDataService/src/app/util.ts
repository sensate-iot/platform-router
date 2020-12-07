/*
 * Utility functions.
 * 
 * @author Michel Megens
 * @email  michel@michelmegens.net
 */

export function toCamelCase(key: any, value: any) {
    if (value !== null && value !== undefined && typeof value === "object") {
        for (let k in value) {
            if (/^[A-Z]/.test(k) && Object.hasOwnProperty.call(value, k)) {
                value[k.charAt(0).toLowerCase() + k.substring(1)] = value[k];
                delete value[k];
            }
        }
    }

    return value;
}

export function getClientIP(forwardedHeader: string): string {
    if (forwardedHeader === null || forwardedHeader === undefined || forwardedHeader === "") {
        return "";
    }

    const result = forwardedHeader.split(",");
    return result[0].replace(/\s/g, "");
}
