fs = require('fs');

let newData = [];

JSON.parse(fs.readFileSync('density.json')).forEach((d)=>{
    newData.push({
        "x1" : d.geometry.coordinates[0][0][0],
        "y1" : d.geometry.coordinates[0][0][1],
        "x2" : d.geometry.coordinates[0][1][0],
        "y2" : d.geometry.coordinates[0][1][1],
        "x3" : d.geometry.coordinates[0][2][0],
        "y3" : d.geometry.coordinates[0][2][1],
        "x4" : d.geometry.coordinates[0][3][0],
        "y4" : d.geometry.coordinates[0][3][1],
        "area" : d.properties.area,
        "pop" : d.properties.pop,
        "rev" : d.properties.rev,
        "m25ans" : d.properties.m25ans,
        "p65ans" : d.properties.p65ans,
        "men_basr" : d.properties.men_basr,
        "men" : d.properties.men,
        "men_coll" : d.properties.men_coll,
        "men_prop" : d.properties.men_prop
    })
})

fs.writeFileSync('new_density.json',JSON.stringify(newData))