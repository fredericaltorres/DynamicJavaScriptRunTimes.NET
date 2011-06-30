/*
    Code to compile CoffeeScript into JavaScript and to execute the JavaScript
*/
function CompileCoffeeScript(code){

    return this.CoffeeScript.compile(code);
}
function RunCoffeeScript(jsCode){
    
    return eval(jsCode);    
}