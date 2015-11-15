/**
 * Defines a set of helpers that can be called to do stuff
 * @returns {} 
 */
function helpers() {
    throw new Error("The helper class should never be instanciated");
}
/**
* Checks if the string is either pure whitespace or undefined
* @param {} input The string to check
* @returns {} 
*/
helpers.isNullOrWhitespace = function (input) {
    if (typeof input === "undefined" || input == null) return true;
    return input.replace(/\s/g, "").length < 1;
}

/**
* Defines accepted characters in names
*/
helpers.patt = /[\w][\wæøåöäÆØÅÖÄ ]+[\w]/;

/**
* Removed the selected index from an array
* @param {Array} arr The array to edit
* @param {Number} index The index at which a variable should be removed
* @returns {Array} The array with the index removed
*/
helpers.removeFromArray = function (arr, index) {
    return arr.slice(0, index).concat(arr.slice(index + 1));
}


/**
 * Gets the query parameters as an object of a query string
 * @param {string} qs The query string 
 * @returns {Object} The query params as an object 
 */
helpers.getQueryParams = function(qs) {
    var parse = function(params, pairs) {
        var pair = pairs[0];
        var parts = pair.split("=");
        var key;
        try {
            key = decodeURIComponent(parts[0]);
        } catch (err) {
            key = parts[0];
        }
        var value;
        try {
            value = decodeURIComponent(parts.slice(1).join("="));
        } catch (err) {
            value = parts.slice(1).join("=");
        }

        // Handle multiple parameters of the same name
        if (typeof params[key] === "undefined") {
            params[key] = value;
        } else {
            params[key] = [].concat(params[key], value);
        }

        return pairs.length === 1 ? params : parse(params, pairs.slice(1));
    }

    // Get rid of leading ?
    return qs.length === 0 ? {} : parse({}, qs.substr(1).split("&"));
}