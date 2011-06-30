# CoffeeScript Demo
#DisplayJavaScript
class Person

    constructor: (lastName, firstName, dateOfBirth) ->

        this.LastName    = lastName
        this.FirstName   = firstName
        this.DateOfBirth = dateOfBirth

    toString: () ->

        return this.LastName + " " + this.FirstName + " " + this.DateOfBirth

p = new Person("Torres","Fred",new Date(1964, 12-1, 11))
print(p.toString())

class Employee extends Person

    constructor: (lastName, firstName, dateOfBirth, company) ->

        super(lastName, firstName, dateOfBirth)
        this.Company = company

    toString: () ->
        return super() + " " + this.Company

e = new Employee("Torres","Fred",new Date(1964, 12-1, 11), "ScerIS")
print(e.toString())
