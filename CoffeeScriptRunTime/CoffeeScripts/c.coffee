#-------------------------------------------------
#
class Person

    constructor: (@LastName, @FirstName, @DateOfBirth) ->

    GetInfo:() ->
        this.LastName + " " + this.FirstName + " " + this.DateOfBirth

    Titi: (p) ->
        null

p = new Person("Torres","Fred","")
alert(p.GetInfo())

#-------------------------------------------------
#
class Employee extends Person

    constructor: (lastName, firstName, dateOfBirth, @Company) ->
        super(lastName, firstName, dateOfBirth)

e = new Employee("Torres","Fred","","ScerIS")
alert(e.GetInfo())
alert(e.Company)
