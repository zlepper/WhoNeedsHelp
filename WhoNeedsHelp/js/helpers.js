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
    qs = qs.split('+').join(' ');
    var params = {}, tokens, re = /[?&]?([^=]+)=([^&]*)/g;
    while (tokens = re.exec(qs)) {
        params[decodeURIComponent(tokens[1])] = decodeURIComponent(tokens[2]);
    }
    return params;
}
