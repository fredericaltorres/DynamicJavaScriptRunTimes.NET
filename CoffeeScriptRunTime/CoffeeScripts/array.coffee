numbers = [0, 1, 2, 3, 4, 5, 6, 7, 8, 9]
copy    = numbers[0...numbers.length]
middle  = copy[3..6]

printJSON(numbers)
printJSON(copy)
printJSON(middle)
printJSON(copy[0..3])
printJSON(copy[0..3][0..1])

six = (one = 1) + (two = 2) + (three = 3)
print("six:#{six}, one:#{one}, two:#{two}, three:#{three}")
