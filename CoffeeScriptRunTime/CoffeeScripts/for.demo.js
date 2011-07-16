// JavaScript Demo

print("Hello World From JavaScript");

for(var i=0; i<20; i++)
    print(i);
    

o = {
    a: 1,
    b: 2,
    c: function(z) {
        return z;
    }
}

for (var k in o)
    if(typeof(o[k])!=='function')
        print(k+" = "+o[k]);

print(o["c"](1));
