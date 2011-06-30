# Objects:
math =
  root:   Math.sqrt
  square: (x) -> x * x
  cube:   (x) -> x * this.square(x)

print(math.root(4))
print(math.cube(4))
