function isNullOrWhitespace(input) {
    if (typeof input === "undefined" || input == null)
        return true;
    return input.replace(/\s/g, "").length < 1;
}
var patt = /[\w][\wæøåöäÆØÅÖÄ ]+[\w]/;
function removeFromArray(arr, index) {
    return arr.slice(0, index).concat(arr.slice(index + 1));
}
function getQueryParams(qs) {
    var parse = function (params, pairs) {
        var pair = pairs[0];
        var parts = pair.split("=");
        var key;
        try {
            key = decodeURIComponent(parts[0]);
        }
        catch (err) {
            key = parts[0];
        }
        var value;
        try {
            value = decodeURIComponent(parts.slice(1).join("="));
        }
        catch (err) {
            value = parts.slice(1).join("=");
        }
        // Handle multiple parameters of the same name
        if (typeof params[key] === "undefined") {
            params[key] = value;
        }
        else {
            params[key] = [].concat(params[key], value);
        }
        return pairs.length === 1 ? params : parse(params, pairs.slice(1));
    };
    // Get rid of leading ?
    return qs.length === 0 ? {} : parse({}, qs.substr(1).split("&"));
}
