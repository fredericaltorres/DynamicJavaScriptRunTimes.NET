function User(userName, firstName, lastName) { 

    this.UserName  = userName;
    this.LastName  = lastName;
    this.FirstName = firstName;
}
var Configuration = {

    Server  : 'TOTO',
    Database: 'Rene',
    Debug   : true,
    Users   : [     
        { 
            UserName : 'bpascal', 
            FirstName: 'blaise', 
            LastName : 'pascal' 
        },
        new User('cmontesquieu', 'charles', 'montesquieu')
    ]
}