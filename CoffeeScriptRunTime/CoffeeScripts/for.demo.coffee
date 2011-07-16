# CoffeeScript Demo
#!DisplayJavaScript

print("Hello World From CoffeeScript")

l = [ 1..20 ]
for i in l
    print(i)

o = { 
    a:1, 
    b:2, 
    c: (z) ->
        return "val:"+z
}
for k,v of o
    if typeof(v) != 'function'
        print(k+" "+v)

print(o["c"](1))
