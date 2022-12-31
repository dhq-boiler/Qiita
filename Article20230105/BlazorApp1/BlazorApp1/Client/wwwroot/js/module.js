class Module {
    temp;
    add(a, b) {
        return a + b;
    }
    subtract(a, b) {
        return a - b;
    }
    multiply(a, b) {
        return a * b;
    }
    divide(a, b) {
        return a / b;
    }
    push(val) {
        temp = val;
    }
    pop() {
        return temp;
    }
}

if (typeof module === 'object') {
    module.exports = Module;
}