function formatDateUS(d){

    return  (d.getMonth()+1) + '/' + 
            d.getDate()      + '/' + 
            d.getFullYear()  + ' ' + 
            d.getHours()     + ':' + 
            d.getMinutes()   + ':' + 
            d.getSeconds();
}