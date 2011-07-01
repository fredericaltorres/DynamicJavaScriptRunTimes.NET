"#!DisplayJavaScript"
f = () ->

    ff = () ->
        return "hello"

    return ff() + " world"

print(f())