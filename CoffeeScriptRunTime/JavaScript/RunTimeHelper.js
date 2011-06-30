/*
    global function need to run CoffeeScript program
*/
function print(){
    var
        i       = 0,
        r       = null,
        values  = [];

    for(i=0; i<arguments.length; i++){
        values.push(arguments[i])
    }
    r = values.join();
    console.log(r);
    return r;
}
function printJSON(){
    var
        i       = 0,
        r       = null,
        values  = [];

    for(i=0; i<arguments.length; i++){
        values.push(JSON.stringify(arguments[i]))
    }
    r = values.join();
    console.log(r);
    return r;
}

function alert(s) { 
    return print(s);          
}
