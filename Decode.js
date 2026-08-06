const vm = require("vm");
const fs = require("fs");

function splitChunks(code) {
    const chunks = [];
    let i = 0;
    const n = code.length;

    while (i < n) {
        while (i < n && (code[i] === '+' || /\s/.test(code[i]))) i++;
        if (i >= n) break;

        let depth = 0;
        let start = i;
        let started = false;

        while (i < n) {
            const c = code[i];

            if (c === '(' || c === '[') {
                depth++;
                started = true;
            } else if (c === ')' || c === ']') {
                depth--;
            }

            i++;

            if (started && depth === 0) {
                let j = i;
                while (j < n && /\s/.test(code[j])) j++;

                if (j < n && code[j] === '[') {
                    continue;
                }

                break;
            }
        }

        const chunk = code.slice(start, i).trim();
        if (chunk) chunks.push(chunk);
    }

    return chunks;
}

function createSandbox() {
    const sandbox = {};

    sandbox.global = sandbox;
    sandbox.globalThis = sandbox;

    sandbox.eval = function(code) {
        return code;
    };

    const safeFunction = (...args) => {
        const body = String(args[args.length - 1]).trim();

        if (body === "return eval") {
            return () => sandbox.eval;
        }

        if (body === "return Function") {
            return () => sandbox.Function;
        }

        if (body.startsWith("return ")) {
            const expr = body.slice(7).trim();
            return () => expr;
        }

        return function () {
            return body;
        };
    };

    sandbox.Function = safeFunction;

    sandbox.String = function(v) {
        return String(v);
    };

    sandbox.String.prototype = String.prototype;

    const proxyHandler = {
        get(target, prop) {
            if (prop === "constructor") {
                return safeFunction;
            }
            return target[prop];
        }
    };

    sandbox.Array = new Proxy(Array, proxyHandler);
    sandbox.Object = new Proxy(Object, proxyHandler);
    sandbox.Boolean = new Proxy(Boolean, proxyHandler);
    sandbox.Number = new Proxy(Number, proxyHandler);
    sandbox.RegExp = new Proxy(RegExp, proxyHandler);

    return vm.createContext(sandbox);
}

function evalChunk(context, chunk) {
    const trimmed = chunk.trim();

    if (trimmed === "()") {
        return "";
    }

    try {
        return String(vm.runInContext(chunk, context));
    } catch {
        return null;
    }
}

function decodeJSFuck(code) {
    const chunks = splitChunks(code);

    if (chunks.length === 0) {
        return "";
    }

    const context = createSandbox();

    const results = chunks.map(chunk => evalChunk(context, chunk));

    return results
        .map(v => v ?? "?")
        .join("");
}


// Main
const jsfuckfile = process.argv[2];

if (!jsfuckfile) {
    process.exit(1);
}

const jsfuck = fs.readFileSync(jsfuckfile, "utf8");

let decoded = decodeJSFuck(jsfuck);

// Remove everything before eval
const evalIndex = decoded.indexOf("eval");

if (evalIndex !== -1) {
    decoded = decoded.slice(evalIndex + 4);
}

// Output ONLY decoded text
process.stdout.write(decoded);